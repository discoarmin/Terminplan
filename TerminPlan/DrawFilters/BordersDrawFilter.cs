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
    public class BordersDrawFilter : IUIElementDrawFilter
    {
        #region Variablen
        /// <summary>Merker, ob die horizontalen Linien entfernt werden sollen</summary>
        bool linieHorizontal;
        
        /// <summary>Spaltenindex: Falls der Wert > -1 ist, wirkt der Filter nur bis zu der angegebenen Spalte</summary>
        int spaltenIndex;
        #endregion Variablen
        
        #region Konstruktor
        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="BordersDrawFilter" /> Klasse.
        /// </summary>
        public BordersDrawFilter(bool horizontal = true, int filterGrenze = -1)
        {
            // Übergebene Variablen speichern
            this.linieHorizontal = horizontal;
            this.spaltenIndex = filterGrenze;
        }    
        #endregion Konstruktor
    
        #region IUIElementDrawFilter Members
        bool IUIElementDrawFilter.DrawElement(DrawPhase drawPhase, ref UIElementDrawParams drawParams)
        {
            // Holt eine Zelle. 
            UltraGridCell cell = drawParams.Element.GetContext(typeof(UltraGridCell)) as UltraGridCell;
            if (cell == null)
            {
                // Hat nicht funktioniert
                Debug.Fail("Es ist nicht möglich, eine Zelle aus dem Element zu bekommen. unerwartet.");
                return false;                                                   // Kein Element gefunden
            }            
    
            // Bestimmen, ob die Spalte Grenzen haben soll oder nicht.
            if (filterGrenze >= 0)
            {
                // Es gibt eine Grenze, also Filter nur bis zu dieser Grenze anwenden
                if (cell.Column.Index < filterGrenze)
                {
                    // Diese Zelle soll keinen Rahmen zeichnen. Es könnte hier einfach <c>true</c> zurückgegeben werden, aber das
                    // würde dort eine Lücke lassen, wo der Zellenrahmen gewesen ist und die Farbe der Zeile würde durchscheinen.
                    // Allerdings zeigen. Es muss also eine durchgezogener Rahmen mit der Hintergrundfarbe der Zelle gezeichnet werden.                    
                    drawParams.DrawBorders(UIElementBorderStyle.Solid, Border3DSide.All, drawParams.AppearanceData.BackColor, drawParams.Element.Rect, drawParams.Element.ClipRect);
        
                    // <c>true</c> zurückgeben, so dass dem Grind bekannt ist, dass das Zeichnen schon durchgeführt wurde. 
                    return true;
                }
                else
                {
                    // <c>false</c> zurückgeben, so dass die Spaltengrenzen im Grind normal gezeichnet werden. 
                    return false;
                }                                
            }
    
            // <c>false</c> zurückgeben, so dass die Spaltengrenzen im Grind normal gezeichnet werden. 
            return false;
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
        
        private void expPdfExporter_RowExported(System.Object sender, Infragistics.Win.UltraWinGrid.DocumentExport.RowExportedEventArgs e)
        {
        	UltraGridRow row = e.GridRow;
            
        	if (lstUnderLine.ContainsKey(row.Index) | lstOverLine.ContainsKey(row.Index)) {
        		// This will only be called for the BeforeDrawBorders phase of a
        		// RowCellAreaUIElement based on the flags returned from GetPhasesToFilter.
        
        		// Draw a border along the top edge of the element.
        		e.ReportRow.Borders = new Infragistics.Documents.Report.Borders(null, null, Infragistics.Documents.Graphics.Pens.Black, null);
        
        
        		//if it is the last row draw a line at the bottom
        		if (row.HasNextSibling == false) {
        			e.ReportRow.Borders = new Infragistics.Documents.Report.Borders(null, null, Infragistics.Documents.Graphics.Pens.Black, Infragistics.Documents.Graphics.Pens.Black);
        		}
        	}
        }        
        #endregion IUIElementDrawFilter Members
    }
}