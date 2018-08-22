using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Carubbi.ExtendedWebBrowser
{
    /// <summary>
    ///     Used in the new navigation events
    /// </summary>
    public class BrowserExtendedNavigatingEventArgs : CancelEventArgs
    {
        /// <summary>
        ///     Creates a new instance of WebBrowserExtendedNavigatingEventArgs
        /// </summary>
        /// <param name="automation">Pointer to the automation object of the browser</param>
        /// <param name="url">The URL to go to</param>
        /// <param name="frame">The name of the frame</param>
        /// <param name="navigationContext">The new window flags</param>
        public BrowserExtendedNavigatingEventArgs(object automation, Uri url, string frame,
            UrlContext navigationContext)
        {
            Url = url;
            Frame = frame;
            NavigationContext = navigationContext;
            AutomationObject = automation;
        }

        /// <summary>
        ///     The URL to navigate to
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public Uri Url { get; }

        /// <summary>
        ///     The name of the frame to navigate to
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string Frame { get; }

        /// <summary>
        ///     The flags when opening a new window
        /// </summary>
        public UrlContext NavigationContext { get; }

        /// <summary>
        ///     The pointer to ppDisp
        /// </summary>
        public object AutomationObject { get; set; }
    }
}