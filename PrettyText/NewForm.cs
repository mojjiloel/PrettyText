using PrettyText.TextFormatters;
using PrettyText.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static PrettyText.Utils.ClassGenerator;

namespace PrettyText
{
    public partial class NewForm : AntdUI.Window
    {
        /// <summary>超过该字符数不再构建树视图（树视图对超大文本没有阅读价值，且构建代价很高）</summary>
        private const int TreeBuildLimit = 3 * 1024 * 1024;

        /// <summary>生成模型类的输入上限</summary>
        private const int ExportGenerateLimit = 2 * 1024 * 1024;

        /// <summary>打开文件时的体积提醒阈值</summary>
        private const long LargeFileWarningSize = 64L * 1024 * 1024;

        /// <summary>超过该字符数时状态栏不再统计行数（避免每次输入都复制整段文本）</summary>
        private const int LineCountLimit = 2 * 1024 * 1024;

        private readonly AppSettings _settings;
        private readonly Timer _statsTimer = new Timer();
        private readonly AntdUI.TooltipComponent _tooltip = new AntdUI.TooltipComponent();

        private bool _isLight = true;
        private bool _wrap = true;
        private bool _restyling;

        private float _customFontSize = 10.5f;
        private Color _customFontColor = Color.FromArgb(40, 40, 40);
        private string _customFontFamily = "Consolas";

        private List<string> _history;
        private string _lastFind = string.Empty;
        private AntdUI.TreeItem _findCursor;

        public NewForm()
        {
            _settings = AppSettings.Load();
            InitializeComponent();
            InitializeUiLogic();
        }

        #region 初始化

        private void InitializeUiLogic()
        {
            // 主题：优先使用上次保存的设置，否则跟随系统
            _isLight = _settings.IsLight ?? ThemeHelper.IsLightMode();
            button_color.Toggle = !_isLight;
            ThemeHelper.SetColorMode(this, _isLight);
            UpdateStatusBarColors();

            // 字体
            _customFontSize = _settings.FontSize;
            _customFontFamily = _settings.FontFamily;
            _customFontColor = Color.FromArgb(_settings.FontColorArgb);

            // 格式下拉（来自注册器）
            var formats = FormatterRegistry.GetAll().Select(f => f.Name).ToList();
            cboFormat.Items.Clear();
            foreach (var format in formats) cboFormat.Items.Add(format);
            if (formats.Count > 0) cboFormat.Text = formats[0];

            // 历史记录
            _history = _settings.History;
            RefreshHistoryCombo();

            // 编辑器
            _wrap = _settings.WordWrap;
            ApplyWrap();
            ApplyFontSettings(_customFontSize, _customFontColor, _customFontFamily, persist: false);
            ApplySyntaxHighlighting(cboFormat.Text ?? string.Empty, txtOutput.Text ?? string.Empty);

            // 统计信息使用防抖计时器，避免每次输入都对整段文本做一次统计
            _statsTimer.Interval = 250;
            _statsTimer.Tick += (s, e) =>
            {
                _statsTimer.Stop();
                UpdateStatsCore();
            };

            BindEvents();

            // 窗口尺寸 / 位置
            RestoreWindowLayout();
            LayoutToolbar();

            UpdateStatsCore();
            lblStatus.Text = "✅ 就绪";
        }

        private void BindEvents()
        {
            txtInput.TextChanged += (s, e) => ScheduleStats();
            txtOutput.TextChanged += (s, e) => ScheduleStats();

            txtInput.AllowDrop = true;
            txtInput.DragEnter += Editor_DragEnter;
            txtInput.DragDrop += Editor_DragDrop;
            AllowDrop = true;
            DragEnter += Editor_DragEnter;
            DragDrop += Editor_DragDrop;

            treeOutput.MouseDown += TreeOutput_MouseDown;

            btnPretty.Click += (s, e) => RunFormat(pretty: true);
            btnMinify.Click += (s, e) => RunFormat(pretty: false);
            btnDetect.Click += btnDetect_Click;
            btnCopy.Click += btnCopy_Click;
            btnOpen.Click += btnOpen_Click;
            btnSave.Click += btnSave_Click;
            btnClear.Click += btnClear_Click;
            btnWrap.Click += btnWrap_Click;
            btnExpandAll.Click += btnExpandAll_Click;
            btnCollapseAll.Click += btnCollapseAll_Click;
            btnFindPrev.Click += btnFindPrev_Click;
            btnFindNext.Click += btnFindNext_Click;
            cboHistory.SelectedIndexChanged += cboHistory_SelectedIndexChanged;
            btnFont.Click += btnFont_Click;
            button_color.Click += Button_color_Click;

            // AntdUI.Input 使用虚拟焦点，回车需要在 VerifyKeyboard 中处理
            txtFind.VerifyKeyboard += (s, e) =>
            {
                if ((e.KeyData & Keys.KeyCode) != Keys.Enter) return;
                e.Result = false;
                FindInternal((e.KeyData & Keys.Shift) != Keys.Shift);
            };
            txtFind.TextChanged += (s, e) =>
            {
                _lastFind = txtFind.Text ?? string.Empty;
                _findCursor = null;
            };

            panelToolbar.SizeChanged += (s, e) => LayoutToolbar();

            BindButtonWithToolTip(panelToolbar);

            select1.SelectedIndex = 0;
            select1.SelectedIndexChanged += Select1_SelectedIndexChanged;
            tabControl1.SelectedIndexChanged += TabControl1_SelectedIndexChanged;

            // 默认停留在结果（Text）标签页
            tabControl1.SelectedIndex = 0;
        }

        private void RestoreWindowLayout()
        {
            MinimumSize = new Size(1180, 560);

            try
            {
                var size = new Size(Math.Max(_settings.WindowWidth, MinimumSize.Width), Math.Max(_settings.WindowHeight, MinimumSize.Height));

                int left = _settings.WindowX;
                int top = _settings.WindowY;
                var area = Screen.PrimaryScreen.WorkingArea;

                if (left != int.MinValue && top != int.MinValue)
                    area = Screen.FromPoint(new Point(left, top)).WorkingArea;

                // 保证窗口完整落在工作区内，避免状态栏被任务栏遮住
                size.Width = Math.Min(size.Width, area.Width);
                size.Height = Math.Min(size.Height, area.Height);
                ClientSize = size;

                if (_settings.WindowX != int.MinValue && _settings.WindowY != int.MinValue)
                {
                    int x = Math.Max(area.Left, Math.Min(left, area.Right - size.Width));
                    int y = Math.Max(area.Top, Math.Min(top, area.Bottom - size.Height));
                    var bounds = new Rectangle(new Point(x, y), size);
                    bool visible = Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(bounds));
                    if (visible)
                    {
                        StartPosition = FormStartPosition.Manual;
                        Location = new Point(x, y);
                    }
                }

                if (_settings.SplitterDistance > 120 && _settings.SplitterDistance < ClientSize.Width - 120)
                    splitContainer1.SplitterDistance = _settings.SplitterDistance;
            }
            catch (Exception)
            {
                // 布局恢复失败时使用设计器默认值
            }
        }

        /// <summary>
        /// 窗口变宽时把查找 / 历史 / 字体等按钮组靠右对齐，避免工具栏大面积留白。
        /// 设计宽度为 1349：更窄时保持设计位置，更宽时整体右移。
        /// </summary>
        private void LayoutToolbar()
        {
            if (panelToolbar == null) return;

            int shift = Math.Max(0, panelToolbar.Width - 1146);

            var rightGroup = new Control[] { btnExpandAll, btnCollapseAll, txtFind, btnFindPrev, btnFindNext, cboHistory, btnFont };
            var design = new int[] { 616, 665, 728, 854, 884, 920, 1076 };

            for (int i = 0; i < rightGroup.Length; i++)
            {
                if (rightGroup[i] == null) continue;
                rightGroup[i].Left = design[i] + shift;
            }
        }

        private void SaveWindowLayout()
        {
            try
            {
                var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
                _settings.IsLight = _isLight;
                _settings.WordWrap = _wrap;
                _settings.FontFamily = _customFontFamily;
                _settings.FontSize = _customFontSize;
                _settings.FontColorArgb = _customFontColor.ToArgb();
                _settings.WindowWidth = bounds.Width;
                _settings.WindowHeight = bounds.Height;
                _settings.WindowX = bounds.X;
                _settings.WindowY = bounds.Y;
                _settings.SplitterDistance = splitContainer1.SplitterDistance;
                _settings.Save();
            }
            catch (Exception)
            {
                // 保存失败不影响关闭
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _statsTimer.Stop();
            SaveWindowLayout();
            base.OnFormClosing(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.E:
                    RunFormat(pretty: true);
                    return true;
                case Keys.Control | Keys.M:
                    RunFormat(pretty: false);
                    return true;
                case Keys.Control | Keys.O:
                    btnOpen_Click(this, EventArgs.Empty);
                    return true;
                case Keys.Control | Keys.S:
                    btnSave_Click(this, EventArgs.Empty);
                    return true;
                case Keys.Control | Keys.F:
                    txtFind.Focus();
                    txtFind.SelectAll();
                    return true;
                case Keys.F5:
                    btnDetect_Click(this, EventArgs.Empty);
                    return true;
            }

            if (keyData == Keys.Enter && txtFind.Focused)
            {
                FindInternal(true);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        #region 主题

        private void Button_color_Click(object sender, EventArgs e)
        {
            _isLight = !_isLight;
            button_color.Toggle = !_isLight;
            ThemeHelper.SetColorMode(this, _isLight);
            ApplyEditorTheme();
            UpdateStatusBarColors();

            // 主题变化后重新着色
            ApplySyntaxHighlighting(cboFormat.Text ?? string.Empty, txtOutput.Text ?? string.Empty);
            ApplyCodeHighlighting(SelectedLanguage());
        }

        private void UpdateStatusBarColors()
        {
            statusPanel.Back = ThemeHelper.StatusBarBackground(_isLight);
            var fore = ThemeHelper.Foreground(_isLight);
            lblStatus.ForeColor = fore;
            lblStats.ForeColor = fore;
        }

        /// <summary>
        /// 刷新三个编辑器的主题配色
        /// </summary>
        private void ApplyEditorTheme()
        {
            var back = ThemeHelper.Background(_isLight);
            var fore = ResolveEditorForeColor(ThemeHelper.Foreground(_isLight));
            var placeholder = ThemeHelper.Placeholder(_isLight);

            txtInput.ApplyTheme(back, fore, placeholder);
            txtOutput.ApplyTheme(back, fore, placeholder);
            input1.ApplyTheme(back, fore, placeholder);
            panelTextOutput.Back = back;
        }

        /// <summary>
        /// 自定义字体颜色在极端明暗下不可读时回退到主题色
        /// </summary>
        private Color ResolveEditorForeColor(Color fallback)
        {
            int brightness = (_customFontColor.R + _customFontColor.G + _customFontColor.B) / 3;
            if (_isLight && brightness > 200) return fallback;
            if (!_isLight && brightness < 90) return fallback;
            return _customFontColor;
        }

        #endregion

        #region 格式化

        private void btnDetect_Click(object sender, EventArgs e)
        {
            try
            {
                var formatter = FormatterRegistry.Resolve(txtInput.Text);
                cboFormat.Text = formatter.Name;
                RunFormat(pretty: true);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ 识别失败: " + ex.Message;
            }
        }

        private void RunFormat(bool pretty)
        {
            var input = txtInput.Text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(input))
            {
                lblStatus.Text = "⚠️ 请先输入或打开文本";
                return;
            }

            try
            {
                AppendHistory(input);

                var selected = (cboFormat.Text ?? string.Empty).Trim();
                ITextFormatter formatter = FormatterRegistry.GetAll()
                    .FirstOrDefault(f => string.Equals(f.Name, selected, StringComparison.OrdinalIgnoreCase));
                if (formatter == null) formatter = FormatterRegistry.Resolve(input);

                string output = null;
                ITextFormatter used = formatter;
                Exception firstError = null;

                try
                {
                    output = pretty ? formatter.FormatPretty(input) : formatter.FormatMinified(input);
                }
                catch (Exception ex)
                {
                    firstError = ex;
                }

                if (output == null)
                {
                    // 下拉框选中的格式和内容不匹配时，自动改用识别到的格式
                    var detected = FormatterRegistry.Resolve(input);
                    if (!string.Equals(detected.Name, formatter.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            output = pretty ? detected.FormatPretty(input) : detected.FormatMinified(input);
                            used = detected;
                        }
                        catch (Exception)
                        {
                            output = null;
                        }
                    }
                }

                if (output == null)
                {
                    // 再尝试去掉外层引号与转义
                    var fallback = SafeUnescape(input);
                    try
                    {
                        output = pretty ? used.FormatPretty(fallback) : used.FormatMinified(fallback);
                    }
                    catch (Exception)
                    {
                        output = null;
                    }
                }

                if (output == null)
                {
                    lblStatus.Text = "❌ 处理失败: " + (firstError == null ? "内容与所选格式不匹配" : firstError.Message);
                    return;
                }

                cboFormat.Text = used.Name;
                ShowOutput(used.Name, output);
                lblStatus.Text = (pretty ? "✨ 已美化: " : "📦 已压缩: ") + used.Name;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ 处理失败: " + ex.Message;
            }
        }

        /// <summary>
        /// 把结果写入输出编辑器（旧实现只清了文本却从未赋值，导致 Text 标签页始终为空）
        /// </summary>
        private void ShowOutput(string format, string output)
        {
            output = output ?? string.Empty;

            _restyling = true;
            try
            {
                txtOutput.Text = output;
            }
            finally
            {
                _restyling = false;
            }

            ApplySyntaxHighlighting(format, output);
            BuildTree(format, output);
            UpdateStatsCore();
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

        private void ApplySyntaxHighlighting(string format, string text)
        {
            SyntaxHighlighter.Apply(txtOutput, format, _isLight);
        }

        private void ApplyCodeHighlighting(LanguageType language)
        {
            SyntaxHighlighter.Apply(input1, language == LanguageType.CSharp ? "C#" : "Java", _isLight);
        }

        #endregion

        #region 统计

        private void ScheduleStats()
        {
            if (_restyling) return;
            _statsTimer.Stop();
            _statsTimer.Start();
        }

        private void UpdateStatsCore()
        {
            lblStats.Text = "📄 输入 " + Describe(txtInput) + "    📝 输出 " + Describe(txtOutput);
        }

        /// <summary>
        /// 大文本下不再取回整段字符串（RichTextBox.Text 会复制全文），只做字符量统计
        /// </summary>
        private static string Describe(RichTextBox box)
        {
            int length = box.TextLength;
            if (length == 0) return "0 行 0 字符";
            if (length > LineCountLimit) return FormatLength(length);
            return CountLines(box.Text) + " 行 " + FormatLength(length);
        }

        private static int CountLines(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            int lines = 1;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n') lines++;
            }
            return lines;
        }

        private static string FormatLength(int chars)
        {
            if (chars < 1024) return chars + " 字符";
            if (chars < 1024 * 1024) return (chars / 1024.0).ToString("0.0") + " KB";
            return (chars / 1048576.0).ToString("0.00") + " MB";
        }

        #endregion

        #region 文件与剪贴板

        private void btnCopy_Click(object sender, EventArgs e)
        {
            var selectedText = txtOutput.SelectedText;
            var textToCopy = !string.IsNullOrEmpty(selectedText) ? selectedText : txtOutput.Text;

            if (string.IsNullOrEmpty(textToCopy))
            {
                lblStatus.Text = "⚠️ 无可复制内容";
                return;
            }

            lblStatus.Text = ClipboardHelper.TrySetText(textToCopy)
                ? "✅ 已复制 " + FormatLength(textToCopy.Length)
                : "❌ 复制失败：剪贴板被其他程序占用";
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtInput.Clear();
            txtOutput.Clear();
            treeOutput.Items.Clear();
            input1.Clear();
            lblStatus.Text = "✅ 已清空";
            UpdateStatsCore();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "All|*.*|Text|*.txt;*.log;*.md;*.cfg|JSON|*.json|XML|*.xml|YAML|*.yml;*.yaml|CSV|*.csv|HTML|*.html;*.htm";
                if (ofd.ShowDialog() != DialogResult.OK) return;
                LoadFile(ofd.FileName);
            }
        }

        private void LoadFile(string path)
        {
            try
            {
                var info = new FileInfo(path);
                if (info.Length > LargeFileWarningSize)
                {
                    var result = MessageBox.Show(this,
                        string.Format("文件约 {0:0.#} MB，打开后界面可能短暂无响应，是否继续？", info.Length / 1048576.0),
                        "PrettyText", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes) return;
                }

                var text = ReadFileText(path);
                txtInput.Text = text;
                lblStatus.Text = "✅ 已打开文件: " + Path.GetFileName(path) + "（" + FormatLength(text.Length) + "）";
                AppendHistory(text);
                UpdateStatsCore();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ 打开失败: " + ex.Message;
            }
        }

        /// <summary>
        /// 读取文本文件：优先按 BOM 判断编码，遇到乱码再按系统 ANSI（中文 GBK）重读。
        /// </summary>
        private static string ReadFileText(string path)
        {
            var text = File.ReadAllText(path, Encoding.UTF8);
            if (text.IndexOf('\uFFFD') >= 0)
            {
                try { text = File.ReadAllText(path, Encoding.Default); }
                catch (Exception) { }
            }
            return text;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Text|*.txt;*.json;*.xml;*.yaml;*.yml;*.csv;*.html|All|*.*";
                sfd.FileName = "output.txt";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    File.WriteAllText(sfd.FileName, txtOutput.Text ?? string.Empty, new UTF8Encoding(false));
                    lblStatus.Text = "✅ 已保存到: " + sfd.FileName;
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "❌ 保存失败: " + ex.Message;
                }
            }
        }

        private void Editor_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void Editor_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null || files.Length == 0) return;
            LoadFile(files[0]);
        }

        #endregion

        #region 字体

        private void btnFont_Click(object sender, EventArgs e)
        {
            using (var dlg = new FontDialog())
            {
                dlg.Font = txtInput.Font;
                dlg.ShowColor = true;
                dlg.Color = _customFontColor;
                dlg.FontMustExist = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    ApplyFontSettings(dlg.Font.Size, dlg.Color, dlg.Font.FontFamily.Name, persist: true);
                    lblStatus.Text = "✅ 已应用自定义字体";
                }
            }
        }

        private void ApplyFontSettings(float size, Color color, string fontFamily, bool persist)
        {
            try
            {
                var font = new Font(fontFamily, size);
                txtInput.Font = font;
                txtOutput.Font = font;
                input1.Font = font;

                _customFontSize = size;
                _customFontColor = color;
                _customFontFamily = fontFamily;

                ApplyEditorTheme();
                ApplySyntaxHighlighting(cboFormat.Text ?? string.Empty, txtOutput.Text ?? string.Empty);
                ApplyCodeHighlighting(SelectedLanguage());

                if (persist) SaveWindowLayout();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ 字体设置失败: " + ex.Message;
            }
        }

        #endregion

        #region 自动换行

        private void btnWrap_Click(object sender, EventArgs e)
        {
            _wrap = !_wrap;
            ApplyWrap();
            lblStatus.Text = _wrap ? "✅ 已开启自动换行" : "✅ 已关闭自动换行";
        }

        private void ApplyWrap()
        {
            txtInput.WordWrap = _wrap;
            txtOutput.WordWrap = _wrap;
            input1.WordWrap = _wrap;

            var scrollBars = _wrap ? RichTextBoxScrollBars.Vertical : RichTextBoxScrollBars.Both;
            txtInput.ScrollBars = scrollBars;
            txtOutput.ScrollBars = scrollBars;
            input1.ScrollBars = scrollBars;

            btnWrap.Toggle = !_wrap;
        }

        #endregion

        #region 历史记录

        private void AppendHistory(string text)
        {
            _settings.AddHistory(text);
            RefreshHistoryCombo();
        }

        private void RefreshHistoryCombo()
        {
            cboHistory.Items.Clear();
            for (int i = 0; i < _history.Count; i++)
            {
                var preview = _history[i].Replace("\r\n", " ").Replace("\n", " ").Replace("\t", " ");
                if (preview.Length > 60) preview = preview.Substring(0, 60) + "...";
                cboHistory.Items.Add("#" + i + " " + preview);
            }
        }

        private void cboHistory_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idx = cboHistory.SelectedIndex;
            if (idx < 0 || idx >= _history.Count) return;

            txtInput.Text = _history[idx];
            lblStatus.Text = "✅ 已从历史载入";
            UpdateStatsCore();
        }

        #endregion

        #region 树视图

        private void BuildTree(string format, string text)
        {
            treeOutput.Items.Clear();

            try
            {
                if (string.IsNullOrEmpty(text)) return;

                if (text.Length > TreeBuildLimit)
                {
                    treeOutput.Items.Add(new AntdUI.TreeItem("内容过大(" + FormatLength(text.Length) + ")，已跳过树视图"));
                    return;
                }

                if (string.Equals(format, "JSON", StringComparison.OrdinalIgnoreCase))
                {
                    BuildTreeFromJson(text);
                }
                else if (string.Equals(format, "XML", StringComparison.OrdinalIgnoreCase))
                {
                    BuildTreeFromXml(text);
                }
                else
                {
                    var node = new AntdUI.TreeItem(format);
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
                serializer.MaxJsonLength = int.MaxValue;
                serializer.RecursionLimit = 200;
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
                parent.Sub.Add(new AntdUI.TreeItem(text) { Tag = text });
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
                    parent.Sub.Add(new AntdUI.TreeItem(text) { Tag = text });
                }
            }
        }

        private void btnExpandAll_Click(object sender, EventArgs e)
        {
            foreach (AntdUI.TreeItem node in treeOutput.Items) ExpandNode(node);
            lblStatus.Text = "✅ 已展开所有节点";
        }

        private void btnCollapseAll_Click(object sender, EventArgs e)
        {
            foreach (AntdUI.TreeItem node in treeOutput.Items) CollapseNode(node);
            lblStatus.Text = "✅ 已折叠所有节点";
        }

        private void ExpandNode(AntdUI.TreeItem node)
        {
            node.Expand = true;
            foreach (AntdUI.TreeItem child in node.Sub) ExpandNode(child);
        }

        private void CollapseNode(AntdUI.TreeItem node)
        {
            node.Expand = false;
            foreach (AntdUI.TreeItem child in node.Sub) CollapseNode(child);
        }

        #endregion

        #region 查找

        private void btnFindPrev_Click(object sender, EventArgs e)
        {
            FindInternal(false);
        }

        private void btnFindNext_Click(object sender, EventArgs e)
        {
            FindInternal(true);
        }

        private void FindInternal(bool forward)
        {
            _lastFind = txtFind.Text ?? string.Empty;
            if (_lastFind.Length == 0)
            {
                lblStatus.Text = "⚠️ 请输入查找内容";
                return;
            }

            // 树视图下沿用按节点匹配的行为
            if (tabControl1.SelectedIndex == 1)
            {
                FindInTree(forward);
                return;
            }

            FindInOutput(forward);
        }

        private void FindInOutput(bool forward)
        {
            var text = txtOutput.Text ?? string.Empty;
            if (text.Length == 0)
            {
                lblStatus.Text = "❌ 没有可查找的内容";
                return;
            }

            int index = -1;
            if (forward)
            {
                int start = Math.Min(txtOutput.SelectionStart + txtOutput.SelectionLength, text.Length);
                index = text.IndexOf(_lastFind, start, StringComparison.OrdinalIgnoreCase);
                if (index < 0) index = text.IndexOf(_lastFind, 0, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                int start = Math.Min(Math.Max(txtOutput.SelectionStart - 1, 0), text.Length - 1);
                index = text.LastIndexOf(_lastFind, start, StringComparison.OrdinalIgnoreCase);
                if (index < 0) index = text.LastIndexOf(_lastFind, text.Length - 1, StringComparison.OrdinalIgnoreCase);
            }

            if (index < 0)
            {
                lblStatus.Text = "❌ 未找到: " + _lastFind;
                return;
            }

            txtOutput.Focus();
            txtOutput.Select(index, _lastFind.Length);
            txtOutput.ScrollToCaret();
            lblStatus.Text = "🔍 已定位到第 " + (CountLines(text.Substring(0, index)) ) + " 行";
        }

        private void FindInTree(bool forward)
        {
            var nodes = GetAllNodes();
            int startIndex = _findCursor != null ? nodes.IndexOf(_findCursor) : (forward ? -1 : nodes.Count);

            if (forward)
            {
                for (int i = startIndex + 1; i < nodes.Count; i++)
                {
                    if (NodeMatches(nodes[i], _lastFind)) { SelectNode(nodes[i]); return; }
                }
            }
            else
            {
                for (int i = startIndex - 1; i >= 0; i--)
                {
                    if (NodeMatches(nodes[i], _lastFind)) { SelectNode(nodes[i]); return; }
                }
            }

            lblStatus.Text = "❌ 未找到: " + _lastFind;
        }

        private void SelectNode(AntdUI.TreeItem node)
        {
            _findCursor = node;
            treeOutput.Focus(node);
            treeOutput.Select(node);
            lblStatus.Text = "✅ 找到匹配项";
        }

        private List<AntdUI.TreeItem> GetAllNodes()
        {
            var nodes = new List<AntdUI.TreeItem>();
            foreach (AntdUI.TreeItem node in treeOutput.Items) CollectNodes(node, nodes);
            return nodes;
        }

        private void CollectNodes(AntdUI.TreeItem node, List<AntdUI.TreeItem> nodes)
        {
            nodes.Add(node);
            foreach (AntdUI.TreeItem child in node.Sub) CollectNodes(child, nodes);
        }

        private bool NodeMatches(AntdUI.TreeItem node, string searchText)
        {
            return (node.Text != null && node.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                   (Convert.ToString(node.Tag) ?? string.Empty).IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        #endregion

        #region 树右键菜单

        private void TreeOutput_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            var menuItems = new List<AntdUI.IContextMenuStripItem>
            {
                new AntdUI.ContextMenuStripItem("📋 复制节点文本") { Tag = "copy_text" },
                new AntdUI.ContextMenuStripItem("📦 复制节点值") { Tag = "copy_value" },
                new AntdUI.ContextMenuStripItemDivider(),
                new AntdUI.ContextMenuStripItem("➕ 展开所有") { Tag = "expand_all" },
                new AntdUI.ContextMenuStripItem("➖ 折叠所有") { Tag = "collapse_all" },
                new AntdUI.ContextMenuStripItemDivider(),
                new AntdUI.ContextMenuStripItem("🔍 用节点内容查找") { Tag = "find_in_output" }
            };

            AntdUI.ContextMenuStrip.open(treeOutput, OnContextMenuItemClick, menuItems.ToArray());
        }

        private void OnContextMenuItemClick(AntdUI.ContextMenuStripItem item)
        {
            switch (item.Tag == null ? null : item.Tag.ToString())
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
                    UseSelectedNodeAsFindText();
                    break;
            }
        }

        private void CopyTreeNodeText()
        {
            var selectedNode = treeOutput.SelectItem;
            if (selectedNode == null) return;
            lblStatus.Text = ClipboardHelper.TrySetText(selectedNode.Text ?? string.Empty)
                ? "✅ 已复制节点文本"
                : "❌ 复制失败：剪贴板被其他程序占用";
        }

        private void CopyTreeNodeValue()
        {
            var selectedNode = treeOutput.SelectItem;
            if (selectedNode == null) return;
            var value = selectedNode.Tag == null ? (selectedNode.Text ?? string.Empty) : selectedNode.Tag.ToString();
            lblStatus.Text = ClipboardHelper.TrySetText(value)
                ? "✅ 已复制节点值"
                : "❌ 复制失败：剪贴板被其他程序占用";
        }

        private void UseSelectedNodeAsFindText()
        {
            var selectedNode = treeOutput.SelectItem;
            if (selectedNode == null) return;

            var searchText = selectedNode.Text ?? Convert.ToString(selectedNode.Tag) ?? string.Empty;
            if (string.IsNullOrEmpty(searchText)) return;

            txtFind.Text = searchText;
            lblStatus.Text = "🔍 已填入查找框，回车即可定位";
        }

        #endregion

        #region 导出模型类

        private void TabControl1_SelectedIndexChanged(object sender, AntdUI.IntEventArgs e)
        {
            if (e.Value == 2) GenerateClassFromInput();
        }

        private void Select1_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenerateClassFromInput();
        }

        private LanguageType SelectedLanguage()
        {
            return string.Equals(select1.Text, "Java", StringComparison.OrdinalIgnoreCase)
                ? LanguageType.Java
                : LanguageType.CSharp;
        }

        private void GenerateClassFromInput()
        {
            try
            {
                string inputText = txtInput.Text;
                if (string.IsNullOrWhiteSpace(inputText)) inputText = txtOutput.Text;

                if (string.IsNullOrWhiteSpace(inputText))
                {
                    input1.Text = "// 请在左侧输入或先格式化 JSON / XML 数据";
                    lblStatus.Text = "⚠️ 请输入 JSON 或 XML 数据";
                    return;
                }

                if (inputText.Length > ExportGenerateLimit)
                {
                    input1.Text = "// 内容过大（" + FormatLength(inputText.Length) + "），已跳过模型类生成";
                    lblStatus.Text = "⚠️ 内容过大，已跳过生成";
                    return;
                }

                var language = SelectedLanguage();
                inputText = inputText.Trim();
                string className = ExtractClassName(inputText) ?? "GeneratedClass";

                string generatedCode = ClassGenerator.GenerateClassFromInput(inputText, language, className);
                input1.Text = generatedCode;
                ApplyCodeHighlighting(language);

                lblStatus.Text = "✅ 已生成 " + select1.Text + " 类定义";
            }
            catch (Exception ex)
            {
                input1.Text = "// 生成错误: " + ex.Message;
                lblStatus.Text = "❌ 生成失败: " + ex.Message;
            }
        }

        private string ExtractClassName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            var trimmed = input.TrimStart();
            try
            {
                if (trimmed.StartsWith("{")) return "JsonModel";
                if (trimmed.StartsWith("[")) return "JsonList";
                if (trimmed.StartsWith("<"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(trimmed,
                        @"<([a-zA-Z][^\s\/>]*)",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo
                            .ToTitleCase(match.Groups[1].Value);
                    }
                }
            }
            catch (Exception)
            {
                // 提取失败时使用默认类名
            }
            return null;
        }

        #endregion

        #region 工具栏提示

        /// <summary>
        /// 旧实现为每个按钮 new 一个 TooltipComponent，这里复用一个实例。
        /// </summary>
        private void BindButtonWithToolTip(Control parent)
        {
            _tooltip.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            _tooltip.ArrowAlign = AntdUI.TAlign.Bottom;

            foreach (Control control in parent.Controls)
            {
                var tip = TipFor(control.Name);
                if (tip != null) _tooltip.SetTip(control, tip);
            }
        }

        private static string TipFor(string name)
        {
            switch (name)
            {
                case "btnPretty": return "格式化 (Ctrl+E)";
                case "btnMinify": return "压缩 (Ctrl+M)";
                case "btnDetect": return "自动检测格式 (F5)";
                case "btnCopy": return "复制结果";
                case "btnOpen": return "打开文件 (Ctrl+O)";
                case "btnSave": return "保存结果 (Ctrl+S)";
                case "btnWrap": return "自动换行";
                case "btnClear": return "清空输入与结果";
                case "btnExpandAll": return "展开所有节点";
                case "btnCollapseAll": return "折叠所有节点";
                case "btnFindPrev": return "查找上一个";
                case "btnFindNext": return "查找下一个 (回车)";
                case "btnFont": return "字体设置";
                default: return null;
            }
        }

        #endregion
    }
}
