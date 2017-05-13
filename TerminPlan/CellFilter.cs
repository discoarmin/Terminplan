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
    using Terminplan;

    public class CellFilter : IUIElementDrawFilter, IDisposable
    {
        private StammDaten frmStammDaten;                                               // Das Stammdaten-Formular

        public CellFilter(StammDaten stammDaten)
        {
            frmStammDaten = stammDaten;                                         // Formular merken
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
                    var tagEintrag = cell.Tag.ToString();                       // Tag-Eintrag der Zelle ermitteln

                    if (tagEintrag.Contains(@"Kommentar"))
                    {
                        // Kennzeichnung für Kommentar zeichnen -->
                        // Punkte erstelllen, die ein Dreieck definieren
                        var point1 = new Point(rect.X + rect.Width - 10, rect.Y);
                        var point2 = new Point(rect.X + rect.Width, rect.Y);
                        var point3 = new Point(rect.X + rect.Width, rect.Y + 10);
                        Point[] curvePoints = { point1, point2, point3 };

                        if (!cell.Tag.ToString().Contains(@"Kommentar")) return false; // Abbruch, da Zelle keinen Kommentar enthält
                        var redBrush = new SolidBrush(Color.Red);                   // Dreick wir in roter Farbe gezeichnet
                        drawParams.Graphics.FillPolygon(redBrush, curvePoints);     // Dreieck zeichnen.
                    }
                    else if (tagEintrag.Contains(@"anzBloecke"))
                    {
                        var point1 = new Point(rect.Left + 5, rect.Top + rect.Height / 2);  // Anzeigestelle des Texteditors berechnen
                        frmStammDaten.ultraTextEditorBloecke.Location = point1; // Anzeigestelle für den Texteditor festlegen
                        frmStammDaten.ultraTextEditorBloecke.Visible = true;
                    }
                }

                return true;                                                    // Kennzeichnung der Zelle erfolgreich
            }
            else if (drawParams.Element is MergedCellUIElement)
            {
                // Es sind verbundene Zellen
                var cell = drawParams.Element.GetContext(typeof(UltraGridCell), true) as UltraGridCell; // die zu zeichnende Zelle ermitteln

                // Das Editor-Control ist in Spalte 'B'
                if (cell.Column.Index == 1)
                {
                    var mergedCells = cell.GetMergedCells();                    // Die verbundenen Zellen ermitteln

                    // Alle gefundenen Verbundene Zellen nach einem Tag durchsuchen
                    for (var i = 0; i < mergedCells.Length; i++)
                    {
                        if (mergedCells[i].Tag != null)
                        {
                            var tagEintrag = mergedCells[i].Tag.ToString();     // Tag-Eintrag der Zelle ermitteln

                            // Es soll ein UltraTextEditor zur Eingabe der Anzahl anzuzeigender Blöcke angezeigt werden
                            if (tagEintrag.Contains(@"anzBloecke"))
                            {
                                var rect1 = drawParams.Element.Rect;            // Maße der Zelle ermitteln
                                var point1 = new Point(rect1.Left + 5, rect1.Top + 5);  // Anzeigestelle des Texteditors berechnen
                                frmStammDaten.ultraTextEditorBloecke.Location = point1; // Anzeigestelle für den Texteditor festlegen
                                frmStammDaten.ultraTextEditorBloecke.Visible = true;
                                return true;                                    // Control erfolgreich gezeichnet
                            }
                        }
                    }
                }
            }

            return false;                                                       // Es wirk keine Zelle gezeichnet
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