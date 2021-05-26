using System.IO;
using UnityEngine;

namespace IVLab.Utilities
{
    [RequireComponent(typeof(Camera))]
    public class Screenshot : MonoBehaviour
    {
        private ScreenshotParams screenshotOnLateUpdate = null;

        private class ScreenshotParams
        {
            public string filePath;
            public int width;
            public int height;
            public bool transparentBackground;
            public int jpgQuality;
        }

        // transparent background only applies to non-jpg images (jpgQuality != -1)
        public byte[] CaptureView(int width, int height, bool transparentBackground, int jpgQuality)
        {
            var camera = GetComponent<Camera>();

            RenderTexture rt = new RenderTexture(width, height, 24);

            Color temp = camera.backgroundColor;

            // Don't screenshot UI elements
            int layerID = LayerMask.NameToLayer("UI");
            int oldCullingMask = camera.cullingMask;
            camera.cullingMask &= ~(1 << layerID);

            if (transparentBackground && jpgQuality < 0)
            {
                camera.backgroundColor = Color.clear;
            }

            camera.targetTexture = rt;
            Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
            camera.Render();

            RenderTexture.active = rt;
            image.ReadPixels(new Rect(0, 0, camera.pixelWidth, camera.pixelHeight), 0, 0);
            image.Apply();

            camera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);

            byte[] bytes;
            if (jpgQuality == -1)
            {
                bytes = image.EncodeToPNG();
            }
            else
            {
                bytes = image.EncodeToJPG(jpgQuality);
            }
            Destroy(image);

            camera.cullingMask = oldCullingMask;
            camera.backgroundColor = temp;
            RenderTexture.active = null;
            return bytes;
        }

        public void SaveScreenshot(string filepath, int width, int height, bool transparentBackground, int jpgQuality=-1)
        {
            byte[] bytes = CaptureView(width, height, transparentBackground, jpgQuality);
            File.WriteAllBytes(filepath, bytes);
        }

        /// <summary>
        ///     Save a screenshot but run the screenshot in LateUpdate() instead of *right now*
        /// </summary>
        public void SaveScreenshotOnLateUpdate(string filepath, int width, int height, bool transparentBackground, int jpgQuality=-1)
        {
            screenshotOnLateUpdate = new ScreenshotParams {
                filePath = filepath,
                width = width,
                height = height,
                transparentBackground = transparentBackground,
                jpgQuality = jpgQuality
            };
        }

        void LateUpdate()
        {
            if (screenshotOnLateUpdate != null)
            {
                SaveScreenshot(
                    screenshotOnLateUpdate.filePath,
                    screenshotOnLateUpdate.width,
                    screenshotOnLateUpdate.height,
                    screenshotOnLateUpdate.transparentBackground,
                    screenshotOnLateUpdate.jpgQuality
                );
                screenshotOnLateUpdate = null;
            }
        }
    }
}