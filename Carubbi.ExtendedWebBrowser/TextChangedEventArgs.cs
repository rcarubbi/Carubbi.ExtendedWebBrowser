using System;

namespace Carubbi.ExtendedWebBrowser
{
    public class TextChangedEventArgs : EventArgs
    {
        public TextChangedEventArgs(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }
}