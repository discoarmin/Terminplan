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
    }
}