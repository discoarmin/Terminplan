// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CellFilters.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Definiert einen Drawfilter zum Anzeigen eines Kommentars in einer Zelle wie in Excel.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  08.05.17  br      Grundversion
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Infragistics.DrawFilters
{
    using System;
    using System.Drawing;
    using Infragistics.Win;
    using Infragistics.Win.UltraWinGrid;

    class CellFilter: IUIElementDrawFilter, IDisposable
    {
        public CellFilter(UltraGrid grid)
        {

        }

        #region IUIElementDrawFilter 

        bool IUIElementDrawFilter.DrawElement(DrawPhase drawPhase, ref UIElementDrawParams drawParams, bool alternateMarker = false)
        {
            // Ermitteln, ob eine Zelle gezeichnet werden soll
            if (drawParams.Element is CellUIElement)
            {
                var cell = drawParams.Element.GetContext(typeof(UltraGridCell), true) as UltraGridCell; // die zu zeichnende Zelle ermitteln
                if (cell != null && cell.Column.Index != 0 && cell.Tag != null)
                {
                    var rect = drawParams.Element.Rect;                 // Maße der Zelle ermittel

                    // Punkte erstelllen, die ein Dreieck definieren
                    Point point1 = new Point(rect.X + rect.Width* 7 / 8, rect.Y);
                    Point point2 = new Point(rect.X + rect.Width, rect.Y);
                    Point point3 = new Point(rect.X + rect.Width, rect.Y + rect.Height / 4);
                    Point[] curvePoints = { point1, point2, point3 };

                    if(alternateMarker)
                    {
                        using (SolidBrush blueBrush = new SolidBrush(cell.Column.Index % 2 == 0 ? Color.Blue : Color.Red))
                        {
                            // Dreieck zeichnen.
                            drawParams.Graphics.FillPolygon(blueBrush, curvePoints);
                        }
                    }
                    else
                    {
                        using (SolidBrush blueBrush = new SolidBrush(Color.Red))
                        {
                            // Dreieck zeichnen.
                            drawParams.Graphics.FillPolygon(blueBrush, curvePoints);
                        }
                    }
                }

                return true;
            }

            return false;

        }

        DrawPhase IUIElementDrawFilter.GetPhasesToFilter(ref UIElementDrawParams drawParams)
        {
            return DrawPhase.AfterDrawElement;
        }

        public void Dispose()
        {
            this.Dispose();
        }

        #endregion
    }
}
