using System;

namespace PrettyText.TextFormatters
{
    public interface ITextFormatter
    {
        string Name { get; }

        bool CanHandle(string input);

        string FormatPretty(string input);

        string FormatMinified(string input);
    }
}


