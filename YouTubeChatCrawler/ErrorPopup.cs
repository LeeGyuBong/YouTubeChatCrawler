﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YouTubeChatCrawler
{
	public partial class ErrorPopup : Form
	{
		public ErrorPopup()
		{
			InitializeComponent();
		}

		private void button1_Click( object sender, EventArgs e )
		{
			this.Close();
		}
	}
}
