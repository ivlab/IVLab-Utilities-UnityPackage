using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;

namespace IVLab.Utilities {
    // Inspired by ColorLoom:
    // https://github.umn.edu/ABRAM308/SculptingVisWebApp/blob/master/applets/static/color-loom/sketch.js
    class ColormapInternal {
        public List<Tuple<float, Color>> entries = new List<Tuple<float, Color>>();

        public ColormapInternal() { }

        public void AddControlPoint(float dataVal, Color color)
        {
            this.entries.Add(Tuple.Create(dataVal, color));
            this.entries.Sort((e1, e2) => {
                if (e1.Item1 < e2.Item1)
                {
                    return -1;
                }
                else if (e1.Item1 > e2.Item1)
                {
                    return 1;
                }
                else {
                    return 0;
                }
            });
        }

        public Color LookupColor(float dataVal)
        {
            if (this.entries.Count == 0)
            {
                return Color.black;
            }
            else if (this.entries.Count == 1)
            {
                return this.entries[0].Item2;
            }
            else
            {
                float minVal = this.entries[0].Item1;
                float maxVal = this.entries[this.entries.Count - 1].Item1;

                // check bounds
                if (dataVal >= maxVal)
                {
                    return this.entries[this.entries.Count - 1].Item2;
                }
                else if (dataVal <= minVal)
                {
                    return this.entries[0].Item2;
                }
                else
                {  // value within bounds

                    // make i = upper control pt and (i-1) = lower control point
                    int i = 1;
                    while (this.entries[i].Item1 < dataVal)
                    {
                        i++;
                    }

                    // convert the two control points to lab space, interpolate
                    // in lab space, then convert back to rgb space
                    Color c1 = this.entries[i - 1].Item2;
                    Color c2 = this.entries[i].Item2;

                    List<float> rgb1 = Lab2Rgb.color2list(c1);
                    List<float> rgb2 = Lab2Rgb.color2list(c2);
                    List<float> lab1 = Lab2Rgb.rgb2lab(rgb1);
                    List<float> lab2 = Lab2Rgb.rgb2lab(rgb2);

                    float v1 = this.entries[i - 1].Item1;
                    float v2 = this.entries[i].Item1;
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
    }

    public class ColormapUtilities
    {
        public static Texture2D ColormapFromFile(string filePath, int texWidth=1024, int texHeight=100)
        {
            string extention = Path.GetExtension(filePath);

            // Read PNG file and produce a Texture2D
            Texture2D image;
            if (extention.ToUpper() == ".PNG")
            {
                // Create and return the image
                image = new Texture2D(1, 1);
                image.LoadImage(File.ReadAllBytes(filePath));
            }
            else if (extention.ToUpper() == ".XML")
            {
                // Read XML file and produce a Texture2D
                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);

                XmlNode colormapNode = doc.DocumentElement.SelectSingleNode("/ColorMaps/ColorMap");
                if (colormapNode == null)
                    colormapNode = doc.DocumentElement.SelectSingleNode("/ColorMap");
                string name = colormapNode.Attributes.GetNamedItem("name") != null ? colormapNode.Attributes.GetNamedItem("name").Value : Path.GetFileName(filePath);

                ColormapInternal colormap = new ColormapInternal();

                foreach (XmlNode pointNode in colormapNode.SelectNodes("Point"))
                {
                    float x = float.Parse(pointNode.Attributes.GetNamedItem("x").Value);
                    float r = float.Parse(pointNode.Attributes.GetNamedItem("r").Value);
                    float g = float.Parse(pointNode.Attributes.GetNamedItem("g").Value);
                    float b = float.Parse(pointNode.Attributes.GetNamedItem("b").Value);

                    Color toAdd = new Color(r, g, b, 1.0f);
                    colormap.AddControlPoint(x, toAdd);
                }

                // Create and return the image
                image = CreateTextureFromColormap(colormap, texWidth, texHeight);
            }
            else
            {
                throw new System.Exception("Colormap must be a .png or a .xml");
                // Read JSON file and produce a Texture2D
                // TODO properly support
            }

            return image;
        }

        // Given a colormap, create a texture by drawing a bunch of little
        // rectangles
        private static Texture2D CreateTextureFromColormap(ColormapInternal colormap, int texWidth=1024, int texHeight=1)
        {
            // Create our texture and get its width
            Texture2D image = new Texture2D(texWidth, texHeight);

            // Initialize to black
            List<Color> pixels = new List<Color>();
            pixels.AddRange(Enumerable.Repeat(Color.black, texWidth * texHeight));

            // Add the first row to the texture
            for (int col = 0; col < texWidth; col++)
            {
                pixels[col] = colormap.LookupColor(col / (float) texWidth);
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

        public static void SaveTextureAsPng(string path, Texture2D texture)
        {
            byte[] pngBytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, pngBytes);
        }
    }
}
