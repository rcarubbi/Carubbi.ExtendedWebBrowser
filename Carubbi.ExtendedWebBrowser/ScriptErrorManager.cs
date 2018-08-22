using System;
using System.Windows.Forms;

namespace Carubbi.ExtendedWebBrowser
{
    internal class ScriptErrorManager
    {
        private static readonly object lockObject = new object();

        private static ScriptErrorManager _instance;

        private ScriptErrorManager()
        {
            ScriptErrors = new NotifyCollection<ScriptError>();
        }

        public static ScriptErrorManager Instance
        {
            get
            {
                if (_instance == null)
                    lock (lockObject)
                    {
                        if (_instance == null) _instance = new ScriptErrorManager();
                    }

                return _instance;
            }
        }

        public NotifyCollection<ScriptError> ScriptErrors { get; }

        public Form ErrorForm { get; set; }

        public bool ShowErrors { get; set; }

        public void RegisterScriptError(Uri url, string description, int lineNumber)
        {
            ScriptErrors.Add(new ScriptError(url, description, lineNumber));
            if (ShowErrors) ShowWindow();
        }

        public void ShowWindow()
        {
            if (ErrorForm == null || ErrorForm.IsDisposed)
                ErrorForm = Activator.CreateInstance(ErrorForm.GetType()) as Form;
            ErrorForm.Show();
        }
    }
}