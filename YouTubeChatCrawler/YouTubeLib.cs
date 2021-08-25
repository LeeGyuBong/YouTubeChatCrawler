using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YouTubeChatCrawler
{
	public class CommentInfo
	{
		public int ParentNo;
		public int ChildNo;
		public string Text;
		public long LikeCount;
		public string AuthorName;
		public DateTime PublishedAt;
		public long ReplyCount;

		public CommentInfo( int no, CommentThread thread )
		{
			ParentNo = no;
			ChildNo = 0;
			Text = thread.Snippet.TopLevelComment.Snippet.TextDisplay;
			LikeCount = ( long )thread.Snippet.TopLevelComment.Snippet.LikeCount;
			AuthorName = thread.Snippet.TopLevelComment.Snippet.AuthorDisplayName;
			PublishedAt = DateTime.Parse( thread.Snippet.TopLevelComment.Snippet.PublishedAt.ToString() );
			ReplyCount = ( long )thread.Snippet.TotalReplyCount;
		}

		public CommentInfo( int no, int cno, Comment thread )
		{
			ParentNo = no;
			ChildNo = cno;
			Text = thread.Snippet.TextDisplay;
			LikeCount = ( long )thread.Snippet.LikeCount;
			AuthorName = thread.Snippet.AuthorDisplayName;
			PublishedAt = DateTime.Parse( thread.Snippet.PublishedAt.ToString() );
		}

		public CommentInfo( LiveChatMessage message )
		{
			Text = message.Snippet.TextMessageDetails.MessageText;
			AuthorName = message.AuthorDetails.DisplayName;
			PublishedAt = DateTime.Parse( message.Snippet.PublishedAt.ToString() );
		}
	}

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
			var ret = Task.Run( async () =>
			{
				

				UserCredential credential;
				using ( var stream = new FileStream( "client_secret.json", FileMode.Open, FileAccess.Read ) )
				{
					credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
						GoogleClientSecrets.FromStream( stream ).Secrets,
						new[] { YouTubeService.Scope.YoutubeReadonly },
						"user",
						CancellationToken.None );
				}

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
			var request = youtubeService.CommentThreads.List( "snippet" );
			request.VideoId = videoId;
			request.Order = CommentThreadsResource.ListRequest.OrderEnum.Relevance;
			request.TextFormat = CommentThreadsResource.ListRequest.TextFormatEnum.PlainText;
			request.MaxResults = 100;
			request.PageToken = nextPageToken;

			try
			{
				var response = request.Execute();
				foreach ( CommentThread item in response.Items )
				{
					try
					{
						CommentInfo info = new CommentInfo( no, item );
						commentList.Add( info );

						string parentId = item.Snippet.TopLevelComment.Id;

						if ( item.Snippet.TotalReplyCount > 0 )
							GetReplyComment( commentList, parentId, no, 1, null );

						no++;
					}
					catch ( Exception e )
					{
						Console.WriteLine( "[GetComment] Error!(Message:{0}, Stack:{1})", e.Message, e.StackTrace );
					}
				}

				if ( response.NextPageToken != null )
					GetComment( commentList, videoId, no, response.NextPageToken );
			}
			catch( Exception e )
			{
				Console.WriteLine( "[GetComment] request.ExecuteAsync Error!(Message:{0}, Stack:{1})", e.Message, e.StackTrace );
			}
		}

		/// <summary>
		/// 유저 대댓글 가져오기
		/// </summary>
		private void GetReplyComment( List<CommentInfo> commentList, string parentId, int no, int cno, string nextPageToken )
		{
			var request = youtubeService.Comments.List( "snippet" );
			request.TextFormat = CommentsResource.ListRequest.TextFormatEnum.PlainText;
			request.MaxResults = 50;
			request.ParentId = parentId;
			request.PageToken = nextPageToken;

			var response = request.Execute();

			foreach ( Comment item in response.Items )
			{
				try
				{
					CommentInfo info = new CommentInfo( no, cno, item );
					commentList.Add( info );

					cno++;
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
			var videoList = youtubeService.Videos.List( "LiveStreamingDetails" );
			videoList.Id = videoId;

			var videoListResponse = videoList.Execute();

			foreach ( var item in videoListResponse.Items )
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
			var request = youtubeService.LiveChatMessages.List( liveChatId, "snippet,authorDetails" );
			request.PageToken = nextPageToken;
			request.MaxResults = 2000;

			var response = request.Execute();
			
			foreach ( LiveChatMessage item in response.Items )
			{
				try
				{
					CommentInfo info = new CommentInfo( item );
					commentList.Add( info );
				}
				catch ( Exception e )
				{
					Console.WriteLine( "[GetLiveComment] Error!(Message:{0}, Stack:{1})", e.Message, e.StackTrace );
				}
			}

			await Task.Delay( ( int )response.PollingIntervalMillis );

			return response.NextPageToken;
		}
	}
}
