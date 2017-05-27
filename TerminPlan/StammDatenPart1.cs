// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StammDatenPart1.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   1. Teil Bearbeitung der Stammdaten des Formulars.
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
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;
    using Feiertage;
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

            // ToDo: Einfügen, wenn nicht am Ende einfgefügt werden soll
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
            var ueberSchriftFarbe = Color.White;                                // Schriftfarbe der Überschrift
            var hinterGrundFarbe = Color.LightGray;                             // Hintergrundfarbe der Überschrift

            ueberSchriftFarbe = this.ultraZoomPanelStammDaten.Appearance.ForeColor;
            hinterGrundFarbe = this.ultraZoomPanelStammDaten.Appearance.BackColor;

            // Alle vorhandenen Zeilen bearbeiten
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
            }

            // Alle Spalten einer Zeile bearbeiten
            foreach (var sp in col)
            {
                // bei allen Themes außer bei Theme 1 werden andere Schriftfarben und Hintergrundfarben eingestellt
                if (this.FrmTerminPlan.CurrentThemeIndex != 0)
                {
                    ueberSchriftFarbe = Color.Black;                            // Schriftfarbe ist schwarz
                    hinterGrundFarbe = Color.DarkGray;                          // Hintergrundfarbe ist Dunkelgrau
                }

                BearbeiteZeile1(sp.Index + 1, hinterGrundFarbe, ueberSchriftFarbe); // Zeile 1 bearbeiten
                BearbeiteZeile2(sp.Index + 1, hinterGrundFarbe, ueberSchriftFarbe); // Zeile 2 bearbeiten

                if (sp.Index == 1)
                {
                    this.BearbeiteSpalte1(sp.Index + 1, hinterGrundFarbe, ueberSchriftFarbe);
                    this.BearbeiteFeiertage(prjStartDatum);                     // Feiertage ab dem Jahr des Startdatums berechnen

                    sp.SortIndicator = SortIndicator.None;
                }
            }

            SetzeHistoryUeberschriften(ueberSchriftFarbe);                      // Überschriften für Bereich zum Zwischenspeicherung der Änderungen
            BearbeiteSpalteS();                                                 // Prozentwerte der Ferigstellung
            BearbeiteDelta();                                                   // Farbeinstellungen der Delta-Werte
            BearbeiteAufgaben();                                                // Checkeditoren für die Aufgaben
            BearbeiteDauer();
        }

        /// <summary>Setzt das Format der Überschriftspalten für die History-Einträge in den Stammdaten.</summary>
        /// <param name="ueberSchriftFarbe">Farbe für die Überschrift.</param>
        private void SetzeHistoryUeberschriften(Color ueberSchriftFarbe)
        {
            // Es sind 4 Überschriften in den Spalten F bis I
            for (var s = 5; s < 9; s++)
            {
                var zelle = this.ultraGridStammDaten.DisplayLayout.Rows[48].Cells[s]; // Zelle ermitteln
                zelle.Appearance.ForeColor = ueberSchriftFarbe;                 // Farbe der Überschrift einstellen
                zelle.Appearance.FontData.Bold = DefaultableBoolean.True;       // Überschrift wird in Fettscrift dargestellt
                zelle.Appearance.FontData.Name = @"Arial";                      // Schriftart einstellen
                zelle.Activation = Activation.NoEdit;                           // Überschrift kann nicht bearbeitet werden
            }
        }

        /// <summary>Stellt Überschriften und Hintergrundfarbe der 1. Zeile ein</summary>
        /// <param name="spalte">Die einzustellende Spalte.</param>
        /// <param name="hinterGrundFarbe">Die einzustellende Hintergrundfarbe.</param>
        /// <param name="ueberSchriftFarbe">Die einzustellende Textfarbe.</param>
        private void BearbeiteZeile1(int spalte, Color hinterGrundFarbe, Color ueberSchriftFarbe)
        {
            var row = this.ultraGridStammDaten.DisplayLayout.Rows[0];
            var rowTest = this.ultraGridStammDaten.DisplayLayout.Rows[1];

            var zelle = row.Cells[spalte - 1];                                  // Zelle in der Spalte ermitteln
            var vonSpalte = 0;                                                  // Bei Merged Cells der Beginn
            var bisSpalte = 0;                                                  // Bei Merged Cells das Ende

            // In der 1. Zeile stehen nur Überschriften, die Zellen können also nicht editiert werden
            zelle.CellDisplayStyle = CellDisplayStyle.PlainText;
            zelle.Appearance.BorderColor = hinterGrundFarbe;

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
                    SetzeUeberSchrift(zelle, hinterGrundFarbe, ueberSchriftFarbe, rowTest); // Zelle als Überschrift kennzeichnen
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

        /// <summary>Stellt Überschriften und Hintergrundfarbe der 2. Zeile ein</summary>
        /// <param name="spalte">Die einzustellende Spalte.</param>
        /// <param name="hinterGrundFarbe">Die einzustellende Hintergrundfarbe.</param>
        /// <param name="ueberSchriftFarbe">Die einzustellende Textfarbe.</param>
        private void BearbeiteZeile2(int spalte, Color hinterGrundFarbe, Color ueberSchriftFarbe)
        {
            var row = this.ultraGridStammDaten.DisplayLayout.Rows[1];
            var rowTest = this.ultraGridStammDaten.DisplayLayout.Rows[2];
            var zelle = row.Cells[spalte - 1];                                  // Zelle in der Spalte ermitteln

            // Spaltespezifische Einstellungen vornehmen
            switch (spalte)
            {
                case 6:                                                         // Projektmitglieder Kürzel
                case 7:                                                         // Projektmitglieder Name und Vorname
                case 9:
                case 14:
                case 19:
                case 21:
                case 23:
                case 25:
                case 27:
                case 28:
                case 29:
                //case 31:
                case 33:
                case 35:
                case 36:
                case 37:
                case 38:
                    SetzeUeberSchrift(zelle, hinterGrundFarbe, ueberSchriftFarbe, rowTest);

                    if (spalte == 6 || spalte == 7)
                    {
                        zelle.Appearance.TextHAlign = HAlign.Left;
                        zelle.Appearance.TextVAlign = VAlign.Bottom;
                    }

                    // Die Spalten 21, 23 und 25 enthalten Checkboxen
                    if (spalte == 21 || spalte == 23 || spalte == 25)
                    {
                        var checkEditor = new UltraCheckEditor();
                        checkEditor.CheckedValueChanged += new System.EventHandler(this.CheckEditorCheckedValueChanged);
                        checkEditor.CheckedChanged += this.CheckEditorCheckedChanged;
                        checkEditor.Click += this.OnCheckEditorClick;

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

                        checkEditor.Tag = zelle;
                        checkEditor.Appearance.BorderColor = hinterGrundFarbe;
                        zelle.Appearance.TextHAlign = HAlign.Left;
                        zelle.Appearance.TextVAlign = VAlign.Bottom;
                        zelle.Appearance.BackColor = hinterGrundFarbe;
                        zelle.Appearance.ForeColor = hinterGrundFarbe;          // Damit der Text für dle Darstellung im Checkeditor nicht sichtbar ist

                        checkEditor.Appearance.BackColor = hinterGrundFarbe;
                        checkEditor.Appearance.ForeColor = ueberSchriftFarbe;
                        checkEditor.Appearance.FontData.Name = @"Arial";
                        checkEditor.Appearance.FontData.SizeInPoints = 8;
                        zelle.EditorComponent = checkEditor;
                        zelle.CellDisplayStyle = CellDisplayStyle.FullEditorDisplay;
                    }

                    // für zusätzliche Angaben wird eine kleinere Schriftart eingestellt
                    if (spalte == 33 || spalte == 35 || spalte == 36 || spalte == 37 || spalte == 38)
                    {
                        zelle.Appearance.FontData.Name = @"Arial";
                        zelle.Appearance.FontData.SizeInPoints = 10;
                        zelle.Appearance.FontData.Bold = DefaultableBoolean.True;

                        // Bei den arbeitsfreien Tagen müssen die zusätzlichen Angaben mehrzeilig
                        // dargestellt werden
                        if (spalte == 33)
                        {
                            zelle.Appearance.FontData.SizeInPoints = 8;
                            zelle.Column.CellMultiLine = DefaultableBoolean.True;
                            zelle.Appearance.ForeColorDisabled = zelle.Appearance.ForeColor;
                            zelle.Activation = Activation.Disabled;
                        }

                        if (spalte == 38)
                        {
                            zelle.Column.CellMultiLine = DefaultableBoolean.True;
                        }
                    }

                    break;

                default:
                    break;
            }
        }

        /// <summary>Einstellungen für das Zeitfenster (Darstellung Delta).</summary>
        private void BearbeiteDelta()
        {
            // Die drei zu bearbeitenden Zeilen ermitteln
            var row = this.ultraGridStammDaten.DisplayLayout.Rows[3];
            var row1 = this.ultraGridStammDaten.DisplayLayout.Rows[4];
            var row2 = this.ultraGridStammDaten.DisplayLayout.Rows[5];
            var row3 = this.ultraGridStammDaten.DisplayLayout.Rows[6];

            // Die Einstellungen befinden sich in den Spalten 27-29
            for (var spalte = 27; spalte < 30; spalte++)
            {
                var zelle = row.Cells[spalte - 1];                              // Zelle der 1. Zeile der Spalte ermitteln
                var zelle1 = row1.Cells[spalte - 1];                            // Zelle der 2. Zeile der Spalte ermitteln
                var zelle2 = row2.Cells[spalte - 1];                            // Zelle der 3. Zeile der Spalte ermitteln
                var zelle3 = row3.Cells[spalte - 1];                            // Zelle der 4. Zeile (dient zur Ermittlung von verbundenen Zellen)

                if (spalte == 27 || spalte == 28 || spalte == 29)
                {
                    if (spalte != 28)
                    {
                        zelle.Appearance.BackColor = Color.FromArgb(9868950);
                        zelle.Appearance.ForeColorDisabled = zelle.Appearance.ForeColor;
                        zelle.SelectedAppearance.BorderColor = zelle.Appearance.BackColor;
                        zelle.ActiveAppearance.BorderColor = zelle.Appearance.BackColor;
                        zelle.Activation = Activation.NoEdit;                   // Zelle kann nicht bearbeitet werden

                        zelle1.Appearance.BackColor = Color.FromArgb(9868950);  // Grau
                        zelle1.Appearance.ForeColorDisabled = zelle1.Appearance.ForeColor;
                        zelle1.ActiveAppearance.BorderColor = zelle1.Appearance.BackColor;
                        zelle1.Activation = Activation.NoEdit;                  // Zelle kann nicht bearbeitet werden
                        zelle1.SelectedAppearance.BorderColor = zelle1.Appearance.BackColor;

                        // Falls in der Zelle kein Wert eingetragen ist, überprüfen, ob Zelle in der nächsten Zeile einen Wert enthält
                        if (!zelle2.IsMergedWith(zelle3))
                        {
                            zelle2.Appearance.BackColor = Color.FromArgb(9868950);
                            zelle2.Appearance.ForeColorDisabled = zelle2.Appearance.ForeColor;
                            zelle2.Activation = Activation.NoEdit;              // Zelle kann nicht bearbeitet werden
                            zelle2.SelectedAppearance.BorderColor = zelle2.Appearance.BackColor;
                            zelle2.ActiveAppearance.BorderColor = zelle2.Appearance.BackColor;
                            continue;                                           // Falls noch eine Zelle mit Daten kommt, nächste Spalte bearbeiten
                        }
                        else
                        {
                            zelle2.Appearance.BackColor = Color.FromArgb(9868950);
                            zelle2.Appearance.ForeColorDisabled = zelle2.Appearance.ForeColor;
                            zelle2.SelectedAppearance.BorderColor = zelle2.Appearance.BackColor;
                            zelle2.ActiveAppearance.BorderColor = zelle2.Appearance.BackColor;
                            zelle2.Activation = Activation.NoEdit;              // Zelle kann nicht bearbeitet werden

                            // in die lezte zu bearbeitende Zelle einen nicht sichtbaren Text eintragen,
                            // da sonst alle mit dieser Zelle verbundenen Zellen eingefärbt werden
                            zelle2.Value = @" ";
                            zelle2.Appearance.ForeColor = zelle2.Appearance.BackColor;
                        }
                    }
                    else
                    {
                        // In dieser Spalte werden die Eingaben gemacht
                        zelle.Appearance.BackColor = Color.FromArgb(70, 227, 3);  // Grün
                        zelle.ActiveAppearance.BackColor = Color.FromArgb(70, 227, 3);
                        zelle.SelectedAppearance.BackColor = Color.FromArgb(70, 227, 3);
                        zelle.ActiveAppearance.BorderColor = Color.FromArgb(70, 227, 3);

                        zelle1.Appearance.BackColor = Color.FromArgb(236, 173, 52); // Orange
                        zelle1.ActiveAppearance.BackColor = Color.FromArgb(236, 173, 52);
                        zelle1.SelectedAppearance.BackColor = Color.FromArgb(236, 173, 52);
                        zelle1.ActiveAppearance.BorderColor = Color.FromArgb(236, 173, 52);

                        zelle2.Activation = Activation.Disabled;                // Zelle kann weder aktiviert noch bearbeitet werden

                        // Falls in der Zelle kein Wert eingetragen, überprüfen, ob Zelle in der nächsten Zeile einen Wert enthält
                        if (!zelle2.IsMergedWith(zelle3))
                        {
                            zelle2.Appearance.BackColor = Color.FromArgb(9868950);
                            zelle2.ActiveAppearance.BackColor = Color.FromArgb(9868950);
                            zelle2.SelectedAppearance.BackColor = Color.FromArgb(9868950);
                            zelle2.ActiveAppearance.BorderColor = Color.FromArgb(9868950);
                            continue;                                           // Da noch eine Zelle mit Daten kommt, nächste Spalte bearbeiten
                        }
                        else
                        {
                            // Nächste Zelle enthält keine Daten
                            zelle2.Appearance.BackColor = Color.FromArgb(9868950);
                            zelle2.ActiveAppearance.BackColor = Color.FromArgb(9868950);
                            zelle2.SelectedAppearance.BackColor = Color.FromArgb(9868950);
                            zelle2.ActiveAppearance.BorderColor = Color.FromArgb(9868950);

                            // in die lezte zu bearbeitende Zelle einen nicht sichtbaren Text eintragen,
                            // da sonst alle mit dieser Zelle verbundenen Zellen eingefärbt werden
                            zelle2.Value = @" ";
                            zelle2.Appearance.ForeColor = zelle2.Appearance.BackColor;
                        }
                    }
                }
            }
        }

        /// <summary>Stellt die Werte in der Spalte 'S' ein.</summary>
        /// <remarks>Diese Spalte enthält die Prozent-Angaben für den Fortschritt</remarks>
        private void BearbeiteSpalteS()
        {
            var table = (DataTable)ultraGridStammDaten.DataSource;              // Datentabelle ermitteln

            // Alle vorhandenen %-Angaben durchgehen
            for (var i = 3; i <= 11; i++)
            {
                var zelle = this.ultraGridStammDaten.DisplayLayout.Rows[i].Cells[18];
                var wert = zelle.Value;
                if (wert.GetType() == typeof(DBNull)) continue;                 // Falls kein Wert vorhanden ist, weiter mit nächster Zeile
                if (wert.ToString().Contains(@"%")) continue;                   // Falls schon neu formatiert, weiter mit nächster Zeile

                var dwert = Convert.ToDouble(wert);                             // Damit Wert in % umgewandelt werden kann, muss er ein Double-Wert sein
                var wertNeu = dwert.ToString("P0");                             // %-Wert ohme Kommastellen
                var row = table.Rows[i];                                        // In diese Zeile wird der Wert eingetragen
                row[18] = wertNeu;                                              // %-Wert eintragen
            }
        }

        /// <summary>Setz eine Zelle als Überschrift.</summary>
        /// <param name="zelle">Die Zelle.</param>
        /// <param name="hinterGrundFarbe">Hintergrundfarbe der Zelle.</param>
        /// <param name="ueberSchriftFarbe">Vordergrundfarbe der Zelle.</param>
        private void SetzeUeberSchrift(UltraGridCell zelle, Color hinterGrundFarbe, Color ueberSchriftFarbe, UltraGridRow rowTest = null)
        {
            if (zelle == null) return;                                          // Falls keine Zelle existiert, abbrechen
            if (rowTest == null)
            {
                zelle.Appearance.BackColor = hinterGrundFarbe;
                zelle.Appearance.ForeColor = ueberSchriftFarbe;
                zelle.Appearance.BorderColor = hinterGrundFarbe;                // Damit keine Zellränder sichtbar sind
                zelle.Appearance.FontData.Bold = DefaultableBoolean.True;
                zelle.Appearance.FontData.Name = @"Arial";
                zelle.Activation = Activation.NoEdit;                           // Überschrift kann nicht bearbeitet werden
            }
            else
            {
                var index = zelle.Column.Index;                                 // Spalte der zu bearbeitenden Zelle
                var zelleTest = rowTest.Cells[index];                           // Zum Test auf verbundene Zellen
                if (!zelle.IsMergedWith(zelleTest))
                {
                    zelle.Appearance.BackColor = hinterGrundFarbe;
                    zelle.Appearance.ForeColor = ueberSchriftFarbe;
                    zelle.Appearance.BorderColor = hinterGrundFarbe;            // Damit keine Zellränder sichtbar sind
                    zelle.Appearance.FontData.Bold = DefaultableBoolean.True;
                    zelle.Appearance.FontData.Name = @"Arial";
                    zelle.Activation = Activation.NoEdit;                       // Überschrift kann nicht bearbeitet werden
                }
                else
                {
                    // Da es sich um eine verbundene Zelle handelt, muss ein unsichtbarer
                    // Text eingetragen werden, da sonst alle verbundenen Zellen die
                    // Heintergrundfarbe annehmen würden
                    zelleTest.Value = @" ";
                    zelleTest.Appearance.BackColor = hinterGrundFarbe;

                    zelle.Appearance.BackColor = hinterGrundFarbe;
                    zelle.Appearance.ForeColor = zelle.Appearance.BackColor;
                    zelle.Appearance.ForeColor = ueberSchriftFarbe;
                    zelle.Appearance.BorderColor = hinterGrundFarbe;            // Damit keine Zellränder sichtbar sind
                    zelle.Appearance.FontData.Bold = DefaultableBoolean.True;
                    zelle.Appearance.FontData.Name = @"Arial";
                    zelle.Activation = Activation.NoEdit;                       // Überschrift kann nicht bearbeitet werden
                }
            }
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
            editor.Click += ultraFormattedLinkLabel1Click;
            editor.Text = zelle.Text;
            zelle.EditorComponent = editor;
            zelle.CellDisplayStyle = CellDisplayStyle.FullEditorDisplay;
            zelle.Activation = Activation.AllowEdit;
        }

        private void ultraFormattedLinkLabel1Click(object sender, EventArgs e)
        {
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

        /// <summary>Setzt einen Wert in die übergebene Zeile und Spalte.</summary>
        /// <param name="grid">das mit der DataSource verbundene Grid.</param>
        /// <param name="zeile">Zeile in welcher der Wert eingetragen werden soll.</param>
        /// <param name="spalte">Spalte, in welcher der Wert eingetragen werden soll.</param>
        /// <param name="neuerWert">der neu einzutragende Wert.</param>
        private void SetDataRowValue(UltraGrid grid, int zeile, int spalte, object neuerWert)
        {
            var art = grid.DataSource.GetType().Name;                           // Name des Typs der Datasource
            if (grid.Name == @"ultraGridDaten")
            {
                var startForm = (StartForm)this.MdiParent;                      // Das Elternfenster holen

                var ds = startForm.Fs.FrmTerminPlan.dataSetPrjDaten;            // DataSet, welches die Projektdaten enthält
                var tabelle = ds.Tables[0];                                     // Es gibt nur eine Tabelle
                if (tabelle.Rows.Count == 0)
                {
                    var neueZeile = tabelle.NewRow();
                    tabelle.Rows.Add(neueZeile);
                }

                tabelle.Rows[zeile][spalte] = neuerWert;
                System.Threading.Thread.Sleep(200);
                return;
            }

            var table = (DataTable)grid.DataSource;                             // Datentabelle ermitteln
            var row = table.Rows[zeile];                                        // Zeile, in Welche der Wert eingetragen werden soll
            row[spalte] = neuerWert;                                            // Neuen Wert zuweisen
            System.Threading.Thread.Sleep(200);
        }

        /// <summary>Setzt einen Wert in die übergebene Zeile und Spalte.</summary>
        /// <param name="grid">das mit der DataSource verbundene Grid.</param>
        /// <param name="vonZeile">Ab dieser Zeile werden die Werte eingetragen.</param>
        /// <param name="anzZeilen">Anzahl der zu bearbeitenden Zeilen.</param>
        /// <param name="spalte">Spalte, in welcher der Wert eingetragen werden soll.</param>
        /// <param name="neuerWert">das Datum, von welchem aus die Berechnung erfolgt.</param>
        private void SetzeDatumsSpalte(UltraGrid grid, int vonZeile, int anzZeilen, int spalte, object neuerWert)
        {
            if (grid == null) return;                                           // Abbruch, wenn kein Grid vorhanden ist

            // Es können nur die maximale Anzahl an Wochen eingegeben werden.
            // Ist diese Anzahl überschritten, so wird die maximale Anzahl an Wochen genommen
            if (anzZeilen > MaxWochen)
            {
                anzZeilen = MaxWochen;
            }
            else
            {
                anzZeilen += vonZeile;                                          // Damit auch alle Datumswerte berechnet werde
            }

            var zaehler = 1;                                                    // Differenz in Anzahl Wochen
            int z;
            var table = (DataTable)grid.DataSource;                             // Datentabelle ermitteln
            DataRow row;                                                        // Zu ändernde Zeile

            for (z = vonZeile; z <= anzZeilen; z++)
            {
                var berechneterWert = Convert.ToDateTime(neuerWert).Date.AddDays(7 * zaehler);
                var eintrag = @"PS + " + zaehler + @"W. - " + berechneterWert.ToShortDateString();
                row = table.Rows[z];                                            // Zeile, in Welche der Wert eingetragen werden soll
                row[spalte] = eintrag;                                          // Neuen Wert zuweisen
                zaehler++;                                                      // Wochenzähler erhöhen
            }

            // Den Rest der Spalte löschen
            for (var i = z; i <= MaxWochen; i++)
            {
                row = table.Rows[i];                                            // Zeile, in Welche der Wert eingetragen werden soll
                row[spalte] = string.Empty;                                     // Inhalt der Zelle Löschen
            }
        }
    }
}