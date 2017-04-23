// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StammDatenExcel.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Excel-Daten lesen und schreiben.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  22.04.17  br      Grundversion
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Terminplan
{
    using System;
    using System.Data;
    using System.Windows.Forms;
    using Infragistics.Documents.Excel;
    using Infragistics.Win.UltraWinGrid;

    /// <summary>
    /// Klasse StammDaten.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class StammDaten : Form
    {
        #region Methoden

        /// <summary>Lädt eine Excel-Tabelle.</summary>
        /// <param name="excelGrid">Grid, welches die Excel-Daten aufnimmt.</param>
        /// <param name="datei">Die zu ladende Excel-Datei.</param>
        /// <param name="workSheet">Das zu ladende Arbeitsblatt.</param>
        private void LadeExcelTabelle(ref UltraGrid excelGrid, string datei, string workSheet)
        {
            var wb = Workbook.Load(datei);                                      // Excel-Tabelle laden

            // Erstellen einer DataTable, welche verwendet wird, um Daten aus der Excel-Datei zu speichern
            var dtExcel = new DataTable();

            object[] rowArray;                                                  // Array zu Aufnahme der Daten einer Zelle für einer Zeile
            var colCount = 0;                                                   // Anzahl Spalten in der zu lesenden Excel-Tabelle

            // Ermitteln, wie viele Spalten im Arbeitsblatt vorhanden sind
            var row = wb.Worksheets[workSheet].Rows[0];                         // Erste Zeile auswählen, um die vorhandenen Spalten zu ermitteln

            // Anzahl Zellen dieser Zeile ermitteln
            foreach (var cell in row.Cells)
            {
                colCount = cell.ColumnIndex + 1;                                // Erhöhe den Spaltenzähler um eins für jede vorhandene Spalte
            }

            // Auch nicht gefüllte Spalten hinzufügen
            foreach (var row1 in wb.Worksheets[workSheet].Rows)
            {
                int ind;                                                        // Spaltenindex einer Zelle

                // Jede Zelle innerhalb der gelesenen Zeile durchlaufen
                foreach (var cell in row1.Cells)
                {
                    ind = cell.ColumnIndex;                                     // Spaltenindex der Zelle ermiteln

                    // Falls die bis jetzt ermittelte Anzahl Spalten erreicht oder Überschritten ist,
                    // muss die entsprechende Anzahl an Spalten hinzugefügt werden
                    if (ind >= colCount)
                    {
                        colCount = ind + 1;                                     // Erhöhe den Spaltenzähler um eins für jede vorhandene Spalte
                    }
                }
            }

            colCount += 2;                                                      // Zur Sicherheit noch 2 Spalten hinzufügen

            // In der DataTable die Anzahl ermittelter Spalten erzeugen
            for (var c = 0; c <= colCount; c++)
            {
                dtExcel.Columns.Add();
            }

            // Jede Zeile des Arbeitsblatts durchlaufen
            foreach (var wrow in wb.Worksheets["Stammdaten"].Rows)
            {
                dtExcel.Rows.Add();                                             // Der DataTable für jede gelesene Zeile aus dem Arbeitsblatt eine Zeile hinzufügen
                rowArray = new object[colCount];                                // Erstellt eine neue Objekt-Array-Instanz, (erforderlich für das Einfügen von Daten in die DataRow)

                int ind;                                                        // Spaltenindex einer Zelle

                // Jede Zelle innerhalb der Zeile durchlaufen, um die Gösse des Arrays zur Aufnahme der Daten aller Zellen einer Zeile
                // an die Anzahl der Zellen anzupassen
                foreach (var cell in wrow.Cells)
                {
                    ind = cell.ColumnIndex;                                     // Spaltenindex der Zelle ermitteln

                    // Falls der Spaltenindex gleich oder grösser als die Göße des Arrays ist,
                    // muss die Grösse des Arrays angepasst werden
                    if (ind >= rowArray.GetLength(0) - 1)
                    {
                        Array.Resize(ref rowArray, ind + 1);
                    }

                    ind = cell.ColumnIndex;                                     // Spaltenindex der Zelle nochmals ermitteln
                    rowArray[ind] = cell.Value;                                 // Daten der Zelle in das Array eintragen
                }

                // Das ItemArray der aktuellen Zeile mit den Zelldaten der gerade gefüllten rowArray-Instanz füllen
                // Falls die Anzahl von Zeilen nicht ausreicht, neue hinzufügen
                if (wrow.Index >= dtExcel.Rows.Count)
                {
                    var diff = wrow.Index - dtExcel.Rows.Count;                 // Anzahl fehlender Zeilen in der Tabelle berechnen

                    // Überprüfen, wie viele Zeilen hinzugefügt erden müssn
                    if (diff == 0)
                    {
                        // Index der einzufügenden Zeile entspricht der Anzahl vorhandener Zeilen,
                        // also muss nur eine Zeile hinzugefügt werden
                        dtExcel.Rows.Add();                                     // Zeile hinzufügen
                    }
                    else
                    {
                        for (var i = 0; i <= diff; i++)
                        {
                            dtExcel.Rows.Add();                                 // Zeile hinzufügen
                        }
                    }
                }

                // Versuchen, die Zeile in der Datentabelle abzulegen. Tritt dabei eine Ausnahme auf,
                // sind zu wenig Zeilen vorhanden
                try
                {
                    dtExcel.Rows[wrow.Index].ItemArray = rowArray;              // versuchen, die gelesene Zeile in die DataTable einzutragen
                }
                catch (IndexOutOfRangeException)
                {
                    // Es sind nicht genügend Zeilen vorhanden. Die noch fehlenden Zeilen müssen hizugefügt werden
                    var diff = wrow.Index - dtExcel.Rows.Count;                 // Anzahl fehlender Zeilen in der Tabelle berechnen
                    if (diff == 0)
                    {
                        // Index der einzufügenden Zeile entspricht der Anzahl vorhandener Zeilen,
                        // also muss nur eine Zeile hinzugefügt werden
                        dtExcel.Rows.Add();                                     // Zeile hinzufügen
                    }
                    else
                    {
                        // Es fehlen mehrere Zeilen. Diese hinzufügen
                        for (var i = 0; i <= diff; i++)
                        {
                            dtExcel.Rows.Add();                                 // Zeile hinzufügen
                        }
                    }

                    dtExcel.Rows[wrow.Index].ItemArray = rowArray;              // jetzt nochmals versuchen, die gelesene Zeile in die DataTable einzutragen
                }
            }

            dtExcel.Rows[0].Delete();                                           // Die 1. Zeile löschen

            excelGrid.DataSource = dtExcel;                                     // Die Datenquelle dem Grid zuweisen
            excelGrid.DataBind();                                               // Damit die gelesenen Daten angezeigt werden
        }
    }

    #endregion Methoden
}