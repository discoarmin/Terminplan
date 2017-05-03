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
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows.Forms;
    using Infragistics.Win.AppStyling;
    using Infragistics.Win.Printing;
    using Infragistics.Win.UltraWinEditors;
    using Infragistics.Win.UltraWinGrid;
    using Infragistics.Win.UltraWinToolbars;

    using AutoCompleteMode = Infragistics.Win.AutoCompleteMode;

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
        /// <param name="e">Die <see cref="ToolEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraToolbarsManagerStammToolValueChanged(object sender, ToolEventArgs e)
        {
        }

        /// <summary>
        /// Behandelt das PropertyChanged-Ereignis des ultraToolbarsManagerStamm Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="PropertyChangedEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void ultraToolbarsManagerStamm_PropertyChanged(object sender, Infragistics.Win.PropertyChangedEventArgs e)
        {
            UltraToolbarsManagerStammPropertyChanged(sender, e);
        }

        /// <summary>
        /// Behandelt das BeforeCellDeactivate-Ereignis des UltraGridStammDaten Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="CancelEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraGridStammDatenBeforeCellDeactivate(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        /// <summary>
        /// Behandelt das ultraGridStammDaten_-Ereignis des UltraGridStammDaten Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="CancelEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraGridStammDatenAfterExitEditMode(object sender, System.EventArgs e)
        {
            var zelle = ((UltraGrid)sender).ActiveCell;                         // Zelle, welche verlassen wird, ermitteln
            var editor = (UltraCheckEditor)zelle?.EditorComponentResolved;      // Eventuell eingebetteten Editor der Zelle ermitteln
            if (editor == null) return;                                         // Wenn kein Editor existiert, kann abgebrochen werden

            var editor1 = editor.Editor;                                        // Eingebundener Editor
            var wert = editor1.CurrentEditText.ToLower();                       // Der Zustand der Checkbox kann nur als Text ermittelt werden

            // Zustand des CheckEditors in die Zelle schreiben
            zelle.Value = wert == @"true" ? @"True" : @"False";
        }

        /// <summary>
        /// Behandelt das BeforeCellActivate-Ereignis des UltraGridStammDaten Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="CancelEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraGridStammDatenBeforeCellActivate(object sender, CancelableCellEventArgs e)
        {
            var zeile = ((UltraGrid)sender).ActiveRow;                          // Zeile, welche angewählt werden soll, ermitteln
            if (zeile.Index == 0)
                e.Cancel = true;
        }

        /// <summary>
        /// Behandelt das LinkLabel_Clicked-Ereignis des UltraFormattedLinkLabel1 Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="LinkClickedEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraFormattedLinkLabel1LinkClicked(object sender, Infragistics.Win.FormattedLinkLabel.LinkClickedEventArgs e)
        {
            this.MailNachrichtErstellen();
        }

        /// <summary>
        /// Behandelt das ValueChanged-Ereignis des UltraGridStammDaten Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="EventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraComboEditorSpatenAusWahlValueChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Behandelt das Click-Ereignis des UltraButtonErase Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="EventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraButtonEraseClick(object sender, EventArgs e)
        {
        }

        /// <summary>
        ///Wird aufgerufen, nachdem die Zelle aktiviert wurde.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="EventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraGridStammDatenAfterCellActivate(object sender, EventArgs e)
        {
            var zelle = ((UltraGrid)sender).ActiveCell;                         // Zelle, welche angewählt wurde, ermitteln

            this.ultraTextEditor1.Text = zelle.Text;                            // den Inhalt in den Editor kopieren

            var colName = DienstProgramme.IntConvertToExcelHeadLine(zelle.Column.Index); // Spaltennamen aus der Spaltennummer ermitteln
            var zeile = zelle.Row.Index.ToString();                             // Aktive Zeile
            this.ultraComboZellen.Text = colName + zeile;                       // Zelleninfo eintragen
        }

        /// <summary>
        /// Behandelt das InitializeLayout-Ereignis des ultraCombo Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="EventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraComboZellenInitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            this.ultraComboZellen.DisplayLayout.Bands[0].Columns[0].AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        }

        /// <summary>
        /// Behandelt das BeforeDropDown-Ereignis des ultraCombo Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="CancelEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraComboZellenBeforeDropDown(object sender, CancelEventArgs e)
        {
            var combo = (UltraCombo)sender;                                     // ComboBox ermitteln
            var neueBreite = (float)0;

            var schriftGroesse = this.ultraComboZellen.Font.SizeInPoints;       // Schriftgrösse des Kontrols
            var anzEintraege = this.dsUeberSchriften.Tables[0].Rows.Count;      // Anzahl dargestellter
            var eintraege = this.dsUeberSchriften.Tables[0].Rows;

            // Die Breite der DropDown-Liste anhand der Einträge einstellen
            for (var i = 0; i < anzEintraege; i++)
            {
                var eintrag = eintraege[i][0].ToString();                       // Überschrift ermitteln
                var istBreite = eintrag.Length * schriftGroesse;                // Breite der Überschrift berechnen

                // Maximale Breite ermitteln
                if (istBreite > neueBreite)
                {
                    neueBreite = istBreite;
                }
            }

            // DropDown-Liste bisschen breiter wie die sichtbare Spalte, so dass kein Scrollbalken dargestellt wird
            var breite = (int)Math.Round(neueBreite, MidpointRounding.AwayFromZero);
            this.ultraComboZellen.DropDownWidth = breite + (int)Math.Round(schriftGroesse * 2, MidpointRounding.AwayFromZero);

            // ermittelte Breite ist auch die Breite der 1. Spalte in der Datentabelle
            combo.DisplayLayout.Bands[0].Columns[0].Width = breite;
        }

        /// <summary>
        /// Behandelt das AfterCloseUp-Ereignis des ultraCombo Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="EventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraComboZellenAfterCloseUp(object sender, EventArgs e)
        {
            var combo = (UltraCombo)sender;                                     // ComboBox ermitteln
            var zeile = combo.ActiveRow;                                        // Zeile der ausgewählten Überschrift
            if (zeile == null) return;                                          // Es kann abgebrochen werden, da keine Überschrift angewählt ist

            UltraGrid grid = null;                                              // Grid im Formular
            // ausgewählte Spalte ermitteln
            var startForm = (StartForm)this.MdiParent;                           // Das Elternfenster holen
            var aktiveTabelle = startForm.activeTab.TextResolved;
            var sollTabelle = zeile.Cells[2].Value.ToString();                  // Überschrift ermitteln

            if (sollTabelle != aktiveTabelle)
            {
                var manager = startForm.tabManager;                             // Tabmanager zum Auswählen des benötigten Tabs
                manager.TabFromKey(sollTabelle).Activate();
            }

            switch (aktiveTabelle)
            {
                case @"Stammdaten":
                    grid = this.ultraGridStammDaten;                            // Das Grid zur Aufnahme der Stammdaten
                    break;

                    // ToDO: hier weitere Tabs abfragen
            }

            if (grid == null) return;                                           // Bearbeitung abbrechen, wenn kein Grid vorhanden ist

            // Alle Spaltenauswahlen löschen
            var anzSpalten = grid.DisplayLayout.Bands[0].Columns.Count;         // Anzahl Spalten im Grid
            for (var s = 0; s < anzSpalten; s++)
            {
                grid.DisplayLayout.Bands[0].Columns[s].Header.Selected = false;
            }

            var vonSpalte = Convert.ToInt32(zeile.Cells[3].Value);              // Erste auszuwählende Spalte
            var bisSpalte = Convert.ToInt32(zeile.Cells[4].Value);              // Letzte auszuwählende Spalte

            for (var i = vonSpalte; i <= bisSpalte; i++)
            {
                grid.DisplayLayout.Bands[0].Columns[i].Header.Selected = true;
            }

            var csr = grid.ActiveColScrollRegion;
            csr.ScrollColIntoView(grid.DisplayLayout.Bands[0].Columns[vonSpalte], true);
        }
    }
}