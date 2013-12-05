using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace HomeOS.Hub.Tools.UpdateManager
{
    public partial class MainForm : Form
    {
        protected RepoAccountInfoForm formRepoAccountInfo;
        protected AzureAccountForm formAzureAccount;

        protected List<HubPlatUpdatePanelRowItem> HubPlatUpdatePanelList;
        protected List<HubOtherUpdatePanelRowItem> HubOtherUpdatePanelList;

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelUpdateHubs;
        private System.Windows.Forms.Button buttonUpdateHubsOther;
        private System.Windows.Forms.Label labelUpdateHubsOtherUpdates;
        private System.Windows.Forms.ComboBox comboBoxUpdateHubsOrgID;
        private System.Windows.Forms.Label labelUpdateHubsOrgID;
        private System.Windows.Forms.Label labelUpdateHubsStudyID;
        private System.Windows.Forms.ComboBox comboBoxUpdateHubsStudyID;
        private System.Windows.Forms.Label labelUpdateHubsPlatformUpdate;
        private System.Windows.Forms.Label labelUpdateHubsPlatformCurrent;
        private System.Windows.Forms.Label labelUpdateHubsPlatformDeployed;

        private System.Windows.Forms.CheckBox checkBoxUpdateHubsOtherAll;
        private System.Windows.Forms.Label labelUpdateHubsOtherWhatsNew;

        public MainForm()
        {
            InitializeComponent();

            List<string> OrgIdList = new List<string>() { "orgId1", "orgId2", "LongOrgId1234567890" };
            List<string> StudyIdList = new List<string>() { "studyId1", "studyId2", "LongStudyId123r567890" };
            LoadUpdateHubsTabComponents(this.tabUpdateHubs, OrgIdList, StudyIdList, 11,2,2);
        }

        private void LoadUpdateHubsTabComponents(TabPage tabUpdateHubs, List<string> OrgIdList, List<string> studyIdList, int rowCount, int platHubUpdates, int otherHubUpdates)
        {
            int currentRow = 0;

            // create the components that are not dynamic
            this.tableLayoutPanelUpdateHubs = new System.Windows.Forms.TableLayoutPanel();
            this.labelUpdateHubsOrgID = new System.Windows.Forms.Label();
            this.comboBoxUpdateHubsOrgID = new System.Windows.Forms.ComboBox();
            this.labelUpdateHubsStudyID = new System.Windows.Forms.Label();
            this.comboBoxUpdateHubsStudyID = new System.Windows.Forms.ComboBox();
            this.labelUpdateHubsPlatformUpdate = new System.Windows.Forms.Label();
            this.labelUpdateHubsPlatformCurrent = new System.Windows.Forms.Label();
            this.labelUpdateHubsPlatformDeployed = new System.Windows.Forms.Label();
            this.labelUpdateHubsOtherUpdates = new System.Windows.Forms.Label();
            this.labelUpdateHubsOtherWhatsNew = new System.Windows.Forms.Label();
            this.checkBoxUpdateHubsOtherAll = new System.Windows.Forms.CheckBox();
            this.buttonUpdateHubsOther = new System.Windows.Forms.Button();

            // suspend all layout
            tabUpdateHubs.SuspendLayout();
            this.tableLayoutPanelUpdateHubs.SuspendLayout();

            this.tableLayoutPanelUpdateHubs.RowCount = rowCount;

            // initialize the non-dynamic components

            // 
            // tableLayoutPanelUpdateHubs
            // 
            this.tableLayoutPanelUpdateHubs.AutoScroll = true;
            this.tableLayoutPanelUpdateHubs.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            this.tableLayoutPanelUpdateHubs.ColumnCount = 6;
            this.tableLayoutPanelUpdateHubs.AutoSize = true;
            //this.tableLayoutPanelUpdateHubs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            //this.tableLayoutPanelUpdateHubs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            //this.tableLayoutPanelUpdateHubs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            //this.tableLayoutPanelUpdateHubs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            //this.tableLayoutPanelUpdateHubs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            //this.tableLayoutPanelUpdateHubs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanelUpdateHubs.Location = new System.Drawing.Point(0, 0);            
            this.tableLayoutPanelUpdateHubs.Name = "tableLayoutPanelUpdateHubs";
            //this.tableLayoutPanelUpdateHubs.Size = tabUpdateHubs.Size;
            this.tableLayoutPanelUpdateHubs.MinimumSize = new Size(tabUpdateHubs.Size.Width, 0);
            this.tableLayoutPanelUpdateHubs.MaximumSize = tabUpdateHubs.Size;
            this.tableLayoutPanelUpdateHubs.TabIndex = 0;

            tabUpdateHubs.Controls.Add(this.tableLayoutPanelUpdateHubs);

            // 
            // labelUpdateHubsOrgID
            // 
            this.labelUpdateHubsOrgID.AutoSize = false;
            this.labelUpdateHubsOrgID.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.labelUpdateHubsOrgID.Location = new System.Drawing.Point(3, 0);
            this.labelUpdateHubsOrgID.Name = "labelUpdateHubsOrgID";
            //this.labelUpdateHubsOrgID.Size = new System.Drawing.Size(60, 26);
            this.labelUpdateHubsOrgID.TabIndex = 1;
            this.labelUpdateHubsOrgID.Text = "Org ID";
            this.labelUpdateHubsOrgID.Margin = new Padding(0);
            // 
            // comboBoxUpdateHubsOrgID
            // 
            this.tableLayoutPanelUpdateHubs.SetColumnSpan(this.comboBoxUpdateHubsOrgID, 2);
            this.comboBoxUpdateHubsOrgID.AutoSize = false;
            this.comboBoxUpdateHubsOrgID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxUpdateHubsOrgID.FormattingEnabled = true;
            //this.comboBoxUpdateHubsOrgID.Location = new System.Drawing.Point(61, 3);
            this.comboBoxUpdateHubsOrgID.Name = "comboBoxUpdateHubsOrgID";
            //this.comboBoxUpdateHubsOrgID.Size = new System.Drawing.Size(204, 20);
            this.comboBoxUpdateHubsOrgID.TabIndex = 0;
            this.comboBoxUpdateHubsOrgID.Margin = new Padding(0);
            this.comboBoxUpdateHubsOrgID.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxUpdateHubsOrgID.DataSource = OrgIdList;
            this.comboBoxUpdateHubsOrgID.SelectedIndexChanged += new System.EventHandler(this.comboBoxUpdateHubsOrgID_SelectedIndexChanged);
            // 
            // labelUpdateHubsStudyID
            // 
            this.labelUpdateHubsStudyID.AutoSize = false;
            this.labelUpdateHubsStudyID.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.labelUpdateHubsStudyID.Location = new System.Drawing.Point(3, 26);
            this.labelUpdateHubsStudyID.Name = "labelUpdateHubsStudyID";
            //this.labelUpdateHubsStudyID.Size = new System.Drawing.Size(52, 26);
            this.labelUpdateHubsStudyID.TabIndex = 3;
            this.labelUpdateHubsStudyID.Text = "Study ID";
            this.labelUpdateHubsStudyID.Margin = new Padding(0);

            // 
            // comboBoxUpdateHubsStudyID
            // 
            this.tableLayoutPanelUpdateHubs.SetColumnSpan(this.comboBoxUpdateHubsStudyID, 2);
            this.comboBoxUpdateHubsStudyID.AutoSize = false;
            this.comboBoxUpdateHubsStudyID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxUpdateHubsStudyID.FormattingEnabled = true;
            //this.comboBoxUpdateHubsStudyID.Location = new System.Drawing.Point(61, 29);
            this.comboBoxUpdateHubsStudyID.Name = "comboBoxUpdateHubsStudyID";
            //this.comboBoxUpdateHubsStudyID.Size = new System.Drawing.Size(204, 20);
            this.comboBoxUpdateHubsStudyID.TabIndex = 2;
            this.comboBoxUpdateHubsStudyID.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxUpdateHubsStudyID.DataSource = studyIdList;
            this.comboBoxUpdateHubsStudyID.SelectedIndexChanged += new System.EventHandler(this.comboBoxUpdateHubsStudyID_SelectedIndexChanged);
            this.comboBoxUpdateHubsStudyID.Margin = new Padding(0);
            // 
            // labelUpdateHubsPlatformUpdate
            // 
            this.labelUpdateHubsPlatformUpdate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelUpdateHubsPlatformUpdate.AutoSize = false;
            this.labelUpdateHubsPlatformUpdate.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            //this.labelUpdateHubsPlatformUpdate.Location = new System.Drawing.Point(3, 49);
            this.labelUpdateHubsPlatformUpdate.Name = "labelUpdateHubsPlatformUpdate";
            //this.labelUpdateHubsPlatformUpdate.Size = new System.Drawing.Size(595, 33);
            this.labelUpdateHubsPlatformUpdate.TabIndex = 4;
            this.labelUpdateHubsPlatformUpdate.Text = "Platform Update";
            this.labelUpdateHubsPlatformUpdate.Margin = new Padding(0);

            // 
            // labelUpdateHubsPlatformCurrent
            // 
            this.labelUpdateHubsPlatformCurrent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelUpdateHubsPlatformCurrent.AutoSize = false;
            this.labelUpdateHubsPlatformCurrent.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            //this.labelUpdateHubsPlatformCurrent.Location = new System.Drawing.Point(3, 49);
            this.labelUpdateHubsPlatformCurrent.Name = "labelUpdateHubsPlatformCurrent";
            //this.labelUpdateHubsPlatformUpdate.Size = new System.Drawing.Size(595, 33);
            this.labelUpdateHubsPlatformCurrent.TabIndex = 4;
            this.labelUpdateHubsPlatformCurrent.Text = "You have";
            this.labelUpdateHubsPlatformCurrent.Margin = new Padding(0);
            // 
            // labelUpdateHubsPlatformDeployed
            // 
            this.labelUpdateHubsPlatformDeployed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelUpdateHubsPlatformDeployed.AutoSize = false;
            this.labelUpdateHubsPlatformDeployed.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // this.labelUpdateHubsPlatformDeployed.Location = new System.Drawing.Point(3, 49);
            this.labelUpdateHubsPlatformDeployed.Name = "labelUpdateHubsPlatformDeployed";
            //this.labelUpdateHubsPlatformDeployed.Size = new System.Drawing.Size(595, 33);
            this.labelUpdateHubsPlatformDeployed.TabIndex = 4;
            this.labelUpdateHubsPlatformDeployed.Text = "Deployed Version";
            this.labelUpdateHubsPlatformDeployed.Margin = new Padding(0);
            // 
            // labelUpdateHubsOtherUpdates
            // 
            this.labelUpdateHubsOtherUpdates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelUpdateHubsOtherUpdates.AutoSize = false;
            this.labelUpdateHubsOtherUpdates.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            //this.labelUpdateHubsOtherUpdates.Location = new System.Drawing.Point(3, 111);
            this.labelUpdateHubsOtherUpdates.Name = "labelUpdateHubsOtherUpdates";
            //this.labelUpdateHubsOtherUpdates.Size = new System.Drawing.Size(595, 30);
            this.labelUpdateHubsOtherUpdates.TabIndex = 9;
            this.labelUpdateHubsOtherUpdates.Text = "Other Updates";
            this.labelUpdateHubsOtherUpdates.Margin = new Padding(0);
            // 
            // checkBoxUpdateHubsOtherAll
            // 
            this.checkBoxUpdateHubsOtherAll.AutoSize = false;
            this.checkBoxUpdateHubsOtherAll.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.checkBoxUpdateHubsOtherAll.Location = new System.Drawing.Point(3, 144);
            this.checkBoxUpdateHubsOtherAll.Name = "checkBoxUpdateHubsOtherAll";
            //this.checkBoxUpdateHubsOtherAll.Size = new System.Drawing.Size(106, 20);
            this.checkBoxUpdateHubsOtherAll.TabIndex = 10;
            this.checkBoxUpdateHubsOtherAll.Text = "All";
            this.checkBoxUpdateHubsOtherAll.UseVisualStyleBackColor = true;
            this.checkBoxUpdateHubsOtherAll.CheckedChanged += new System.EventHandler(this.checkBoxUpdateHubsOtherAll_CheckedChanged);
            this.checkBoxUpdateHubsOtherAll.Margin = new Padding(0);
            // 
            // labelUpdateHubsOtherWhatsNew
            // 
            this.labelUpdateHubsOtherWhatsNew.AutoSize = false;
            this.labelUpdateHubsOtherWhatsNew.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelUpdateHubsOtherWhatsNew.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            //this.labelUpdateHubsOtherWhatsNew.Location = new System.Drawing.Point(352, 141);
            this.labelUpdateHubsOtherWhatsNew.Name = "labelUpdateHubsOtherWhatsNew";
            //this.labelUpdateHubsOtherWhatsNew.Size = new System.Drawing.Size(246, 26);
            this.labelUpdateHubsOtherWhatsNew.TabIndex = 11;
            this.labelUpdateHubsOtherWhatsNew.Text = "What\'s New?";
            this.labelUpdateHubsOtherWhatsNew.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelUpdateHubsOtherWhatsNew.Margin = new Padding(0);
            // 
            // buttonUpdateHubsOther
            // 
            //this.buttonUpdateHubsOther.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            //| System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUpdateHubsOther.AutoSize = false;
            this.buttonUpdateHubsOther.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.buttonUpdateHubsOther.Location = new System.Drawing.Point(3, 372);
            this.buttonUpdateHubsOther.Name = "buttonUpdateHubsOther";
            //this.buttonUpdateHubsOther.Size = new System.Drawing.Size(106, 23);
            this.buttonUpdateHubsOther.TabIndex = 1;
            this.buttonUpdateHubsOther.Text = "Update ";
            this.buttonUpdateHubsOther.UseVisualStyleBackColor = true;
            this.buttonUpdateHubsOther.Click += new System.EventHandler(this.buttonUpdateHubsOther_Click);
            this.buttonUpdateHubsOther.Margin = new Padding(0);

            // add some of the non-dyamic components to the layout panel

            this.tableLayoutPanelUpdateHubs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22));
            this.tableLayoutPanelUpdateHubs.Controls.Add(this.labelUpdateHubsOrgID, 0, currentRow);
            this.tableLayoutPanelUpdateHubs.Controls.Add(this.comboBoxUpdateHubsOrgID, 1, currentRow++);
            this.tableLayoutPanelUpdateHubs.SetColumnSpan(this.comboBoxUpdateHubsOrgID, 2);

            this.tableLayoutPanelUpdateHubs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22));
            this.tableLayoutPanelUpdateHubs.Controls.Add(this.labelUpdateHubsStudyID, 0, currentRow);
            this.tableLayoutPanelUpdateHubs.Controls.Add(this.comboBoxUpdateHubsStudyID, 1, currentRow++);
            this.tableLayoutPanelUpdateHubs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22));
            this.tableLayoutPanelUpdateHubs.SetColumnSpan(this.comboBoxUpdateHubsStudyID, 2);
            this.tableLayoutPanelUpdateHubs.Controls.Add(this.labelUpdateHubsPlatformUpdate, 0, currentRow++);
            this.tableLayoutPanelUpdateHubs.SetColumnSpan(this.labelUpdateHubsPlatformUpdate, 6);
            this.tableLayoutPanelUpdateHubs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22));
            this.tableLayoutPanelUpdateHubs.Controls.Add(this.labelUpdateHubsPlatformDeployed, 1, currentRow);
            this.tableLayoutPanelUpdateHubs.Controls.Add(this.labelUpdateHubsPlatformCurrent, 2, currentRow++);

            // create and initialize the dynamic components - platform update rows

            this.HubPlatUpdatePanelList = new List<HubPlatUpdatePanelRowItem>();
            for (int i = 0; i < platHubUpdates; ++i)
            {
                this.tableLayoutPanelUpdateHubs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22));
                this.HubPlatUpdatePanelList.Add(new HubPlatUpdatePanelRowItem(this.tableLayoutPanelUpdateHubs, currentRow++, "Hub " + i.ToString(), "0.0.0.1", "1.0.0.0"));
            }

            // add some more of the non dynamic components of the layout

            this.tableLayoutPanelUpdateHubs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22));
            this.tableLayoutPanelUpdateHubs.Controls.Add(this.labelUpdateHubsOtherUpdates, 0, currentRow++);
            this.tableLayoutPanelUpdateHubs.SetColumnSpan(this.labelUpdateHubsOtherUpdates, 6);

            this.tableLayoutPanelUpdateHubs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22));
            this.tableLayoutPanelUpdateHubs.Controls.Add(this.checkBoxUpdateHubsOtherAll, 0, currentRow);
            this.tableLayoutPanelUpdateHubs.Controls.Add(this.labelUpdateHubsOtherWhatsNew, 3, currentRow++);
            this.tableLayoutPanelUpdateHubs.SetColumnSpan(this.labelUpdateHubsOtherWhatsNew, 3);


            // create and initialize the dynamic components - Other update rows

            this.HubOtherUpdatePanelList = new List<HubOtherUpdatePanelRowItem>();
            for (int i = 0; i < otherHubUpdates; ++i)
            {
                this.tableLayoutPanelUpdateHubs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22));
                this.HubOtherUpdatePanelList.Add(new HubOtherUpdatePanelRowItem(this.tableLayoutPanelUpdateHubs, currentRow++, "Hub " + i.ToString(), true, true, true));
            }

            // add the Update button component add the bottom 

            this.tableLayoutPanelUpdateHubs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22));
            this.tableLayoutPanelUpdateHubs.Controls.Add(this.buttonUpdateHubsOther, 0, currentRow);

            Debug.Assert(currentRow == rowCount-1);

            // resume layout 

            tabUpdateHubs.ResumeLayout(false);
            this.tableLayoutPanelUpdateHubs.ResumeLayout(false);
            this.tableLayoutPanelUpdateHubs.PerformLayout();

        }

        private void btnSetupBrowseFolder_Click(object sender, EventArgs e)
        {
            if (this.folderBrowserWorkingDir.ShowDialog() == DialogResult.OK)
            {
                this.maskTextBoxSetupWorkingFolder.Text = folderBrowserWorkingDir.SelectedPath;
            }

        }

        private void btnSetupValidate_Click(object sender, EventArgs e)
        {

        }

        private void btnSetupEditRepoAcctInfo_Click(object sender, EventArgs e)
        {
            if (null == this.formRepoAccountInfo)
            {
                this.formRepoAccountInfo = new RepoAccountInfoForm();
            }

            this.formRepoAccountInfo.ShowDialog(this);
        }

        private void btnSetupRemoveRepoAcctInfo_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show(this, "Remove the FTP Account Information?", "Remove Repository Account", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2))
            {
            }

        }

        private void btnSetupEditAzureAcctInfo_Click(object sender, EventArgs e)
        {
            if (null == this.formAzureAccount)
            {
                this.formAzureAccount = new AzureAccountForm();
            }

            this.formAzureAccount.ShowDialog(this);

        }

        private void btnSetupRemoveAzureAcctInfo_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show(this, "Remove the Azure Account Information?", "Remove Azure Account", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2))
            {
            }
        }

        private void comboBoxUpdateHubsOrgID_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBoxUpdateHubsStudyID_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkBoxUpdateHubsOtherAll_CheckedChanged(object sender, EventArgs e)
        {

        }


        private void buttonUpdateHubsOther_Click(object sender, EventArgs e)
        {

        }

    }
}
