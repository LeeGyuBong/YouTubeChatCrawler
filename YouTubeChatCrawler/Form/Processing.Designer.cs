namespace YouTubeChatCrawler
{
	partial class Processing
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ProcessingView = new System.Windows.Forms.ListView();
			this.SuspendLayout();
			// 
			// ProcessingView
			// 
			this.ProcessingView.HideSelection = false;
			this.ProcessingView.Location = new System.Drawing.Point(12, 12);
			this.ProcessingView.Name = "ProcessingView";
			this.ProcessingView.Size = new System.Drawing.Size(776, 426);
			this.ProcessingView.TabIndex = 0;
			this.ProcessingView.UseCompatibleStateImageBehavior = false;
			// 
			// Processing
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.ProcessingView);
			this.Cursor = System.Windows.Forms.Cursors.Default;
			this.MaximizeBox = false;
			this.Name = "Processing";
			this.Text = "Processing";
			this.ResumeLayout(false);

		}

		#endregion

		public System.Windows.Forms.ListView ProcessingView;
	}
}