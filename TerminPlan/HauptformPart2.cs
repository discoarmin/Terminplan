// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HauptformPart2.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Ausgelagerte Funktionen des Hauptformulars.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  30.01.17  br      Grundversion
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Terminplan
{
    using Infragistics.Win.UltraWinSchedule;

    /// <summary>
    /// Klasse TerminPlanForm (Hauptformular).
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class TerminPlanForm
    {
        /// <summary>Setzt den Text des Headers einer Spalte</summary>
        /// <param name="column">Die Spate.</param>
        /// <param name="text">anzuzeigender Text.</param>
        private static void SetColumnHeaderText(TaskField column, string text)
        {
            var resourceName = 
            string.Format(@"TaskProxy_ProertyName.{0}", column);
            
            
        }
        
        private static void SetColumnHeaders()
        {
        }

        /// <summary>Speichert die übergebene Datei</summary>
        /// <param name="dateiName">Name der zu speichernden Datei mit Pfadangabe.</param>
        private void Speichern(string dateiName)
        {
            this.datasetTp.
        }
    }
}
