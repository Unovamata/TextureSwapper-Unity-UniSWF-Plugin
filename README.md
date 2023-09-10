<p align="center">
  <img src="https://raw.githubusercontent.com/Unovamata/Unity-Texture-Manager-and-Colorizer/0cfddf04c7ddb4f4399a57ba1df1270a70350fa2/logo.png" />
</p>

# Unity UniSWF Texture Manager
This repository serves a singular purpose: facilitating the swapping of textures within a texture sheet. When working with UniSWF, you can easily modify texture components as required in this specific use case.

# Table of Contents

* [Dependencies](#Dependencies)
* [Installation](#Installation)
* [Usage](#Usage)
* [Features](#Features)

# Dependencies

- Unity's 2D Sprite Package.
- .Net Framework 4.X.
- Unity's IL2CPP Scripting Backend.
- Python (Optional)

# Installation

- [Download the latest release of this repository. You will download a .unitypackage file.](https://github.com/Unovamata/Unity-UniSWF-Texture-Manager/releases "Download the latest release of this repository.") **Download the .unitypackage file.**
- Open the project where you want to deploy this extension.
- Install the **"Unity 2D Sprite"** package from the Package Manager: **"Window" > "Package Manager"**. 
- Inside the **Package Manager** search for **"Packages: In Project"**, click that button and select **Unity Registry**, scroll down until you find **2D Sprite**, click on it and **"Install"** it.
- After Unity loaded the project's assets, go to:  **"Assets" > "Import Package" > "Custom Package..."**
- A new window will be opened. Search for the **"Texture.Manager.unitypackage"** file you have just downloaded. 
- Select it and load it in your project.
- Yet again, a new window will be opened, this time, it will open inside of Unity.
- Once there, click on the **"Import"** button.
- Unity will throw 3 errors. To fix them, go to: **"Edit" > "Project Settings..."** and the **"Project Settings"** window will open.
- Inside the project settings window, go to the **Player** option or click on the search bar and look for these two entries in the options: **"Scripting Backend", "Api Compatibility Settings"**
- Configure to **"IL2CPP" in the Scripting Backend** and **".Net Framework" in the Api Compatibility Settings**.
- And with that, the package will be installed. 
- For testing purposes, head to the "CrAP" scene provided in the "Scenes" folder and run the project.

# Usage
**[Video Tutorial](https://drive.google.com/file/d/1kezgYbWq9qnccIp2Wk744ITlxWI94URy/view?usp=sharing "Here you have a video detailing the usage of the tool")**

# Features

- A Python environment to run Python scripts and code. **This runs separate from your installation of Python.**
- Texture swapping and switching not limited to UniSWF.
- Allows texture swapping for multiple texture sheets (The destination texture can have segments of Texture A, B, and C all at the same time).
- Generation of textures in real-time with little to no slowdown.
- Texture scaling based on the destination size with Bilinear image scaling.
- Automatic data and file segmentation if needed for a more cohesive project structure.
- Texture swapping with multiple skeletons from an animations.
- Supports .SWF files.
- Texture swapping does not affects animation.
