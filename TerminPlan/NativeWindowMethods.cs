// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NativeWindowsMethods.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Definition von API-Methoden.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  06.01.17  br      Grundversion
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable All
namespace TerminPlan
{
    using System;
    using System.Runtime.InteropServices;

    internal class NativeWindowMethods
    {
        #region Konstanten

        internal static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        internal const int SW_SHOWNA = 0x0008;
        internal const int SWP_NOACTIVATE = 0x0010;
        internal const int SWP_NOSIZE = 0x0001;
        internal const int WM_MOUSEACTIVATE = 0x0021;
        internal const int MA_NOACTIVATE = 3;
        internal const int WM_NCHITTEST = 0x84;
        internal const int HTTRANSPARENT = (-1);

        #endregion Konstanten

        #region APIs

        #region SetWindowPos

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32", ExactSpelling = true, CharSet = CharSet.Auto)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int x, int y, int cx, int cy, int flags);

        #endregion SetWindowPos

        #region ShowWindow

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32", ExactSpelling = true, CharSet = CharSet.Auto)]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        #endregion ShowWindow

        #endregion //APIs
    }
}
