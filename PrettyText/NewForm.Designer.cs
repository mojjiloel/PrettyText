namespace PrettyText
{
    partial class NewForm
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
            AntdUI.Tabs.StyleLine styleLine6 = new AntdUI.Tabs.StyleLine();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewForm));
            this.pageHeader1 = new AntdUI.PageHeader();
            this.button_color = new AntdUI.Button();
            this.splitContainer1 = new AntdUI.Splitter();
            this.panelLeft = new AntdUI.Panel();
            this.txtInput = new AntdUI.Input();
            this.panelRight = new AntdUI.Panel();
            this.tabControl1 = new AntdUI.Tabs();
            this.tabPage1 = new AntdUI.TabPage();
            this.txtOutput = new AntdUI.Input();
            this.tabPage2 = new AntdUI.TabPage();
            this.treeOutput = new AntdUI.Tree();
            this.panelToolbar = new AntdUI.Panel();
            this.btnPretty = new AntdUI.Button();
            this.btnMinify = new AntdUI.Button();
            this.btnDetect = new AntdUI.Button();
            this.cboFormat = new AntdUI.Select();
            this.btnCopy = new AntdUI.Button();
            this.btnOpen = new AntdUI.Button();
            this.btnSave = new AntdUI.Button();
            this.btnExpandAll = new AntdUI.Button();
            this.btnCollapseAll = new AntdUI.Button();
            this.txtFind = new AntdUI.Input();
            this.btnFindPrev = new AntdUI.Button();
            this.btnFindNext = new AntdUI.Button();
            this.btnWrap = new AntdUI.Button();
            this.cboHistory = new AntdUI.Select();
            this.btnFont = new AntdUI.Button();
            this.statusPanel = new AntdUI.Panel();
            this.lblStatus = new AntdUI.Label();
            this.lblStats = new AntdUI.Label();
            this.pageHeader1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelLeft.SuspendLayout();
            this.panelRight.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panelToolbar.SuspendLayout();
            this.statusPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // pageHeader1
            // 
            this.pageHeader1.Controls.Add(this.button_color);
            this.pageHeader1.Dock = System.Windows.Forms.DockStyle.Top;
            this.pageHeader1.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.pageHeader1.Icon = global::PrettyText.Properties.Resources.app_preview;
            this.pageHeader1.Location = new System.Drawing.Point(0, 0);
            this.pageHeader1.Margin = new System.Windows.Forms.Padding(4);
            this.pageHeader1.Name = "pageHeader1";
            this.pageHeader1.ShowButton = true;
            this.pageHeader1.ShowIcon = true;
            this.pageHeader1.Size = new System.Drawing.Size(1349, 40);
            this.pageHeader1.TabIndex = 0;
            this.pageHeader1.Text = "PrettyText - 文本格式化工具";
            // 
            // button_color
            // 
            this.button_color.Dock = System.Windows.Forms.DockStyle.Right;
            this.button_color.Ghost = true;
            this.button_color.IconRatio = 0.6F;
            this.button_color.IconSvg = "SunOutlined";
            this.button_color.Location = new System.Drawing.Point(1155, 0);
            this.button_color.Name = "button_color";
            this.button_color.Radius = 0;
            this.button_color.Size = new System.Drawing.Size(50, 40);
            this.button_color.TabIndex = 2;
            this.button_color.ToggleIconSvg = "MoonOutlined";
            this.button_color.WaveSize = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Cursor = System.Windows.Forms.Cursors.Default;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 100);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panelLeft);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panelRight);
            this.splitContainer1.Size = new System.Drawing.Size(1349, 448);
            this.splitContainer1.SplitterDistance = 618;
            this.splitContainer1.TabIndex = 2;
            // 
            // panelLeft
            // 
            this.panelLeft.Controls.Add(this.txtInput);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(618, 448);
            this.panelLeft.TabIndex = 0;
            // 
            // txtInput
            // 
            this.txtInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtInput.Location = new System.Drawing.Point(0, 0);
            this.txtInput.Multiline = true;
            this.txtInput.Name = "txtInput";
            this.txtInput.PlaceholderText = "请输入要格式化的文本...";
            this.txtInput.Size = new System.Drawing.Size(618, 448);
            this.txtInput.TabIndex = 0;
            // 
            // panelRight
            // 
            this.panelRight.Controls.Add(this.tabControl1);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(0, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(727, 448);
            this.panelRight.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Cursor = System.Windows.Forms.Cursors.Default;
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Pages.Add(this.tabPage1);
            this.tabControl1.Pages.Add(this.tabPage2);
            this.tabControl1.Size = new System.Drawing.Size(727, 448);
            this.tabControl1.Style = styleLine6;
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.txtOutput);
            this.tabPage1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPage1.Location = new System.Drawing.Point(0, 30);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(727, 418);
            this.tabPage1.TabIndex = 2;
            this.tabPage1.Text = "Text";
            // 
            // txtOutput
            // 
            this.txtOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtOutput.Location = new System.Drawing.Point(0, 0);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.PlaceholderText = "格式化后的文本将显示在这里...";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.Size = new System.Drawing.Size(727, 418);
            this.txtOutput.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.treeOutput);
            this.tabPage2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPage2.Location = new System.Drawing.Point(0, 30);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(727, 418);
            this.tabPage2.TabIndex = 3;
            this.tabPage2.Text = "Tree";
            // 
            // treeOutput
            // 
            this.treeOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeOutput.Location = new System.Drawing.Point(0, 0);
            this.treeOutput.Name = "treeOutput";
            this.treeOutput.Size = new System.Drawing.Size(727, 418);
            this.treeOutput.TabIndex = 0;
            // 
            // panelToolbar
            // 
            this.panelToolbar.Controls.Add(this.btnPretty);
            this.panelToolbar.Controls.Add(this.btnMinify);
            this.panelToolbar.Controls.Add(this.btnDetect);
            this.panelToolbar.Controls.Add(this.cboFormat);
            this.panelToolbar.Controls.Add(this.btnCopy);
            this.panelToolbar.Controls.Add(this.btnOpen);
            this.panelToolbar.Controls.Add(this.btnSave);
            this.panelToolbar.Controls.Add(this.btnExpandAll);
            this.panelToolbar.Controls.Add(this.btnCollapseAll);
            this.panelToolbar.Controls.Add(this.txtFind);
            this.panelToolbar.Controls.Add(this.btnFindPrev);
            this.panelToolbar.Controls.Add(this.btnFindNext);
            this.panelToolbar.Controls.Add(this.btnWrap);
            this.panelToolbar.Controls.Add(this.cboHistory);
            this.panelToolbar.Controls.Add(this.btnFont);
            this.panelToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelToolbar.Location = new System.Drawing.Point(0, 40);
            this.panelToolbar.Name = "panelToolbar";
            this.panelToolbar.Size = new System.Drawing.Size(1349, 60);
            this.panelToolbar.TabIndex = 3;
            // 
            // btnPretty
            // 
            this.btnPretty.BorderWidth = 1F;
            this.btnPretty.IconSvg = "<svg viewBox=\"0 0 24 24\"><path d=\"M7 15v2h10v-2zm-4 6h18v-2H3zm0-8h18v-2H3zm4-6v2" +
    "h10V7zM3 3v2h18V3z\"/></svg>";
            this.btnPretty.JoinMode = AntdUI.TJoinMode.Left;
            this.btnPretty.Location = new System.Drawing.Point(10, 15);
            this.btnPretty.Name = "btnPretty";
            this.btnPretty.Size = new System.Drawing.Size(50, 30);
            this.btnPretty.TabIndex = 0;
            // 
            // btnMinify
            // 
            this.btnMinify.BorderWidth = 1F;
            this.btnMinify.IconSvg = "<svg viewBox=\"0 0 24 24\"><path d=\"M3 21h18v-2H3zm0-4h18v-2H3zm0-4h18v-2H3zm0-4h18" +
    "V7H3zm0-6v2h18V3z\"/></svg>";
            this.btnMinify.JoinMode = AntdUI.TJoinMode.LR;
            this.btnMinify.Location = new System.Drawing.Point(60, 15);
            this.btnMinify.Name = "btnMinify";
            this.btnMinify.Size = new System.Drawing.Size(50, 30);
            this.btnMinify.TabIndex = 1;
            // 
            // btnDetect
            // 
            this.btnDetect.BorderWidth = 1F;
            this.btnDetect.IconSvg = resources.GetString("btnDetect.IconSvg");
            this.btnDetect.JoinMode = AntdUI.TJoinMode.Right;
            this.btnDetect.Location = new System.Drawing.Point(110, 15);
            this.btnDetect.Name = "btnDetect";
            this.btnDetect.Size = new System.Drawing.Size(50, 30);
            this.btnDetect.TabIndex = 2;
            // 
            // cboFormat
            // 
            this.cboFormat.Location = new System.Drawing.Point(158, 15);
            this.cboFormat.Name = "cboFormat";
            this.cboFormat.Size = new System.Drawing.Size(120, 30);
            this.cboFormat.TabIndex = 3;
            // 
            // btnCopy
            // 
            this.btnCopy.BorderWidth = 1F;
            this.btnCopy.IconSvg = resources.GetString("btnCopy.IconSvg");
            this.btnCopy.JoinMode = AntdUI.TJoinMode.Left;
            this.btnCopy.Location = new System.Drawing.Point(288, 15);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(50, 30);
            this.btnCopy.TabIndex = 4;
            // 
            // btnOpen
            // 
            this.btnOpen.BorderWidth = 1F;
            this.btnOpen.IconSvg = "<svg viewBox=\"0 0 24 24\"><path d=\"M15 22H6c-1.1 0-2-.9-2-2V4c0-1.1.9-2 2-2h8l6 6v" +
    "6h-2V9h-5V4H6v16h9zm4-.34v-2.24l2.95 2.95l1.41-1.41L20.41 18h2.24v-2H17v5.66z\"/>" +
    "</svg>";
            this.btnOpen.JoinMode = AntdUI.TJoinMode.LR;
            this.btnOpen.Location = new System.Drawing.Point(338, 15);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(50, 30);
            this.btnOpen.TabIndex = 5;
            // 
            // btnSave
            // 
            this.btnSave.BorderWidth = 1F;
            this.btnSave.IconSvg = resources.GetString("btnSave.IconSvg");
            this.btnSave.JoinMode = AntdUI.TJoinMode.Right;
            this.btnSave.Location = new System.Drawing.Point(388, 15);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(50, 30);
            this.btnSave.TabIndex = 6;
            // 
            // btnExpandAll
            // 
            this.btnExpandAll.BorderWidth = 1F;
            this.btnExpandAll.JoinMode = AntdUI.TJoinMode.Left;
            this.btnExpandAll.Location = new System.Drawing.Point(616, 15);
            this.btnExpandAll.Name = "btnExpandAll";
            this.btnExpandAll.Size = new System.Drawing.Size(50, 30);
            this.btnExpandAll.TabIndex = 7;
            this.btnExpandAll.Text = "➕ ";
            // 
            // btnCollapseAll
            // 
            this.btnCollapseAll.BorderWidth = 1F;
            this.btnCollapseAll.JoinMode = AntdUI.TJoinMode.Right;
            this.btnCollapseAll.Location = new System.Drawing.Point(665, 15);
            this.btnCollapseAll.Name = "btnCollapseAll";
            this.btnCollapseAll.Size = new System.Drawing.Size(50, 30);
            this.btnCollapseAll.TabIndex = 8;
            this.btnCollapseAll.Text = "➖ ";
            // 
            // txtFind
            // 
            this.txtFind.Location = new System.Drawing.Point(728, 15);
            this.txtFind.Name = "txtFind";
            this.txtFind.PlaceholderText = "🔍 查找...";
            this.txtFind.Size = new System.Drawing.Size(120, 30);
            this.txtFind.TabIndex = 9;
            // 
            // btnFindPrev
            // 
            this.btnFindPrev.BorderWidth = 1F;
            this.btnFindPrev.JoinMode = AntdUI.TJoinMode.Left;
            this.btnFindPrev.Location = new System.Drawing.Point(858, 15);
            this.btnFindPrev.Name = "btnFindPrev";
            this.btnFindPrev.Size = new System.Drawing.Size(30, 30);
            this.btnFindPrev.TabIndex = 10;
            this.btnFindPrev.Text = "◄";
            // 
            // btnFindNext
            // 
            this.btnFindNext.BorderWidth = 1F;
            this.btnFindNext.JoinMode = AntdUI.TJoinMode.LR;
            this.btnFindNext.Location = new System.Drawing.Point(888, 15);
            this.btnFindNext.Name = "btnFindNext";
            this.btnFindNext.Size = new System.Drawing.Size(30, 30);
            this.btnFindNext.TabIndex = 11;
            this.btnFindNext.Text = "►";
            // 
            // btnWrap
            // 
            this.btnWrap.BorderWidth = 1F;
            this.btnWrap.JoinMode = AntdUI.TJoinMode.Right;
            this.btnWrap.Location = new System.Drawing.Point(918, 15);
            this.btnWrap.Name = "btnWrap";
            this.btnWrap.Size = new System.Drawing.Size(30, 30);
            this.btnWrap.TabIndex = 12;
            this.btnWrap.Text = "↵";
            // 
            // cboHistory
            // 
            this.cboHistory.Location = new System.Drawing.Point(974, 15);
            this.cboHistory.Name = "cboHistory";
            this.cboHistory.Size = new System.Drawing.Size(150, 30);
            this.cboHistory.TabIndex = 14;
            // 
            // btnFont
            // 
            this.btnFont.BorderWidth = 1F;
            this.btnFont.IconSvg = resources.GetString("btnFont.IconSvg");
            this.btnFont.Location = new System.Drawing.Point(1130, 15);
            this.btnFont.Name = "btnFont";
            this.btnFont.Size = new System.Drawing.Size(50, 30);
            this.btnFont.TabIndex = 15;
            // 
            // statusPanel
            // 
            this.statusPanel.Controls.Add(this.lblStatus);
            this.statusPanel.Controls.Add(this.lblStats);
            this.statusPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusPanel.Location = new System.Drawing.Point(0, 548);
            this.statusPanel.Name = "statusPanel";
            this.statusPanel.Size = new System.Drawing.Size(1349, 22);
            this.statusPanel.TabIndex = 4;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSizeMode = AntdUI.TAutoSize.Auto;
            this.lblStatus.Location = new System.Drawing.Point(10, 4);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(40, 16);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "✅ 就绪";
            // 
            // lblStats
            // 
            this.lblStats.AutoSizeMode = AntdUI.TAutoSize.Auto;
            this.lblStats.Location = new System.Drawing.Point(1299, 4);
            this.lblStats.Name = "lblStats";
            this.lblStats.Size = new System.Drawing.Size(0, 16);
            this.lblStats.TabIndex = 1;
            // 
            // NewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1349, 570);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusPanel);
            this.Controls.Add(this.panelToolbar);
            this.Controls.Add(this.pageHeader1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "NewForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PrettyText - 文本格式化工具";
            this.pageHeader1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panelLeft.ResumeLayout(false);
            this.panelRight.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.panelToolbar.ResumeLayout(false);
            this.statusPanel.ResumeLayout(false);
            this.statusPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private AntdUI.PageHeader pageHeader1;
        private AntdUI.Splitter splitContainer1;
        private AntdUI.Panel panelLeft;
        private AntdUI.Input txtInput;
        private AntdUI.Panel panelRight;
        private AntdUI.Tabs tabControl1;
        private AntdUI.Input txtOutput;
        private AntdUI.Tree treeOutput;
        private AntdUI.Panel panelToolbar;
        private AntdUI.Button btnPretty;
        private AntdUI.Button btnMinify;
        private AntdUI.Button btnDetect;
        private AntdUI.Select cboFormat;
        private AntdUI.Button btnCopy;
        private AntdUI.Button btnOpen;
        private AntdUI.Button btnSave;
        private AntdUI.Button btnExpandAll;
        private AntdUI.Button btnCollapseAll;
        private AntdUI.Input txtFind;
        private AntdUI.Button btnFindPrev;
        private AntdUI.Button btnFindNext;
        private AntdUI.Button btnWrap;
        private AntdUI.Select cboHistory;
        private AntdUI.Button btnFont;
        private AntdUI.Panel statusPanel;
        private AntdUI.Label lblStatus;
        private AntdUI.Label lblStats;
        private AntdUI.TabPage tabPage1;
        private AntdUI.TabPage tabPage2;
        private AntdUI.Button button_color;
    }
}