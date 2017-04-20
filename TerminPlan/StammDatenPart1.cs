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
    using System.Globalization;
    using System.Windows.Forms;
    using Infragistics.Win.AppStyling;
    using Infragistics.Win.Printing;
    using Infragistics.Win.UltraWinToolbars;

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
    }
}