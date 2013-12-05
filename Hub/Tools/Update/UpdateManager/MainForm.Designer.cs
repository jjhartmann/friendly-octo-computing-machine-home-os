namespace HomeOS.Hub.Tools.UpdateManager
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.controlMainFormTab = new System.Windows.Forms.TabControl();
            this.tabSetup = new System.Windows.Forms.TabPage();
            this.tableLayoutPanelSetup = new System.Windows.Forms.TableLayoutPanel();
            this.labelSetupWorkingFolderHelp = new System.Windows.Forms.Label();
            this.maskTextBoxSetupWorkingFolder = new System.Windows.Forms.MaskedTextBox();
            this.maskTextBoxSetupRepoAcctInfo = new System.Windows.Forms.MaskedTextBox();
            this.maskTextBoxAzureAcctInfo = new System.Windows.Forms.MaskedTextBox();
            this.btnSetupValidate = new System.Windows.Forms.Button();
            this.btnSetupRemoveRepoAcctInfo = new System.Windows.Forms.Button();
            this.btnSetupRemoveAzureAcctInfo = new System.Windows.Forms.Button();
            this.labelSetupValidateHelp = new System.Windows.Forms.Label();
            this.labelSetupAzureAccount = new System.Windows.Forms.Label();
            this.labelSetupRepoAccount = new System.Windows.Forms.Label();
            this.btnSetupBrowseFolder = new System.Windows.Forms.Button();
            this.btnSetupEditRepoAcctInfo = new System.Windows.Forms.Button();
            this.btnSetupEditAzureAcctInfo = new System.Windows.Forms.Button();
            this.tabUpdateHubs = new System.Windows.Forms.TabPage();
            this.folderBrowserWorkingDir = new System.Windows.Forms.FolderBrowserDialog();
            this.controlMainFormTab.SuspendLayout();
            this.tabSetup.SuspendLayout();
            this.tableLayoutPanelSetup.SuspendLayout();
            this.SuspendLayout();
            // 
            // controlMainFormTab
            // 
            this.controlMainFormTab.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.controlMainFormTab.Controls.Add(this.tabSetup);
            this.controlMainFormTab.Controls.Add(this.tabUpdateHubs);
            this.controlMainFormTab.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.controlMainFormTab.HotTrack = true;
            this.controlMainFormTab.Location = new System.Drawing.Point(0, 1);
            this.controlMainFormTab.Name = "controlMainFormTab";
            this.controlMainFormTab.SelectedIndex = 0;
            this.controlMainFormTab.Size = new System.Drawing.Size(643, 425);
            this.controlMainFormTab.TabIndex = 0;
            // 
            // tabSetup
            // 
            this.tabSetup.Controls.Add(this.tableLayoutPanelSetup);
            this.tabSetup.Location = new System.Drawing.Point(4, 25);
            this.tabSetup.Name = "tabSetup";
            this.tabSetup.Padding = new System.Windows.Forms.Padding(3);
            this.tabSetup.Size = new System.Drawing.Size(635, 396);
            this.tabSetup.TabIndex = 0;
            this.tabSetup.Text = "Set Up";
            this.tabSetup.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanelSetup
            // 
            this.tableLayoutPanelSetup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanelSetup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.tableLayoutPanelSetup.ColumnCount = 5;
            this.tableLayoutPanelSetup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelSetup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelSetup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 76F));
            this.tableLayoutPanelSetup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 78F));
            this.tableLayoutPanelSetup.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 157F));
            this.tableLayoutPanelSetup.Controls.Add(this.labelSetupWorkingFolderHelp, 0, 0);
            this.tableLayoutPanelSetup.Controls.Add(this.maskTextBoxSetupWorkingFolder, 0, 1);
            this.tableLayoutPanelSetup.Controls.Add(this.maskTextBoxSetupRepoAcctInfo, 0, 3);
            this.tableLayoutPanelSetup.Controls.Add(this.maskTextBoxAzureAcctInfo, 0, 5);
            this.tableLayoutPanelSetup.Controls.Add(this.btnSetupValidate, 3, 1);
            this.tableLayoutPanelSetup.Controls.Add(this.btnSetupRemoveRepoAcctInfo, 3, 3);
            this.tableLayoutPanelSetup.Controls.Add(this.btnSetupRemoveAzureAcctInfo, 3, 5);
            this.tableLayoutPanelSetup.Controls.Add(this.labelSetupValidateHelp, 4, 1);
            this.tableLayoutPanelSetup.Controls.Add(this.labelSetupAzureAccount, 0, 4);
            this.tableLayoutPanelSetup.Controls.Add(this.labelSetupRepoAccount, 0, 2);
            this.tableLayoutPanelSetup.Controls.Add(this.btnSetupBrowseFolder, 2, 1);
            this.tableLayoutPanelSetup.Controls.Add(this.btnSetupEditRepoAcctInfo, 2, 3);
            this.tableLayoutPanelSetup.Controls.Add(this.btnSetupEditAzureAcctInfo, 2, 5);
            this.tableLayoutPanelSetup.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelSetup.Name = "tableLayoutPanelSetup";
            this.tableLayoutPanelSetup.RowCount = 6;
            this.tableLayoutPanelSetup.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.72464F));
            this.tableLayoutPanelSetup.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 49.27536F));
            this.tableLayoutPanelSetup.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 73F));
            this.tableLayoutPanelSetup.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 91F));
            this.tableLayoutPanelSetup.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanelSetup.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 145F));
            this.tableLayoutPanelSetup.Size = new System.Drawing.Size(635, 398);
            this.tableLayoutPanelSetup.TabIndex = 0;
            // 
            // labelSetupWorkingFolderHelp
            // 
            this.labelSetupWorkingFolderHelp.AutoSize = true;
            this.tableLayoutPanelSetup.SetColumnSpan(this.labelSetupWorkingFolderHelp, 5);
            this.labelSetupWorkingFolderHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelSetupWorkingFolderHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSetupWorkingFolderHelp.Location = new System.Drawing.Point(3, 0);
            this.labelSetupWorkingFolderHelp.Name = "labelSetupWorkingFolderHelp";
            this.labelSetupWorkingFolderHelp.Size = new System.Drawing.Size(629, 30);
            this.labelSetupWorkingFolderHelp.TabIndex = 9;
            this.labelSetupWorkingFolderHelp.Text = "Working folder location (where the golden bits you want to deploy are)";
            this.labelSetupWorkingFolderHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // maskTextBoxSetupWorkingFolder
            // 
            this.maskTextBoxSetupWorkingFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanelSetup.SetColumnSpan(this.maskTextBoxSetupWorkingFolder, 2);
            this.maskTextBoxSetupWorkingFolder.Location = new System.Drawing.Point(3, 33);
            this.maskTextBoxSetupWorkingFolder.Name = "maskTextBoxSetupWorkingFolder";
            this.maskTextBoxSetupWorkingFolder.Size = new System.Drawing.Size(318, 20);
            this.maskTextBoxSetupWorkingFolder.TabIndex = 6;
            // 
            // maskTextBoxSetupRepoAcctInfo
            // 
            this.tableLayoutPanelSetup.SetColumnSpan(this.maskTextBoxSetupRepoAcctInfo, 2);
            this.maskTextBoxSetupRepoAcctInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.maskTextBoxSetupRepoAcctInfo.Location = new System.Drawing.Point(3, 135);
            this.maskTextBoxSetupRepoAcctInfo.Name = "maskTextBoxSetupRepoAcctInfo";
            this.maskTextBoxSetupRepoAcctInfo.Size = new System.Drawing.Size(318, 20);
            this.maskTextBoxSetupRepoAcctInfo.TabIndex = 3;
            // 
            // maskTextBoxAzureAcctInfo
            // 
            this.tableLayoutPanelSetup.SetColumnSpan(this.maskTextBoxAzureAcctInfo, 2);
            this.maskTextBoxAzureAcctInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.maskTextBoxAzureAcctInfo.Location = new System.Drawing.Point(3, 255);
            this.maskTextBoxAzureAcctInfo.Name = "maskTextBoxAzureAcctInfo";
            this.maskTextBoxAzureAcctInfo.Size = new System.Drawing.Size(318, 20);
            this.maskTextBoxAzureAcctInfo.TabIndex = 0;
            // 
            // btnSetupValidate
            // 
            this.btnSetupValidate.Location = new System.Drawing.Point(403, 33);
            this.btnSetupValidate.Name = "btnSetupValidate";
            this.btnSetupValidate.Size = new System.Drawing.Size(72, 20);
            this.btnSetupValidate.TabIndex = 8;
            this.btnSetupValidate.Text = "Validate";
            this.btnSetupValidate.UseVisualStyleBackColor = true;
            this.btnSetupValidate.Click += new System.EventHandler(this.btnSetupValidate_Click);
            // 
            // btnSetupRemoveRepoAcctInfo
            // 
            this.btnSetupRemoveRepoAcctInfo.Location = new System.Drawing.Point(403, 135);
            this.btnSetupRemoveRepoAcctInfo.Name = "btnSetupRemoveRepoAcctInfo";
            this.btnSetupRemoveRepoAcctInfo.Size = new System.Drawing.Size(72, 20);
            this.btnSetupRemoveRepoAcctInfo.TabIndex = 5;
            this.btnSetupRemoveRepoAcctInfo.Text = "Remove";
            this.btnSetupRemoveRepoAcctInfo.UseVisualStyleBackColor = true;
            this.btnSetupRemoveRepoAcctInfo.Click += new System.EventHandler(this.btnSetupRemoveRepoAcctInfo_Click);
            // 
            // btnSetupRemoveAzureAcctInfo
            // 
            this.btnSetupRemoveAzureAcctInfo.Location = new System.Drawing.Point(403, 255);
            this.btnSetupRemoveAzureAcctInfo.Name = "btnSetupRemoveAzureAcctInfo";
            this.btnSetupRemoveAzureAcctInfo.Size = new System.Drawing.Size(72, 20);
            this.btnSetupRemoveAzureAcctInfo.TabIndex = 2;
            this.btnSetupRemoveAzureAcctInfo.Text = "Remove";
            this.btnSetupRemoveAzureAcctInfo.UseVisualStyleBackColor = true;
            this.btnSetupRemoveAzureAcctInfo.Click += new System.EventHandler(this.btnSetupRemoveAzureAcctInfo_Click);
            // 
            // labelSetupValidateHelp
            // 
            this.labelSetupValidateHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelSetupValidateHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSetupValidateHelp.Location = new System.Drawing.Point(481, 30);
            this.labelSetupValidateHelp.Name = "labelSetupValidateHelp";
            this.tableLayoutPanelSetup.SetRowSpan(this.labelSetupValidateHelp, 2);
            this.labelSetupValidateHelp.Size = new System.Drawing.Size(151, 102);
            this.labelSetupValidateHelp.TabIndex = 10;
            this.labelSetupValidateHelp.Text = "This ensures you have the right binaries with your configuration";
            // 
            // labelSetupAzureAccount
            // 
            this.tableLayoutPanelSetup.SetColumnSpan(this.labelSetupAzureAccount, 2);
            this.labelSetupAzureAccount.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.labelSetupAzureAccount.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSetupAzureAccount.Location = new System.Drawing.Point(3, 229);
            this.labelSetupAzureAccount.Name = "labelSetupAzureAccount";
            this.labelSetupAzureAccount.Size = new System.Drawing.Size(318, 23);
            this.labelSetupAzureAccount.TabIndex = 12;
            this.labelSetupAzureAccount.Text = "Azure Account";
            // 
            // labelSetupRepoAccount
            // 
            this.tableLayoutPanelSetup.SetColumnSpan(this.labelSetupRepoAccount, 2);
            this.labelSetupRepoAccount.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.labelSetupRepoAccount.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSetupRepoAccount.Location = new System.Drawing.Point(3, 107);
            this.labelSetupRepoAccount.Name = "labelSetupRepoAccount";
            this.labelSetupRepoAccount.Size = new System.Drawing.Size(318, 25);
            this.labelSetupRepoAccount.TabIndex = 11;
            this.labelSetupRepoAccount.Text = "Repository Account";
            // 
            // btnSetupBrowseFolder
            // 
            this.btnSetupBrowseFolder.Location = new System.Drawing.Point(327, 33);
            this.btnSetupBrowseFolder.Name = "btnSetupBrowseFolder";
            this.btnSetupBrowseFolder.Size = new System.Drawing.Size(70, 20);
            this.btnSetupBrowseFolder.TabIndex = 7;
            this.btnSetupBrowseFolder.Text = "Browse";
            this.btnSetupBrowseFolder.UseVisualStyleBackColor = true;
            this.btnSetupBrowseFolder.Click += new System.EventHandler(this.btnSetupBrowseFolder_Click);
            // 
            // btnSetupEditRepoAcctInfo
            // 
            this.btnSetupEditRepoAcctInfo.Location = new System.Drawing.Point(327, 135);
            this.btnSetupEditRepoAcctInfo.Name = "btnSetupEditRepoAcctInfo";
            this.btnSetupEditRepoAcctInfo.Size = new System.Drawing.Size(70, 20);
            this.btnSetupEditRepoAcctInfo.TabIndex = 4;
            this.btnSetupEditRepoAcctInfo.Text = "Edit";
            this.btnSetupEditRepoAcctInfo.UseVisualStyleBackColor = true;
            this.btnSetupEditRepoAcctInfo.Click += new System.EventHandler(this.btnSetupEditRepoAcctInfo_Click);
            // 
            // btnSetupEditAzureAcctInfo
            // 
            this.btnSetupEditAzureAcctInfo.Location = new System.Drawing.Point(327, 255);
            this.btnSetupEditAzureAcctInfo.Name = "btnSetupEditAzureAcctInfo";
            this.btnSetupEditAzureAcctInfo.Size = new System.Drawing.Size(70, 20);
            this.btnSetupEditAzureAcctInfo.TabIndex = 1;
            this.btnSetupEditAzureAcctInfo.Text = "Edit";
            this.btnSetupEditAzureAcctInfo.UseVisualStyleBackColor = true;
            this.btnSetupEditAzureAcctInfo.Click += new System.EventHandler(this.btnSetupEditAzureAcctInfo_Click);
            // 
            // tabUpdateHubs
            // 
            this.tabUpdateHubs.Location = new System.Drawing.Point(4, 25);
            this.tabUpdateHubs.Name = "tabUpdateHubs";
            this.tabUpdateHubs.Padding = new System.Windows.Forms.Padding(3);
            this.tabUpdateHubs.Size = new System.Drawing.Size(635, 396);
            this.tabUpdateHubs.TabIndex = 1;
            this.tabUpdateHubs.Text = "Update Hubs";
            this.tabUpdateHubs.UseVisualStyleBackColor = true;
            // 
            // folderBrowserWorkingDir
            // 
            this.folderBrowserWorkingDir.Description = "Please navigate to the working folder directory";
            this.folderBrowserWorkingDir.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.folderBrowserWorkingDir.ShowNewFolderButton = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(644, 427);
            this.Controls.Add(this.controlMainFormTab);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "LoT Update Manager";
            this.controlMainFormTab.ResumeLayout(false);
            this.tabSetup.ResumeLayout(false);
            this.tableLayoutPanelSetup.ResumeLayout(false);
            this.tableLayoutPanelSetup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl controlMainFormTab;
        private System.Windows.Forms.TabPage tabSetup;
        private System.Windows.Forms.TabPage tabUpdateHubs;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSetup;
        private System.Windows.Forms.MaskedTextBox maskTextBoxAzureAcctInfo;
        private System.Windows.Forms.Button btnSetupEditAzureAcctInfo;
        private System.Windows.Forms.Button btnSetupRemoveAzureAcctInfo;
        private System.Windows.Forms.Label labelSetupWorkingFolderHelp;
        private System.Windows.Forms.MaskedTextBox maskTextBoxSetupWorkingFolder;
        private System.Windows.Forms.MaskedTextBox maskTextBoxSetupRepoAcctInfo;
        private System.Windows.Forms.Button btnSetupEditRepoAcctInfo;
        private System.Windows.Forms.Button btnSetupValidate;
        private System.Windows.Forms.Button btnSetupRemoveRepoAcctInfo;
        private System.Windows.Forms.Button btnSetupBrowseFolder;
        private System.Windows.Forms.Label labelSetupValidateHelp;
        private System.Windows.Forms.Label labelSetupAzureAccount;
        private System.Windows.Forms.Label labelSetupRepoAccount;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserWorkingDir;
    }
}