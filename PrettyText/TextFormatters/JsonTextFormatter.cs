using System;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace PrettyText.TextFormatters
{
    public class JsonTextFormatter : ITextFormatter
    {
        public string Name { get { return "JSON"; } }

        public bool CanHandle(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            var trimmed = input.TrimStart();
            if (!(trimmed.StartsWith("{") || trimmed.StartsWith("["))) return false;
            try
            {
                // Use JavaScriptSerializer for .NET 4.0 compatibility
                var serializer = new JavaScriptSerializer();
                serializer.MaxJsonLength = int.MaxValue;
                serializer.RecursionLimit = 100;
                serializer.DeserializeObject(NormalizeEscapes(input));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string FormatPretty(string input)
        {
            object data = Parse(input);
            return SerializeWithIndent(data, 2);
        }

        public string FormatMinified(string input)
        {
            object data = Parse(input);
            return SerializeWithIndent(data, 0);
        }

        private static object Parse(string input)
        {
            var serializer = new JavaScriptSerializer();
            // 增加最大 JSON 长度限制,支持更大的 JSON
            serializer.MaxJsonLength = int.MaxValue;
            // 增加递归深度限制,支持深层嵌套
            serializer.RecursionLimit = 100;
            return serializer.DeserializeObject(NormalizeEscapes(input));
        }

        private static string NormalizeEscapes(string input)
        {
            // Attempt to recover common escaped sequences (e.g. \" wrapped or extra backslashes)
            var trimmed = input.Trim();
            if ((trimmed.StartsWith("\"") && trimmed.EndsWith("\"")) || (trimmed.StartsWith("'") && trimmed.EndsWith("'")))
            {
                trimmed = trimmed.Substring(1, trimmed.Length - 2);
            }
            // Replace double-escaped quotes \" -> " only when appears as escaped
            //trimmed = trimmed.Replace("\\\"", "\"");
            return trimmed;
        }

        private static string SerializeWithIndent(object data, int indent)
        {
            // JavaScriptSerializer doesn't support pretty print; implement manually
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            serializer.RecursionLimit = 100;
            var raw = serializer.Serialize(data);
            if (indent <= 0) return raw;
            return PrettyPrintJson(raw, indent);
        }

        private static string PrettyPrintJson(string json, int indentSize)
        {
            var sb = new StringBuilder();
            bool inString = false;
            int depth = 0;
            int i = 0;
            
            while (i < json.Length)
            {
                char c = json[i];
                
                // 处理转义字符
                if (c == '\\' && !inString)
                {
                    sb.Append(c);
                    i++;
                    continue;
                }
                
                if (c == '\\' && inString)
                {
                    sb.Append(c);
                    if (i + 1 < json.Length)
                    {
                        sb.Append(json[i + 1]);
                        i += 2;
                        continue;
                    }
                    i++;
                    continue;
                }
                
                // 切换字符串状态
                if (c == '"')
                {
                    inString = !inString;
                    sb.Append(c);
                    i++;
                    continue;
                }

                // 如果在字符串内,直接输出
                if (inString)
                {
                    sb.Append(c);
                    i++;
                    continue;
                }

                // 处理 JSON 结构字符
                switch (c)
                {
                    case '{':
                    case '[':
                        sb.Append(c);
                        depth++;
                        // 查看下一个非空白字符
                        int nextIdx = i + 1;
                        while (nextIdx < json.Length && char.IsWhiteSpace(json[nextIdx])) nextIdx++;
                        // 如果下一个不是结束符,则换行
                        if (nextIdx < json.Length && json[nextIdx] != '}' && json[nextIdx] != ']')
                        {
                            sb.Append('\n');
                            sb.Append(new string(' ', depth * indentSize));
                        }
                        break;
                        
                    case '}':
                    case ']':
                        // 检查上一个非空白字符
                        int prevIdx = i - 1;
                        while (prevIdx >= 0 && char.IsWhiteSpace(json[prevIdx])) prevIdx--;
                        bool needNewlineBefore = prevIdx >= 0 && json[prevIdx] != '{' && json[prevIdx] != '[';
                        
                        depth = Math.Max(0, depth - 1);
                        if (needNewlineBefore)
                        {
                            sb.Append('\n');
                            sb.Append(new string(' ', depth * indentSize));
                        }
                        sb.Append(c);
                        break;
                        
                    case ',':
                        sb.Append(c);
                        sb.Append('\n');
                        sb.Append(new string(' ', depth * indentSize));
                        break;
                        
                    case ':':
                        sb.Append(" : ");
                        break;
                        
                    default:
                        // 保留非空白字符
                        if (!char.IsWhiteSpace(c))
                        {
                            sb.Append(c);
                        }
                        break;
                }
                i++;
            }
            return sb.ToString();
        }
    }
}


