// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StammDaten.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Ereignisbehandlung des Formulars.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  04.02.17  br      Grundversion
//                  01.04.17  br      Einfärben 2. Toolbarmanaer
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Terminplan
{
    using System.Globalization;
    using System.Windows.Forms;
    using Infragistics.Win.AppStyling;
    using Infragistics.Win.UltraWinToolbars;

    public partial class StammDaten : Form
    {
        #region Konstruktor

        /// <summary>Initialisiert eine neue Instanz der  <see cref="StammDaten"/> Klasse. </summary>
        public StammDaten()
        {
            var culture = new CultureInfo("de-DE");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

            this.InitializeComponent();
        }

        #region Dispose

        /// <summary> Bereinigung aller verwendeter Ressourcen </summary>
        /// <param name="disposing">true, falls verwaltete Ressourcen entsorgt werden sollen; sonst false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                // Deaktivieren der Ereignisprozedur OnApplicationStyleChanged()
                StyleManager.StyleChanged -= this.OnApplicationStyleChanged;

                this.components.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion Dispose

        #endregion Konstruktor

        #region Ereignisprozeduren

        #region OnApplicationStyleChanged

        /// <summary> Behandelt das StyleChanged-Ereignis des Application Styling Managers </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.AppStyling.StyleChangedEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void OnApplicationStyleChanged(object sender, StyleChangedEventArgs e)
        {
            ApplicationStyleChanged(sender, e);
        }

        #endregion OnApplicationStyleChanged

        /// <summary>
        /// Behandelt das ToolClick-Ereignis of the ultraToolbarsManager1 control.
        /// </summary>
        /// <remarks>
        /// Die jeweilige Aktion wird nur durchgeführt, wenn sie nicht schon durchgeführt wurde
        /// </remarks>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.UltraWinToolbars.ToolClickEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraToolbarsManagerStammToolClick(object sender, ToolClickEventArgs e)
        {
        }

        /// <summary>
        /// Wird aufgerufen, wenn sich der Zoomfaktor ändert.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="System.EventArgs"/> Instanz,  welche die Ereignisdaten enthält.</param>
        private void OnZoomFactorChanged(object sender, System.EventArgs e)
        {
        }

        #endregion Ereignisprozeduren

        #region Überschreibungen der Basisklasse

        #region OnLoad

        /// <summary>
        /// Löst das <see cref="E:System.Windows.Forms.Form.Load" /> Ereignis aus.
        /// </summary>
        /// <param name="e">Ein <see cref="T:System.EventArgs" /> welches die Ereignisdaten enthält.</param>
        protected override void OnLoad(System.EventArgs e)
        {
            OnColorizeImages();                                                 // Farbe der Bilder an das eingestellte Farbschema anpassen
            OnInitializeUi();                                                   // Oberfläche initialisieren

            // Ereignisprozedur zum Ändern des Schemas festlegen
            StyleManager.StyleChanged += this.OnApplicationStyleChanged;
        }

        #endregion OnLoad

        #endregion Überschreibungen der Basisklasse
    }
}