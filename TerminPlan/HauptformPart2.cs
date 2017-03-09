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
    using System.Data;
    using System.IO;
    using System.Windows.Forms;
    using Resources = Infragistics.Win.UltraWinGanttView.Resources;

    /// <summary>
    /// Klasse TerminPlanForm (Hauptformular).
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class TerminPlanForm
    {
        /// <summary>Deutsche Texte in der GanttView-Dll setzen</summary>
        private void SetResourceStrings()
        {
            // Jedes Element muss einzeln eingestellt werden
            var rc = Resources.Customizer;        // Zum Ändern der anzuzeigenden Texte in der GanttView

            // Context-Menü
            rc.SetCustomizedString(@"GanttViewContextMenuItem_AddSubTask_Text", @"Unterpunkt hinzufügen für '{0}'");  // Add sub-task for '{0}
            rc.SetCustomizedString(@"GanttViewContextMenuItem_CollapseTask_Text", @"Unterbegriffe ausblenden");       // Collaps Tasks
            rc.SetCustomizedString(@"GanttViewContextMenuItem_DisableTask_Text", @"Vorgang deaktivieren");            // Inactivate Task
            rc.SetCustomizedString(@"GanttViewContextMenuItem_EnableTask_Text", @"Vorgang aktivieren");               // Activate Task
            rc.SetCustomizedString(@"GanttViewContextMenuItem_ExpandTask_Text", @"Unterpunkte anzeigen");             // Show sub-items
            rc.SetCustomizedString(@"GanttViewContextMenuItem_IndentTask_Text", @"Vorgang nach links verschieben");   // Indent Task
            rc.SetCustomizedString(@"GanttViewContextMenuItem_InsertTask_Text", @"Vorgang einfügen");                 // Insert Task
            rc.SetCustomizedString(@"GanttViewContextMenuItem_OutdentTask_Text", @"Vorgang nach rechts verschieben"); // Outdent Task
            rc.SetCustomizedString(@"GanttViewContextMenuItem_RemoveTask_Text", @"Vorgang löschen");                  // Delete Task
            rc.SetCustomizedString(@"GanttViewContextMenuItem_ShowDialog_Text", @"Informationen über den Vorgang");   // Task Information

            // Fehlermeldungen im Grid
            rc.SetCustomizedString(@"Grid_Error_MessageBoxText_Generic", @"Der Wert '{0}' ist ungültig für '{1}'.");  // Unterpunkt hinzufügen
            rc.SetCustomizedString(@"Grid_Error_MessageBoxTitle_Generic", @"Fehler.");                                // Error
            rc.SetCustomizedString(@"Grid_Error_MessageBoxTitle_TaskException", @"Fehler beim Vorgang");              // Task Error

            rc.SetCustomizedString(@"NewSubTask_Text", @"Neuer Unterpunkt");                                          // New sub-task
            rc.SetCustomizedString(@"NewTask_Text", @"Neuer Vorgang");                                                // New Task
            rc.SetCustomizedString(@"Task_Name_Unnamed", @"unbenannt");                                               // unnamed

            // Vorgangs-Dialog
            rc.SetCustomizedString(@"TaskDialog_AdvancedTab", @"Erweitert");                                          // Advanced
            rc.SetCustomizedString(@"TaskDialog_BtnCancel", @"Abbruch");                                              // Cancel
            rc.SetCustomizedString(@"TaskDialog_CalendarInfoNullError", @"CalendarInfo sollte niemals null zurückgeben.");  // CalendarInfo should never return null.
            rc.SetCustomizedString(@"TaskDialog_ErrorTitle", @"Fehler");                                              // Error
            rc.SetCustomizedString(@"TaskDialog_GeneralTab", @"Allgemein");                                           // General
            rc.SetCustomizedString(@"TaskDialog_InvalidOwnerError", @"Ungültiger Besitzer gefunden. Bitte einen gültigen Wert verwenden.");   // Invalid Owner found. Please use a valid value
            rc.SetCustomizedString(@"TaskDialog_InvalidResourceNameError", @"Bitte einen gültigen Ressourcennamen eingeben"); // Please enter a valid resource name
            rc.SetCustomizedString(@"TaskDialog_InvalidTabKeyError", @"Unbekannte Tabulatortaste in der FromTabDialogPage");  // Unknown tab key in FromTabDialogPage
            rc.SetCustomizedString(@"TaskDialog_InvalidTaskContraint", @"Bitte geben Sie einen gültigen Wert für die Einschränkung des Vorgangs ein.");       // Please enter a valid Task Constraint Value
            rc.SetCustomizedString(@"TaskDialog_InvalidTaskDependencyTypeError", @"Bitte einen gültigen Typ für die Abhängigkeit des Vorgangs eingeben.");    // Unterpunkt hinzufügen
            rc.SetCustomizedString(@"TaskDialog_InvalidTaskNameError", @"Bitte einen gültigen Vorgangsnamen eingeben.");  // Please enter a valid task name.
            rc.SetCustomizedString(@"TaskDialog_lblConstraintDate", @"Termin der Einschränkung:");                    // Constraint date:
            rc.SetCustomizedString(@"TaskDialog_lblConstraintTask", @"Vorgang einschränken");                         // Constrain Task
            rc.SetCustomizedString(@"TaskDialog_lblConstraintType", @"Art der Einschränkung:");                       // Constraint type:
            rc.SetCustomizedString(@"TaskDialog_lblDates", @"Termine");                                               // Dates
            rc.SetCustomizedString(@"TaskDialog_lblDeadline", @"Deadline:");                                          // Deadline:
            rc.SetCustomizedString(@"TaskDialog_lblDuration", @"Dauer:");                                             // Duration:
            rc.SetCustomizedString(@"TaskDialog_lblEndDate", @"Ende:");                                               // Finish:
            rc.SetCustomizedString(@"TaskDialog_lblMilestone", @"Vorgang als Meilenstein markieren");                 // Mark task as milestone
            rc.SetCustomizedString(@"TasKDialog_lblName", @"Name:");                                                  // Name:
            rc.SetCustomizedString(@"TaskDialog_lblNotes", @"Notizen:");                                              // Notes:
            rc.SetCustomizedString(@"TaskDialog_lblPrecentageComplete", @"Prozent fertiggestellt:");                  // Percent Complete:
            rc.SetCustomizedString(@"TaskDialog_lblPredecessors", @"Vorgänger:");                                     // Predecessors:
            rc.SetCustomizedString(@"TaskDialog_lblResources", @"Ressourcen:");                                       // Resources:
            rc.SetCustomizedString(@"TaskDialog_lblStartDate", @"Start:");                                            // Start:
            rc.SetCustomizedString(@"TaskDialog_NotesTab", @"Notitzen");                                              // Notes
            rc.SetCustomizedString(@"TaskDialog_NullOwnerError", @"Der Besitzerr darf nicht null sein.");             // The owner should not be null.
            rc.SetCustomizedString(@"TaskDialog_NumericEditorDurationMaskDays", @"d");                                // d
            rc.SetCustomizedString(@"TaskDialog_PredecessorsTab", @"Vorgänger");                                      // Predecessors
            rc.SetCustomizedString(@"TaskDialog_ProjectNullError", @"Das Projekt sollte niemals null zurückgeben.");  // Project should never return null.
            rc.SetCustomizedString(@"TaskDialog_ResourceNameCaption", @"Ressourcenname");                             // Resource Name
            rc.SetCustomizedString(@"TaskDialog_ResourcesTab", @"Ressourcen");                                        // Resources
            rc.SetCustomizedString(@"TaskDialog_RowNumberCaption", @"Zeilennummer");                                  // Row Number
            rc.SetCustomizedString(@"TaskDialog_ShowNotSupported",
                @"Der Vorgangs-Dialog unterstützt keine nicht-modale Anzeige. Verwenden Sie die ShowDialog-Methode, um den Dialog zu starten.");  // The TaskDialog does not support non-modal display. Use the ShowDialog method to launch the dialog.
            rc.SetCustomizedString(@"TaskDialog_TaskDependencyTypeCaption", @"Typ für die Abhängigkeit des Vorgangs");  // Task Dependency Type
            rc.SetCustomizedString(@"TaskDialog_TaskNameCaption", @"Name des Vorgangs");                              // Task Name
            rc.SetCustomizedString(@"TaskDialog_Title", @"Informationen zum Vorgang");                                // Task Information
            rc.SetCustomizedString(@"TaskDialog_UpdateFailedTitle", @"Update fehlgeschlagen");                        // Update Failed

            rc.SetCustomizedString(@"TaskProxy_PropertyDisplayName_Task.Constraint", @"Art der Einschränkung");       // Constraint Type
            rc.SetCustomizedString(@"TaskProxy_PropertyDisplayName_Task.ConstraintDateTime", @"Datum der Einschränkung");  // Constraint Date
            rc.SetCustomizedString(@"TaskProxy_PropertyDisplayName_Task.Deadline", @"Deadline");                      // Deadline
            rc.SetCustomizedString(@"TaskProxy_PropertyDisplayName_Task.Dependencies", @"Vorgänger");                 // Predecessors
            rc.SetCustomizedString(@"TaskProxy_PropertyDisplayName_Task.Duration", @"Dauer");                         // Duration
            rc.SetCustomizedString(@"TaskProxy_PropertyDisplayName_Task.EndDateTime", @"Ende");                       // Finish
            rc.SetCustomizedString(@"TaskProxy_PropertyDisplayName_Task.Milestone", @"Meilenstein");                  // Milestone
            rc.SetCustomizedString(@"TaskProxy_PropertyDisplayName_Task.Name", @"Vorgang");                           // Task Name
            rc.SetCustomizedString(@"TaskProxy_PropertyDisplayName_Task.Notes", @"Notitzen");                         // Notes
            rc.SetCustomizedString(@"TaskProxy_PropertyDisplayName_Task.PercentComplete", @"% Fertiggestellt");       // % Complete
            rc.SetCustomizedString(@"TaskProxy_PropertyDisplayName_Task.Resources", @"Ressourcennamen");              // Resource Name
            rc.SetCustomizedString(@"TaskProxy_PropertyDisplayName_Task.StartDateTime", @"Start");                    // Start

            // Zeitspanne
            rc.SetCustomizedString(@"TimeSpanEditor_DayDisplayString_Plural", @"Tage");                               // days
            rc.SetCustomizedString(@"TimeSpanEditor_DayDisplayString_Singular", @"Tag");                              // day
            rc.SetCustomizedString(@"TimeSpanEditor_HourDisplayString_Plural", @"h");                                 // hrs
            rc.SetCustomizedString(@"TimeSpanEditor_HourDisplayString_Singular", @"h");                               // hr
            rc.SetCustomizedString(@"TimeSpanEditor_MinuteDisplayString_Plural", @"min");                             // mins
            rc.SetCustomizedString(@"TimeSpanEditor_MinuteDisplayString_Singular", @"min");                           // min
            rc.SetCustomizedString(@"TimeSpanEditor_WeekDisplayString_Plural", @"Wochen");                            // wks
            rc.SetCustomizedString(@"TimeSpanEditor_WeekDisplayString_Singular", @"Woche");                           // wk

            // ToolTip
            rc.SetCustomizedString(@"ToolTipSettings_DefaultTitle", @"Vorgang");                                      // Task
            rc.SetCustomizedString(@"ToolTipSettings_DefaultTitle_Summary", @"Zusammenfassung");                      // Summary
            rc.SetCustomizedString(@"ToolTipSettings_DefaultTitle_Milestone", @"Meilenstein");                        // Milestone
            rc.SetCustomizedString(@"ToolTipSettings_DefaultTitle_PercentComplete", @"Fortschritt");                  // Progress
            rc.SetCustomizedString(@"ToolTipSettings_Prefix_CompleteThrough", @"Fertiggestellt durch");               // Complete Through
            rc.SetCustomizedString(@"ToolTipSettings_Prefix_Duration", @"Dauer");                                     // Duration
            rc.SetCustomizedString(@"ToolTipSettings_Prefix_Finish", @"Ende");                                        // Finish
            rc.SetCustomizedString(@"ToolTipSettings_Prefix_Name", @"Vorgang");                                       // Task
            rc.SetCustomizedString(@"ToolTipSettings_Prefix_PercentComplete", @"% Fertiggestellt");                   // % Complete
            rc.SetCustomizedString(@"ToolTipSettings_Prefix_Start", @"Start");                                        // Start
            rc.SetCustomizedString(@"ToolTipSettingsTaskDependency_Prefix_Dependent", @"Nach");                       // To
            rc.SetCustomizedString(@"ToolTipSettingsTaskDependency_Prefix_Predecessor", @"von");                      // From
            rc.SetCustomizedString(@"ToolTipSettingsTaskDependency_Prefix_TaskDependencyType", @"Verknüpfung mit Vorgang");  // Task Link
        }

        /// <summary>Speichert die übergebene Datei</summary>
        /// <param name="dateiName">Name der zu speichernden Datei mit Pfadangabe.</param>
        private void Speichern(string dateiName)
        {
            //var writer = new System.IO.StreamWriter();
            DatasetTp.AcceptChanges();                                          // Damit alle Änderungen übernommen werden
            var streamWrite = new FileStream
                    (dateiName, FileMode.Create);
            //this.DatasetTp.WriteXml(dateiName, XmlWriteMode.WriteSchema);
            DatasetTp.WriteXml(streamWrite, XmlWriteMode.WriteSchema);
        }

        /// <summary>Speichert die übergebene Datei unter einem anderen Namen</summary>
        /// <param name="dateiName">Name der zu speichernden Datei mit Pfadangabe.</param>
        private void SpeichernUnter(string dateiName)
        {
            // Pfadangabe und Dateinamen trennen
            if (dateiName == null) return;                                      // Falls nichts übergeben wurde, kann hier abgebrochen werden

            var directoryName = Path.GetDirectoryName(dateiName);
            var fileNane = Path.GetFileName(dateiName);

            // Speichern-Dialog anzeigen
            var saveFileDialog1 = new SaveFileDialog
            {
                Filter = @"XML Dateien|*.xml",
                Title = @"Terminplan speichern",
                InitialDirectory = directoryName,
                FileName = fileNane
            };

            saveFileDialog1.ShowDialog();

            // Falls ein Name eingegeben ist, kann die Datei jetzt gespeichert werden.
            if (saveFileDialog1.FileName != "")
            {
                Speichern(saveFileDialog1.FileName);
                prjHinzugefuegt = false;                                        // Damit Auswahl 'Speichern' wieder freigeschaltet wird
            }
        }
    }
}
