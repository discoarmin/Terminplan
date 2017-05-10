namespace Infragistics.DrawFilters
{
    #region NoFocusRectDrawFilter

    /// <summary>
    /// Normally an element's appearance can be easily controlled by setting various properties on Appearance
    /// objects exposed by Presentation Layer Framework (PLF) based controls.  However, occasionally a requirement
    /// comes along that isn't supported by the control. This is where a draw filter is invaluable.
    ///
    /// All controls that are based on the PLF expose a very flexible, fine-grained drawing extensibility mechanism.
    /// To customize the drawing of one or more UIElements of a control you need to implement the
    /// IUIElementDrawFilter interface on an object and set the DrawFilter property of the control to that
    /// object at runtime.
    ///
    /// This simple draw filter shows how to "turn off" the focus rectangle that is normally drawn when a control,
    /// or part of a control, has the input focus. There is no property to turn off the focus rectangle on most
    /// Infragistics Win controls, but since all the controls use the Infragistics DotNet Framework, you can use this
    /// DrawFilter to remove the focus rectangle from almost any Infragistics Win control.
    /// </summary>
    public class NoFocusRectDrawFilter : Infragistics.Win.IUIElementDrawFilter
    {
        #region Implementation of IUIElementDrawFilter

        #region GetPhasesToFilter

        /// <summary>
        /// This method is passed a UIElementDrawParams structure and returns a bit-flag enumeration called DrawPhase.
        /// The passed in structure exposes a property that returns the element to be rendered as well as properties
        /// and methods to support rendering operations like Graphics, BackBrush, DrawBorders etc. The returned
        /// DrawPhase bit-flags specify which phase(s) of the drawing operation to filter for this element
        /// (the DrawElement method below will be called for each bit returned).
        /// The DrawPhase enumeration allows you to filter the drawing before or after each drawing operation of
        /// an element (e.g. theme, backcolor, image background, borders, foreground, image and/or child elements).
        /// </summary>
        public Infragistics.Win.DrawPhase GetPhasesToFilter(ref Infragistics.Win.UIElementDrawParams drawParams)
        {
            // Indicates that we want our DrawElement method called before the focus rect is drawn.
            return Infragistics.Win.DrawPhase.BeforeDrawFocus;
        }

        #endregion GetPhasesToFilter

        #region DrawElement

        /// <summary>
        /// This method is passed the same UIElementDrawParams structure as GetPhasesToFilter() and a bit flag indicating
        /// which single draw phase is being performed. The method returns a boolean. If false is returned then the default
        /// drawing for that phase will be performed. If true is returned for a 'Before' phase then the default drawing
        /// for that phase will be skipped. Note: returning true for the BeforeDrawElement phase will cause all the other
        /// phases to be skipped (even if bits for those phases were returned by the call to GetPhasesToFilter).
        /// Also, if themes are active, returning true for the BeforeDrawTheme phase will skip all phases up to but not
        /// including the BeforeDrawChildElements phase. The BeforeDrawFocus phase is only called if the control has focus
        /// (or the forceDrawAsFocused parameter was set to true on the call to the Draw method) and the element's virtual
        /// DrawsFocusRect property returns true.
        /// </summary>
        public bool DrawElement(Infragistics.Win.DrawPhase drawPhase, ref Infragistics.Win.UIElementDrawParams drawParams)
        {
            // Returning true means that the default rendering should not occur
            // (i.e. the focus rect should NOT be drawn).
            return true;
        }

        #endregion DrawElement

        #endregion Implementation of IUIElementDrawFilter
    }

    #endregion NoFocusRectDrawFilter
}