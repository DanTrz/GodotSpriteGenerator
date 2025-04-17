using System;
using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class SaveGameData : Resource
{
    //SPRITE GENERATOR VARIABLES AND CONFIG pixelShaderResolution //SpriteSize
    [Export] public string SaveFileName { get; set; }

    [Export] public bool CreateSpriteSheet { get; set; }
    [Export] public int SpriteSize { get; set; }
    [Export] public int SpriteSizeOptionButtonSelected { get; set; }
    [Export] public int PixelResolution { get; set; }
    [Export] public int PixelResolutionButtonSelected { get; set; }

    [Export] public int EffectOptionSelected { get; set; }
    [Export] public float Outline3DValue { get; set; }
    [Export] public float Outline3DBlendValue { get; set; }
    [Export] public Color Outline3DColor;

    [Export] public float DepthLine3DValue { get; set; }
    [Export] public float DepthLine3DBlendValue { get; set; }
    [Export] public float DepthLineThresholdValue { get; set; }
    [Export] public Color DepthLine3DColor;
    //[Export] public Godot.Collections.Array<int> AnimationAngles;
    [Export] public int[] AnimationAngles;
    [Export] public int FrameSkipStep { get; set; }
    [Export] public bool MoveSpriteSheet { get; set; }
    [Export] public float RGBLevels { get; set; }
    [Export] public float DitheringValue { get; set; }
    [Export] public bool ShowGrid { get; set; }
    [Export] public int AnimationMethodSelected { get; set; }

    //MODEL POSITION MANAGER VARIABLES AND CONFIG
    [Export] public float CameraDistance { get; set; }
    [Export] public float CameraRotation { get; set; }

    [Export] public float ModelPositionXAxis { get; set; }
    [Export] public float ModelPositionYAxis { get; set; }
    [Export] public float ModelPositionZAxis { get; set; }

    [Export] public float ModelRotationXAxis { get; set; }
    [Export] public float ModelRotationYAxis { get; set; }
    [Export] public float ModelRotationZAxis { get; set; }

    //IMAGE EDITOR VARIABLES
    [Export] public float SaturationValue { get; set; }
    [Export] public float BrightnessValue { get; set; }
    [Export] public float ContrastValue { get; set; }
    [Export] public float Outline2DValue { get; set; }
    [Export] public Color Outline2DColor { get; set; }
    [Export] public float Inline2DValue { get; set; }
    [Export] public Color Inline2DColor { get; set; }
    [Export] public bool ColorReductionIsOn { get; set; }
    [Export] public float ColorReducNumColors { get; set; }

    //PALETTE AND PERSIST COLORS
    public List<Color> PersistColors { get; set; }

    [Export] public bool UseExternalPalette { get; set; }

    [Export] public Godot.Collections.Array<Color> ExternalPalette { get; set; }

    //Used by SAVEGAMEMANAGER to Save Data for a new SaveGame file
    public void GetSaveGameDataFromNodes(SpriteGenerator SpriteGenParentNode, ImageEditorMainPanel ImgEditorParentNode)
    {
        CreateSpriteSheet = SpriteGenParentNode.GenerateSpriteSheetCheckBtn.ButtonPressed;
        SpriteSize = SpriteGenParentNode.SpriteSize;
        SpriteSizeOptionButtonSelected = SpriteGenParentNode.SpriteSizeOptionButton.Selected;
        PixelResolution = SpriteGenParentNode.PixelResolution;
        PixelResolutionButtonSelected = SpriteGenParentNode.PixelationLevelOptionBtn.Selected;

        EffectOptionSelected = SpriteGenParentNode.EffectsChoicesOptionBtn.Selected;
        Outline3DValue = (float)SpriteGenParentNode.Outline3DValueSlider.Value;
        Outline3DBlendValue = (float)SpriteGenParentNode.Outline3DBlendFactorSlider.Value;
        Outline3DColor = SpriteGenParentNode.Outline3DColorPicker.Color;
        DepthLine3DValue = (float)SpriteGenParentNode.DepthlineThicknessSlider.Value;
        DepthLine3DBlendValue = (float)SpriteGenParentNode.DepthBlendValueSlider.Value;
        DepthLineThresholdValue = (float)SpriteGenParentNode.DepthThresholdSlider.Value;
        DepthLine3DColor = SpriteGenParentNode.Depthline3DColorPicker.Color;
        AnimationAngles = SpriteGenParentNode.AngleSelectionItemList.GetSelectedItems();
        FrameSkipStep = SpriteGenParentNode.FrameSkipStep;

        MoveSpriteSheet = SpriteGenParentNode.MoveSpriteSheetCheckButton.ButtonPressed;
        RGBLevels = (float)SpriteGenParentNode.MaxColorPaletteSpinBox.Value;
        DitheringValue = (float)SpriteGenParentNode.DitheringStrenghtSlider.Value;
        ShowGrid = SpriteGenParentNode.ShowGridCheckButton.ButtonPressed;
        AnimationMethodSelected = SpriteGenParentNode.AnimMethodOptionBtn.Selected;

        //MODEL POSITION MANAGER VARIABLES AND CONFIG
        CameraDistance = float.Parse(SpriteGenParentNode.ModelPositionManager.CamDistancelLineTextEdit.Text);
        CameraRotation = float.Parse(SpriteGenParentNode.ModelPositionManager.CamXRotationLineTextEdit.Text);

        ModelPositionXAxis = float.Parse(SpriteGenParentNode.ModelPositionManager.PosXAxisLineTextEdit.Text);
        ModelPositionYAxis = float.Parse(SpriteGenParentNode.ModelPositionManager.PosYAxisLineTextEdit.Text);
        ModelPositionZAxis = float.Parse(SpriteGenParentNode.ModelPositionManager.PosZAxisLineTextEdit.Text);

        ModelRotationXAxis = float.Parse(SpriteGenParentNode.ModelPositionManager.RotationXAxisLineTextEdit.Text);
        ModelRotationYAxis = float.Parse(SpriteGenParentNode.ModelPositionManager.RotationYAxisLineTextEdit.Text);
        ModelRotationZAxis = float.Parse(SpriteGenParentNode.ModelPositionManager.RotationZAxisLineTextEdit.Text);

        //IMAGE EDITOR VARIABLES
        SaturationValue = (float)ImgEditorParentNode.SaturationSlider.Value;
        BrightnessValue = (float)ImgEditorParentNode.BrightnessSlider.Value;
        ContrastValue = (float)ImgEditorParentNode.ContrastSlider.Value;
        Outline2DValue = (float)ImgEditorParentNode.Outline2DSlider.Value;
        Outline2DColor = ImgEditorParentNode.Outline2DColorPicker.Color;
        Inline2DValue = (float)ImgEditorParentNode.Inline2DSlider.Value;
        Inline2DColor = ImgEditorParentNode.Inline2DColorPicker.Color;
        ColorReductionIsOn = ImgEditorParentNode.ColorReductionCheckbox.ButtonPressed;
        ColorReducNumColors = (float)ImgEditorParentNode.ColorCountSpinBox.Value;

        //PALETTE AND PERSIST COLORS
        PersistColors = ImgEditorParentNode.PaletteLoaderFlow.PersistPaletteColors;
        UseExternalPalette = ImgEditorParentNode.PaletteLoaderFlow.UseExternalPaletteCheckBtn.ButtonPressed;
        ExternalPalette = ImgEditorParentNode.PaletteLoaderFlow.ExternalPaletteColors;
    }

    //Used by SAVEGAMEMANAGER to LOAD Data from a SaveGame file to the Game, Nodes, Scenes. 
    public void LoadSaveGameDataToNodes(SaveGameData saveGameDataToLoad, SpriteGenerator SpriteGenParentNode, ImageEditorMainPanel ImgEditorParentNode)
    {
        {

            SpriteGenParentNode.GenerateSpriteSheetCheckBtn.ButtonPressed = saveGameDataToLoad.CreateSpriteSheet;
            SpriteGenParentNode.GenerateSpriteSheetCheckBtn.Text = saveGameDataToLoad.CreateSpriteSheet.ToString();
            SpriteGenParentNode.SpriteSize = saveGameDataToLoad.SpriteSize;
            SpriteGenParentNode.PixelResolution = saveGameDataToLoad.PixelResolution;
            SpriteGenParentNode.SpriteSizeOptionButton.Selected = saveGameDataToLoad.SpriteSizeOptionButtonSelected;
            SpriteGenParentNode.PixelationLevelOptionBtn.Selected = saveGameDataToLoad.PixelResolutionButtonSelected;
            SpriteGenParentNode.EffectsChoicesOptionBtn.Selected = saveGameDataToLoad.EffectOptionSelected;
            SpriteGenParentNode.Outline3DValueSlider.Value = saveGameDataToLoad.Outline3DValue;
            SpriteGenParentNode.Outline3DBlendFactorSlider.Value = saveGameDataToLoad.Outline3DBlendValue;
            SpriteGenParentNode.Outline3DColorPicker.Color = saveGameDataToLoad.Outline3DColor;
            SpriteGenParentNode.DepthlineThicknessSlider.Value = saveGameDataToLoad.DepthLine3DValue;
            SpriteGenParentNode.DepthBlendValueSlider.Value = saveGameDataToLoad.DepthLine3DBlendValue;
            SpriteGenParentNode.DepthThresholdSlider.Value = saveGameDataToLoad.DepthLineThresholdValue;
            SpriteGenParentNode.Depthline3DColorPicker.Color = saveGameDataToLoad.DepthLine3DColor;

            SpriteGenParentNode.LoadAllAngles(saveGameDataToLoad.AnimationAngles);

            SpriteGenParentNode.FrameSkipStep = saveGameDataToLoad.FrameSkipStep;
            SpriteGenParentNode.MoveSpriteSheetCheckButton.ButtonPressed = saveGameDataToLoad.MoveSpriteSheet;
            SpriteGenParentNode.MoveSpriteSheetCheckButton.Text = saveGameDataToLoad.MoveSpriteSheet.ToString();
            SpriteGenParentNode.FrameStepTextEdit.Text = saveGameDataToLoad.FrameSkipStep.ToString();

            //MoveSpriteSheet = SpriteGenParentNode.FrameSkipStep;
            SpriteGenParentNode.MaxColorPaletteSpinBox.Value = saveGameDataToLoad.RGBLevels;
            SpriteGenParentNode.DitheringStrenghtSlider.Value = saveGameDataToLoad.DitheringValue;
            SpriteGenParentNode.ShowGridCheckButton.ButtonPressed = saveGameDataToLoad.ShowGrid;
            SpriteGenParentNode.ShowGridCheckButton.Text = saveGameDataToLoad.ShowGrid.ToString();

            SpriteGenParentNode.AnimMethodOptionBtn.Selected = saveGameDataToLoad.AnimationMethodSelected;

            //MODEL POSITION MANAGER VARIABLES AND CONFIG
            SpriteGenParentNode.ModelPositionManager.CamDistancelLineTextEdit.Text = saveGameDataToLoad.CameraDistance.ToString("0.0");
            SpriteGenParentNode.ModelPositionManager.CamXRotationLineTextEdit.Text = saveGameDataToLoad.CameraRotation.ToString("0.0");

            SpriteGenParentNode.ModelPositionManager.PosXAxisLineTextEdit.Text = saveGameDataToLoad.ModelPositionXAxis.ToString("0.0");
            SpriteGenParentNode.ModelPositionManager.PosYAxisLineTextEdit.Text = saveGameDataToLoad.ModelPositionYAxis.ToString("0.0");
            SpriteGenParentNode.ModelPositionManager.PosZAxisLineTextEdit.Text = saveGameDataToLoad.ModelPositionZAxis.ToString("0.0");

            SpriteGenParentNode.ModelPositionManager.RotationXAxisLineTextEdit.Text = saveGameDataToLoad.ModelRotationXAxis.ToString("0.0");
            SpriteGenParentNode.ModelPositionManager.RotationYAxisLineTextEdit.Text = saveGameDataToLoad.ModelRotationYAxis.ToString("0.0");
            SpriteGenParentNode.ModelPositionManager.RotationZAxisLineTextEdit.Text = saveGameDataToLoad.ModelRotationZAxis.ToString("0.0");

            //IMAGE EDITOR VARIABLES
            ImgEditorParentNode.SaturationSlider.Value = saveGameDataToLoad.SaturationValue;
            ImgEditorParentNode.BrightnessSlider.Value = saveGameDataToLoad.BrightnessValue;
            ImgEditorParentNode.ContrastSlider.Value = saveGameDataToLoad.ContrastValue;
            ImgEditorParentNode.Outline2DSlider.Value = saveGameDataToLoad.Outline2DValue;
            ImgEditorParentNode.Outline2DColorPicker.Color = saveGameDataToLoad.Outline2DColor;
            ImgEditorParentNode.Inline2DSlider.Value = saveGameDataToLoad.Inline2DValue;
            ImgEditorParentNode.Inline2DColorPicker.Color = saveGameDataToLoad.Inline2DColor;
            ImgEditorParentNode.ColorReductionCheckbox.ButtonPressed = saveGameDataToLoad.ColorReductionIsOn;
            ImgEditorParentNode.ColorCountSpinBox.Value = saveGameDataToLoad.ColorReducNumColors;

            //PALETTE AND PERSIST COLORS
            ImgEditorParentNode.PaletteLoaderFlow.PersistPaletteColors = saveGameDataToLoad.PersistColors;
            ImgEditorParentNode.PaletteLoaderFlow.UseExternalPaletteCheckBtn.ButtonPressed = saveGameDataToLoad.UseExternalPalette;
            ImgEditorParentNode.PaletteLoaderFlow.ExternalPaletteColors = saveGameDataToLoad.ExternalPalette;
        }
    }
}
