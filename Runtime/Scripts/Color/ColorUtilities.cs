/* ColorUtilities.cs
 *
 * Copyright (c) 2021, University of Minnesota
 * Author: Bridger Herman <herma582@umn.edu>
 *
 * Replacement for Unity's ColorUtility class which has bugs when converting to/from HTML colors.
 */

using System;
using UnityEngine;

namespace IVLab.Utilities
{
    public static class ColorUtilities
    {
        /// <summary>
        /// Converts a hex HTML color (e.g. `#00ff00`) into a Unity color
        /// </summary>
        public static Color HexToColor(string hexColor)
        {
            byte red = Convert.ToByte("0x" + hexColor.Substring(1, 2), 16);
            byte green = Convert.ToByte("0x" + hexColor.Substring(3, 2), 16);
            byte blue = Convert.ToByte("0x" + hexColor.Substring(5, 2), 16);
            float redFloat = (float)red / (float)byte.MaxValue;
            float greenFloat = (float)green / (float)byte.MaxValue;
            float blueFloat = (float)blue / (float)byte.MaxValue;
            return new Color(redFloat, greenFloat, blueFloat);
        }

        /// <summary>
        /// Converts a Unity color into an HTML hex color (e.g. `#00ff00`)
        /// </summary>
        public static string ColorToHex(Color color)
        {
            byte[] channels = new byte[] {
                (byte) (color.r * byte.MaxValue),
                (byte) (color.g * byte.MaxValue),
                (byte) (color.b * byte.MaxValue)
            };
            return '#' + BitConverter.ToString(channels).Replace("-", string.Empty);
        }
    }
}
