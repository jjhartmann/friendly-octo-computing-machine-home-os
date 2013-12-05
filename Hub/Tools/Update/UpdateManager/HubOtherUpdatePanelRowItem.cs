using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HomeOS.Hub.Tools.UpdateManager
{
    public class HubOtherUpdatePanelRowItem
    {
        private System.Windows.Forms.CheckBox checkBoxUpdateHubsOtherHubId;
        private System.Windows.Forms.Button buttonUpdateHubsOtherViewConfigs;
        private System.Windows.Forms.Button buttonUpdateHubsOtherDownloadConfigs;
        private System.Windows.Forms.LinkLabel linkLabelUpdateHubsOtherApps;
        private System.Windows.Forms.LinkLabel linkLabelUpdateHubsOtherScouts;
        private System.Windows.Forms.LinkLabel linkLabelUpdateHubsOtherDrivers;

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private int rowNumber;

        public HubOtherUpdatePanelRowItem(System.Windows.Forms.TableLayoutPanel tableLayoutPanel, int rowNumber, string hubId, bool appsDiff, bool driversDiff, bool scoutsDiff)
        {
            this.tableLayoutPanel = tableLayoutPanel;
            this.rowNumber = rowNumber;

            // 
            // checkBoxUpdateHubsOtherHubId
            // 
            this.checkBoxUpdateHubsOtherHubId = new System.Windows.Forms.CheckBox();
            this.checkBoxUpdateHubsOtherHubId.AutoSize = true;
            this.checkBoxUpdateHubsOtherHubId.Dock = System.Windows.Forms.DockStyle.Fill;
            // this.checkBoxUpdateHubsOtherHubId.Location = new System.Drawing.Point(3, 170);
            this.checkBoxUpdateHubsOtherHubId.Name = "checkBoxUpdateHubsOtherHubId" + this.rowNumber.ToString();
            //this.checkBoxUpdateHubsOtherHubId.Size = new System.Drawing.Size(106, 23);
            this.checkBoxUpdateHubsOtherHubId.TabIndex = 12;
            this.checkBoxUpdateHubsOtherHubId.Text = hubId;
            this.checkBoxUpdateHubsOtherHubId.UseVisualStyleBackColor = true;
            this.checkBoxUpdateHubsOtherHubId.Margin = new Padding(0);
            this.checkBoxUpdateHubsOtherHubId.CheckedChanged += new System.EventHandler(this.checkBoxUpdateHubsOtherHubId_CheckedChanged);
            // 
            // buttonUpdateHubsOtherViewConfigs
            // 
            this.buttonUpdateHubsOtherViewConfigs = new System.Windows.Forms.Button();
            this.buttonUpdateHubsOtherViewConfigs.AutoSize = false;
            this.buttonUpdateHubsOtherViewConfigs.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.buttonUpdateHubsOtherViewConfigs.Location = new System.Drawing.Point(115, 170);
            this.buttonUpdateHubsOtherViewConfigs.Name = "buttonUpdateHubsOtherViewConfigs" + this.rowNumber.ToString();
            //this.buttonUpdateHubsOtherViewConfigs.Size = new System.Drawing.Size(113, 23);
            this.buttonUpdateHubsOtherViewConfigs.TabIndex = 13;
            this.buttonUpdateHubsOtherViewConfigs.Text = "View Configs";
            this.buttonUpdateHubsOtherViewConfigs.UseVisualStyleBackColor = true;
            this.buttonUpdateHubsOtherViewConfigs.Margin = new Padding(0);
            this.buttonUpdateHubsOtherViewConfigs.Click += new System.EventHandler(this.buttonUpdateHubsOtherViewConfigs_Click);
            // 
            // buttonUpdateHubsOtherDownloadConfigs
            // 
            this.buttonUpdateHubsOtherDownloadConfigs = new System.Windows.Forms.Button();
            this.buttonUpdateHubsOtherDownloadConfigs.AutoSize = false;
            this.buttonUpdateHubsOtherDownloadConfigs.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.buttonUpdateHubsOtherDownloadConfigs.Location = new System.Drawing.Point(234, 170);
            this.buttonUpdateHubsOtherDownloadConfigs.Name = "buttonUpdateHubsOtherDownloadConfigs" + this.rowNumber.ToString();
            //this.buttonUpdateHubsOtherDownloadConfigs.Size = new System.Drawing.Size(112, 23);
            this.buttonUpdateHubsOtherDownloadConfigs.TabIndex = 14;
            this.buttonUpdateHubsOtherDownloadConfigs.Text = "Download Configs";
            this.buttonUpdateHubsOtherDownloadConfigs.UseVisualStyleBackColor = true;
            this.buttonUpdateHubsOtherDownloadConfigs.Margin = new Padding(0);
            this.buttonUpdateHubsOtherDownloadConfigs.Click += new System.EventHandler(this.buttonUpdateHubsOtherDownloadConfigs_Click);
            // 
            // linkLabelUpdateHubsOtherApps
            // 

            if (appsDiff)
            {
                this.linkLabelUpdateHubsOtherApps = new System.Windows.Forms.LinkLabel();
                this.linkLabelUpdateHubsOtherApps.AutoSize = false;
                this.linkLabelUpdateHubsOtherApps.Dock = System.Windows.Forms.DockStyle.Fill;
                //this.linkLabelUpdateHubsOtherApps.Location = new System.Drawing.Point(352, 167);
                this.linkLabelUpdateHubsOtherApps.Name = "linkLabelUpdateHubsOtherApps" + this.rowNumber.ToString();
                //this.linkLabelUpdateHubsOtherApps.Size = new System.Drawing.Size(65, 29);
                this.linkLabelUpdateHubsOtherApps.TabIndex = 16;
                this.linkLabelUpdateHubsOtherApps.TabStop = true;
                this.linkLabelUpdateHubsOtherApps.Text = "Apps";
                this.linkLabelUpdateHubsOtherApps.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                this.linkLabelUpdateHubsOtherApps.Margin = new Padding(0);
                this.linkLabelUpdateHubsOtherApps.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelUpdateHubsOtherApps_LinkClicked);
            }

            // 
            // linkLabelUpdateHubsOtherScouts
            // 
            if (scoutsDiff)
            {
                this.linkLabelUpdateHubsOtherScouts = new System.Windows.Forms.LinkLabel();
                this.linkLabelUpdateHubsOtherScouts.AutoSize = false;
                this.linkLabelUpdateHubsOtherScouts.Dock = System.Windows.Forms.DockStyle.Fill;
                //this.linkLabelUpdateHubsOtherScouts.Location = new System.Drawing.Point(510, 167);
                this.linkLabelUpdateHubsOtherScouts.Name = "linkLabelUpdateHubsOtherScouts" + this.rowNumber.ToString();
                //this.linkLabelUpdateHubsOtherScouts.Size = new System.Drawing.Size(88, 29);
                this.linkLabelUpdateHubsOtherScouts.TabIndex = 15;
                this.linkLabelUpdateHubsOtherScouts.TabStop = true;
                this.linkLabelUpdateHubsOtherScouts.Text = "Scouts";
                this.linkLabelUpdateHubsOtherScouts.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                this.linkLabelUpdateHubsOtherScouts.Margin = new Padding(0);
                this.linkLabelUpdateHubsOtherScouts.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelUpdateHubsOtherScouts_LinkClicked);
            }

            // 
            // linkLabelUpdateHubsOtherDrivers
            //
            if (driversDiff)
            {
                this.linkLabelUpdateHubsOtherDrivers = new System.Windows.Forms.LinkLabel();
                this.linkLabelUpdateHubsOtherDrivers.AutoSize = false;
                this.linkLabelUpdateHubsOtherDrivers.Dock = System.Windows.Forms.DockStyle.Fill;
                //this.linkLabelUpdateHubsOtherDrivers.Location = new System.Drawing.Point(423, 167);
                this.linkLabelUpdateHubsOtherDrivers.Name = "linkLabelUpdateHubsOtherDrivers" + this.rowNumber.ToString();
                //this.linkLabelUpdateHubsOtherDrivers.Size = new System.Drawing.Size(81, 29);
                this.linkLabelUpdateHubsOtherDrivers.TabIndex = 17;
                this.linkLabelUpdateHubsOtherDrivers.TabStop = true;
                this.linkLabelUpdateHubsOtherDrivers.Text = "Drivers";
                this.linkLabelUpdateHubsOtherDrivers.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                this.linkLabelUpdateHubsOtherDrivers.Margin = new Padding(0);
                this.linkLabelUpdateHubsOtherDrivers.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelUpdateHubsOtherDrivers_LinkClicked);
            }

            this.tableLayoutPanel.Controls.Add(this.checkBoxUpdateHubsOtherHubId, 0, rowNumber);
            this.tableLayoutPanel.Controls.Add(this.buttonUpdateHubsOtherViewConfigs, 1, rowNumber);
            this.tableLayoutPanel.Controls.Add(this.buttonUpdateHubsOtherDownloadConfigs, 2, rowNumber);

            if (appsDiff)
            {
                this.tableLayoutPanel.Controls.Add(this.linkLabelUpdateHubsOtherApps, 3, rowNumber);
            }
            if (driversDiff)
            {
                this.tableLayoutPanel.Controls.Add(this.linkLabelUpdateHubsOtherDrivers, 4, rowNumber);
            }
            if (scoutsDiff)
            {
                this.tableLayoutPanel.Controls.Add(this.linkLabelUpdateHubsOtherScouts, 5, rowNumber);
            }

        }

        private void checkBoxUpdateHubsOtherHubId_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void buttonUpdateHubsOtherViewConfigs_Click(object sender, EventArgs e)
        {

        }

        private void buttonUpdateHubsOtherDownloadConfigs_Click(object sender, EventArgs e)
        {

        }

        private void linkLabelUpdateHubsOtherApps_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void linkLabelUpdateHubsOtherDrivers_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void linkLabelUpdateHubsOtherScouts_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

    }
}
