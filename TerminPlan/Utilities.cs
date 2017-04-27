// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utilities.cs" company="EST GmbH + CO.KG">
//   Copyright (c) EST GmbH + CO.KG. All rights reserved.
// </copyright>
// <summary>
//   Dienstprogramme für Terminplan.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// <remarks>
//     <para>Autor: Armin Brenner</para>
//     <para>
//        History : Datum     bearb.  Änderung
//                  --------  ------  ------------------------------------
//                  06.01.17  br      Grundversion
// </para>
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace Terminplan
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using Infragistics.Win;
    using Infragistics.Win.UltraWinToolbars;
    using Resources = Properties.Resources;

    /// <summary>
    /// Utility class to perform context-independent functionality
    /// </summary>
    internal class DienstProgramme
    {
        #region Static ExecutingAssembly

        private static Assembly executingAssembly;

        /// <summary>Holt das ausführbare Assembly</summary>
        /// <value>das ausführbare Assembly.</value>
        private static Assembly ExecutingAssembly
        {
            get
            {
                return executingAssembly ?? (executingAssembly = Assembly.GetExecutingAssembly());
            }
        }

        #endregion Static ExecutingAssembly

        #region SetRibbonGroupToolsEnabledState

        /// <summary>
        /// Sets the Enabled property of the tools within the specified RibbonGroup.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="enabledState">if set to <c>true</c> [enabled state].</param>
        internal static void SetRibbonGroupToolsEnabledState(RibbonGroup group, bool enabledState)
        {
            if (group == null)
                return;

            foreach (var tool in group.Tools)
            {
                tool.SharedProps.Enabled = enabledState;
            }
        }

        #endregion SetRibbonGroupToolsEnabledState

        #region ColorizeImages

        /// <summary>
        /// Creates new images using a pixel-based color replacement on the image from the provided imagelist.
        /// </summary>
        /// <param name="oldColor">The old Color.</param>
        /// <param name="colors">The dictionary containing the new resolved colors</param>
        /// <param name="defaultImageList">The default ImageList.</param>
        /// <param name="colorizedImageList">The colorized ImageList.</param>
        internal static void ColorizeImages(Color oldColor, Dictionary<string, Color> colors, ref ImageList defaultImageList, ref ImageList colorizedImageList)
        {
            // loop through the default imageliss, colorize the image and put it into the colorized imagelist
            foreach (var key in defaultImageList.Images.Keys)
            {
                var bitmap = defaultImageList.Images[key] as Bitmap;
                if (bitmap == null)
                {
                    continue;                                                   // Weitersuchen, da kein Bild gefunden
                }

                bitmap = bitmap.Clone() as Bitmap;
                foreach (var colorKey in colors.Keys.Where(colorKey => key.EndsWith(colorKey)))
                {
                    DrawUtility.ReplaceColor(ref bitmap, oldColor, colors[colorKey]);
                    break;
                }

                // Falls das Bild schon in der geänderten Liste vorhanden ist, dieses aus der Liste löschen
                var index = colorizedImageList.Images.IndexOfKey(key);
                if (index >= 0)
                {
                    var oldImage = colorizedImageList.Images[index];            // Bild ermitteln
                    colorizedImageList.Images.RemoveByKey(key);                 // Bild aus Liste löschen
                    oldImage.Dispose();                                         // Ressourcen für dieses Bild löschen
                }

                // Falls Bild vorhanden ist, dieses in die Liste der geänderten Bilder eintragen
                if (bitmap != null)
                {
                    colorizedImageList.Images.Add(key, bitmap);
                }
            }
        }

        #endregion ColorizeImages

        #region GetAssemblyAttribute

        /// <summary>
        /// Retrieves the string representation of the request AssemblyAttribute from the project's AssemblyInfo.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        internal static string GetAssemblyAttribute<T>(Func<T, string> value)
            where T : Attribute
        {
            T attribute = (T)Attribute.GetCustomAttribute(ExecutingAssembly, typeof(T));
            return value.Invoke(attribute);
        }

        #endregion GetAssemblyAttribute

        #region GetData

        /// <summary> R bindenden Daten abuft die zu </summary>
        /// <returns>die gelesenen Daten als DataSet</returns>
        internal static DataSet GetData(string fileName)
        {
            // Ermitteln, ob Datei existiert. Wenn nicht, wird der DateiÖffnen-Dialog angezeigt
            if (!File.Exists(fileName))
            {
                // Dialog zum Öffnen einer Datei anzeigen
                var dateiName = OeffneXmlDatei();
                if (dateiName == string.Empty) return null;                     // Abbruch, da keine Datei ausgewählt wurde
                fileName = dateiName;
            }

            // Konvertiert den Stream in ein DataSet
            var data = new DataSet();
            data.ReadXml(fileName);
            return data;
        }

        /// <summary>Dialog zum Öffnen einer XML-Datei</summary>
        /// <returns>die ausgewählte Datei, bei Abbruch Leerstring</returns>
        public static string OeffneXmlDatei()
        {
            // Dialog zum Öffnen einer Datei anzeigen
            var openFileDialog1 = new OpenFileDialog
            {
                Filter = @"XML Dateien|*.xml",
                Title = @"Terminplan öffnen",
            };

            return openFileDialog1.ShowDialog() != DialogResult.OK ? string.Empty : openFileDialog1.FileName;
        }

        #endregion GetData

        internal static void PutData(string fileName, DataSet data)
        {
            data.WriteXml(fileName);
        }

        #region GetEmbeddedResourceStream

        /// <summary>
        /// Gets the embedded resource stream.
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        /// <returns></returns>
        internal static Stream GetEmbeddedResourceStream(string resourceName)
        {
            var stream = ExecutingAssembly.GetManifestResourceStream(resourceName);
            Debug.Assert(stream != null, Resources.DienstProgramme_GetEmbeddedResourceStream_Unable_to_locate_embedded_resource_, Resources.DienstProgramme_GetEmbeddedResourceStream_Resource_name___0_, resourceName);
            return stream;
        }

        #endregion GetEmbeddedResourceStream

        #region GetLocalizedString

        /// <summary>
        /// Localizes a string using the ResourceManager.
        /// </summary>
        /// <param name="currentString"></param>
        /// <returns></returns>
        internal static string GetLocalizedString(string currentString)
        {
            var rm = Resources.ResourceManager;
            var localizedString = rm.GetString(currentString);
            return (string.IsNullOrEmpty(localizedString) ? currentString : localizedString).Replace('_', ' ');
        }

        #endregion GetLocalizedString

        #region GetStyleLibraryResourceNames

        /// <summary>
        /// Gets an array of  style library resource names.
        /// </summary>
        /// <returns></returns>
        internal static string[] GetStyleLibraryResourceNames()
        {
            var resourceStrings = new List<string>(ExecutingAssembly.GetManifestResourceNames());
            return resourceStrings.FindAll(i => i.EndsWith(@".isl")).ToArray();
        }

        #endregion GetStyleLibraryResourceNames

        #region ToggleDefaultableBoolean

        /// <summary>
        /// Toggles a DefaultableBoolean value.
        /// </summary>
        /// <param name="value">Value to be toggled.</param>
        /// <returns>Changed value.</returns>
        internal static DefaultableBoolean ToggleDefaultableBoolean(DefaultableBoolean value)
        {
            value = value == DefaultableBoolean.True ? DefaultableBoolean.False : DefaultableBoolean.True;

            return value;
        }

        #endregion ToggleDefaultableBoolean

        /// <summary>GUID erstellen und formatieren </summary>
        /// <returns>die GUID als formatierter String</returns>
        public static string GetGuId()
        {
            var retwert = Guid.NewGuid().ToString(@"D");
            return retwert;
        }

        // <summary>
        /// Dieses Funktion kann dazu benutzt werden, um eine integer Zahl in eine Buchstaben Kombination
        /// umzuwandeln die der Kopfzeile eines Excel Dokumentes entspricht
        /// </summary>
        /// <param name="colNumber">die umzuwandelnde Spaltenummer</param>
        /// <returns>(string) die jeweilige Spaltenbezeichnung</returns>
        public static string IntConvertToExcelHeadLine(int colNumber)
        {
            var colBez = string.Empty;                                          // Spaltenbezeichnung
            var rest = 0;                                                       // Wenn ungleich 0, dann ist Spaltenbezeichnung mehrstellig

            // Ist die Zahl größer als 26 ist das Ergebniss mindestens 2 Stellig
            // also muß eine Schleifenverarbeitung durchgeführt werden
            if (colNumber > 26)
            {
                do
                {
                    // Ganzzahl ermitteln für den nächsten durchlauf
                    colNumber = Math.DivRem(colNumber, 26, out rest);
                    if (rest == 0)
                    {
                        colNumber -= 1;
                        rest = 26;
                    }

                    // Umwandlung in einen Buchstaben (die + 64 sind nötig, um auf die Ascii-Zeichen für die Grossbuchstaben zu kommen)
                    colBez = (char)(rest + 64) + colBez;                        // Buchstaben zur Bezeichnung hinzufügen
                } while (colNumber > 26);
            }

            colBez = (char)(colNumber + 64) + colBez;                           // Buchstaben zur Bezeichnung hinzufügen

            return colBez;
        }

        /// <summary>Extrahiert alle Zahlen aus einer Zeichenkette</summary>
        /// <param name="inputString">die zu untersuchende Zeichenkette</param>
        /// <returns>die gefundenen Zahlen, sonst Leerstring</returns>
        public static string ZahlenInString(string inputString)
        {
            var pattern = "[0-9]";                                              // Es soll nach Zahlen gefiltert werden
            var r = new Regex(pattern);                                         // Neue Instanz von Regular Expressions erzeugen
            var mc = r.Matches(inputString);                                    // Aufzählung der gefundenen Zahlen
            var retVal = string.Empty;                                          // Leerstring als Rückgabewert vorgeben

            // Aufzählung durchgehen und alle gefundene Zahlen zu einer Zeichenkette zusammensetzen
            for (var i = 0; i < mc.Count; i++)
            {
                retVal += mc[i].Value;
            }
            return retVal;
        }

        //private Control cloneObject(Control src_ctl)
        //{
        //    var t = src_ctl.GetType();
        //    var obj = Activator.CreateInstance(t);
        //    var dst_ctl = (Control)obj;

        //    var src_pdc = TypeDescriptor.GetProperties(src_ctl);
        //    var dst_pdc = TypeDescriptor.GetProperties(dst_ctl);

        //    for (int i = 0; i < src_pdc.Count; i++)
        //    {
        //        string prop_name = src_pdc[i].Name;

        //        if (prop_name == "Parent" || prop_name == "Handle")
        //            continue;

        //        if (src_pdc[i].Attributes.Contains(DesignerSerializationVisibilityAttribute.Content))
        //        {
        //            object collection_val = src_pdc[i].GetValue(src_ctl);
        //            if ((collection_val is IList) == true)
        //            {
        //                foreach (object child in (IList)collection_val)
        //                {
        //                    object new_child = null;

        //                    if (child == null)
        //                        continue;

        //                    if (child is Control)
        //                    {
        //                        new_child = cloneObject(child as Control);
        //                    }
        //                    else if (child is ICloneable)
        //                        new_child = (child as ICloneable).Clone();
        //                    else
        //                        new_child = ZoomHelper.cloneObject(child); //TODO

        //                    object dst_collection_val = dst_pdc[i].GetValue(dst_ctl);
        //                    ((IList)dst_collection_val).Add(new_child);
        //                }
        //            }
        //        }
        //        else if (src_pdc[i].Attributes.Contains(DesignerSerializationVisibilityAttribute.Visible) && !src_pdc[i].IsReadOnly)
        //        {
        //            //dst_pdc[src_pdc[i].Name].SetValue(dst_ctl, src_pdc[i].GetValue(src_ctl));
        //            object child = src_pdc[i].GetValue(src_ctl);
        //            object new_child = null;

        //            if (child == null)
        //                continue;

        //            if (child is Control)
        //                new_child = cloneObject(child as Control);
        //            else if (child is ICloneable)
        //                new_child = (child as ICloneable).Clone();
        //            else
        //                new_child = ZoomHelper.cloneObject(child);

        //            dst_pdc[src_pdc[i].Name].SetValue(dst_ctl, new_child);
        //        }
        //    }

        //    foreach (EventInfo einfo in t.GetEvents())
        //    {
        //        //EVENTS
        //    }

        //    return dst_ctl;

        //    //}
        //    /*catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //        return null;
        //    }*/
        //}
    }
}