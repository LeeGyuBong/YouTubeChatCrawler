using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.ListView;

namespace YouTubeChatCrawler
{
	public partial class Form1 : Form
	{
		Thread childThread = null;

		public Form1()
		{
			YouTubeLib.Instance.Init();
			InitializeComponent();
		}

		private void SearchStart_Click( object sender, EventArgs e )
		{
			string url = VideoUrl.Text;
			if ( string.IsNullOrEmpty( url ) )
			{
				ErrorPopup popup = new ErrorPopup();
				popup.ShowDialog();

				return;
			}

			if( childThread != null )
			{
				childThread.Interrupt();
				childThread.Join();
			}

			string videoId = url.Split( '=' ).Last();

			if ( IsLive.Checked )
			{
				// 라이브
				string liveChatId = YouTubeLib.Instance.GetliveChatID( videoId );

				CommentViewr.Clear();

				CommentViewr.View = View.Details;

				childThread = new Thread( GetLiveAsync );
				childThread.IsBackground = true;
				childThread.Start( liveChatId );

				CommentViewr.Columns.Add( "아이디", 100, HorizontalAlignment.Left );
				CommentViewr.Columns.Add( "댓글", 420, HorizontalAlignment.Left );
				CommentViewr.Columns.Add( "날짜", 150, HorizontalAlignment.Left );
			}
			else
			{
				// 일반 영상
				List<CommentInfo> commentInfos = new List<CommentInfo>();
				YouTubeLib.Instance.GetComment( commentInfos, videoId, 1, null );

				CommentViewr.Clear();
				CommentViewr.View = View.Details;

				foreach ( CommentInfo comment in commentInfos )
				{
					string No;
					if ( comment.ChildNo == 0 )
					{
						No = string.Format( "{0:0000}", comment.ParentNo );
					}
					else
					{
						No = string.Format( "{0:0000}-{1:000}", comment.ParentNo, comment.ChildNo );
					}

					ListViewItem item = new ListViewItem( No );
					item.SubItems.Add( comment.AuthorName );
					item.SubItems.Add( comment.Text );
					item.SubItems.Add( comment.PublishedAt.ToString( "s" ) );

					CommentViewr.Items.Add( item );
				}

				CommentViewr.Columns.Add( "No", 100, HorizontalAlignment.Left );
				CommentViewr.Columns.Add( "아이디", 100, HorizontalAlignment.Left );
				CommentViewr.Columns.Add( "댓글", 420, HorizontalAlignment.Left );
				CommentViewr.Columns.Add( "날짜", 150, HorizontalAlignment.Left );
			}
		}

		private void GetLiveAsync( Object liveChatId )
		{
			List<CommentInfo> commentInfos = new List<CommentInfo>();

			try
			{
				string nextToken = null;
				bool search = true;
				while ( search )
				{
					nextToken = YouTubeLib.Instance.GetLiveMessage( commentInfos, liveChatId.ToString(), nextToken ).GetAwaiter().GetResult();

					int totalCount = commentInfos.Count;
					int i = 0; 
					foreach ( CommentInfo comment in commentInfos )
					{
						ListViewItem item = new ListViewItem( comment.AuthorName );
						item.SubItems.Add( comment.Text );
						item.SubItems.Add( comment.PublishedAt.ToString( "s" ) );

						i++;
						CommentViewr.Invoke( new Action( () =>
						{
							CommentViewr.Items.Add( item );
							if( totalCount == i )
								CommentViewr.TopItem = CommentViewr.Items[ CommentViewr.Items.Count - 1 ];
						} ) );
					}

					if ( nextToken == null )
						search = false;

					commentInfos.Clear();
				}
			}
			catch ( Exception )
			{
			}
		}

		private void Export_Click( object sender, EventArgs e )
		{
			if ( CommentViewr.Items.Count <= 0 )
			{
				return;
			}

			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Title = "저장경로를 지정하세요.";
			saveFileDialog.OverwritePrompt = true;
			saveFileDialog.Filter = "csv (*.csv)|*.csv|모든파일(*.*)|*.*";
			saveFileDialog.InitialDirectory = "C:\\";

			string fileName = "Export";
			if( saveFileDialog.ShowDialog() == DialogResult.OK )
			{
				fileName = saveFileDialog.FileName;

				//https://blog.live2skull.kr/csharp/excel/csharp-excel-export/
				FileInfo excelFile = new FileInfo( fileName );
				if ( excelFile.Exists ) excelFile.Delete();

				string[] sheets = new string[] { "데이터" };
				ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

				using ( ExcelPackage excel = new ExcelPackage() )
				{
					excel.Workbook.Worksheets.Add( sheets[ 0 ] );

					ExcelWorksheet sheet = excel.Workbook.Worksheets[ sheets[ 0 ] ];

					List<object[]> dataRows = new List<object[]>();
					for ( int i = 0; i < CommentViewr.Items.Count; ++i )
					{
						var thisItem = CommentViewr.Items[ i ];
						int ColumnCount = thisItem.SubItems.Count;
						object[] dataRow = new object[ ColumnCount ];

						if ( i == 0 )
						{
							// 칼럼 헤더 추가
							object[] HeaderRow = new object[ ColumnCount ];
							for ( int columnCount = 0; columnCount < ColumnCount; ++columnCount )
								HeaderRow[ columnCount ] = CommentViewr.Columns[ columnCount ].Text;

							dataRows.Add( HeaderRow );
						}

						for ( int j = 0; j < ColumnCount; ++j )
							dataRow[ j ] = thisItem.SubItems[ j ].Text;

						dataRows.Add( dataRow );
					}

					string headerRange = String.Format( "B2:{0}2", Char.ConvertFromUtf32( CommentViewr.Items.Count + 64 ) );
					sheet.Cells[ headerRange ].LoadFromArrays( dataRows );

					excel.SaveAs( excelFile );
				}

				SuccesscsPopup popup = new SuccesscsPopup();
				popup.ShowDialog();
			}
		}
	}
}
