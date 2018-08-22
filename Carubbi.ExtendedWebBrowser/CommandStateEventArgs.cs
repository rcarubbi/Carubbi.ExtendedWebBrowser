using System;

namespace Carubbi.ExtendedWebBrowser
{
    /// <summary>
    ///     Represents event information for the main form, when the command state of the active browser changes
    /// </summary>
    public class CommandStateEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="CommandStateEventArgs" /> class
        /// </summary>
        /// <param name="commands">A list of commands that are available</param>
        public CommandStateEventArgs(BrowserCommands commands)
        {
            BrowserCommands = commands;
        }

        /// <summary>
        ///     Gets a list of commands that are available
        /// </summary>
        public BrowserCommands BrowserCommands { get; }
    }
}