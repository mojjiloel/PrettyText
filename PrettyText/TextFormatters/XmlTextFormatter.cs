using System;
using System.IO;
using System.Text;
using System.Xml;

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
            var xml = LoadXmlSecure(NormalizeEscapes(input));
            xml.PreserveWhitespace = false;
            var settings = new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = "\n", NewLineHandling = NewLineHandling.Replace, OmitXmlDeclaration = false };
            using (var sw = new StringWriter())
            using (var xw = XmlWriter.Create(sw, settings))
            {
                xml.Save(xw);
                xw.Flush();
                return sw.ToString();
            }
        }

        public string FormatMinified(string input)
        {
            var xml = LoadXmlSecure(NormalizeEscapes(input));
            xml.PreserveWhitespace = false;
            var settings = new XmlWriterSettings { Indent = false, NewLineHandling = NewLineHandling.None, OmitXmlDeclaration = false };
            using (var sw = new StringWriter())
            using (var xw = XmlWriter.Create(sw, settings))
            {
                xml.Save(xw);
                xw.Flush();
                return sw.ToString();
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
    }
}


