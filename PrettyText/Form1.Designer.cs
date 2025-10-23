namespace PrettyText
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.txtInput = new System.Windows.Forms.RichTextBox();
            this.tabOutput = new System.Windows.Forms.TabControl();
            this.tabText = new System.Windows.Forms.TabPage();
            this.txtOutput = new System.Windows.Forms.RichTextBox();
            this.tabTree = new System.Windows.Forms.TabPage();
            this.treeOutput = new System.Windows.Forms.TreeView();
            this.ctxTree = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ctxCopyNode = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxCopyValue = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxCopyPath = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnPretty = new System.Windows.Forms.ToolStripButton();
            this.btnMinify = new System.Windows.Forms.ToolStripButton();
            this.btnDetect = new System.Windows.Forms.ToolStripButton();
            this.sep1 = new System.Windows.Forms.ToolStripSeparator();
            this.cboFormat = new System.Windows.Forms.ToolStripComboBox();
            this.sep2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnCopy = new System.Windows.Forms.ToolStripButton();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.sep3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnExpandAll = new System.Windows.Forms.ToolStripButton();
            this.btnCollapseAll = new System.Windows.Forms.ToolStripButton();
            this.sep4 = new System.Windows.Forms.ToolStripSeparator();
            this.txtFind = new System.Windows.Forms.ToolStripTextBox();
            this.btnFindPrev = new System.Windows.Forms.ToolStripButton();
            this.btnFindNext = new System.Windows.Forms.ToolStripButton();
            this.sep5 = new System.Windows.Forms.ToolStripSeparator();
            this.btnWrap = new System.Windows.Forms.ToolStripButton();
            this.btnTheme = new System.Windows.Forms.ToolStripButton();
            this.sep6 = new System.Windows.Forms.ToolStripSeparator();
            this.cboHistory = new System.Windows.Forms.ToolStripComboBox();
            this.sep7 = new System.Windows.Forms.ToolStripSeparator();
            this.btnFont = new System.Windows.Forms.ToolStripDropDownButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStats = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabOutput.SuspendLayout();
            this.tabText.SuspendLayout();
            this.tabTree.SuspendLayout();
            this.ctxTree.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 33);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.txtInput);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabOutput);
            this.splitContainer1.Size = new System.Drawing.Size(800, 395);
            this.splitContainer1.SplitterDistance = 380;
            this.splitContainer1.TabIndex = 0;
            // 
            // txtInput
            // 
            this.txtInput.BackColor = System.Drawing.Color.White;
            this.txtInput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtInput.DetectUrls = false;
            this.txtInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtInput.Font = new System.Drawing.Font("Consolas", 10.5F);
            this.txtInput.HideSelection = false;
            this.txtInput.Location = new System.Drawing.Point(0, 0);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(380, 395);
            this.txtInput.TabIndex = 0;
            this.txtInput.Text = "";
            // 
            // tabOutput
            // 
            this.tabOutput.Controls.Add(this.tabText);
            this.tabOutput.Controls.Add(this.tabTree);
            this.tabOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabOutput.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.tabOutput.Location = new System.Drawing.Point(0, 0);
            this.tabOutput.Name = "tabOutput";
            this.tabOutput.SelectedIndex = 0;
            this.tabOutput.Size = new System.Drawing.Size(416, 395);
            this.tabOutput.TabIndex = 0;
            // 
            // tabText
            // 
            this.tabText.Controls.Add(this.txtOutput);
            this.tabText.Location = new System.Drawing.Point(4, 26);
            this.tabText.Name = "tabText";
            this.tabText.Padding = new System.Windows.Forms.Padding(3);
            this.tabText.Size = new System.Drawing.Size(408, 365);
            this.tabText.TabIndex = 0;
            this.tabText.Text = "Text";
            this.tabText.UseVisualStyleBackColor = true;
            // 
            // txtOutput
            // 
            this.txtOutput.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtOutput.DetectUrls = false;
            this.txtOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtOutput.Font = new System.Drawing.Font("Consolas", 10.5F);
            this.txtOutput.HideSelection = false;
            this.txtOutput.Location = new System.Drawing.Point(3, 3);
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.Size = new System.Drawing.Size(402, 359);
            this.txtOutput.TabIndex = 0;
            this.txtOutput.Text = "";
            // 
            // tabTree
            // 
            this.tabTree.Controls.Add(this.treeOutput);
            this.tabTree.Location = new System.Drawing.Point(4, 26);
            this.tabTree.Name = "tabTree";
            this.tabTree.Padding = new System.Windows.Forms.Padding(3);
            this.tabTree.Size = new System.Drawing.Size(408, 373);
            this.tabTree.TabIndex = 1;
            this.tabTree.Text = "Tree";
            this.tabTree.UseVisualStyleBackColor = true;
            // 
            // treeOutput
            // 
            this.treeOutput.BackColor = System.Drawing.Color.WhiteSmoke;
            this.treeOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeOutput.ContextMenuStrip = this.ctxTree;
            this.treeOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeOutput.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.treeOutput.ItemHeight = 22;
            this.treeOutput.Location = new System.Drawing.Point(3, 3);
            this.treeOutput.Name = "treeOutput";
            this.treeOutput.Size = new System.Drawing.Size(402, 367);
            this.treeOutput.TabIndex = 0;
            // 
            // ctxTree
            // 
            this.ctxTree.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxCopyNode,
            this.ctxCopyValue,
            this.ctxCopyPath});
            this.ctxTree.Name = "ctxTree";
            this.ctxTree.Size = new System.Drawing.Size(149, 70);
            // 
            // ctxCopyNode
            // 
            this.ctxCopyNode.Name = "ctxCopyNode";
            this.ctxCopyNode.Size = new System.Drawing.Size(148, 22);
            this.ctxCopyNode.Text = "复制节点文本";
            this.ctxCopyNode.Click += new System.EventHandler(this.ctxCopyNode_Click);
            // 
            // ctxCopyValue
            // 
            this.ctxCopyValue.Name = "ctxCopyValue";
            this.ctxCopyValue.Size = new System.Drawing.Size(148, 22);
            this.ctxCopyValue.Text = "复制节点值";
            this.ctxCopyValue.Click += new System.EventHandler(this.ctxCopyValue_Click);
            // 
            // ctxCopyPath
            // 
            this.ctxCopyPath.Name = "ctxCopyPath";
            this.ctxCopyPath.Size = new System.Drawing.Size(148, 22);
            this.ctxCopyPath.Text = "复制节点路径";
            this.ctxCopyPath.Click += new System.EventHandler(this.ctxCopyPath_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.toolStrip1.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnPretty,
            this.btnMinify,
            this.btnDetect,
            this.sep1,
            this.cboFormat,
            this.sep2,
            this.btnCopy,
            this.btnOpen,
            this.btnSave,
            this.sep3,
            this.btnExpandAll,
            this.btnCollapseAll,
            this.sep4,
            this.txtFind,
            this.btnFindPrev,
            this.btnFindNext,
            this.sep5,
            this.btnWrap,
            this.btnTheme,
            this.sep6,
            this.cboHistory,
            this.sep7,
            this.btnFont});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(8, 2, 2, 2);
            this.toolStrip1.Size = new System.Drawing.Size(800, 33);
            this.toolStrip1.Stretch = true;
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnPretty
            // 
            this.btnPretty.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnPretty.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.btnPretty.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnPretty.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPretty.Name = "btnPretty";
            this.btnPretty.Size = new System.Drawing.Size(57, 26);
            this.btnPretty.Text = "✨ 美化";
            this.btnPretty.Click += new System.EventHandler(this.btnPretty_Click);
            // 
            // btnMinify
            // 
            this.btnMinify.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnMinify.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnMinify.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMinify.Name = "btnMinify";
            this.btnMinify.Size = new System.Drawing.Size(56, 26);
            this.btnMinify.Text = "📦 压缩";
            this.btnMinify.Click += new System.EventHandler(this.btnMinify_Click);
            // 
            // btnDetect
            // 
            this.btnDetect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnDetect.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnDetect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDetect.Name = "btnDetect";
            this.btnDetect.Size = new System.Drawing.Size(80, 26);
            this.btnDetect.Text = "🔍 自动识别";
            this.btnDetect.Click += new System.EventHandler(this.btnDetect_Click);
            // 
            // sep1
            // 
            this.sep1.Name = "sep1";
            this.sep1.Size = new System.Drawing.Size(6, 29);
            // 
            // cboFormat
            // 
            this.cboFormat.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.cboFormat.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.cboFormat.Items.AddRange(new object[] {
            "JSON",
            "XML",
            "Plain"});
            this.cboFormat.Name = "cboFormat";
            this.cboFormat.Size = new System.Drawing.Size(140, 29);
            this.cboFormat.ToolTipText = "选择或输入格式类型";
            // 
            // sep2
            // 
            this.sep2.Name = "sep2";
            this.sep2.Size = new System.Drawing.Size(6, 29);
            // 
            // btnCopy
            // 
            this.btnCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(54, 26);
            this.btnCopy.Text = "📋 复制";
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(56, 26);
            this.btnOpen.Text = "📁 打开";
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnSave
            // 
            this.btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(56, 26);
            this.btnSave.Text = "💾 保存";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // sep3
            // 
            this.sep3.Name = "sep3";
            this.sep3.Size = new System.Drawing.Size(6, 29);
            // 
            // btnExpandAll
            // 
            this.btnExpandAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnExpandAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExpandAll.Name = "btnExpandAll";
            this.btnExpandAll.Size = new System.Drawing.Size(80, 26);
            this.btnExpandAll.Text = "➕ 展开全部";
            this.btnExpandAll.Click += new System.EventHandler(this.btnExpandAll_Click);
            // 
            // btnCollapseAll
            // 
            this.btnCollapseAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnCollapseAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCollapseAll.Name = "btnCollapseAll";
            this.btnCollapseAll.Size = new System.Drawing.Size(80, 26);
            this.btnCollapseAll.Text = "➖ 折叠全部";
            this.btnCollapseAll.Click += new System.EventHandler(this.btnCollapseAll_Click);
            // 
            // sep4
            // 
            this.sep4.Name = "sep4";
            this.sep4.Size = new System.Drawing.Size(6, 29);
            // 
            // txtFind
            // 
            this.txtFind.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.txtFind.Name = "txtFind";
            this.txtFind.Size = new System.Drawing.Size(150, 23);
            this.txtFind.ToolTipText = "🔍 查找... (Enter 下一处)";
            this.txtFind.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtFind_KeyDown);
            // 
            // btnFindPrev
            // 
            this.btnFindPrev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnFindPrev.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFindPrev.Name = "btnFindPrev";
            this.btnFindPrev.Size = new System.Drawing.Size(23, 21);
            this.btnFindPrev.Text = "◄";
            this.btnFindPrev.Click += new System.EventHandler(this.btnFindPrev_Click);
            // 
            // btnFindNext
            // 
            this.btnFindNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnFindNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFindNext.Name = "btnFindNext";
            this.btnFindNext.Size = new System.Drawing.Size(23, 21);
            this.btnFindNext.Text = "►";
            this.btnFindNext.Click += new System.EventHandler(this.btnFindNext_Click);
            // 
            // sep5
            // 
            this.sep5.Name = "sep5";
            this.sep5.Size = new System.Drawing.Size(6, 28);
            // 
            // btnWrap
            // 
            this.btnWrap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnWrap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnWrap.Name = "btnWrap";
            this.btnWrap.Size = new System.Drawing.Size(23, 21);
            this.btnWrap.Text = "↵";
            this.btnWrap.Click += new System.EventHandler(this.btnWrap_Click);
            // 
            // btnTheme
            // 
            this.btnTheme.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnTheme.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnTheme.Name = "btnTheme";
            this.btnTheme.Size = new System.Drawing.Size(80, 21);
            this.btnTheme.Text = "🌙 暗色主题";
            this.btnTheme.Click += new System.EventHandler(this.btnTheme_Click);
            // 
            // sep6
            // 
            this.sep6.Name = "sep6";
            this.sep6.Size = new System.Drawing.Size(6, 28);
            // 
            // cboHistory
            // 
            this.cboHistory.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.cboHistory.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.cboHistory.Name = "cboHistory";
            this.cboHistory.Size = new System.Drawing.Size(180, 25);
            this.cboHistory.ToolTipText = "📜 历史记录";
            this.cboHistory.SelectedIndexChanged += new System.EventHandler(this.cboHistory_SelectedIndexChanged);
            // 
            // sep7
            // 
            this.sep7.Name = "sep7";
            this.sep7.Size = new System.Drawing.Size(6, 29);
            // 
            // btnFont
            // 
            this.btnFont.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnFont.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFont.Name = "btnFont";
            this.btnFont.Size = new System.Drawing.Size(69, 26);
            this.btnFont.Text = "🎨 字体";
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.statusStrip1.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus,
            this.lblStats});
            this.statusStrip1.Location = new System.Drawing.Point(0, 428);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(52, 17);
            this.lblStatus.Text = "✅ 就绪";
            // 
            // lblStats
            // 
            this.lblStats.Name = "lblStats";
            this.lblStats.Size = new System.Drawing.Size(0, 17);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "PrettyText - 文本格式化";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabOutput.ResumeLayout(false);
            this.tabText.ResumeLayout(false);
            this.tabTree.ResumeLayout(false);
            this.ctxTree.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.RichTextBox txtInput;
        private System.Windows.Forms.TabControl tabOutput;
        private System.Windows.Forms.TabPage tabText;
        private System.Windows.Forms.RichTextBox txtOutput;
        private System.Windows.Forms.TabPage tabTree;
        private System.Windows.Forms.TreeView treeOutput;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnPretty;
        private System.Windows.Forms.ToolStripButton btnMinify;
        private System.Windows.Forms.ToolStripButton btnDetect;
        private System.Windows.Forms.ToolStripSeparator sep1;
        private System.Windows.Forms.ToolStripComboBox cboFormat;
        private System.Windows.Forms.ToolStripSeparator sep2;
        private System.Windows.Forms.ToolStripButton btnCopy;
        private System.Windows.Forms.ToolStripButton btnOpen;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ToolStripSeparator sep3;
        private System.Windows.Forms.ToolStripButton btnExpandAll;
        private System.Windows.Forms.ToolStripButton btnCollapseAll;
        private System.Windows.Forms.ToolStripSeparator sep4;
        private System.Windows.Forms.ToolStripTextBox txtFind;
        private System.Windows.Forms.ToolStripButton btnFindPrev;
        private System.Windows.Forms.ToolStripButton btnFindNext;
        private System.Windows.Forms.ToolStripSeparator sep5;
        private System.Windows.Forms.ToolStripButton btnWrap;
        private System.Windows.Forms.ToolStripButton btnTheme;
        private System.Windows.Forms.ToolStripSeparator sep6;
        private System.Windows.Forms.ToolStripComboBox cboHistory;
        private System.Windows.Forms.ToolStripSeparator sep7;
        private System.Windows.Forms.ToolStripDropDownButton btnFont;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblStats;
        private System.Windows.Forms.ContextMenuStrip ctxTree;
        private System.Windows.Forms.ToolStripMenuItem ctxCopyNode;
        private System.Windows.Forms.ToolStripMenuItem ctxCopyValue;
        private System.Windows.Forms.ToolStripMenuItem ctxCopyPath;
    }
}