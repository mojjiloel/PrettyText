﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PrettyText.TextFormatters;

namespace PrettyText
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeUiLogic();
        }

        private void InitializeUiLogic()
        {
            // 填充格式下拉（来自注册器）
            cboFormat.Items.Clear();
            foreach (var f in FormatterRegistry.GetAll()) cboFormat.Items.Add(f.Name);
            cboFormat.Text = "JSON";

            // 应用现代化样式
            toolStrip1.Renderer = new ToolStripProfessionalRenderer(new ModernColorTable());

            // 启用拖拽文件导入
            this.AllowDrop = true;
            this.DragEnter += Form1_DragEnter;
            this.DragDrop += Form1_DragDrop;

            // 历史记录
            _history = new List<string>();
            LoadHistory();

            // 快捷键
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

            // 文本变化统计
            this.txtInput.TextChanged += (s, e) => UpdateStats();
            this.txtOutput.TextChanged += (s, e) => UpdateStats();

            // 读取配置
            LoadConfig();
            UpdateStats();

            // 初始化字体设置菜单
            InitializeFontMenu();
        }

        private void btnPretty_Click(object sender, EventArgs e)
        {
            RunFormat(pretty: true);
        }

        private void btnMinify_Click(object sender, EventArgs e)
        {
            RunFormat(pretty: false);
        }

        private void btnDetect_Click(object sender, EventArgs e)
        {
            try
            {
                var formatter = FormatterRegistry.Resolve(txtInput.Text);
                cboFormat.Text = formatter.Name;
                lblStatus.Text = "✅ 识别为 " + formatter.Name;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ 识别失败: " + ex.Message;
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (tabOutput.SelectedTab == tabText)
            {
                var sel = txtOutput.SelectedText;
                var textToCopy = !string.IsNullOrEmpty(sel) ? sel : (txtOutput.Text ?? string.Empty);
                if (string.IsNullOrEmpty(textToCopy)) { lblStatus.Text = "⚠️ 无可复制内容"; return; }
                Clipboard.SetText(textToCopy);
                lblStatus.Text = "✅ 已复制 Text 输出" + (string.IsNullOrEmpty(sel) ? "" : "(选中部分)");
            }
            else
            {
                if (treeOutput.SelectedNode != null)
                {
                    var tagStr = Convert.ToString(treeOutput.SelectedNode.Tag);
                    var nodeText = treeOutput.SelectedNode.Text ?? string.Empty;
                    var textToCopy = string.IsNullOrEmpty(tagStr) ? nodeText : tagStr;
                    if (string.IsNullOrEmpty(textToCopy)) { lblStatus.Text = "⚠️ 无可复制内容"; return; }
                    Clipboard.SetText(textToCopy);
                    lblStatus.Text = "✅ 已复制选中节点内容";
                }
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "All|*.*|Text|*.txt;*.log;*.md;*.cfg|JSON|*.json|XML|*.xml|YAML|*.yml;*.yaml|CSV|*.csv|HTML|*.html;*.htm";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var text = System.IO.File.ReadAllText(ofd.FileName, Encoding.UTF8);
                        txtInput.Text = text;
                        lblStatus.Text = "✅ 已打开文件: " + System.IO.Path.GetFileName(ofd.FileName);
                    }
                    catch (Exception ex)
                    {
                        lblStatus.Text = "❌ 打开失败: " + ex.Message;
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Text|*.txt;*.json;*.xml;*.yaml;*.yml;*.csv;*.html|All|*.*";
                sfd.FileName = "output.txt";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.WriteAllText(sfd.FileName, txtOutput.Text ?? string.Empty, Encoding.UTF8);
                    lblStatus.Text = "✅ 已保存到: " + sfd.FileName;
                }
            }
        }

        private void RunFormat(bool pretty)
        {
            try
            {
                var input = txtInput.Text ?? string.Empty;
                AppendHistory(input);
                var selected = (cboFormat.Text ?? "").Trim();
                ITextFormatter formatter = FormatterRegistry.GetAll().FirstOrDefault(f => string.Equals(f.Name, selected, StringComparison.OrdinalIgnoreCase));
                if (formatter == null)
                {
                    formatter = FormatterRegistry.Resolve(input);
                    cboFormat.Text = formatter.Name;
                }

                string output;
                try
                {
                    output = pretty ? formatter.FormatPretty(input) : formatter.FormatMinified(input);
                }
                catch
                {
                    // 尝试自动降级:去除外层引号与转义后再试
                    var fallback = SafeUnescape(input);
                    output = pretty ? formatter.FormatPretty(fallback) : formatter.FormatMinified(fallback);
                }
                txtOutput.Text = output;
                BuildTree(formatter, output);
                lblStatus.Text = (pretty ? "✨ 已美化: " : "📦 已压缩: ") + formatter.Name;
                UpdateStats();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ 处理失败: " + ex.Message;
            }
        }

        private static string SafeUnescape(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            var t = text.Trim();
            if ((t.StartsWith("\"") && t.EndsWith("\"")) || (t.StartsWith("'") && t.EndsWith("'")))
            {
                t = t.Substring(1, t.Length - 2);
            }
            t = t.Replace("\\\"", "\"");
            t = t.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t");
            return t;
        }

        private void BuildTree(ITextFormatter formatter, string text)
        {
            treeOutput.BeginUpdate();
            treeOutput.Nodes.Clear();
            try
            {
                if (string.Equals(formatter.Name, "JSON", StringComparison.OrdinalIgnoreCase))
                {
                    BuildTreeFromJson(text);
                }
                else if (string.Equals(formatter.Name, "XML", StringComparison.OrdinalIgnoreCase))
                {
                    BuildTreeFromXml(text);
                }
                else if (string.Equals(formatter.Name, "YAML", StringComparison.OrdinalIgnoreCase))
                {
                    BuildTreeFromYaml(text);
                }
                else if (string.Equals(formatter.Name, "CSV", StringComparison.OrdinalIgnoreCase))
                {
                    BuildTreeFromCsv(text);
                }
                else if (string.Equals(formatter.Name, "HTML", StringComparison.OrdinalIgnoreCase))
                {
                    BuildTreeFromHtml(text);
                }
                else
                {
                    var node = new TreeNode("Text") { Tag = text };
                    treeOutput.Nodes.Add(node);
                }
            }
            finally
            {
                treeOutput.EndUpdate();
            }
        }

        private void BuildTreeFromJson(string json)
        {
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var obj = serializer.DeserializeObject(json);
            var root = new TreeNode("JSON");
            BuildJsonNode(root, obj);
            treeOutput.Nodes.Add(root);
            root.Expand();
            UpdateStats();
        }

        private void BuildJsonNode(TreeNode parent, object value)
        {
            if (value is System.Collections.IDictionary dict)
            {
                foreach (System.Collections.DictionaryEntry kv in dict)
                {
                    var child = new TreeNode(Convert.ToString(kv.Key));
                    BuildJsonNode(child, kv.Value);
                    parent.Nodes.Add(child);
                }
                parent.Tag = serializerSafeToString(value);
            }
            else if (value is System.Collections.IEnumerable list && !(value is string))
            {
                int index = 0;
                foreach (var item in list)
                {
                    var child = new TreeNode("[" + index + "]");
                    BuildJsonNode(child, item);
                    parent.Nodes.Add(child);
                    index++;
                }
                parent.Tag = serializerSafeToString(value);
            }
            else
            {
                var text = serializerSafeToString(value);
                parent.Nodes.Add(new TreeNode(text) { Tag = text });
                parent.Tag = text;
            }
        }

        private static string serializerSafeToString(object value)
        {
            if (value == null) return "null";
            if (value is string s) return s;
            return Convert.ToString(value);
        }

        private void BuildTreeFromXml(string xmlText)
        {
            var doc = new System.Xml.XmlDocument();
            doc.XmlResolver = null;
            doc.LoadXml(xmlText);
            var root = new TreeNode(doc.DocumentElement.Name) { Tag = doc.DocumentElement.OuterXml };
            BuildXmlNode(root, doc.DocumentElement);
            treeOutput.Nodes.Add(root);
            root.Expand();
            UpdateStats();
        }

        private void BuildXmlNode(TreeNode parent, System.Xml.XmlNode node)
        {
            if (node.Attributes != null)
            {
                foreach (System.Xml.XmlAttribute attr in node.Attributes)
                {
                    var attrNode = new TreeNode("@" + attr.Name + "=" + attr.Value) { Tag = attr.Value };
                    parent.Nodes.Add(attrNode);
                }
            }
            foreach (System.Xml.XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == System.Xml.XmlNodeType.Element)
                {
                    var childNode = new TreeNode(child.Name) { Tag = child.OuterXml };
                    BuildXmlNode(childNode, child);
                    parent.Nodes.Add(childNode);
                }
                else if (child.NodeType == System.Xml.XmlNodeType.Text || child.NodeType == System.Xml.XmlNodeType.CDATA)
                {
                    var text = child.InnerText;
                    parent.Nodes.Add(new TreeNode(text) { Tag = text });
                }
            }
        }

        private void BuildTreeFromYaml(string yaml)
        {
            var root = new TreeNode("YAML") { Tag = yaml };
            foreach (var line in yaml.Replace("\r\n", "\n").Split('\n'))
            {
                if (line.Length == 0) continue;
                root.Nodes.Add(new TreeNode(line.TrimEnd()) { Tag = line.Trim() });
            }
            treeOutput.Nodes.Add(root);
            root.Expand();
            UpdateStats();
        }

        private void BuildTreeFromCsv(string csv)
        {
            var lines = csv.Replace("\r\n", "\n").Split('\n');
            var root = new TreeNode("CSV");
            for (int i = 0; i < lines.Length; i++)
            {
                var row = lines[i];
                if (row.Length == 0) continue;
                var rowNode = new TreeNode("Row " + i.ToString());
                foreach (var cell in row.Split(','))
                {
                    var cellText = cell.Trim();
                    rowNode.Nodes.Add(new TreeNode(cellText) { Tag = cellText });
                }
                root.Nodes.Add(rowNode);
            }
            treeOutput.Nodes.Add(root);
            root.Expand();
            UpdateStats();
        }

        private void BuildTreeFromHtml(string html)
        {
            var root = new TreeNode("HTML") { Tag = html };
            foreach (var token in html.Replace("<", "\n<").Split('\n'))
            {
                var t = token.Trim();
                if (t.StartsWith("<") && t.Length > 1)
                {
                    root.Nodes.Add(new TreeNode(t.Length > 60 ? t.Substring(0, 60) + "..." : t) { Tag = t });
                }
            }
            treeOutput.Nodes.Add(root);
            root.Expand();
            UpdateStats();
        }

        private void ctxCopyNode_Click(object sender, EventArgs e)
        {
            if (treeOutput.SelectedNode == null) return;
            Clipboard.SetText(treeOutput.SelectedNode.Text);
            lblStatus.Text = "✅ 已复制节点文本";
        }

        private void ctxCopyValue_Click(object sender, EventArgs e)
        {
            if (treeOutput.SelectedNode == null) return;
            var value = Convert.ToString(treeOutput.SelectedNode.Tag) ?? string.Empty;
            Clipboard.SetText(value);
            lblStatus.Text = "✅ 已复制节点值";
        }

        private void ctxCopyPath_Click(object sender, EventArgs e)
        {
            if (treeOutput.SelectedNode == null) return;
            var path = BuildNodePath(treeOutput.SelectedNode);
            Clipboard.SetText(path);
            lblStatus.Text = "✅ 已复制节点路径";
        }

        private static string BuildNodePath(TreeNode node)
        {
            var stack = new System.Collections.Generic.Stack<string>();
            var cur = node;
            while (cur != null)
            {
                stack.Push(cur.Text);
                cur = cur.Parent;
            }
            return string.Join("/", stack.ToArray());
        }

        private void UpdateStats()
        {
            var input = txtInput.Text ?? string.Empty;
            var output = txtOutput.Text ?? string.Empty;
            int inLines = input.Length == 0 ? 0 : input.Replace("\r\n", "\n").Split('\n').Length;
            int outLines = output.Length == 0 ? 0 : output.Replace("\r\n", "\n").Split('\n').Length;
            lblStats.Text = "📄 输入 行:" + inLines.ToString() + " 字符:" + input.Length.ToString() +
                            "   |   📝 输出 行:" + outLines.ToString() + " 字符:" + output.Length.ToString();
        }

        // 快捷键：Ctrl+O 打开，Ctrl+S 保存输出，Ctrl+F 查找框聚焦，Ctrl+E 美化，Ctrl+M 压缩，F5 自动识别+美化
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.O) { btnOpen_Click(sender, EventArgs.Empty); e.Handled = true; return; }
            if (e.Control && e.KeyCode == Keys.S) { btnSave_Click(sender, EventArgs.Empty); e.Handled = true; return; }
            if (e.Control && e.KeyCode == Keys.F) { txtFind.Focus(); e.Handled = true; return; }
            if (e.Control && e.KeyCode == Keys.E) { btnPretty_Click(sender, EventArgs.Empty); e.Handled = true; return; }
            if (e.Control && e.KeyCode == Keys.M) { btnMinify_Click(sender, EventArgs.Empty); e.Handled = true; return; }
            if (e.KeyCode == Keys.F5) { btnDetect_Click(sender, EventArgs.Empty); btnPretty_Click(sender, EventArgs.Empty); e.Handled = true; return; }
        }

        // 配置持久化（窗口、分割条、主题、换行）
        private string ConfigPath
        {
            get
            {
                var dir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PrettyText");
                if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                return System.IO.Path.Combine(dir, "config.ini");
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UpdateStats();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveConfig();
            base.OnFormClosing(e);
        }

        private void LoadConfig()
        {
            try
            {
                if (!System.IO.File.Exists(ConfigPath)) return;
                var dict = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var line in System.IO.File.ReadAllLines(ConfigPath))
                {
                    var idx = line.IndexOf('=');
                    if (idx > 0)
                    {
                        var k = line.Substring(0, idx).Trim();
                        var v = line.Substring(idx + 1).Trim();
                        dict[k] = v;
                    }
                }
                int w, h, x, y, split;
                if (int.TryParse(GetOrDefault(dict, "Width", this.Width.ToString()), out w)) this.Width = w;
                if (int.TryParse(GetOrDefault(dict, "Height", this.Height.ToString()), out h)) this.Height = h;
                if (int.TryParse(GetOrDefault(dict, "X", this.Left.ToString()), out x)) this.Left = x;
                if (int.TryParse(GetOrDefault(dict, "Y", this.Top.ToString()), out y)) this.Top = y;
                if (int.TryParse(GetOrDefault(dict, "Splitter", this.splitContainer1.SplitterDistance.ToString()), out split)) this.splitContainer1.SplitterDistance = split;
                bool dark; if (bool.TryParse(GetOrDefault(dict, "Dark", "false"), out dark)) { _dark = dark; ApplyTheme(_dark); }
                bool wrap; if (bool.TryParse(GetOrDefault(dict, "Wrap", "false"), out wrap)) { txtOutput.WordWrap = wrap; }

                // 加载字体设置
                float fontSize;
                if (float.TryParse(GetOrDefault(dict, "FontSize", "10.5"), out fontSize)) _customFontSize = fontSize;
                
                var fontColorStr = GetOrDefault(dict, "FontColor", "40,40,40");
                var colorParts = fontColorStr.Split(',');
                if (colorParts.Length == 3)
                {
                    int r, g, b;
                    if (int.TryParse(colorParts[0], out r) && int.TryParse(colorParts[1], out g) && int.TryParse(colorParts[2], out b))
                    {
                        _customFontColor = Color.FromArgb(r, g, b);
                    }
                }

                _customFontFamily = GetOrDefault(dict, "FontFamily", "Consolas");

                // 应用字体设置
                ApplyFontSettings(_customFontSize, _customFontColor, _customFontFamily);
            }
            catch { }
        }

        private string GetOrDefault(System.Collections.Generic.Dictionary<string, string> dict, string key, string defVal)
        {
            string v; return dict.TryGetValue(key, out v) ? v : defVal;
        }

        private void SaveConfig()
        {
            try
            {
                var lines = new List<string>();
                lines.Add("Width=" + this.Width);
                lines.Add("Height=" + this.Height);
                lines.Add("X=" + this.Left);
                lines.Add("Y=" + this.Top);
                lines.Add("Splitter=" + this.splitContainer1.SplitterDistance);
                lines.Add("Dark=" + _dark);
                lines.Add("Wrap=" + txtOutput.WordWrap);

                // 保存字体设置
                lines.Add("FontSize=" + _customFontSize);
                lines.Add("FontColor=" + _customFontColor.R + "," + _customFontColor.G + "," + _customFontColor.B);
                lines.Add("FontFamily=" + _customFontFamily);

                System.IO.File.WriteAllLines(ConfigPath, lines.ToArray());
            }
            catch { }
        }

        private void btnExpandAll_Click(object sender, EventArgs e)
        {
            treeOutput.BeginUpdate();
            treeOutput.ExpandAll();
            treeOutput.EndUpdate();
        }

        private void btnCollapseAll_Click(object sender, EventArgs e)
        {
            treeOutput.BeginUpdate();
            treeOutput.CollapseAll();
            treeOutput.EndUpdate();
        }

        private string _lastFind;
        private TreeNode _findCursor;
        private void txtFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _lastFind = txtFind.Text;
                FindNext();
                e.Handled = true;
            }
        }

        private void btnFindNext_Click(object sender, EventArgs e)
        {
            _lastFind = txtFind.Text;
            FindNext();
        }

        private void btnFindPrev_Click(object sender, EventArgs e)
        {
            _lastFind = txtFind.Text;
            FindPrev();
        }

        private void FindNext()
        {
            if (string.IsNullOrEmpty(_lastFind)) return;
            var start = _findCursor ?? (treeOutput.Nodes.Count > 0 ? treeOutput.Nodes[0] : null);
            var node = NextNode(start);
            while (node != null)
            {
                if ((node.Text != null && node.Text.IndexOf(_lastFind, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (Convert.ToString(node.Tag) ?? string.Empty).IndexOf(_lastFind, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    treeOutput.SelectedNode = node;
                    node.EnsureVisible();
                    _findCursor = node;
                    return;
                }
                node = NextNode(node);
            }
            lblStatus.Text = "❌ 未找到";
        }

        private void FindPrev()
        {
            if (string.IsNullOrEmpty(_lastFind)) return;
            var start = _findCursor ?? (treeOutput.Nodes.Count > 0 ? treeOutput.Nodes[0] : null);
            var node = PrevNode(start);
            while (node != null)
            {
                if ((node.Text != null && node.Text.IndexOf(_lastFind, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (Convert.ToString(node.Tag) ?? string.Empty).IndexOf(_lastFind, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    treeOutput.SelectedNode = node;
                    node.EnsureVisible();
                    _findCursor = node;
                    return;
                }
                node = PrevNode(node);
            }
            lblStatus.Text = "❌ 未找到";
        }

        private static TreeNode NextNode(TreeNode node)
        {
            if (node == null) return null;
            if (node.FirstNode != null) return node.FirstNode;
            while (node != null && node.NextNode == null) node = node.Parent;
            return node == null ? null : node.NextNode;
        }

        private static TreeNode PrevNode(TreeNode node)
        {
            if (node == null) return null;
            if (node.PrevNode != null)
            {
                node = node.PrevNode;
                while (node.LastNode != null) node = node.LastNode;
                return node;
            }
            return node.Parent;
        }

        private void btnWrap_Click(object sender, EventArgs e)
        {
            txtOutput.WordWrap = !txtOutput.WordWrap;
            lblStatus.Text = txtOutput.WordWrap ? "✅ 已开启换行" : "❌ 已关闭换行";
        }

        // 拖拽导入
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    var path = files[0];
                    var text = System.IO.File.ReadAllText(path);
                    txtInput.Text = text;
                    lblStatus.Text = "✅ 已加载文件: " + System.IO.Path.GetFileName(path);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ 拖拽载入失败: " + ex.Message;
            }
        }

        // 暗色主题
        private bool _dark;
        private void btnTheme_Click(object sender, EventArgs e)
        {
            _dark = !_dark;
            ApplyTheme(_dark);
        }

        private void ApplyTheme(bool dark)
        {
            var back = dark ? System.Drawing.Color.FromArgb(32, 32, 32) : System.Drawing.Color.White;
            var fore = dark ? System.Drawing.Color.Gainsboro : System.Drawing.Color.FromArgb(40, 40, 40);
            var backAlt = dark ? System.Drawing.Color.FromArgb(28, 28, 28) : System.Drawing.Color.WhiteSmoke;
            var toolbarBack = dark ? System.Drawing.Color.FromArgb(45, 45, 45) : System.Drawing.Color.FromArgb(250, 250, 250);
            var accentColor = dark ? System.Drawing.Color.FromArgb(0, 150, 255) : System.Drawing.Color.FromArgb(0, 122, 204);
            
            this.BackColor = dark ? System.Drawing.Color.FromArgb(24, 24, 24) : System.Drawing.Color.FromArgb(240, 240, 240);
            
            // 文本框样式
            txtInput.BackColor = back; 
            txtInput.ForeColor = fore;
            txtOutput.BackColor = backAlt; 
            txtOutput.ForeColor = fore;
            
            // Tree视图样式
            treeOutput.BackColor = backAlt; 
            treeOutput.ForeColor = fore;
            treeOutput.LineColor = dark ? System.Drawing.Color.FromArgb(60, 60, 60) : System.Drawing.Color.FromArgb(200, 200, 200);
            
            // Tab样式
            tabOutput.BackColor = dark ? System.Drawing.Color.FromArgb(32, 32, 32) : System.Drawing.SystemColors.Control;
            tabText.BackColor = backAlt;
            tabTree.BackColor = backAlt;
            
            // 工具栏样式
            toolStrip1.BackColor = toolbarBack;
            toolStrip1.ForeColor = fore;
            
            // 状态栏样式
            statusStrip1.BackColor = toolbarBack;
            statusStrip1.ForeColor = fore;
            lblStatus.ForeColor = dark ? System.Drawing.Color.FromArgb(200, 200, 200) : System.Drawing.Color.FromArgb(60, 60, 60);
            lblStats.ForeColor = dark ? System.Drawing.Color.FromArgb(180, 180, 180) : System.Drawing.Color.FromArgb(80, 80, 80);
            
            // 更新按钮文字和颜色
            btnPretty.ForeColor = accentColor;
            btnMinify.ForeColor = accentColor;
            btnDetect.ForeColor = accentColor;
            
            // 主题切换按钮
            btnTheme.Text = dark ? "☀️ 亮色主题" : "🌙 暗色主题";
        }

        // 历史记录
        private List<string> _history;
        private const int MaxHistory = 20;
        private string HistoryPath
        {
            get
            {
                var dir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PrettyText");
                if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                return System.IO.Path.Combine(dir, "history.txt");
            }
        }

        private void AppendHistory(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            
            // 去重：如果已存在相同内容，先移除旧的
            var existingIndex = _history.FindIndex(h => string.Equals(h, text, StringComparison.Ordinal));
            if (existingIndex >= 0)
            {
                _history.RemoveAt(existingIndex);
            }
            
            // 插入到最前面
            _history.Insert(0, text);
            if (_history.Count > MaxHistory) _history.RemoveAt(_history.Count - 1);
            RefreshHistoryCombo();
            try 
            { 
                // 使用特殊分隔符保存历史，避免换行符冲突
                var lines = new List<string>();
                foreach (var item in _history)
                {
                    // 使用 Base64 编码保存，确保多行文本不被破坏
                    var bytes = Encoding.UTF8.GetBytes(item);
                    var encoded = Convert.ToBase64String(bytes);
                    lines.Add(encoded);
                }
                System.IO.File.WriteAllLines(HistoryPath, lines.ToArray());
            }
            catch (Exception)
            {
                // 忽略历史保存失败（权限/磁盘问题），不影响主流程
            }
        }

        private void LoadHistory()
        {
            try
            {
                if (System.IO.File.Exists(HistoryPath))
                {
                    var lines = System.IO.File.ReadAllLines(HistoryPath);
                    _history = new List<string>();
                    foreach (var line in lines)
                    {
                        try
                        {
                            // 从 Base64 解码
                            var bytes = Convert.FromBase64String(line);
                            var decoded = Encoding.UTF8.GetString(bytes);
                            _history.Add(decoded);
                        }
                        catch
                        {
                            // 如果解码失败（可能是旧格式），直接使用原文本
                            _history.Add(line);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 忽略历史加载失败，不影响主流程
            }
            RefreshHistoryCombo();
        }

        private void RefreshHistoryCombo()
        {
            cboHistory.Items.Clear();
            int i = 0;
            foreach (var h in _history)
            {
                var preview = h.Replace("\r\n", " ").Replace("\n", " ");
                if (preview.Length > 60) preview = preview.Substring(0, 60) + "...";
                cboHistory.Items.Add("#" + i.ToString() + " " + preview);
                i++;
            }
        }

        private void cboHistory_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idx = cboHistory.SelectedIndex;
            if (idx >= 0 && idx < _history.Count)
            {
                txtInput.Text = _history[idx];
                lblStatus.Text = "✅ 已从历史载入";
            }
        }

        // 字体设置功能
        private class FontPreset
        {
            public string Name { get; set; }
            public float Size { get; set; }
            public Color Color { get; set; }
            public string FontFamily { get; set; }

            public FontPreset(string name, float size, Color color, string fontFamily = "Consolas")
            {
                Name = name;
                Size = size;
                Color = color;
                FontFamily = fontFamily;
            }
        }

        private List<FontPreset> _fontPresets = new List<FontPreset>
        {
            new FontPreset("默认 (10.5pt)", 10.5f, Color.FromArgb(40, 40, 40)),
            new FontPreset("小号 (9pt)", 9f, Color.FromArgb(40, 40, 40)),
            new FontPreset("大号 (12pt)", 12f, Color.FromArgb(40, 40, 40)),
            new FontPreset("超大 (14pt)", 14f, Color.FromArgb(40, 40, 40)),
            new FontPreset("护眼绿 (10.5pt)", 10.5f, Color.FromArgb(0, 100, 0)),
            new FontPreset("深蓝 (11pt)", 11f, Color.FromArgb(0, 51, 102)),
            new FontPreset("经典黑 (10pt)", 10f, Color.Black),
        };

        private void InitializeFontMenu()
        {
            btnFont.DropDownItems.Clear();

            // 添加预设配置
            foreach (var preset in _fontPresets)
            {
                var item = new ToolStripMenuItem(preset.Name);
                item.Tag = preset;
                item.Click += FontPreset_Click;
                btnFont.DropDownItems.Add(item);
            }

            // 添加分隔符
            btnFont.DropDownItems.Add(new ToolStripSeparator());

            // 添加自定义字体选项
            var customItem = new ToolStripMenuItem("⚙️ 自定义...");
            customItem.Click += CustomFont_Click;
            btnFont.DropDownItems.Add(customItem);
        }

        private void FontPreset_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item == null) return;

            var preset = item.Tag as FontPreset;
            if (preset != null)
            {
                ApplyFontSettings(preset.Size, preset.Color, preset.FontFamily);
                lblStatus.Text = "✅ 已应用字体预设: " + preset.Name;
            }
        }

        private void CustomFont_Click(object sender, EventArgs e)
        {
            using (var dlg = new FontDialog())
            {
                dlg.Font = txtInput.Font;
                dlg.ShowColor = true;
                dlg.Color = txtInput.ForeColor;
                dlg.FontMustExist = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    ApplyFontSettings(dlg.Font.Size, dlg.Color, dlg.Font.FontFamily.Name);
                    lblStatus.Text = "✅ 已应用自定义字体";
                }
            }
        }

        private void ApplyFontSettings(float size, Color color, string fontFamily)
        {
            try
            {
                var font = new Font(fontFamily, size);
                txtInput.Font = font;
                txtOutput.Font = font;
                txtInput.ForeColor = color;
                txtOutput.ForeColor = color;

                // 如果是暗色主题，需要确保颜色可见
                if (_dark)
                {
                    // 暗色主题下，如果选择的颜色太暗，自动调亮
                    var brightness = (color.R + color.G + color.B) / 3;
                    if (brightness < 80)
                    {
                        txtInput.ForeColor = Color.Gainsboro;
                        txtOutput.ForeColor = Color.Gainsboro;
                    }
                }

                // 保存字体设置
                _customFontSize = size;
                _customFontColor = color;
                _customFontFamily = fontFamily;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ 字体设置失败: " + ex.Message;
            }
        }

        // 字体设置的持久化变量
        private float _customFontSize = 10.5f;
        private Color _customFontColor = Color.FromArgb(40, 40, 40);
        private string _customFontFamily = "Consolas";
    }


    // 现代化工具栏配色方案
    public class ModernColorTable : ProfessionalColorTable
    {
        public override Color ToolStripGradientBegin => Color.FromArgb(250, 250, 250);
        public override Color ToolStripGradientMiddle => Color.FromArgb(250, 250, 250);
        public override Color ToolStripGradientEnd => Color.FromArgb(250, 250, 250);
        public override Color ButtonSelectedGradientBegin => Color.FromArgb(0, 122, 204, 30);
        public override Color ButtonSelectedGradientMiddle => Color.FromArgb(0, 122, 204, 40);
        public override Color ButtonSelectedGradientEnd => Color.FromArgb(0, 122, 204, 30);
        public override Color ButtonPressedGradientBegin => Color.FromArgb(0, 122, 204, 50);
        public override Color ButtonPressedGradientMiddle => Color.FromArgb(0, 122, 204, 60);
        public override Color ButtonPressedGradientEnd => Color.FromArgb(0, 122, 204, 50);
        public override Color ButtonSelectedBorder => Color.FromArgb(0, 122, 204);
        public override Color ButtonPressedBorder => Color.FromArgb(0, 122, 204);
        public override Color SeparatorDark => Color.FromArgb(200, 200, 200);
        public override Color SeparatorLight => Color.FromArgb(230, 230, 230);
    }
}
