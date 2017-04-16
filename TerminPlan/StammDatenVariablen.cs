// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StammDatenVariablen.Cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Zusammenfassung für StammDaten, Variablen.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  02.04.17  br      Grundversion
//      </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------
namespace Terminplan
{
    using Infragistics.Win.AppStyling;

    /// <summary>
    /// Die Variablendeklaration.
    /// </summary>
    public partial class StammDaten
    {
        #region Aufzählungen

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

        /// <summary> Pfad zu den Farbeinstallungen </summary>
        public static StyleManager StyleManagerStammDaten;

        /// <summary> Merker für rekursive Zellaktivierung </summary>
        private bool cellActivationRecursionFlag;

        #endregion Variablen

        #region Eigenschaften

        /// <summary>Setzt das Formular für den Terminplan</summary>
        public TerminPlanForm FrmTerminPlan { private get; set; }

        /// <summary>Setzt den Projektnamen</summary>
        public string ProjektName { private get; set; }

        #endregion Eigenschaften
    }
}