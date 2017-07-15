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
//                  01.04.17  br      Einfärben 2. Toolbarmanager
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Terminplan
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;
    using Infragistics.DrawFilters;
    using Infragistics.Win;
    using Infragistics.Win.AppStyling;
    using Infragistics.Win.Printing;
    using Infragistics.Win.UltraWinEditors;
    using Infragistics.Win.UltraWinGrid;
    using Infragistics.Win.UltraWinToolbars;
    using WinGridZoomGrid;

    using AutoCompleteMode = Infragistics.Win.AutoCompleteMode;

    //using Infragistics.Win.UltraWinGrid.ExcelExport;
    //using Infragistics.Documents.Excel;

    public partial class StammDaten : Form
    {
        private int distanz;                                                    // Splitterdistanz
        private bool erweitert;                                                 // Merker, ob das Textfeld erweitert idt oder nicht

        #region Konstruktor

        /// <summary>Initialisiert eine neue Instanz der  <see cref="StammDaten"/> Klasse. </summary>
        public StammDaten()
        {
            var culture = new CultureInfo("de-DE");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

            this.InitializeComponent();

            this.zoomGridStamm = new WinGridZoomGrid.GridZoomProperty();        // Zoomdaten im Grid für Stammdaten
            this.zoomGridGrund = new WinGridZoomGrid.GridZoomProperty();        // Zoomdaten im Grid für Grunddaten

            this.InitialisiereZoom();                                           // Eigenschaften des Grids für das Zoomen ermitteln

            this.ultraTilePanel1.DrawFilter = new NoFocusRectDrawFilter();      // Damit beim Aktivieren einer Zelle kein Rechteck dargestellt wird
            this.cellFilter = new CellFilter(this);
            this.ultraGridStammDaten.DrawFilter = this.cellFilter;
            this.distanz = this.splitContainer1.SplitterDistance;
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

                case @"Font_Italic":                                            // Kursivschrift
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
            distanz = this.splitContainer1.SplitterDistance;                    // Splitterdistanz merken
            // Ereignisprozedur zum Ändern des Schemas festlegen
            StyleManager.StyleChanged += this.OnApplicationStyleChanged;
        }

        #endregion OnLoad

        #endregion Überschreibungen der Basisklasse

        public void InitialisiereZoom()
        {
            // Eigenschaften des Grids für das Zoomen ermitteln
            this.zoomGridStamm.GetGridOriginZoomProperty(this.ultraGridStammDaten); // Originaleinstellungen ermitteln
            this.zoomGridAktuell = this.zoomGridStamm;
            this.zoomGridAktuell.ZoomGrid(1, this.ultraGridStammDaten);         // Gestartet wird mit 100%

        }

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
        /// <param name="e">Die <see cref="System.ComponentModel.PropertyChangedEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void ultraToolbarsManagerStamm_PropertyChanged(object sender, Infragistics.Win.PropertyChangedEventArgs e)
        {
            UltraToolbarsManagerStammPropertyChanged(sender, e);
        }

        /// <summary>
        /// Behandelt das BeforeCellDeactivate-Ereignis des UltraGridStammDaten Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <private param name = "e" > Die < see cref="CancelEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>

        private void OnUltraGridStammDatenBeforeCellDeactivate(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Kommentar1.Visible)
            {
                Kommentar1.Visible = false;
            }
        }

        /// <summary>
        /// Behandelt das ultraGridStammDaten_-Ereignis des UltraGridStammDaten Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="CancelEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraGridStammDatenAfterExitEditMode(object sender, System.EventArgs e)
        {
            var zelle = ((UltraGrid)sender).ActiveCell;                         // Zelle, welche verlassen wird, ermitteln

            // ultraComboEditorFirma

            try
            {
                UltraCheckEditor editor = null;

                // Eventuell eingebetteten Editor der Zelle ermitteln
                if (zelle.EditorComponentResolved != null) editor = zelle.EditorComponentResolved as UltraCheckEditor;
                if (editor == null) return;                                     // Wenn kein Editor existiert, kann abgebrochen werden

                var editor1 = editor.Editor;                                    // Eingebundener Editor
                var wert = editor1.CurrentEditText.ToLower();                   // Der Zustand der Checkbox kann nur als Text ermittelt werden

                // Zustand des CheckEditors in die Zelle schreiben
                zelle.Value = wert == @"true" ? @"True" : @"False";
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            { }
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

            var colName = DienstProgramme.IntConvertToExcelHeadLine(zelle.Column.Index + 1); // Spaltennamen aus der Spaltennummer ermitteln
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
            var aktiveTabelle = startForm.ActiveTab.TextResolved;
            var sollTabelle = zeile.Cells[2].Value.ToString();                  // Überschrift ermitteln

            if (sollTabelle != aktiveTabelle)
            {
                var manager = startForm.TabManager;                             // Tabmanager zum Auswählen des benötigten Tabs
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

        private void UltraComboEditorFirmaAfterCloseUp(object sender, EventArgs e)
        {
        }

        private void UltraComboEditorFirmaTextChanged(object sender, EventArgs e)
        {
        }

        private void UltraComboEditorFirmaValueChanged(object sender, EventArgs e)
        {
        }

        private void UltraComboEditorFirmaSelectionChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Initialisiert das Layout des Grids.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="InitializeLayoutEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraGridStammDatenInitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            // Spalte'S' beinhaltet eine %-Darstellung
            this.ultraGridStammDaten.DisplayLayout.Bands[0].Columns[18].Format = "P";

            e.Layout.Override.MergedCellStyle = MergedCellStyle.Always;
            e.Layout.Override.MergedCellContentArea = MergedCellContentArea.VirtualRect;

            // Liste zur Auswahl der Firmen generieren
            if (!e.Layout.ValueLists.Exists(@"Firmenliste"))
            {
                this.vlFirmen = e.Layout.ValueLists.Add(@"Firmenliste");
                this.vlFirmen.ValueListItems.Add(0, @"Schmid");
                this.vlFirmen.ValueListItems.Add(1, @"Inda Markert");
                this.vlFirmen.ValueListItems.Add(2, @"EST");
                this.vlFirmen.ValueListItems.Add(3, @"EST intern");
            }

            this.vlFirmen.Appearance.FontData.SizeInPoints = 10;
            this.vlFirmen.Appearance.BackColor = SystemColors.ControlLight;
            this.vlFirmen.Appearance.ForeColor = Color.Blue;
            this.vlFirmen.Appearance.BorderColor = Color.Silver;
            this.vlFirmen.SortStyle = ValueListSortStyle.AscendingByValue;
            this.vlFirmen.SelectedIndex = 0;

            // Liste zur Auswahl der Berechnungsart generieren
            if (!e.Layout.ValueLists.Exists(@"Berechnungsart"))
            {
                this.vlBerechnungsArt = e.Layout.ValueLists.Add(@"Berechnungsart");
                this.vlBerechnungsArt.ValueListItems.Add(0, @"Arbeitstage");
                this.vlBerechnungsArt.ValueListItems.Add(1, @"Wochentage");
            }

            this.vlBerechnungsArt.Appearance.FontData.SizeInPoints = 10;
            this.vlBerechnungsArt.Appearance.BackColor = SystemColors.ControlLight;
            this.vlBerechnungsArt.Appearance.ForeColor = Color.Blue;
            this.vlBerechnungsArt.Appearance.BorderColor = Color.Silver;
            this.vlBerechnungsArt.SortStyle = ValueListSortStyle.AscendingByValue;
            this.vlBerechnungsArt.SelectedIndex = 0;
        }

        /// <summary>
        /// Wird ausgelöst, wenn aus der Firmenliste eine Liste ausgewählt wird.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="InitializeLayoutEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void UltraGridStammDatenCellListSelect(object sender, CellEventArgs e)
        {
            if (e.Cell.ValueList == null) return;                                // Abbrechen, falls es keine Zellenliste gibt

            var startForm = (StartForm)this.MdiParent;                          // Das Elternfenster holen
            var terminPlan = startForm.Fs.FrmTerminPlan;                        // Das Fenster des Terminplans holen
            terminPlan.FirmenIndex = e.Cell.ValueList.SelectedItemIndex;        // Index der ausgewählten Firma merken

            var grid = startForm.Fs.FrmTerminPlan.ultraGridDaten;               // Grid, welches das Firmenlogo enthält
            var zelle = grid.DisplayLayout.Rows[0].Cells[2];                    // Zelle, welches das Logo enthält

            // Firmenlogo einstellen
            switch (terminPlan.FirmenIndex)
            {
                case 0:                                                         // Schmid
                    zelle.Appearance.ImageBackground = Properties.Resources.Schmid_Maschinenbau1;
                    break;

                case 1:                                                         // Inda
                    zelle.Appearance.ImageBackground = Properties.Resources.Inda;
                    break;

                default:                                                        // EST
                    zelle.Appearance.ImageBackground = Properties.Resources.EST;
                    break;
            }

            var breite = zelle.Appearance.ImageBackground.Width;
            zelle.Column.Width = breite;
            grid.UpdateData();
        }

        /// <summary>
        /// Behandelt das ValueChanged Ereignis des ultraDateTimeEditorPrjStart Controls.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="EventArgs"/> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraDateTimeEditorPrjStartValueChanged(object sender, EventArgs e)
        {
            var startForm = (StartForm)this.MdiParent;                          // Das Elternfenster holen
            var editor = (UltraDateTimeEditor)sender;                           // Damit der geänderte Wert ermittelt werden kann
            prjStartDatum = Convert.ToDateTime(editor.Value);                   // Ausgewähltes Datum ermitteln

            // Das eingestelle Datum in die Zelle im Grid eintragen
            editor.Parent.Text = prjStartDatum.ToString();
            var eintrag = @"PS - " + editor.Parent.Text;
            SetDataRowValue(ultraGridStammDaten, 3, 13, eintrag);               // Startdatum in die Datumsspalte (Spalte 'M') eintragen

            var eintrag1 = @"Projektstart [PS]: " + prjStartDatum.ToShortTimeString();
            SetDataRowValue(startForm.Fs.FrmTerminPlan.ultraGridDaten, 1, 0, eintrag1); // Projektstart im Terminplan eintragen
        }

        private void ultraTextEditorBloecke_ValueChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Behandelt das MouseEnterElement Ereignis des ultraGridStammDaten Controls.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="UIElementEventArgs"/> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraGridStammDatenMouseEnterElement(object sender, UIElementEventArgs e)
        {
            if (e.Element is CellUIElement)
            {
                var zelle = e.Element.SelectableItem as UltraGridCell;          // Zelle unter dem Cursor
                var spalte = (UltraGridColumn)e.Element.GetContext();           // ermittelte Spalte im Grid für die Cursorposition
                if (spalte != null && zelle.Tag != null)
                {
                    var sb = new StringBuilder();                               // Zum Zusammenstellen des Kommentars

                    // Ermitteln, welcher Kommentar angezeigt werden soll (steht im Tag der Zelle)
                    var welcherKommentar = zelle.Tag.ToString();
                    if (welcherKommentar.Contains(@"Kommentar"))
                    {
                        if (welcherKommentar == @"Kommentar1")
                        {
                            sb.Append(@"{\rtf1\ansi");
                            sb.Append(@"\b Hier einstellen, ob die Historie der \b0\line");
                            sb.Append(@"\b Zeitänderungen sofort nach der \b0\line");
                            sb.Append(@"\b Änderung erzeugt werden soll. \b0\line\line");
                            sb.Append(@"\b Möglichkeiten: \b0\line");
                            sb.Append(@"    - sofort automatisch:       1\line");
                            sb.Append(@"    - von Hand über Menü:     0 \line\line");
                            sb.Append(@"\b Achtung: Bei großen Tabellen sollte aus \b0\line");
                            sb.Append(@"\b Geschwindigkeitsgründen dieser Wert auf 0\b0\line");
                            sb.Append(@"\b stehen und die Tabelle von Hand \b0\line");
                            sb.Append(@"\b erzeugt werden.\b0}");
                        }
                        else if (welcherKommentar == @"Kommentar2")
                        {
                            sb.Append(@"{\rtf1\ansi");
                            sb.Append(@"\b Hier einstellen, wie die Zeilenhöhe im Projekt- \b0\line");
                            sb.Append(@"\b plan automatisch eingestellt werden soll: \b0\line\line");
                            sb.Append(@"    - nicht:                                 0\line");
                            sb.Append(@"    - nur grau hinterlegte Zeilen:   1 \line");
                            sb.Append(@"    - alle Zeilen:                          2 ");
                        }
                        else if (welcherKommentar == @"Kommentar3")
                        {
                            sb.Append(@"{\rtf1\ansi");
                            sb.Append(@"\b Hier einstellen, ob die Zeilenhöhe im \b0\line");
                            sb.Append(@"\b Projektplan bei Zellen mit automatischen \b0\line");
                            sb.Append(@"\b Zellenumbruch an den Inhalt der Zelle \b0\line");
                            sb.Append(@"\b angepasst werden soll. Geprüft wird nur \b0\line");
                            sb.Append(@"\b die Spalte 'C' (Bezeichnung des \b0\line");
                            sb.Append(@"\b Arbeitsiinhalts). \b0\line\line");
                            sb.Append(@"\b Möglichkeiten: \b0\line");
                            sb.Append(@"    - automatisch:       1\line");
                            sb.Append(@"    - von Hand:            0 \line");
                            sb.Append(@"\b Achtung: Bei großen Tabellen sollte aus \b0\line");
                            sb.Append(@"\b Geschwindigkeitsgründen dieser Wert \b0\line");
                            sb.Append(@"\b auf 0 stehen und die Tabelle von Hand \b0\line");
                            sb.Append(@"\b korrigiert werden \b0");
                        }
                        else if (welcherKommentar == @"Kommentar4")
                        {
                            sb.Append(@"{\rtf1\ansi");
                            sb.Append(@"\b Art für die Erzeugung des Status \b0\line");
                            sb.Append(@"\b Möglichkeiten: \b0\line");
                            sb.Append(@"    - über Mittelwert:  1 \line");
                            sb.Append(@"    - über Formel:      0 ");
                        }
                        else if (welcherKommentar == @"Kommentar5")
                        {
                            sb.Append(@"{\rtf1\ansi");
                            sb.Append(@"\b Hier das gewünschte Intervall \b0\line");
                            sb.Append(@"\b zum Auffrischen des Status \b0\line");
                            sb.Append(@"\b eingeben \b0\line\line");
                            sb.Append(@"\b Beispiele: \b0\line");
                            sb.Append(@"    - für 2 Stunden:                             02:00:00 \line");
                            sb.Append(@"    - für 5 Minuten:                              00:05:00 \line");
                            sb.Append(@"    - für 10 Sekunden:                         00:00:10 \line");
                            sb.Append(@"    - kein automatisches Auffrischen:   00:00:00 ");
                        }

                        Kommentar1.Rtf = sb.ToString();
                        Kommentar1.AutoSize = true;

                        var neuX1 = e.Element.DrawingRect.Right;
                        var neuY1 = e.Element.DrawingRect.Top;
                        var korr = Kommentar1.Height + 20;
                        Kommentar1.Location = new Point(neuX1 + 5, neuY1 - korr);
                        Kommentar1.Visible = true;
                        Kommentar1.Invalidate();
                    }
                    else if (welcherKommentar == @"anzBloecke")
                    {
                        //ultraTextEditorBloecke.Invalidate();
                        //ultraTextEditorBloecke.Update();
                    }
                }
                else
                {
                    if (Kommentar1.Visible) Kommentar1.Visible = false;
                }
            }
            else if (e.Element is MergedCellUIElement)
            {
                ultraTextEditorBloecke.Invalidate();
                ultraTextEditorBloecke.Update();
            }
        }

        private void ultraGridStammDaten_MouseHover(object sender, EventArgs e)
        {
            Bitmap memoryImage;
            Graphics myGraphics = this.CreateGraphics();
            memoryImage = new Bitmap(1, 1, myGraphics);
            Graphics memoryGraphics = Graphics.FromImage(memoryImage);

            memoryGraphics.CopyFromScreen(Cursor.Position.X, Cursor.Position.Y, 0, 0, new Size(1, 1));
            Color c1 = memoryImage.GetPixel(0, 0);

            var txt = @"PosX:" + Cursor.Position.X + @" PosY:" + Cursor.Position.Y + @"Farbe:" + c1.ToString();
            Debug.WriteLine("MouseHover" + txt);

            var zelle = ((UltraGrid)sender).ActiveCell;
            if (zelle != null && zelle.Tag != null && zelle.Tag.ToString() == @"Kommentar1")
            {
                var region = zelle.GetUIElement().Region.GetRegionData().Data;
                var x = region[10];
                var y = region[11];
                var breite = zelle.Width;
                var location = Kommentar1.Location;

                var posX = Cursor.Position.X;
                var posY = Cursor.Position.Y;

                var rec_WA = Screen.FromControl(this).WorkingArea;
                this.Location = new Point(Convert.ToInt32(rec_WA.X + ((rec_WA.Width - this.Width) / 2)), Convert.ToInt32(rec_WA.Y + ((rec_WA.Height - this.Height) / 2)));

                Kommentar1.Clear();

                var grid = (UltraGrid)sender;

                var sb = new StringBuilder();
                sb.Append(@"{\rtf1\ansi");
                sb.Append(@"\b Hier einstellen, ob die Historie der \b0\line");
                sb.Append(@"\b Zeitänderungen sofort nach der \b0\line");
                sb.Append(@"\b Änderung erzeugt werden soll. \b0\line\line");
                sb.Append(@"\b Möglichkeiten: \b0\line");
                sb.Append(@"    - sofort automatisch:       1\line");
                sb.Append(@"    - von Hand über Menü:     0 \line\line");
                sb.Append(@"\b Achtung: Bei großen Tabellen sollte aus \b0\line");
                sb.Append(@"\b Geschwindigkeitsgründen dieser Wert auf 0\b0\line");
                sb.Append(@"\b stehen und die Tabelle von Hand \b0\line");
                sb.Append(@"\b erzeugt werden.\b0}");

                Kommentar1.Rtf = sb.ToString();

                Kommentar1.Location = new Point(x + breite * 2 / 3, y + zelle.Height * 10);
                Kommentar1.Visible = true;
                Kommentar1.Invalidate();
            }
        }

        private void ultraGridStammDaten_MouseLeave(object sender, EventArgs e)
        {
            var zelle = ((UltraGrid)sender).ActiveCell;
            if (zelle != null) Debug.WriteLine("MouseLeave" + zelle.ToString());

            if (Kommentar1.Visible)
            {
                //Kommentar1.Visible = false;
            }
        }

        private void ultraGridStammDaten_MouseEnter(object sender, EventArgs e)
        {
            Bitmap memoryImage;
            Graphics myGraphics = this.CreateGraphics();
            memoryImage = new Bitmap(1, 1, myGraphics);
            Graphics memoryGraphics = Graphics.FromImage(memoryImage);

            memoryGraphics.CopyFromScreen(Cursor.Position.X, Cursor.Position.Y, 0, 0, new Size(1, 1));
            Color c1 = memoryImage.GetPixel(0, 0);

            if (c1.R == 255 && c1.G == 0 && c1.B == 0)
            {
            }

            var txt = @"PosX:" + Cursor.Position.X + @" PosY:" + Cursor.Position.Y + @"Farbe:" + c1.ToString();
            Debug.WriteLine("MouseEnnter" + txt);
        }

        private void Kommentar1_ContentsResized(object sender, ContentsResizedEventArgs e)
        {
            ((RichTextBox)sender).AutoSize = true;
            ((RichTextBox)sender).Height = e.NewRectangle.Height + 5;
        }

        /// <summary>
        ///Behandelt das AfterCellUpdate Ereignis des ultraGridStammDaten Controls.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="CellEventArgs"/> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraGridStammDatenAfterCellUpdate(object sender, CellEventArgs e)
        {
            // Ermitteln, welche Zelle geändert wurde
            var zeile = e.Cell.Row.Index;                                       // Zeile der geänderten Zelle
            var spalte = e.Cell.Column.Index;                                   // Spalte der geänderten Zelle

            // Ermitteln, ob es das Startdatum des Projekts ist
        }

        /// <summary>
        ///Behandelt das CellChange Ereignis des ultraGridStammDaten Controls.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="CellEventArgs"/> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraGridStammDatenCellChange(object sender, CellEventArgs e)
        {
            // Ermitteln, welche Zelle geändert wurde
            var zeile = e.Cell.Row.Index;                                       // Zeile der geänderten Zelle
            var spalte = e.Cell.Column.Index;                                   // Spalte der geänderten Zelle
            var startForm = (StartForm)this.MdiParent;                          // Das Elternfenster holen
            var zelle = ((UltraGrid)sender).ActiveCell;                         // Zelle, welche angewählt wurde, ermitteln

            this.ultraTextEditor1.Text = zelle.Text;                            // den Inhalt in den Editor kopieren

            // Ermitteln, ob der Projektname geändert wurde
            if (zeile == 9 && spalte == 1)
            {
                System.Threading.Thread.Sleep(200);

                // Es muss der Text genommen werden, da in Value noch der alte Wert steht
                var obj = e.Cell.Text as Object;
                SetDataRowValue(startForm.Fs.FrmTerminPlan.ultraGridDaten, 0, 0, obj); // Projektname im Terminplan eintragen
                var anzPixel = startForm.Fs.FrmTerminPlan.ultraGridDaten.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints;
                var breite = e.Cell.Text.Length * anzPixel;                     // Breite der Spalte anhand des Inhalts
                zelle.Column.Width = (int)breite + 5;
                startForm.Fs.FrmTerminPlan.ultraGridDaten.Invalidate();
                startForm.Fs.FrmTerminPlan.ultraGridDaten.Update();
            }

            // Ermitteln, ob der Projektleiter geändert wurde
            if (zeile == 12 && spalte == 1)
            {
                System.Threading.Thread.Sleep(200);

                // Es muss der Text genommen werden, da in Value noch der alte Wert steht
                var obj = e.Cell.Text as Object;

                SetDataRowValue(startForm.Fs.FrmTerminPlan.ultraGridDaten, 0, 1, obj); // Projektleiter im Terminplan eintragen
                startForm.Fs.FrmTerminPlan.ultraGridDaten.Invalidate();
                startForm.Fs.FrmTerminPlan.ultraGridDaten.Update();
            }

            // Ermitteln, ob es das Startdatum des Projekts ist (steht in 'B15')
            if (zeile == 15 && spalte == 1)
            {
                var prjStartDatum = Convert.ToDateTime(e.Cell.Text);            // Ausgewähltes Datum ermitteln
                                                                                // Das eingestelle Datum in die Zelle im Grid eintragen
                var neuerWert = prjStartDatum.ToShortDateString();
                var eintrag = @"PS - " + neuerWert;
                var einTragObj = eintrag as Object;
                SetDataRowValue(ultraGridStammDaten, 3, 13, einTragObj);

                // Anzahl darzustellender Wochen ermitteln (steht in Zelle 'B25')
                var anzZeilen = ultraGridStammDaten.DisplayLayout.Rows[25].Cells[1].Text;
                var anzahlZeilen = 120;
                if (anzZeilen.GetType() == typeof(DBNull))
                {
                    anzZeilen = @"120";
                }

                anzahlZeilen = Convert.ToInt32(anzZeilen);

                SetzeDatumsSpalte(ultraGridStammDaten, 6, anzahlZeilen, 13, neuerWert);
                BearbeiteFeiertage(prjStartDatum);                              // Feiertage ab dem Jahr des Startdatums berechnen
            }
        }

        private void ultraFormattedLinkLabel1_Click(object sender, EventArgs e)
        {
        }

        private void ultraFormattedLinkLabel1_Enter(object sender, EventArgs e)
        {
        }

        /// <summary>
        ///Behandelt das _EditorButtonClick Ereignis des ultraTextEditor1 Controls.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="EditorButtonEventArgs"/> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraTextEditor1EditorButtonClick(object sender, EditorButtonEventArgs e)
        {
            if (rtfLocation.X == 0 && rtfLocation.Y == 0) return;

            if (this.erweitert)
            {
                this.erweitert = false;
                this.splitContainer1.SplitterDistance = this.distanz;
            }
            else
            {
                this.erweitert = true;
                //this.ultraTextEditor1.Height = this.ultraTextEditor1.Height * 3;
                this.splitContainer1.SplitterDistance = this.distanz * 2;
            }

            return;
            richTextBoxZelle.Width = ultraTextEditor1.Width;
            richTextBoxZelle.Height = ultraTextEditor1.Height;
            richTextBoxZelle.Visible = true;
            return;
            
            // Damit man die Richtextbox über der Textbox anzeigen kann, muss sie in einem eigenen Fenster laufen
            var popup = new Form()
            {
                TopMost = true,
                //StartPosition = FormStartPosition.CenterScreen,
                //StartPosition = FormStartPosition.CenterParent,
                Location = rtfLocation,
                Text = string.Empty,
                ControlBox = false,
                ShowIcon = false,
                ShowInTaskbar = false,
                FormBorderStyle = FormBorderStyle.None,
                SizeGripStyle = SizeGripStyle.Hide,
                //AutoScaleMode = AutoScaleMode.Dpi,
            };

            //popup.Size = richTextBoxZelle.Size;
            var groesse = new Size(ultraTextEditor1.Width,
                ultraTextEditor1.Height * 4);
            popup.Size = groesse;
            popup.Controls.Add(richTextBoxZelle);
            //popup.Show();
        }

        private void ultraTextEditor1_MouseDown(object sender, MouseEventArgs e)
        {
            // rtfLocation = this.PointToClient(ultraTextEditor1.PointToScreen((new Point(e.X,
            //     e.Y))));
            rtfLocation = this.PointToClient(ultraTile2.PointToScreen(new Point((ultraTile2.Location.X),
                ultraTile2.Location.Y + (int)(ultraTextEditor1.Size.Height * 2))));

            rtfLocation.Y += 15;
            rtfLocation.X += 300;

            richTextBoxZelle.Text = ultraTextEditor1.Text;
            richTextBoxZelle.BackColor = ultraTextEditor1.BackColor;
            richTextBoxZelle.Dock = DockStyle.Fill;
        }

        private void ultraGridStammDaten_SizeChanged(object sender, EventArgs e)
        {
            var scrollpos = this.ultraGridStammDaten.ActiveRowScrollRegion.ScrollPosition;
            Refresh();
            this.ultraGridStammDaten.ActiveRowScrollRegion.ScrollPosition = scrollpos;
        }
    }
}