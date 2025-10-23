using System;

namespace PrettyText.TextFormatters
{
    public class PlainTextFormatter : ITextFormatter
    {
        public string Name { get { return "Plain"; } }

        public bool CanHandle(string input)
        {
            return true;
        }

        public string FormatPretty(string input)
        {
            return input ?? string.Empty;
        }

        public string FormatMinified(string input)
        {
            return input ?? string.Empty;
        }
    }
}


