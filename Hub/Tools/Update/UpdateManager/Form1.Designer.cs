using System.Windows.Forms;
namespace HomeOS.Hub.Tools.UpdateManager
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.account = new System.Windows.Forms.MaskedTextBox();
            this.initialInputPanel = new System.Windows.Forms.Panel();
            this.studyID = new System.Windows.Forms.MaskedTextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.orgID = new System.Windows.Forms.MaskedTextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.container = new System.Windows.Forms.MaskedTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.accountKey = new System.Windows.Forms.MaskedTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.outputPanel = new System.Windows.Forms.Panel();
            this.datemodified = new System.Windows.Forms.Label();
            this.tabWindow = new System.Windows.Forms.TabControl();
            this.hubList = new System.Windows.Forms.ListView();
            this.Hub = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.initialInputPanel.SuspendLayout();
            this.outputPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(802, 112);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Fetch";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Azure Account";
            // 
            // account
            // 
            this.account.Location = new System.Drawing.Point(99, 10);
            this.account.Name = "account";
            this.account.Size = new System.Drawing.Size(343, 20);
            this.account.TabIndex = 2;
            this.account.Text = "homelab";
            // 
            // initialInputPanel
            // 
            this.initialInputPanel.Controls.Add(this.studyID);
            this.initialInputPanel.Controls.Add(this.label5);
            this.initialInputPanel.Controls.Add(this.orgID);
            this.initialInputPanel.Controls.Add(this.label4);
            this.initialInputPanel.Controls.Add(this.container);
            this.initialInputPanel.Controls.Add(this.label3);
            this.initialInputPanel.Controls.Add(this.accountKey);
            this.initialInputPanel.Controls.Add(this.label2);
            this.initialInputPanel.Controls.Add(this.account);
            this.initialInputPanel.Controls.Add(this.label1);
            this.initialInputPanel.Controls.Add(this.button1);
            this.initialInputPanel.Location = new System.Drawing.Point(12, 12);
            this.initialInputPanel.Name = "initialInputPanel";
            this.initialInputPanel.Size = new System.Drawing.Size(891, 148);
            this.initialInputPanel.TabIndex = 3;
            this.initialInputPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // studyID
            // 
            this.studyID.Location = new System.Drawing.Point(684, 77);
            this.studyID.Name = "studyID";
            this.studyID.Size = new System.Drawing.Size(193, 20);
            this.studyID.TabIndex = 10;
            this.studyID.Text = "Default";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(626, 80);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "StudyID";
            // 
            // orgID
            // 
            this.orgID.Location = new System.Drawing.Point(99, 77);
            this.orgID.Name = "orgID";
            this.orgID.Size = new System.Drawing.Size(343, 20);
            this.orgID.TabIndex = 8;
            this.orgID.Text = "Default";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 80);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "OrgID";
            // 
            // container
            // 
            this.container.Location = new System.Drawing.Point(684, 10);
            this.container.Name = "container";
            this.container.Size = new System.Drawing.Size(193, 20);
            this.container.TabIndex = 6;
            this.container.Text = "configs";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(626, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Container";
            // 
            // accountKey
            // 
            this.accountKey.Location = new System.Drawing.Point(99, 43);
            this.accountKey.Name = "accountKey";
            this.accountKey.Size = new System.Drawing.Size(778, 20);
            this.accountKey.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Account Key";
            // 
            // outputPanel
            // 
            this.outputPanel.Controls.Add(this.datemodified);
            this.outputPanel.Controls.Add(this.tabWindow);
            this.outputPanel.Controls.Add(this.hubList);
            this.outputPanel.Location = new System.Drawing.Point(12, 176);
            this.outputPanel.Name = "outputPanel";
            this.outputPanel.Size = new System.Drawing.Size(891, 428);
            this.outputPanel.TabIndex = 4;
            this.outputPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.outputPanel_Paint);
            // 
            // datemodified
            // 
            this.datemodified.AutoSize = true;
            this.datemodified.Location = new System.Drawing.Point(146, 5);
            this.datemodified.Name = "datemodified";
            this.datemodified.Size = new System.Drawing.Size(10, 13);
            this.datemodified.TabIndex = 5;
            this.datemodified.Text = ":";
            // 
            // tabWindow
            // 
            this.tabWindow.Location = new System.Drawing.Point(146, 21);
            this.tabWindow.Name = "tabWindow";
            this.tabWindow.SelectedIndex = 0;
            this.tabWindow.Size = new System.Drawing.Size(731, 392);
            this.tabWindow.TabIndex = 1;
            // 
            // hubList
            // 
            this.hubList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Hub});
            this.hubList.Location = new System.Drawing.Point(16, 21);
            this.hubList.Name = "hubList";
            this.hubList.Size = new System.Drawing.Size(124, 392);
            this.hubList.TabIndex = 0;
            this.hubList.UseCompatibleStateImageBehavior = false;
            this.hubList.View = System.Windows.Forms.View.Details;
            this.hubList.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.hubList_SelectedIndexChanged);
            // 
            // Hub
            // 
            this.Hub.Text = "HubID";
            this.Hub.Width = 99;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(915, 616);
            this.Controls.Add(this.outputPanel);
            this.Controls.Add(this.initialInputPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Configuration Explorer";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.initialInputPanel.ResumeLayout(false);
            this.initialInputPanel.PerformLayout();
            this.outputPanel.ResumeLayout(false);
            this.outputPanel.PerformLayout();
            this.ResumeLayout(false);

        }



        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.MaskedTextBox account;
        private System.Windows.Forms.Panel initialInputPanel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.MaskedTextBox accountKey;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.MaskedTextBox studyID;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.MaskedTextBox orgID;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.MaskedTextBox container;
        private System.Windows.Forms.Panel outputPanel;
        private System.Windows.Forms.ListView hubList;
        private System.Windows.Forms.ColumnHeader Hub;
        private System.Windows.Forms.TabControl tabWindow;
        private Label datemodified;
    }
}

