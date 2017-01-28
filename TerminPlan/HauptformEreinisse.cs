// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HauptformEreignisse.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Ereignisbehandlung des Formulars.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  06.01.17  br      Grundversion
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Terminplan
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    using Infragistics.Win;
    using Infragistics.Win.UltraWinSchedule;
    using Infragistics.Win.UltraWinToolbars;
    using Infragistics.Win.UltraWinGanttView;
    using Infragistics.Win.Printing;

    /// <summary>
    /// Klasse TerminPlanForm (Hauptformular).
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class TerminPlanForm : Form
    {
        #region Ereignisprozeduren

        #region ApplicationStyleChanged
        /// <summary> Behandelt das StyleChanged-Ereignis des Application Styling Managers </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.AppStyling.StyleChangedEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void ApplicationStyleChanged(object sender, Infragistics.Win.AppStyling.StyleChangedEventArgs e)
        {
            this.ultraGanttView1.PerformAutoSizeAllGridColumns();               // Größe aller Gridspalten anpassen

            // Bilder an das ausgewählte Farbschema anpassen.
            this.ColorizeImages();
//            this.ChangeIcon();
        }
        #endregion ApplicationStyleChanged

        #region UltraCalendarInfo1CalendarInfoChanged
        /// <summary> Behandelt das CalendarInfoChanged-Ereignis des ultraCalendarInfo1 Kontrols. </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="CalendarInfoChangedEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void UltraCalendarInfo1CalendarInfoChanged(object sender, CalendarInfoChangedEventArgs e)
        {
            var activeTask = this.ultraGanttView1.ActiveTask;                   // aktiven Arbeitsinhalt ermitteln
            
            // Falls kein Arbeitsinhalt ausgewäht ist, kann abgebrochen werden
            if (activeTask == null)
            {
                return;
            }
            
            // Überprüfen, ob sich der Ferigugsgrad des aktiven Arbeitsinhalts geändert hat.
            // Wenn ja, muss der Status des aktiven Arbeitsinhalts neu ermittelt werden.
            var propInfo = e.PropChangeInfo.FindTrigger(activeTask);            // Informationen über die Art der Änderung
            
            // Ermitteln, ob die richtige Änderung ausgewählt ist
            if (propInfo != null &&
                propInfo.PropId is TaskPropertyIds &&
                (TaskPropertyIds)propInfo.PropId == TaskPropertyIds.Level)
            {
                this.UpdateTasksToolsState(activeTask);                         // Status des Arbeitsinhalts anpassen
            }
        }
        #endregion UltraCalendarInfo1CalendarInfoChanged

        #region UltraGanttView1ActiveTaskChanging
        /// <summary>
        /// Behandelt das ActiveTaskChanging-Ereignis des ultraGanttView1 Kontrols.
        /// Wird ausgelöst, wenn auf einen anderen Arbeitsinhalt gewechselt wird.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="ActiveTaskChangingEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void UltraGanttView1ActiveTaskChanging(object sender, ActiveTaskChangingEventArgs e)
        {
            var newActiveTask = e.NewActiveTask;                                // Zur Aufnahme der Daten des jetzt aktiven Arbeitsinhalts
            this.UpdateTasksToolsState(newActiveTask);                          // Status des jetzigenArbeitsinhalts anpassen
            this.UpdateToolsRequiringActiveTask(newActiveTask != null);         // Überprüft den Status aller Werkzeuge, welche die aktive Aufgabe erfordert
        }
        #endregion UltraGanttView1ActiveTaskChanging

        #region UltraGanttView1CellActivating
        /// <summary> 
        /// Behandelt das CellActivating-Ereignis des ultraGanttView1 Kontrols. 
        /// Wird aufgerufen, wenn eine Zelle aktiviert wird.
        /// </summary>  
        /// <remarks>
        /// Es werden die Buttons für die Einstellung der Schrift an die ausgewählte Zelle angepasst.
        /// </remarks>    
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="CellActivatingEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void UltraGanttView1CellActivating(object sender, CellActivatingEventArgs e)
        {
            var originalValue = this.cellActivationRecursionFlag;               // Zustand des Merkers für die Zellaktivierung merken
            this.cellActivationRecursionFlag = true;                            // Merker setzen, dass Zelle aktiviert wurde
            try
            {
                var activeTask = e.TaskFieldInfo.Task;                          // aktiven Arbeitsinhalt oder Aufgabe ermitteln

                // Nur bearbeiten, wenn ein Arbeitsinhalt ausgewählt ist
                if (activeTask != null)
                {
                    var activeField = e.TaskFieldInfo.TaskField;                // aktive Zelle ermitteln
                    
                    // Nur bearbeiten, falls aktive Zelle einen Wert enthält
                    if (activeField.HasValue)
                    {
                        // Aussehen der aktivierten Zelle ermitteln
                        var appearance = activeTask.GridSettings.CellSettings[(TaskField)activeField].Appearance;
                        var fontData = appearance.FontData;                     // Schrifteinstellung

                        // Setzt den Zustand des Buttons für die Fettschrift für die aktive Zelle
                        ((StateButtonTool)this.ultraToolbarsManager1.Tools[@"Font_Bold"]).Checked = (fontData.Bold == DefaultableBoolean.True);

                        // Setzt den Zustand des Buttons für Kursiv-Schrift für die aktive Zelle
                        ((StateButtonTool)this.ultraToolbarsManager1.Tools[@"Font_Italic"]).Checked = (fontData.Italic == DefaultableBoolean.True);

                        // Setzt den Zustand des Buttons für unterstrichene Schrift der aktiven Zelle
                        ((StateButtonTool)this.ultraToolbarsManager1.Tools[@"Font_Underline"]).Checked = (fontData.Underline == DefaultableBoolean.True);

                        // Name der Schriftart in der Fontliste aktualisieren
                        var fontName = fontData.Name;                           // Name der Schriftart ermitteln
                        
                        // Falls keine Schriftart ermittelt wurde, Normalschrift auswählen,
                        // sonst die ermittelte Schriftart anzeigen
                        if (fontName != null)
                        {
                            ((FontListTool)this.ultraToolbarsManager1.Tools[@"FontList"]).Text = fontName;
                        }
                        else
                        {
                            ((FontListTool)this.ultraToolbarsManager1.Tools[@"FontList"]).SelectedIndex = 0;
                        }

                        // Falls die Schriftgröße > 0 ist, sie in der Combobox zur Asuwahl der Schriftart
                        // auswählen.
                        // Falls keine Schriftgröße ausgewäht ist, Standardgröße asuwählen.
                        var fontSize = fontData.SizeInPoints;
                        if (Math.Abs(fontSize) > 0.1)
                        {
                            ((ComboBoxTool)(this.ultraToolbarsManager1.Tools[@"FontSize"])).Value = fontSize;
                        }
                        else
                        {
                            ((ComboBoxTool)(this.ultraToolbarsManager1.Tools[@"FontSize"])).SelectedIndex = 0;
                        }
                    }
                }

                this.OnUpdateFontToolsState(e.TaskFieldInfo.TaskField.HasValue);  // Anzeige der Schriftart aktualisieren 
            }
            finally
            {
                this.cellActivationRecursionFlag = originalValue;               // Zustand der Zellaktivierung wieder herstellen
            }
        }
        #endregion UltraGanttView1CellActivating

        #region UltraGanttView1CellDeactivating
        /// <summary>
        /// Behandelt das CellDeactivating-Ereignis des ultraGanttView1 Kontrols.
        /// Wird aufgerufen, wenn eine Zelle deaktiviert wird
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.UltraWinGanttView.CellDeactivatingEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void UltraGanttView1CellDeactivating(object sender, CellDeactivatingEventArgs e)
        {
            this.OnUpdateFontToolsState(false);                                 // Anzeige der Schriftart zurücksetzen
            this.UpdateTasksToolsState(null);                                   // Werkzeuge für die Bearbeitung der Arbeitsinhalte zurücksetzen
        }
        #endregion UltraGanttView1CellDeactivating

        #region UltraGanttView1TaskAdded
        /// <summary>
        /// Behandelt das TaskAdded-Ereignis des ultraGanttView1 Kontrols.
        /// Wird aufgerufen, wenn ein neuer Arbeitsinhalt hinzugefügt wurde.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="TaskAddedEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void UltraGanttView1TaskAdded(object sender, TaskAddedEventArgs e)
        {
            // Überprüft den Status aller Werkzeuge, welche der neue Arbeitsinhalt erfordert.
            // Wenn kein Arbeitsinhalt ausgewählt ist, den Status aller Werkzeuge zurücksetzen.
            this.UpdateToolsRequiringActiveTask(this.ultraGanttView1.ActiveTask != null);
        }
        #endregion UltraGanttView1TaskAdded

        #region UltraGanttView1TaskDeleted
        /// <summary>
        /// Behandelt das TaskDeleted-Ereignis des ultraGanttView1 Kontrols.
        /// Wird aufgerufen, wenn ein Arbeitsinhalt gelöscht wurde.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="TaskDeletedEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void UltraGanttView1TaskDeleted(object sender, TaskDeletedEventArgs e)
        {
            // Überprüft den Status aller Werkzeuge, welche jetzt einen aktiven Arbeitsinhalt erfordern.
            // Wenn kein Arbeitsinhalt ausgewählt ist, den Status aller Werkzeuge zurücksetzen
            this.UpdateToolsRequiringActiveTask(this.ultraGanttView1.ActiveTask != null);
        }
        #endregion UltraGanttView1TaskDeleted

        #region UltraToolbarsManager1PropertyChanged

        /// <summary>
        /// Behandelt das PropertyChanged-Ereignis des ultraToolbarsManager1 Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.PropertyChangedEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void UltraToolbarsManager1PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var trigger = e.ChangeInfo.FindTrigger(null);                       // Ermitteln, welche Eigenschaft geändert wurde
            
            // Nur bearbeiten, wenn es eine Eigenschaft ist, welche vom Toolbars-Manager verwaltet wird
            if (trigger == null || !(trigger.Source is SharedProps) || !(trigger.PropId is Infragistics.Win.UltraWinToolbars.PropertyIds))
            {
                return;
            }

            // ID auswerten
            switch ((Infragistics.Win.UltraWinToolbars.PropertyIds)trigger.PropId)
            {
                case Infragistics.Win.UltraWinToolbars.PropertyIds.Enabled:     // Nur freigegebene Eigenschaften bearbeiten
                    var sharedProps = (SharedProps)trigger.Source;              // Kontrol ermitteln

                    // Falls mehrere Instanzen des Kontrols vorhanden sind, die erste Instanz nehmen,
                    // bei nur einer Instanz muss diese genommen werden
                    var tool = (sharedProps.ToolInstances.Count > 0) ? sharedProps.ToolInstances[0] : sharedProps.RootTool; // Name desSchlüssels zusammenstellen
                    var imageKey = string.Format(@"{0}_{1}", tool.Key, tool.EnabledResolved ? @"Normal" : @"Disabled");
                        
                    // Schlüssel des Bildes in die entsprechende Appearance-Eigenschaft eintragen
                    if (this.ilColorizedImagesLarge.Images.ContainsKey(imageKey))
                    {
                        sharedProps.AppearancesLarge.Appearance.Image = imageKey; // Für große Bilder
                    }
                        
                    if (this.ilColorizedImagesSmall.Images.ContainsKey(imageKey))
                    {
                        sharedProps.AppearancesSmall.Appearance.Image = imageKey; // Für kleine Bilder  
                    }
                        
                    break;
            }
        }
        #endregion UltraToolbarsManagerPropertyChanged

        #region UltraToolbarsManagerToolClick
        /// <summary>
        /// Behandelt das ToolClick-Ereignis of the ultraToolbarsManager1 control.
        /// </summary>
        /// <remarks>
        /// Die jeweilige Aktion wird nur durchgeführt, wenn sie nicht schon durchgeführt wurde
        /// </remarks>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.UltraWinToolbars.ToolClickEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void UltraToolbarsManagerToolClick(object sender, ToolClickEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "Font_Bold":                                               // Fettschrift
                    if (this.cellActivationRecursionFlag == false)
                    {
                        this.UpdateFontProperty(FontProperties.Bold);
                    }
                    break;

                case "Font_Italic":                                             // Kursivschrift
                    if (this.cellActivationRecursionFlag == false)
                    {
                        this.UpdateFontProperty(FontProperties.Italics);
                    }
                    break;

                case "Font_Underline":
                    if (this.cellActivationRecursionFlag == false) // Unterstrichene Schrift
                    {
                        this.UpdateFontProperty(FontProperties.Underline);
                    }
                    break;

                case "Font_BackColor":                                          // Hintergrundfarbe
                    this.SetTextBackColor();
                    break;

                case "Font_ForeColor":                                          // Vordergrundfarbe
                    this.SetTextForeColor();
                    break;

                case "FontList":                                                // Liste mit den Schriftarten
                    this.UpdateFontName();
                    break;

                case "FontSize":                                                // Schriftgröße
                    this.UpdateFontSize();
                    break;

                case "Insert_Task_Task":                                        // Neuen Arbeitsinhalt oder neue Aufgabe am Ende hinzufügen
                    this.AddNewTask(false);
                    break;

                case "Insert_Task_TaskAtSelectedRow":                           // Neuen Arbeitsinhalt oder neue Aufgabe nach der aktuellen Zeile hinzufügen
                    this.AddNewTask(true);
                    break;

                case "Tasks_PercentComplete_0":                                 // Fertigungsgrad 0%
                    this.SetTaskPercentage(0);
                    break;

                case "Tasks_PercentComplete_25":                                // Fertigungsgrad 25%
                    this.SetTaskPercentage(25);
                    break;

                case "Tasks_PercentComplete_50":                                // Fertigungsgrad 50%
                    this.SetTaskPercentage(50);
                    break;

                case "Tasks_PercentComplete_75":                                // Fertigungsgrad 75%
                    this.SetTaskPercentage(75);
                    break;

                case "Tasks_PercentComplete_100":                               // Fertigungsgrad 100%
                    this.SetTaskPercentage(100);
                    break;

                case "Tasks_MoveLeft":                                          // Nach links verschieben
                    this.PerformIndentOrOutdent(GanttViewAction.OutdentTask);
                    break;

                case "Tasks_MoveRight":                                         // Nach rechts verschieben
                    this.PerformIndentOrOutdent(GanttViewAction.IndentTask);
                    break;

                case "Tasks_Delete":                                            // Arbeitsinhalt oder Aufgabe löschen
                    this.DeleteTask();
                    break;

                case "Schedule_OnMoveTask_1Day":                                // Starttermin 1 Tag später
                    this.MoveTask(GanttViewAction.MoveTaskDateForward, TimeSpanForMoving.OneDay);
                    break;
                
                case "Schedule_OnMoveTask_1Week":                               // Starttermin 1 Woche später
                    this.MoveTask(GanttViewAction.MoveTaskDateForward, TimeSpanForMoving.OneWeek);
                    break;
                
                case "Schedule_MoveTask_4Weeks":                                // Starttermin 4 Wochen später
                    this.MoveTask(GanttViewAction.MoveTaskDateForward, TimeSpanForMoving.FourWeeks);
                    break;
                
                case "Schedule_MoveTask_MoveTaskBackwards1Day":                 // Sterttrmin 1 Tag früher
                    this.MoveTask(GanttViewAction.MoveTaskDateBackward, TimeSpanForMoving.OneDay);
                    break;
                
                case "Schedule_MoveTask_MoveTaskBackwards1Week":                // Starttermin 1 Woche früher
                    this.MoveTask(GanttViewAction.MoveTaskDateBackward, TimeSpanForMoving.OneWeek);
                    break;
                
                case "Schedule_MoveTask_MoveTaskBackwards4Weeks":               // Starttermin 4 Wochen früher
                    this.MoveTask(GanttViewAction.MoveTaskDateBackward, TimeSpanForMoving.FourWeeks);
                    break;

                case "Properties_TaskInformation":                              // Informatinen über den Arbeitsinhalt oder die Aufgabe
                    this.ultraGanttView1.DisplayTaskDialog(this.ultraGanttView1.ActiveTask);
                    break;

                case "Properties_Notes":
                    this.ultraGanttView1.TaskDialogDisplaying += this.OnUltraGanttView1TaskDialogDisplaying;
                    this.ultraGanttView1.DisplayTaskDialog(this.ultraGanttView1.ActiveTask);
                    this.ultraGanttView1.TaskDialogDisplaying -= this.OnUltraGanttView1TaskDialogDisplaying;
                    break;

                case "Insert_Milestone":
                    this.ultraGanttView1.ActiveTask.Milestone = !this.ultraGanttView1.ActiveTask.Milestone;
                    break;

                case "TouchMode":
                    var touchModeListTool = e.Tool as ListTool;
                    if (touchModeListTool != null && touchModeListTool.SelectedItem == null)
                    {
                        touchModeListTool.SelectedItemIndex = e.ListToolItem.Index;
                    }

                    this.ultraTouchProvider1.Enabled = (e.ListToolItem.Key == @"Touch");
                    break;

                case "ThemeList":                    
                    var themeListTool = e.Tool as ListTool;
                    if (themeListTool != null && themeListTool.SelectedItem == null)
                    {
                        themeListTool.SelectedItemIndex = e.ListToolItem.Index;
                    }

                    var key = e.ListToolItem.Key;
                    if (this.themePaths[this.currentThemeIndex] != key)
                    {
                        this.currentThemeIndex = e.ListToolItem.Index;
                        Infragistics.Win.AppStyling.StyleManager.Load(DienstProgramme.GetEmbeddedResourceStream(key));
                    }
                    break;

                case "Print":
                    var printPreview = new UltraPrintPreviewDialog { Document = this.ultraGanttViewPrintDocument1 };
                    printPreview.ShowDialog(this);
                    break;

                case "Exit":
                case "Close":
                    Application.Exit();
                    break;

                case "Neu":                                                     // Neuen Terminplan anlegen
                    break;

                case "Speichern":                                               // Terminplan speichern
                    break;

                case "Speichern unter":                                         // Terminplan unter anderem Namen speichern
                    break;
            }
        }
        #endregion UltraToolbarsManagerToolClick

        #region UltraToolbarsManager1ToolValueChanged
        /// <summary>
        /// Behandelt das ToolValueChanged-Ereignis des ultraToolbarsManager1 Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">The <see cref="ToolEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void UltraToolbarsManager1ToolValueChanged(object sender, ToolEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "Font_BackColor":
                    this.SetTextBackColor();
                    break;
                case "Font_ForeColor":
                    this.OnSetTextForeColor();
                    break;
                case "FontList":
                    this.UpdateFontName();
                    break;
                case "FontSize":
                    this.UpdateFontSize();
                    break;
            }
        }
        #endregion UltraToolbarsManager1ToolValueChanged

        #region UltraTouchProvider1PropertyChanged
        /// <summary>
        /// Behandelt das PropertyChanged-Ereignis des ultraTouchProvider1 Kontrols.
        /// </summary>
        /// <remarks>
        /// Dient zum automatischen Größeneinstellung der Gridspalten
        /// </remarks>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.PropertyChangedEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void UltraTouchProvider1PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var propChanged = e.ChangeInfo;
            if (propChanged.PropId is Infragistics.Win.Touch.TouchProviderPropertyIds &&
                ((Infragistics.Win.Touch.TouchProviderPropertyIds)propChanged.PropId) == Infragistics.Win.Touch.TouchProviderPropertyIds.Enabled)
            {
                this.ultraGanttView1.PerformAutoSizeAllGridColumns();           // Größe Gridspalten einstellen
            }
        }
        #endregion UltraTouchProvider1PropertyChanged
        #endregion Ereignisprozeduren

        #region Methoden
        #region AddNewTask
        /// <summary>
        /// Fügt dem GanttView einen neuen Arbeitsinhalt hinzu
        /// </summary>
        /// <param name="addAtSelectedRow">
        /// Füget bei true einen neuen Arbeitsinhalt an der ausgewählten Zeile ein, 
        /// bei false am unteren Rand des ganttViews
        /// </param>
        private void AddNewTask(bool addAtSelectedRow)
        {
            TasksCollection parentCollection = null;                            // Sammlung übergeordneter Arbeitsinhalte löschen
            var calendarInfo = this.ultraGanttView1.CalendarInfo;               // Kalenderinfo festlegen
            var activeTask = this.ultraGanttView1.ActiveTask;                   // aktiven Arbeitsinhalt ermitteln
            var projekt = calendarInfo.Projects[1];                             // Projekt ermitteln
            int insertionIndex;                                                 // Index des neuen Arbeitsinhalts
            DateTime start;                                                     // Startdatum des Arbeitsinhalts
            var addToRootcollection = true;                                     // Eintrag wird zum Wurzelknoten hinzugefügt
            
            // Ermitteln, ob bei der ausgewählten Zeile oder am Ende ein neuer Arbeitsinhalt eingefügt werden soll 
            if (addAtSelectedRow)
            {
                // Einfügen an ausgewählter Zeile
                if (activeTask != null)
                {
                    var parentTask = activeTask.Parent;                         // Übergeordneten Arbeitsinhalt ermitteln
                    
                    // Falls ein übergeordneter Arbeitsinhalt vorhanden ist, besteht die Sammlung der 
                    // übergeordneten Arbeitsinhalten aus den bisherigen übergeordneten
                    // Arbeitsinhalten, sonst aus den Arbeitsinhalten der Kalenderinfo ermitteln
                    parentCollection = parentTask != null ? parentTask.Tasks : calendarInfo.Tasks;
                    insertionIndex = parentCollection.IndexOf(activeTask);      // Index des bisherigen Arbeitsinhalts
                    
                    // Das Startdatum ist davon abhängig, ob es sich um einen übergeordneten Arbeitsinhalt
                    // handelt oder nicht
                    // ReSharper disable once MergeConditionalExpression
                    start = parentTask != null ? parentTask.StartDateTime : projekt.StartDate;
                    addToRootcollection = false;                                // Eintrag wird nicht zum Wurzelknoten hinzugefügt
                }
                else
                {
                    // es existiert kein aktiver Arbeitsinhalt 
                    insertionIndex = calendarInfo.Tasks.Count;                  // Index ist die bisherige Anzahl Arbeitsinhalten des übergeordneten Arbeitsinhalts
                    start = projekt.StartDate;                                  // Das Startdatum ist das Startdatum des Projekts
                }
            }
            else
            {
                // Einfügen am Ende
                // Die Sammlung der übergeordneten Arbeitsinhalten besteht aus den Arbeitsinhalten der Kalenderinfo
                parentCollection = calendarInfo.Tasks;
                insertionIndex = calendarInfo.Tasks.Count;                      // Index ist die bisherige Anzahl Arbeitsinhalten des übergeordneten Arbeitsinhalts
                start = projekt.StartDate;                                      // Das Startdatum ist das Startdatum des Projekts
            }

            // Nur bearbeiten, falls ein übergeordeter Arbeitsinhalt vorhanden ist
            if (parentCollection == null)
            {
                return;                                                         // Abbruch, da kein übergeordneter Arbeitsinhalt
            }

            //  Neue Aufgabe oder neuen Arbeitsinhalt hinzufügen
            var taskName = this.rm.GetString("NewTaskName");                    // Namen des Arbeitsinhalts oder der Aufgabe ermitteln
            Task newTask;
                
            // Ermitteln, ob es sich um eine Aufgabe oder einen Arbeitsinhalt handelt
            if (addToRootcollection == false &&
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                activeTask != null &&
                activeTask.Parent != null)
            {
                // Es handelt sich um einen Arbeitsinhalt
                newTask = activeTask.Parent.Tasks.Insert(insertionIndex, start, TimeSpan.FromDays(1), taskName);// newTask);
            }
            else
            {
                // Es handelt sich um eine Aufgabe
                newTask = calendarInfo.Tasks.Insert(insertionIndex, start, TimeSpan.FromDays(1), taskName);
            }
                
            newTask.Project = projekt;                                          // Projektname dem hinzugefügten Elemnt zuweisen  
            newTask.RowHeight = TaskRowHeight;
        }
        #endregion AddNewTask

        #region BindArbInhaltData
        /// <summary>
        /// Bindet die Daten (Arbeitsinhalte) an die UltraCalendarInfo
        /// </summary>
        /// <param name="data">Die Daten.</param>
        private void BindArbInhaltData(DataSet data)
        {
            // Setzt die BindingContextControl-Eigenschaft,
            // um auf dieses Formular zu verweisen
            #region BindingContext
            this.ultraCalendarInfo1.DataBindingsForTasks.BindingContextControl = this;
            this.ultraCalendarInfo1.DataBindingsForProjects.BindingContextControl = this;
            this.ultraCalendarInfo1.DataBindingsForOwners.BindingContextControl = this;
            #endregion BindingContext

            //  Legt die Databinding-Mitglieder für Projekte fest 
            #region Projekte
            this.ultraCalendarInfo1.DataBindingsForProjects.SetDataBinding(data, @"Projekte");
            this.ultraCalendarInfo1.DataBindingsForProjects.IdMember = @"ProjektID";
            this.ultraCalendarInfo1.DataBindingsForProjects.KeyMember = @"ProjektKey";
            this.ultraCalendarInfo1.DataBindingsForProjects.NameMember = @"ProjektName";
            this.ultraCalendarInfo1.DataBindingsForProjects.StartDateMember = @"ProjektStart";
            #endregion Projekte

            //  Legt die Databinding-Mitglieder für Arbeitsinhalte fest
            #region Arbeitsinhalte
            this.ultraCalendarInfo1.DataBindingsForTasks.SetDataBinding(data, @"Arbeitsinhalt_Aufgaben");

            // Grundlegende Eigenschaften für die Arbeitsinhalte
            this.ultraCalendarInfo1.DataBindingsForTasks.NameMember = @"TaskName";
            this.ultraCalendarInfo1.DataBindingsForTasks.DurationMember = @"TaskDauer";
            this.ultraCalendarInfo1.DataBindingsForTasks.StartDateTimeMember = @"TaskStartTime";
            this.ultraCalendarInfo1.DataBindingsForTasks.IdMember = @"TaskID";
            this.ultraCalendarInfo1.DataBindingsForTasks.ProjectKeyMember = @"ProjektKey";
            this.ultraCalendarInfo1.DataBindingsForTasks.ParentTaskIdMember = @"ParentTaskID";

            this.ultraCalendarInfo1.DataBindingsForTasks.ConstraintMember = @"Einschraenkung";
            this.ultraCalendarInfo1.DataBindingsForTasks.PercentCompleteMember = @"TaskFertigInProzent";

            // Alle anderen Eigenschaften
            this.ultraCalendarInfo1.DataBindingsForTasks.AllPropertiesMember = @"AlleEigenschaften";
            #endregion Arbeitsinhalte

            // Legt die Databinding-Mitglieder für den Besitzer fest.
            // Wird in die Kalenderinfo eingebunden.
            #region Besitzer
            this.ultraCalendarInfo1.DataBindingsForOwners.SetDataBinding(data, @"Besitzer");
            this.ultraCalendarInfo1.DataBindingsForOwners.BindingContextControl = this;
            this.ultraCalendarInfo1.DataBindingsForOwners.KeyMember = @"Key";
            this.ultraCalendarInfo1.DataBindingsForOwners.NameMember = @"Name";
            this.ultraCalendarInfo1.DataBindingsForOwners.EmailAddressMember = @"EmailAddresse";
            this.ultraCalendarInfo1.DataBindingsForOwners.VisibleMember = @"Sichtbar";
            this.ultraCalendarInfo1.DataBindingsForOwners.AllPropertiesMember = @"AlleEigenschaften";
            #endregion Besitzer

            // Das Projekt dem GanttView Control zuweisen.
            this.ultraGanttView1.Project = this.ultraGanttView1.CalendarInfo.Projects[1];
        }
        #endregion BindProjectData

        #region ChangeIcon
        /// <summary>
        /// Ändert das Symbol.
        /// </summary>
        private void ChangeIcon()
        {
            // Anhand des Farbschemas den Namen des zum Farbschema gehörenden Icons zusammensetzen
            var iconPath = this.themePaths[this.currentThemeIndex].Replace(@"StyleLibraries.", @"Images.AppIcon - ").Replace(@".isl", @".ico");

            var stream = DienstProgramme.GetEmbeddedResourceStream(iconPath);   // Zum Laden des Farbschemas

            // Falls Farbschema existiert, kann Icon geladen werden
            if (stream != null)
            {
                this.Icon = new Icon(stream);                                   // Icon laden
            }
        }
        #endregion ChangeIcon

        #region ColorizeImages
        /// <summary>
        /// Färbt die Bilder in den großen und kleinen Bildlisten mit den Standardbildern 
        /// und platziert die neuen Bilder in den farbigen Bildlisten.
        /// </summary>
        private void ColorizeImages()
        {
            // Unterbindet das Zeichnen im UltraToolbarsManager,
            // damit die neuen Farben eingestellt werden können-
            var shouldSuspendPainting = !this.ultraToolbarsManager1.IsUpdating; // Ermitteln, ob gerade gezeichnet wird

            // Neue Farben können eingestellt werden, falls nicht gerade aufgefrischt wird
            if (shouldSuspendPainting)
            {
                this.ultraToolbarsManager1.BeginUpdate();                       // Auffrischen starten
            }

            // Bildlisten mit den neuen Bildern setzen
            var largeImageList = this.ultraToolbarsManager1.ImageListLarge;
            var smallImageList = this.ultraToolbarsManager1.ImageListSmall;

            try
            {
                // Bildlisten im UltraToolbarsManager löschen, damit weiter andere
                // Farben eingestellt werden können
                this.ultraToolbarsManager1.ImageListLarge = null;
                this.ultraToolbarsManager1.ImageListSmall = null;

                ToolBase resolveTool = null;                                    // gefundenes Tool löschen, damit neues erstellt werden kann

                // Nur bearbeiten, falls das Tool"Insert_Task" existiert
                if (this.ultraToolbarsManager1.Tools.Exists(@"Insert_Task"))
                {
                    resolveTool = this.ultraToolbarsManager1.Tools[@"Insert_Task"]; // Tool merken

                    // Alle Instanzen auf der Suche nach dem Tool in der RibbonGroup durchsuchen
                    foreach (var instanceTool in resolveTool.SharedProps.ToolInstances.Cast<ToolBase>().Where(instanceTool => instanceTool.OwnerIsRibbonGroup))
                    {
                        resolveTool = instanceTool;                             // Tool gefunden, Suche kann abgebrochen werden
                        break;
                    }
                }

                // Nur weiter bearbeiten, wenn ein Tool gefunden wurde
                if (resolveTool == null)
                {
                    return;
                }

                // Holt die eingestellten Farben                
                var colors = new Dictionary<string, Color>();                   // Neue Liste mit den Farben erzeugen

                // Standard-Vordergrundfarbe einstellen
                var appData = new AppearanceData();                             // Neue Einstellungen für Infragistics
                var requestedProps = AppearancePropFlags.ForeColor;             // Vordergrundfarbe soll eingestellt werden
                
                // Das aktuelle Erscheinungsbild des Tools löschen
                resolveTool.ResolveAppearance(ref appData, ref requestedProps);
                colors[@"Normal"] = appData.ForeColor;                          // Standard-Vordergrundfarbe in Liste eintragen

                // Aktive Vordergrundfarbe einstellen
                appData = new AppearanceData();                                 // Neue Einstellungen für Infragistics
                requestedProps = AppearancePropFlags.ForeColor | AppearancePropFlags.BackColor; // Ermitteln, ob Vorder- oder Hintergrundfarbe bearbeiteet werden soll
                
                // Das aktuelle Erscheinungsbild des Tools löschen
                resolveTool.ResolveAppearance(ref appData, ref requestedProps, true, false);
                colors[@"Active"] = appData.ForeColor;                          // Aktive Vordergrundfarbe in Liste eintragen

                // Hintergrundfarbe einstellen
                if (appData.BackColor.IsEmpty || appData.BackColor.Equals(Color.Transparent))
                {
                    // Hintergrundfabe festlegen, falls keine Farbe oder 'Transparent' angegeben ist
                    appData = new AppearanceData();                             // Neue Einstellungen für Infragistics
                    requestedProps = AppearancePropFlags.BackColor;             // Hintergrundfarbe soll eingestellt werden
                    
                    // Löscht das aktuelle Erscheinungsbild für die Registerkarte des RibbonTab
                    this.ultraToolbarsManager1.Ribbon.Tabs[0].ResolveTabItemAppearance(ref appData, ref requestedProps);
                    colors[@"Disabled"] = appData.BackColor;                    // Hintergrundfarbe für 'gesperrt' in Liste eintragen
                }
                else
                {
                    // Es ist eine Farbe angegeben, diese dann in die Liste eintragen
                    colors[@"Disabled"] = appData.BackColor;
                }

                // Die Standardbilder haben die Farbe 'Magenta'. Diese Farbe muss ersetzt werden
                var replacementColor = Color.Magenta;

                // Die Bilder in den großen und kleinen Bildlisten mit den Standardbildern
                // an das ausgewählte Farbschema anpassen
                DienstProgramme.ColorizeImages(replacementColor, colors, ref this.ilDefaultImagesLarge, ref this.ilColorizedImagesLarge);
                DienstProgramme.ColorizeImages(replacementColor, colors, ref this.ilDefaultImagesSmall, ref this.ilColorizedImagesSmall);

                // Sicherstellen, dass der UltraToolbarsManager die neuen farbigen Bilder verwendet
                largeImageList = this.ilColorizedImagesLarge;
                smallImageList = this.ilColorizedImagesSmall;
            }
            catch
            {
                // Sicherstellen, dass der UltraToolbarsManager die neuen farbigen Bilder verwendet
                largeImageList = this.ilDefaultImagesLarge;
                smallImageList = this.ilDefaultImagesSmall;
            }
            finally
            {
                this.ultraToolbarsManager1.ImageListLarge = largeImageList;
                this.ultraToolbarsManager1.ImageListSmall = smallImageList;

                // Zeichnen im UltraToolbarsManager fortsetzen, falls es unterbrochen war
                if (shouldSuspendPainting)
                {
                    this.ultraToolbarsManager1.EndUpdate();                     // Auffrischen ist fertig
                }
            }
        }
        #endregion ColorizeImages
        #endregion Methoden
    }
}
