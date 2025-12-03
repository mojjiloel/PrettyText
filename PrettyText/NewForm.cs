using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions; // 添加正则表达式支持
using System.Windows.Forms;
using PrettyText.TextFormatters;
using PrettyText.Utils;

namespace PrettyText
{
    public partial class NewForm : AntdUI.Window
    {
        private List<string> _history;
        private const int MaxHistory = 20;
        private bool _dark;
        private float _customFontSize = 10.5f;
        private Color _customFontColor = Color.FromArgb(40, 40, 40);
        private string _customFontFamily = "Consolas";
        private bool isLight = true;
        public NewForm()
        {
            InitializeComponent();
            InitializeUiLogic();
        }

        private void InitializeUiLogic()
        {            
            //根据系统亮暗初始化一次
            isLight = ThemeHelper.IsLightMode();
            button_color.Toggle = !isLight;
            ThemeHelper.SetColorMode(this, isLight);
            
            // 初始化状态栏颜色
            UpdateStatusBarColors();

            // 填充格式下拉（来自注册器）
            var formats = FormatterRegistry.GetAll().Select(f => f.Name).ToList();
            cboFormat.Items.Clear();
            foreach (var format in formats)
            {
                cboFormat.Items.Add(format);
            }
            if (formats.Count > 0)
                cboFormat.Text = formats[0];

            // 历史记录
            _history = new List<string>();
            LoadHistory();

            // 初始化字体设置
            ApplyFontSettings(_customFontSize, _customFontColor, _customFontFamily);
            
            // 应用初始语法高亮
            ApplySyntaxHighlighting(cboFormat.Text ?? "", txtOutput.Text ?? "");

            // 更新统计信息
            UpdateStats();

            // 绑定事件
            txtInput.TextChanged += (s, e) => UpdateStats();
            txtOutput.TextChanged += (s, e) => UpdateStats();
            
            // 绑定树控件右键菜单事件
            treeOutput.MouseDown += TreeOutput_MouseDown;
            
            // 绑定按钮事件
            btnPretty.Click += (s, e) => RunFormat(pretty: true);
            btnMinify.Click += (s, e) => RunFormat(pretty: false);
            btnDetect.Click += btnDetect_Click;
            btnCopy.Click += btnCopy_Click;
            btnOpen.Click += btnOpen_Click;
            btnSave.Click += btnSave_Click;
            btnExpandAll.Click += btnExpandAll_Click;
            btnCollapseAll.Click += btnCollapseAll_Click;
            btnFindPrev.Click += btnFindPrev_Click;
            btnFindNext.Click += btnFindNext_Click;
            btnWrap.Click += btnWrap_Click;
            cboHistory.SelectedIndexChanged += cboHistory_SelectedIndexChanged;
            btnFont.Click += btnFont_Click;

            button_color.Click += Button_color_Click;

            BindButtonWithToolTip(panelToolbar);
        }

        private void Button_color_Click(object sender, EventArgs e)
        {
            isLight = !isLight;
            //这里使用了Toggle属性切换图标
            button_color.Toggle = !isLight;
            ThemeHelper.SetColorMode(this, isLight);
            UpdateStatusBarColors();
            
            // 重新应用语法高亮
            ReapplySyntaxHighlighting();
        }
        
        /// <summary>
        /// 重新应用语法高亮（主题切换时调用）
        /// </summary>
        private void ReapplySyntaxHighlighting()
        {
            var format = cboFormat.Text ?? "";
            ApplySyntaxHighlighting(format, txtOutput.Text ?? "");
        }
        
        /// <summary>
        /// 更新状态栏颜色以匹配当前主题
        /// </summary>
        private void UpdateStatusBarColors()
        {
            if (isLight)
            {
                // 浅色主题
                statusPanel.Back = Color.White;
                lblStatus.ForeColor = Color.Black;
                lblStats.ForeColor = Color.Black;
            }
            else
            {
                // 深色主题
                statusPanel.Back = Color.FromArgb(31, 31, 31);
                lblStatus.ForeColor = Color.White;
                lblStats.ForeColor = Color.White;
            }
        }

        private void btnDetect_Click(object sender, EventArgs e)
        {
            try
            {
                var formatter = FormatterRegistry.Resolve(txtInput.Text);
                cboFormat.Text = formatter.Name;
                lblStatus.Text = "✅ 识别为 " + formatter.Name;

                // 自动触发美化按钮
                RunFormat(pretty: true);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ 识别失败: " + ex.Message;
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            // 复制选中的文本或全部文本
            var selectedText = txtOutput.SelectedText;
            var textToCopy = !string.IsNullOrEmpty(selectedText) ? selectedText : txtOutput.Text;
            
            if (string.IsNullOrEmpty(textToCopy))
            {
                lblStatus.Text = "⚠️ 无可复制内容";
                return;
            }
            
            Clipboard.SetText(textToCopy);
            lblStatus.Text = "✅ 已复制文本";
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
                        AppendHistory(text);
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
                    try
                    {
                        System.IO.File.WriteAllText(sfd.FileName, txtOutput.Text ?? string.Empty, Encoding.UTF8);
                        lblStatus.Text = "✅ 已保存到: " + sfd.FileName;
                    }
                    catch (Exception ex)
                    {
                        lblStatus.Text = "❌ 保存失败: " + ex.Message;
                    }
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
                ApplySyntaxHighlighting(formatter.Name, output); // 添加语法高亮
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
            // 清空现有节点
            treeOutput.Items.Clear();
            
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
                else
                {
                    // 对于其他格式，创建一个简单的节点
                    var node = new AntdUI.TreeItem(formatter.Name);
                    node.Tag = text;
                    treeOutput.Items.Add(node);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ 构建树失败: " + ex.Message;
            }
        }

        private void BuildTreeFromJson(string json)
        {
            try
            {
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var obj = serializer.DeserializeObject(json);
                var root = new AntdUI.TreeItem("JSON");
                BuildJsonNode(root, obj);
                treeOutput.Items.Add(root);
                root.Expand = true;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ JSON解析失败: " + ex.Message;
            }
        }

        private void BuildJsonNode(AntdUI.TreeItem parent, object value)
        {
            if (value is System.Collections.IDictionary dict)
            {
                foreach (System.Collections.DictionaryEntry kv in dict)
                {
                    var child = new AntdUI.TreeItem(Convert.ToString(kv.Key));
                    BuildJsonNode(child, kv.Value);
                    parent.Sub.Add(child);
                }
                parent.Tag = serializerSafeToString(value);
            }
            else if (value is System.Collections.IEnumerable list && !(value is string))
            {
                int index = 0;
                foreach (var item in list)
                {
                    var child = new AntdUI.TreeItem("[" + index + "]");
                    BuildJsonNode(child, item);
                    parent.Sub.Add(child);
                    index++;
                }
                parent.Tag = serializerSafeToString(value);
            }
            else
            {
                var text = serializerSafeToString(value);
                var child = new AntdUI.TreeItem(text);
                child.Tag = text;
                parent.Sub.Add(child);
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
            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.XmlResolver = null;
                doc.LoadXml(xmlText);
                var root = new AntdUI.TreeItem(doc.DocumentElement.Name);
                root.Tag = doc.DocumentElement.OuterXml;
                BuildXmlNode(root, doc.DocumentElement);
                treeOutput.Items.Add(root);
                root.Expand = true;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ XML解析失败: " + ex.Message;
            }
        }

        private void BuildXmlNode(AntdUI.TreeItem parent, System.Xml.XmlNode node)
        {
            if (node.Attributes != null)
            {
                foreach (System.Xml.XmlAttribute attr in node.Attributes)
                {
                    var attrNode = new AntdUI.TreeItem("@" + attr.Name + "=" + attr.Value);
                    attrNode.Tag = attr.Value;
                    parent.Sub.Add(attrNode);
                }
            }
            
            foreach (System.Xml.XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == System.Xml.XmlNodeType.Element)
                {
                    var childNode = new AntdUI.TreeItem(child.Name);
                    childNode.Tag = child.OuterXml;
                    BuildXmlNode(childNode, child);
                    parent.Sub.Add(childNode);
                }
                else if (child.NodeType == System.Xml.XmlNodeType.Text || child.NodeType == System.Xml.XmlNodeType.CDATA)
                {
                    var text = child.InnerText;
                    var textNode = new AntdUI.TreeItem(text);
                    textNode.Tag = text;
                    parent.Sub.Add(textNode);
                }
            }
        }

        private void btnExpandAll_Click(object sender, EventArgs e)
        {
            // 展开所有节点
            foreach (AntdUI.TreeItem node in treeOutput.Items)
            {
                ExpandNode(node);
            }
            lblStatus.Text = "✅ 已展开所有节点";
        }

        private void ExpandNode(AntdUI.TreeItem node)
        {
            node.Expand = true;
            foreach (AntdUI.TreeItem child in node.Sub)
            {
                ExpandNode(child);
            }
        }

        private void btnCollapseAll_Click(object sender, EventArgs e)
        {
            // 折叠所有节点
            foreach (AntdUI.TreeItem node in treeOutput.Items)
            {
                CollapseNode(node);
            }
            lblStatus.Text = "✅ 已折叠所有节点";
        }

        private void CollapseNode(AntdUI.TreeItem node)
        {
            node.Expand = false;
            foreach (AntdUI.TreeItem child in node.Sub)
            {
                CollapseNode(child);
            }
        }

        private string _lastFind = "";
        private AntdUI.TreeItem _findCursor;

        private void btnFindPrev_Click(object sender, EventArgs e)
        {
            _lastFind = txtFind.Text;
            FindPrev();
        }

        private void btnFindNext_Click(object sender, EventArgs e)
        {
            _lastFind = txtFind.Text;
            FindNext();
        }

        private void FindNext()
        {
            if (string.IsNullOrEmpty(_lastFind)) return;
            
            // 在树中查找下一个匹配项
            var nodes = GetAllNodes();
            var startIndex = _findCursor != null ? nodes.IndexOf(_findCursor) : -1;
            
            for (int i = startIndex + 1; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (NodeMatches(node, _lastFind))
                {
                    _findCursor = node;
                    // 选择节点（如果AntdUI.Tree支持）
                    lblStatus.Text = "✅ 找到匹配项";
                    return;
                }
            }
            
            lblStatus.Text = "❌ 未找到";
        }

        private void FindPrev()
        {
            if (string.IsNullOrEmpty(_lastFind)) return;
            
            // 在树中查找上一个匹配项
            var nodes = GetAllNodes();
            var startIndex = _findCursor != null ? nodes.IndexOf(_findCursor) : nodes.Count;
            
            for (int i = startIndex - 1; i >= 0; i--)
            {
                var node = nodes[i];
                if (NodeMatches(node, _lastFind))
                {
                    _findCursor = node;
                    // 选择节点（如果AntdUI.Tree支持）
                    lblStatus.Text = "✅ 找到匹配项";
                    return;
                }
            }
            
            lblStatus.Text = "❌ 未找到";
        }

        private List<AntdUI.TreeItem> GetAllNodes()
        {
            var nodes = new List<AntdUI.TreeItem>();
            foreach (AntdUI.TreeItem node in treeOutput.Items)
            {
                CollectNodes(node, nodes);
            }
            return nodes;
        }

        private void CollectNodes(AntdUI.TreeItem node, List<AntdUI.TreeItem> nodes)
        {
            nodes.Add(node);
            foreach (AntdUI.TreeItem child in node.Sub)
            {
                CollectNodes(child, nodes);
            }
        }

        private bool NodeMatches(AntdUI.TreeItem node, string searchText)
        {
            return (node.Text != null && node.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                   (Convert.ToString(node.Tag) ?? string.Empty).IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void btnWrap_Click(object sender, EventArgs e)
        {
            // 切换文本换行
            lblStatus.Text = "✅ 切换换行模式";
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
        }

        private void LoadHistory()
        {
            // 简化历史记录加载，实际项目中可以从文件加载
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

        private void btnFont_Click(object sender, EventArgs e)
        {
            using (var dlg = new FontDialog())
            {
                dlg.Font = txtInput.Font;
                dlg.ShowColor = true;
                // 修复颜色类型转换问题
                dlg.Color = txtInput.ForeColor.HasValue ? txtInput.ForeColor.Value : Color.Black;
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

        private void UpdateStats()
        {
            var input = txtInput.Text ?? string.Empty;
            var output = txtOutput.Text ?? string.Empty;
            int inLines = input.Length == 0 ? 0 : input.Replace("\r\n", "\n").Split('\n').Length;
            int outLines = output.Length == 0 ? 0 : output.Replace("\r\n", "\n").Split('\n').Length;
            lblStats.Text = "📄 输入 行:" + inLines.ToString() + " 字符:" + input.Length.ToString() +
                            "   |   📝 输出 行:" + outLines.ToString() + " 字符:" + output.Length.ToString();
        }

        /// <summary>
        /// 应用语法高亮
        /// </summary>
        /// <param name="format">格式类型</param>
        /// <param name="text">文本内容</param>
        private void ApplySyntaxHighlighting(string format, string text)
        {
            // 清除现有的样式
            txtOutput.ClearStyle(true);
            
            // 根据格式类型应用不同的高亮规则
            switch (format.ToUpper())
            {
                case "JSON":
                    HighlightJson(text);
                    break;
                case "XML":
                    HighlightXml(text);
                    break;
                default:
                    // 对于其他格式，应用基础的主题颜色
                    ApplyThemeColors();
                    break;
            }
        }
        
        /// <summary>
        /// 应用主题颜色到整个文本
        /// </summary>
        private void ApplyThemeColors()
        {
            if (isLight)
            {
                // 浅色主题
                txtInput.ForeColor = txtOutput.ForeColor = Color.Black;
                txtInput.BackColor = txtOutput.BackColor = Color.White;
            }
            else
            {
                // 深色主题
                txtInput.ForeColor = txtOutput.ForeColor = Color.White;
                txtInput.BackColor = txtOutput.BackColor = Color.FromArgb(31, 31, 31);
            }
        }

        /// <summary>
        /// JSON语法高亮
        /// </summary>
        /// <param name="json">JSON文本</param>
        private void HighlightJson(string json)
        {
            // 应用基础主题颜色
            ApplyThemeColors();
            
            if (isLight)
            {
                // 浅色主题的JSON高亮
                HighlightJsonLight(json);
            }
            else
            {
                // 深色主题的JSON高亮
                HighlightJsonDark(json);
            }
        }
        
        private void HighlightJsonLight(string json)
        {
            // 关键字颜色 (true, false, null)
            HighlightPattern(json, @"\b(true|false|null)\b", Color.Blue, Color.Empty);
            
            // 字符串颜色
            HighlightPattern(json, @"""([^""\\]|\\.)*""", Color.Brown, Color.Empty);
            
            // 数字颜色
            HighlightPattern(json, @"\b\d+(\.\d+)?\b", Color.Green, Color.Empty);
            
            // 结构符号颜色
            HighlightPattern(json, @"[{}[\]:,]", Color.Black, Color.Empty);
        }
        
        private void HighlightJsonDark(string json)
        {
            // 关键字颜色 (true, false, null)
            HighlightPattern(json, @"\b(true|false|null)\b", Color.Cyan, Color.Empty);
            
            // 字符串颜色
            HighlightPattern(json, @"""([^""\\]|\\.)*""", Color.Orange, Color.Empty);
            
            // 数字颜色
            HighlightPattern(json, @"\b\d+(\.\d+)?\b", Color.LimeGreen, Color.Empty);
            
            // 结构符号颜色
            HighlightPattern(json, @"[{}[\]:,]", Color.White, Color.Empty);
        }
        
        /// <summary>
        /// XML语法高亮
        /// </summary>
        /// <param name="xml">XML文本</param>
        private void HighlightXml(string xml)
        {
            // 应用基础主题颜色
            ApplyThemeColors();
            
            if (isLight)
            {
                // 浅色主题的XML高亮
                HighlightXmlLight(xml);
            }
            else
            {
                // 深色主题的XML高亮
                HighlightXmlDark(xml);
            }
        }
        
        private void HighlightXmlLight(string xml)
        {
            // 标签颜色
            HighlightPattern(xml, @"<[^>]*>", Color.Blue, Color.Empty);
            
            // 属性名颜色
            HighlightPattern(xml, @"\s+(\w+(?==))", Color.Red, Color.Empty);
            
            // 属性值颜色
            HighlightPattern(xml, @"=""([^""]*)""", Color.Brown, Color.Empty);
            
            // 注释颜色
            HighlightPattern(xml, @"<!--[\s\S]*?-->", Color.Green, Color.Empty);
        }
        
        private void HighlightXmlDark(string xml)
        {
            // 标签颜色
            HighlightPattern(xml, @"<[^>]*>", Color.Cyan, Color.Empty);
            
            // 属性名颜色
            HighlightPattern(xml, @"\s+(\w+(?==))", Color.Orange, Color.Empty);
            
            // 属性值颜色
            HighlightPattern(xml, @"=""([^""]*)""", Color.Yellow, Color.Empty);
            
            // 注释颜色
            HighlightPattern(xml, @"<!--[\s\S]*?-->", Color.LimeGreen, Color.Empty);
        }
        
        /// <summary>
        /// 使用正则表达式高亮文本模式
        /// </summary>
        /// <param name="text">要处理的文本</param>
        /// <param name="pattern">正则表达式模式</param>
        /// <param name="foreColor">前景色</param>
        /// <param name="backColor">背景色</param>
        private void HighlightPattern(string text, string pattern, Color foreColor, Color backColor)
        {
            try
            {
                var regex = new Regex(pattern, RegexOptions.Multiline);
                var matches = regex.Matches(text);
                
                foreach (Match match in matches)
                {
                    if (match.Length > 0)
                    {
                        txtOutput.SetStyle(match.Index, match.Length, txtOutput.Font, foreColor, backColor);
                    }
                }
            }
            catch
            {
                // 忽略正则表达式错误
            }
        }

        private void TreeOutput_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // 创建右键菜单项
                var menuItems = new List<AntdUI.IContextMenuStripItem>
                {
                    new AntdUI.ContextMenuStripItem("📋 复制节点文本") { Tag = "copy_text" },
                    new AntdUI.ContextMenuStripItem("📦 复制节点值") { Tag = "copy_value" },
                    new AntdUI.ContextMenuStripItemDivider(),
                    new AntdUI.ContextMenuStripItem("➕ 展开所有") { Tag = "expand_all" },
                    new AntdUI.ContextMenuStripItem("➖ 折叠所有") { Tag = "collapse_all" },
                    new AntdUI.ContextMenuStripItemDivider(),
                    new AntdUI.ContextMenuStripItem("🔍 在输出中查找") { Tag = "find_in_output" }
                };

                // 显示右键菜单
                AntdUI.ContextMenuStrip.open(treeOutput, OnContextMenuItemClick, menuItems.ToArray());
            }
        }

        private void BindButtonWithToolTip(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is AntdUI.Button button)
                {
                    AntdUI.TooltipComponent tooltip = new AntdUI.TooltipComponent()
                    {
                        Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(134))),
                    };

                    var name = control.Name;
                    switch (name)
                    {
                        case "btnPretty":
                            tooltip.ArrowAlign = AntdUI.TAlign.Bottom;
                            tooltip.SetTip(control, "格式化");
                            break;
                        case "btnMinify":
                            tooltip.ArrowAlign = AntdUI.TAlign.Bottom;
                            tooltip.SetTip(control, "压缩");
                            break;
                        case "btnDetect":
                            tooltip.ArrowAlign = AntdUI.TAlign.Bottom;
                            tooltip.SetTip(control, "自动检测格式");
                            break;
                        case "btnCopy":
                            tooltip.ArrowAlign = AntdUI.TAlign.Bottom;
                            tooltip.SetTip(control, "复制");
                            break;
                        case "btnOpen":
                            tooltip.ArrowAlign = AntdUI.TAlign.Bottom;
                            tooltip.SetTip(control, "打开文件");
                            break;
                        case "btnSave":
                            tooltip.ArrowAlign = AntdUI.TAlign.Bottom;
                            tooltip.SetTip(control, "保存文件");
                            break;
                        case "btnExpandAll":
                            tooltip.ArrowAlign = AntdUI.TAlign.Bottom;
                            tooltip.SetTip(control, "展开所有节点");
                            break;
                        case "btnCollapseAll":
                            tooltip.ArrowAlign = AntdUI.TAlign.Bottom;
                            tooltip.SetTip(control, "折叠所有节点");
                            break;
                        case "btnFindPrev":
                            tooltip.ArrowAlign = AntdUI.TAlign.Bottom;
                            tooltip.SetTip(control, "查找上一个");
                            break;
                        case "btnFindNext":
                            tooltip.ArrowAlign = AntdUI.TAlign.Bottom;
                            tooltip.SetTip(control, "查找下一个");
                            break;
                        case "btnWrap":
                            tooltip.ArrowAlign = AntdUI.TAlign.Bottom;
                            tooltip.SetTip(control, "切换换行");
                            break;
                        case "btnFont":
                            tooltip.ArrowAlign = AntdUI.TAlign.Bottom;
                            tooltip.SetTip(control, "字体设置");
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void OnContextMenuItemClick(AntdUI.ContextMenuStripItem item)
        {
            switch (item.Tag?.ToString())
            {
                case "copy_text":
                    CopyTreeNodeText();
                    break;
                case "copy_value":
                    CopyTreeNodeValue();
                    break;
                case "expand_all":
                    btnExpandAll_Click(null, EventArgs.Empty);
                    break;
                case "collapse_all":
                    btnCollapseAll_Click(null, EventArgs.Empty);
                    break;
                case "find_in_output":
                    FindInOutput();
                    break;
            }
        }

        private void CopyTreeNodeText()
        {
            var selectedNode = treeOutput.SelectItem;
            if (selectedNode != null)
            {
                try
                {
                    Clipboard.SetText(selectedNode.Text ?? string.Empty);
                    lblStatus.Text = "✅ 已复制节点文本";
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "❌ 复制失败: " + ex.Message;
                }
            }
        }

        private void CopyTreeNodeValue()
        {
            var selectedNode = treeOutput.SelectItem;
            if (selectedNode != null)
            {
                try
                {
                    var value = selectedNode.Tag?.ToString() ?? selectedNode.Text ?? string.Empty;
                    Clipboard.SetText(value);
                    lblStatus.Text = "✅ 已复制节点值";
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "❌ 复制失败: " + ex.Message;
                }
            }
        }

        private void FindInOutput()
        {
            var selectedNode = treeOutput.SelectItem;
            if (selectedNode != null)
            {
                var searchText = selectedNode.Text ?? selectedNode.Tag?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(searchText))
                {
                    txtFind.Text = searchText;
                    // 可以在这里添加实际的查找逻辑
                    lblStatus.Text = "🔍 已在查找框中填入节点文本";
                }
            }
        }
    }
}