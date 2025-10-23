using System;
using System.Text;

namespace PrettyText.TextFormatters
{
    // 轻量 YAML 判定与格式化（.NET 4.0 无第三方库场景下：仅做缩进规范化与基本校验）
    public class YamlTextFormatter : ITextFormatter
    {
        public string Name { get { return "YAML"; } }

        public bool CanHandle(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            var t = input.TrimStart();
            // 简单特征：包含冒号的键值、列表项前缀 - 
            return t.Contains(": ") || t.StartsWith("-") || t.Contains("- ");
        }

        public string FormatPretty(string input)
        {
            return NormalizeIndent(input);
        }

        public string FormatMinified(string input)
        {
            // YAML 不建议压缩，返回原文
            return input ?? string.Empty;
        }

        private static string NormalizeIndent(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            var lines = text.Replace("\r\n", "\n").Split('\n');
            var sb = new StringBuilder();
            foreach (var raw in lines)
            {
                var line = raw.Replace("\t", "  ");
                sb.AppendLine(line.TrimEnd());
            }
            return sb.ToString();
        }
    }
}


