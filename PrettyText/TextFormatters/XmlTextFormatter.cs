using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;

namespace PrettyText.TextFormatters
{
    public class XmlTextFormatter : ITextFormatter
    {
        public string Name { get { return "XML"; } }

        public bool CanHandle(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            var trimmed = input.TrimStart();
            if (!(trimmed.StartsWith("<") && trimmed.Contains(">"))) return false;
            try
            {
                var xml = LoadXmlSecure(NormalizeEscapes(input));
                return xml != null;
            }
            catch
            {
                return false;
            }
        }

        public string FormatPretty(string input)
        {
            var originalInput = NormalizeEscapes(input);
            var xml = LoadXmlSecure(originalInput);
            xml.PreserveWhitespace = false;
                    
            // 保留原始的XML声明
            string originalDeclaration = ExtractXmlDeclaration(originalInput);
                    
            var settings = new XmlWriterSettings { 
                Indent = true, 
                IndentChars = "  ", 
                NewLineChars = "\n", 
                NewLineHandling = NewLineHandling.Replace, 
                OmitXmlDeclaration = true  // 我们将手动添加原始声明
            };
            using (var sw = new StringWriter())
            using (var xw = XmlWriter.Create(sw, settings))
            {
                xml.Save(xw);
                xw.Flush();
                string formattedContent = sw.ToString();
                        
                // 重新添加原始的XML声明
                if (!string.IsNullOrEmpty(originalDeclaration))
                {
                    return originalDeclaration + "\n" + formattedContent;
                }
                return formattedContent;
            }
        }
        
        public string FormatMinified(string input)
        {
            var originalInput = NormalizeEscapes(input);
            var xml = LoadXmlSecure(originalInput);
            xml.PreserveWhitespace = false;
                    
            // 保留原始的XML声明
            string originalDeclaration = ExtractXmlDeclaration(originalInput);
                    
            var settings = new XmlWriterSettings { 
                Indent = false, 
                NewLineHandling = NewLineHandling.None, 
                OmitXmlDeclaration = true  // 我们将手动添加原始声明
            };
            using (var sw = new StringWriter())
            using (var xw = XmlWriter.Create(sw, settings))
            {
                xml.Save(xw);
                xw.Flush();
                string minifiedContent = sw.ToString();
                        
                // 重新添加原始的XML声明
                if (!string.IsNullOrEmpty(originalDeclaration))
                {
                    return originalDeclaration + "\n" + minifiedContent;
                }
                return minifiedContent;
            }
        }

        private static XmlDocument LoadXmlSecure(string xmlContent)
        {
            var settings = new XmlReaderSettings();
#if NET40
            settings.DtdProcessing = DtdProcessing.Prohibit;
#else
            settings.DtdProcessing = DtdProcessing.Prohibit;
#endif
            settings.XmlResolver = null;
            using (var sr = new StringReader(xmlContent))
            using (var xr = XmlReader.Create(sr, settings))
            {
                var doc = new XmlDocument();
                doc.XmlResolver = null; // extra safeguard
                doc.Load(xr);
                return doc;
            }
        }

        private static string NormalizeEscapes(string input)
        {
            var trimmed = input.Trim();
            if ((trimmed.StartsWith("\"") && trimmed.EndsWith("\"")) || (trimmed.StartsWith("'") && trimmed.EndsWith("'")))
            {
                trimmed = trimmed.Substring(1, trimmed.Length - 2);
            }
            trimmed = trimmed.Replace("\\\"", "\"");
            return trimmed;
        }
        
        /// <summary>
        /// 提取XML声明中的编码信息
        /// </summary>
        /// <param name="xmlContent">XML内容</param>
        /// <returns>编码名称，如果未找到则返回null</returns>
        private static string GetXmlEncoding(string xmlContent)
        {
            if (string.IsNullOrEmpty(xmlContent)) return null;
            
            var trimmed = xmlContent.TrimStart();
            if (!trimmed.StartsWith("<?xml")) return null;
            
            // 查找encoding属性
            var encodingMatch = System.Text.RegularExpressions.Regex.Match(trimmed, 
                @"encoding\s*=\s*[""']([^""']*)[""']", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            return encodingMatch.Success ? encodingMatch.Groups[1].Value : null;
        }
        
        /// <summary>
        /// 提取完整的XML声明
        /// </summary>
        /// <param name="xmlContent">XML内容</param>
        /// <returns>XML声明，如果未找到则返回null</returns>
        private static string ExtractXmlDeclaration(string xmlContent)
        {
            if (string.IsNullOrEmpty(xmlContent)) return null;
            
            var trimmed = xmlContent.TrimStart();
            if (!trimmed.StartsWith("<?xml")) return null;
            
            // 查找整个XML声明
            var declarationMatch = System.Text.RegularExpressions.Regex.Match(trimmed, 
                @"<\?xml[\s\S]*?\?>", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            return declarationMatch.Success ? declarationMatch.Value : null;
        }
    }
}


