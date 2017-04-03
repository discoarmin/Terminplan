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
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Threading;
    using System.Windows.Forms;
    using Infragistics.Win;
    using Infragistics.Win.UltraMessageBox;
    using Infragistics.Win.UltraWinSchedule;
    using Infragistics.Win.UltraWinToolbars;
    using Resources = Properties.Resources;

    /// <summary>
    /// Klasse TerminPlanForm (Hauptformular).
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class TerminPlanForm
    {
        #region Methoden

        #region InitializeUi

        /// <summary> Initialisiert die Oberfl�che. </summary>
        private void InitializeUi()
        {
            var culture = CultureInfo.InstalledUICulture;                       // Sprache des Betriebssystems ermitteln
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            var col = ultraGanttView1.GridSettings.ColumnSettings.Values;

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
            var themeTool = (ListTool)ultraToolbarsManager1.Tools[@"ThemeList"];

            // Alle vorhandenen Farbschematas durchgehen
            foreach (var resourceName in themePaths)
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
            ((ListTool)ultraToolbarsManager1.Tools[@"TouchMode"]).SelectedItemIndex = 0; // Erstes Element als Auswahl

            // Erstellt eine Liste mit verschiedenen Schriftgr��en
            PopulateFontSizeValueList();                                   // Fontliste f�llen
            ((ComboBoxTool)(ultraToolbarsManager1.Tools[@"FontSize"])).SelectedIndex = 0;
            ((FontListTool)ultraToolbarsManager1.Tools[@"FontList"]).SelectedIndex = 0;
            OnUpdateFontToolsState(false);                                 // Font ist nicht ausw�hlbar

            // Aboutbox initialisieren
            Control control = new AboutControl()
            {
                Visible = false,                                                // Aboutbox ist nicht sichtbar
                Parent = this                                                   // Das Hauptformular ist das Elternformular
            };                                                                  // Neue Instanz der Aboutbox erzeugen

            ((PopupControlContainerTool)ultraToolbarsManager1.Tools[@"About"]).Control = control; // Aboutbox in die Tools f�r den UltraToolbarsManager setzen

            // Gr��e der Spalten so einstellen, dass alle Daten sichtbar sind.
            ultraGanttView1.PerformAutoSizeAllGridColumns();

            // Die Bilder entsprechend dem aktuellen Farbschema einf�rben.
            ColorizeImages();
            ultraToolbarsManager1.Ribbon.FileMenuButtonCaption = Resources.ribbonFileTabCaption; // Beschriftung des Datei-Men�s-Button eintragen
        }

        #endregion InitializeUi

        #region CreateNewTasks

        /// <summary>Erzeugt die erforderlichen Vorg�nge zum Erstellen eines neuen Terminplans</summary>
        /// <param name="neuesProjekt">Referenz auf das neu erzeugte Projekt.</param>
        /// <param name="startDatum">das Startdatum des Projekts.</param>
        /// <param name="prjKey">Schl�ssel des Projekts, entspricht der Kommissionsnummer.</param>
        /// <param name="aufgaben">Liste mit allen anzulegenden Aufgaben.</param>
        /// <param name="arbInhalt">der anzulegende Arbeitsinhalt.</param>
        private void ErzeugeNeueTasks(ref Project neuesProjekt,
            DateTime startDatum,
            string prjKey,
            IReadOnlyList<string> aufgaben,
            string arbInhalt)
        {
            // Den Arbeitsinhalt generieren. Es wird eine Dauer von f�nf Tagen angenommen. Kann nachtr�glich ge�ndert werden
            var arbInhaltTask = this.ultraCalendarInfo1.Tasks.Add(startDatum, TimeSpan.FromDays(5), @"Arbeitsinhalt_Aufgaben", prjKey);
            arbInhaltTask.Name = arbInhalt;                                     // Bezeichnung des Arbeitsinhalts

            var anzAufgaben = aufgaben.Count;                                   // Ermitteln, wie viele Aufgaben angelegt werden m�sse

            // Alle vorhandenen Aufgaben eintragen
            for (var a = 0; a < anzAufgaben; a++)
            {
                // Aufgabe f�r den Arbeitsinhalt generieren. Es wird eine Dauer von zwei Tagen angenommen. Kann nachtr�glich ge�ndert werden
                var aufabeTask = arbInhaltTask.Tasks.Add(DateTime.Today, TimeSpan.FromDays(2), aufgaben[a]); // Eintrag f�r eine Aufgabe

                // Einschr�nkung f�r diese Aufgabe erstellen
                aufabeTask.Constraint = TaskConstraint.AsSoonAsPossible;        // So bald wie m�glich
            }
        }

        /// <summary>Erzeugt die Besitzer-Eintr�ge f�r den neuen Terminplan</summary>
        /// <param name="besitzer">Liste mit allenPersonen, welche die Aufgaben bearbeiten k�nnen.</param>
        private void ErzeugeBesitzer(IReadOnlyList<Besitzer> besitzer, ref DataSet neuesDataSet)
        {
            //            const string AlleEigenschaften = "AAEAAAD/////AQAAAAAAAAAMAgAAAG9JbmZyYWdpc3RpY3M0Lldpbi5VbHRyYVdpblNjaGVkdWxlLnYxNC4xLCBWZXJzaW9uPTE0LjEuMC45MDAwLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPTdkZDVjMzE2M2YyY2QwY2IFAQAAACdJbmZyYWdpc3RpY3MuV2luLlVsdHJhV2luU2NoZWR1bGUuT3duZXIBAAAAA0tleQECAAAABgMAAAALUGVyc29uIE5hbWUL";

            var anzBesitzer = besitzer.Count;                                   // Anzahl Personen, welche die Aufgaben bearbeiten k�nnen
            this.ultraCalendarInfo1.Owners.Clear();                             // Erst mal alle Besitzer l�schen
            var dieBesitzer = neuesDataSet.Tables.Add(@"Besitzer");             // Tabelle f�r die Besitzer erzeugen

            // Spalten f�r die Tabelle 'Besitzer' definieren
            dieBesitzer.Columns.Add(@"Key", typeof(String));                    // Voller Name des Besitzers
            dieBesitzer.Columns.Add(@"Name", typeof(String));                   // Spalte f�r das Namensk�rzel
            dieBesitzer.Columns.Add(@"EmailAddresse", typeof(String));          // Email-Addresse des Besitzers
            dieBesitzer.Columns.Add(@"Sichtbar", typeof(Boolean));              // Besitzers sichtbar
            dieBesitzer.Columns.Add(@"AlleEigenschaften", typeof(Byte[]));      // Bin�re nicht angebundene Daten

            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();    // Zum Eintragen des ByteArrays

            // Alle Personen eintragen, welche die Aufgaben bearbeiten d�rfen
            for (var owner = 0; owner < anzBesitzer; owner++)
            {
                var besitzerName = besitzer[owner].Key;                         // Name des Besitzers ermitteln
                var kuerzel = besitzer[owner].Name;                             // Kurzbezeichnung
                var sichtbar = besitzer[owner].Sichtbar;                        // Ist Besitzer sichtbar
                var email = besitzer[owner].EmailAddresse;                      // E-Mail Addresse des Besitzers

                // Besitzer-Tabelle erstellen, wenn sie noch nicht existiert und zugeh�rige Werte eintragen
                // Die �bergebenen Besitzer hinzuf�gen
                dieBesitzer.Rows.Add(
                    besitzerName,
                    besitzerName + @"(" + kuerzel + @")",
                    email,
                    sichtbar,
                    null);                                                      // Zeile hinzuf�gen

                // Da alle Besitzer gel�scht wurden, die Liste wieder hinzuf�gen. Wird so
                // emacht, falls sich bei den Besitzern etwas ge�ndert hat
                try
                {
                    var aufgabeOwner = this.ultraCalendarInfo1.Owners.Add(besitzerName, besitzerName + @" (" + kuerzel + @")");
                    aufgabeOwner.Visible = sichtbar;
                    aufgabeOwner.EmailAddress = email;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        /// <summary>Erstellt ein Dataset mit den f�r einen neuen Terminplan ben�titen Daten</summary>
        /// <param name="prjKey">Schl�ssel des Projekts, entspricht der Kommissionsnummer.</param>
        /// <param name="prjStart">das Startdatum des Projekts.</param>
        /// <param name="prjName">Name des Projekts.</param>
        /// <param name="aufgaben">Liste mit allen anzulegenden Aufgaben.</param>
        /// <param name="arbInhalt">der anzulegende Arbeitsinhalt.</param>
        /// <returns></returns>
        private DataSet GetNewProjectData(
            string prjKey,
            DateTime prjStart,
            string prjName,
            IReadOnlyList<string> aufgaben,
            string arbInhalt)
        {
            var neuesDataSet = new DataSet();                                   // DataSet erzeugen
            neuesDataSet.Locale = new CultureInfo(@"de-DE");

            var dieProjekte = neuesDataSet.Tables.Add(@"Projekte");             // Tabelle f�r die Projekte erzeugen

            // Spalten f�r die Tabelle 'Projekte' definieren
            dieProjekte.Columns.Add(@"ProjektID", typeof(String));
            dieProjekte.Columns.Add(@"ProjektKey", typeof(String));
            dieProjekte.Columns.Add(@"ProjektName", typeof(String));
            dieProjekte.Columns.Add(@"ProjektStart", typeof(DateTime));

            // Die eingegebenen Daten in das DataSet eintragen
            dieProjekte.Rows.Add(new Object[] { DienstProgramme.GetGuId(), prjKey, prjName, prjStart });

            var theTasks = neuesDataSet.Tables.Add(@"Arbeitsinhalt_Aufgaben");  // Tabelle f�r die Arbeitsinhalte und Aufgaben erzeugen

            // Spalten f�r die Tabelle 'Arbeitsinhalt_Aufgaben' definieren
            theTasks.Columns.Add(@"TaskID", typeof(String));
            theTasks.Columns.Add(@"ProjektKey", typeof(String));
            theTasks.Columns.Add(@"TaskName", typeof(String));
            theTasks.Columns.Add(@"TaskStartTime", typeof(DateTime));
            theTasks.Columns.Add(@"TaskDauer", typeof(TimeSpan));
            theTasks.Columns.Add(@"ParentTaskID", typeof(String));
            theTasks.Columns.Add(@"Einschraenkung", typeof(object));
            theTasks.Columns.Add(@"TaskFertigInProzent", typeof(String));

            // Die Task-Eigenschaften sind alle von einzelnen Mitgliedern abgedeckt. Aber wir k�nnten Platz in der Datenbank
            // durch die Speicherung von Daten als Bin�r sparen, indem die betroffenen Felder nicht angebunden werden, sondern
            // in der Eigenschft 'AllProperties' eingetragen werden.
            theTasks.Columns.Add(@"AlleEigenschaften", typeof(Byte[]));
            var arbInhaltTaskid = DienstProgramme.GetGuId();                     // Neue GUID erzeugen

            // Bei einem neuen Terminplan gibt es einen Arbeitsinhalt. Die zugeh�rigen Aufgaben werden einer Liste entnommen
            // a) Arbeitsinhalt erzeugen. Hier wird eine Dauer von 5 Tagen vorgegeben. Kann im Terminplan ge�ndert werden.
            theTasks.Rows.Add(new Object[]
            {
                arbInhaltTaskid,                                                // GUID
                prjKey,                                                         // Schl�ssel des Projekts, entspricht der Kommissionsnummer
                arbInhalt,                                                      // Name des Arbeitsinhalts
                prjStart,                                                       // Startzeitpunkt des Arbeitsinhalts ist der Projektstart
                TimeSpan.FromDays(5),                                           // Die Dauer betr�gt 5 Tage
                null,                                                           // Es gibt keine Eltern
                TaskConstraint.AsSoonAsPossible,                                // Start soll so bald wie m�glich erfolgen
                null                                                            // Fertiggestellt ist noch nichts
            });

            var anzAufgaben = aufgaben.Count;                                   // Ermitteln, wie viele Aufgaben angelegt werden m�sse

            // TODO: Falls es Abh�ngigkeiten gibt, diese eintragen
            // Alle vorhandenen Aufgaben eintragen
            for (var a = 0; a < anzAufgaben; a++)
            {
                // Aufgabe f�r den Arbeitsinhalt generieren. Es wird eine Dauer von zwei Tagen angenommen. Kann nachtr�glich ge�ndert werden
                theTasks.Rows.Add(new Object[]
                {
                    DienstProgramme.GetGuId(),                                  // GUID
                    prjKey,                                                     // Schl�ssel des Projekts, entspricht der Kommissionsnummer
                    aufgaben[a],                                                // Bezeichnung der Aufgabe
                    prjStart,                                                   // Startzeitpunkt der Aufgabe ist der Projektstart
                    TimeSpan.FromDays(2),                                       // Die Dauer betr�gt 2 Tage
                    arbInhaltTaskid,                                            // ID des zugeh�rigen Arbeitsinhalts
                    TaskConstraint.StartNoEarlierThan,                          // nicht vorher starten
                    25                                                          // 25% fertig (damit was angzeigt wird)
                });
            }

            return neuesDataSet;
        }

        /// <summary>Erstellt die Datenbindungen f�r das neue Projekt.</summary>
        /// <param name="ds">das zugeh�rige DataSet.</param>
        private void ErstelleDatenBindungen(DataSet ds)
        {
            //  Legt die BindingContextControl-Eigenschaft fest, um auf dieses Formular zu verweisen
            this.ultraCalendarInfo1.DataBindingsForTasks.BindingContextControl = this;
            this.ultraCalendarInfo1.DataBindingsForProjects.BindingContextControl = this;

            //  Setzt die DataBinding-Mitglieder f�r Projekte fest
            this.ultraCalendarInfo1.DataBindingsForProjects.SetDataBinding(ds, @"Projekte");
            this.ultraCalendarInfo1.DataBindingsForProjects.IdMember = @"ProjektID";
            this.ultraCalendarInfo1.DataBindingsForProjects.KeyMember = @"ProjektKey";
            this.ultraCalendarInfo1.DataBindingsForProjects.NameMember = @"ProjektName";
            this.ultraCalendarInfo1.DataBindingsForProjects.StartDateMember = @"ProjektStart";

            //  Setzt die DataBinding-Mitglieder f�r die Arbeitsinhalte und Aufgaben fest
            this.ultraCalendarInfo1.DataBindingsForTasks.SetDataBinding(ds, @"Arbeitsinhalt_Aufgaben");

            // Grundlegende Aufgabeneigenschaften
            this.ultraCalendarInfo1.DataBindingsForTasks.NameMember = @"TaskName";
            this.ultraCalendarInfo1.DataBindingsForTasks.DurationMember = @"TaskDauer";
            this.ultraCalendarInfo1.DataBindingsForTasks.StartDateTimeMember = @"TaskStartTime";
            this.ultraCalendarInfo1.DataBindingsForTasks.IdMember = @"TaskID";
            this.ultraCalendarInfo1.DataBindingsForTasks.ProjectKeyMember = "ProjektKey";
            this.ultraCalendarInfo1.DataBindingsForTasks.ParentTaskIdMember = @"ParentTaskID";

            this.ultraCalendarInfo1.DataBindingsForTasks.ConstraintMember = @"Einschraenkung";
            this.ultraCalendarInfo1.DataBindingsForTasks.PercentCompleteMember = @"TaskFertigInProzent";

            // Alle anderen Eigeenschaften
            this.ultraCalendarInfo1.DataBindingsForTasks.AllPropertiesMember = @"AlleEigenschaften";

            // Da eine Aufgabe angezeigt werden soll, welche zu einer explizit definierten Projekt geh�rt
            // (d.h. nicht das UnassignedProject), muss das Projekt zu der Projekt-Eigenschaft des
            // UltraGanttView-Steuerelements zugeordnet werden, damit das Steuerelemnt dieses Projekt anzeigen kann.
            this.ultraGanttView1.CalendarInfo = this.ultraCalendarInfo1;
            this.ultraGanttView1.Project = this.ultraGanttView1.CalendarInfo.Projects[1];
        }

        #endregion CreateNewTasks

        #region DeleteTask

        /// <summary> L�scht den aktiven Arbeitsinhalt oder die aktive Aufgabe </summary>
        private void DeleteTask()
        {
            var activeTask = ultraGanttView1.ActiveTask;                   // Aktiven Arbeitsinhalt oder Aufgabe ermitteln
            try
            {
                // Nur bearbeiten, falls ein Arbeitsinhalt oder eine Aufgabe aktiv ist
                if (activeTask != null)
                {
                    var parent = activeTask.Parent;                             // Arbeitsinhalt ermitteln

                    if (parent == null)
                    {
                        // Es handelt sich um eine Aufgabe. Diese l�schen
                        ultraCalendarInfo1.Tasks.Remove(activeTask);
                    }
                    else
                    {
                        // Arbeitsinhalt l�schen
                        parent.Tasks.Remove(activeTask);
                    }
                }

                // Status aktualisieren
                var newActiveTask = ultraGanttView1.ActiveTask;
                UpdateTasksToolsState(newActiveTask);
                UpdateToolsRequiringActiveTask(newActiveTask != null);
            }
            catch (TaskException ex)
            {
                UltraMessageBoxManager.Show(ex.Message, rm.GetString("MessageBox_Error"));
            }
        }

        #endregion DeleteTask

        #region MoveTask

        /// <summary>
        /// Verschiebt Start- und Enddatum der Aufgabe r�ckw�rts oder vorw�rts um eine bestimmte Zeitspanne
        /// </summary>
        /// <param name="action">Aufz�hlung der unterst�tzten ganttView-Aktionen</param>
        /// <param name="moveTimeSpan">Zeitspanne zum Verschieben des Start- und Enddatums der Aufgabe</param>
        private void MoveTask(GanttViewAction action, TimeSpanForMoving moveTimeSpan)
        {
            var activeTask = ultraGanttView1.ActiveTask;                   // Aktive Aufgabe oder aktiven Arbeitsinhalt ermitteln

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

        #endregion MoveTask

        #region PerformIndentOrOutdent

        /// <summary>
        /// F�hrt Einr�ckung oder Auslagerung der aktiven Aufgabe oder des aktiven Atrbeitsinhalts durch
        /// </summary>
        /// <param name="action">die auszuf�hrende Aktion(Einr�ckung oder Auslagerung)</param>
        private void PerformIndentOrOutdent(GanttViewAction action)
        {
            var activeTask = ultraGanttView1.ActiveTask;                   // Aktive Aufgabe oder aktiven Arbeitsinhalt ermitteln

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
                UltraMessageBoxManager.Show(ex.Message, rm.GetString("MessageBox_Error"));
            }
        }

        #endregion PerformIndentOrOutdent

        #region PopulateFontSizeValueList

        /// <summary> Liste mit den Schriftgr��en erstellen </summary>
        private void PopulateFontSizeValueList()
        {
            // Schriftgr��en f�r die Liste vorgeben und neue Liste erstellen
            var fontSizeList = new List<float>(new float[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 });

            // Jeden Eintrag der Liste in das Tool f�r die Schriftgr��e des UltraToolbarsManager eintragen
            foreach (var i in fontSizeList)
            {
                ((ComboBoxTool)(ultraToolbarsManager1.Tools[@"FontSize"])).ValueList.ValueListItems.Add(i);
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
            var activeTask = ultraGanttView1.ActiveTask;                   // Aktive Aufgabe oder aktiven Arbeitsinhalt ermitteln
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
                UltraMessageBoxManager.Show(ex.Message, rm.GetString("MessageBox_Error"));
            }
        }

        #endregion SetTaskPercentage

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
            var fontColor = ((PopupColorPickerTool)ultraToolbarsManager1.Tools[@"Font_ForeColor"]).SelectedColor;
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

        /// <summary> Aktualisiert den Namen der Schriftart je nach dem im FontListTool ausgew�hlten Wert. </summary>
        private void UpdateFontName()
        {
            // Namen der ausgew�hlte Schriftart aus der Fontliste ermitteln
            var fontName = ((FontListTool)ultraToolbarsManager1.Tools[@"FontList"]).Text;
            var activeTask = ultraGanttView1.ActiveTask;                  // Aktive Aufgabe oder aktiven Arbeitsinhalt ermitteln

            // Nur bearbeiten, falls ein aktiver Arbeitsinhalt oder eine aktive Ausgabe existiert
            if (activeTask == null)
            {
                return;
            }

            var activeField = ultraGanttView1.ActiveField;                 // Aktive Zelle ermitteln

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
            var item = (ValueListItem)((ComboBoxTool)(ultraToolbarsManager1.Tools[@"FontSize"])).SelectedItem;

            // Nur bearbeiten, falls ein Wert vorhanden ist
            if (item == null)
            {
                return;
            }

            var fontSize = (float)item.DataValue;                               // Schriftgr��e
            var activeTask = ultraGanttView1.ActiveTask;                   // Aktive Aufgabe oder aktiven Arbeitsinhalt ermitteln

            // Nur bearbeiten, falls ein aktiver Arbeitsinhalt oder eine aktive Ausgabe existiert
            if (activeTask == null)
            {
                return;
            }

            var activeField = ultraGanttView1.ActiveField;                 // Aktive Zelle ermitteln

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
            var group = ultraToolbarsManager1.Ribbon.Tabs[@"Ribbon_Task"].Groups[@"RibbonGrp_Tasks"];

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
            ultraToolbarsManager1.Tools[@"Tasks_Delete"].SharedProps.Enabled = enabled;                     // L�schen freigeben oder sperren
            ultraToolbarsManager1.Tools[@"Insert_Milestone"].SharedProps.Enabled = enabled;                 // Meilensteine freigeben oder sperren
            ultraToolbarsManager1.Tools[@"Properties_TaskInformation"].SharedProps.Enabled = enabled;       // Anzeige der Informationen zum aktuellen Arbeitsinhalt oder der aktuellen Aufgabe freigeben oder sperren
            ultraToolbarsManager1.Tools[@"Properties_Notes"].SharedProps.Enabled = enabled;                 // Anzeige der Beschreibung freigeben oder sperren
            ultraToolbarsManager1.Tools[@"Insert_Task_TaskAtSelectedRow"].SharedProps.Enabled = enabled;    // Einf�gen bei ausgew�hlter Zeile freigeben oder sperren
        }

        #endregion UpdateToolsRequiringActiveTask

        #endregion Methoden
    }
}