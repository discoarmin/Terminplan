// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HauptformEreignisse2.cs" company="EST GmbH + CO.KG">
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
//                  08.01.17  br      Grundversion
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Terminplan
{
    using Infragistics.Win;
    using Infragistics.Win.UltraMessageBox;
    using Infragistics.Win.UltraWinSchedule;
    using Infragistics.Win.UltraWinToolbars;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Forms;

    /// <summary>
    /// Klasse TerminPlanForm (Hauptformular).
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class TerminPlanForm
    {
        #region Methoden
        #region DeleteTask
        /// <summary> Löscht den aktiven Arbeitsinhalt oder die aktive Aufgabe </summary>
        private void DeleteTask()
        {
            var activeTask = this.ultraGanttView1.ActiveTask;                   // Aktiven Arbeitsinhalt oder Aufgabe ermitteln
            try
            {
                // Nur bearbeiten, falls ein Arbeitsinhalt oder eine Aufgabe aktiv ist
                if (activeTask != null)
                {
                    var parent = activeTask.Parent;                             // Arbeitsinhalt ermitteln

                    if (parent == null)
                    {
                        // Es handelt sich um eine Aufgabe. Diese löschen
                        this.ultraCalendarInfo1.Tasks.Remove(activeTask);       
                    }
                    else
                    {
                        // Arbeitsinhalt löschen
                        parent.Tasks.Remove(activeTask);
                    }
                }

                // Status aktualisieren
                var newActiveTask = this.ultraGanttView1.ActiveTask;
                this.UpdateTasksToolsState(newActiveTask);
                this.UpdateToolsRequiringActiveTask(newActiveTask != null);
            }
            catch (TaskException ex)
            {
                UltraMessageBoxManager.Show(ex.Message, this.rm.GetString("MessageBox_Error"));
            }
        }
        #endregion DeleteTask

        #region InitializeUi
        /// <summary> Initialisiert die Oberfläche. </summary>
        private void InitializeUi()
        {
            var culture = CultureInfo.InstalledUICulture;                       // Sprache des Betriebssystems ermitteln
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

            var col = this.ultraGanttView1.GridSettings.ColumnSettings.Values;

            // Spaltenbreite einstellen
            foreach (var de in col)
            {
                // Arbeitsinhalt oder Aufgabe
                if (de.Key.ToLower() == @"name")
                {
                    //de.Text = "Arbeitsinhalt/Aufgabe";
                    de.Text = @"Verfahren";
                    de.Visible = DefaultableBoolean.True;
                }

                // Dauer
                if (de.Key.ToLower() == @"duration")
                {
                    de.Text = @"Dauer";
                    de.Visible = DefaultableBoolean.True;
                }

                // Start
                if (de.Key.ToLower() == @"start")
                {
                    de.Text = @"Start";
                    de.Visible = DefaultableBoolean.True;
                }

                // Ende
                if (de.Key.ToLower() == @"enddatetime")
                {
                    de.Text = @"Ende";
                    de.Visible = DefaultableBoolean.True;
                }

                // Fertig in %
                if (de.Key.ToLower() == @"percentcomplete")
                {
                    de.Text = @"Status";
                    de.Visible = DefaultableBoolean.True;
                }
            }

            //this.ultraGanttView1.GridAreaWidth = splitterWeite;

            // Füllt die Liste mit den Farbschematas
            var selectedIndex = 0;                                              // Index des ausgewählten Farbschemas (1. Element)
            var themeTool = (ListTool)this.ultraToolbarsManager1.Tools[@"ThemeList"];

            // Alle vorhandenen Farbschematas durchgehen
            foreach (var resourceName in this.themePaths)
            {
                var item = new ListToolItem(resourceName);                      // Eintrag aus der liste

                // In der Liste erscheint nur der Name des Farbschemas ohne Endung in Dateinamen
                var libraryName = resourceName.Replace(@".isl", string.Empty);
                item.Text = libraryName.Remove(0, libraryName.LastIndexOf('.') + 1);
                themeTool.ListToolItems.Add(item);                              // Name des Farbschemas der Liste hinzufügen

                // Farbschema 4 (dunkle Farbe Excel) auswählen
                if (item.Text.Contains(@"04"))
                {
                    selectedIndex = item.Index;
                }
            }

            themeTool.SelectedItemIndex = selectedIndex;                        // Ausgewähltes Farbschema als Standard setzen

            // Das richtigen Listenelement für den Touch-Modus auswählen
            ((ListTool)this.ultraToolbarsManager1.Tools[@"TouchMode"]).SelectedItemIndex = 0; // Erstes Element als Auswahl

            // Erstellt eine Liste mit verschiedenen Schriftgrößen
            this.PopulateFontSizeValueList();                                   // Fontliste füllen
            ((ComboBoxTool)(this.ultraToolbarsManager1.Tools[@"FontSize"])).SelectedIndex = 0;
            ((FontListTool)this.ultraToolbarsManager1.Tools[@"FontList"]).SelectedIndex = 0;
            this.OnUpdateFontToolsState(false);                                 // Font ist nicht auswählbar

            // Aboutbox initialisieren
            Control control = new AboutControl()
            {
                Visible = false,                                                // Aboutbox ist nicht sichtbar
                Parent = this                                                   // Das Hauptformular ist das Elternformular
            };                                                                  // Neue Instanz der Aboutbox erzeugen

            ((PopupControlContainerTool)this.ultraToolbarsManager1.Tools[@"About"]).Control = control; // Aboutbox in die Tools für den UltraToolbarsManager setzen

            // Größe der Spalten so einstellen, dass alle Daten sichtbar sind.
            this.ultraGanttView1.PerformAutoSizeAllGridColumns();

            // Die Bilder entsprechend dem aktuellen Farbschema einfärben.
            this.ColorizeImages();
            this.ultraToolbarsManager1.Ribbon.FileMenuButtonCaption = Properties.Resources.ribbonFileTabCaption; // Beschriftung des Datei-Menüs-Button eintragen
        }
        #endregion InitializeUi

        #region MoveTask
        /// <summary>
        /// Verschiebt Start- und Enddatum der Aufgabe rückwärts oder vorwärts um eine bestimmte Zeitspanne
        /// </summary>
        /// <param name="action">Aufzählung der unterstützten ganttView-Aktionen</param>
        /// <param name="moveTimeSpan">Zeitspanne zum Verschieben des Start- und Enddatums der Aufgabe</param>
        private void MoveTask(GanttViewAction action, TimeSpanForMoving moveTimeSpan)
        {
            var activeTask = this.ultraGanttView1.ActiveTask;                   // Aktive Aufgabe oder aktiven Arbeitsinhalt ermitteln

            // Nur bearbeiten, falls ein Arbeitsinhalt oder eine Aufgabe existiert und
            // wenn es sich nicht um keine Summe handelt
            if (activeTask == null || activeTask.IsSummary)
            {
                return;                                                         // Abbruch, da Bedingungen nicht erfüllt sind 
            }

            // Aktion auswerten
            switch (action)
            {
                case GanttViewAction.MoveTaskDateForward:                       // Datum vorverlegen
                    {
                        // Zeitspanne auswerten
                        // ReSharper disable once SwitchStatementMissingSomeCases
                        switch (moveTimeSpan)
                        {
                            case TimeSpanForMoving.OneDay:                      // einen Tag
                                activeTask.StartDateTime = activeTask.StartDateTime.AddDays(1);
                                break;
                            case TimeSpanForMoving.OneWeek:                     // eine Woche
                                activeTask.StartDateTime = activeTask.StartDateTime.AddDays(7);
                                break;
                            case TimeSpanForMoving.FourWeeks:                   // vier Wochen
                                activeTask.StartDateTime = activeTask.StartDateTime.AddDays(28);
                                break;
                        }
                    }
                    break;

                case GanttViewAction.MoveTaskDateBackward:                      // Datum zurückverlegen
                    {
                        // Zeitspanne auswerten
                        switch (moveTimeSpan)
                        {
                            case TimeSpanForMoving.OneDay:                      // einen Tag
                                activeTask.StartDateTime = activeTask.StartDateTime.Subtract(TimeSpan.FromDays(1));
                                break;
                            case TimeSpanForMoving.OneWeek:                     // eine Woche
                                activeTask.StartDateTime = activeTask.StartDateTime.Subtract(TimeSpan.FromDays(7));
                                break;
                            case TimeSpanForMoving.FourWeeks:                   // vier Wochen
                                activeTask.StartDateTime = activeTask.StartDateTime.Subtract(TimeSpan.FromDays(28));
                                break;
                        }
                    }
                    break;
            }
        }
        #endregion  MoveTask

        #region PerformIndentOrOutdent
        /// <summary>
        /// Führt Einrückung oder Auslagerung der aktiven Aufgabe oder des aktiven Atrbeitsinhalts durch
        /// </summary>
        /// <param name="action">die auszuführende Aktion(Einrückung oder Auslagerung)</param>
        private void PerformIndentOrOutdent(GanttViewAction action)
        {
            var activeTask = this.ultraGanttView1.ActiveTask;                   // Aktive Aufgabe oder aktiven Arbeitsinhalt ermitteln

            try
            {
                // Nur bearbeiten, falls ein aktiver Arbeitsinhalt oder eine aktive Ausgabe existiert
                if (activeTask == null)
                {
                    return;
                }

                // Aktion auswerten
                switch (action)
                {
                    case GanttViewAction.IndentTask:                            // Einrückung
                            
                        // Falls es möglich ist, Einrückung durchführen
                        if (activeTask.CanIndent())
                        {
                            activeTask.Indent();
                        }
                            
                        break;
                            
                    case GanttViewAction.OutdentTask:                           // Auslagerung

                        // Falls es mölich ist, Auslagerung durchführen
                        if (activeTask.CanOutdent())
                        {
                            activeTask.Outdent();
                        }
                            
                        break;
                }
            }
            catch (Exception ex)
            {
                // Bei einem aufgetretenen Fehler diesen anzeigen
                UltraMessageBoxManager.Show(ex.Message, this.rm.GetString("MessageBox_Error"));
            }
        }
        #endregion PerformIndentOrOutdent

        #region PopulateFontSizeValueList
        /// <summary> Liste mit den Schriftgrößen erstellen </summary>
        private void PopulateFontSizeValueList()
        {
            // Schriftgrößen für die Liste vorgeben und neue Liste erstellen
            var fontSizeList = new List<float> ( new float [] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72}); 
            
            // Jeden Eintrag der Liste in das Tool für die Schriftgröße des UltraToolbarsManager eintragen
            foreach(var i in fontSizeList)
            {
               ((ComboBoxTool)(this.ultraToolbarsManager1.Tools[@"FontSize"])).ValueList.ValueListItems.Add(i);
            }
        }
        #endregion PopulateFontSizeValueList

        #region SetTaskPercentage
        /// <summary>
        /// Weist der aktiven Aufgabe oder der aktiven Arbeitsanweisung einen Prozentsatz des Fertigungsgrads zu       
        /// </summary>
        /// <param name="prozentSatz">der zuzuweisende Prozentsatz</param>
        private void SetTaskPercentage(float prozentSatz)
        {
            var activeTask = this.ultraGanttView1.ActiveTask;                   // Aktive Aufgabe oder aktiven Arbeitsinhalt ermitteln
            try
            {
                // Nur bearbeiten, falls ein aktiver Arbeitsinhalt oder eine aktive Ausgabe existiert
                if (activeTask != null)
                {
                    activeTask.PercentComplete = prozentSatz;                   // Prozentsatz zuweisen
                }
            }
            catch (TaskException ex)
            {
                // Bei einem aufgetretenen Fehler diesen anzeigen
                UltraMessageBoxManager.Show(ex.Message, this.rm.GetString("MessageBox_Error"));
            }
        }
        #endregion /SetTaskPercentage

        #region SetTextBackColor
        /// <summary>
        /// Aktualisiert den Wert der Hintergrundfarbe des Textes in der aktiven Zelle abhängig von der 
        /// im PopupColorPickerTool ausgewählten Farbe.
        /// </summary>
        private void SetTextBackColor()
        {
            // Ausgewählte Farbe aus dem ColorPicker ermitteln
            var fontBgColor = ((PopupColorPickerTool)this.ultraToolbarsManager1.Tools[@"Font_BackColor"]).SelectedColor;
            var activeTask = this.ultraGanttView1.ActiveTask;                   // Aktive Aufgabe oder aktiven Arbeitsinhalt ermitteln
            
            // Nur bearbeiten, falls ein aktiver Arbeitsinhalt oder eine aktive Ausgabe existiert
            if (activeTask == null)
            {
                return;
            }

            var activeField = this.ultraGanttView1.ActiveField;                 // Aktive Zelle ermitteln
                
            // Nur bearbeiten, falls in der aktiven Zelle einen Wert enthält
            if (activeField.HasValue)
            {
                activeTask.GridSettings.CellSettings[(TaskField)activeField].Appearance.BackColor = fontBgColor; // Hintergrundfarbe der Schrift setzen
            }
        }
        #endregion SetTextBackColor

        #region SetTextForeColor
        /// <summary>
        /// Aktualisiert den Wert der Vordergrundfarbe des Textes in der aktiven Zelle abhängig von der 
        /// im PopupColorPickerTool ausgewählten Farbe.
        /// </summary>
        private void SetTextForeColor()
        {
            // Ausgewählte Farbe aus dem ColorPicker ermitteln
            var fontColor = ((PopupColorPickerTool)this.ultraToolbarsManager1.Tools[@"Font_ForeColor"]).SelectedColor;
            var activeTask = this.ultraGanttView1.ActiveTask;                   // Aktive Aufgabe oder aktiven Arbeitsinhalt ermitteln
            
            // Nur bearbeiten, falls ein aktiver Arbeitsinhalt oder eine aktive Ausgabe existiert
            if (activeTask == null)
            {
                return;
            }

            var activeField = this.ultraGanttView1.ActiveField;                 // Aktive Zelle ermitteln
            if (activeField.HasValue)
            {
                activeTask.GridSettings.CellSettings[(TaskField)activeField].Appearance.ForeColor = fontColor; // Vordergrundfarbe der Schrift setzen
            }
        }
        #endregion SetTextForeColor

        #region UpdateFontName
        /// <summary> Aktualisiert den Namen der Schriftart je nach dem im FontListTool ausgewählten Wert. </summary>
        private void UpdateFontName()
        {
            // Namen der ausgewählte Schriftart aus der Fontliste ermitteln
            var fontName = ((FontListTool)this.ultraToolbarsManager1.Tools[@"FontList"]).Text;
            var activeTask = this.ultraGanttView1.ActiveTask;                  // Aktive Aufgabe oder aktiven Arbeitsinhalt ermitteln

            // Nur bearbeiten, falls ein aktiver Arbeitsinhalt oder eine aktive Ausgabe existiert
            if (activeTask == null)
            {
                return;
            }

            var activeField = this.ultraGanttView1.ActiveField;                 // Aktive Zelle ermitteln
                
            // Nur bearbeiten, falls in der aktiven Zelle einen Wert enthält
            if (activeField.HasValue)
            {
                activeTask.GridSettings.CellSettings[(TaskField)activeField].Appearance.FontData.Name = fontName; // Namen der Schriftart der Zelle zuweisen
            }
        }
        #endregion UpdateFontName

        #region UpdateFontSize

        /// <summary> Aktualisiert die Schriftgröße je nach dem im ComboBoxTool ausgewählten Wert. </summary>
        private void UpdateFontSize()
        {
            // Größe der ausgewählte Schriftart aus dem ComboBoxTool ermitteln
            var item = (ValueListItem)((ComboBoxTool)(this.ultraToolbarsManager1.Tools[@"FontSize"])).SelectedItem;

            // Nur bearbeiten, falls ein Wert vorhanden ist
            if (item == null)
            {
                return;
            }

            var fontSize = (float)item.DataValue;                               // Schriftgröße 
            var activeTask = this.ultraGanttView1.ActiveTask;                   // Aktive Aufgabe oder aktiven Arbeitsinhalt ermitteln
                
            // Nur bearbeiten, falls ein aktiver Arbeitsinhalt oder eine aktive Ausgabe existiert
            if (activeTask == null)
            {
                return;
            }

            var activeField = this.ultraGanttView1.ActiveField;                 // Aktive Zelle ermitteln
                    
            // Nur bearbeiten, falls in der aktiven Zelle einen Wert enthält
            if (activeField.HasValue)
            {
                activeTask.GridSettings.CellSettings[(TaskField)activeField].Appearance.FontData.SizeInPoints = fontSize; // Schriftgröße der Zelle zuweisen
            }
        }
        #endregion UpdateFontSize

        #region UpdateFontProperty

        /// <summary>Methode, um verschiedene Eigenschaften der Schriftart zu aktualisieren</summary>
        /// <remarks> Die Eigenschaften werden umgeschaltet.</remarks>
        /// <param name="propertyToUpdate">Aufzählung von Eigenschaften, welche von der Schriftart abhängig sind</param>
        private void UpdateFontProperty(FontProperties propertyToUpdate)
        {
            var activeTask = this.ultraGanttView1.ActiveTask;                   // Aktive Aufgabe oder aktiven Arbeitsinhalt ermitteln
            
            // Nur bearbeiten, falls ein aktiver Arbeitsinhalt oder eine aktive Ausgabe existiert
            if (activeTask != null)
            {
                var activeField = this.ultraGanttView1.ActiveField;             // Aktive Zelle ermitteln
                if (activeField.HasValue)
                {
                    var activeTaskActiveCellFontData = activeTask.GridSettings.CellSettings[(TaskField)activeField].Appearance.FontData; // Daten der Schrift in der aktiven Zelle ermitteln
                    
                    // Art der Daten auswerten
                    switch (propertyToUpdate)
                    {
                        case FontProperties.Bold:                               // Fettschrift
                            activeTaskActiveCellFontData.Bold = DienstProgramme.ToggleDefaultableBoolean(activeTaskActiveCellFontData.Bold); // Fettschrift aus- oder einschalten
                            break;
                        case FontProperties.Italics:                            // Kursiv
                            activeTaskActiveCellFontData.Italic = DienstProgramme.ToggleDefaultableBoolean(activeTaskActiveCellFontData.Italic); // Kursivschrift umschalten
                            break;
                        case FontProperties.Underline:                          // Unterstrichen
                            activeTaskActiveCellFontData.Underline = DienstProgramme.ToggleDefaultableBoolean(activeTaskActiveCellFontData.Underline); // unterstrichene Schrift umschalten
                            break;
                    }
                }
            }

            this.cellActivationRecursionFlag = false;                           // Zellen müssen nicht rekursiv bearbeitet werden, gilt also nur für eine Zelle
        }
        #endregion UpdateFontProperty

        #region UpdateTasksToolsState
        /// <summary> Überprüft den Status der Werkzeuge in der RibbonGruppe "RibbonGrp_Tasks" </summary>
        /// <param name="activeTask">Der aktive Arbeitsinhalt oder die aktive Aufgabe.</param>
        private void UpdateTasksToolsState(Task activeTask)
        {
            // Gruppe "RibbonGrp_Tasks" laden 
            var group = this.ultraToolbarsManager1.Ribbon.Tabs[@"Ribbon_Task"].Groups[@"RibbonGrp_Tasks"];

            // Nur bearbeiten, falls ein aktiver Arbeitsinhalt oder eine aktive Ausgabe existiert
            if (activeTask != null)
            {
                // Der Fertigungsgrad des Arbeitsinhalts basiert auf den Fertigungsgraden der untergeordneten Aufgaben                
                DienstProgramme.SetRibbonGroupToolsEnabledState(group, !activeTask.IsSummary);
                group.Tools[@"Tasks_MoveLeft"].SharedProps.Enabled = activeTask.CanOutdent(); // Freigeben, wenn noch nach links verschoben werden kann
                group.Tools[@"Tasks_MoveRight"].SharedProps.Enabled = activeTask.CanIndent(); // Freigeben, wenn noch nach rechts verschoben werden kann
                group.Tools[@"Tasks_Delete"].SharedProps.Enabled = true;        // Löschen freigeben
            }
            else
            {
                // Es gibt weder einen aktiven Arbeitsinhalt noch eine aktive Aufgabe
                DienstProgramme.SetRibbonGroupToolsEnabledState(group, false);  // Gruppe ist gesperrt
            }
        }
        #endregion UpdateTasksToolsState

        #region UpdateToolsRequiringActiveTask
        /// <summary> Überprüft den Status aller Werkzeuge, die eine aktiven Arbeitsinhalt erfordern. </summary>
        /// <param name="enabled">falls auf <c>true</c>gesetzt, wird Werkzeug freigegeben, sonst gesperrt.</param>
        private void UpdateToolsRequiringActiveTask(bool enabled)
        {
            this.ultraToolbarsManager1.Tools[@"Tasks_Delete"].SharedProps.Enabled = enabled;                     // Löschen freigeben oder sperren
            this.ultraToolbarsManager1.Tools[@"Insert_Milestone"].SharedProps.Enabled = enabled;                 // Meilensteine freigeben oder sperren
            this.ultraToolbarsManager1.Tools[@"Properties_TaskInformation"].SharedProps.Enabled = enabled;       // Anzeige der Informationen zum aktuellen Arbeitsinhalt oder der aktuellen Aufgabe freigeben oder sperren
            this.ultraToolbarsManager1.Tools[@"Properties_Notes"].SharedProps.Enabled = enabled;                 // Anzeige der Beschreibung freigeben oder sperren
            this.ultraToolbarsManager1.Tools[@"Insert_Task_TaskAtSelectedRow"].SharedProps.Enabled = enabled;    // Einfügen bei ausgewählter Zeile freigeben oder sperren
        }
        #endregion UpdateToolsRequiringActiveTask
        #endregion Methoden
    }
}