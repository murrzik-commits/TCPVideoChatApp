namespace Server_GUI
{
    partial class Server
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
            this.DisconnectButton = new System.Windows.Forms.Button();
            this.ExportButton = new System.Windows.Forms.Button();
            this.ServerTextBox = new System.Windows.Forms.TextBox();
            this.SendButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.pictureBoxRemoteVideo = new System.Windows.Forms.PictureBox();
            this.audioLevelPanel = new System.Windows.Forms.Panel();
            this.listBoxClients = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRemoteVideo)).BeginInit();
            this.SuspendLayout();
            // 
            // DisconnectButton
            // 
            this.DisconnectButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.DisconnectButton.Location = new System.Drawing.Point(674, 583);
            this.DisconnectButton.Name = "DisconnectButton";
            this.DisconnectButton.Size = new System.Drawing.Size(123, 21);
            this.DisconnectButton.TabIndex = 0;
            this.DisconnectButton.Text = "Disconnected";
            this.DisconnectButton.UseVisualStyleBackColor = false;
            // 
            // ExportButton
            // 
            this.ExportButton.ForeColor = System.Drawing.SystemColors.WindowText;
            this.ExportButton.Location = new System.Drawing.Point(876, 584);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(83, 20);
            this.ExportButton.TabIndex = 2;
            this.ExportButton.Text = "Export";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // ServerTextBox
            // 
            this.ServerTextBox.Location = new System.Drawing.Point(802, 557);
            this.ServerTextBox.Name = "ServerTextBox";
            this.ServerTextBox.Size = new System.Drawing.Size(246, 20);
            this.ServerTextBox.TabIndex = 3;
            // 
            // SendButton
            // 
            this.SendButton.ForeColor = System.Drawing.SystemColors.WindowText;
            this.SendButton.Location = new System.Drawing.Point(965, 584);
            this.SendButton.Name = "SendButton";
            this.SendButton.Size = new System.Drawing.Size(83, 20);
            this.SendButton.TabIndex = 4;
            this.SendButton.Text = "Send";
            this.SendButton.UseVisualStyleBackColor = true;
            this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.flowLayoutPanel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(803, 23);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(245, 530);
            this.flowLayoutPanel1.TabIndex = 5;
            // 
            // pictureBoxRemoteVideo
            // 
            this.pictureBoxRemoteVideo.Location = new System.Drawing.Point(35, 23);
            this.pictureBoxRemoteVideo.Name = "pictureBoxRemoteVideo";
            this.pictureBoxRemoteVideo.Size = new System.Drawing.Size(762, 554);
            this.pictureBoxRemoteVideo.TabIndex = 6;
            this.pictureBoxRemoteVideo.TabStop = false;
            // 
            // audioLevelPanel
            // 
            this.audioLevelPanel.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.audioLevelPanel.Location = new System.Drawing.Point(12, 23);
            this.audioLevelPanel.Name = "audioLevelPanel";
            this.audioLevelPanel.Size = new System.Drawing.Size(17, 581);
            this.audioLevelPanel.TabIndex = 7;
            // 
            // listBoxClients
            // 
            this.listBoxClients.FormattingEnabled = true;
            this.listBoxClients.Location = new System.Drawing.Point(374, 585);
            this.listBoxClients.Name = "listBoxClients";
            this.listBoxClients.Size = new System.Drawing.Size(294, 82);
            this.listBoxClients.TabIndex = 0;
            // 
            // Server
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1060, 679);
            this.Controls.Add(this.listBoxClients);
            this.Controls.Add(this.audioLevelPanel);
            this.Controls.Add(this.pictureBoxRemoteVideo);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.SendButton);
            this.Controls.Add(this.ServerTextBox);
            this.Controls.Add(this.ExportButton);
            this.Controls.Add(this.DisconnectButton);
            this.ForeColor = System.Drawing.Color.Snow;
            this.Name = "Server";
            this.Text = "Server";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRemoteVideo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button DisconnectButton;
        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.TextBox ServerTextBox;
        private System.Windows.Forms.Button SendButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.PictureBox pictureBoxRemoteVideo;
        private System.Windows.Forms.Panel audioLevelPanel;
        private System.Windows.Forms.ListBox listBoxClients;
    }
}

