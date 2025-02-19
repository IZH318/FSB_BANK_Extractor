// Form1.Designer.cs
namespace FSB_BANK_Extractor_CS_GUI
{
    partial class FSB_BANK_Extractor_CS_GUI
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FSB_BANK_Extractor_CS_GUI));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelProgress = new System.Windows.Forms.ToolStripStatusLabel();
            this.groupBoxOutputDir = new System.Windows.Forms.GroupBox();
            this.radioButtonOutputDirCustom = new System.Windows.Forms.RadioButton();
            this.buttonOutputDirBrowse = new System.Windows.Forms.Button();
            this.textBoxOutputDirPath = new System.Windows.Forms.TextBox();
            this.radioButtonOutputDirExe = new System.Windows.Forms.RadioButton();
            this.radioButtonOutputDirRes = new System.Windows.Forms.RadioButton();
            this.checkBoxVerboseLog = new System.Windows.Forms.CheckBox();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.listViewFiles = new System.Windows.Forms.ListView();
            this.columnHeaderFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonAddFiles = new System.Windows.Forms.Button();
            this.buttonAddFolder = new System.Windows.Forms.Button();
            this.buttonRemoveSelectedFiles = new System.Windows.Forms.Button();
            this.buttonClearFileList = new System.Windows.Forms.Button();
            this.buttonBatchExtract = new System.Windows.Forms.Button();
            this.toolStripStatusLabelSubSound = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelFile = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.메뉴ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.도움말ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.정보ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.끝내기ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1.SuspendLayout();
            this.groupBoxOutputDir.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.AllowMerge = false;
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelProgress});
            this.statusStrip1.Location = new System.Drawing.Point(0, 532);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 12, 0);
            this.statusStrip1.Size = new System.Drawing.Size(539, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 17;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabelProgress
            // 
            this.toolStripStatusLabelProgress.Name = "toolStripStatusLabelProgress";
            this.toolStripStatusLabelProgress.Size = new System.Drawing.Size(526, 17);
            this.toolStripStatusLabelProgress.Spring = true;
            this.toolStripStatusLabelProgress.Text = "Idle";
            this.toolStripStatusLabelProgress.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBoxOutputDir
            // 
            this.groupBoxOutputDir.Controls.Add(this.radioButtonOutputDirCustom);
            this.groupBoxOutputDir.Controls.Add(this.buttonOutputDirBrowse);
            this.groupBoxOutputDir.Controls.Add(this.textBoxOutputDirPath);
            this.groupBoxOutputDir.Controls.Add(this.radioButtonOutputDirExe);
            this.groupBoxOutputDir.Controls.Add(this.radioButtonOutputDirRes);
            this.groupBoxOutputDir.Controls.Add(this.checkBoxVerboseLog);
            this.groupBoxOutputDir.Location = new System.Drawing.Point(12, 226);
            this.groupBoxOutputDir.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxOutputDir.Name = "groupBoxOutputDir";
            this.groupBoxOutputDir.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxOutputDir.Size = new System.Drawing.Size(515, 84);
            this.groupBoxOutputDir.TabIndex = 3;
            this.groupBoxOutputDir.TabStop = false;
            this.groupBoxOutputDir.Text = "Output Options";
            // 
            // radioButtonOutputDirCustom
            // 
            this.radioButtonOutputDirCustom.AutoSize = true;
            this.radioButtonOutputDirCustom.Location = new System.Drawing.Point(6, 58);
            this.radioButtonOutputDirCustom.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.radioButtonOutputDirCustom.Name = "radioButtonOutputDirCustom";
            this.radioButtonOutputDirCustom.Size = new System.Drawing.Size(137, 16);
            this.radioButtonOutputDirCustom.TabIndex = 4;
            this.radioButtonOutputDirCustom.Text = "Custom output path:";
            this.radioButtonOutputDirCustom.UseVisualStyleBackColor = true;
            this.radioButtonOutputDirCustom.CheckedChanged += new System.EventHandler(this.RadioButton_OutputDirCustom_CheckedChanged);
            // 
            // buttonOutputDirBrowse
            // 
            this.buttonOutputDirBrowse.Enabled = false;
            this.buttonOutputDirBrowse.Location = new System.Drawing.Point(409, 55);
            this.buttonOutputDirBrowse.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonOutputDirBrowse.Name = "buttonOutputDirBrowse";
            this.buttonOutputDirBrowse.Size = new System.Drawing.Size(100, 22);
            this.buttonOutputDirBrowse.TabIndex = 6;
            this.buttonOutputDirBrowse.Text = "Browse";
            this.buttonOutputDirBrowse.UseVisualStyleBackColor = true;
            this.buttonOutputDirBrowse.Click += new System.EventHandler(this.Button_OutputDirBrowse_Click);
            // 
            // textBoxOutputDirPath
            // 
            this.textBoxOutputDirPath.Enabled = false;
            this.textBoxOutputDirPath.Location = new System.Drawing.Point(149, 55);
            this.textBoxOutputDirPath.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxOutputDirPath.Name = "textBoxOutputDirPath";
            this.textBoxOutputDirPath.ReadOnly = true;
            this.textBoxOutputDirPath.Size = new System.Drawing.Size(254, 21);
            this.textBoxOutputDirPath.TabIndex = 5;
            // 
            // radioButtonOutputDirExe
            // 
            this.radioButtonOutputDirExe.AutoSize = true;
            this.radioButtonOutputDirExe.Location = new System.Drawing.Point(6, 38);
            this.radioButtonOutputDirExe.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.radioButtonOutputDirExe.Name = "radioButtonOutputDirExe";
            this.radioButtonOutputDirExe.Size = new System.Drawing.Size(153, 16);
            this.radioButtonOutputDirExe.TabIndex = 1;
            this.radioButtonOutputDirExe.Text = "Same path as program";
            this.radioButtonOutputDirExe.UseVisualStyleBackColor = true;
            // 
            // radioButtonOutputDirRes
            // 
            this.radioButtonOutputDirRes.AutoSize = true;
            this.radioButtonOutputDirRes.Checked = true;
            this.radioButtonOutputDirRes.Location = new System.Drawing.Point(6, 18);
            this.radioButtonOutputDirRes.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.radioButtonOutputDirRes.Name = "radioButtonOutputDirRes";
            this.radioButtonOutputDirRes.Size = new System.Drawing.Size(176, 16);
            this.radioButtonOutputDirRes.TabIndex = 0;
            this.radioButtonOutputDirRes.TabStop = true;
            this.radioButtonOutputDirRes.Text = "Same path as resource file";
            this.radioButtonOutputDirRes.UseVisualStyleBackColor = true;
            // 
            // checkBoxVerboseLog
            // 
            this.checkBoxVerboseLog.AutoSize = true;
            this.checkBoxVerboseLog.Location = new System.Drawing.Point(370, 19);
            this.checkBoxVerboseLog.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.checkBoxVerboseLog.Name = "checkBoxVerboseLog";
            this.checkBoxVerboseLog.Size = new System.Drawing.Size(128, 16);
            this.checkBoxVerboseLog.TabIndex = 4;
            this.checkBoxVerboseLog.Text = "Verbose Log Save";
            this.checkBoxVerboseLog.UseVisualStyleBackColor = true;
            // 
            // textBoxLog
            // 
            this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLog.Location = new System.Drawing.Point(12, 370);
            this.textBoxLog.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLog.Size = new System.Drawing.Size(515, 150);
            this.textBoxLog.TabIndex = 6;
            // 
            // listViewFiles
            // 
            this.listViewFiles.AllowDrop = true;
            this.listViewFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFileName,
            this.columnHeaderStatus});
            this.listViewFiles.FullRowSelect = true;
            this.listViewFiles.HideSelection = false;
            this.listViewFiles.Location = new System.Drawing.Point(12, 36);
            this.listViewFiles.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.listViewFiles.MultiSelect = false;
            this.listViewFiles.Name = "listViewFiles";
            this.listViewFiles.Size = new System.Drawing.Size(515, 150);
            this.listViewFiles.TabIndex = 7;
            this.listViewFiles.UseCompatibleStateImageBehavior = false;
            this.listViewFiles.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderFileName
            // 
            this.columnHeaderFileName.Text = "File Name";
            this.columnHeaderFileName.Width = 350;
            // 
            // columnHeaderStatus
            // 
            this.columnHeaderStatus.Text = "Status";
            this.columnHeaderStatus.Width = 100;
            // 
            // buttonAddFiles
            // 
            this.buttonAddFiles.Location = new System.Drawing.Point(12, 190);
            this.buttonAddFiles.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonAddFiles.Name = "buttonAddFiles";
            this.buttonAddFiles.Size = new System.Drawing.Size(100, 22);
            this.buttonAddFiles.TabIndex = 8;
            this.buttonAddFiles.Text = "Add Files";
            this.buttonAddFiles.UseVisualStyleBackColor = true;
            this.buttonAddFiles.Click += new System.EventHandler(this.Button_AddFiles_Click);
            // 
            // buttonAddFolder
            // 
            this.buttonAddFolder.Location = new System.Drawing.Point(118, 190);
            this.buttonAddFolder.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonAddFolder.Name = "buttonAddFolder";
            this.buttonAddFolder.Size = new System.Drawing.Size(100, 22);
            this.buttonAddFolder.TabIndex = 9;
            this.buttonAddFolder.Text = "Add Folder";
            this.buttonAddFolder.UseVisualStyleBackColor = true;
            this.buttonAddFolder.Click += new System.EventHandler(this.Button_AddFolder_Click);
            // 
            // buttonRemoveSelectedFiles
            // 
            this.buttonRemoveSelectedFiles.Location = new System.Drawing.Point(321, 190);
            this.buttonRemoveSelectedFiles.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonRemoveSelectedFiles.Name = "buttonRemoveSelectedFiles";
            this.buttonRemoveSelectedFiles.Size = new System.Drawing.Size(100, 22);
            this.buttonRemoveSelectedFiles.TabIndex = 10;
            this.buttonRemoveSelectedFiles.Text = "Remove Selected";
            this.buttonRemoveSelectedFiles.UseVisualStyleBackColor = true;
            this.buttonRemoveSelectedFiles.Click += new System.EventHandler(this.Button_RemoveSelectedFiles_Click);
            // 
            // buttonClearFileList
            // 
            this.buttonClearFileList.Location = new System.Drawing.Point(427, 190);
            this.buttonClearFileList.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonClearFileList.Name = "buttonClearFileList";
            this.buttonClearFileList.Size = new System.Drawing.Size(100, 22);
            this.buttonClearFileList.TabIndex = 11;
            this.buttonClearFileList.Text = "Clear File List";
            this.buttonClearFileList.UseVisualStyleBackColor = true;
            this.buttonClearFileList.Click += new System.EventHandler(this.Button_ClearFileList_Click);
            // 
            // buttonBatchExtract
            // 
            this.buttonBatchExtract.Location = new System.Drawing.Point(12, 324);
            this.buttonBatchExtract.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonBatchExtract.Name = "buttonBatchExtract";
            this.buttonBatchExtract.Size = new System.Drawing.Size(515, 32);
            this.buttonBatchExtract.TabIndex = 12;
            this.buttonBatchExtract.Text = "Start Batch Extract";
            this.buttonBatchExtract.UseVisualStyleBackColor = true;
            this.buttonBatchExtract.Click += new System.EventHandler(this.Button_BatchExtract_Click);
            // 
            // toolStripStatusLabelSubSound
            // 
            this.toolStripStatusLabelSubSound.Name = "toolStripStatusLabelSubSound";
            this.toolStripStatusLabelSubSound.Size = new System.Drawing.Size(0, 17);
            this.toolStripStatusLabelSubSound.Text = "toolStripStatusLabelSubSound";
            this.toolStripStatusLabelSubSound.Visible = false;
            // 
            // toolStripStatusLabelFile
            // 
            this.toolStripStatusLabelFile.Name = "toolStripStatusLabelFile";
            this.toolStripStatusLabelFile.Size = new System.Drawing.Size(0, 17);
            this.toolStripStatusLabelFile.Text = "toolStripStatusLabelFile";
            this.toolStripStatusLabelFile.Visible = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.메뉴ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(539, 24);
            this.menuStrip1.TabIndex = 18;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 메뉴ToolStripMenuItem
            // 
            this.메뉴ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.도움말ToolStripMenuItem,
            this.정보ToolStripMenuItem,
            this.toolStripSeparator1,
            this.끝내기ToolStripMenuItem});
            this.메뉴ToolStripMenuItem.Name = "메뉴ToolStripMenuItem";
            this.메뉴ToolStripMenuItem.Size = new System.Drawing.Size(82, 20);
            this.메뉴ToolStripMenuItem.Text = "메뉴(Menu)";
            // 
            // 도움말ToolStripMenuItem
            // 
            this.도움말ToolStripMenuItem.Name = "도움말ToolStripMenuItem";
            this.도움말ToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.도움말ToolStripMenuItem.Text = "도움말(Help)";
            this.도움말ToolStripMenuItem.Click += new System.EventHandler(this.도움말ToolStripMenuItem_Click);
            // 
            // 정보ToolStripMenuItem
            // 
            this.정보ToolStripMenuItem.Name = "정보ToolStripMenuItem";
            this.정보ToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.정보ToolStripMenuItem.Text = "정보(About)";
            this.정보ToolStripMenuItem.Click += new System.EventHandler(this.정보ToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(140, 6);
            // 
            // 끝내기ToolStripMenuItem
            // 
            this.끝내기ToolStripMenuItem.Name = "끝내기ToolStripMenuItem";
            this.끝내기ToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.끝내기ToolStripMenuItem.Text = "끝내기(Exit)";
            this.끝내기ToolStripMenuItem.Click += new System.EventHandler(this.끝내기ToolStripMenuItem_Click);
            // 
            // FSB_BANK_Extractor_CS_GUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(539, 554);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.buttonBatchExtract);
            this.Controls.Add(this.buttonClearFileList);
            this.Controls.Add(this.buttonRemoveSelectedFiles);
            this.Controls.Add(this.buttonAddFolder);
            this.Controls.Add(this.buttonAddFiles);
            this.Controls.Add(this.listViewFiles);
            this.Controls.Add(this.textBoxLog);
            this.Controls.Add(this.groupBoxOutputDir);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.Name = "FSB_BANK_Extractor_CS_GUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FSB/BANK Extractor GUI";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBoxOutputDir.ResumeLayout(false);
            this.groupBoxOutputDir.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxOutputDir;
        private System.Windows.Forms.RadioButton radioButtonOutputDirCustom;
        private System.Windows.Forms.Button buttonOutputDirBrowse;
        private System.Windows.Forms.TextBox textBoxOutputDirPath;
        private System.Windows.Forms.RadioButton radioButtonOutputDirExe;
        private System.Windows.Forms.RadioButton radioButtonOutputDirRes;
        private System.Windows.Forms.CheckBox checkBoxVerboseLog;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.ListView listViewFiles;
        private System.Windows.Forms.ColumnHeader columnHeaderFileName;
        private System.Windows.Forms.ColumnHeader columnHeaderStatus;
        private System.Windows.Forms.Button buttonAddFiles;
        private System.Windows.Forms.Button buttonAddFolder;
        private System.Windows.Forms.Button buttonRemoveSelectedFiles;
        private System.Windows.Forms.Button buttonClearFileList;
        private System.Windows.Forms.Button buttonBatchExtract;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelProgress;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelSubSound;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelFile;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 메뉴ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 도움말ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 정보ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem 끝내기ToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
    }
}