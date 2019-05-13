namespace CsrpServer
{
	partial class Form1
	{
		/// <summary>
		/// 必要なデザイナー変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナーで生成されたコード

		/// <summary>
		/// デザイナー サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディターで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.loggingTextBox1 = new Rapidnack.Common.LoggingTextBox();
			this.SuspendLayout();
			// 
			// loggingTextBox1
			// 
			this.loggingTextBox1.BackColor = System.Drawing.SystemColors.Window;
			this.loggingTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.loggingTextBox1.Location = new System.Drawing.Point(0, 0);
			this.loggingTextBox1.Multiline = true;
			this.loggingTextBox1.Name = "loggingTextBox1";
			this.loggingTextBox1.ReadOnly = true;
			this.loggingTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.loggingTextBox1.Size = new System.Drawing.Size(480, 320);
			this.loggingTextBox1.TabIndex = 1;
			// 
			// Form1
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(480, 320);
			this.Controls.Add(this.loggingTextBox1);
			this.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Form1";
			this.Text = "CSRP Server";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private Rapidnack.Common.LoggingTextBox loggingTextBox1;
	}
}

