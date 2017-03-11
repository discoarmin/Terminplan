using System.Drawing;

using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;

namespace Infragistics.DrawFilters
{

    #region SelectedCellForeColorDrawFilter
    /// <summary>
    /// The SelectedCellForeColorDrawFilter removes the Selected color and 
    /// draws cells with the "non-selected" ForeColor. 
    /// </summary> 
    public class SelectedCellForeColorDrawFilter : IUIElementDrawFilter
    {
        #region Implementation of IUIElementDrawFilter

        #region GetPhasesToFilter
        public DrawPhase GetPhasesToFilter(ref UIElementDrawParams drawParams)
        {
            // In order to handle the drawing of a cell's text, we need to perform the drawing of the 
            // TextUIElement inside the cell. First we check to see if the UIElement about to be drawn
            // is a TextUIElementBase. Then we check to see if it's context is a cell.  The "context"
            // of a UIElement refers to the logical part of a control that the UIElement is associated with.
            // If it's context is a cell, we return the BeforeDrawForeGround phase, so that we can draw the 
            // foreground ourself. 
            if(drawParams.Element is TextUIElementBase &&
               drawParams.Element.GetContext(typeof(UltraGridCell)) != null)
                return DrawPhase.BeforeDrawForeground;
            return DrawPhase.None;
        }
        #endregion GetPhasesToFilter

        #region DrawElement
        public bool DrawElement(DrawPhase drawPhase, ref UIElementDrawParams drawParams)
        {
            // Get a reference to the cell containing the TextUIElementBase on which the text will be drawn.
            UltraGridCell cell = drawParams.Element.SelectableItem as UltraGridCell;

            // Just in case something goes wrong, returning false tells the grid
            // that the normal rendering should occur.
            if(cell == null)
            {
                return false;
            }

            // Check to see if the cell is drawing while selected. 
            // There are three possible ways this can occur. 
            if(cell.Selected || cell.Row.Selected || cell.Column.Header.Selected)
            {
                Color textColor = Color.Empty;

                if(cell.HasAppearance)
                {
                    // Check to see if the cell has an appearance.
                    // If so, use the cell's appearance to draw the text.
                    textColor = cell.Appearance.ForeColor;
                }
                if(textColor.IsEmpty && cell.Row.HasCellAppearance)
                {
                    // If the cell does not have an appearance or if it's ForeColor is Empty,
                    // check to see if the row has a CellAppearance.
                    // If so, use the row's CellAppearance to draw the text
                    textColor = cell.Row.CellAppearance.ForeColor;
                }
                if(textColor.IsEmpty && cell.Column.HasCellAppearance)
                {
                    // If the cell and row do not have appearances or if their ForeColor values are Empty,
                    // check to see if the column has a CellAppearance.
                    // If so, use the column's CellAppearance to draw the text. 
                    textColor = cell.Column.CellAppearance.ForeColor;
                }

                // If we were able to find a suitable ForeColor, assign that color to the AppearanceData
                // that will be used to draw the text.
                if(! textColor.IsEmpty)
                {
                    drawParams.AppearanceData.ForeColor = textColor;
                }
            }

            // Returning false indicates that the normal rendering should occur.
            // If we successfully set the ForeColor, the ForeColor we specified will be used
            // to draw the text.
            return false;
        }
        #endregion DrawElement

        #endregion Implementation of IUIElementDrawFilter
    }
    #endregion SelectedCellForeColorDrawFilter
}