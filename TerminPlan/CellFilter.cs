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

    public class CellFilter : IUIElementDrawFilter, IDisposable
    {
        public CellFilter(UltraGrid grid)
        {
        }

        #region IUIElementDrawFilter

        bool IUIElementDrawFilter.DrawElement(DrawPhase drawPhase, ref UIElementDrawParams drawParams)
        {
            // Ermitteln, ob eine Zelle gezeichnet werden soll
            if (drawParams.Element is CellUIElement)
            {
                var cell = drawParams.Element.GetContext(typeof(UltraGridCell), true) as UltraGridCell; // die zu zeichnende Zelle ermitteln
                if (cell != null && cell.Column.Index != 0 && cell.Tag != null)
                {
                    var rect = drawParams.Element.Rect;                         // Maße der Zelle ermitteln

                    // Punkte erstelllen, die ein Dreieck definieren
                    var point1 = new Point(rect.X + rect.Width - 10, rect.Y);
                    var point2 = new Point(rect.X + rect.Width, rect.Y);
                    var point3 = new Point(rect.X + rect.Width, rect.Y + 10);
                    Point[] curvePoints = { point1, point2, point3 };

                    var redBrush = new SolidBrush(Color.Red);

                    switch (cell.Tag.ToString())
                    {
                        default:                                                // Standardmässig wird rot eingestellt
                            // Dreieck zeichnen.
                            drawParams.Graphics.FillPolygon(redBrush, curvePoints);
                            break;

                        case @"Kommentar2":
                            redBrush = new SolidBrush(Color.FromArgb(254, 0, 0));
                            // Dreieck zeichnen.
                            drawParams.Graphics.FillPolygon(redBrush, curvePoints);
                            break;

                        case @"Kommentar3":
                            redBrush = new SolidBrush(Color.FromArgb(253, 0, 0));
                            // Dreieck zeichnen.
                            drawParams.Graphics.FillPolygon(redBrush, curvePoints);
                            break;

                        case @"Kommentar4":
                            redBrush = new SolidBrush(Color.FromArgb(253, 0, 0));
                            // Dreieck zeichnen.
                            drawParams.Graphics.FillPolygon(redBrush, curvePoints);
                            break;
                    }

                    // }
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

        #endregion IUIElementDrawFilter
    }
}