using System;
using System.Drawing;
using System.Windows.Forms;

namespace PrettyText.Controls
{
    /// <summary>
    /// 面向大文本的编辑器控件。
    ///
    /// 原生 RichTextBox 的文本由系统维护，插入 10MB 文本约 1 秒、额外内存约 20MB；
    /// 而 AntdUI.Input 采用逐字符缓存的自绘实现，插入 3MB 需要 8 秒、额外内存近 900MB，
    /// 这正是"左侧输入框接收不了大文本"的根因，因此这里改用原生富文本控件。
    /// 同时保留了占位符与主题配色，外观与原控件保持一致。
    /// </summary>
    public class CodeEditor : RichTextBox
    {
        private const int WM_PAINT = 0x000F;

        private string _placeholder = string.Empty;
        private Color _placeholderColor = Color.FromArgb(160, 160, 160);

        public CodeEditor()
        {
            BorderStyle = BorderStyle.None;
            WordWrap = true;
            ScrollBars = RichTextBoxScrollBars.Both;
            HideSelection = false;
            DetectUrls = false;
            AcceptsTab = true;
            EnableAutoDragDrop = false;
            MaxLength = int.MaxValue;
            Font = new Font("Consolas", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
        }

        /// <summary>
        /// 文本为空时显示的占位提示
        /// </summary>
        public string PlaceholderText
        {
            get { return _placeholder; }
            set
            {
                _placeholder = value ?? string.Empty;
                Invalidate();
            }
        }

        /// <summary>
        /// 按主题刷新配色
        /// </summary>
        public void ApplyTheme(Color backColor, Color foreColor, Color placeholderColor)
        {
            BackColor = backColor;
            ForeColor = foreColor;
            _placeholderColor = placeholderColor;
            Invalidate();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (TextLength == 0) Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg != WM_PAINT) return;
            if (_placeholder.Length == 0 || TextLength > 0) return;

            using (var g = Graphics.FromHwnd(Handle))
            using (var brush = new SolidBrush(_placeholderColor))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                g.DrawString(_placeholder, Font, brush, 2f, 3f);
            }
        }
    }
}
