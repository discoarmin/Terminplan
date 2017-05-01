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
    using System.Windows.Forms;
    using System.Drawing;
    using Microsoft.Office.Interop.Outlook;
    using Infragistics.Win;
    using Infragistics.Win.UltraWinGrid;
    using Infragistics.Win.UltraWinEditors;

    using Outlook = Microsoft.Office.Interop.Outlook;

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
            }
        }

        private void BearbeiteZeile1(int spalte, Color hinterGrundFarbe, Color ueberSchriftFarbe)
        {
            var row = this.ultraGridStammDaten.DisplayLayout.Rows[0];
            var zelle = row.Cells[spalte - 1];                                  // Zelle in der Spalte ermitteln

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
                    SetzeUeberSchrift(zelle, hinterGrundFarbe, ueberSchriftFarbe);

                    if (spalte == 6 || spalte == 7)
                    {
                        zelle.Appearance.BorderColor = hinterGrundFarbe;
                    }
                    break;

                default:
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
                        checkEditor.CheckedValueChanged += new System.EventHandler(this.CheckEditor_CheckedValueChanged);
                        checkEditor.CheckedChanged += this.CheckEditor_CheckedChanged;
                        checkEditor.Click += this.OnCheckEditorClick;

                        checkEditor.Tag = zelle;
                        zelle.Appearance.TextHAlign = HAlign.Left;
                        zelle.Appearance.TextVAlign = VAlign.Bottom;

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

        private void BearbeiteSpalte1(int spalte, Color hinterGrundFarbe, Color ueberSchriftFarbe)
        {
            // Die Überschrift für Zeile 1 und 2 wird an anderer Stelle verarbeitet
            // Zeile 4 enthält eineÜberschrift
            var zelle = this.ultraGridStammDaten.DisplayLayout.Rows[3].Cells[1];
            SetzeUeberSchrift(zelle, hinterGrundFarbe, ueberSchriftFarbe);

            // Zeile 6 enthält eine Überschrift
            zelle = this.ultraGridStammDaten.DisplayLayout.Rows[5].Cells[1];
            SetzeUeberSchrift(zelle, hinterGrundFarbe, ueberSchriftFarbe);

            // Zeile 7 enthält eine URL
            zelle = this.ultraGridStammDaten.DisplayLayout.Rows[6].Cells[1];
            SetzeUrl(ref zelle);
        }

        private void SetzeUeberSchrift(UltraGridCell zelle, Color hinterGrundFarbe, Color ueberSchriftFarbe)
        {
            zelle.Appearance.BackColor = hinterGrundFarbe;
            zelle.Appearance.ForeColor = ueberSchriftFarbe;
            zelle.Appearance.FontData.Bold = DefaultableBoolean.True;
            zelle.Appearance.FontData.Name = @"Arial";
        }

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

        private void CheckEditor_CheckedChanged(object sender, System.EventArgs e)
        {
            //throw new System.NotImplementedException();
        }

        private void CheckEditor_CheckedValueChanged(object sender, System.EventArgs e)
        {
            var editor = (UltraCheckEditor)sender;
            var wert = editor.Checked;
            var zelle = (UltraGridCell)editor.Tag;                              // Aus dem UltraCheckEditor die zugehörige Zelle ermitteln

            if (zelle == null) return;                                          // Wenn keine Zelle existiert, kann abgebrochen werden
            if (wert)
                zelle.Value = @"True";
            else
                zelle.Value = @"False";
        }
    }
}