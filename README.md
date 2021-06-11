# IVLab-Utilities

## Installation:

### Non-development (read-only) Package use
1. In Unity, open Window -> Package Manager. 
2. Click the ```+``` button
3. Select ```Add package from git URL```
4. Paste ```git@github.umn.edu:ivlab-cs/IVLab-Utilities-UnityPackage.git``` for the latest package
  - If you want to access a particular release or branch, you can append ```#<tag or branch>``` at the end, e.g. ```git@github.umn.edu:ivlab-cs/IVLab-Utilities-UnityPackage.git#v0.0.1```

To switch to a new version or branch, just repeat the above steps. 

### Development use in a git-managed project
1. Navigate your terminal or Git tool into your version-controlled Unity project's main folder. 
2. Add this repository as a submodule: ```git submodule add git@github.umn.edu:ivlab-cs/IVLab-Utilities-UnityPackage.git Assets/IVLab-Utilities; git submodule update --init --recursive```
3. See https://git-scm.com/book/en/v2/Git-Tools-Submodules for more details on working with Submodules. 

### Development use in a non git-managed project
1. Navigate your terminal or Git tool into your non version-controlled Unity project's main folder. 
2. Clone this repository into the Assets folder: ```git clone git@github.umn.edu:ivlab-cs/IVLab-Utilities-UnityPackage.git Assets/IVLab-Utilities```

## Contents

### Editor

- FunctionDebugger: Enables a user to run methods of a MonoBehaviour by clicking
  a GUI button in the Unity Editor.

### Runtime

- Scripts
  - Bounds: Extension methods and classes for dealing with Unity Bounds objects
  - Bytes: Converting bytes to objects/arrays and vice versa;
    compressing/decompressing bytes
  - Color: Classes and methods for dealing with color interpolation, colormaps,
    and textures
  - Debuggers: Classes for helping debug in the Unity editor
  - ExtensionMethods: [C# Extension Methods](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods) for various classes to make working with them easier.
  - Gizmos: Wireframe helpers to show up in the Unity scene view to aid in debugging
  - ObjectPooling: Simple object pool to avoid unnecessary/wasteful memory allocations
  - Singleton: "Singleton" MonoBehaviour objects for a single object instance that
    persists throughout the lifetime of a Unity app
  - SpaceTransforms: Classes and helpers for transforming between coordinate
    spaces (data space, room space, world space)
  - Streaming: Methods for sending/receiving bytes across a stream (i.e. a socket)
  - Threading: Helpers for working with multi-threaded code in Unity
  - UIHelpers: User-facing elements like color pickers and camera interactors
- Materials
- Prefabs
- Models

## Documentation

[Auto-generated documentation for the UnityPackage is
available](https://pages.github.umn.edu/ivlab-cs/IVLab-Utilities-UnityPackage/api/IVLab.Utilities.html). To
re-generate the documentation,
 follow the instructions in the
[Documentation](./Documentation) folder.