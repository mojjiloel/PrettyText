using System;
using System.Text;

namespace PrettyText.TextFormatters
{
    public class CsvTextFormatter : ITextFormatter
    {
        public string Name { get { return "CSV"; } }

        public bool CanHandle(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            var hasComma = input.IndexOf(',') >= 0;
            var hasLine = input.IndexOf('\n') >= 0 || input.IndexOf('\r') >= 0;
            return hasComma && hasLine;
        }

        public string FormatPretty(string input)
        {
            return NormalizeCsv(input);
        }

        public string FormatMinified(string input)
        {
            // 将多余空白去除
            return NormalizeCsv(input).Replace("\r\n", "\n");
        }

        private static string NormalizeCsv(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            var lines = text.Replace("\r\n", "\n").Split('\n');
            var sb = new StringBuilder();
            foreach (var raw in lines)
            {
                var line = raw.TrimEnd();
                sb.AppendLine(line);
            }
            return sb.ToString();
        }
    }
}


