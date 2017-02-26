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
//        History : Datum     bearb.  �nderung
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
        /// <summary> L�scht den aktiven Arbeitsinhalt oder die aktive Aufgabe </summary>
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
                        // Es handelt sich um eine Aufgabe. Diese l�schen
                        this.ultraCalendarInfo1.Tasks.Remove(activeTask);       
                    }
                    else
                    {
                        // Arbeitsinhalt l�schen
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
        /// <summary> Initialisiert die Oberfl�che. </summary>
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

            // F�llt die Liste mit den Farbschematas
            var selectedIndex = 0;                                              // Index des ausgew�hlten Farbschemas (1. Element)
            var themeTool = (ListTool)this.ultraToolbarsManager1.Tools[@"ThemeList"];

            // Alle vorhandenen Farbschematas durchgehen
            foreach (var resourceName in this.themePaths)
            {
                var item = new ListToolItem(resourceName);                      // Eintrag aus der liste

                // In der Liste erscheint nur der Name des Farbschemas ohne Endung in Dateinamen
                var libraryName = resourceName.Replace(@".isl", string.Empty);
                item.Text = libraryName.Remove(0, libraryName.LastIndexOf('.') + 1);
                themeTool.ListToolItems.Add(item);                              // Name des Farbschemas der Liste hinzuf�gen

                // Farbschema 4 (dunkle Farbe Excel) ausw�hlen
                if (item.Text.Contains(@"04"))
                {
                    selectedIndex = item.Index;
                }
            }

            themeTool.SelectedItemIndex = selectedIndex;                        // Ausgew�hltes Farbschema als Standard setzen

            // Das richtigen Listenelement f�r den Touch-Modus ausw�hlen
            ((ListTool)this.ultraToolbarsManager1.Tools[@"TouchMode"]).SelectedItemIndex = 0; // Erstes Element als Auswahl

            // Erstellt eine Liste mit verschiedenen Schriftgr��en
            this.PopulateFontSizeValueList();                                   // Fontliste f�llen
            ((ComboBoxTool)(this.ultraToolbarsManager1.Tools[@"FontSize"])).SelectedIndex = 0;
            ((FontListTool)this.ultraToolbarsManager1.Tools[@"FontList"]).SelectedIndex = 0;
            this.OnUpdateFontToolsState(false);                                 // Font ist nicht ausw�hlbar

            // Aboutbox initialisieren
            Control control = new AboutControl()
            {
                Visible = false,                                                // Aboutbox ist nicht sichtbar
                Parent = this                                                   // Das Hauptformular ist das Elternformular
            };                                                                  // Neue Instanz der Aboutbox erzeugen

            ((PopupControlContainerTool)this.ultraToolbarsManager1.Tools[@"About"]).Control = control; // Aboutbox in die Tools f�r den UltraToolbarsManager setzen

            // Gr��e der Spalten so einstellen, dass alle Daten sichtbar sind.
            this.ultraGanttView1.PerformAutoSizeAllGridColumns();

            // Die Bilder entsprechend dem aktuellen Farbschema einf�rben.
            this.ColorizeImages();
            this.ultraToolbarsManager1.Ribbon.FileMenuButtonCaption = Properties.Resources.ribbonFileTabCaption; // Beschriftung des Datei-Men�s-Button eintragen
        }
        #endregion InitializeUi

        #region MoveTask
        /// <summary>
        /// Verschiebt Start- und Enddatum der Aufgabe r�ckw�rts oder vorw�rts um eine bestimmte Zeitspanne
        /// </summary>
        /// <param name="action">Aufz�hlung der unterst�tzten ganttView-Aktionen</param>
        /// <param name="moveTimeSpan">Zeitspanne zum Verschieben des Start- und Enddatums der Aufgabe</param>
        private void MoveTask(GanttViewAction action, TimeSpanForMoving moveTimeSpan)
        {
            var activeTask = this.ultraGanttView1.ActiveTask;                   // Aktive Aufgabe oder aktiven Arbeitsinhalt ermitteln

            // Nur bearbeiten, falls ein Arbeitsinhalt oder eine Aufgabe existiert und
            // wenn es sich nicht um keine Summe handelt
            if (activeTask == null || activeTask.IsSummary)
            {
                return;                                                         // Abbruch, da Bedingungen nicht erf�llt sind 
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

                case GanttViewAction.MoveTaskDateBackward:                      // Datum zur�ckverlegen
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
        /// F�hrt Einr�ckung oder Auslagerung der aktiven Aufgabe oder des aktiven Atrbeitsinhalts durch
        /// </summary>
        /// <param name="action">die auszuf�hrende Aktion(Einr�ckung oder Auslagerung)</param>
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
                    case GanttViewAction.IndentTask:                            // Einr�ckung
                            
                        // Falls es m�glich ist, Einr�ckung durchf�hren
                        if (activeTask.CanIndent())
                        {
                            activeTask.Indent();
                        }
                            
                        break;
                            
                    case GanttViewAction.OutdentTask:                           // Auslagerung

                        // Falls es m�lich ist, Auslagerung durchf�hren
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
        /// <summary> Liste mit den Schriftgr��en erstellen </summary>
        private void PopulateFontSizeValueList()
        {
            // Schriftgr��en f�r die Liste vorgeben und neue Liste erstellen
            var fontSizeList = new List<float> ( new float [] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72}); 
            
            // Jeden Eintrag der Liste in das Tool f�r die Schriftgr��e des UltraToolbarsManager eintragen
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
        /// Aktualisiert den Wert der Hintergrundfarbe des Textes in der aktiven Zelle abh�ngig von der 
        /// im PopupColorPickerTool ausgew�hlten Farbe.
        /// </summary>
        private void SetTextBackColor()
        {
            // Ausgew�hlte Farbe aus dem ColorPicker ermitteln
            var fontBgColor = ((PopupColorPickerTool)this.ultraToolbarsManager1.Tools[@"Font_BackColor"]).SelectedColor;
            var activeTask = this.ultraGanttView1.ActiveTask;                   // Aktive Aufgabe oder aktiven Arbeitsinhalt ermitteln
            
            // Nur bearbeiten, falls ein aktiver Arbeitsinhalt oder eine aktive Ausgabe existiert
            if (activeTask == null)
            {
                return;
            }

            var activeField = this.ultraGanttView1.ActiveField;                 // Aktive Zelle ermitteln
                
            // Nur bearbeiten, falls in der aktiven Zelle einen Wert enth�lt
            if (activeField.HasValue)
            {
                activeTask.GridSettings.CellSettings[(TaskField)activeField].Appearance.BackColor = fontBgColor; // Hintergrundfarbe der Schrift setzen
            }
        }
        #endregion SetTextBackColor

        #region SetTextForeColor
        /// <summary>
        /// Aktualisiert den Wert der Vordergrundfarbe des Textes in der aktiven Zelle abh�ngig von der 
        /// im PopupColorPickerTool ausgew�hlten Farbe.
        /// </summary>
        private void SetTextForeColor()
        {
            // Ausgew�hlte Farbe aus dem ColorPicker ermitteln
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
        /// <summary> Aktualisiert den Namen der Schriftart je nach dem im FontListTool ausgew�hlten Wert. </summary>
        private void UpdateFontName()
        {
            // Namen der ausgew�hlte Schriftart aus der Fontliste ermitteln
            var fontName = ((FontListTool)this.ultraToolbarsManager1.Tools[@"FontList"]).Text;
            var activeTask = this.ultraGanttView1.ActiveTask;                  // Aktive Aufgabe oder aktiven Arbeitsinhalt ermitteln

            // Nur bearbeiten, falls ein aktiver Arbeitsinhalt oder eine aktive Ausgabe existiert
            if (activeTask == null)
            {
                return;
            }

            var activeField = this.ultraGanttView1.ActiveField;                 // Aktive Zelle ermitteln
                
            // Nur bearbeiten, falls in der aktiven Zelle einen Wert enth�lt
            if (activeField.HasValue)
            {
                activeTask.GridSettings.CellSettings[(TaskField)activeField].Appearance.FontData.Name = fontName; // Namen der Schriftart der Zelle zuweisen
            }
        }
        #endregion UpdateFontName

        #region UpdateFontSize

        /// <summary> Aktualisiert die Schriftgr��e je nach dem im ComboBoxTool ausgew�hlten Wert. </summary>
        private void UpdateFontSize()
        {
            // Gr��e der ausgew�hlte Schriftart aus dem ComboBoxTool ermitteln
            var item = (ValueListItem)((ComboBoxTool)(this.ultraToolbarsManager1.Tools[@"FontSize"])).SelectedItem;

            // Nur bearbeiten, falls ein Wert vorhanden ist
            if (item == null)
            {
                return;
            }

            var fontSize = (float)item.DataValue;                               // Schriftgr��e 
            var activeTask = this.ultraGanttView1.ActiveTask;                   // Aktive Aufgabe oder aktiven Arbeitsinhalt ermitteln
                
            // Nur bearbeiten, falls ein aktiver Arbeitsinhalt oder eine aktive Ausgabe existiert
            if (activeTask == null)
            {
                return;
            }

            var activeField = this.ultraGanttView1.ActiveField;                 // Aktive Zelle ermitteln
                    
            // Nur bearbeiten, falls in der aktiven Zelle einen Wert enth�lt
            if (activeField.HasValue)
            {
                activeTask.GridSettings.CellSettings[(TaskField)activeField].Appearance.FontData.SizeInPoints = fontSize; // Schriftgr��e der Zelle zuweisen
            }
        }
        #endregion UpdateFontSize

        #region UpdateFontProperty

        /// <summary>Methode, um verschiedene Eigenschaften der Schriftart zu aktualisieren</summary>
        /// <remarks> Die Eigenschaften werden umgeschaltet.</remarks>
        /// <param name="propertyToUpdate">Aufz�hlung von Eigenschaften, welche von der Schriftart abh�ngig sind</param>
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

            this.cellActivationRecursionFlag = false;                           // Zellen m�ssen nicht rekursiv bearbeitet werden, gilt also nur f�r eine Zelle
        }
        #endregion UpdateFontProperty

        #region UpdateTasksToolsState
        /// <summary> �berpr�ft den Status der Werkzeuge in der RibbonGruppe "RibbonGrp_Tasks" </summary>
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
                group.Tools[@"Tasks_Delete"].SharedProps.Enabled = true;        // L�schen freigeben
            }
            else
            {
                // Es gibt weder einen aktiven Arbeitsinhalt noch eine aktive Aufgabe
                DienstProgramme.SetRibbonGroupToolsEnabledState(group, false);  // Gruppe ist gesperrt
            }
        }
        #endregion UpdateTasksToolsState

        #region UpdateToolsRequiringActiveTask
        /// <summary> �berpr�ft den Status aller Werkzeuge, die eine aktiven Arbeitsinhalt erfordern. </summary>
        /// <param name="enabled">falls auf <c>true</c>gesetzt, wird Werkzeug freigegeben, sonst gesperrt.</param>
        private void UpdateToolsRequiringActiveTask(bool enabled)
        {
            this.ultraToolbarsManager1.Tools[@"Tasks_Delete"].SharedProps.Enabled = enabled;                     // L�schen freigeben oder sperren
            this.ultraToolbarsManager1.Tools[@"Insert_Milestone"].SharedProps.Enabled = enabled;                 // Meilensteine freigeben oder sperren
            this.ultraToolbarsManager1.Tools[@"Properties_TaskInformation"].SharedProps.Enabled = enabled;       // Anzeige der Informationen zum aktuellen Arbeitsinhalt oder der aktuellen Aufgabe freigeben oder sperren
            this.ultraToolbarsManager1.Tools[@"Properties_Notes"].SharedProps.Enabled = enabled;                 // Anzeige der Beschreibung freigeben oder sperren
            this.ultraToolbarsManager1.Tools[@"Insert_Task_TaskAtSelectedRow"].SharedProps.Enabled = enabled;    // Einf�gen bei ausgew�hlter Zeile freigeben oder sperren
        }
        #endregion UpdateToolsRequiringActiveTask
        #endregion Methoden
    }
}