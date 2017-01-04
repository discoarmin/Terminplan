using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using Infragistics.Win;
using Infragistics.Win.UltraWinSchedule;
using Infragistics.Win.UltraWinGanttView.Grid;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win.UltraWinGanttView.Internal;
using Infragistics.Win.UltraWinToolbars;
using Infragistics.Win.UltraWinSchedule.TaskUI;
using Infragistics.Win.UltraWinGanttView;
using Infragistics.Win.UltraMessageBox;
using Infragistics.Win.UltraWinListView;
using System.Diagnostics;
using Infragistics.Shared;
using Infragistics.Win.Printing;
using System.Threading;

namespace Terminplan
{
    /// <summary>
    /// Klasse .
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class TerminplanForm : Form
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
            this.ChangeIcon();
        }
        #endregion ApplicationStyleChanged

        #region UltraCalendarInfo1CalendarInfoChanged
        /// <summary> Behandelt das CalendarInfoChanged-Ereignis des ultraCalendarInfo1 Kontrols. </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="CalendarInfoChangedEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void UltraCalendarInfo1CalendarInfoChanged(object sender, CalendarInfoChangedEventArgs e)
        {
            Task activeTask = this.ultraGanttView1.ActiveTask;                  // aktiven Arbeitsinhalt ermitteln
            
            // Falls kein Arbeitsinhalt ausgewäht ist, kann abgebrochen werden
            if (activeTask == null)
            {
                return;
            }
            
            // Überprüfen, ob sich der Ferigugsgrad des aktiven Arbeitsinhalts geändert hat.
            // Wenn ja, muss der Status des aktiven Arbeitsinhalts neu ermittelt werden.
            PropChangeInfo propInfo = e.PropChangeInfo.FindTrigger(activeTask); // Informationen über die Art der Änderung
            
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
            Task newActiveTask = e.NewActiveTask;                               // Zur Aufnahme der Daten des jetzt aktiven Arbeitsinhalts
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
            bool originalValue = this.cellActivationRecursionFlag;              // Zustand des Merkers für die Zellaktivierung merken
            this.cellActivationRecursionFlag = true;                            // Merker setzen, dass Zelle aktiviert wurde
            try
            {
                Task activeTask = e.TaskFieldInfo.Task;                         // aktiven Arbeitsinhalt ermitteln

                // Nur bearbeiten, wenn ein Arbeitsinhalt ausgewählt ist
                if (activeTask != null)
                {
                    TaskField? activeField = e.TaskFieldInfo.TaskField;         // aktive Zelle ermitteln
                    
                    // Nur bearbeiten, falls aktive Zelle einen Wert enthält
                    if (activeField.HasValue)
                    {
                        // Aussehen der aktivierten Zelle ermitteln
                        Infragistics.Win.AppearanceBase appearance = activeTask.GridSettings.CellSettings[(TaskField)activeField].Appearance;
                        FontData fontData = appearance.FontData;                // Schrifteinstellung

                        // Setzt den Zustand des Buttons für die Fettschrift für die aktive Zelle
                        ((StateButtonTool)this.ultraToolbarsManager1.Tools["Font_Bold"]).Checked = (fontData.Bold == DefaultableBoolean.True);

                        // Setzt den Zustand des Buttons für Kursiv-Schrift für die aktive Zelle
                        ((StateButtonTool)this.ultraToolbarsManager1.Tools["Font_Italic"]).Checked = (fontData.Italic == DefaultableBoolean.True);

                        // Setzt den Zustand des Buttons für unterstrichene Schrift der aktiven Zelle
                        ((StateButtonTool)this.ultraToolbarsManager1.Tools["Font_Underline"]).Checked = (fontData.Underline == DefaultableBoolean.True);

                        // Name der Schriftart in der Fontliste aktualisieren
                        var fontName = fontData.Name;                           // Name der Schriftart ermitteln
                        
                        // Falls keine Schriftart ermittelt wurde, Normalschrift auswählen,
                        // sonst die ermittelte Schriftart anzeigen
                        if (fontName != null)
                        {
                            ((FontListTool)this.ultraToolbarsManager1.Tools["FontList"]).Text = fontName;
                        }
                        else
                        {
                            ((FontListTool)this.ultraToolbarsManager1.Tools["FontList"]).SelectedIndex = 0;
                        }

                        // Falls die Schriftgröße > 0 ist, sie in der Combobox zur Asuwahl der Schriftart
                        // auswählen.
                        // Falls keine Schriftgröße ausgewäht ist, Standardgröße asuwählen.
                        float fontSize = fontData.SizeInPoints;
                        if (fontSize != 0)
                        {
                            ((ComboBoxTool)(this.ultraToolbarsManager1.Tools["FontSize"])).Value = fontSize;
                        }
                        else
                        {
                            ((ComboBoxTool)(this.ultraToolbarsManager1.Tools["FontSize"])).SelectedIndex = 0;
                        }
                    }
                }

                this.UpdateFontToolsState(e.TaskFieldInfo.TaskField.HasValue);  // Anzeige der Schriftart aktualisieren 
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
        private void UltraGanttView1CellDeactivating(object sender, Infragistics.Win.UltraWinGanttView.CellDeactivatingEventArgs e)
        {
            this.UpdateFontToolsState(false);                                   // Anzeige der Schriftart zurücksetzen
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
        private void UltraToolbarsManager1PropertyChanged(object sender, Infragistics.Win.PropertyChangedEventArgs e)
        {
            PropChangeInfo trigger = e.ChangeInfo.FindTrigger(null);            // Ermitteln, welche Eigenschaft geändert wurde
            
            // Nur bearbeiten, wenn es eine Eigenschaft ist, welche vom Toolbars-Manager verwalter wird
            if (trigger != null &&
                trigger.Source is SharedProps &&
                trigger.PropId is Infragistics.Win.UltraWinToolbars.PropertyIds)
            {
                // ID auswerten
                switch ((Infragistics.Win.UltraWinToolbars.PropertyIds)trigger.PropId)
                {
                    case Infragistics.Win.UltraWinToolbars.PropertyIds.Enabled: // Nur freigegebene Eigenschaften bearbeiten
                        SharedProps sharedProps = (SharedProps)trigger.Source;  // Kontrol ermitteln

                        // Falls mehrere Instanzen des Kontrols vorhanden sind, die erste Instanz nehmen,
                        // bei nur einer Instanz muss diese genommen werden
                        ToolBase tool = (sharedProps.ToolInstances.Count > 0) ? sharedProps.ToolInstances[0] : sharedProps.RootTool; // Name desSchlüssels zusammenstellen
                        string imageKey = string.Format("{0}_{1}", tool.Key, tool.EnabledResolved ? "Normal" : "Disabled");
                        
                        // Schlüssel des Bildes in die entsprechende Appearance-Eigenschaft eintragen
                        if (this.ilColorizedImagesLarge.Images.ContainsKey(imageKey))
                            sharedProps.AppearancesLarge.Appearance.Image = imageKey; // Für große Bilder
                        if (this.ilColorizedImagesSmall.Images.ContainsKey(imageKey))
                            sharedProps.AppearancesSmall.Appearance.Image = imageKey; // Für kleine Bilder  
                        
                        break;
                }
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
        private void UltraToolbarsManagerToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "Font_Bold":                                               // Fettschrift
                    if (cellActivationRecursionFlag == false)
                        this.UpdateFontProperty(FontProperties.Bold);
                    break;

                case "Font_Italic":                                             // Kursivschrift
                    if (cellActivationRecursionFlag == false)
                        this.UpdateFontProperty(FontProperties.Italics);
                    break;

                case "Font_Underline":
                    if (cellActivationRecursionFlag == false)                   // Unterstrichene Schrift
                        this.UpdateFontProperty(FontProperties.Underline);
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

                case "FontSize":
                    this.UpdateFontSize();
                    break;

                case "Insert_Task_Task":
                    this.AddNewTask(false);
                    break;

                case "Insert_Task_TaskAtSelectedRow":
                    this.AddNewTask(true);
                    break;

                case "Tasks_PercentComplete_0":
                    this.SetTaskPercentage(0);
                    break;

                case "Tasks_PercentComplete_25":
                    this.SetTaskPercentage(25);
                    break;

                case "Tasks_PercentComplete_50":
                    this.SetTaskPercentage(50);
                    break;

                case "Tasks_PercentComplete_75":
                    this.SetTaskPercentage(75);
                    break;

                case "Tasks_PercentComplete_100":
                    this.SetTaskPercentage(100);
                    break;

                case "Tasks_MoveLeft":
                    this.PerformIndentOrOutdent(GanttViewAction.OutdentTask);
                    break;

                case "Tasks_MoveRight":
                    this.PerformIndentOrOutdent(GanttViewAction.IndentTask);
                    break;

                case "Tasks_Delete":
                    this.DeleteTask();
                    break;

                case "Schedule_MoveTask_1Day":
                    this.MoveTask(GanttViewAction.MoveTaskDateForward, TimeSpanForMoving.OneDay);
                    break;
                
                case "Schedule_MoveTask_1Week":
                    this.MoveTask(GanttViewAction.MoveTaskDateForward, TimeSpanForMoving.OneWeek);
                    break;
                
                case "Schedule_MoveTask_4Weeks":
                    this.MoveTask(GanttViewAction.MoveTaskDateForward, TimeSpanForMoving.FourWeeks);
                    break;
                
                case "Schedule_MoveTask_MoveTaskBackwards1Day":
                    this.MoveTask(GanttViewAction.MoveTaskDateBackward, TimeSpanForMoving.OneDay);
                    break;
                
                case "Schedule_MoveTask_MoveTaskBackwards1Week":
                    this.MoveTask(GanttViewAction.MoveTaskDateBackward, TimeSpanForMoving.OneWeek);
                    break;
                
                case "Schedule_MoveTask_MoveTaskBackwards4Weeks":
                    this.MoveTask(GanttViewAction.MoveTaskDateBackward, TimeSpanForMoving.FourWeeks);
                    break;

                case "Properties_TaskInformation":
                    this.ultraGanttView1.DisplayTaskDialog(this.ultraGanttView1.ActiveTask);
                    break;

                case "Properties_Notes":
                    this.ultraGanttView1.TaskDialogDisplaying += new TaskDialogDisplayingHandler(OnUltraGanttView1TaskDialogDisplaying);
                    this.ultraGanttView1.DisplayTaskDialog(this.ultraGanttView1.ActiveTask);
                    this.ultraGanttView1.TaskDialogDisplaying -= new TaskDialogDisplayingHandler(OnUltraGanttView1TaskDialogDisplaying);
                    break;

                case "Insert_Milestone":
                    this.ultraGanttView1.ActiveTask.Milestone = !this.ultraGanttView1.ActiveTask.Milestone;
                    break;

                case "TouchMode":
                    ListTool touchModeListTool = e.Tool as ListTool;
                    if (touchModeListTool.SelectedItem == null)
                        touchModeListTool.SelectedItemIndex = e.ListToolItem.Index;
                    this.ultraTouchProvider1.Enabled = (e.ListToolItem.Key == "Touch");
                    break;

                case "ThemeList":                    
                    ListTool themeListTool = e.Tool as ListTool;
                    if (themeListTool.SelectedItem == null)
                        themeListTool.SelectedItemIndex = e.ListToolItem.Index;

                    string key = e.ListToolItem.Key;
                    if (this.themePaths[this.currentThemeIndex] != key)
                    {
                        this.currentThemeIndex = e.ListToolItem.Index;
                        Infragistics.Win.AppStyling.StyleManager.Load(Utilities.GetEmbeddedResourceStream(key));
                    }
                    break;

                case "Print":
                    UltraPrintPreviewDialog printPreview = new UltraPrintPreviewDialog();
                    printPreview.Document = this.ultraGanttViewPrintDocument1;
                    printPreview.ShowDialog(this);
                    break;

                case "Exit":
                case "Close":
                    Application.Exit();
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
                    this.SetTextForeColor();
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
        private void UltraTouchProvider1PropertyChanged(object sender, Infragistics.Win.PropertyChangedEventArgs e)
        {
            OnUltraGanttView1ActiveTaskChanging propChanged = e.ChangeInfo;
            if (propChanged.PropId is Infragistics.Win.Touch.TouchProviderPropertyIds &&
                ((Infragistics.Win.Touch.TouchProviderPropertyIds)propChanged.PropId) == Infragistics.Win.Touch.TouchProviderPropertyIds.Enabled)
            {
                this.ultraGanttView1.PerformAutoSizeAllGridColumns();
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
            TasksCollection parentCollection = null;
            UltraCalendarInfo calendarInfo = this.ultraGanttView1.CalendarInfo;
            Task activeTask = this.ultraGanttView1.ActiveTask;
            Project project = calendarInfo.Projects[1];
            int insertionIndex;
            DateTime start;
            bool addToRootcollection = true;
            if (addAtSelectedRow == true)
            {
                if (activeTask != null)
                {
                    Task parentTask = activeTask.Parent;
                    parentCollection = parentTask != null ? parentTask.Tasks : calendarInfo.Tasks;
                    insertionIndex = parentCollection.IndexOf(activeTask);
                    start = parentTask != null ? parentTask.StartDateTime : project.StartDate;
                    addToRootcollection = false;
                }
                else
                {
                    insertionIndex = calendarInfo.Tasks.Count;
                    start = project.StartDate;
                }
            }
            else
            {
                parentCollection = calendarInfo.Tasks;
                insertionIndex = calendarInfo.Tasks.Count;
                start = project.StartDate;
            }

            if (parentCollection != null)
            {
                //  Insert the task
                string taskName = rm.GetString("NewTaskName");
                Task newTask;
                if (addToRootcollection == false &&
                    activeTask != null &&
                    activeTask.Parent != null)
                {
                  newTask = activeTask.Parent.Tasks.Insert(insertionIndex, start, TimeSpan.FromDays(1), taskName);// newTask);
                }
                else
                {
                   newTask = calendarInfo.Tasks.Insert(insertionIndex, start, TimeSpan.FromDays(1), taskName);
                }
                newTask.Project = project;
                newTask.RowHeight = TaskRowHeight;
            }
        }
        #endregion AddNewTask

        #region BindProjectData
        /// <summary>
        /// Bindet die Daten an die UltraCalendarInfo
        /// </summary>
        /// <param name="data">Die Daten.</param>
        private void BindProjectData(DataSet data)
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
            this.ultraCalendarInfo1.DataBindingsForProjects.SetDataBinding(data, "Projects");
            this.ultraCalendarInfo1.DataBindingsForProjects.IdMember = "ProjectID";
            this.ultraCalendarInfo1.DataBindingsForProjects.KeyMember = "ProjectKey";
            this.ultraCalendarInfo1.DataBindingsForProjects.NameMember = "ProjectName";
            this.ultraCalendarInfo1.DataBindingsForProjects.StartDateMember = "ProjectStartTime";
            #endregion Projekte

            //  Legt die Databinding-Mitglieder für Arbeitsinhalte fest
            #region Arbeitsinhalte
            this.ultraCalendarInfo1.DataBindingsForTasks.SetDataBinding(data, "Tasks");

            // Grundlegende Eigenschaften für die Arbeitsinhalte
            this.ultraCalendarInfo1.DataBindingsForTasks.NameMember = "TaskName";
            this.ultraCalendarInfo1.DataBindingsForTasks.DurationMember = "TaskDuration";
            this.ultraCalendarInfo1.DataBindingsForTasks.StartDateTimeMember = "TaskStartTime";
            this.ultraCalendarInfo1.DataBindingsForTasks.IdMember = "TaskID";
            this.ultraCalendarInfo1.DataBindingsForTasks.ProjectKeyMember = "ProjectKey";
            this.ultraCalendarInfo1.DataBindingsForTasks.ParentTaskIdMember = "ParentTaskID";

            this.ultraCalendarInfo1.DataBindingsForTasks.ConstraintMember = "Constraint";
            this.ultraCalendarInfo1.DataBindingsForTasks.PercentCompleteMember = "TaskPercentComplete";

            // Alle anderen Eigenschaften
            this.ultraCalendarInfo1.DataBindingsForTasks.AllPropertiesMember = "AllProperties";
            #endregion Arbeitsinhalte

            // Legt die Databinding-Mitglieder für den Besitzer fest.
            // Wird in die Kalenderinfo eingebunden.
            #region Besitzer
            this.ultraCalendarInfo1.DataBindingsForOwners.SetDataBinding(data, "Owners");
            this.ultraCalendarInfo1.DataBindingsForOwners.BindingContextControl = this;
            this.ultraCalendarInfo1.DataBindingsForOwners.KeyMember = "Key";
            this.ultraCalendarInfo1.DataBindingsForOwners.NameMember = "Name";
            this.ultraCalendarInfo1.DataBindingsForOwners.EmailAddressMember = "EmailAddress";
            this.ultraCalendarInfo1.DataBindingsForOwners.VisibleMember = "Visible";
            this.ultraCalendarInfo1.DataBindingsForOwners.AllPropertiesMember = "AllProperties";
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
            // Anhand des Farbschemas den Namen des zum Farbschema ehörenden Icons zusammensetzen
            string iconPath = this.themePaths[this.currentThemeIndex].Replace("StyleLibraries.", "Images.AppIcon - ").Replace(".isl", ".ico");

            System.IO.Stream stream = Utilities.GetEmbeddedResourceStream(iconPath);

            // Falls Farbschema existiert, dieses laden
            if (stream != null)
                this.Icon = new Icon(stream);                                   // Farbschema laden
        }
        #endregion ChangeIcon

        #region ColorizeImages
        /// <summary>
        /// Färbt die Bilder in den großen und kleinen Bildlisten mit den Standardbildern 
        /// und platziert die neuen Bilder in den farbigen Bildlisten.
        /// </summary>
        private void ColorizeImages()
        {
            // Unterbindet das Zeichnen im UltraToolbarsManaer,
            // damit die neuen Farben eingestellt werden können-
            bool shouldSuspendPainting = !this.ultraToolbarsManager1.IsUpdating; // Ermitteln, ob gerade gezeichnet wird

            // Neue Farben können eingestellt werden, falls nicht gerade aufgefrischt wird
            if (shouldSuspendPainting)
                this.ultraToolbarsManager1.BeginUpdate();

            // Bildlisten mit den neuen Bildern setzen
            ImageList largeImageList = this.ultraToolbarsManager1.ImageListLarge;
            ImageList smallImageList = this.ultraToolbarsManager1.ImageListSmall;

            try
            {
                // Bildlisten im UltraToolbarsManager löschen, damit weiter andere
                // Farben eingestellt werden können
                this.ultraToolbarsManager1.ImageListLarge = null;
                this.ultraToolbarsManager1.ImageListSmall = null;

                ToolBase resolveTool = null;                                    // gefundenes Tool löschen

                if (this.ultraToolbarsManager1.Tools.Exists("Insert_Task"))
                {
                    resolveTool = this.ultraToolbarsManager1.Tools["Insert_Task"];

                    // loop through all instances looking for the tool in the RibbonGroup.
                    foreach (ToolBase instanceTool in resolveTool.SharedProps.ToolInstances)
                    {
                        if (instanceTool.OwnerIsRibbonGroup)
                        {
                            resolveTool = instanceTool;
                            break;
                        }
                    }
                }

                if (resolveTool == null)
                    return;

                // Get the resolved colors
                Dictionary<string, Color> colors = new Dictionary<string, Color>();
                AppearanceData appData = new AppearanceData();
                AppearancePropFlags requestedProps = AppearancePropFlags.ForeColor; 
                resolveTool.ResolveAppearance(ref appData, ref requestedProps);
                colors["Normal"] = appData.ForeColor;

                appData = new AppearanceData();
                requestedProps = AppearancePropFlags.ForeColor | AppearancePropFlags.BackColor;
                resolveTool.ResolveAppearance(ref appData, ref requestedProps, true, false);
                colors["Active"] = appData.ForeColor;

                if (appData.BackColor.IsEmpty || appData.BackColor.Equals(Color.Transparent))
                {
                    appData = new AppearanceData();
                    requestedProps = AppearancePropFlags.BackColor;
                    this.ultraToolbarsManager1.Ribbon.Tabs[0].ResolveTabItemAppearance(ref appData, ref requestedProps);
                    colors["Disabled"] = appData.BackColor;
                }
                else
                    colors["Disabled"] = appData.BackColor;

                Color replacementColor = Color.Magenta;

                Utilities.ColorizeImages(replacementColor, colors, ref this.ilDefaultImagesLarge, ref this.ilColorizedImagesLarge);
                Utilities.ColorizeImages(replacementColor, colors, ref this.ilDefaultImagesSmall, ref this.ilColorizedImagesSmall);

                // Make sure the UltraToolbarsManager is using the new colorized images
                largeImageList = this.ilColorizedImagesLarge;
                smallImageList = this.ilColorizedImagesSmall;
            }
            catch
            {
                // Make sure the UltraToolbarsManager is using the new colorized images
                largeImageList = this.ilDefaultImagesLarge;
                smallImageList = this.ilDefaultImagesSmall;
            }
            finally
            {
                this.ultraToolbarsManager1.ImageListLarge = largeImageList;
                this.ultraToolbarsManager1.ImageListSmall = smallImageList;

                // Resume painting on the UltraToolbarsManager
                if (shouldSuspendPainting)
                    this.ultraToolbarsManager1.EndUpdate();
            }
        }

        #endregion // ColorizeImages

        #region DeleteTask

        /// <summary>
        /// Deletes the active task
        /// </summary>
        private void DeleteTask()
        {
            Task activeTask = this.ultraGanttView1.ActiveTask;
            try
            {
                if (activeTask != null)
                {
                    Task parent = activeTask.Parent;

                    if (parent == null)
                        this.ultraCalendarInfo1.Tasks.Remove(activeTask);
                    else
                        parent.Tasks.Remove(activeTask);
                }

                Task newActiveTask = this.ultraGanttView1.ActiveTask;
                this.UpdateTasksToolsState(newActiveTask);
                this.UpdateToolsRequiringActiveTask(newActiveTask != null);
            }
            catch (TaskException ex)
            {
                UltraMessageBoxManager.Show(ex.Message, rm.GetString("MessageBox_Error"));
            }
        }
        #endregion // DeleteTask

        #region InitializeUI

        /// <summary>
        /// Initializes the UI.
        /// </summary>
        private void InitializeUI()
        {
            // Populate the themes list
            int selectedIndex = 0;
            ListTool themeTool = (ListTool)this.ultraToolbarsManager1.Tools["ThemeList"];
            foreach (string resourceName in this.themePaths)
            {
                ListToolItem item = new ListToolItem(resourceName);
                string libraryName = resourceName.Replace(".isl", string.Empty);
                item.Text = libraryName.Remove(0, libraryName.LastIndexOf('.') + 1);
                themeTool.ListToolItems.Add(item);

                if (item.Text.Contains("02"))
                    selectedIndex = item.Index;
            }
            themeTool.SelectedItemIndex = selectedIndex;

            // Select the proper touch mode list item.
            ((ListTool)this.ultraToolbarsManager1.Tools["TouchMode"]).SelectedItemIndex = 0;

            // Creates a valueList with various font sizes
            this.PopulateFontSizeValueList();
            ((ComboBoxTool)(this.ultraToolbarsManager1.Tools["FontSize"])).SelectedIndex = 0;
            ((FontListTool)this.ultraToolbarsManager1.Tools["FontList"]).SelectedIndex = 0;
            this.UpdateFontToolsState(false);

            Control control = new AboutControl();
            control.Visible = false;
            control.Parent = this;
            ((PopupControlContainerTool)this.ultraToolbarsManager1.Tools["About"]).Control = control;

            // Autosize the columns so all the data is visible.
            this.ultraGanttView1.PerformAutoSizeAllGridColumns();

            // Colorize the images to match the current theme.
            this.ColorizeImages();

            this.ultraToolbarsManager1.Ribbon.FileMenuButtonCaption = Properties.Resources.ribbonFileTabCaption;
        }

        #endregion //InitializeUI

        #region MoveTask

        /// <summary>
        /// Moves start and end dates of the task backward or foward by a specific timespan
        /// </summary>
        /// <param name="action">Enumeration of ganttView actions supported</param>
        /// <param name="moveTimeSpan">TimeSpan for moving the start and end dates of the task</param>
        private void MoveTask(GanttViewAction action, TimeSpanForMoving moveTimeSpan)
        {
            Task activeTask = this.ultraGanttView1.ActiveTask;

            if (activeTask != null && activeTask.IsSummary == false)
            {
                switch (action)
                {
                    case GanttViewAction.MoveTaskDateForward:
                        {
                            switch (moveTimeSpan)
                            {
                                case TimeSpanForMoving.OneDay:
                                    activeTask.StartDateTime = activeTask.StartDateTime.AddDays(1);
                                    break;
                                case TimeSpanForMoving.OneWeek:
                                    activeTask.StartDateTime = activeTask.StartDateTime.AddDays(7);
                                    break;
                                case TimeSpanForMoving.FourWeeks:
                                    activeTask.StartDateTime = activeTask.StartDateTime.AddDays(28);
                                    break;
                            }
                        }
                        break;

                    case GanttViewAction.MoveTaskDateBackward:
                        {
                            switch (moveTimeSpan)
                            {
                                case TimeSpanForMoving.OneDay:
                                    activeTask.StartDateTime = activeTask.StartDateTime.Subtract(TimeSpan.FromDays(1));
                                    break;
                                case TimeSpanForMoving.OneWeek:
                                    activeTask.StartDateTime = activeTask.StartDateTime.Subtract(TimeSpan.FromDays(7));
                                    break;
                                case TimeSpanForMoving.FourWeeks:
                                    activeTask.StartDateTime = activeTask.StartDateTime.Subtract(TimeSpan.FromDays(28));
                                    break;
                            }
                        }
                        break;
                }
            }
        }
        #endregion  // MoveTask

        #region PerformIndentOrOutdent

        /// <summary>
        /// Perfoms indent or outdent on active task
        /// </summary>
        /// <param name="action">Action to be performed(indent or outdent)</param>
        private void PerformIndentOrOutdent(GanttViewAction action)
        {
            Task activeTask = this.ultraGanttView1.ActiveTask;

            try
            {
                if (activeTask != null)
                {
                    switch (action)
                    {
                        case GanttViewAction.IndentTask:
                            if (activeTask.CanIndent())
                                activeTask.Indent();
                            break;
                        case GanttViewAction.OutdentTask:
                            if (activeTask.CanOutdent())
                                activeTask.Outdent();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                UltraMessageBoxManager.Show(ex.Message, rm.GetString("MessageBox_Error"));
            }
        }
        #endregion // PerformIndentOrOutdent

        #region PopulateFontSizeValueList

        /// <summary>
        /// Populates the list of font sizes
        /// </summary>
        private void PopulateFontSizeValueList()
        {
            List<float> fontSizeList = new List<float> ( new float [] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72}); 
            foreach(float i in fontSizeList)
            {
               ((ComboBoxTool)(this.ultraToolbarsManager1.Tools["FontSize"])).ValueList.ValueListItems.Add(i);
            }
        }
        #endregion // PopulateFontSizeValueList

        #region SetTaskPercentage

        /// <summary>
        /// Assigns a percentage complete value to the active task
        /// </summary>
        /// <param name="percent">Percentage to be assigned to the active task</param>
        private void SetTaskPercentage(float percent)
        {
            Task activeTask = this.ultraGanttView1.ActiveTask;
            try
            {
                if (activeTask != null)
                {
                    activeTask.PercentComplete = percent;
                }
            }
            catch (TaskException ex)
            {
                UltraMessageBoxManager.Show(ex.Message, rm.GetString("MessageBox_Error"));
            }
        }
        #endregion // SetTaskPercentage

        #region SetTextBackColor

        /// <summary>
        /// Updates size of the background color of the active cell depending upon the color
        /// selected from the PopupColorPickerTool.
        /// </summary>
        private void SetTextBackColor()
        {
            Color fontBGColor = ((PopupColorPickerTool)this.ultraToolbarsManager1.Tools["Font_BackColor"]).SelectedColor;
            Task activeTask = this.ultraGanttView1.ActiveTask;
            if (activeTask != null)
            {
                TaskField? activeField = this.ultraGanttView1.ActiveField;
                if (activeField.HasValue)
                {
                    activeTask.GridSettings.CellSettings[(TaskField)activeField].Appearance.BackColor = fontBGColor;
                }
            }
        }
        #endregion //SetTextBackColor

        #region SetTextForeColor

        /// <summary>
        /// Updates fore color of the text in the active cell depending upon the color
        /// selected from the PopupColorPickerTool.
        /// </summary>
        private void SetTextForeColor()
        {
            Color fontColor = ((PopupColorPickerTool)this.ultraToolbarsManager1.Tools["Font_ForeColor"]).SelectedColor;
            Task activeTask = this.ultraGanttView1.ActiveTask;
            if (activeTask != null)
            {
                TaskField? activeField = this.ultraGanttView1.ActiveField;
                if (activeField.HasValue)
                {
                    activeTask.GridSettings.CellSettings[(TaskField)activeField].Appearance.ForeColor = fontColor;
                }
            }
        }
        #endregion // SetTextForeColor

        #region ShowSplashScreen

        /// <summary>
        /// Shows the splash screen.
        /// </summary>
        private void ShowSplashScreen()
        {
            SplashScreen splashScreen = new SplashScreen();
            Application.Run(splashScreen);
            Application.ExitThread();
        }
        #endregion //ShowSplashScreen

        #region UpdateFontToolsState

        /// <summary>
        /// Updates the Enabled property for tools in the Font RibbonGroup
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        private void UpdateFontToolsState(bool enabled)
        {
            Utilities.SetRibbonGroupToolsEnabledState(this.ultraToolbarsManager1.Ribbon.Tabs[0].Groups["RibbonGrp_Font"], enabled);
        }

        #endregion // UpdateFontToolsState

        #region UpdateFontName

        /// <summary>
        /// Updates the font depending upon the value selected from the FontListTool.
        /// </summary>
        private void UpdateFontName()
        {
            string fontName = ((FontListTool)this.ultraToolbarsManager1.Tools["FontList"]).Text;
            Task activeTask = this.ultraGanttView1.ActiveTask;
            if (activeTask != null)
            {
                TaskField? activeField = this.ultraGanttView1.ActiveField;
                if (activeField.HasValue)
                {
                    activeTask.GridSettings.CellSettings[(TaskField)activeField].Appearance.FontData.Name = fontName;
                }
            }
        }
        #endregion // UpdateFontName

        #region UpdateFontSize

        /// <summary>
        /// Updates size of the font depending upon the value selected from the ComboBoxTool.
        /// </summary>
        private void UpdateFontSize()
        {
            ValueListItem item = (ValueListItem)((ComboBoxTool)(this.ultraToolbarsManager1.Tools["FontSize"])).SelectedItem;
            if (item != null)
            {
                float fontSize = (float)item.DataValue;
                Task activeTask = this.ultraGanttView1.ActiveTask;
                if (activeTask != null)
                {
                    TaskField? activeField = this.ultraGanttView1.ActiveField;
                    if (activeField.HasValue)
                    {
                        activeTask.GridSettings.CellSettings[(TaskField)activeField].Appearance.FontData.SizeInPoints = fontSize;
                    }
                }
            }
        }
        #endregion // UpdateFontSize

        #region UpdateFontProperty

        /// <summary>
        /// Method to update various font properties.
        /// </summary>
        /// <param name="propertyToUpdate">Enumeration of font related properties</param>
        private void UpdateFontProperty(FontProperties propertyToUpdate)
        {
            Task activeTask = this.ultraGanttView1.ActiveTask;
            if (activeTask != null)
            {
                TaskField? activeField = this.ultraGanttView1.ActiveField;
                if (activeField.HasValue)
                {
                    FontData activeTaskActiveCellFontData = activeTask.GridSettings.CellSettings[(TaskField)activeField].Appearance.FontData;
                    switch (propertyToUpdate)
                    {
                        case FontProperties.Bold:
                            activeTaskActiveCellFontData.Bold = Utilities.ToggleDefaultableBoolean(activeTaskActiveCellFontData.Bold);
                            break;
                        case FontProperties.Italics:
                            activeTaskActiveCellFontData.Italic = Utilities.ToggleDefaultableBoolean(activeTaskActiveCellFontData.Italic);
                            break;
                        case FontProperties.Underline:
                            activeTaskActiveCellFontData.Underline = Utilities.ToggleDefaultableBoolean(activeTaskActiveCellFontData.Underline);
                            break;
                    }
                }
            }
            this.cellActivationRecursionFlag = false;
        }
        #endregion // UpdateFontProperty

        #region UpdateTasksToolsState

        /// <summary>
        /// Verifies the state of the tools in the Tasks RibbonGroup.
        /// </summary>
        /// <param name="activeTask">The active task.</param>
        private void UpdateTasksToolsState(Task activeTask)
        {
            RibbonGroup group = this.ultraToolbarsManager1.Ribbon.Tabs["Ribbon_Task"].Groups["RibbonGrp_Tasks"];

            if (activeTask != null)
            {
                // For summary tasks, the completion percentage is based on it's child tasks
                Utilities.SetRibbonGroupToolsEnabledState(group, !activeTask.IsSummary);

                group.Tools["Tasks_MoveLeft"].SharedProps.Enabled = activeTask.CanOutdent();
                group.Tools["Tasks_MoveRight"].SharedProps.Enabled = activeTask.CanIndent();
                group.Tools["Tasks_Delete"].SharedProps.Enabled = true;
            }
            else
            {
                Utilities.SetRibbonGroupToolsEnabledState(group, false);
            }
        }

        #endregion // UpdateTasksToolsState

        #region UpdateToolsRequiringActiveTask

        /// <summary>
        /// Überprüft den Status aller Werkzeuge, die eine aktiven Arbeitsinhalt erfordern.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        private void UpdateToolsRequiringActiveTask(bool enabled)
        {
            this.ultraToolbarsManager1.Tools["Tasks_Delete"].SharedProps.Enabled = enabled;
            this.ultraToolbarsManager1.Tools["Insert_Milestone"].SharedProps.Enabled = enabled;
            this.ultraToolbarsManager1.Tools["Properties_TaskInformation"].SharedProps.Enabled = enabled;
            this.ultraToolbarsManager1.Tools["Properties_Notes"].SharedProps.Enabled = enabled;
            this.ultraToolbarsManager1.Tools["Insert_Task_TaskAtSelectedRow"].SharedProps.Enabled = enabled;
        }

        #endregion // UpdateToolsRequiringActiveTask

        #endregion Methoden

        #region SplashScreen Events

        #region Events

        /// <summary>
        /// Fired when the staus of the form initialization has changed.
        /// </summary>
        internal static event Terminplan.SplashScreen.InitializationStatusChangedEventHandler InitializationStatusChanged;

        /// <summary>
        /// Fired when the staus of the form initialization has completed.
        /// </summary>
        internal static event EventHandler InitializationComplete;

        /// <summary>
        /// The initialization completed
        /// </summary>
        bool initializationCompleted = false;

        #region OnInitializationComplete
        /// <summary>
        /// Called when [initialization complete].
        /// </summary>
        protected virtual void OnInitializationComplete()
        {
            if (this.initializationCompleted == false)
            {
                this.initializationCompleted = true;

                if (TerminplanForm.InitializationComplete != null)
                    TerminplanForm.InitializationComplete(this, EventArgs.Empty);
            }
        }
        #endregion OnInitializationStatusChanged

        #region OnInitializationStatusChanged
        /// <summary>
        /// Called when [initialization status changed].
        /// </summary>
        /// <param name="status">The status.</param>
        protected virtual void OnInitializationStatusChanged(string status)
        {
            if (TerminplanForm.InitializationStatusChanged != null)
                TerminplanForm.InitializationStatusChanged(this, new Terminplan.SplashScreen.InitializationStatusChangedEventArgs(status));
        }

        /// <summary>
        /// Called when [initialization status changed].
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="showProgressBar">if set to <c>true</c> [show progress bar].</param>
        /// <param name="percentComplete">The percent complete.</param>
        protected virtual void OnInitializationStatusChanged(string status, bool showProgressBar, int percentComplete)
        {
            if (TerminplanForm.InitializationStatusChanged != null)
                TerminplanForm.InitializationStatusChanged(this, new Terminplan.SplashScreen.InitializationStatusChangedEventArgs(status, showProgressBar, percentComplete));
        }
        #endregion OnInitializationStatusChanged

        #endregion Events		

        #endregion //SplashScreen Events
    }
}
