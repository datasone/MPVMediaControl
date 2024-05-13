namespace MPVMediaControl
{
    partial class CommandEditor
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
            this.playCmdRstBtn = new System.Windows.Forms.Button();
            this.saveBtn = new System.Windows.Forms.Button();
            this.playCmdText = new System.Windows.Forms.TextBox();
            this.infoLabel = new System.Windows.Forms.Label();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.playLabel = new System.Windows.Forms.Label();
            this.pauseLabel = new System.Windows.Forms.Label();
            this.pauseCmdText = new System.Windows.Forms.TextBox();
            this.pauseCmdRstBtn = new System.Windows.Forms.Button();
            this.prevLabel = new System.Windows.Forms.Label();
            this.prevCmdText = new System.Windows.Forms.TextBox();
            this.prevCmdRstBtn = new System.Windows.Forms.Button();
            this.nextLabel = new System.Windows.Forms.Label();
            this.nextCmdText = new System.Windows.Forms.TextBox();
            this.nextCmdRstBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // playCmdRstBtn
            // 
            this.playCmdRstBtn.Location = new System.Drawing.Point(695, 85);
            this.playCmdRstBtn.Name = "playCmdRstBtn";
            this.playCmdRstBtn.Size = new System.Drawing.Size(90, 41);
            this.playCmdRstBtn.TabIndex = 0;
            this.playCmdRstBtn.Text = "Reset";
            this.playCmdRstBtn.UseVisualStyleBackColor = true;
            this.playCmdRstBtn.Click += new System.EventHandler(this.button1_Click);
            // 
            // saveBtn
            // 
            this.saveBtn.Location = new System.Drawing.Point(599, 273);
            this.saveBtn.Name = "saveBtn";
            this.saveBtn.Size = new System.Drawing.Size(90, 41);
            this.saveBtn.TabIndex = 1;
            this.saveBtn.Text = "Save";
            this.saveBtn.UseVisualStyleBackColor = true;
            this.saveBtn.Click += new System.EventHandler(this.button2_Click);
            // 
            // playCmdText
            // 
            this.playCmdText.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.playCmdText.Location = new System.Drawing.Point(119, 90);
            this.playCmdText.Name = "playCmdText";
            this.playCmdText.Size = new System.Drawing.Size(570, 36);
            this.playCmdText.TabIndex = 2;
            // 
            // infoLabel
            // 
            this.infoLabel.AutoSize = true;
            this.infoLabel.Location = new System.Drawing.Point(13, 13);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(710, 48);
            this.infoLabel.TabIndex = 3;
            this.infoLabel.Text = "This edits commands sent to mpv while corresponding media controls are activated," +
    "\nuseful if you want to remap media keys.";
            // 
            // cancelBtn
            // 
            this.cancelBtn.Location = new System.Drawing.Point(695, 273);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(90, 41);
            this.cancelBtn.TabIndex = 4;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.button3_Click);
            // 
            // playLabel
            // 
            this.playLabel.AutoSize = true;
            this.playLabel.Location = new System.Drawing.Point(17, 96);
            this.playLabel.Name = "playLabel";
            this.playLabel.Size = new System.Drawing.Size(50, 24);
            this.playLabel.TabIndex = 5;
            this.playLabel.Text = "Play:";
            // 
            // pauseLabel
            // 
            this.pauseLabel.AutoSize = true;
            this.pauseLabel.Location = new System.Drawing.Point(17, 143);
            this.pauseLabel.Name = "pauseLabel";
            this.pauseLabel.Size = new System.Drawing.Size(68, 24);
            this.pauseLabel.TabIndex = 8;
            this.pauseLabel.Text = "Pause:";
            // 
            // pauseCmdText
            // 
            this.pauseCmdText.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pauseCmdText.Location = new System.Drawing.Point(119, 137);
            this.pauseCmdText.Name = "pauseCmdText";
            this.pauseCmdText.Size = new System.Drawing.Size(570, 36);
            this.pauseCmdText.TabIndex = 7;
            // 
            // pauseCmdRstBtn
            // 
            this.pauseCmdRstBtn.Location = new System.Drawing.Point(695, 132);
            this.pauseCmdRstBtn.Name = "pauseCmdRstBtn";
            this.pauseCmdRstBtn.Size = new System.Drawing.Size(90, 41);
            this.pauseCmdRstBtn.TabIndex = 6;
            this.pauseCmdRstBtn.Text = "Reset";
            this.pauseCmdRstBtn.UseVisualStyleBackColor = true;
            this.pauseCmdRstBtn.Click += new System.EventHandler(this.button4_Click);
            // 
            // prevLabel
            // 
            this.prevLabel.AutoSize = true;
            this.prevLabel.Location = new System.Drawing.Point(17, 187);
            this.prevLabel.Name = "prevLabel";
            this.prevLabel.Size = new System.Drawing.Size(88, 24);
            this.prevLabel.TabIndex = 11;
            this.prevLabel.Text = "Previous:";
            // 
            // prevCmdText
            // 
            this.prevCmdText.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.prevCmdText.Location = new System.Drawing.Point(119, 184);
            this.prevCmdText.Name = "prevCmdText";
            this.prevCmdText.Size = new System.Drawing.Size(570, 36);
            this.prevCmdText.TabIndex = 10;
            // 
            // prevCmdRstBtn
            // 
            this.prevCmdRstBtn.Location = new System.Drawing.Point(695, 179);
            this.prevCmdRstBtn.Name = "prevCmdRstBtn";
            this.prevCmdRstBtn.Size = new System.Drawing.Size(90, 41);
            this.prevCmdRstBtn.TabIndex = 9;
            this.prevCmdRstBtn.Text = "Reset";
            this.prevCmdRstBtn.UseVisualStyleBackColor = true;
            this.prevCmdRstBtn.Click += new System.EventHandler(this.button5_Click);
            // 
            // nextLabel
            // 
            this.nextLabel.AutoSize = true;
            this.nextLabel.Location = new System.Drawing.Point(17, 237);
            this.nextLabel.Name = "nextLabel";
            this.nextLabel.Size = new System.Drawing.Size(54, 24);
            this.nextLabel.TabIndex = 14;
            this.nextLabel.Text = "Next:";
            // 
            // nextCmdText
            // 
            this.nextCmdText.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nextCmdText.Location = new System.Drawing.Point(119, 231);
            this.nextCmdText.Name = "nextCmdText";
            this.nextCmdText.Size = new System.Drawing.Size(570, 36);
            this.nextCmdText.TabIndex = 13;
            // 
            // nextCmdRstBtn
            // 
            this.nextCmdRstBtn.Location = new System.Drawing.Point(695, 226);
            this.nextCmdRstBtn.Name = "nextCmdRstBtn";
            this.nextCmdRstBtn.Size = new System.Drawing.Size(90, 41);
            this.nextCmdRstBtn.TabIndex = 12;
            this.nextCmdRstBtn.Text = "Reset";
            this.nextCmdRstBtn.UseVisualStyleBackColor = true;
            this.nextCmdRstBtn.Click += new System.EventHandler(this.button6_Click);
            // 
            // CommandEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(797, 326);
            this.Controls.Add(this.nextLabel);
            this.Controls.Add(this.nextCmdText);
            this.Controls.Add(this.nextCmdRstBtn);
            this.Controls.Add(this.prevLabel);
            this.Controls.Add(this.prevCmdText);
            this.Controls.Add(this.prevCmdRstBtn);
            this.Controls.Add(this.pauseLabel);
            this.Controls.Add(this.pauseCmdText);
            this.Controls.Add(this.pauseCmdRstBtn);
            this.Controls.Add(this.playLabel);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.infoLabel);
            this.Controls.Add(this.playCmdText);
            this.Controls.Add(this.saveBtn);
            this.Controls.Add(this.playCmdRstBtn);
            this.Name = "CommandEditor";
            this.Text = "CommandEditor";
            this.Load += new System.EventHandler(this.CommandEditor_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button playCmdRstBtn;
        private System.Windows.Forms.Button saveBtn;
        private System.Windows.Forms.TextBox playCmdText;
        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Label playLabel;
        private System.Windows.Forms.Label pauseLabel;
        private System.Windows.Forms.TextBox pauseCmdText;
        private System.Windows.Forms.Button pauseCmdRstBtn;
        private System.Windows.Forms.Label prevLabel;
        private System.Windows.Forms.TextBox prevCmdText;
        private System.Windows.Forms.Button prevCmdRstBtn;
        private System.Windows.Forms.Label nextLabel;
        private System.Windows.Forms.TextBox nextCmdText;
        private System.Windows.Forms.Button nextCmdRstBtn;
    }
}