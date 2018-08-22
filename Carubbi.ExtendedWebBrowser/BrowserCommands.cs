using System;

namespace Carubbi.ExtendedWebBrowser
{
    /// <summary>
    ///     Este enum representa os comandos possíveis do navegador
    /// </summary>
    [Flags]
    public enum BrowserCommands
    {
        /// <summary>
        ///     Usado quando nenhum comando está disponível
        /// </summary>
        None = 0,
        Home = 1,
        Search = 2,
        Back = 4,
        Forward = 8,
        Stop = 16,
        Reload = 32,
        Print = 64,
        PrintPreview = 128
    }
}