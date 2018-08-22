using System;

namespace Carubbi.ExtendedWebBrowser
{
    internal class ScriptError
    {
        public ScriptError(Uri url, string description, int lineNumber)
        {
            Url = url;
            Description = description;
            LineNumber = lineNumber;
        }

        public int LineNumber { get; }

        public string Description { get; }

        public Uri Url { get; }
    }
}