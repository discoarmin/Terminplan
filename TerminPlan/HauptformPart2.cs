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
    using System.IO;
    using System.Windows.Forms;

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

        private void SetResourceStrings()
        {
            // Jedes Element muss einzeln eingestellt werden
            var rc = Infragistics.Win.UltraWinGanttView.Resources.Customizer;        // Zum Ändern der anzuzeigenden Texte in der GanttView

            // Context-Menü
            rc.SetCustomizedString("GanttViewContextMenuItem_AddSubTask_Text", "Unterpunkt hinzufügen für '{0}'");  // Add sub-task for '{0}
            rc.SetCustomizedString("GanttViewContextMenuItem_CollapseTask_Text", "Unterbegriffe ausblenden");       // Collaps Tasks
            rc.SetCustomizedString("GanttViewContextMenuItem_DisableTask_Text", "Vorgang deaktivieren");            // Inactivate Task
            rc.SetCustomizedString("GanttViewContextMenuItem_EnableTask_Text", "Vorgang aktivieren");               // Activate Task
            rc.SetCustomizedString("GanttViewContextMenuItem_ExpandTask_Text", "Unterpunkte anzeigen");             // Show sub-items
            rc.SetCustomizedString("GanttViewContextMenuItem_IndentTask_Text", "Vorgang nach links verschieben");   // Indent Task
            rc.SetCustomizedString("GanttViewContextMenuItem_InsertTask_Text", "Vorgang einfügen");                 // Insert Task
            rc.SetCustomizedString("GanttViewContextMenuItem_OutdentTask_Text", "Vorgang nach rechts verschieben"); // Outdent Task
            rc.SetCustomizedString("GanttViewContextMenuItem_RemoveTask_Text", "Vorgang löschen");                  // Delete Task
            rc.SetCustomizedString("GanttViewContextMenuItem_ShowDialog_Text", "Informationen über den Vorgang");   // Task Information

            // Fehlermeldungen im Grid
            rc.SetCustomizedString("Grid_Error_MessageBoxText_Generic", "Der Wert "{0}" ist ungültig für '{1}'.");  // Unterpunkt hinzufügen
            rc.SetCustomizedString("Grid_Error_MessageBoxTitle_Generic", "Fehler.");                                // Error
            rc.SetCustomizedString("Grid_Error_MessageBoxTitle_TaskException", "Fehler beim Vorgang");              // Task Error




        }

        /// <summary>Speichert die übergebene Datei</summary>
        /// <param name="dateiName">Name der zu speichernden Datei mit Pfadangabe.</param>
        private void Speichern(string dateiName)
        {
            //var writer = new System.IO.StreamWriter();
            this.datasetTp.WriteXml(dateiName, System.Data.XmlWriteMode.WriteSchema);
        }

        /// <summary>Speichert die übergebene Datei unter einem anderen Namen</summary>
        /// <param name="dateiName">Name der zu speichernden Datei mit Pfadangabe.</param>
        private void SpeichernUnter(string dateiName)
        {
            // Pfadangabe und Dateinamen trennen
            if (dateiName == null) return;                                      // Falls nichts übergeben wurde, kann hier abebrochen werden

            var directoryName = Path.GetDirectoryName(dateiName);
            var fileBane = Path.GetFileName(dateiName);

            // Speichern-Dialog anzeigen
            SaveFileDialog saveFileDialog1 = new SaveFileDialog()
            {
                Filter = "XML Dateien|*.xml",
                Title = "Terminplan speichern"
            };
            saveFileDialog1.ShowDialog();

            // Falls ein Name eingegeben ist, kann die Datei jetzt gespeichert werden.
            if (saveFileDialog1.FileName != "")
            {
                this.Speichern(saveFileDialog1.FileName);
            }
        }
    }
}
