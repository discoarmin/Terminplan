// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HauptFormPart1.Cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Zusammenfassung für HauptForm, ausgelagerte Methoden.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  07.03.17  br      Grundversion
//      </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------
// ReSharper disable CatchAllClause
// ReSharper disable UnusedParameter.Local
namespace Terminplan
{
    using System;
    using System.Data;
    using System.Data.OleDb;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Windows.Forms;
    using Resources = Properties.Resources;

    /// <summary>
    /// Zusammenfassung für HauptForm.
    /// </summary>
    [SuppressMessage("ReSharper", "EmptyGeneralCatchClause")]
    [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
    public partial class TerminPlanForm
    {
        private void AnzeigeDatumEinstellen()
        {
            // Alle Einträge im Grid der GanttView durchsuchen nach dem kleinsten StartDatum
            var anzeigeDatum = DateTime.MaxValue;                               // Zum Vergleich das höchstmögliche Datum vorgeben
            var verfahrensDatum = DateTime.Now;                                 // Eingegebenes Datum
            var anzZeilen = ultraGanttView1.Project.Tasks.Count;                // Anzahl vorhandener Arbeitsinhalte
            var zaehler = anzZeilen;                                            // Schleifenzähler
            var anzAufgaben = 0;                                                // Anzahl Aufgaben eines Arbeitsinhalts
            
            // Alle vorhandenen Arbeitsinhalt bearbeiten
            for (var i = 0; i < zaehler; i++)
            {
                verfahrensDatum = ultraGanttView1.Project.Tasks[i].StartDateTime; // Startdatum des Arbeitsinhalts ermitteln
                anzeigeDatum = VergleicheDatum(verfahrensDatum,
                    anzeigeDatum);                                              // Startdatum mit dem bisher niedrigsten Datumswert vergleichen               
                anzAufgaben = ultraGanttView1.Project.Tasks[i].Tasks.Count;     // Anzahl Aufgaben des Arbeitsinhalts
                
                // Alle vorhandenen Aufgaben des Arbeitsinhalts
                for (var a = 0; a < anzAufgaben; a++) 
                {
                    verfahrensDatum = ultraGanttView1.Project.Tasks[i].Tasks[a].StartDateTime; // Startdatum der Aufgabe ermitteln
                    anzeigeDatum = VergleicheDatum(verfahrensDatum,
                        anzeigeDatum);                                          // Startdatum mit dem bisher niedrigsten Datumswert vergleichen               
                }
            }
            
            ultraGanttView1.EnsureDateTimeVisible(anzeigeDatum);                // Zeitleiste so verschieben, dass das Startdatum angezeigt wird           
        }

        private static DateTime VergleicheDatum(DateTime datum1, DateTime datum2)
        {
            var result = DateTime.Compare(datum1, datum2);
            
            // Vergleich auswerten
            return result <= 0 ? datum1 : datum2;
        }
    }
}