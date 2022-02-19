using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor;

/// <summary>
/// Builds a tarball of each custom dependency specified in
/// `packageNamesToBuild`. This script will build each listed package into the
/// current project's `Packages` folder.
/// </summary>
[ExecuteInEditMode]
public class Builder : MonoBehaviour
{
    // Headers display backwards for some reason
    [Header("4. Add these tgz files as 'tarball packages' in the Package Manager")]
    [Header("3. Verify that all .tgz files build correctly and are now in your Packages folder")]
    [Header("2. Check the 'Build' checkbox to build all packages")]
    [Header("  find it in Package Manager or in Packages/manifest.json)")]
    [Header("1. Define which packages to build (using their package name -")]
    [Header("STEPS TO BUILD PACKAGES:")]

    [Tooltip("Add package names you want to build here")]
    public List<string> packageNamesToBuild = new List<string>();

    [Tooltip("Press this 'button' to build your packages")]
    public bool build;


    private string buildFolder;

    void Update()
    {
        if (build)
        {
            buildFolder = Path.Combine(Application.dataPath, "..", "Packages");
            List();
            build = false;
        }
    }

    static ListRequest Request;

    void List()
    {
        Request = Client.List();    // List packages installed for the project
        EditorApplication.update += Progress;
    }

    void Progress()
    {
        if (Request.IsCompleted)
        {
            if (Request.Status == StatusCode.Success)
            {
                int n = 0;
                foreach (var package in Request.Result)
                {
                    if (packageNamesToBuild.Contains(package.name))
                    {
                        Client.Pack(package.resolvedPath, buildFolder);
                        Debug.Log("Built package: " + package.name);
                        n += 1;
                    }
                }
                Debug.Log($"Built {n} packages to {buildFolder}");
            }
            else if (Request.Status >= StatusCode.Failure)
                Debug.Log(Request.Error.message);

            EditorApplication.update -= Progress;
        }
    }
}
