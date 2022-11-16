namespace SPGenerator {
    partial class frmMain {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.btnConnect = new System.Windows.Forms.Button();
            this.edtPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblUserName = new System.Windows.Forms.Label();
            this.cbServers = new System.Windows.Forms.ComboBox();
            this.lblAuthentication = new System.Windows.Forms.Label();
            this.lblServers = new System.Windows.Forms.Label();
            this.cbAuthentication = new System.Windows.Forms.ComboBox();
            this.edtUserName = new System.Windows.Forms.TextBox();
            this.grpOperation = new System.Windows.Forms.GroupBox();
            this.cbCreateHistoryAndTrigger = new System.Windows.Forms.CheckBox();
            this.cbIncludeSPDescInHeader = new System.Windows.Forms.CheckBox();
            this.cbAddCommentHeader = new System.Windows.Forms.CheckBox();
            this.cbOmitDbo = new System.Windows.Forms.CheckBox();
            this.cbEncloseWithBrackets = new System.Windows.Forms.CheckBox();
            this.edtAuthor = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbDatabase = new System.Windows.Forms.ComboBox();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.edtSaveFolder = new System.Windows.Forms.TextBox();
            this.lblDestFolder = new System.Windows.Forms.Label();
            this.btnRun = new System.Windows.Forms.Button();
            this.grpDbConn = new System.Windows.Forms.GroupBox();
            this.grpLog = new System.Windows.Forms.GroupBox();
            this.txtLog = new System.Windows.Forms.RichTextBox();
            this.cbSavelLog = new System.Windows.Forms.CheckBox();
            this.grpOperation.SuspendLayout();
            this.grpDbConn.SuspendLayout();
            this.grpLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConnect.Location = new System.Drawing.Point(337, 117);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 4;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // edtPassword
            // 
            this.edtPassword.Location = new System.Drawing.Point(173, 119);
            this.edtPassword.Name = "edtPassword";
            this.edtPassword.PasswordChar = '*';
            this.edtPassword.Size = new System.Drawing.Size(154, 21);
            this.edtPassword.TabIndex = 3;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(170, 103);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(53, 13);
            this.lblPassword.TabIndex = 7;
            this.lblPassword.Text = "Password";
            // 
            // lblUserName
            // 
            this.lblUserName.AutoSize = true;
            this.lblUserName.Location = new System.Drawing.Point(17, 103);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(59, 13);
            this.lblUserName.TabIndex = 6;
            this.lblUserName.Text = "User Name";
            // 
            // cbServers
            // 
            this.cbServers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbServers.FormattingEnabled = true;
            this.cbServers.Location = new System.Drawing.Point(6, 35);
            this.cbServers.Name = "cbServers";
            this.cbServers.Size = new System.Drawing.Size(406, 21);
            this.cbServers.TabIndex = 0;
            this.cbServers.SelectedIndexChanged += new System.EventHandler(this.cbServers_SelectedIndexChanged);
            // 
            // lblAuthentication
            // 
            this.lblAuthentication.AutoSize = true;
            this.lblAuthentication.Location = new System.Drawing.Point(6, 59);
            this.lblAuthentication.Name = "lblAuthentication";
            this.lblAuthentication.Size = new System.Drawing.Size(77, 13);
            this.lblAuthentication.TabIndex = 3;
            this.lblAuthentication.Text = "Authentication";
            // 
            // lblServers
            // 
            this.lblServers.AutoSize = true;
            this.lblServers.Location = new System.Drawing.Point(6, 19);
            this.lblServers.Name = "lblServers";
            this.lblServers.Size = new System.Drawing.Size(69, 13);
            this.lblServers.TabIndex = 2;
            this.lblServers.Text = "Server Name";
            // 
            // cbAuthentication
            // 
            this.cbAuthentication.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAuthentication.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAuthentication.FormattingEnabled = true;
            this.cbAuthentication.Items.AddRange(new object[] {
            "SqlServer Authentication",
            "Windows Authentication"});
            this.cbAuthentication.Location = new System.Drawing.Point(6, 75);
            this.cbAuthentication.Name = "cbAuthentication";
            this.cbAuthentication.Size = new System.Drawing.Size(406, 21);
            this.cbAuthentication.TabIndex = 1;
            this.cbAuthentication.SelectedIndexChanged += new System.EventHandler(this.cbAuthentication_SelectedIndexChanged);
            // 
            // edtUserName
            // 
            this.edtUserName.Location = new System.Drawing.Point(20, 119);
            this.edtUserName.Name = "edtUserName";
            this.edtUserName.Size = new System.Drawing.Size(147, 21);
            this.edtUserName.TabIndex = 2;
            // 
            // grpOperation
            // 
            this.grpOperation.Controls.Add(this.cbSavelLog);
            this.grpOperation.Controls.Add(this.cbCreateHistoryAndTrigger);
            this.grpOperation.Controls.Add(this.cbIncludeSPDescInHeader);
            this.grpOperation.Controls.Add(this.cbAddCommentHeader);
            this.grpOperation.Controls.Add(this.cbOmitDbo);
            this.grpOperation.Controls.Add(this.cbEncloseWithBrackets);
            this.grpOperation.Controls.Add(this.edtAuthor);
            this.grpOperation.Controls.Add(this.label1);
            this.grpOperation.Controls.Add(this.cbDatabase);
            this.grpOperation.Controls.Add(this.lblDatabase);
            this.grpOperation.Controls.Add(this.btnBrowse);
            this.grpOperation.Controls.Add(this.edtSaveFolder);
            this.grpOperation.Controls.Add(this.lblDestFolder);
            this.grpOperation.Controls.Add(this.btnRun);
            this.grpOperation.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpOperation.Location = new System.Drawing.Point(3, 154);
            this.grpOperation.Name = "grpOperation";
            this.grpOperation.Size = new System.Drawing.Size(418, 198);
            this.grpOperation.TabIndex = 3;
            this.grpOperation.TabStop = false;
            this.grpOperation.Text = "Operation";
            // 
            // cbCreateHistoryAndTrigger
            // 
            this.cbCreateHistoryAndTrigger.AutoSize = true;
            this.cbCreateHistoryAndTrigger.Location = new System.Drawing.Point(184, 176);
            this.cbCreateHistoryAndTrigger.Name = "cbCreateHistoryAndTrigger";
            this.cbCreateHistoryAndTrigger.Size = new System.Drawing.Size(188, 17);
            this.cbCreateHistoryAndTrigger.TabIndex = 13;
            this.cbCreateHistoryAndTrigger.Text = "Create History Table and Triggers";
            this.cbCreateHistoryAndTrigger.UseVisualStyleBackColor = true;
            // 
            // cbIncludeSPDescInHeader
            // 
            this.cbIncludeSPDescInHeader.AutoSize = true;
            this.cbIncludeSPDescInHeader.Location = new System.Drawing.Point(200, 160);
            this.cbIncludeSPDescInHeader.Name = "cbIncludeSPDescInHeader";
            this.cbIncludeSPDescInHeader.Size = new System.Drawing.Size(180, 17);
            this.cbIncludeSPDescInHeader.TabIndex = 12;
            this.cbIncludeSPDescInHeader.Text = "Include SP Description in header";
            this.cbIncludeSPDescInHeader.UseVisualStyleBackColor = true;
            // 
            // cbAddCommentHeader
            // 
            this.cbAddCommentHeader.AutoSize = true;
            this.cbAddCommentHeader.Location = new System.Drawing.Point(184, 144);
            this.cbAddCommentHeader.Name = "cbAddCommentHeader";
            this.cbAddCommentHeader.Size = new System.Drawing.Size(131, 17);
            this.cbAddCommentHeader.TabIndex = 11;
            this.cbAddCommentHeader.Text = "Add Comment Header";
            this.cbAddCommentHeader.UseVisualStyleBackColor = true;
            this.cbAddCommentHeader.CheckedChanged += new System.EventHandler(this.cbAddCommentHeader_CheckedChanged);
            // 
            // cbOmitDbo
            // 
            this.cbOmitDbo.AutoSize = true;
            this.cbOmitDbo.Location = new System.Drawing.Point(8, 160);
            this.cbOmitDbo.Name = "cbOmitDbo";
            this.cbOmitDbo.Size = new System.Drawing.Size(128, 17);
            this.cbOmitDbo.TabIndex = 10;
            this.cbOmitDbo.Text = "Omit dbo from names";
            this.cbOmitDbo.UseVisualStyleBackColor = true;
            // 
            // cbEncloseWithBrackets
            // 
            this.cbEncloseWithBrackets.AutoSize = true;
            this.cbEncloseWithBrackets.Location = new System.Drawing.Point(8, 144);
            this.cbEncloseWithBrackets.Name = "cbEncloseWithBrackets";
            this.cbEncloseWithBrackets.Size = new System.Drawing.Size(133, 17);
            this.cbEncloseWithBrackets.TabIndex = 9;
            this.cbEncloseWithBrackets.Text = "Enclose names with [ ]";
            this.cbEncloseWithBrackets.UseVisualStyleBackColor = true;
            // 
            // edtAuthor
            // 
            this.edtAuthor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.edtAuthor.Location = new System.Drawing.Point(6, 73);
            this.edtAuthor.Name = "edtAuthor";
            this.edtAuthor.Size = new System.Drawing.Size(406, 21);
            this.edtAuthor.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Author";
            // 
            // cbDatabase
            // 
            this.cbDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDatabase.FormattingEnabled = true;
            this.cbDatabase.Location = new System.Drawing.Point(6, 33);
            this.cbDatabase.Name = "cbDatabase";
            this.cbDatabase.Size = new System.Drawing.Size(406, 21);
            this.cbDatabase.TabIndex = 0;
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(9, 17);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(53, 13);
            this.lblDatabase.TabIndex = 5;
            this.lblDatabase.Text = "Database";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(311, 114);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(19, 19);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "…";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // edtSaveFolder
            // 
            this.edtSaveFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.edtSaveFolder.Location = new System.Drawing.Point(6, 113);
            this.edtSaveFolder.Name = "edtSaveFolder";
            this.edtSaveFolder.Size = new System.Drawing.Size(325, 21);
            this.edtSaveFolder.TabIndex = 1;
            // 
            // lblDestFolder
            // 
            this.lblDestFolder.AutoSize = true;
            this.lblDestFolder.Location = new System.Drawing.Point(6, 97);
            this.lblDestFolder.Name = "lblDestFolder";
            this.lblDestFolder.Size = new System.Drawing.Size(64, 13);
            this.lblDestFolder.TabIndex = 1;
            this.lblDestFolder.Text = "Save Folder";
            // 
            // btnRun
            // 
            this.btnRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRun.Location = new System.Drawing.Point(336, 112);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 23);
            this.btnRun.TabIndex = 4;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // grpDbConn
            // 
            this.grpDbConn.Controls.Add(this.btnConnect);
            this.grpDbConn.Controls.Add(this.edtPassword);
            this.grpDbConn.Controls.Add(this.lblPassword);
            this.grpDbConn.Controls.Add(this.lblUserName);
            this.grpDbConn.Controls.Add(this.cbServers);
            this.grpDbConn.Controls.Add(this.lblAuthentication);
            this.grpDbConn.Controls.Add(this.lblServers);
            this.grpDbConn.Controls.Add(this.cbAuthentication);
            this.grpDbConn.Controls.Add(this.edtUserName);
            this.grpDbConn.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpDbConn.Location = new System.Drawing.Point(3, 3);
            this.grpDbConn.Name = "grpDbConn";
            this.grpDbConn.Size = new System.Drawing.Size(418, 151);
            this.grpDbConn.TabIndex = 0;
            this.grpDbConn.TabStop = false;
            this.grpDbConn.Text = "Database Connection";
            // 
            // grpLog
            // 
            this.grpLog.Controls.Add(this.txtLog);
            this.grpLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpLog.Location = new System.Drawing.Point(3, 352);
            this.grpLog.Name = "grpLog";
            this.grpLog.Size = new System.Drawing.Size(418, 106);
            this.grpLog.TabIndex = 4;
            this.grpLog.TabStop = false;
            this.grpLog.Text = "Log";
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Location = new System.Drawing.Point(6, 20);
            this.txtLog.Name = "txtLog";
            this.txtLog.Size = new System.Drawing.Size(406, 80);
            this.txtLog.TabIndex = 8;
            this.txtLog.Text = "";
            // 
            // cbSavelLog
            // 
            this.cbSavelLog.AutoSize = true;
            this.cbSavelLog.Location = new System.Drawing.Point(8, 176);
            this.cbSavelLog.Name = "cbSavelLog";
            this.cbSavelLog.Size = new System.Drawing.Size(102, 17);
            this.cbSavelLog.TabIndex = 14;
            this.cbSavelLog.Text = "Save Log to File";
            this.cbSavelLog.UseVisualStyleBackColor = true;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(424, 461);
            this.Controls.Add(this.grpLog);
            this.Controls.Add(this.grpOperation);
            this.Controls.Add(this.grpDbConn);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(440, 500);
            this.Name = "frmMain";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Text = "Proc Generator";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMain_FormClosed);
            this.Shown += new System.EventHandler(this.frmMain_Shown);
            this.grpOperation.ResumeLayout(false);
            this.grpOperation.PerformLayout();
            this.grpDbConn.ResumeLayout(false);
            this.grpDbConn.PerformLayout();
            this.grpLog.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblAuthentication;
        private System.Windows.Forms.Label lblServers;
        private System.Windows.Forms.ComboBox cbAuthentication;
        private System.Windows.Forms.TextBox edtUserName;
        private System.Windows.Forms.ComboBox cbServers;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox edtPassword;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.GroupBox grpOperation;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox edtSaveFolder;
        private System.Windows.Forms.Label lblDestFolder;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.ComboBox cbDatabase;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.TextBox edtAuthor;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox grpDbConn;
        private System.Windows.Forms.GroupBox grpLog;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.CheckBox cbOmitDbo;
        private System.Windows.Forms.CheckBox cbEncloseWithBrackets;
        private System.Windows.Forms.CheckBox cbIncludeSPDescInHeader;
        private System.Windows.Forms.CheckBox cbAddCommentHeader;
        private System.Windows.Forms.CheckBox cbCreateHistoryAndTrigger;
        private System.Windows.Forms.CheckBox cbSavelLog;
    }
}