using System;
using System.IO;
using System.Text;
using System.Xml;

namespace PrettyText.TextFormatters
{
    public class HtmlTextFormatter : ITextFormatter
    {
        public string Name { get { return "HTML"; } }

        public bool CanHandle(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            var t = input.TrimStart();
            return t.StartsWith("<") && (t.Contains("<html") || t.Contains("<div") || t.Contains("<body") || t.Contains("<head") || t.Contains("<span"));
        }

        public string FormatPretty(string input)
        {
            // 通过 XmlWriter 进行缩进（对 XHTML/可解析片段效果最好）
            var safe = input;
            try
            {
                var settings = new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = "\n", NewLineHandling = NewLineHandling.Replace, OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment };
                using (var sw = new StringWriter())
                using (var xw = XmlWriter.Create(sw, settings))
                {
                    xw.WriteRaw(safe);
                    xw.Flush();
                    return sw.ToString();
                }
            }
            catch
            {
                // 回退：简单换行+缩进处理（不严格）
                return SimplePrettyHtml(safe);
            }
        }

        public string FormatMinified(string input)
        {
            // 去除多余空白
            var s = input.Replace("\r\n", "\n");
            var sb = new StringBuilder(s.Length);
            bool inSpace = false;
            foreach (var ch in s)
            {
                if (char.IsWhiteSpace(ch))
                {
                    if (!inSpace) { sb.Append(' '); inSpace = true; }
                }
                else
                {
                    sb.Append(ch);
                    inSpace = false;
                }
            }
            return sb.ToString().Trim();
        }

        private static string SimplePrettyHtml(string html)
        {
            var s = html.Replace("<", "\n<").Replace(">", ">\n");
            var lines = s.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            int depth = 0;
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.StartsWith("</")) depth = Math.Max(0, depth - 1);
                sb.Append(new string(' ', depth * 2)).AppendLine(line);
                if (line.StartsWith("<") && !line.StartsWith("</") && !line.EndsWith("/>") && !line.Contains("</")) depth++;
            }
            return sb.ToString();
        }
    }
}


