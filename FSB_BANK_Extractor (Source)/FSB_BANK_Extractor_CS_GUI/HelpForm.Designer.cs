// HelpForm.Designer.cs
namespace FSB_BANK_Extractor_CS_GUI
{
    partial class HelpForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HelpForm));
            this.richTextBoxHelpContent = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // richTextBoxHelpContent
            // 
            this.richTextBoxHelpContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxHelpContent.Location = new System.Drawing.Point(0, 0);
            this.richTextBoxHelpContent.Name = "richTextBoxHelpContent";
            this.richTextBoxHelpContent.ReadOnly = true;
            this.richTextBoxHelpContent.Size = new System.Drawing.Size(838, 441);
            this.richTextBoxHelpContent.TabIndex = 0;
            this.richTextBoxHelpContent.Text = "";
            // 
            // HelpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(838, 441);
            this.Controls.Add(this.richTextBoxHelpContent);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "HelpForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "도움말(Help)";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBoxHelpContent;
    }
}