// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BordersDrawFilter.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Definiert einen Drawfilter zum Unterdrücken der Zellen-Rahmen in einem Grid.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  06.03.17  br      Grundversion
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Infragistics.DrawFilters
{
    using System.Diagnostics;
    using System.Windows.Forms;
    using Win;
    using Win.UltraWinGrid;

    public class BordersDrawFilter : IUIElementDrawFilter
    {
        #region Variablen
        /// <summary>Spaltenindex: Falls der Wert > -1 ist, wirkt der Filter nur bis zu der angegebenen Spalte</summary>
        readonly int spaltenIndex;

        /// <summary>Gibt die Seiten an, bei welchen der Rahmen nicht gezeichnet werden soll</summary>
        private readonly Border3DSide side;

        #endregion Variablen
        
        #region Konstruktor
        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="BordersDrawFilter" /> Klasse.
        /// </summary>
        /// <param name="pos">welche Seite nicht gezeichnet werden soll</param>
        /// <remarks>0 = kein Rahmen, 1 = horizontal, 2 = vertikal</remarks>
        /// <param name="filterGrenze">Spaltenindex, bis zu welcher Spalte der Filter gelten soll</param>
        /// <remarks>-1 = alle Spalten, anderer Wert: Index der ersten nicht zu bearbeitenden Spalte</remarks>
        public BordersDrawFilter(int pos = 0, int filterGrenze = -1)
        {
            // Übergebene Variablen speichern
            spaltenIndex = filterGrenze;

            // Einstellen, welche Seiten vom Rahmen nicht gezeichnet werden sollen
            switch (pos)
            {
                default:                                                        // Standardmäßig wird der Filter auf den gesamten Rahmen angewendet
                    side = Border3DSide.All;
                    break;

                case 1:                                                         // horizontale Linien sollen nicht gezeichnet werden
                    side = Border3DSide.Top | Border3DSide.Bottom;
                    break;

                case 2:                                                         // vertikale Linien sollen nicht gezeichnet werden
                    side = Border3DSide.Left | Border3DSide.Right;
                    break;
            }
        }    
        #endregion Konstruktor
    
        #region IUIElementDrawFilter Members
        bool IUIElementDrawFilter.DrawElement(DrawPhase drawPhase, ref UIElementDrawParams drawParams)
        {
            // Holt eine Zelle. 
            var cell = drawParams.Element.GetContext(typeof(UltraGridCell)) as UltraGridCell;
            if (cell == null)
            {
                // Hat nicht funktioniert
                Debug.Fail(@"Es ist nicht möglich, eine Zelle aus dem Element zu bekommen. unerwartet.");
                return false;                                                   // Kein Element gefunden
            }            
    
            // Bestimmen, ob die Spalte Grenzen haben soll oder nicht.
            if (spaltenIndex >= 0)
            {
                // Es gibt eine Grenze, also Filter nur bis zu dieser Grenze anwenden
                if (cell.Column.Index >= spaltenIndex) return false;            // <c>false</c> zurückgeben, so dass die Spaltengrenzen im Grind normal gezeichnet werden. 

                // Diese Zelle soll keinen Rahmen zeichnen. Es könnte hier einfach <c>true</c> zurückgegeben werden, aber das
                // würde dort eine Lücke hinterlassen, wo der Zellenrahmen gewesen ist und die Farbe der Zeile würde durchscheinen.
                // Allerdings zeigen. Es muss also eine durchgezogener Rahmen mit der Hintergrundfarbe der Zelle gezeichnet werden.                    
                drawParams.DrawBorders(UIElementBorderStyle.Solid, side, drawParams.AppearanceData.BackColor, drawParams.Element.Rect, drawParams.Element.ClipRect);
        
                // <c>true</c> zurückgeben, so dass dem Grind bekannt ist, dass das Zeichnen schon durchgeführt wurde. 
                return true;
            }
            
            // Der Filter soll auf das gesamte Grid angewendet werden
            drawParams.DrawBorders(UIElementBorderStyle.Solid, side, drawParams.AppearanceData.BackColor, drawParams.Element.Rect, drawParams.Element.ClipRect);

            // <c>true</c> zurückgeben, so dass dem Grind bekannt ist, dass das Zeichnen schon durchgeführt wurde. 
            return true;
        }
    
        DrawPhase IUIElementDrawFilter.GetPhasesToFilter(ref UIElementDrawParams drawParams)
        {
            // Behandelt das Zeichnen der Rahmen für CellUIElements
            if (drawParams.Element is CellUIElement)
            {
                return DrawPhase.BeforeDrawBorders;
            }
            
            return DrawPhase.None;                                              // Es muss nichts getan werden
        }
        
        //private void expPdfExporter_RowExported(System.Object sender, Win.UltraWinGrid.DocumentExport.RowExportedEventArgs e)
        //{
        //    var row = e.GridRow;                                                // Zu bearbeitende Zeile
            
        //    if (lstUnderLine.ContainsKey(row.Index) | lstOverLine.ContainsKey(row.Index)) {
        //        // This will only be called for the BeforeDrawBorders phase of a
        //        // RowCellAreaUIElement based on the flags returned from GetPhasesToFilter.
        
        //        // Draw a border along the top edge of the element.
        //        e.ReportRow.Borders = new Infragistics.Documents.Report.Borders(null, null, Infragistics.Documents.Graphics.Pens.Black, null);
        
        
        //        //if it is the last row draw a line at the bottom
        //        if (row.HasNextSibling == false) {
        //            e.ReportRow.Borders = new Infragistics.Documents.Report.Borders(null, null, Infragistics.Documents.Graphics.Pens.Black, Infragistics.Documents.Graphics.Pens.Black);
        //        }
        //    }
        //}        
        #endregion IUIElementDrawFilter Members
    }
}