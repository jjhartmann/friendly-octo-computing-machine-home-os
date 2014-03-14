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
            this.tabConfigs = new System.Windows.Forms.TabPage();
            this.tabModuleScouts = new System.Windows.Forms.TabPage();
            this.tabPlatform = new System.Windows.Forms.TabPage();
            this.controlMainFormTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // controlMainFormTab
            // 
            this.controlMainFormTab.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.controlMainFormTab.Controls.Add(this.tabSetup);
            this.controlMainFormTab.Controls.Add(this.tabConfigs);
            this.controlMainFormTab.Controls.Add(this.tabModuleScouts); 
            this.controlMainFormTab.Controls.Add(this.tabPlatform);
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
            this.tabSetup.Location = new System.Drawing.Point(4, 25);
            this.tabSetup.Name = "tabSetup";
            this.tabSetup.Padding = new System.Windows.Forms.Padding(3);
            this.tabSetup.Size = new System.Drawing.Size(635, 396);
            this.tabSetup.TabIndex = 0;
            this.tabSetup.Text = "Set Up";
            this.tabSetup.UseVisualStyleBackColor = true;
            // 
            // tabConfigs
            // 
            this.tabConfigs.Location = new System.Drawing.Point(4, 25);
            this.tabConfigs.Name = "tabConfigs";
            this.tabConfigs.Padding = new System.Windows.Forms.Padding(3);
            this.tabConfigs.Size = new System.Drawing.Size(635, 396);
            this.tabConfigs.TabIndex = 1;
            this.tabConfigs.Text = "Configs";
            this.tabConfigs.UseVisualStyleBackColor = true;
            // 
            // tabModuleScouts
            // 
            this.tabModuleScouts.Location = new System.Drawing.Point(4, 25);
            this.tabModuleScouts.Name = "tabModuleScouts";
            this.tabModuleScouts.Padding = new System.Windows.Forms.Padding(3);
            this.tabModuleScouts.Size = new System.Drawing.Size(635, 396);
            this.tabModuleScouts.TabIndex = 1;
            this.tabModuleScouts.Text = "Modules/Scouts";
            this.tabModuleScouts.UseVisualStyleBackColor = true;

            // 
            // tabPlatform
            // 
            this.tabPlatform.Location = new System.Drawing.Point(4, 25);
            this.tabPlatform.Name = "tabPlatform";
            this.tabPlatform.Padding = new System.Windows.Forms.Padding(3);
            this.tabPlatform.Size = new System.Drawing.Size(635, 396);
            this.tabPlatform.TabIndex = 1;
            this.tabPlatform.Text = "Platform";
            this.tabPlatform.UseVisualStyleBackColor = true;
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
            this.ResumeLayout(false);

        }


        #endregion

        private System.Windows.Forms.TabControl controlMainFormTab;
        private System.Windows.Forms.TabPage tabSetup;
        private System.Windows.Forms.TabPage tabConfigs;
        private System.Windows.Forms.TabPage tabModuleScouts;
        private System.Windows.Forms.TabPage tabPlatform;
    }
}