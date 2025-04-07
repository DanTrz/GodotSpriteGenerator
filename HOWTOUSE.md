# Installation options
* Option 1 - Get the executable from [GitHub Releases](https://github.com/DanTrz/GodotSpriteGenerator/releases) 
* Option 2 - Run the Project in Godot Version 4.4+ (Use .Net version)

# SpriteGenerator Overview
There are two main tabs in the SpriteGenerator:
1. SpriteGenerator Tab => To generate Sprites and SpriteSheets
2. Image Editor Tab => To apply Color Reduction and to tweak image Saturation, Brightness, Outline etc
3. There is a built-in Low-Poly Voxel model with replaceble parts for you to play with and create placeholder NPCs of quick prototyping.

Some known bugs or issues include:
* Entire save configuration is broken and will be fixed in the next update
* Performance is not great if your model is too complex or with too many colors (in case of Color Reduction)
* Some UI elements are just placeholders for now.
* Sprites and SpriteSheets need to be manually loaded into the Image Editor (They don't transfer from the Sprite Generator Tab yet)

## Tab 1: SpriteGenerator
![SpriteGenTab](https://github.com/user-attachments/assets/658b807c-5b46-4f35-bb30-21739340dc82)
* Choose the options you want and click Start Generation Button to generate your Sprite and SpriteSheet
* By default, the sprites and sprite sheet will be saved in Godot's USER DATA folder. Usually located at: `C:\Users\{username}\AppData\Roaming\Godot\app_userdata\SpriteGenerator`
* You can access the folder location from the Main Settings Tab (Uper right corner)

Key options in this tab are:

* <--Key Settings-->
* Create SpriteSheet = True/False
* Create Sprite = True/False
* Sprite Size = 64x64/128x128/256x256/384x384/512x512 (These are the actual size of the images, not the resolution)
* Sprite Resolution = 64x64/128x128/256x256/384x384/512x512 (This applies a pixelation effect in case the Sprite Size is greater than the resolution)
* Effect Choice = No Effect/Unshaded/Toon Shading
* Outline 3D = Add outline 3D
* Outline Color = Change the color of the outline (Only applicable if Color Blend is set to 0.9 or greater)
* Outline Color blend = Default behaviour the Outline gets it's color from the neighbor pixel in the model, you can tweak it here. 
* DepthLine 3D = Add DepthLine
* DepthLine Color = Similar to the Outline - You can set a custom color for the DepthLine or use the Neighbor pixel color
* DepthLine Color blend = Same logic as Outline Color Blend
* DepthLine Threshold = Defines how much the DepthLine is visible and how sensitive it is to changes in depth
* Angles = Define the angles for the Sprite Sheet and Sprite. This will auto-rotate your Model to match the angles.
* Frame Skip = Allows to reduce the number of frames in the sprite sheet. This is useful if you don't want to create an animation for every single frame. Higher numbers means less frames.
* Move SpriteSheet (Currently Broken - Doesn't do anything yet) =
* RGB Levels = To create a more "Pixel" and Old School look and feel, I'm limiting the RGB levels. You can tweak it.
* ShowGrid = Shows a Grid in front of your model
* Animation Method = Define if you want to Generate sprites based on Animation Player Animations or if you want to generate a static sprite (That will simple rotate the model in the Y Axis)

* <--Mesh / Parts Replacer-->
* There is one built in model included in the Project that includes replaceable parts
* In the future, the idea is that you will be able to upload your own models with replaceable parts
* For now, only the built in model works with replaceable parts, but feel free to tweak it and leverage the code.
* You can edit the model, change the weapon, clothes, color of some parts, etc. 

* <--Load Animation-->
* Allows to choose from a long list of pre-defined animations (Some are from Mixamo, some are from My own work in Blender)


## Tab 2: Image Editor
![ImageEditorTab](https://github.com/user-attachments/assets/27ecfd9f-4cea-46fc-915b-91cddf9adf08)
* Currently you need to Load the Sprite or Sprite Sheet via the "Load file" button. Future versions will automaticaly load the file from the SpriteGenerator Tab

Key options in this tab are:
* <--Image Correction Options-->
* Saturation, Brightness, Contrast => These are self-explanatory. You can use the slider to adjust them.
* 2D Outline -> this is another option to add Outline (Not the 3D one, it has pros and cons, try both)
* 2D Inline -> This will add a thin line to the image

* <--Color Reduction Options-->
* Allows to reduce the number of colors in the image. You can set the number of colors to reduce to.

* <-- Palette and Persist Color Options-->
* Persit Colors => You can add Colors that won't be affected the color reduction or by the External palette. These colors will aways stay present in the model.
* External Palette => If you set the External Palette Options to True, You can load an external palette file to change the colors of the model. This works with Palette HEX files.

## Adding your Own models
![customModelScene](https://github.com/user-attachments/assets/425879ce-b70e-4663-9900-e7dc94237c84)

* Open the Scene "ModelScene3D.tscn". 
* Add your 3D model as child of the Node "Model3DPivot_AddYourModelHere"
* In the built in Example, my model scene is called BaseModel_LowPoly. you need to repalce this scene with your own 3D model Scene.
* Tweak the model size, scale and position to make sure it feets the Camera in this scene.
* Don't add more than one Node as Child of "Model3DPivot_AddYourModelHere". If you want to edit your custom scene 3D do that in a separate branch scene. 












