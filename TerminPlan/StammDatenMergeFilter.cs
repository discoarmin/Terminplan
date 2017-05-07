// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StammDatenMergeFilter.Cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Verbindet die Zellen zweier benachbarter Spalten in einer Zeile.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  24.04.17  br      Grundversion
//      </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------
namespace Terminplan
{
    using System.Collections.Generic;
    using Infragistics.Win;
    using Infragistics.Win.UltraWinGrid;
    using System.Drawing;

    /// <summary>
    /// Filter zum zusammenführen von Zellen.
    /// </summary>
    internal class MyCreation : IUIElementCreationFilter
    {
        #region IUIElementCreationFilter Members

        public void AfterCreateChildElements(UIElement parent)
        {
            var row = parent as RowCellAreaUIElement;
            if (row != null && row.HasChildElements)
            {
                var remcell = new List<CellUIElement>();
                var cell = (CellUIElement)row.ChildElements[0];

                for (int i = 1; i < row.ChildElements.Count; i++)
                {
                    if (!(row.ChildElements[i] is CellUIElement)) continue;

                    var nextCell = (CellUIElement)row.ChildElements[i];
                    if (cell.Cell.Value.ToString() == nextCell.Cell.Value.ToString())
                    {
                        var s = cell.Rect.Size;
                        s.Width += nextCell.Rect.Width;
                        cell.Rect = new Rectangle(cell.Rect.Location, s);
                        nextCell.Rect = new Rectangle(0, 0, 0, 0);
                        remcell.Add(nextCell);
                    }
                    else
                    {
                        cell = nextCell;
                    }
                }

                foreach (CellUIElement rc in remcell)
                {
                    row.ChildElements.Remove(rc);
                }
            }
        }

        public bool BeforeCreateChildElements(UIElement parent)
        {
            return false;//throw new NotImplementedException();
        }

        #endregion IUIElementCreationFilter Members
    }
}