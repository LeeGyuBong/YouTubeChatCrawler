namespace YouTubeChatCrawler
{
	partial class Form1
	{
		/// <summary>
		/// 필수 디자이너 변수입니다.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 사용 중인 모든 리소스를 정리합니다.
		/// </summary>
		/// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form 디자이너에서 생성한 코드

		/// <summary>
		/// 디자이너 지원에 필요한 메서드입니다. 
		/// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.VideoUrl = new System.Windows.Forms.TextBox();
			this.SearchStart = new System.Windows.Forms.Button();
			this.IsLive = new System.Windows.Forms.CheckBox();
			this.CommentViewr = new System.Windows.Forms.ListView();
			this.Export = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(20, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "Url";
			// 
			// VideoUrl
			// 
			this.VideoUrl.Location = new System.Drawing.Point(39, 10);
			this.VideoUrl.Name = "VideoUrl";
			this.VideoUrl.Size = new System.Drawing.Size(322, 21);
			this.VideoUrl.TabIndex = 1;
			// 
			// SearchStart
			// 
			this.SearchStart.Location = new System.Drawing.Point(434, 8);
			this.SearchStart.Name = "SearchStart";
			this.SearchStart.Size = new System.Drawing.Size(75, 23);
			this.SearchStart.TabIndex = 2;
			this.SearchStart.Text = "시작";
			this.SearchStart.UseVisualStyleBackColor = true;
			this.SearchStart.Click += new System.EventHandler(this.SearchStart_Click);
			// 
			// IsLive
			// 
			this.IsLive.AutoSize = true;
			this.IsLive.Location = new System.Drawing.Point(368, 13);
			this.IsLive.Name = "IsLive";
			this.IsLive.Size = new System.Drawing.Size(60, 16);
			this.IsLive.TabIndex = 3;
			this.IsLive.Text = "라이브";
			this.IsLive.UseVisualStyleBackColor = true;
			// 
			// CommentViewr
			// 
			this.CommentViewr.HideSelection = false;
			this.CommentViewr.Location = new System.Drawing.Point(12, 37);
			this.CommentViewr.Name = "CommentViewr";
			this.CommentViewr.Size = new System.Drawing.Size(776, 401);
			this.CommentViewr.TabIndex = 4;
			this.CommentViewr.UseCompatibleStateImageBehavior = false;
			// 
			// Export
			// 
			this.Export.Location = new System.Drawing.Point(713, 8);
			this.Export.Name = "Export";
			this.Export.Size = new System.Drawing.Size(75, 23);
			this.Export.TabIndex = 5;
			this.Export.Text = "추출";
			this.Export.UseVisualStyleBackColor = true;
			this.Export.Click += new System.EventHandler(this.Export_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.Export);
			this.Controls.Add(this.CommentViewr);
			this.Controls.Add(this.IsLive);
			this.Controls.Add(this.SearchStart);
			this.Controls.Add(this.VideoUrl);
			this.Controls.Add(this.label1);
			this.Name = "Form1";
			this.Text = "Main";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox VideoUrl;
		private System.Windows.Forms.Button SearchStart;
		private System.Windows.Forms.CheckBox IsLive;
		private System.Windows.Forms.Button Export;
		private System.Windows.Forms.ListView CommentViewr;
	}
}

