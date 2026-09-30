using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PrettyText.Utils
{
    /// <summary>
    /// 基于正则的轻量语法着色。
    ///
    /// 与旧实现（AntdUI.Input.SetStyle 逐段调用、且每段都做一次 O(n) 的换行修正）相比：
    /// 1. 一次性关闭重绘，着色完成后再统一刷新，避免大文本时反复重绘；
    /// 2. 超过 <see cref="MaxHighlightLength"/> 时直接跳过着色，保证大文本仍然流畅；
    /// 3. 有上限的着色区间数量，避免病态输入导致界面假死。
    /// </summary>
    public static class SyntaxHighlighter
    {
        /// <summary>
        /// 超过该字符数不做语法着色（几 MB 的文本着色收益极低、耗时极高）
        /// </summary>
        public const int MaxHighlightLength = 200000;

        private const int MaxStyledRanges = 30000;
        private const int WM_SETREDRAW = 0x000B;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private sealed class Rule
        {
            public Regex Pattern;
            public Color Light;
            public Color Dark;
        }

        private static readonly Dictionary<string, List<Rule>> _rules =
            new Dictionary<string, List<Rule>>(StringComparer.OrdinalIgnoreCase);

        static SyntaxHighlighter()
        {
            _rules["JSON"] = new List<Rule>
            {
                RuleOf(@"\b(true|false|null)\b", Color.FromArgb(0, 0, 255), Color.FromArgb(86, 156, 214)),
                RuleOf(@"-?\b\d+(\.\d+)?([eE][+-]?\d+)?\b", Color.FromArgb(9, 134, 88), Color.FromArgb(181, 206, 168)),
                RuleOf(@"""([^""\\]|\\.)*""", Color.FromArgb(163, 21, 21), Color.FromArgb(206, 145, 120))
            };

            _rules["XML"] = new List<Rule>
            {
                RuleOf(@"<[^>]*>", Color.FromArgb(128, 0, 0), Color.FromArgb(86, 156, 214)),
                RuleOf(@"\s+(\w+(?==))", Color.FromArgb(255, 0, 0), Color.FromArgb(156, 220, 254)),
                RuleOf(@"""([^""]*)""", Color.FromArgb(0, 0, 255), Color.FromArgb(206, 145, 120)),
                RuleOf(@"<!--[\s\S]*?-->", Color.FromArgb(0, 128, 0), Color.FromArgb(106, 153, 85))
            };

            var csharp = new List<Rule>
            {
                RuleOf(@"\b(public|private|protected|internal|class|interface|struct|enum|void|int|double|float|bool|string|object|var|dynamic|if|else|for|foreach|while|do|switch|case|break|continue|return|new|this|base|using|namespace|static|const|readonly|virtual|override|abstract|sealed|partial|ref|out|params|try|catch|finally|throw|yield|await|async|get|set|add|remove|null|true|false)\b",
                    Color.FromArgb(0, 0, 255), Color.FromArgb(86, 156, 214)),
                RuleOf(@"\b(String|Int32|Double|Boolean|DateTime|List|Dictionary|IEnumerable|IList|IDictionary|Attributes?|JsonProperty|XmlElement)\b",
                    Color.FromArgb(43, 145, 175), Color.FromArgb(78, 201, 176)),
                RuleOf(@"//[^\n]*", Color.FromArgb(0, 128, 0), Color.FromArgb(106, 153, 85)),
                RuleOf(@"""([^""\\]|\\.)*""", Color.FromArgb(163, 21, 21), Color.FromArgb(206, 145, 120))
            };
            _rules["C#"] = csharp;
            _rules["CSHARP"] = csharp;

            var java = new List<Rule>
            {
                RuleOf(@"\b(public|private|protected|class|interface|void|int|long|double|float|boolean|String|Object|if|else|for|while|do|switch|case|break|continue|return|new|this|import|package|static|final|abstract|extends|implements|try|catch|finally|throw|throws|null|true|false)\b",
                    Color.FromArgb(0, 0, 255), Color.FromArgb(86, 156, 214)),
                RuleOf(@"\b(System|ArrayList|HashMap|List|Map|Date|Calendar|Override)\b",
                    Color.FromArgb(43, 145, 175), Color.FromArgb(78, 201, 176)),
                RuleOf(@"//[^\n]*", Color.FromArgb(0, 128, 0), Color.FromArgb(106, 153, 85)),
                RuleOf(@"""([^""\\]|\\.)*""", Color.FromArgb(163, 21, 21), Color.FromArgb(206, 145, 120))
            };
            _rules["JAVA"] = java;
            _rules["Java"] = java;
        }

        private static Rule RuleOf(string pattern, Color light, Color dark)
        {
            return new Rule
            {
                Pattern = new Regex(pattern, RegexOptions.Multiline | RegexOptions.Compiled),
                Light = light,
                Dark = dark
            };
        }

        /// <summary>
        /// 重新着色。返回 false 表示因文本过大或没有对应规则而只应用了基础色。
        /// </summary>
        public static bool Apply(RichTextBox box, string format, bool isLight)
        {
            if (box == null || box.IsDisposed) return false;

            string text = box.Text ?? string.Empty;
            List<Rule> rules;
            if (!_rules.TryGetValue((format ?? string.Empty).Trim(), out rules)) rules = null;

            int selStart = box.SelectionStart;
            int selLength = box.SelectionLength;
            bool redrawDisabled = false;

            if (box.IsHandleCreated)
            {
                SendMessage(box.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
                redrawDisabled = true;
            }
            box.SuspendLayout();

            bool styled = false;
            try
            {
                if (text.Length == 0) return false;

                // 基础色（同时清掉上一次的着色）
                box.SelectAll();
                box.SelectionColor = box.ForeColor;
                box.SelectionStart = 0;
                box.SelectionLength = 0;

                if (rules == null || text.Length > MaxHighlightLength) return false;

                int ranges = 0;
                foreach (var rule in rules)
                {
                    var color = isLight ? rule.Light : rule.Dark;
                    foreach (Match match in rule.Pattern.Matches(text))
                    {
                        if (match.Length <= 0) continue;
                        box.Select(match.Index, match.Length);
                        box.SelectionColor = color;
                        if (++ranges >= MaxStyledRanges) break;
                    }
                    if (ranges >= MaxStyledRanges) break;
                }
                styled = true;
            }
            catch (Exception)
            {
                // 着色失败不影响正常使用
            }
            finally
            {
                int start = Math.Min(selStart, box.TextLength);
                box.SelectionStart = start;
                box.SelectionLength = Math.Min(selLength, box.TextLength - start);
                box.ResumeLayout();

                if (redrawDisabled)
                {
                    SendMessage(box.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
                    box.Invalidate();
                }
            }

            return styled;
        }
    }
}
