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
//                  20.04.17  br      Grundversion
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Terminplan
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;
    using Infragistics.Win;
    using Infragistics.Win.UltraWinEditors;
    using Infragistics.Win.UltraWinGrid;

    public partial class StammDaten : Form
    {
        /// <summary>
        /// fügt einen neuen Datensatz in den Stammdaten ein.
        /// </summary>
        /// <param name="amEnde">Merker, ob der Datensatz am ende der Tabelle eingefügt werden soll.</param>
        private void InsertStammDatenDs(bool amEnde)
        {
            // ermitteln, ob überhaupt Datensätze vorhanden sind
            var anzZeilen = this.ultraGridStammDaten.Rows.Count;                // Anzahl vorhandener Datensätze
            if (anzZeilen == 0 || amEnde)
            {
                this.ultraGridStammDaten.DisplayLayout.Bands[0].AddNew();
            }
        }

        /// <summary>
        /// Stellt die einzelnen Zellen des Arbeitsblattes so ein wie im Original Arbeitsblatt in Excel
        /// </summary>
        private void StelleArbeitsBlattEin()
        {
            var col = this.ultraGridStammDaten.DisplayLayout.Bands[0].Columns;  // Alle Spalten der Tabelle

            // Zeilennummern im Grid anzeigen
            this.ultraGridStammDaten.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.VisibleIndex;
            this.ultraGridStammDaten.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            this.ultraGridStammDaten.DisplayLayout.Override.RowSelectorWidth = 40;
            this.ultraGridStammDaten.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.SeparateElement;
            this.ultraGridStammDaten.DisplayLayout.Bands[0].SortedColumns.Clear();
            this.ultraGridStammDaten.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.Select;

            // Zellen der Spalte einstellen
            var ueberSchriftFarbe = Color.White;
            var hinterGrundFarbe = Color.LightGray;

            ueberSchriftFarbe = this.ultraZoomPanelStammDaten.Appearance.ForeColor;
            hinterGrundFarbe = this.ultraZoomPanelStammDaten.Appearance.BackColor;

            for (var r = 0; r < this.rowCount; r++)
            {
                // Zeilenhöhe einstellen
                switch (r)
                {
                    case 1:                                                     // Zeile 2
                        this.ultraGridStammDaten.Rows[r].Height = 48;
                        break;

                    case 2:                                                     // Zeile 3 wird nicht angezeigt
                        this.ultraGridStammDaten.Rows[r].Hidden = true;
                        break;

                    case 5:                                                     // Zeile 6
                        this.ultraGridStammDaten.Rows[r].Height = 41;
                        break;

                    case 47:                                                    // Zeile 48
                        this.ultraGridStammDaten.Rows[r].Height = 43;
                        break;

                    case 53:                                                    // Zeile 54
                    case 56:                                                    // Zeile 57
                        this.ultraGridStammDaten.Rows[r].Height = 43;
                        break;
                }

                //this.ultraGridStammDaten.DisplayLayout.Rows[r].Appearance.BorderColor = Color.Transparent;
            }

            foreach (var sp in col)
            {
                // bei allen Themes außer bei Theme 1
                if (this.FrmTerminPlan.CurrentThemeIndex != 0)
                {
                    ueberSchriftFarbe = Color.Black;
                    hinterGrundFarbe = Color.DarkGray;
                }

                this.BearbeiteZeile1(sp.Index + 1, hinterGrundFarbe, ueberSchriftFarbe); // Zeile 1 bearbeiten
                this.BearbeiteZeile2(sp.Index + 1, hinterGrundFarbe, ueberSchriftFarbe); // Zeile 2 bearbeiten

                if (sp.Index == 1)
                    this.BearbeiteSpalte1(sp.Index + 1, hinterGrundFarbe, ueberSchriftFarbe);

                sp.SortIndicator = SortIndicator.None;
            }
        }

        private void BearbeiteZeile1(int spalte, Color hinterGrundFarbe, Color ueberSchriftFarbe)
        {
            var row = this.ultraGridStammDaten.DisplayLayout.Rows[0];
            var zelle = row.Cells[spalte - 1];                                  // Zelle in der Spalte ermitteln
            var vonSpalte = 0;                                                  // Bei Merged Cells der Beginn
            var bisSpalte = 0;                                                  // Bei Merged Cells das Ende

            // In der 1. Zeile stehen nur Überschriften, die Zellen können also nicht editiert werden
            zelle.CellDisplayStyle = CellDisplayStyle.PlainText;

            switch (spalte)
            {
                case 2:
                case 6:
                case 7:
                case 9:
                case 14:
                case 19:
                case 21:
                case 23:
                case 25:
                case 27:
                case 28:
                case 29:
                case 31:
                case 33:
                case 35:
                case 36:
                case 37:
                case 38:
                    SetzeUeberSchrift(zelle, hinterGrundFarbe, ueberSchriftFarbe); // Zelle als Überschrift kennzeichnen
                    vonSpalte = bisSpalte = spalte - 1;                         // Spaltennummer der Zelle merken
                    if (spalte == 6 || spalte == 7)
                    {
                        zelle.Appearance.BorderColor = hinterGrundFarbe;
                        vonSpalte = 5;
                        bisSpalte = 6;
                    }

                    // Überschrift in Combobox eintragen
                    if (!string.IsNullOrEmpty(zelle.Text))
                    {
                        this.dsUeberSchriften.Tables[0].Rows.Add(zelle.Text,
                            zelle.Column.Index,
                           @"Stammdaten",
                            vonSpalte,
                            bisSpalte);
                    }

                    break;
            }
        }

        private void BearbeiteZeile2(int spalte, Color hinterGrundFarbe, Color ueberSchriftFarbe)
        {
            var row = this.ultraGridStammDaten.DisplayLayout.Rows[1];
            var zelle = row.Cells[spalte - 1];                                  // Zelle in der Spalte ermitteln

            switch (spalte)
            {
                case 6:
                case 7:
                case 14:
                case 19:
                case 21:
                case 23:
                case 25:
                case 27:
                case 28:
                case 29:
                case 31:
                case 33:
                case 35:
                case 36:
                case 37:
                case 38:
                    SetzeUeberSchrift(zelle, hinterGrundFarbe, ueberSchriftFarbe);

                    if (spalte == 6 || spalte == 7)
                    {
                        zelle.Appearance.TextHAlign = HAlign.Left;
                        zelle.Appearance.TextVAlign = VAlign.Bottom;
                        zelle.Appearance.BorderColor = hinterGrundFarbe;
                    }

                    // Die Spalten 32, 23 und 25 enthalten Checkboxen
                    if (spalte == 21 || spalte == 23 || spalte == 25)
                    {
                        var checkEditor = new UltraCheckEditor();
                        checkEditor.CheckedValueChanged += new System.EventHandler(this.CheckEditorCheckedValueChanged);
                        checkEditor.CheckedChanged += this.CheckEditorCheckedChanged;
                        checkEditor.Click += this.OnCheckEditorClick;

                        checkEditor.Tag = zelle;
                        zelle.Appearance.TextHAlign = HAlign.Left;
                        zelle.Appearance.TextVAlign = VAlign.Bottom;
                        zelle.Appearance.BackColor = hinterGrundFarbe;
                        zelle.Appearance.ForeColor = hinterGrundFarbe;          // Damit der Text für dle Darstellung im Checkeditor nicht sichtbar ist
                        // Zustand der Check-Editoren setzen
                        if (zelle.Text == @"False")
                        {
                            checkEditor.Checked = false;
                        }
                        else
                        {
                            checkEditor.Checked = true;
                        }

                        switch (spalte)
                        {
                            case 21:
                                checkEditor.Text = @"Benutzerdef 1 anzeigen";
                                break;

                            case 23:
                                checkEditor.Text = @"Benutzerdef 2 anzeigen";
                                break;

                            case 25:
                                checkEditor.Text = @"Benutzerdef 3 anzeigen";
                                break;
                        }

                        checkEditor.Appearance.BackColor = hinterGrundFarbe;
                        checkEditor.Appearance.ForeColor = ueberSchriftFarbe;
                        checkEditor.Appearance.FontData.Name = @"Arial";
                        checkEditor.Appearance.FontData.SizeInPoints = 8;
                        zelle.EditorComponent = checkEditor;
                        zelle.CellDisplayStyle = CellDisplayStyle.FullEditorDisplay;
                    }
                    break;

                default:
                    break;
            }
        }

        /// <summary>Setzt Überschriften und Controls in einer Spalte.</summary>
        /// <param name="spalte">Die zu bearbeitende Spalte.</param>
        /// <param name="hinterGrundFarbe">einzustellende Hintergrundfarbe.</param>
        /// <param name="ueberSchriftFarbe">einzustellende Vordergrundfarbe.</param>
        private void BearbeiteSpalte1(int spalte, Color hinterGrundFarbe, Color ueberSchriftFarbe)
        {
            DateTime datumsWert;

            // Die Überschrift für Zeile 1 und 2 wird an anderer Stelle verarbeitet
            // Zeile 3 enthält eine Überschrift (Adresse)
            var zelle = this.ultraGridStammDaten.DisplayLayout.Rows[3].Cells[1];
            SetzeUeberSchrift(zelle, hinterGrundFarbe, ueberSchriftFarbe);

            // Zeile 5 enthält eine Überschrift (Email)
            zelle = this.ultraGridStammDaten.DisplayLayout.Rows[5].Cells[1];
            SetzeUeberSchrift(zelle, hinterGrundFarbe, ueberSchriftFarbe);

            // Zeile 6 enthält eine URL (Mailadresse)
            zelle = this.ultraGridStammDaten.DisplayLayout.Rows[6].Cells[1];
            SetzeUrl(ref zelle);

            // Zeile 8 enthält eine Überschrift (Projektname)
            zelle = this.ultraGridStammDaten.DisplayLayout.Rows[8].Cells[1];
            SetzeUeberSchrift(zelle, hinterGrundFarbe, ueberSchriftFarbe);

            // Zeile 11 enthält eine Überschrift (Projektleiter)
            zelle = this.ultraGridStammDaten.DisplayLayout.Rows[11].Cells[1];
            SetzeUeberSchrift(zelle, hinterGrundFarbe, ueberSchriftFarbe);

            // Zeile 14 enthält eine Überschrift (Projektstart)
            zelle = this.ultraGridStammDaten.DisplayLayout.Rows[14].Cells[1];
            SetzeUeberSchrift(zelle, hinterGrundFarbe, ueberSchriftFarbe);

            // Der Projektstart (Zeile 15) ist ein Datum
            zelle = this.ultraGridStammDaten.DisplayLayout.Rows[15].Cells[1];
            //var wert = zelle.Value;
            //DateTime datumsWert;

            DateTime.TryParseExact(zelle.Value.ToString(), @"yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out datumsWert);

            zelle.Style = Infragistics.Win.UltraWinGrid.ColumnStyle.DateTime;   // Zelle enthält ein Datum
            var editor = this.ultraDateTimeEditorPrjStart;
            editor.Appearance.BorderColor = ueberSchriftFarbe;

            // Note: Nur zum Test
            if (datumsWert.Year < 2000)
            {
                // Note: Nur zum Test
                editor.Value = @"29.06.2016";
                // editor.Value = DateTime.Today;
                zelle.Value = editor.Value;
            }

            zelle.EditorComponent = editor;
            zelle.CellDisplayStyle = CellDisplayStyle.FullEditorDisplay;
            zelle.Activation = Activation.AllowEdit;
            //this.ultraGridStammDaten.DisplayLayout.Bands[0].Columns[1].

            // Zeile 17 enthält eine Überschrift (Revisionsstand)
            zelle = this.ultraGridStammDaten.DisplayLayout.Rows[17].Cells[1];
            SetzeUeberSchrift(zelle, hinterGrundFarbe, ueberSchriftFarbe);

            // Zeile 19 enthält eine Überschrift (Berechnungsart)
            zelle = this.ultraGridStammDaten.DisplayLayout.Rows[19].Cells[1];
            zelle.Value = @"Berechnungsart";
            SetzeUeberSchrift(zelle, hinterGrundFarbe, ueberSchriftFarbe);

            // Zeile 20 enthält Auswahl Berechnungsart
            zelle = this.ultraGridStammDaten.DisplayLayout.Rows[20].Cells[1];
            SetzeComboBerechnungsArt(ref zelle);
            zelle.Value = zelle.ValueList.GetText(0);                           // Beim Start ist 'arbeitstage' ausgewählt
            zelle.Appearance.ForeColor = Color.Blue;

            // Zeile 22 enthält eine Überschrift (Auswahl Firma)
            zelle = this.ultraGridStammDaten.DisplayLayout.Rows[22].Cells[1];
            zelle.Value = @"Auswahl Firma:";
            SetzeUeberSchrift(zelle, hinterGrundFarbe, ueberSchriftFarbe);

            // Zeile 23 enthält Auswahl Firma
            zelle = this.ultraGridStammDaten.DisplayLayout.Rows[23].Cells[1];
            SetzeComboFirma(ref zelle);
            zelle.Value = zelle.ValueList.GetText(0);                           // Beim Start ist Fa. Schmid ausgewählt
            zelle.Appearance.ForeColor = Color.Blue;

            // Zeile 24 enthält eine Überschrift (Anzahl Wochen)
            zelle = this.ultraGridStammDaten.DisplayLayout.Rows[24].Cells[1];
            zelle.Value = @"Anzahl Wochen:";
            SetzeUeberSchrift(zelle, hinterGrundFarbe, ueberSchriftFarbe);

            // Zeile 25 enthält die Anzahl darzustellender Wochen
            zelle = this.ultraGridStammDaten.DisplayLayout.Rows[26].Cells[1];
            zelle.Appearance.BorderColor = ueberSchriftFarbe;

            // Zeile 26 enthält eine Überschrift (Anzahl Spaltenblöcke)
            zelle = this.ultraGridStammDaten.DisplayLayout.Rows[27].Cells[1];
            zelle.Value = @"Anzahl Spaltenblöcke:";
            SetzeUeberSchrift(zelle, hinterGrundFarbe, ueberSchriftFarbe);

            // Zeile 32 enthält eine Überschrift (Anzahl anzuzeigender Blöcke)
            zelle = this.ultraGridStammDaten.DisplayLayout.Rows[32].Cells[1];
            zelle.Value = @"Anzahl anzuzeigender Blöcke";
            SetzeUeberSchrift(zelle, hinterGrundFarbe, ueberSchriftFarbe);

            // Zellen 33, 34 und 35 der Spalte 'B' verbinden
            for (var i = 33; i <= 35; i++)
            {
                this.ultraGridStammDaten.Rows[i].Cells[1].Selected = true;
                this.ultraGridStammDaten.Rows[i].Cells[1].M
            }

            var selCells = this.ultraGridStammDaten.Selected.Cells;
            if (selCells != null)
            {
                this.ultraGridStammDaten.DisplayLayout.Bands[0].Columns[1].MergedCellEvaluationType = MergedCellEvaluationType.MergeSameValue
                selCells.Mer
            }
            MergedCellContentArea
        }

        /// <summary>Setz eine Zelle als Überschrift.</summary>
        /// <param name="zelle">Die Zelle.</param>
        /// <param name="hinterGrundFarbe">Hintergrundfarbe der Zelle.</param>
        /// <param name="ueberSchriftFarbe">Vordergrundfarbe der Zelle.</param>
        private void SetzeUeberSchrift(UltraGridCell zelle, Color hinterGrundFarbe, Color ueberSchriftFarbe)
        {
            if (zelle == null) return;                                          // Falls keine Zelle existiert, abbrechen
            zelle.Appearance.BackColor = hinterGrundFarbe;
            zelle.Appearance.ForeColor = ueberSchriftFarbe;
            zelle.Appearance.FontData.Bold = DefaultableBoolean.True;
            zelle.Appearance.FontData.Name = @"Arial";
            zelle.Activation = Activation.NoEdit;                               // Überschrift kann nicht bearbeitet werden
        }

        /// <summary>Setzt eine DropDownList in die übergebene Zelle.</summary>
        /// <param name="zelle">Zelle, in welche die DropDownList eingefügt werden soll.</param>
        private void SetzeComboFirma(ref UltraGridCell zelle)
        {
            if (zelle == null) return;                                          // Falls keine Zelle existiert, abbrechen

            zelle.Style = Infragistics.Win.UltraWinGrid.ColumnStyle.DropDownList;  // Zelle enthält eine DropDownList
            zelle.ValueList = this.vlFirmen;
        }

        /// <summary>Setzt eine DropDownList in die übergebene Zelle.</summary>
        /// <param name="zelle">Zelle, in welche die DropDownList eingefügt werden soll.</param>
        private void SetzeComboBerechnungsArt(ref UltraGridCell zelle)
        {
            if (zelle == null) return;                                          // Falls keine Zelle existiert, abbrechen

            zelle.Style = Infragistics.Win.UltraWinGrid.ColumnStyle.DropDownList;  // Zelle enthält eine DropDownList
            zelle.ValueList = this.vlBerechnungsArt;
        }

        /// <summary>Setzt ein LinkLabel als Editor in die übergebene Zelle.</summary>
        /// <param name="zelle">Zelle, in welche das LinkLabel eingefügt werden soll.</param>
        private void SetzeUrl(ref UltraGridCell zelle)
        {
            if (zelle == null) return;                                          // Falls keine Zelle existiert, abbrechen

            zelle.Style = Infragistics.Win.UltraWinGrid.ColumnStyle.URL;        // Zelle enthält eine URL
            var editor = this.ultraFormattedLinkLabel1;
            editor.Text = zelle.Text;
            zelle.EditorComponent = editor;
            zelle.CellDisplayStyle = CellDisplayStyle.FullEditorDisplay;
            zelle.Activation = Activation.AllowEdit;
        }

        private void OnCheckEditorClick(object sender, System.EventArgs e)
        {
        }

        private void CheckEditorCheckedChanged(object sender, System.EventArgs e)
        {
            //throw new System.NotImplementedException();
        }

        /// <summary>Wird aufgerufen, wenn sich der Zustand der Checkbox im Checkeditor geändert hat.</summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="System.EventArgs"/> Instanz, welche die Ereignisdaten enthält.</param>
        private void CheckEditorCheckedValueChanged(object sender, System.EventArgs e)
        {
            var editor = (UltraCheckEditor)sender;                              // Check-Editor ermitteln
            var wert = editor.Checked;                                          // Zustand der Checkbox
            var zelle = (UltraGridCell)editor.Tag;                              // Aus dem UltraCheckEditor die zugehörige Zelle ermitteln

            if (zelle == null) return;                                          // Wenn keine Zelle existiert, kann abgebrochen werden

            // Je nach Zustand der Checkbox den Text in der Zelle setzen
            if (wert)
                zelle.Value = @"True";
            else
                zelle.Value = @"False";
        }
    }
}