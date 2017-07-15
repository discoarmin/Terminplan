// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WinGridZoomGrid.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Zoomen eines Grids
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  14.10.16  br      Grundversion
//      </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------
namespace WinGridZoomGrid
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Infragistics.Win.UltraWinGrid;

    /// <summary>Klasse zum Zoomen des Inhalts eines Grids</summary>
    public class WinGridZoomGrid
    {
        /// <summary>Klasse für die Spaltengrösse eines Grids eines Grids</summary>
        public class ColumnSize
        {
            /// <summary>Holt oder setzt den Schlüssel einer Spalte</summary>
            public string Key { get; set; }

            /// <summary>Holt oder setzt die Breite einer Spalte</summary>
            public float Width { get; set; }

            /// <summary>Holt oder setzt Index der Tabelle bei verschachtelten Tabellen</summary>
            public int BandIndex { get; set; }

            /// <summary>Setzt die Grösse einer Spalte</summary>
            /// <param name="cKey">Der Schlüssel der zu bearbeitenden Spalte</param>
            /// <param name="cWidth">Einzustellende Breite</param>
            /// <param name="cBandIndex">Index der Tabelle</param>
            public ColumnSize(string cKey, float cWidth, int cBandIndex)
            {
                Key = cKey;
                Width = cWidth;
                BandIndex = cBandIndex;
            }

        }

        /// <summary>Klasse für die Zeilengrösse eines Grids eines Grids</summary>
        public class RowSize
        {
            /// <summary>Holt oder setzt den Index einer Zeile</summary>
            public int Index { get; set; }

            /// <summary>Holt oder setzt die Höhe einer Zeile</summary>
            public float Height { get; set; }

            /// <summary>Holt oder setzt Index der Tabelle bei verschachtelten Tabellen</summary>
            public int BandIndex { get; set; }

            /// <summary>Setzt die Grösse einer Spalte</summary>
            /// <param name="cIndex">Index der zu bearbeitenden Zeile</param>
            /// <param name="cHeight">Einzustellende Höhe</param>
            /// <param name="cBandIndex">Index der Tabelle</param>
            public RowSize(int cIndex, float cHeight, int cBandIndex)
            {
                this.Index = cIndex;
                this.Height = cHeight;
                this.BandIndex = cBandIndex;
            }
        }

        /// <summary>Klasse für die Aufnahme der Zoomeigenschaften</summary>
        public class GridZoomProperty
        {
            /// <summary>Holt oder setzt die Originalgrösse einer Spalte</summary>
            public List<ColumnSize> OriginCs { get; set; }

            /// <summary>Holt oder setzt die Originalgrösse einer Zeile</summary>
            public List<RowSize> OriginRs { get; set; }

            /// <summary>Holt oder setzt die originale Schriftgrösse</summary>
            public float OriginFSize { get; set; }

            /// <summary>Holt den Zoomfaktor</summary>
            public int ZoomFaktor { get; set; }

            /// <summary>
            /// Initialisiert eine neue Instanz der <see cref="GridZoomProperty" /> Klasse.
            /// </summary>
            public GridZoomProperty()
            {
                this.OriginCs = new List<ColumnSize>();                         // Liste für die originale Spaltengrösse
                this.OriginRs = new List<RowSize>();                            // Liste für die originale Zeilengrösse
                this.OriginFSize = 1;
                this.ZoomFaktor = 100;                                          // Zoomfaktor ist 100%
            }

            /// <summary>
            /// Initialisiert eine neue Instanz der <see cref="GridZoomProperty" /> Klasse.
            /// </summary>
            /// <param name="originCSize">Liste für die orfiginale Spaltengrösse</param>
            /// <param name="originRSize">Liste für die originale Zeilengrösse</param>
            /// <param name="originFontSize">Liste für die originale Schriftgrösse</param>
            public GridZoomProperty(List<ColumnSize> originCSize,
                List<RowSize> originRSize,
                float originFontSize)
            {
                this.OriginFSize = originFontSize;
                this.OriginCs = originCSize;
                this.OriginRs = originRSize;
                this.ZoomFaktor = 100;                                          // Zoomfaktor ist 100%
            }

            /// <summary>Ermittelt die originalen Grideigenschaften, weche gezoomt werden sollen</summary>
            public void GetGridOriginZoomProperty(UltraGrid grid)
            {
                this.OriginFSize = grid.Font.Size;                              // Schriftgrösse

                // Bei hierarchischen Grids alle Hierarchien durchgehen
                // a) Spalten einer Hierarchie bearbeiten
                foreach (var addcol in grid.DisplayLayout.Bands.Cast<UltraGridBand>().SelectMany(band =>
                    (from UltraGridColumn col in band.Columns select new ColumnSize(col.Key, col.Width, band.Index))))
                {
                    this.OriginCs.Add(addcol);                                  // Spalte der Liste für die originale Spaltengrösse hinzufügen
                }

                // b) Zeilen einer Hierarchie bearbeiten
                for (var i = 0; i < grid.Rows.Count; i++)
                {
                    var addrow = new RowSize(grid.Rows[i].Index, grid.Rows[i].Height, grid.Rows[i].Band.Index);
                    this.OriginRs.Add(addrow);                                  // Zeile der Liste für die originale Zeilenngrösse hinzufügen
                }

                //foreach (var addrow in grid.Rows.Select(row => new RowSize(row.Index, row.Height, row.Band.Index)))
                //{
                //    this.OriginRs.Add(addrow);                                  // Zeile der Liste für die originale Zeilenngrösse hinzufügen
                //}
            }

            /// <summary>
            /// Initialisiert eine neue Instanz der <see cref="GridZoomProperty" /> Klasse.
            /// </summary>
            /// <param name="zoomIndex">Zoomfaktor in %, um welchen das Grid verändert werden soll</param>
            /// <param name="grid">Das zu verändernde Grid</param>
            public void ZoomGrid(float zoomIndex, UltraGrid grid)
            {
                this.ZoomFaktor = (int)zoomIndex*100;                           // Zoomfaktor merken

                // Jede Spalte aus der Originalliste verändern
                foreach (var cs in this.OriginCs)
                {
                    try
                    {
                        grid.DisplayLayout.Bands[cs.BandIndex].Columns[cs.Key].Width =
                    (int)(zoomIndex * cs.Width);
                    }
                    catch { }
                }

                // Jede Zeile aus der Originalliste verändern
                foreach (var rs in this.OriginRs)
                {
                    try
                    {
                        grid.DisplayLayout.Rows[rs.Index].Height =
                            (int)(zoomIndex * rs.Height);
                    }
                    catch { }
                }

                // Schriftgröße des Grids einstellen
                var gridf = grid.Font;
                var f = new Font(gridf.FontFamily, OriginFSize * zoomIndex, gridf.Style);
                grid.Font = f;
            }
        }
    }
}
