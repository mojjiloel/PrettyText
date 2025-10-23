using System;
using System.Collections.Generic;
using System.Linq;

namespace PrettyText.TextFormatters
{
    public static class FormatterRegistry
    {
        private static readonly List<ITextFormatter> _formatters = new List<ITextFormatter>();

        static FormatterRegistry()
        {
            // Register built-ins
            _formatters.Add(new JsonTextFormatter());
            _formatters.Add(new XmlTextFormatter());
            _formatters.Add(new PlainTextFormatter());
            _formatters.Add(new YamlTextFormatter());
            _formatters.Add(new CsvTextFormatter());
            _formatters.Add(new HtmlTextFormatter());
        }

        public static IEnumerable<ITextFormatter> GetAll()
        {
            return _formatters;
        }

        public static void Register(ITextFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            // Replace existing by name
            var existing = _formatters.FindIndex(f => string.Equals(f.Name, formatter.Name, StringComparison.OrdinalIgnoreCase));
            if (existing >= 0) _formatters[existing] = formatter; else _formatters.Add(formatter);
        }

        public static ITextFormatter Resolve(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return new PlainTextFormatter();
            // Prioritize confident handlers first
            foreach (var f in _formatters)
            {
                try
                {
                    if (f.CanHandle(input)) return f;
                }
                catch
                {
                    // ignore probe errors
                }
            }
            return new PlainTextFormatter();
        }
    }
}


