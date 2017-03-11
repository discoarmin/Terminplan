using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
//

namespace Infragistics.DrawFilters
{
    #region SortIndicatorDrawFilter

    /// <summary>
    /// Allows you to customize the sort indicators used by the Infragistics WinGrid.
    /// You pass the paths of the images to be used to the constructor; one path for the "ascending" image 
    /// and another path for the "descending" image.
    /// </summary>
    public class SortIndicatorDrawFilter : IUIElementDrawFilter
    {
        #region Data

        protected readonly string ascendingImage;
        protected readonly string descendingImage;

        #endregion Data

        #region Constructor

        /// <summary>
        /// Creates a DrawFilter which allows you to specify any two images to be displayed for the WinGrid's sort indicators. 
        /// </summary>
        /// <param name="ascendingImage">The path of the image file to use for the "ascending" sort indicator.</param>
        /// <param name="descendingImage">The path of the image file to use for the "descending" sort indicator.</param>
        public SortIndicatorDrawFilter(string ascendingImage, string descendingImage)
        {
            // Perform some safety checks.
            if (!File.Exists(ascendingImage))
                throw new ArgumentException("The \"ascendingImage\" argument (" + ascendingImage + ") is not an existing file.");

            if (!File.Exists(descendingImage))
                throw new ArgumentException("The \"descendingImage\" argument (" + descendingImage + ") is not an existing file.");

            // Initialize the member data.
            this.ascendingImage = ascendingImage;
            this.descendingImage = descendingImage;
        }

        #endregion Constructor

        #region IUIElementDrawFilter Members

        #region GetPhasesToFilter

        DrawPhase IUIElementDrawFilter.GetPhasesToFilter(ref UIElementDrawParams drawParams)
        {
            // If a sort indicator element is about to be rendered, then we want our filter to be used
            // just before it is actually rendered so that we can provide our own custom rendering.
            return drawParams.Element is SortIndicatorUIElement ? DrawPhase.BeforeDrawElement : DrawPhase.None;
        }

        #endregion GetPhasesToFilter

        #region DrawElement

        // To return FALSE from this method indicates that the element should draw itself as normal.
        // To return TRUE  from this method indicates that the element should not draw itself. 
        bool IUIElementDrawFilter.DrawElement(DrawPhase drawPhase, ref UIElementDrawParams drawParams)
        {
            // Get the column header to which the sort indicator belongs.
            ColumnHeader header = drawParams.Element.GetContext(typeof(ColumnHeader)) as ColumnHeader;

            // Just to be safe...
            if (header == null || header.Column == null)
                return false;

            // Find out if the column associated with the column header is being sorted ascending or descending.  
            bool sortAscending = (header.Column.SortIndicator == SortIndicator.Ascending);

            #region Comments...
            // Get the rectangle in which the sort indicator exists and decrease the height a little
            // so that the bottom of the image does not interfere with the thin line that a header 
            // draws when the mouse cursor is over it. Increase the width of the rectangle so that the images
            // are drawn more smoothly.  Also move the rectangle 2 pixels to the left so that the splitter bar 
            // on the edge of the column header does not overlap the image.  
            #endregion Comments...
            Rectangle rect = drawParams.Element.Rect;
            rect.Inflate(+1, -1);
            rect.Offset(-2, 0);

            // Setup an ImageAttributes and ColorMap so that the blank white area
            // surrounding the sort indicator image is painted transparently.
            ImageAttributes imageAttr = new ImageAttributes();
            ColorMap[] colorMap = new ColorMap[1];
            colorMap[0] = new ColorMap();
            colorMap[0].OldColor = Color.White;
            colorMap[0].NewColor = Color.Transparent;
            imageAttr.SetRemapTable(colorMap);

            try
            {
                // Get the bitmap to use and then render it.
                using (Bitmap bmp = new Bitmap(sortAscending ? ascendingImage : descendingImage))
                {
                    drawParams.Graphics.DrawImage(
                        bmp,
                        rect,
                        0,
                        0,
                        bmp.Width,
                        bmp.Height,
                        GraphicsUnit.Pixel,
                        imageAttr
                        );
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(
                    false,
                    "An exception occurred while trying to render a custom sort indicator: " + ex.Message,
                    "Was one of the images used for the custom sort indicators deleted or renamed?"
                    );

                // If something went wrong (ex. the image file was deleted) then let the default rendering occur.
                return false;
            }

            // If we get to this point then our custom sort indicator was successfully rendered.
            // Returning true prevents the normal sort indicator from being rendered on top of what we just drew.
            return true;
        }

        #endregion DrawElement

        #endregion IUIElementDrawFilter Members
    }

    #endregion SortIndicatorDrawFilter
}