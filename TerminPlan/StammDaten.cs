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
    using System.IO;
    using System.Windows.Forms;
    using Infragistics.Win.AppStyling;
    using Infragistics.Win.Printing;
    using Infragistics.Win.UltraWinToolbars;

    //using Infragistics.Win.UltraWinGrid.ExcelExport;
    //using Infragistics.Documents.Excel;

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
            // Ermitteln, auf welches Tool geklickt wurde
            switch (e.Tool.Key)
            {
                case @"Font_Bold":                                              // Fettschrift
                    if (this.cellActivationRecursionFlag == false)
                    {
                        this.UpdateFontProperty(FontProperties.Bold);
                    }
                    break;

                case @"Font_Italic":                                             // Kursivschrift
                    if (this.cellActivationRecursionFlag == false)
                    {
                        this.UpdateFontProperty(FontProperties.Italics);
                    }
                    break;

                case @"Font_Underline":
                    if (this.cellActivationRecursionFlag == false)              // Unterstrichene Schrift
                    {
                        this.UpdateFontProperty(FontProperties.Underline);
                    }
                    break;

                case @"Font_BackColor":                                          // Hintergrundfarbe
                    this.SetTextBackColor();
                    break;

                case @"Font_ForeColor":                                          // Vordergrundfarbe
                    this.SetTextForeColor();
                    break;

                case @"FontList":                                                // Liste mit den Schriftarten
                    this.UpdateFontName();
                    break;

                case @"FontSize":                                                // Schriftgröße
                    this.UpdateFontSize();
                    break;

                case @"ThemeList":                                              // Liste mit den FArben für das Aussehen der Anwendung
                    var themeListTool = e.Tool as ListTool;
                    if (themeListTool != null && themeListTool.SelectedItem == null)
                    {
                        themeListTool.SelectedItemIndex = e.ListToolItem.Index;
                    }

                    var key = e.ListToolItem.Key;
                    if (this.FrmTerminPlan.ThemePaths[this.FrmTerminPlan.CurrentThemeIndex] != key)
                    {
                        this.FrmTerminPlan.CurrentThemeIndex = e.ListToolItem.Index;
                        StyleManager.Load(DienstProgramme.GetEmbeddedResourceStream(key));
                    }
                    break;

                case @"Print":                                                  // Ausdruck
                    var printPreview = new UltraPrintPreviewDialog { Document = this.ultraGridPrintDocumentStamm };
                    printPreview.ShowDialog(this);
                    break;

                case @"Exit":                                                   // Beenden der Anwendung
                case @"Close":
                    Application.Exit();
                    break;

                case @"Insert_Ds_Button":                                       // Datensatz am Ende einfügen
                    this.InsertStammDatenDs(true);
                    break;

                case @"Insert_Ds_AtSelectedRow":                                // Datensatz bei aktiver Zeile einfügen
                    this.InsertStammDatenDs(false);
                    break;
            }
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
            ColorizeImages();                                                   // Farbe der Bilder an das eingestellte Farbschema anpassen

            // Excel-Datei einlesen. Falls es eine Datei für das Projekt gibt, wird diese genommen,
            // sonst eine Vorlage
            var stammdatenDatei = TerminPlanForm.PrjName + @".xlsx";
            if (File.Exists(stammdatenDatei))
            {
                this.LadeExcelTabelle(ref this.ultraGridStammDaten, stammdatenDatei, @"Stammdaten");
            }
            else
            {
                // Projekt-Datei aexiastiert als Excel-Datei nicht, also Vorlage laden
                if (File.Exists(@"Vorlage.xlsx"))
                {
                    this.LadeExcelTabelle(ref this.ultraGridStammDaten, @"Vorlage.xlsx", @"Stammdaten");
                }
            }

            InitializeUi();                                                     // Oberfläche initialisieren

            // Ereignisprozedur zum Ändern des Schemas festlegen
            StyleManager.StyleChanged += this.OnApplicationStyleChanged;
        }

        #endregion OnLoad

        #endregion Überschreibungen der Basisklasse

        /// <summary>
        /// Behandelt das ToolValueChanged-Ereignis des ultraToolbarsManagerStamm Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">The <see cref="ToolEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraToolbarsManagerStammToolValueChanged(object sender, ToolEventArgs e)
        {
        }

        /// <summary>
        /// Behandelt das PropertyChanged-Ereignis des ultraToolbarsManagerStamm Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void ultraToolbarsManagerStamm_PropertyChanged(object sender, Infragistics.Win.PropertyChangedEventArgs e)
        {
            UltraToolbarsManagerStammPropertyChanged(sender, e);
        }
    }
}