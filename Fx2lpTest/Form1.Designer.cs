namespace Fx2lpTest
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
			this.panel1 = new System.Windows.Forms.Panel();
			this.loggingTextBox1 = new Rapidnack.Common.LoggingTextBox();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(160, 320);
			this.panel1.TabIndex = 0;
			// 
			// loggingTextBox1
			// 
			this.loggingTextBox1.BackColor = System.Drawing.SystemColors.Window;
			this.loggingTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.loggingTextBox1.Location = new System.Drawing.Point(160, 0);
			this.loggingTextBox1.Multiline = true;
			this.loggingTextBox1.Name = "loggingTextBox1";
			this.loggingTextBox1.ReadOnly = true;
			this.loggingTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.loggingTextBox1.Size = new System.Drawing.Size(320, 320);
			this.loggingTextBox1.TabIndex = 1;
			// 
			// Form1
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(480, 320);
			this.Controls.Add(this.loggingTextBox1);
			this.Controls.Add(this.panel1);
			this.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.Name = "Form1";
			this.Text = "FX2LP Test";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		private Rapidnack.Common.LoggingTextBox loggingTextBox1;
	}
}

