using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace YouTubeChatCrawler
{
	public class YouTubeLib
	{
		#region Singleton

		private static YouTubeLib __instance;
		private static object __lock = new object();

		public static YouTubeLib Instance
		{
			get
			{
				lock ( __lock )
				{
					if ( __instance == null )
					{
						__instance = new YouTubeLib();
					}
				}

				return __instance;
			}
		}

		private YouTubeLib()
		{
		}

		#endregion Singleton

		private static YouTubeService youtubeService = null;

		public void Init()
		{
			Task ret = Task.Run( async () =>
			{
				// OAuth 권한 셋팅
				UserCredential credential;
				using ( FileStream stream = new FileStream( "client_secret.json", FileMode.Open, FileAccess.Read ) )
				{
					credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
						GoogleClientSecrets.FromStream( stream ).Secrets,
						new[] { YouTubeService.Scope.YoutubeReadonly },
						"user",
						CancellationToken.None );
				}

				// YouTube Data API V3 권한 셋팅
				youtubeService = new YouTubeService( new BaseClientService.Initializer
				{
					ApplicationName = "YouTube_Chat_Crawler",
					ApiKey = "AIzaSyAGGlRWhbhVbYgPQWEBzRoO18DjfndR4Is",
				} );
			} );
			ret.Wait();
		}

		/// <summary>
		/// 유저 댓글 가져오기
		/// </summary>
		public void GetComment( List<CommentInfo> commentList, string videoId, int no, string nextPageToken )
		{
			CommentThreadsResource.ListRequest request = youtubeService.CommentThreads.List( "snippet, replies" );
			request.VideoId = videoId;
			request.Order = CommentThreadsResource.ListRequest.OrderEnum.Relevance;
			request.TextFormat = CommentThreadsResource.ListRequest.TextFormatEnum.PlainText;
			request.MaxResults = 100;
			request.PageToken = nextPageToken;

			try
			{
				CommentThreadListResponse response = request.Execute();

				foreach ( CommentThread item in response.Items )
				{
					CommentInfo info = new CommentInfo( no, item );
					commentList.Add( info );

					string parentId = item.Snippet.TopLevelComment.Id;

					long? TotalReplyCount = item.Snippet.TotalReplyCount;
					if ( TotalReplyCount > 0 )
					{
						// Part Replies가 가져오는 코멘트 수는 5개
						// 5개가 넘으면 Comment.list를 통해 재검색하여 모든 데이터를 가져와야함
						if ( TotalReplyCount > 5 )
						{
							List<CommentInfo> repliesList = new List<CommentInfo>();
							GetReplyComment( repliesList, parentId, no, 1, null );

							repliesList.Sort( ( x, y ) => x.PublishedAt.CompareTo( y.PublishedAt ) );
							commentList.AddRange( repliesList );
						}
						else
						{
							int cno = 1;
							for ( int i = item.Replies.Comments.Count; i > 0; --i )
							{
								CommentInfo replie = new CommentInfo( no, cno, item.Replies.Comments[ i - 1 ] );
								commentList.Add( replie );
								cno++;
							}
						}
					}

					no++;
				}

				if ( response.NextPageToken != null )
					GetComment( commentList, videoId, no, response.NextPageToken );
			}
			catch ( Exception e )
			{
				Console.WriteLine( "[GetComment] request.ExecuteAsync Error!(Message:{0}, Stack:{1})", e.Message, e.StackTrace );
			}
		}

		/// <summary>
		/// 유저 대댓글 가져오기
		/// </summary>
		private void GetReplyComment( List<CommentInfo> commentList, string parentId, int no, int cno, string nextPageToken )
		{
			CommentsResource.ListRequest request = youtubeService.Comments.List( "snippet" );
			request.TextFormat = CommentsResource.ListRequest.TextFormatEnum.PlainText;
			request.MaxResults = 100;
			request.ParentId = parentId;
			request.PageToken = nextPageToken;

			CommentListResponse response = request.Execute();

			cno = response.Items.Count;
			foreach ( Comment item in response.Items )
			{
				try
				{
					CommentInfo info = new CommentInfo( no, cno, item );
					commentList.Add( info );

					cno--;
				}
				catch ( Exception e )
				{
					Console.WriteLine( "[GetReplyComment] Error!(Message:{0}, Stack:{1})", e.Message, e.StackTrace );
				}
			}

			if ( response.NextPageToken != null )
				GetReplyComment( commentList, parentId, no, cno, response.NextPageToken );
		}

		/// <summary>
		/// 라이브 스트리밍 ID 가져오기 / 유튜브 라이브 스트리밍은 ID를 표기하지 않기에 아래 함수로 받아와야함
		/// </summary>
		public string GetliveChatID( string videoId )
		{
			VideosResource.ListRequest videoList = youtubeService.Videos.List( "LiveStreamingDetails" );
			videoList.Id = videoId;

			VideoListResponse videoListResponse = videoList.Execute();

			foreach ( Video item in videoListResponse.Items )
			{
				return item.LiveStreamingDetails.ActiveLiveChatId;
			}

			return null;
		}

		/// <summary>
		/// 라이브 스트리밍 코멘트 가져오기
		/// </summary>
		public async Task<string> GetLiveMessage( List<CommentInfo> commentList, string liveChatId, string nextPageToken )
		{
			LiveChatMessagesResource.ListRequest request = youtubeService.LiveChatMessages.List( liveChatId, "snippet,authorDetails" );
			request.PageToken = nextPageToken;
			request.MaxResults = 2000;

			LiveChatMessageListResponse response = null;
			try
			{
				response = request.Execute();

				foreach ( LiveChatMessage item in response.Items )
				{
					CommentInfo info = new CommentInfo( item );
					commentList.Add( info );
				}

				await Task.Delay( ( int )response.PollingIntervalMillis );
			}
			catch ( Exception e )
			{
				// TODO : 상세한 예외처리
				//forbidden( 403 ) forbidden 지정된 라이브 채팅에 대한 메시지를 검색하는 데 필요한 권한이 없습니다.
				//forbidden( 403 ) liveChatDisabled 지정된 방송에 대해 라이브 채팅을 사용할 수 없습니다.
				//forbidden( 403 ) liveChatEnded 지정된 라이브 채팅은 더 이상 라이브가 아닙니다.
				//notFound( 404 )  liveChatNotFound 검색하려는 라이브 채팅을 찾을 수 없습니다.요청 liveChatId매개변수 의 값 이 올바른지 확인하	십시  오.
				//rateLimitExceeded rateLimitExceeded   이전 요청 이후에 요청이 너무 빨리 전송되었습니다.이 오류는 메시지 검색을 위한 API 요청 이		YouTube의    새 로고침 빈도보다 더 자주 전송되어 불필요하게 대역폭을 낭비할 때 발생합니다.

				Console.WriteLine( "[GetLiveComment] Error!(Message:{0}, Stack:{1})", e.Message, e.StackTrace );
			}

			return response?.NextPageToken;
		}
	}
}