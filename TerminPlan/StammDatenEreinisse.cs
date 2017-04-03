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
//                  31.03.17  br      Einfärben 2. Toolbarmanager
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Terminplan
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using Infragistics.Win;
    using Infragistics.Win.AppStyling;
    using Infragistics.Win.Printing;
    using Infragistics.Win.Touch;
    using Infragistics.Win.UltraWinGrid;
    using Infragistics.Win.UltraWinToolbars;
    using PropertyIds = Infragistics.Win.UltraWinToolbars.PropertyIds;

    /// <summary>
    /// Klasse TerminPlanForm (Hauptformular).
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
    public partial class StammDaten : Form
    {
        #region Ereignisprozeduren

        #region ApplicationStyleChanged

        /// <summary> Behandelt das StyleChanged-Ereignis des Application Styling Managers </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.AppStyling.StyleChangedEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void ApplicationStyleChanged(object sender, StyleChangedEventArgs e)
        {
            // Bilder an das ausgewählte Farbschema anpassen.
            ColorizeImages();
        }

        #endregion ApplicationStyleChanged

        #region UltraToolbarsManager2PropertyChanged

        /// <summary>
        /// Behandelt das PropertyChanged-Ereignis des ultraToolbarsManager2 Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.PropertyChangedEventArgs" /> Instanz,welche die Ereignisdaten enthält.</param>
        private void UltraToolbarsManager2PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var trigger = e.ChangeInfo.FindTrigger(null);                       // Ermitteln, welche Eigenschaft geändert wurde

            // Nur bearbeiten, wenn es eine Eigenschaft ist, welche vom Toolbars-Manager verwaltet wird
            if (trigger == null || !(trigger.Source is SharedProps) || !(trigger.PropId is PropertyIds))
            {
                return;
            }

            // ID auswerten
            switch ((PropertyIds)trigger.PropId)
            {
                case PropertyIds.Enabled:     // Nur freigegebene Eigenschaften bearbeiten
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

        #endregion UltraToolbarsManager2PropertyChanged

        #region UltraToolbarsManagerToolClick

        /// <summary>
        /// Behandelt das ToolClick-Ereignis of the ultraToolbarsManager2 control.
        /// </summary>
        /// <remarks>
        /// Die jeweilige Aktion wird nur durchgeführt, wenn sie nicht schon durchgeführt wurde
        /// </remarks>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">Die <see cref="Infragistics.Win.UltraWinToolbars.ToolClickEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void OnUltraToolbarsManagerToolClick(object sender, ToolClickEventArgs e)
        {
            // Ermitteln, auf welches Tool geklickt wurde
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
                    if (this.cellActivationRecursionFlag == false)              // Unterstrichene Schrift
                    {
                        this.UpdateFontProperty(FontProperties.Underline);
                    }
                    break;

                case "Font_BackColor":                                          // Hintergrundfarbe
                    SetTextBackColor();
                    break;

                case "Font_ForeColor":                                          // Vordergrundfarbe
                    SetTextForeColor();
                    break;

                case "FontList":                                                // Liste mit den Schriftarten
                    UpdateFontName();
                    break;

                case "FontSize":                                                // Schriftgröße
                    UpdateFontSize();
                    break;

                case "Insert_Task_Task":                                        // Neuen Arbeitsinhalt oder neue Aufgabe am Ende hinzufügen
                    AddNewTask(false);
                    break;

                case "Insert_Task_TaskAtSelectedRow":                           // Neuen Arbeitsinhalt oder neue Aufgabe nach der aktuellen Zeile hinzufügen
                    AddNewTask(true);
                    break;

                case "Tasks_PercentComplete_0":                                 // Fertigungsgrad 0%
                    SetTaskPercentage(0);
                    break;

                case "Tasks_PercentComplete_25":                                // Fertigungsgrad 25%
                    SetTaskPercentage(25);
                    break;

                case "Tasks_PercentComplete_50":                                // Fertigungsgrad 50%
                    SetTaskPercentage(50);
                    break;

                case "Tasks_PercentComplete_75":                                // Fertigungsgrad 75%
                    SetTaskPercentage(75);
                    break;

                case "Tasks_PercentComplete_100":                               // Fertigungsgrad 100%
                    SetTaskPercentage(100);
                    break;

                case "Tasks_MoveLeft":                                          // Nach links verschieben
                    PerformIndentOrOutdent(GanttViewAction.OutdentTask);
                    break;

                case "Tasks_MoveRight":                                         // Nach rechts verschieben
                    PerformIndentOrOutdent(GanttViewAction.IndentTask);
                    break;

                case "Tasks_Delete":                                            // Arbeitsinhalt oder Aufgabe löschen
                    DeleteTask();
                    break;

                case "Schedule_OnMoveTask_1Day":                                // Starttermin 1 Tag später
                    MoveTask(GanttViewAction.MoveTaskDateForward, TimeSpanForMoving.OneDay);
                    break;

                case "Schedule_OnMoveTask_1Week":                               // Starttermin 1 Woche später
                    MoveTask(GanttViewAction.MoveTaskDateForward, TimeSpanForMoving.OneWeek);
                    break;

                case "Schedule_MoveTask_4Weeks":                                // Starttermin 4 Wochen später
                    MoveTask(GanttViewAction.MoveTaskDateForward, TimeSpanForMoving.FourWeeks);
                    break;

                case "Schedule_MoveTask_MoveTaskBackwards1Day":                 // Sterttrmin 1 Tag früher
                    MoveTask(GanttViewAction.MoveTaskDateBackward, TimeSpanForMoving.OneDay);
                    break;

                case "Schedule_MoveTask_MoveTaskBackwards1Week":                // Starttermin 1 Woche früher
                    MoveTask(GanttViewAction.MoveTaskDateBackward, TimeSpanForMoving.OneWeek);
                    break;

                case "Schedule_MoveTask_MoveTaskBackwards4Weeks":               // Starttermin 4 Wochen früher
                    MoveTask(GanttViewAction.MoveTaskDateBackward, TimeSpanForMoving.FourWeeks);
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
                        StyleManager.Load(DienstProgramme.GetEmbeddedResourceStream(key));
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
                    ErstelleNeuesProjekt();                                     // Neuen Terminplan hinzufügen
                    break;

                case "Open":                                                    // Datei laden
                    LadeDatei();
                    break;

                case "Speichern":                                               // Terminplan speichern
                    //this.Speichern(Path.Combine(Application.StartupPath, @"Data.TestDatenEST.XML"));
                    //this.Speichern(Path.Combine(Application.StartupPath, @"Data.TestDaten1EST.XML"));

                    // Ermitteln, ob eine Datei geladen wurde. Wenn nicht, muss 'Speichern unter' aufgerufen werden
                    if (string.IsNullOrEmpty(this.GeladeneDatei))
                    {
                        SpeichernUnter(Path.Combine(Application.StartupPath, prjName + @"Terminplan.XML"));
                    }
                    else
                    {
                        Speichern(this.GeladeneDatei);                          // geladene Datei speichern
                    }
                    break;

                case "Speichern unter":                                         // Terminplan unter anderem Namen speichern
                    SpeichernUnter(Path.Combine(Application.StartupPath, @"Terminplan.XML"));
                    break;
            }
        }

        #endregion UltraToolbarsManagerToolClick

        #region ultraToolbarsManager2ToolValueChanged

        /// <summary>
        /// Behandelt das ToolValueChanged-Ereignis des ultraToolbarsManager2 Kontrols.
        /// </summary>
        /// <param name="sender">Die Quelle des Ereignisses.</param>
        /// <param name="e">The <see cref="ToolEventArgs" /> Instanz, welche die Ereignisdaten enthält.</param>
        private void ultraToolbarsManager2ToolValueChanged(object sender, ToolEventArgs e)
        {
            switch (e.Tool.Key)
            {
                case "Font_BackColor":
                    SetTextBackColor();
                    break;

                case "Font_ForeColor":
                    OnSetTextForeColor();
                    break;

                case "FontList":
                    UpdateFontName();
                    break;

                case "FontSize":
                    UpdateFontSize();
                    break;
            }
        }

        #endregion ultraToolbarsManager2ToolValueChanged

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
            if (propChanged.PropId is TouchProviderPropertyIds &&
                ((TouchProviderPropertyIds)propChanged.PropId) == TouchProviderPropertyIds.Enabled)
            {
                this.ultraGanttView1.PerformAutoSizeAllGridColumns();           // Größe Gridspalten einstellen
            }
        }

        #endregion UltraTouchProvider1PropertyChanged

        #endregion Ereignisprozeduren

        #region Methoden

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
            var fontColor = ((PopupColorPickerTool)this.ultraToolbarsManager2.Tools[@"Font_ForeColor"]).SelectedColor;
            var activeTask = ultraGanttView1.ActiveTask;                   // Aktive Aufgabe oder aktiven Arbeitsinhalt ermitteln

            // Nur bearbeiten, falls ein aktiver Arbeitsinhalt oder eine aktive Ausgabe existiert
            if (activeTask == null)
            {
                return;
            }

            var activeField = ultraGanttView1.ActiveField;                 // Aktive Zelle ermitteln
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
            var fontName = ((FontListTool)ultraToolbarsManager1.Tools[@"FontList"]).Text;
            var activeTask = ultraGanttView1.ActiveTask;                  // Aktive Aufgabe oder aktiven Arbeitsinhalt ermitteln

            // Nur bearbeiten, falls ein aktiver Arbeitsinhalt oder eine aktive Ausgabe existiert
            if (activeTask == null)
            {
                return;
            }

            var activeField = ultraGanttView1.ActiveField;                 // Aktive Zelle ermitteln

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
            var item = (ValueListItem)((ComboBoxTool)(ultraToolbarsManager1.Tools[@"FontSize"])).SelectedItem;

            // Nur bearbeiten, falls ein Wert vorhanden ist
            if (item == null)
            {
                return;
            }

            var fontSize = (float)item.DataValue;                               // Schriftgröße
            var activeTask = ultraGanttView1.ActiveTask;                   // Aktive Aufgabe oder aktiven Arbeitsinhalt ermitteln

            // Nur bearbeiten, falls ein aktiver Arbeitsinhalt oder eine aktive Ausgabe existiert
            if (activeTask == null)
            {
                return;
            }

            var activeField = ultraGanttView1.ActiveField;                 // Aktive Zelle ermitteln

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
            var activeCell = this.ultraGridStammDaten.ActiveCell;               // Aktive Zelle im Grid ermitteln ermitteln
            UltraGridCell zelle;
            // Nur bearbeiten, falls keine aktive Zelle existiert
            if (activeCell == null)
            {
                // Falls es kein aktive Zelle gibt, können auch Zellen selektiert sein
                var anzZeilen = this.ultraGridStammDaten.Rows.Count;           // Anzahl Zeilen im Grid
                var anzSpalten = this.ultraGridStammDaten.DisplayLayout.Bands[0].Columns.Count; // Anzahl Spalten des Grids

                // Alle Zeilen und Spalten durchgehen und selektierte Zellen ermitteln
                for (var z = 0; z < anzZeilen; z++)
                {
                    for (var s = 0; s < anzSpalten; s++)
                    {
                        zelle = this.ultraGridStammDaten.Rows[z].Cells[s];      // Zelle im Grid
                        if (zelle.Selected)
                        {
                            // Die Zelle ist ausgewählt, Fontdaten einstellen
                            this.StelleFontEigenschaftEin(ref zelle, propertyToUpdate);
                        }
                    }
                }
            }
            else
            {
                // Die Zelle ist aktiv, Fontdaten einstellen
                this.StelleFontEigenschaftEin(ref activeCell, propertyToUpdate);
            }

            this.cellActivationRecursionFlag = false;                           // Zellen müssen nicht rekursiv bearbeitet werden, gilt also nur für eine Zelle
        }

        /// <summary>
        /// Methode, um verschiedene Eigenschaften der Schriftart zu aktualisieren
        /// </summary>
        /// <param name="zelle">Die zu bearbeitende Zelle.</param>
        /// <param name="propertyToUpdate">Aufzählung von Eigenschaften, welche von der Schriftart abhängig sind</param>
        /// <remarks>
        /// Die Eigenschaften werden umgeschaltet.
        /// </remarks>
        private void StelleFontEigenschaftEin(ref UltraGridCell zelle, FontProperties propertyToUpdate)
        {
            // Art der Daten auswerten
            switch (propertyToUpdate)
            {
                case FontProperties.Bold:                                       // Fettschrift
                    zelle.Appearance.FontData.Bold = DienstProgramme.ToggleDefaultableBoolean(zelle.Appearance.FontData.Bold); // Fettschrift aus- oder einschalten
                    break;

                case FontProperties.Italics:                            // Kursiv
                    activeTaskActiveCellFontData.Italic = DienstProgramme.ToggleDefaultableBoolean(activeTaskActiveCellFontData.Italic); // Kursivschrift umschalten
                    break;

                case FontProperties.Underline:                          // Unterstrichen
                    activeTaskActiveCellFontData.Underline = DienstProgramme.ToggleDefaultableBoolean(activeTaskActiveCellFontData.Underline); // unterstrichene Schrift umschalten
                    break;
            }
        }

        #endregion UpdateFontProperty

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
            this.ultraGanttView1.CalendarInfo = this.ultraCalendarInfo1;

            var anzProject = this.ultraGanttView1.CalendarInfo.Projects.Count;
            try
            {
                this.ultraGanttView1.Project = this.ultraGanttView1.CalendarInfo.Projects[1];
            }
            catch (Exception)
            {
                this.ultraGanttView1.Project = this.ultraGanttView1.CalendarInfo.Projects[anzProject - 1];
            }
        }

        #endregion BindArbInhaltData

        #region ChangeIcon

        /// <summary>
        /// Ändert das Symbol.
        /// </summary>
        private void ChangeIcon()
        {
            // Anhand des Farbschemas den Namen des zum Farbschema gehörenden Icons zusammensetzen
            var iconPath = themePaths[currentThemeIndex].Replace(@"StyleLibraries.", @"Images.AppIcon - ").Replace(@".isl", @".ico");

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
            var shouldSuspendPainting = !this.ultraToolbarsManager2.IsUpdating; // Ermitteln, ob gerade gezeichnet wird

            // Neue Farben können eingestellt werden, falls nicht gerade aufgefrischt wird
            if (shouldSuspendPainting)
            {
                this.ultraToolbarsManager2.BeginUpdate();                       // Auffrischen starten
            }

            // Bildlisten mit den neuen Bildern setzen
            var largeImageList = this.ultraToolbarsManager2.ImageListLarge;
            var smallImageList = this.ultraToolbarsManager2.ImageListSmall;

            try
            {
                // Bildlisten im UltraToolbarsManager löschen, damit weiter andere
                // Farben eingestellt werden können
                this.ultraToolbarsManager2.ImageListLarge = null;
                this.ultraToolbarsManager2.ImageListSmall = null;

                ToolBase resolveTool = null;                                    // gefundenes Tool löschen, damit neues erstellt werden kann

                // Nur bearbeiten, falls das Tool"Insert_Task" existiert
                if (this.ultraToolbarsManager2.Tools.Exists(@"Insert_Task"))
                {
                    resolveTool = this.ultraToolbarsManager2.Tools[@"Insert_Task"]; // Tool merken

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
                    this.ultraToolbarsManager2.Ribbon.Tabs[0].ResolveTabItemAppearance(ref appData, ref requestedProps);
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
                this.ultraToolbarsManager2.ImageListLarge = largeImageList;
                this.ultraToolbarsManager2.ImageListSmall = smallImageList;

                // Zeichnen im UltraToolbarsManager fortsetzen, falls es unterbrochen war
                if (shouldSuspendPainting)
                {
                    this.ultraToolbarsManager2.EndUpdate();                     // Auffrischen ist fertig
                }
            }
        }

        #endregion ColorizeImages

        #endregion Methoden
    }
}