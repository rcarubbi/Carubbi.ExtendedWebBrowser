using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using Carubbi.Web.Utils;
using Carubbi.WindowsAppHelper;

namespace Carubbi.ExtendedWebBrowser
{
    /// <summary>
    ///     Manages the tabs, and their contents
    /// </summary>
    public class ExtendedWebBrowserWindowManager : IWindowManager
    {
        private readonly TabControl _tabControl;

        public ExtendedWebBrowserWindowManager(TabControl tabControl)
        {
            _tabControl = tabControl;
        }

        /// <summary>
        ///     Closes the active tab
        /// </summary>
        public void Close(int index)
        {
            // Find the active page
            TabPage page = null;
            _tabControl.InvokeIfRequired(tc => page = (tc as TabControl).TabPages[index]);
            // Check wheter there is actually a page selected
            if (page != null)
                _tabControl.InvokeIfRequired(tc =>
                {
                    foreach (Control c in page.Controls)
                        if (c is Panel)
                            foreach (Control wb in c.Controls)
                                (wb as WebBrowser).DisposeBrowser();
                    page.Controls.Clear();

                    // Remove the page
                    (tc as TabControl).TabPages.Remove(page);
                    // Dispose the page (controls on the page are also disposed this way)
                    page.Dispose();
                });


            _tabControl.InvokeIfRequired(tc =>
            {
                var tabControl = tc as TabControl;
                if (tabControl.TabPages.Count == 0) tabControl.Visible = false;
            });
        }

        /// <summary>
        ///     Opens a new browser tab, and navigates to the home page
        /// </summary>
        /// <returns>The instance of the browser created</returns>
        public ExtendedWebBrowser New(string title)
        {
            return New(true, title);
        }

        /// <summary>
        ///     Opens a new browser tab
        /// </summary>
        /// <param name="navigateHome">true to immediately navigate to the homepage, otherwise false</param>
        /// <returns>The instance of the browser created</returns>
        // We cannot dispose the browsercontrol here
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]
        public ExtendedWebBrowser New(bool navigateHome, string title)
        {
            ExtendedWebBrowser webBrowser = null;


            _tabControl.InvokeIfRequired(tc =>
            {
                webBrowser = new ExtendedWebBrowser(title);
                var tabControl = tc as TabControl;
                // Create a new tab page
                var page = new TabPage {Text = title};


                // Create a new browser control
                var panel = new Panel();
                panel.Controls.Add(webBrowser);
                webBrowser.Dock = DockStyle.Fill;
                // Set the page as the Tag of the browser control, and vice-versa, this will come in handy later
                panel.Tag = page;
                page.Tag = panel;
                // Dock the browser control
                panel.Dock = DockStyle.Fill;
                // Add the browser control to the tab page
                page.Controls.Add(panel);
                if (navigateHome) (panel.Controls[0] as ExtendedWebBrowser).GoHome();
                // Wire some events
                (panel.Controls[0] as ExtendedWebBrowser).StatusTextChanged += WebBrowser_StatusTextChanged;
                (panel.Controls[0] as ExtendedWebBrowser).DocumentTitleChanged += WebBrowser_DocumentTitleChanged;
                (panel.Controls[0] as ExtendedWebBrowser).DocumentCompleted += WebBrowser_DocumentCompleted;
                (panel.Controls[0] as ExtendedWebBrowser).Quit += WebBrowser_Quit;

                // Add the new page to the tab control
                tabControl.TabPages.Add(page);
                tabControl.SelectedTab = page;
                tabControl.Visible = true;

                webBrowser = panel.Controls[0] as ExtendedWebBrowser;
            });


            return webBrowser;
        }

        public void Open(Uri url)
        {
            var browser = New(false, url.ToString());
            browser.Navigate(url);
        }


        public void SetTitleName(int tabIndex, string title)
        {
            _tabControl.InvokeIfRequired(tc => (tc as TabControl).TabPages[tabIndex].Text = title);
        }

        public string GetTitleName(int tabIndex)
        {
            var text = string.Empty;
            _tabControl.InvokeIfRequired(tc => text = (tc as TabControl).TabPages[tabIndex].Text);
            return text;
        }


        public event EventHandler<TextChangedEventArgs> StatusTextChanged;

        public void ChangeActiveBrowser(int index)
        {
            _tabControl.InvokeIfRequired(tc => (tc as TabControl).SelectTab(index));
        }

        public ExtendedWebBrowser ActiveBrowser
        {
            get
            {
                TabPage page = null;
                _tabControl.InvokeIfRequired(tc => page = (tc as TabControl).SelectedTab);
                if (page != null)
                {
                    var control = page.Tag as Panel;
                    if (control != null)
                        return control.Controls[0] as ExtendedWebBrowser;
                }

                return null;
            }
        }

        public event EventHandler<CommandStateEventArgs> CommandStateChanged;

        public ExtendedWebBrowser this[int index]
        {
            get
            {
                TabPage page = null;

                _tabControl.InvokeIfRequired(tc => page = (tc as TabControl).TabPages[index]);
                if (page != null)
                {
                    var control = page.Tag as Panel;
                    if (control != null)
                        return control.Controls[0] as ExtendedWebBrowser;
                }

                return null;
            }
        }

        public int LastTabIndex
        {
            get
            {
                var lastTabIndex = 0;
                _tabControl.InvokeIfRequired(tc => lastTabIndex = (tc as TabControl).TabCount - 1);
                return lastTabIndex;
            }
        }

        public void CloseAllTabs()
        {
            while (LastTabIndex > -1) Close(LastTabIndex);
        }


        private static Panel PanelFromBrowser(ExtendedWebBrowser browser)
        {
            // This is a little nasty. The Extended Web Browser is nested in 
            // a panel, wich is nested in the browser control

            // Since we want to avoid a NullReferenceException, do some checking

            // Check if we got a extended web browser
            if (browser == null)
                return null;

            // Check if it got a parent
            if (browser.Parent == null)
                return null;

            // Return the parent of the parent using a safe cast.
            return browser.Parent.Parent as Panel;
        }

        #region Events that cause the status of toolbar buttons or menu items to change

        protected void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            ((WebBrowser) sender).Document.Window.Error += Window_Error;
        }

        protected void Window_Error(object sender, HtmlElementErrorEventArgs e)
        {
            // Ignore the error and suppress the error dialog box. 
            e.Handled = true;
        }


        protected void WebBrowser_Quit(object sender, EventArgs e)
        {
            // This event is launched when window.close() is called from script
            var brw = sender as ExtendedWebBrowser;
            if (brw == null)
                return;
            // See which page it was on...
            var panel = PanelFromBrowser(brw);
            if (panel == null)
                return;

            var page = panel.Tag as TabPage;
            if (page == null)
                return;

            // We got a page, remove & dispose it.
            _tabControl.InvokeIfRequired(tc => (tc as TabControl).TabPages.Remove(page));
            page.Dispose();

            var tabCount = 0;
            _tabControl.InvokeIfRequired(tc => tabCount = (tc as TabControl).TabPages.Count);
            if (tabCount == 0)
                _tabControl.InvokeIfRequired(tc => tc.Visible = false);
        }

        protected void WebBrowser_DocumentTitleChanged(object sender, EventArgs e)
        {
            // Update the title of the tab page of the control.
            var ewb = sender as ExtendedWebBrowser;
            // Return if we got nothing (shouldn't happen)
            if (ewb == null) return;

            // This is a little nasty. The Extended Web Browser is nested in 
            // a panel, wich is nested in the browser control
            var panel = PanelFromBrowser(ewb);
            // If we got null, return
            if (panel == null) return;

            // The Tag of the BrowserControl should point to the TabPage
            var page = panel.Tag as TabPage;
            // If not, return
            if (page == null) return;

            // Update the tabPage
            // Keep it user-friendly, don't do those awful long web page titles 
            // in tabs and make sure the title is never empty

            var tabIndex = _tabControl.TabPages.IndexOf(page);

            var documentTitle = GetTitleName(tabIndex) ?? ewb.DocumentTitle;

            if (documentTitle.Length > 30) documentTitle = documentTitle.Substring(0, 30) + "...";
            page.Text = documentTitle;
            // Set the full title as a tooltip
            page.ToolTipText = ewb.DocumentTitle;
        }

        protected void WebBrowser_StatusTextChanged(object sender, EventArgs e)
        {
            // First, see if the active page is calling, or another page
            var ewb = sender as ExtendedWebBrowser;
            // Return if we got nothing (shouldn't happen)
            if (ewb == null) return;

            // This is a little nasty. The Extended Web Browser is nested in 
            // a panel, wich is nested in the browser control
            var panel = PanelFromBrowser(ewb);

            // The Tag of the BrowserControl should point to the TabPage
            var page = panel.Tag as TabPage;
            // If not, return
            if (page == null) return;

            // See if 'page' is the active page
            TabPage selectedTab = null;
            _tabControl.InvokeIfRequired(tc => selectedTab = (tc as TabControl).SelectedTab);
            if (selectedTab == page) OnStatusTextChanged(new TextChangedEventArgs(ewb.StatusText));
        }

        protected virtual void OnCommandStateChanged(CommandStateEventArgs e)
        {
            if (CommandStateChanged != null)
                CommandStateChanged(this, e);
        }

        /// <summary>
        ///     Raises the StatusTextChanged event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnStatusTextChanged(TextChangedEventArgs e)
        {
            if (StatusTextChanged != null)
                StatusTextChanged(this, e);
        }

        #endregion
    }
}