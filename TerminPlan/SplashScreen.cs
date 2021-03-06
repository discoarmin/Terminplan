﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SplashScreen.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Startbildschirm für Terminplan.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  07.01.17  br      Grundversion
//                  19.01.17  br      Äbänderung au Thread
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable ParameterHidesMember
namespace Terminplan
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Resources;
    using Infragistics.Win;
    using TerminPlan;
    using Resources = Properties.Resources;

    /// <summary>Klasse für den Begrüßungsbildschirm</summary>
    public sealed partial class SplashScreen : Form
    {
        #region Private Members
        private readonly ResourceManager rm = Resources.ResourceManager;

        private ContainerControl sender;                                        // das aufrufende Element

        #endregion Private Members

        #region Konstructor    
        /// <summary>Initialisiert eine neue Instanz der <see cref="SplashScreen"/> Klasse.</summary>
        public SplashScreen()
        {
            DoubleBuffered = true;

            // Die Begrüßungsanzeige sollte nicht anwählbar sein
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.StandardClick, false);

            // Für Windows Form-Designer-Unterstützung erforderlich
            InitializeComponent();
            InitializeUi();
        }

        ///// <summary>Initialisiert eine neue Instanz der <see cref="SplashScreen"/> Klasse.</summary>
        ///// <param name="msender">Das aufrufende Element.</param>
        ///// <param name="msenderDelegate">Delegate zur Kommunikation mit dem Hauptfenster</param>
        //public SplashScreen(ContainerControl msender,
        //                    Delegate msenderDelegate)
        //{
        //    // Übergabeparameter speichern
        //    this.Sender = msender;
        //    this.SenderDelegate = msenderDelegate;

        //    this.DoubleBuffered = true;

        //    // Die Begrüßungsanzeige sollte nicht anwählbar sein
        //    this.SetStyle(ControlStyles.Selectable, false);
        //    this.SetStyle(ControlStyles.StandardClick, false);

        //    // Für Windows Form-Designer-Unterstützung erforderlich
        //    this.InitializeComponent();
        //    this.InitializeUi();
        //}

        #endregion Konstructor

        #region Eigenschaften
        public ContainerControl Sender
        {
            private get
            {
                return sender;
            }
            set
            {
                sender = value;
            }
        }

        /// <summary>Holt oder setzt die Callback-Funktion </summary>
        /// <value>Die Callback-Funktion</value>
        public Delegate SenderDelegate { private get; set; }
        #endregion Eigenschaften

        #region Methoden
        #region CloseMe
        /// <summary> Schließt den Begrüßungsbildschirm </summary>
        public void CloseMe()
        {
            Close();
            Dispose();
        }
        #endregion CloseMe

        #region InitializeUi
        private void InitializeUi()
        {
            LocalizeStrings();
            HookEvents();
        }
        #endregion InitializeUi

        #region HookEvents
        private void HookEvents()
        {
            TerminPlanForm.InitializationStatusChanged += ApplicationInitializationStatusChanged;
        }
        #endregion HookEvents

        #region UnHookEvents
        private void UnHookEvents()
        {
            TerminPlanForm.InitializationStatusChanged -= ApplicationInitializationStatusChanged;
        }
        #endregion UnHookEvents

        #region UpdateStatusLabel
        private void UpdateStatusLabel(string status)
        {
            lblStatus.Text = status;
        }
        #endregion UpdateStatusLabel

        #region LocalizeStrings
        private void LocalizeStrings()
        {
            lblAppName.Text = AboutControl.ApplicationName;
            lblVersion.Text = string.Format(@" v {0}", AboutControl.Version);
            var resourceManager = rm;
            if (resourceManager != null)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                lblStatus.Text = string.Format(resourceManager.GetString(@"Application_Starting"), Resources.Title);
            }
        }
        #endregion LocalizeStrings

        #endregion Private Methods

        #region Delegates
        private delegate void UpdateStringDelegate(string text);
        #endregion Delegates

        #region Event Handlers

        #region ApplicationInitializationStatusChanged
        // ReSharper disable once ParameterHidesMember
        private void ApplicationInitializationStatusChanged(object sender, InitializationStatusChangedEventArgs e)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new UpdateStringDelegate(UpdateStatusLabel), new object[] { e.Status });
            }
            else
            {
                UpdateStatusLabel(e.Status);
            }
        }

        #endregion ApplicationInitializationStatusChanged

        //#region ApplicationInitializationComplete
        //private void ApplicationInitializationComplete(object sender, EventArgs e)
        //{
        //    if (this.Sender != null && this.SenderDelegate != null)
        //    {
        //        //this.Sender.BeginInvoke(this.SenderDelegate);
        //        this.Sender.BeginInvoke(new MethodInvoker(this.CloseMe));
        //    }
        //    //if (this.InvokeRequired)
        //    //{
        //    //    try
        //    //    {
        //    //        //this.Close();
        //    //        //this.Invoke(new MethodInvoker(this.CloseMe));
        //    //        this.Sender.Invoke(new MethodInvoker(this.CloseMe));
        //    //    }
        //    //    catch
        //    //    {
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    this.Close();
        //    //}

        //    //Application.DoEvents();
        //}
        //#endregion ApplicationInitializationComplete
        #endregion Event Handlers

        #region Base class overrides
        #region SetVisibleCore
        protected override void SetVisibleCore(bool visible)
        {
            if (visible)
            {
                // The Application.Run call will force the visible property to
                // true but that will cause the splash screen to be activated
                // thereby deactivating other windows before the app has fully
                // loaded. In an effort to prevent this, we'll use apis to show
                // the splash screen without activating it. Note, since we are
                // showing the form, we have to do the centering and so I removed
                // the FormStartPosition property setting.
                //
                var topMost = true;
                //var topMost = false;
                var formRect = new Rectangle(Point.Empty, Size);
                var screenRect = Utilities.ScreenFromPoint(Cursor.Position).Bounds;
                DrawUtility.AdjustHAlign(HAlign.Center, ref formRect, screenRect);
                DrawUtility.AdjustVAlign(VAlign.Middle, ref formRect, screenRect);
                var location = formRect.Location;

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                var insertAfter = topMost ? NativeWindowMethods.HWND_TOPMOST : IntPtr.Zero;

                Form form = this;
                NativeWindowMethods.SetWindowPos(form.Handle, insertAfter, location.X, location.Y, 0, 0, NativeWindowMethods.SWP_NOACTIVATE | NativeWindowMethods.SWP_NOSIZE);
                NativeWindowMethods.ShowWindow(form.Handle, NativeWindowMethods.SW_SHOWNA);
            }

            base.SetVisibleCore(visible);
        }
        #endregion SetVisibleCore

        #region WndProc
        protected override void WndProc(ref Message message)
        {
            // We don't want the splash screen to be clickable and if possible
            // prevent its activation.
            switch (message.Msg)
            {
                case NativeWindowMethods.WM_MOUSEACTIVATE:
                    message.Result = (IntPtr)NativeWindowMethods.MA_NOACTIVATE;
                    break;
                case NativeWindowMethods.WM_NCHITTEST:
                    message.Result = (IntPtr)NativeWindowMethods.HTTRANSPARENT;
                    break;
                default:
                    Debug.WriteLine(message.ToString(), DateTime.Now.ToString("hh:mm:ss:ffffff"));
                    base.WndProc(ref message);
                    break;
            }
        }
        #endregion //WndProc

        #endregion Base class overrides

        #region Event-related
        #region Delegates

        /// <summary>
        /// Event handler for the <see cref="StyleSetList.StyleSetSelectionCommitted"/> event
        /// </summary>
        internal delegate void InitializationStatusChangedEventHandler(object sender, InitializationStatusChangedEventArgs e);

        #endregion Delegates

        #region Event Args

        #region InitializationStatusChangedEventArgs
        internal class InitializationStatusChangedEventArgs : EventArgs
        {
            private bool showProgressBar = false;
            private int percentComplete = 100;
            private string status = null;

            public InitializationStatusChangedEventArgs(string status) : this(status, false, 0) { }

            public InitializationStatusChangedEventArgs(string status, bool showProgressBar, int percentComplete)
            {
                this.status = status;
                this.showProgressBar = showProgressBar;
                this.percentComplete = percentComplete;
            }

            public string Status
            {
                get { return status; }
            }

            public bool ShowProgressBar
            {
                get { return showProgressBar; }
            }

            public int PercentComplete
            {
                get { return percentComplete; }
            }
        }
        #endregion InitializationStatusChangedEventArgs

        #endregion Event Args

        #endregion //Event-related
    }
}
