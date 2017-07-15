// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StartForm.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   MIDI-Fenster zur Aufnahme des Terminplans und der Stammdaten.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  08.03.17  br      Grundversion
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Terminplan
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Forms;
    using Infragistics.Win.Misc;
    using Infragistics.Win.UltraWinTabbedMdi;

    public partial class StartForm : Form
    {
        #region Variablen

        /// <summary>Control zum Darstellen von MDI-Formulare als Tabs</summary>
        public UltraTabbedMdiManager TabManager;

        /// <summary>der ausgewählte Tab</summary>
        public MdiTab ActiveTab;

        /// <summary> das aktive Panel </summary>
        private UltraZoomPanel activeZoomPanel;

        /// <summary> Liste mit allen zoombaren Panels </summary>
        private List<UltraZoomPanel> zoomPanels;

        /// <summary> Formular mit den Stanndaten </summary>
        public StammDaten Fs;

        /// <summary> Formular mit dem Terminplan </summary>
        public TerminPlanForm Ft;

        #endregion Variablen

        #region Konstruktor

        /// <summary> Initialisiert eine neue Instanz der <see cref="StartForm"/> Klasse.
        /// </summary>
        public StartForm()
        {
            var culture = new CultureInfo("de-DE");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

            InitializeComponent();
        }

        #endregion Konstruktor

        #region InitializeZoomPanels

        private void InitializeZoomPanels()
        {
            this.zoomPanels = new List<UltraZoomPanel>() { this.Fs.ultraZoomPanelStammDaten, this.Ft.ultraZoomPanelTerminPlan };
        }

        #endregion InitializeZoomPanels

        /// <summary>
        /// Löst das <see cref="System.Windows.Forms.Form.Load" /> Ereignis aus.
        /// </summary>
        /// <param name="e">Ein <see cref="T:System.EventArgs" /> welches die Ereignisdaten enthält.</param>
        protected override void OnLoad(EventArgs e)
        {
            // Hier müssen die beiden Formulare geladen werden
            // a) Stammdaten
            this.Fs = new StammDaten { Tag = @"Stammdaten", Text = @"Stammdaten", MdiParent = this };

            // b) Formular für den Terminplan
            this.Ft = new TerminPlanForm { Tag = @"Terminplan", Text = @"Terminplan", MdiParent = this };

            this.Fs.FrmTerminPlan = this.Ft; // Dem Projektplan die Stammdaten bekannt machen
            this.Ft.FrmStammDaten = this.Fs; // Den Stammdaten den Projektplan bekant machen

            // Formulare anzeigen
            this.Fs.Show();
            this.Ft.Show();

            base.OnLoad(e);

            this.ultraStatusBarStart.Panels[@"ZoomInfo"].Text = @"100 %";
        }

        /// <summary> Wird aufgerufen, wenn sich der Wert des trackBarZoom - Controls ändert. </summary>
        /// <param name="sender">Die Quelledes Ereignisses.</param>
        /// <param name="e">Die <see cref="EventArgs"/> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnTrackBarZoomValueChanged(object sender, EventArgs e)
        {
            double zoomLevel = 100;

            // Zoompanel für Zoom je nach ausgewähltem Tab einstellen
            switch (this.ultraTabbedMdiManager1.ActiveTab.TextResolved)
            {
                default:                                                        // Standardmäßig ist der Terminplan ausgewählt
                    zoomLevel = Convert.ToDouble(this.trackBarZoom.Value);
                    this.Ft.ultraZoomPanelTerminPlan.ZoomProperties.ZoomFactor = (float)((zoomLevel / 100) / 5);  // Das Zoomen erfolgt ausschließlich über das Zoompanel
                    this.Ft.ultraZoomPanelGanttView.ZoomProperties.ZoomFactor = (float)(zoomLevel / 100);  // Das Zoomen erfolgt ausschließlich über das Zoompanel
                    break;

                case @"Stammdaten":                                             // Stammdaten
                    zoomLevel = Convert.ToDouble(this.trackBarZoomStamm.Value);
                    this.Fs.ultraZoomPanelStammDaten.ZoomProperties.ZoomFactor = (float)(zoomLevel / 100);
                    break;

                case @"Grunddaten":                                             // Grunddaten
                    break;
            }

            // ReSharper disable once LocalizableElement
            var anzeigeWert = (zoomLevel * 10d).ToString("P0", CultureInfo.CreateSpecificCulture("de-DE"));
            this.ultraStatusBarStart.Panels[@"ZoomInfo"].Text = anzeigeWert;
        }

        /// <summary> Behandelt das TabSelected-Ereignis des ultraTabbedMdiManager1 </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.UltraWinTabbedMdi.MdiTabEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void OnUltraTabbedMdiManager1TabSelected(object sender, MdiTabEventArgs e)
        {
            this.TabManager = (UltraTabbedMdiManager)sender;                    // Die Quelle ist ein UltraTabbedMdiManager
            this.ActiveTab = e.Tab;                                             // ausgewählten Tab ermitteln

            // Alle Trckbars zum Zoomen auf unsichtbar
            this.trackBarZoom.Visible = false;
            this.trackBarZoomStamm.Visible = false;
            this.trackBarZoomGrund.Visible = false;

            // Trackbar für Zoom je nach ausgewähltem Tab einstellen
            switch (this.ActiveTab.TextResolved)
            {
                default:                                                        // Standardmäßig ist der Terminplan ausgewählt
                    this.ultraStatusBarStart.Panels[@"Zoom"].Control = this.trackBarZoom;
                    this.trackBarZoom.Visible = true;                           // Trackbar  für den Terminplan einschalten
                    break;

                case @"Stammdaten":                                                         // Stammdaten
                    this.ultraStatusBarStart.Panels[@"Zoom"].Control = this.trackBarZoomStamm;
                    this.trackBarZoomStamm.Visible = true;                      // Trackbar  für die Stammdaten einschalten
                    break;

                case @"Grunddaten":                                                         // Grunddaten
                    this.ultraStatusBarStart.Panels[@"Zoom"].Control = this.trackBarZoomGrund;
                    this.trackBarZoomGrund.Visible = true;                      // Trackbar  für die Grunddaten einschalten
                    break;
            }
            //activeZoomPanel = (UltraZoomPanel)activeTab.t  .Form.   C     TabPage.Controls[0];
        }
    }
}