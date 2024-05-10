<p align="center">
  <img src="https://raw.githubusercontent.com/Unovamata/TextureSwapper-Unity-UniSWF-Plugin/main/logo.png" />
</p>

# TextureSwapper - Unity / UniSWF Texture Manager Plugin

The TextureSwapper plugin creates a platform to swap, manage, and change segments of textures in Unity's runtime within a texture sheet. I developed this plugin to modify segments of a model created with UniSWF in real-time in a performant way. Still, the TextureSwapper plugin can adapt to any use case involving textures.
If your use case requires you to swap segments of a texture in real-time, this is the tool for you.

# Table of Contents

* [Dependencies](#Dependencies)
* [Installation](#Installation)
* [Features](#Features)

# Dependencies

- Unity's 2D Sprite Package.
- .Net Framework 4.X.
- Unity's IL2CPP Scripting Backend.

# Installation

- [Download the latest release of this repository. You will download a .unitypackage file.](https://github.com/Unovamata/Unity-UniSWF-Texture-Manager/releases "Download the latest release of this repository.") **Download the .unitypackage file.**
- Open the project where you want to deploy this extension.
- Install the **"Unity 2D Sprite"** package from the Package Manager: **"Window" > "Package Manager"**. 
- Inside the **Package Manager** search for **"Packages: In Project"**, click that button and select **Unity Registry**, scroll down until you find **2D Sprite**, click on it and **"Install"** it.
- After Unity loaded the project's assets, go to:  **"Assets" > "Import Package" > "Custom Package..."**
- Search for the **"Texture.Manager.unitypackage"** file you have just downloaded. 
- Select it and load it in your project.
- Once there, click on the **"Import"** button.
- Unity will throw 3 errors. To fix them, go to **"Edit" > "Project Settings..."**, and the **"Project Settings"** window will open.
- Inside the project settings window, go to the **Player** option or click on the search bar and look for these 2 entries in the options: **"Scripting Backend," "API Compatibility Settings"**
- Configure to **"IL2CPP" in the Scripting Backend** and **".Net Framework" in the Api Compatibility Settings** and Unity will install the package successfully. 
- For testing purposes, head to the "CrAP" scene provided in the "Scenes" folder and run the project.

# Features

- Texture swapping and switching regardless of use case.
- Allows texture segment swapping for singular or multiple texture sheets.
- Generation of textures in real-time with little to no slowdown.
- Texture scaling based on the destination size with Bilinear image scaling.
- Automatic data and file segmentation if needed for a more cohesive project structure.
- Texture swapping with multiple skeletons from animations.
- Supports .SWF files.
- Texture swapping does not affect animation.
