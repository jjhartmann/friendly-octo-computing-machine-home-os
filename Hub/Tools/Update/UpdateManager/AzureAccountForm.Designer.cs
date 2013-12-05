namespace HomeOS.Hub.Tools.UpdateManager
{
    partial class AzureAccountForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AzureAccountForm));
            this.labelAzureAccountName = new System.Windows.Forms.Label();
            this.maskedTextBoxAzureAccountName = new System.Windows.Forms.MaskedTextBox();
            this.labelAzureAccountKey = new System.Windows.Forms.Label();
            this.maskedTextBoxAzureAccountKey = new System.Windows.Forms.MaskedTextBox();
            this.buttonAzureAccountAdd = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelAzureAccountName
            // 
            this.labelAzureAccountName.AutoSize = true;
            this.labelAzureAccountName.Location = new System.Drawing.Point(26, 31);
            this.labelAzureAccountName.Name = "labelAzureAccountName";
            this.labelAzureAccountName.Size = new System.Drawing.Size(78, 13);
            this.labelAzureAccountName.TabIndex = 0;
            this.labelAzureAccountName.Text = "Account Name";
            // 
            // maskedTextBoxAzureAccountName
            // 
            this.maskedTextBoxAzureAccountName.Location = new System.Drawing.Point(110, 28);
            this.maskedTextBoxAzureAccountName.Name = "maskedTextBoxAzureAccountName";
            this.maskedTextBoxAzureAccountName.Size = new System.Drawing.Size(197, 20);
            this.maskedTextBoxAzureAccountName.TabIndex = 1;
            this.maskedTextBoxAzureAccountName.MaskInputRejected += new System.Windows.Forms.MaskInputRejectedEventHandler(this.maskedTextBoxAzureAccountName_MaskInputRejected);
            // 
            // labelAzureAccountKey
            // 
            this.labelAzureAccountKey.AutoSize = true;
            this.labelAzureAccountKey.Location = new System.Drawing.Point(36, 74);
            this.labelAzureAccountKey.Name = "labelAzureAccountKey";
            this.labelAzureAccountKey.Size = new System.Drawing.Size(68, 13);
            this.labelAzureAccountKey.TabIndex = 2;
            this.labelAzureAccountKey.Text = "Account Key";
            // 
            // maskedTextBoxAzureAccountKey
            // 
            this.maskedTextBoxAzureAccountKey.Location = new System.Drawing.Point(110, 71);
            this.maskedTextBoxAzureAccountKey.Name = "maskedTextBoxAzureAccountKey";
            this.maskedTextBoxAzureAccountKey.Size = new System.Drawing.Size(197, 20);
            this.maskedTextBoxAzureAccountKey.TabIndex = 3;
            this.maskedTextBoxAzureAccountKey.MaskInputRejected += new System.Windows.Forms.MaskInputRejectedEventHandler(this.maskedTextBoxAzureAccountKey_MaskInputRejected);
            // 
            // buttonAzureAccountAdd
            // 
            this.buttonAzureAccountAdd.Location = new System.Drawing.Point(232, 142);
            this.buttonAzureAccountAdd.Name = "buttonAzureAccountAdd";
            this.buttonAzureAccountAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonAzureAccountAdd.TabIndex = 4;
            this.buttonAzureAccountAdd.Text = "Add";
            this.buttonAzureAccountAdd.UseVisualStyleBackColor = true;
            this.buttonAzureAccountAdd.Click += new System.EventHandler(this.buttonAzureAccountAdd_Click);
            // 
            // AzureAccountForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(338, 177);
            this.Controls.Add(this.buttonAzureAccountAdd);
            this.Controls.Add(this.maskedTextBoxAzureAccountKey);
            this.Controls.Add(this.labelAzureAccountKey);
            this.Controls.Add(this.maskedTextBoxAzureAccountName);
            this.Controls.Add(this.labelAzureAccountName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AzureAccountForm";
            this.Text = "Azure Account";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelAzureAccountName;
        private System.Windows.Forms.MaskedTextBox maskedTextBoxAzureAccountName;
        private System.Windows.Forms.Label labelAzureAccountKey;
        private System.Windows.Forms.MaskedTextBox maskedTextBoxAzureAccountKey;
        private System.Windows.Forms.Button buttonAzureAccountAdd;
    }
}