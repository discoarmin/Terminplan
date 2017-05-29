// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StammDatenVariablen.Cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Zusammenfassung f�r StammDaten, Variablen.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  �nderung
//                  --------  ------  ------------------------------------
//                  02.04.17  br      Grundversion
//      </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------
namespace Terminplan
{
    using System;
    using System.Drawing;
    using Infragistics.DrawFilters;
    using Infragistics.Win;
    using Infragistics.Win.AppStyling;
    using Sanity;

    /// <summary>
    /// Die Variablendeklaration.
    /// </summary>
    public partial class StammDaten
    {
        #region Konstanten

        public const int MaxWochen = 350;                                       // Maximale Anzahl einzugebender Wochen
        public const int MaxWochenProJahr = 53;                                 // Maximale Anzal Wochen pro Jahr

        #endregion Konstanten

        #region Aufz�hlungen

        #region Eigenschaften Schriftart

        /// <summary>
        /// Enumeration of font related properties.
        /// </summary>
        private enum FontProperties
        {
            /// <summary> Fettschrift </summary>
            Bold,

            /// <summary> Schr�gschrift </summary>
            Italics,

            /// <summary> Unterstrichen </summary>
            Underline,
        }

        #endregion Eigenschaften Schriftart

        #endregion Aufz�hlungen

        #region Variablen

        /// <summary>Enth�lt Merker, ob Arbeitsinhalt onHold enth�lt </summary>
        public static bool[] OnHold;

        /// <summary> Pfad zu den Farbeinstallungen </summary>
        public static StyleManager StyleManagerStammDaten;

        /// <summary>Startdatum des Projekts</summary>
        public static DateTime prjStartDatum;

        /// <summary>Revisionsstand des Projekts</summary>
        public static DateTime prjRevisionsStand;

        /// <summary>Gerade bearbeitete Zoomdaten</summary>
        public WinGridZoomGrid.WinGridZoomGrid.GridZoomProperty zoomGridAktuell;

        /// <summary> Merker f�r rekursive Zellaktivierung </summary>
        private bool cellActivationRecursionFlag;

        /// <summary> Anzahl Zeilen in der Stammdatentabelle </summary>
        private int rowCount;

        /// <summary> Liste mit den auszuw�hlenden Firmen </summary>
        private ValueList vlFirmen;

        /// <summary> Liste mit den Berschnungsarten f�r die Dauer eines Vorgangs</summary>
        private ValueList vlBerechnungsArt;

        /// <summary>Zoomdaten f�r Stammdaten</summary>
        private WinGridZoomGrid.WinGridZoomGrid.GridZoomProperty zoomGridStamm;

        /// <summary>Zoomdaten f�r Grunddaten</summary>
        private WinGridZoomGrid.WinGridZoomGrid.GridZoomProperty zoomGridGrund;

        /// <summary>Filter f�r eine Zelle</summary>
        private CellFilter cellFilter;

        /// <summary>Anzahl anzuzeigender Spaltenbl�cke</summary>
        private int anzSpaltenBloecke;

        /// <summary>Anzahl darzustellender Wochen</summary>
        private int anzWochen;

        /// <summary>Position der Textbox zur Aufnahme der Zellentexte</summary>
        private Point rtfLocation;
        #endregion Variablen

        #region Eigenschaften

        /// <summary>Setzt das Formular f�r den Terminplan</summary>
        public TerminPlanForm FrmTerminPlan { private get; set; }

        /// <summary>Setzt den Projektnamen</summary>
        public string ProjektName { private get; set; }

        #endregion Eigenschaften
    }
}