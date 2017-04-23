// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HauptFormVariablen.Cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Zusammenfassung für HauptForm, Variablen.
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
namespace Terminplan
{
    using System.Data;
    using System.Resources;
    using Infragistics.Win.AppStyling;
    using Resources = Properties.Resources;

    /// <summary>
    /// Die Variablendeklaration.
    /// </summary>
    public partial class TerminPlanForm
    {
        #region Aufzählungen

        #region GanttViewAktion

        /// <summary> Aufzählung der Aktionen, die durchgeführt werden können. </summary>
        private enum GanttViewAction
        {
            /// <summary> Aufgabe nach rechts einziehen </summary>
            IndentTask,

            /// <summary> Aufgabe nach links harausziehen </summary>
            OutdentTask,

            /// <summary> Termin für die Aufgabe später </summary>
            MoveTaskDateForward,

            /// <summary> Termin für die Aufgabe früher </summary>
            MoveTaskDateBackward,
        }

        #endregion GanttViewAktion

        #region Stardatum verschieben

        /// <summary> Aufzählung Zeitspannen, die ausgewählt werden können. </summary>
        private enum TimeSpanForMoving
        {
            /// <summary> Ein Tag </summary>
            OneDay,

            /// <summary> Eine Woche </summary>
            OneWeek,

            /// <summary> Einen Monat (4 Wochen) </summary>
            FourWeeks,
        }

        #endregion Stardatum verschieben

        #region Eigenschaften Schriftart

        /// <summary>
        /// Enumeration of font related properties.
        /// </summary>
        private enum FontProperties
        {
            /// <summary> Fettschrift </summary>
            Bold,

            /// <summary> Schrägschrift </summary>
            Italics,

            /// <summary> Unterstrichen </summary>
            Underline,
        }

        #endregion Eigenschaften Schriftart

        #endregion Aufzählungen

        #region Variablen

        /// <summary>Delegate zum Melden, dass der Begrüßungsbildschirm geschlossen werden kann </summary>
        public delegate void SplashScreenCloseDelegate();

        /// <summary>Delegate zum Schließen des Begrüßungsbildschrms</summary>
        public delegate void CloseDelagate();

        /// <summary>Dataset zur Aufnahme der Daten des Terminplans</summary>
        public DataSet DatasetTp;

        /// <summary> Merker für rekursive Zellaktivierung </summary>
        private bool cellActivationRecursionFlag;

        /// <summary> Merker, ob neues Projekt hinzugefügt wurde</summary>
        private bool prjHinzugefuegt;

        /// <summary> Der ResourceManager </summary>
        private ResourceManager rm = Resources.ResourceManager;

        /// <summary> Zeilenhöhe einer Arbeitsaufgabe </summary>
        private const int TaskRowHeight = 30;

        /// <summary> Höhe der Aufgabenleiste </summary>
        // ReSharper disable once UnusedMember.Local
        private const int TaskBarHeight = 20;

        /// <summary>
        /// Holt oder setzt den Pfad zu den Farbeinstellungen
        /// </summary>
        public string[] TemePaths { get; set; }

        /// <summary> Pfad zu den Farbeinstallungen </summary>
        public static StyleManager StyleManagerIntern;

        /// <summary>Name des Projekts</summary>
        public static string PrjName;

        #endregion Variablen

        #region Eigenschaften

        /// <summary>Setzt das Formular für den Terminplan</summary>
        public StammDaten FrmStammDaten { private get; set; }

        /// <summary>Setzt die zu bearbeitende Terminplan-Datei</summary>
        public string GeladeneDatei { get; private set; }

        /// <summary>
        /// Holt oder setzt den Index des momentanen Farbschemas
        /// </summary>
        public int CurrentThemeIndex { get; set; }

        /// <summary>
        /// Holt oder setzt den Pfad zu den Farbeinstellungen
        /// </summary>
        public string[] ThemePaths { get; set; }

        #endregion Eigenschaften
    }
}