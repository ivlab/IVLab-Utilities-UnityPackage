/* TextureUtilities.cs
 *
 * Copyright (c) 2021, University of Minnesota
 * Author: Bridger Herman <herma582@umn.edu>
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

namespace IVLab.Utilities
{
    public static class TextureUtilities
    {
        /// <summary>
        /// Create a texture gradient (series of distinct textures), stacked vertically.
        /// For example, if we have 3 textures: 1024x100, 2048x100, and
        /// 1024x100, the resulting image will be 2048x300. By default, all
        /// textures are scaled to the max width.
        /// </summary>
        public static Texture2D MakeTextureGradientVertical(List<Texture2D> textures)
        {
            // Calculate the resulting width/height
            int maxWidth = textures.Select((tex) => tex.width).Max();
            int totalHeight = textures.Select((tex) => tex.height).Sum();

            if (textures.Count == 0)
            {
                Debug.LogError("Must be at least one texture in texture gradient");
                return null;
            }

            List<Color> finalPixels = new List<Color>();

            // For every texture, scale horizontally if necessary and add the pixels
            foreach (Texture2D tex in textures)
            {
                Texture2D rescaled = tex;
                if (tex.width != maxWidth)
                {
                    rescaled = ScaleTexture(rescaled, maxWidth, tex.height);
                    Debug.LogWarning("Texture scaled when making texture gradient, resulting gradient may not be optimal");
                }
                finalPixels.AddRange(rescaled.GetPixels());
            }

            Texture2D finalImage = new Texture2D(maxWidth, totalHeight);
            finalImage.SetPixels(finalPixels.ToArray());
            finalImage.Apply();
            return finalImage;
        }

        /// <summary>
        ///     Make a gradient out of a set of textures. Textures will be squashed
        ///     together horizontally with no resizing.
        /// </summary>
        public static Texture2D MakeTextureGradient(List<Texture2D> textures)
        {
            return MakeTextureGradient(textures, null);
        }

        /// <summary>
        ///     Make a gradient out of a set of textures. Textures will be squashed
        ///     together horizontally. The textures will be resized according to `stops`.
        /// </summary>
        public static Texture2D MakeTextureGradient(List<Texture2D> textures, List<float> stops)
        {
            int totalWidth = textures.Select((tex) => tex.width).Sum();
            int maxHeight = textures.Select((tex) => tex.height).Max();

            if (stops != null && stops.Count + 1 != textures.Count)
            {
                Debug.LogError("Number of textures in gradient must equal number of stops + 1");
                return null;
            }

            if (textures.Count == 0)
            {
                Debug.LogError("Must be at least one texture in texture gradient");
                return null;
            }

            // There's only one texture and no stops
            if (textures.Count == 1)
            {
                return textures[0];
            }

            // Find (percentage-wise) how wide each texture should be
            List<float> widthPercentages = new List<float>();
            if (stops != null)
            {
                widthPercentages.Add(stops[0]);
                for (int i = 1; i < stops.Count; i++)
                {
                    widthPercentages.Add(stops[i] - stops[i - 1]);
                }
                widthPercentages.Add(1.0f - stops[stops.Count - 1]);
            }

            // Resize each texture and add its pixels to the big array
            List<Color> finalPixels = new List<Color>();
            for (int i = 0; i < textures.Count; i++)
            {
                // If we have stops provided, resize them to fit within their
                // allotted percentage of the overall width
                Texture2D transposed;
                if (stops != null)
                {
                    Texture2D scaled = ScaleTexture(textures[i], (int)(totalWidth * widthPercentages[i] + 1), maxHeight);
                    transposed = TransposeTexture(scaled);
                }
                else
                {
                    // Otherwise, just smoosh them together
                    transposed = TransposeTexture(textures[i]);
                }
                finalPixels.AddRange(transposed.GetPixels());
            }

            Texture2D finalTex = new Texture2D(maxHeight, totalWidth);
            finalTex.SetPixels(finalPixels.ToArray());
            finalTex.Apply();
            return TransposeTexture(finalTex);
        }

        /// <summary>
        ///     Transpose a texture from tall to wide or vice versa.
        ///     WARNING: this is SLOW.
        /// </summary>
        public static Texture2D TransposeTexture(Texture2D inputTexture)
        {
            Color[] pixels = inputTexture.GetPixels();
            Color[] outputPixels = new Color[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                int inputRow = i / inputTexture.width;
                int inputCol = i % inputTexture.width;

                // Swap rows and cols
                int outputIndex = inputCol * inputTexture.height + inputRow;
                outputPixels[outputIndex] = pixels[i];
            }
            Texture2D outputTexture = new Texture2D(inputTexture.height, inputTexture.width);
            outputTexture.SetPixels(outputPixels);
            outputTexture.Apply();
            return outputTexture;
        }

        /// <summary>
        ///     Rescale a texture to a new height/width, using a point sampling
        ///     method (TODO bilinear or gaussian?)
        ///     WARNING: this is SLOW.
        /// </summary>
        public static Texture2D ScaleTexture(Texture2D inputTexture, int outputWidth, int outputHeight)
        {
            Color[] pixels = inputTexture.GetPixels();
            Color[] outputPixels = new Color[outputWidth * outputHeight];

            // Scale factor from output -> input
            float scaleFactorRows = (float)inputTexture.height / (float)outputHeight;
            float scaleFactorCols = (float)inputTexture.width / (float)outputWidth;

            for (int row = 0; row < outputHeight; row++)
            {
                for (int col = 0; col < outputWidth; col++)
                {
                    float inputRow = (float)row * scaleFactorRows;
                    float inputCol = (float)col * scaleFactorCols;

                    // Point sampling, just using floor
                    int inputIndex = (int)(inputRow * inputTexture.width + inputCol);
                    int outputIndex = row * outputWidth + col;

                    // Map pixels output -> input
                    outputPixels[outputIndex] = pixels[inputIndex];
                }
            }

            Texture2D outputTexture = new Texture2D(outputWidth, outputHeight);
            outputTexture.SetPixels(outputPixels);
            outputTexture.Apply();
            return outputTexture;
        }

    }
}