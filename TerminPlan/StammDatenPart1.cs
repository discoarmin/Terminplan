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
    using Infragistics.Win.AppStyling;
    using Infragistics.Win.Printing;
    using Infragistics.Win.UltraWinToolbars;
    using System.Drawing;
    using Infragistics.Win;
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

            // Zellen der Spalte einstellen
            var ueberSchriftFarbe = Color.White;
            var hinterGrundFarbe = Color.LightGray;

            for (var r = 0; r < this.rowCount; r++)
            {
                this.ultraGridStammDaten.DisplayLayout.Rows[r].Appearance.BorderColor = Color.Transparent;

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

            foreach (var sp in col)
            {
                // bei allen Themes außer bei Theme 1
                if (this.FrmTerminPlan.CurrentThemeIndex != 0)
                {
                    sp.CellAppearance.BackColor = Color.White;                  // Standwrdmäßig ist sie Hintergrundfarbe der Zellen weiaa
                    sp.CellAppearance.ForeColor = Color.Black;                  // Schwarze Schriftfarbe

                    ueberSchriftFarbe = Color.Black;
                    hinterGrundFarbe = Color.DarkGray;
                }

                this.BearbeiteZeile1(sp.Index + 1, hinterGrundFarbe, ueberSchriftFarbe); // Zeile 1 bearbeiten

                var row = this.ultraGridStammDaten.DisplayLayout.Rows[0];
            }
        }

        private void BearbeiteZeile1(int spalte, Color hinterGrundFarbe, Color ueberSchriftFarbe)
        {
            var row = this.ultraGridStammDaten.DisplayLayout.Rows[0];
            var zelle = row.Cells[spalte - 1];                                  // Zelle in der Spalte ermitteln

            switch (spalte)
            {
                case 2:
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
                    zelle.Appearance.BackColor = hinterGrundFarbe;
                    zelle.Appearance.ForeColor = ueberSchriftFarbe;
                    zelle.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                    zelle.Appearance.FontData.Name = @"Arial";
                    break;

                default:
                    if ((spalte <= 57) && (spalte >= 42))
                    {
                        zelle.Appearance.BackColor = hinterGrundFarbe;
                        zelle.Appearance.ForeColor = ueberSchriftFarbe;
                        zelle.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                        zelle.Appearance.FontData.Name = @"Arial";
                    }
                    break;
            }
        }

        private void BearbeiteZeile2(int spalte, Color hinterGrundFarbe, Color ueberSchriftFarbe)
        {
            var row = this.ultraGridStammDaten.DisplayLayout.Rows[0];
            var zelle = row.Cells[spalte - 1];                                  // Zelle in der Spalte ermitteln

            switch (spalte)
            {
                case 2:
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
                    zelle.Appearance.BackColor = hinterGrundFarbe;
                    zelle.Appearance.ForeColor = ueberSchriftFarbe;
                    zelle.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                    zelle.Appearance.FontData.Name = @"Arial";
                    break;

                default:
                    if ((spalte <= 57) && (spalte >= 42))
                    {
                        zelle.Appearance.BackColor = hinterGrundFarbe;
                        zelle.Appearance.ForeColor = ueberSchriftFarbe;
                        zelle.Appearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                        zelle.Appearance.FontData.Name = @"Arial";
                    }
                    break;
            }
        }
    }
}