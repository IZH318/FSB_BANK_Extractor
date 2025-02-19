// FSBDetailsForm.designer.cs
namespace FSB_BANK_Extractor_CS_GUI
{
    partial class FSBDetailsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FSBDetailsForm));
            this.treeViewDetails = new System.Windows.Forms.TreeView();
            this.labelLoading = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // treeViewDetails
            // 
            this.treeViewDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewDetails.Location = new System.Drawing.Point(0, 0);
            this.treeViewDetails.Name = "treeViewDetails";
            this.treeViewDetails.Size = new System.Drawing.Size(838, 441);
            this.treeViewDetails.TabIndex = 0;
            // 
            // labelLoading
            // 
            this.labelLoading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelLoading.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelLoading.Location = new System.Drawing.Point(0, 0);
            this.labelLoading.Name = "labelLoading";
            this.labelLoading.Size = new System.Drawing.Size(838, 441);
            this.labelLoading.TabIndex = 1;
            this.labelLoading.Text = "Loading file information...";
            this.labelLoading.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelLoading.Visible = false;
            // 
            // FSBDetailsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(838, 441);
            this.Controls.Add(this.labelLoading);
            this.Controls.Add(this.treeViewDetails);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FSBDetailsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FSB File Details (TreeView)";
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TreeView treeViewDetails;
        private System.Windows.Forms.Label labelLoading;
    }
}