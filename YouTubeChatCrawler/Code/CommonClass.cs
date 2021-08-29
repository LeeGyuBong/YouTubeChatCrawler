using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;

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

	// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
	public class Icon
	{
		public string id { get; set; }
		public string url { get; set; }
		public int? height { get; set; }
		public int? width { get; set; }
	}

	public class Badge
	{
		public List<Icon> icons { get; set; }
		public string title { get; set; }
	}

	public class Image
	{
		public string id { get; set; }
		public string url { get; set; }
		public int? height { get; set; }
		public int? width { get; set; }
	}

	public class Author
	{
		public List<Badge> badges { get; set; }
		public string id { get; set; }
		public List<Image> images { get; set; }
		public string name { get; set; }
	}

	public class Emote
	{
		public string id { get; set; }
		public List<Image> images { get; set; }
		public bool is_custom_emoji { get; set; }
		public string name { get; set; }
		public List<string> search_terms { get; set; }
		public List<string> shortcuts { get; set; }
	}

	public class ChatDownloaderItem
	{
		public string action_type { get; set; }
		public Author author { get; set; }
		public List<Emote> emotes { get; set; }
		public string message { get; set; }
		public string message_id { get; set; }
		public string message_type { get; set; }
		public float time_in_seconds { get; set; }
		public string time_text { get; set; }
		public long timestamp { get; set; }
	}
}