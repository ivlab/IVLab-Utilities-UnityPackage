using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Utilities
{
    /// <summary>
    /// Attach this script to any GameObject to stop the application when the
    /// user presses `esc`.
    /// </summary>
    public class EscapeToQuit : MonoBehaviour
    {
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Quitting!");
                // save any game data here
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }
    }
}