/* ColormapUtilities.cs
 *
 * Copyright (c) 2023 University of Minnesota
 * Author: Bridger Herman <herma582@umn.edu>, Daniel F. Keefe
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.Linq;


namespace IVLab.Utilities {
    // Inspired by ColorLoom:
    // https://github.umn.edu/ABRAM308/SculptingVisWebApp/blob/master/applets/static/color-loom/sketch.js
    /// <summary>
    /// A color map class that supports:
    /// * colors defined at multiple control points that do not need to be evenly spaced
    /// * interpolation in Lab space
    /// * initializing the map from:
    ///   - Paraview .xml format (used on sciviscolor.org)
    ///   - a regular 2D texture with a gradient running left to right
    ///   - an ABR ColorMapVisAsset
    ///   - a list of control points defined directly in the Unity editor
    /// </summary>
    public class Colormap
    {
        /// <summary>
        /// small internal class to store control point data/color pairs
        /// </summary>
        [Serializable]
        public class ControlPt
        {
            public ControlPt(float d, Color c)
            {
                dataVal = d;
                col = c;
            }
            public float dataVal;
            public Color col;
        }

        public List<ControlPt> controlPts = new List<ControlPt>();

        public Colormap() { }

        public void AddControlPt(float dataVal, Color color)
        {
            // remove any control point already associated with this dataVal
            RemoveControlPt(dataVal, false);
            this.controlPts.Add(new ControlPt(dataVal, color));
            // re-sort the list by dataVal
            this.controlPts = this.controlPts.OrderBy(c => c.dataVal).ToList();
        }

        /// <summary>
        /// Returns true if a control point with the dataVal was found.
        /// Otherwise, returns false and optionally prints a warning.
        /// </summary>
        public bool RemoveControlPt(float dataVal, bool warnOnNotFound=true)
        {
            int i = 0;
            while (i < controlPts.Count) {
                if (controlPts[i].dataVal == dataVal) {
                    controlPts.RemoveAt(i);
                    return true;
                }
                i++;
            }
            if (warnOnNotFound) {
                Debug.LogWarning("ColorMap::removeControlPt no control point with data val = " + dataVal);
            }
            return false;
        }

        /// <summary>
        /// Look up a color within the colormap, interpolating between control
        /// points in CIELab space
        /// </summary>
        public Color LookupColor(float dataVal)
        {
            if (this.controlPts.Count == 0)
            {
                return Color.black;
            }
            else if (this.controlPts.Count == 1)
            {
                return this.controlPts[0].col;
            }
            else
            {
                float minVal = this.controlPts[0].dataVal;
                float maxVal = this.controlPts[this.controlPts.Count - 1].dataVal;

                // check bounds
                if (dataVal >= maxVal)
                {
                    return this.controlPts[this.controlPts.Count - 1].col;
                }
                else if (dataVal <= minVal)
                {
                    return this.controlPts[0].col;
                }
                else
                {  // value within bounds

                    // make i = upper control pt and (i-1) = lower control point
                    int i = 1;
                    while (this.controlPts[i].dataVal < dataVal)
                    {
                        i++;
                    }

                    // convert the two control points to lab space, interpolate
                    // in lab space, then convert back to rgb space
                    Color c1 = this.controlPts[i - 1].col;
                    Color c2 = this.controlPts[i].col;

                    List<float> rgb1 = Lab2Rgb.color2list(c1);
                    List<float> rgb2 = Lab2Rgb.color2list(c2);
                    List<float> lab1 = Lab2Rgb.rgb2lab(rgb1);
                    List<float> lab2 = Lab2Rgb.rgb2lab(rgb2);

                    float v1 = this.controlPts[i - 1].dataVal;
                    float v2 = this.controlPts[i].dataVal;
                    float alpha = (dataVal - v1) / (v2 - v1);

                    List<float> labFinal = new List<float> {
                        lab1[0] * (1.0f - alpha) + lab2[0] * alpha,
                        lab1[1] * (1.0f - alpha) + lab2[1] * alpha,
                        lab1[2] * (1.0f - alpha) + lab2[2] * alpha
                    };

                    List<float> rgbFinal = Lab2Rgb.lab2rgb(labFinal);

                    return Lab2Rgb.list2color(rgbFinal);
                }
            } 
        }

        /// <summary>
        ///  Convert a Unity gradient to a Colormap. Useful for
        ///  interoperability with the Unity editor.
        /// </summary>
        /// <remarks>
        /// > [!WARN]
        /// > Unity's <see cref="Gradient"/> has a max of 8 color/alpha keys, so
        /// if your <see cref="Colormap"/> contains more keys than that, DATA
        /// LOSS WILL OCCUR.
        /// </remarks>
        public static Colormap FromUnityGradient(Gradient g)
        {
            Colormap cmap = new Colormap();
            foreach (GradientColorKey key in g.colorKeys)
            {
                cmap.AddControlPt(key.time, key.color);
            }
            return cmap;
        }

        /// <summary>
        /// Load a colormap from XML string (ParaView's XML Colormap format --
        /// like those exported by sciviscolor.org)
        /// </summary>
        public static Colormap FromXML(string xmlText)
        {
            // Read XML file and produce a Texture2D
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlText);

            XmlNode colormapNode = doc.DocumentElement.SelectSingleNode("/ColorMaps/ColorMap");
            if (colormapNode == null)
                colormapNode = doc.DocumentElement.SelectSingleNode("/ColorMap");

            Colormap colormap = new Colormap();
            foreach (XmlNode pointNode in colormapNode.SelectNodes("Point"))
            {
                float x = float.Parse(pointNode.Attributes.GetNamedItem("x").Value);
                float r = float.Parse(pointNode.Attributes.GetNamedItem("r").Value);
                float g = float.Parse(pointNode.Attributes.GetNamedItem("g").Value);
                float b = float.Parse(pointNode.Attributes.GetNamedItem("b").Value);

                Color toAdd = new Color(r, g, b, 1.0f);
                colormap.AddControlPt(x, toAdd);
            }

            // Create and return the image
            return colormap;
        }

        /// <summary>
        /// Load a colormap from an XML file (ParaView's XML Colormap format --
        /// like those exported by sciviscolor.org)
        /// </summary>
        public static Colormap FromXMLFile(string filePath)
        {
            string xmlText = File.ReadAllText(filePath);
            Colormap cmap = FromXML(xmlText);
            return cmap;
        }

        /// <summary>
        /// Load a colormap from a Texture2D.
        /// </summary>
        /// <remarks>
        /// > [!WARN]
        /// > This method is lossy (it creates control points based on the
        /// > number of samples you define).
        /// </remarks>
        public static Colormap FromTexture2D(Texture2D texture, int numSamples = 11)
        {
            Colormap cmap = new Colormap();
            for (int i = 0; i <= numSamples; i++) {
                float val = (float)i / numSamples;
                val = Mathf.Clamp(val, 0.01f, 0.99f);  // GetPixelBilinear doesn't like lookups at 1.0f on the x
                Color c = texture.GetPixelBilinear(val, 0.5f);
                cmap.AddControlPt(val, c);
            }
            return cmap;
        }

        /// <summary>
        /// Load a colormap from PNG file.
        /// </summary>
        /// <remarks>
        /// > [!WARN]
        /// > This method is lossy (it creates control points based on the
        /// > number of samples you define).
        /// </remarks>
        public static Colormap FromPNGFile(string filePath, int numSamples = 11)
        {
            byte[] pngBytes = File.ReadAllBytes(filePath);
            Texture2D img = new Texture2D(2, 2);
            img.LoadImage(pngBytes);
            return Colormap.FromTexture2D(img, numSamples);
        }

        // Given a colormap, create a texture by drawing a bunch of little
        // rectangles
        public Texture2D ToTexture2D(int texWidth=1024, int texHeight=1)
        {
            // Create our texture and get its width
            Texture2D image = new Texture2D(texWidth, texHeight);

            // Initialize to black
            List<Color> pixels = new List<Color>();
            pixels.AddRange(Enumerable.Repeat(Color.black, texWidth * texHeight));

            // Add the first row to the texture
            for (int col = 0; col < texWidth; col++)
            {
                pixels[col] = LookupColor(col / (float) (texWidth - 1));
            }

            // Copy to the rest of the image if greater than one row
            for (int row = 1; row < texHeight; row++)
            {
                for (int col = 0; col < texWidth; col++)
                {
                    pixels[col + row * texWidth] = pixels[col];
                }
            }

            image.SetPixels(pixels.ToArray());
            image.Apply();
            return image;
        }

        /// <summary>
        /// Save a Colormap to a PNG file.
        /// </summary>
        public void ToPNGFile(string path, int texWidth, int texHeight)
        {
            Texture2D texture = this.ToTexture2D(texWidth, texHeight);
            byte[] pngBytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, pngBytes);
        }

        /// <summary>
        /// Convert the colormap to an XML string.
        /// </summary>
        public string ToXML()
        {
            var cmap = new XElement("ColorMap",
                new XAttribute("space", "CIELAB"),
                new XAttribute("indexedLookup", "false"),
                new XAttribute("name", "IVLab Colormap")
            );

            foreach (var pt in this.controlPts)
            {
                cmap.Add(
                    new XElement("Point",
                        new XAttribute("r", pt.col.r),
                        new XAttribute("g", pt.col.g),
                        new XAttribute("b", pt.col.b),
                        new XAttribute("x", pt.dataVal)
                ));
            }

            var doc = new XDocument(new XElement("ColorMaps", cmap));
            return doc.ToString();
        }


        /// <summary>
        ///  Convert the colormap to a Unity gradient. Useful for
        ///  interoperability with the Unity editor.
        /// </summary>
        /// <remarks>
        /// > [!WARN]
        /// > Unity's <see cref="Gradient"/> has a max of 8 color/alpha keys, so
        /// if your <see cref="Colormap"/> contains more keys than that, DATA
        /// LOSS WILL OCCUR. If there are more than 8 control points, this
        /// method will sample the colormap at 8 equidistant intervals
        /// throughout the colormap instead.
        /// </remarks>
        public Gradient ToUnityGradient()
        {
            List<GradientColorKey> colorKeys = new List<GradientColorKey>();

            if (this.controlPts.Count <= 8)
            {
                foreach (ControlPt pt in this.controlPts)
                {
                    colorKeys.Add(new GradientColorKey(pt.col, pt.dataVal));
                }
            }
            else
            {
                // Gradient() has max of 8 points. Use 8 points including
                // left/right endpoint.
                const int MaxPts = 7;
                float interval = 1.0f / (float) MaxPts;
                for (int pt = 0; pt <= MaxPts; pt++)
                {
                    float percent = pt * interval;
                    Color color = this.LookupColor(percent);
                    colorKeys.Add(new GradientColorKey(color, percent));
                }
            }

            // Alpha doesn't do anything for colormaps
            var alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
            alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);

            var g = new Gradient();
            g.SetKeys(colorKeys.ToArray(), alphaKeys.ToArray());
            return g;
        }
    }
}
