# IVLab-Utilities

## Installation:

### Non-development Package use
- In Unity, open Window -> Package Manager. 
- Click the ```+``` button
- Select ```Add package from git URL```
- Paste ```git@github.umn.edu:ivlab-cs/IVLab-Utilities-UnityPackage.git``` for the latest package
  - If you want to access a particular release or branch, you can append ```#<tag or branch>``` at the end, e.g. ```git@github.umn.edu:ivlab-cs/IVLab-Utilities-UnityPackage.git#v0.0.1```
- To switch to a new version or branch, just repeat the above steps. 

### Development use in a git-managed project
- Navigate your terminal or Git tool into your version-controlled Unity project's Assets folder. 
- Add this repository as a submodule: ```<projectpath>/Assets > git submodule add git@github.umn.edu:ivlab-cs/IVLab-Utilities-UnityPackage.git IVLab-Utilities```
- See https://git-scm.com/book/en/v2/Git-Tools-Submodules for more details on working with Submodules. 

### Development use in a non git-managed project
- Navigate your terminal or Git tool into your non version-controlled Unity project's Assets folder. 
- Clone this repository into the Assets folder: ```<project path>/Assets > git clone git@github.umn.edu:ivlab-cs/IVLab-Utilities-UnityPackage.git IVLab-Utilities```

