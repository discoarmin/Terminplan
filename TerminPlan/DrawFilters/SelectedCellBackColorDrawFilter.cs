// -----------------------------------------------------------------------
// <copyright file="SelectedCellBackColorDrawFilter.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Verschiedene Hilfsfunktionen.
// </summary>
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  25.04.13  br      Grundversion
//      </para>
// </remarks>
// -----------------------------------------------------------------------
namespace Infragistics.DrawFilters
{
    using System.Drawing;

    using Infragistics.Win;
    using Infragistics.Win.UltraWinGrid;

    #region SelectedCellBackColorDrawFilter
    /// <summary>
    /// Das SelectedCellBackColorDrawFilter entfernt die ausgewählte Hintergrundfarbe
    /// zeichnet sie mit der übergebenen Hintergrudfarbe
    /// Wird keine Farbe übergeben, wird weiß genommen
    /// Dieser Filkter wirkt sich nur auf die übergebene Spalte des Grids aus
    /// </summary>
    public class SelectedCellBackColorDrawFilter : IUIElementDrawFilter
    {
        #region Implementation of IUIElementDrawFilter
        /// <summary>Die zu bearbeitense Spalte</summary>
        private readonly uint col;

        /// <summary>Die einzustellende Hintergrundfarbe</summary>
        private readonly Color backColor = Color.Empty;

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="SelectedCellBackColorDrawFilter"/> Klasse.
        /// </summary>
        /// <param name="filterCol">Spalten in welcher der Filter angewendet werden soll.</param>
        public SelectedCellBackColorDrawFilter(uint filterCol)
        {
            this.col = filterCol;                                               // zu bearbeitende Spalte merken
            this.backColor = Color.White;                                       // Falls keine Farbe übergeben wird, Weiß einstellen
        }

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="SelectedCellBackColorDrawFilter"/> Klasse.
        /// </summary>
        /// <param name="filterCol">Spalten in welcher der Filter angewendet werden soll.</param>
        /// <param name="farbe">Die einzustellende Hintergrundfarbe.</param>
        public SelectedCellBackColorDrawFilter(uint filterCol, Color farbe)
        {
            this.col = filterCol;                                               // zu bearbeitende Spalte merken
            this.backColor = Color.White;                                       // Falls keine Farbe übergeben wird, Weiß einstellen

            if (farbe != Color.Empty)
            {
                this.backColor = farbe;                                         // Hintergrundfarbe merken
            }
        }

        #region GetPhasesToFilter
        /// <summary>
        /// Wird aufgerufen, bevor jedes Element gezeichnet wird.
        /// </summary>
        /// <param name="drawParams">Die <see cref="T:Infragistics.Win.UIElementDrawParams" />-Parameter, um Zeichnungsinformationen zur Verfügung zu stellen.</param>
        /// <returns>
        /// Bitkodierte Information, welche angibt, welche Phasen des Zeichenvorgangs zu filtern sind. Die DrawElement-Methode wird nur für diese Phasen aufgerufen.
        /// </returns>
        public DrawPhase GetPhasesToFilter(ref UIElementDrawParams drawParams)
        {
            // Um das Zeichnen von Text einer Zelle zu behandeln, muss das Zeichnen des TextUIElement innerhalb der Zelle durchgeführt werden.
            // Zuerst wird überprüft, ob das zu zeichnende UIElement ein TextUIElementBase ist. Dann wird überprüft, ob der Kontext eine Zelle ist.
            // Der "Kontext" eines UIElement bezieht sich auf den logischen Teil eines Steuerelements, welcher dem UIElement zugeordnet ist.
            // Wenn der Kontext eine Zelle ist, wird die AfterDrawBackColor-Phase zurückgegeben, so dass die Hintergrundfarbe selbst durchgeführt werden kann.
            EditorWithTextUIElement slot = drawParams.Element as EditorWithTextUIElement;
            if (slot != null)
            {
                return DrawPhase.AfterDrawBackColor;                            // Phase nach dem Zeichnen der Hintergrundfarbe filtern
            }

            return DrawPhase.None;                                              // Keine Phase bearbeiten
        }
        #endregion GetPhasesToFilter

        #region DrawElement
        /// <summary>
        /// Wird während des Zeichenvorgangs eines UIElement für eine bestimmte Phase der Operation aufgerufen.
        /// Dies wird nur für diejenigen Phasen, welche von der GetPhasesToFilter-Methode zurückgegebenen werden, aufgerufen.
        /// </summary>
        /// <param name="drawPhase">Enthält ein einzelnes Bit, welches die aktuelle Zeichenphase identifiziert.</param>
        /// <param name="drawParams">Die <see cref="T:Infragistics.Win.UIElementDrawParams" />-Parameter, um Zeichnungsinformationen zur Verfügung zu stellen.</param>
        /// <returns>
        /// Wird true zurückgegeben, so bedeutet dies, dass diese Phase behandelt wurde und die Standardverarbeitung übersprungen werden sollte
        /// </returns>
        public bool DrawElement(DrawPhase drawPhase, ref UIElementDrawParams drawParams)
        {
            // Referenz auf diejenige Zelle holen, welche die TextUIElementBase enthält, auf welcher der Text gezeichnet werden soll
            UltraGridCell cell = drawParams.Element.SelectableItem as UltraGridCell;

            // Das UIElement, welches den Text enthält
            EditorWithTextUIElement slot = drawParams.Element as EditorWithTextUIElement;

            // Für den Fall, dass etwas schief gegangen ist, wird 'false' zurückgegeben.
            // Damit wird dem Grid mitgeteilt, das 'normal' weitergezeichnet werden soll.
            if (cell == null)
            {
                return false;
            }

            // falls nicht die ausgewählte Spalte gezeichnet wird, 'normal' weiterzeichnen
            if (cell.Column.Index != this.col)
            {
                return false;
            }

            // Überprüfen, ob die Hintergrundfarbe gezeichnet wurde, und ob das UIElement vorhanden ist.
            if (drawPhase == DrawPhase.AfterDrawBackColor && slot != null)
            {
                // Eigene Hintergrundfarbe kann gezeichent werden
                drawParams.AppearanceData.BackColor = this.backColor;
                drawParams.DrawBackColor(
                    ref drawParams.AppearanceData,
                    new Rectangle(
                        slot.Rect.X,
                        slot.Rect.Y,
                        slot.Rect.Width,
                        slot.Rect.Height),
                   new Rectangle(
                        slot.Rect.X,
                        slot.Rect.Y,
                        slot.Rect.Width,
                        slot.Rect.Height),
                   false);
            }

            // Die Rückgabe von 'false' bedeutet, dass das 'normale' Zeichnen erfolgen soll.
            // Nach der Festlegung der Hintergrundfarbe der Zelle wird diejenige Hintergrundfarbe, die angegeben wurde,
            // zum Zeichnen des Hintergrudes verwendet werden.
            return false;
        }
        #endregion DrawElement

        #endregion Implementation of IUIElementDrawFilter
    }
    #endregion SelectedCellForeColorDrawFilter
}