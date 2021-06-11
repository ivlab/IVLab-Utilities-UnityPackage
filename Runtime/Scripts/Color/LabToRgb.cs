using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Utilities
{
    // ----- BEGIN EXTERNAL CODE FOR RGB-LAB CONVERSION -----
    // https://github.com/antimatter15/rgb-lab

    /*
    MIT License
    Copyright (c) 2014 Kevin Kwok <antimatter15@gmail.com>
    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:
    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
    */

    // the following functions are based off of the pseudocode
    // found on www.easyrgb.com

    /// <summary>
    /// Conversions for RGB and CIE Lab color spaces and Unity color objects
    /// </summary>
    class Lab2Rgb {
        public static List<float> color2list(Color color)
        {
            return new List<float> {
                color.r,
                color.g,
                color.b
            };
        }

        public static Color list2color(List<float> list)
        {
            return new Color(list[0], list[1], list[2], 1.0f);
        }

        public static List<float> lab2rgb(List<float> lab) {
            float y = (lab[0] + 16.0f) / 116.0f,
                x = lab[1] / 500.0f + y,
                z = y - lab[2] / 200.0f,
                r, g, b;

            x = 0.95047f * ((x * x * x > 0.008856f) ? x * x * x : (x - 16.0f / 116.0f) / 7.787f);
            y = 1.00000f * ((y * y * y > 0.008856f) ? y * y * y : (y - 16.0f / 116.0f) / 7.787f);
            z = 1.08883f * ((z * z * z > 0.008856f) ? z * z * z : (z - 16.0f / 116.0f) / 7.787f);

            r = x * 3.2406f + y * -1.5372f + z * -0.4986f;
            g = x * -0.96890f + y * 1.8758f + z * 0.0415f;
            b = x * 0.05570f + y * -0.2040f + z * 1.0570f;

            r = (r > 0.0031308f) ? (1.055f * Mathf.Pow(r, 1.0f / 2.4f) - 0.055f) : 12.92f * r;
            g = (g > 0.0031308f) ? (1.055f * Mathf.Pow(g, 1.0f / 2.4f) - 0.055f) : 12.92f * g;
            b = (b > 0.0031308f) ? (1.055f * Mathf.Pow(b, 1.0f / 2.4f) - 0.055f) : 12.92f * b;

            // Colors are already 0-1 in Unity
            return new List<float> {
                Mathf.Max(0.0f, Mathf.Min(1.0f, r)),
                Mathf.Max(0.0f, Mathf.Min(1.0f, g)),
                Mathf.Max(0.0f, Mathf.Min(1.0f, b))
            };
        }


        public static List<float> rgb2lab(List<float> rgb) {
            // Colors are already 0-1 in Unity
            // var r = rgb[0] / 255,
            //     g = rgb[1] / 255,
            //     b = rgb[2] / 255,
            float r = rgb[0],
                g = rgb[1],
                b = rgb[2],
                x, y, z;

            r = (r > 0.04045f) ? Mathf.Pow((r + 0.055f) / 1.055f, 2.4f) : r / 12.92f;
            g = (g > 0.04045f) ? Mathf.Pow((g + 0.055f) / 1.055f, 2.4f) : g / 12.92f;
            b = (b > 0.04045f) ? Mathf.Pow((b + 0.055f) / 1.055f, 2.4f) : b / 12.92f;

            x = (r * 0.4124f + g * 0.3576f + b * 0.1805f) / 0.95047f;
            y = (r * 0.2126f + g * 0.7152f + b * 0.0722f) / 1.00000f;
            z = (r * 0.0193f + g * 0.1192f + b * 0.9505f) / 1.08883f;

            x = (x > 0.008856f) ? Mathf.Pow(x, 1.0f / 3.0f) : (7.787f * x) + 16.0f / 116.0f;
            y = (y > 0.008856f) ? Mathf.Pow(y, 1.0f / 3.0f) : (7.787f * y) + 16.0f / 116.0f;
            z = (z > 0.008856f) ? Mathf.Pow(z, 1.0f / 3.0f) : (7.787f * z) + 16.0f / 116.0f;

            return new List<float> {
                (116.0f * y) - 16.0f,
                500.0f * (x - y),
                200.0f * (y - z)
            };
        }
    }
}