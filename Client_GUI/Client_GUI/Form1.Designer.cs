using System.Windows.Forms;

namespace Client_GUI
{
    partial class Client
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ExportButton = new System.Windows.Forms.Button();
            this.SendButton = new System.Windows.Forms.Button();
            this.ClientTextBox = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.VideoCallButton = new System.Windows.Forms.Button();
            this.PictureBoxLocalVideo = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxLocalVideo)).BeginInit();
            this.SuspendLayout();
            // 
            // ExportButton
            // 
            this.ExportButton.Location = new System.Drawing.Point(520, 343);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(60, 22);
            this.ExportButton.TabIndex = 2;
            this.ExportButton.Text = "Экспорт";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // SendButton
            // 
            this.SendButton.Location = new System.Drawing.Point(586, 343);
            this.SendButton.Name = "SendButton";
            this.SendButton.Size = new System.Drawing.Size(84, 22);
            this.SendButton.TabIndex = 3;
            this.SendButton.Text = "Отправить";
            this.SendButton.UseVisualStyleBackColor = true;
            this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // ClientTextBox
            // 
            this.ClientTextBox.Location = new System.Drawing.Point(420, 317);
            this.ClientTextBox.Name = "ClientTextBox";
            this.ClientTextBox.Size = new System.Drawing.Size(250, 20);
            this.ClientTextBox.TabIndex = 4;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.flowLayoutPanel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(420, 37);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(250, 274);
            this.flowLayoutPanel1.TabIndex = 5;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // VideoCallButton
            // 
            this.VideoCallButton.Location = new System.Drawing.Point(312, 343);
            this.VideoCallButton.Name = "VideoCallButton";
            this.VideoCallButton.Size = new System.Drawing.Size(100, 22);
            this.VideoCallButton.TabIndex = 6;
            this.VideoCallButton.Text = "Трансляция";
            this.VideoCallButton.UseVisualStyleBackColor = true;
            this.VideoCallButton.Click += new System.EventHandler(this.VideoCallButton_Click);
            // 
            // PictureBoxLocalVideo
            // 
            this.PictureBoxLocalVideo.BackColor = System.Drawing.Color.DarkGray;
            this.PictureBoxLocalVideo.Location = new System.Drawing.Point(12, 37);
            this.PictureBoxLocalVideo.Name = "PictureBoxLocalVideo";
            this.PictureBoxLocalVideo.Size = new System.Drawing.Size(400, 300);
            this.PictureBoxLocalVideo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PictureBoxLocalVideo.TabIndex = 7;
            this.PictureBoxLocalVideo.TabStop = false;
            // 
            // Client
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(683, 383);
            this.Controls.Add(this.PictureBoxLocalVideo);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.ClientTextBox);
            this.Controls.Add(this.SendButton);
            this.Controls.Add(this.ExportButton);
            this.Controls.Add(this.VideoCallButton);
            this.Name = "Client";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Видеочат";
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxLocalVideo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.Button SendButton;
        private System.Windows.Forms.TextBox ClientTextBox;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button VideoCallButton;
        private System.Windows.Forms.PictureBox PictureBoxLocalVideo;
    }
}

