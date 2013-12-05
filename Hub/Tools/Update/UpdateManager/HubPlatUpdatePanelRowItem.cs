using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Controls;

namespace HomeOS.Hub.Tools.UpdateManager
{
    public class HubPlatUpdatePanelRowItem
    {
        private System.Windows.Forms.CheckBox checkBoxUpdateHubsPlatHubId;
        private System.Windows.Forms.Label labelUpdateHubsPlatDeployedVer;
        private System.Windows.Forms.Label labelUpdateHubsPlatYourVer;
        private System.Windows.Forms.Button buttonUpdateHubsPlatUpdate;

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private int rowNumber;

        public HubPlatUpdatePanelRowItem(System.Windows.Forms.TableLayoutPanel tableLayoutPanel, int rowNumber, string hubId, string deployedVer, string yourVer)
        {
            this.tableLayoutPanel = tableLayoutPanel;
            this.rowNumber = rowNumber;

            // 
            // checkBoxUpdateHubsPlatHubId
            // 
            this.checkBoxUpdateHubsPlatHubId = new System.Windows.Forms.CheckBox();
            this.checkBoxUpdateHubsPlatHubId.AutoSize = true;
            this.checkBoxUpdateHubsPlatHubId.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.checkBoxUpdateHubsPlatHubId.Location = new System.Drawing.Point(3, 85);
            this.checkBoxUpdateHubsPlatHubId.Name = "checkBoxUpdateHubsPlatHubId" + this.rowNumber.ToString();
            //this.checkBoxUpdateHubsPlatHubId.Size = new System.Drawing.Size(106, 23);
            this.checkBoxUpdateHubsPlatHubId.TabIndex = 5;
            this.checkBoxUpdateHubsPlatHubId.Text = hubId;
            this.checkBoxUpdateHubsPlatHubId.UseVisualStyleBackColor = true;
            this.checkBoxUpdateHubsPlatHubId.CheckedChanged += new System.EventHandler(this.checkBoxUpdateHubsPlatHubId_CheckedChanged);
            this.checkBoxUpdateHubsPlatHubId.Margin = new Padding(0);

            // 
            // labelUpdateHubsPlatDeployedVer
            // 
            this.labelUpdateHubsPlatDeployedVer = new System.Windows.Forms.Label();
            this.labelUpdateHubsPlatDeployedVer.AutoSize = false;
            this.labelUpdateHubsPlatDeployedVer.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.labelUpdateHubsPlatDeployedVer.Location = new System.Drawing.Point(115, 82);
            this.labelUpdateHubsPlatDeployedVer.Name = "labelUpdateHubsPlatDeployedVer" + this.rowNumber.ToString();
            //this.labelUpdateHubsPlatDeployedVer.Size = new System.Drawing.Size(113, 29);
            this.labelUpdateHubsPlatDeployedVer.TabIndex = 6;
            this.labelUpdateHubsPlatDeployedVer.Text = deployedVer;
            this.labelUpdateHubsPlatDeployedVer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelUpdateHubsPlatDeployedVer.Margin = new Padding(0);
            // 
            // labelUpdateHubsPlatYourVer
            // 
            this.labelUpdateHubsPlatYourVer = new System.Windows.Forms.Label();
            this.labelUpdateHubsPlatYourVer.AutoSize = false;
            this.labelUpdateHubsPlatYourVer.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.labelUpdateHubsPlatYourVer.Location = new System.Drawing.Point(234, 82);
            this.labelUpdateHubsPlatYourVer.Name = "labelUpdateHubsPlatYourVer" + this.rowNumber.ToString();
            //this.labelUpdateHubsPlatYourVer.Size = new System.Drawing.Size(112, 29);
            this.labelUpdateHubsPlatYourVer.TabIndex = 7;
            this.labelUpdateHubsPlatYourVer.Text = yourVer;
            this.labelUpdateHubsPlatYourVer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelUpdateHubsPlatYourVer.Margin = new Padding(0);
            // 
            // buttonUpdateHubsPlatUpdate
            // 
            this.buttonUpdateHubsPlatUpdate = new System.Windows.Forms.Button();
            this.tableLayoutPanel.SetColumnSpan(this.buttonUpdateHubsPlatUpdate, 2);
            this.buttonUpdateHubsPlatUpdate.AutoSize = false;
            this.buttonUpdateHubsPlatUpdate.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.buttonUpdateHubsPlatUpdate.Location = new System.Drawing.Point(352, 85);
            this.buttonUpdateHubsPlatUpdate.Name = "buttonUpdateHubsPlatUpdate" + this.rowNumber.ToString();
            //this.buttonUpdateHubsPlatUpdate.Size = new System.Drawing.Size(152, 23);
            this.buttonUpdateHubsPlatUpdate.TabIndex = 8;
            this.buttonUpdateHubsPlatUpdate.Text = "Update Platform";
            this.buttonUpdateHubsPlatUpdate.UseVisualStyleBackColor = true;
            this.buttonUpdateHubsPlatUpdate.Margin = new Padding(0);
            this.buttonUpdateHubsPlatUpdate.Click += new System.EventHandler(this.buttonUpdateHubsPlatUpdate_Click);

            this.tableLayoutPanel.Controls.Add(this.checkBoxUpdateHubsPlatHubId, 0, rowNumber);
            this.tableLayoutPanel.Controls.Add(this.labelUpdateHubsPlatDeployedVer, 1, rowNumber);
            this.tableLayoutPanel.Controls.Add(this.labelUpdateHubsPlatYourVer, 2, rowNumber);
            this.tableLayoutPanel.Controls.Add(this.buttonUpdateHubsPlatUpdate, 3, rowNumber);

        }

        private void checkBoxUpdateHubsPlatHubId_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void buttonUpdateHubsPlatUpdate_Click(object sender, EventArgs e)
        {
            TableLayoutControlCollection tableLayoutCtrlColl = this.tableLayoutPanel.Controls;
            System.Windows.Forms.Control checkbox = tableLayoutCtrlColl[this.checkBoxUpdateHubsPlatHubId.Name];
            System.Windows.Forms.Control labelDepVer = tableLayoutCtrlColl[this.labelUpdateHubsPlatDeployedVer.Name];
            System.Windows.Forms.Control labelCurVer = tableLayoutCtrlColl[this.labelUpdateHubsPlatYourVer.Name];
            string hubId = checkbox.Text;
            string deployedVersion = labelDepVer.Text;
            string currentVersion = labelCurVer.Text;

            if (DialogResult.Yes == MessageBox.Show((IWin32Window)sender, string.Format("Are you sure you want to update the platform for {0} from the deployed version {1} to your current version {2}?", hubId, deployedVersion, currentVersion), "Update Platform", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2))
            {
            }

        }


    }
}
