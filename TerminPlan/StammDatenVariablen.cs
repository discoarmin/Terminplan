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
    using Infragistics.Win;
    using Infragistics.Win.AppStyling;

    /// <summary>
    /// Die Variablendeklaration.
    /// </summary>
    public partial class StammDaten
    {
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

        /// <summary> Pfad zu den Farbeinstallungen </summary>
        public static StyleManager StyleManagerStammDaten;

        /// <summary> Merker f�r rekursive Zellaktivierung </summary>
        private bool cellActivationRecursionFlag;

        /// <summary> Anzahl Zeilen in der Stammdatentabelle </summary>
        private int rowCount;

        /// <summary> Liste mit den auszuw�hlenden Firmen </summary>
        private ValueList vlFirmen;

        /// <summary> Liste mit den Berschnungsarten f�r die Dauer eines Vorgangs</summary>
        private ValueList vlBerechnungsArt;

        #endregion Variablen

        #region Eigenschaften

        /// <summary>Setzt das Formular f�r den Terminplan</summary>
        public TerminPlanForm FrmTerminPlan { private get; set; }

        /// <summary>Setzt den Projektnamen</summary>
        public string ProjektName { private get; set; }

        #endregion Eigenschaften
    }
}