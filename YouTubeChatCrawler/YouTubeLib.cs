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
	class YouTubeLib
	{
		public static YouTubeService youtubeService = null;

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
		}

		public async Task Init()
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
		}

		/// <summary>
		/// 유저 댓글 가져오기
		/// </summary>
		public async Task GetComment( List<CommentInfo> commentList, string videoId, YouTubeService youtubeService, int no, string nextPageToken )
		{
			var request = youtubeService.CommentThreads.List( "snippet" );
			request.VideoId = videoId;
			request.Order = CommentThreadsResource.ListRequest.OrderEnum.Relevance;
			request.TextFormat = CommentThreadsResource.ListRequest.TextFormatEnum.PlainText;
			request.MaxResults = 100;
			request.PageToken = nextPageToken;

			var response = await request.ExecuteAsync();
			foreach ( CommentThread item in response.Items )
			{
				try
				{
					CommentInfo info = new CommentInfo( no, item );
					commentList.Add( info );

					string parentId = item.Snippet.TopLevelComment.Id;

					if ( item.Snippet.TotalReplyCount > 0 )
						await GetReplyComment( commentList, youtubeService, parentId, no, 1, null );

					no++;
				}
				catch ( Exception e )
				{
					Console.WriteLine( "[GetComment] Error!(Message:{0}, Stack:{1})", e.Message, e.StackTrace );
				}
			}

			if ( response.NextPageToken != null )
				await GetComment( commentList, videoId, youtubeService, no, response.NextPageToken );
		}

		/// <summary>
		/// 유저 대댓글 가져오기
		/// </summary>
		private async Task GetReplyComment( List<CommentInfo> commentList, YouTubeService youtubeService, string parentId, int no, int cno, string nextPageToken )
		{
			var request = youtubeService.Comments.List( "snippet" );
			request.TextFormat = CommentsResource.ListRequest.TextFormatEnum.PlainText;
			request.MaxResults = 50;
			request.ParentId = parentId;
			request.PageToken = nextPageToken;

			var response = await request.ExecuteAsync();

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
				await GetReplyComment( commentList, youtubeService, parentId, no, cno, response.NextPageToken );
		}

		/// <summary>
		/// 라이브 스트리밍 ID 가져오기 / 유튜브 라이브 스트리밍은 ID를 표기하지 않기에 아래 함수로 받아와야함
		/// </summary>
		private string GetliveChatID( string videoId, YouTubeService youtubeService )
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
		private async Task GetLiveMessage( string videoId, YouTubeService youtubeService, string nextPageToken )
		{
			string liveChatId = GetliveChatID( videoId, youtubeService );
			if ( string.IsNullOrEmpty( liveChatId ) )
			{
				return;
			}

			var request = youtubeService.LiveChatMessages.List( liveChatId, "snippet,authorDetails" );
			request.PageToken = nextPageToken;
			request.MaxResults = 100;

			var response = await request.ExecuteAsync();
			foreach ( var item in response.Items )
			{
				try
				{
					Console.WriteLine( $"[{item.AuthorDetails.DisplayName}][{DateTime.Parse( item.Snippet.PublishedAt.ToString() )}] // {item.Snippet.DisplayMessage}" );
				}
				catch ( Exception e )
				{
					Console.WriteLine( "[GetLiveComment] Error!(Message:{0}, Stack:{1})", e.Message, e.StackTrace );
				}
			}
			Console.WriteLine( "{0} / {1}", response.PageInfo.TotalResults, response.PageInfo.ResultsPerPage );

			await Task.Delay( ( int )response.PollingIntervalMillis );

			await GetLiveMessage( liveChatId, youtubeService, response.NextPageToken );
		}
	}
}
