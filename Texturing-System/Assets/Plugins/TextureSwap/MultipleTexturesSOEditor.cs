using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

////////////////////////////////////////////////////////////////////

//Custom Editor;
[CustomEditor(typeof(MultipleTexturesSO))] // Replace with the name of your ScriptableObject class
public class MultipleTexturesSOEditor : Editor{
    public bool saveTextureAsPng = false;

    public override void OnInspectorGUI(){
        
        MultipleTexturesSO textures = (MultipleTexturesSO) target;

        if(textures.GetLimbs().Count == 0)
            EditorGUILayout.HelpBox("- Ensure the texture is located anywhere within the 'Resources' folder in the project's root.", MessageType.Warning);
        EditorGUILayout.Space();

        base.OnInspectorGUI();
        string subGroupName = textures.GetSubGroupRoute();

        try { textures.SetPath(); } catch {}

        //Naming a subgroup of folders if needed;
        if (!subGroupName.Equals("") && textures.GetLimbs().Count == 0) {
            EditorGUILayout.HelpBox("Specify this field ONLY IF the texture belongs to a subgroup of a main group.", MessageType.Info);
        }

        //Removing the "/" character as it can generate conflicts between routes;
        if (subGroupName.EndsWith("/")){
            textures.SetSubGroupRoute(subGroupName.Substring(0, subGroupName.Length - 1));
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Save Texture Assets + PNGs?", GUILayout.Width(250));
        saveTextureAsPng = EditorGUILayout.Toggle(saveTextureAsPng);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        // Display a label for a property
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Skeleton Scriptable Object Data:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(textures.GetLimbs().Count + " Limbs found");
        EditorGUILayout.EndHorizontal();

        //Confirm search and load button;
        if (GUILayout.Button("Load Limb Data From Texture")){
            textures.ClearLimbs();
            textures.LoadTextureData(textures, saveTextureAsPng);
        }

        //Clearing padding artifacts;
        if (GUILayout.Button("Clean Texture Padding Artifacts")){
            foreach(Limb limb in textures.GetLimbs()) {
                Texture2D source = ConvertDXT5ToRGBA32(limb.GetSourceTexture());

                int padding = (int) Prefs.padding / 2;

                if(padding == 0) return;

                int x = limb.GetX(), y = limb.GetY(), w = limb.GetWidth(), h = limb.GetHeight();

                Utils.ClearTextureAt(new Vector4(x, y + h - padding, w, padding), source);
                Utils.ClearTextureAt(new Vector4(x + w - padding, y, padding, h), source);
                Utils.ClearTextureAt(new Vector4(x, y, w, padding), source);
                Utils.ClearTextureAt(new Vector4(x, y, padding, h), source);

                Utils.SaveTexture(source, textures.GetTexturePath(limb.GetSourceTexture()));
            }
        }

        /*if(GUILayout.Button("Generate Texture Masks")) {
            PythonEngine.Initialize();

            using (Py.GIL()){
                Utils.PythonGoToFolder();

                dynamic sys = Py.Import("sys");

                try {
                    dynamic script = Py.Import("generateMasks");
                    string imagePath = @"C:\Users\Administrator\Documents\GitHub\LPSO-Revived-UniSWF-Texture-Colorizer\Unity Project\Texture Colorizer\Assets\Resources\Skeletons\Kitty\Kitty1\Ear Left.png";

                    dynamic PIL = Py.Import("PIL.Image");
                    dynamic image = PIL.open(imagePath);

                    dynamic mask = script.CreateMask(image, new object[] { 207, 207, 207 }, 42, 0);
                } catch (PythonException ex){ /*Utils.PythonErrorHandling(ex); }
            }

            PythonEngine.Shutdown();
        }*/
    
        EditorGUILayout.Space();

        GUILayout.Label("Limb List:", EditorStyles.boldLabel);

        bool showMaskColorMetadata = false;

        if (showMaskColorMetadata) {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField("Data in Clipboard:");
            ColorAndThresholdSetter(clipboardColor, clipboardThreshold);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();
        }

        try {
            foreach(Limb limb in textures.GetLimbs()) {
                LoadLimbInGUI(limb, 2, true);
            }
        } catch { }
    }

    public static Color clipboardColor = new Color();
    public static int clipboardThreshold = 0;

    public Texture2D ConvertDXT5ToRGBA32(Texture2D dxt5Texture){
        Texture2D rgba32Texture = new Texture2D(dxt5Texture.width, dxt5Texture.height, TextureFormat.RGBA32, false);
        rgba32Texture.SetPixels32(dxt5Texture.GetPixels32());
        rgba32Texture.Apply();
        return rgba32Texture;
    }

    //Loads all the needed data in the GUI;
    public static void LoadLimbInGUI(Limb limb, float rectSize, bool showMetadata) {
        int boxHeight = 20;
        string name = limb.GetName();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(name, EditorStyles.boldLabel);

        // Display the label and read-only texture field
        ShowTextureInField(limb.GetTexture(), "Main Texture", rectSize);

        try{
            ShowTextureInField(limb.GetSourceTexture(), limb.GetSourceTexture().name, 1);
        } catch {}

        if (!showMetadata) {
            EditorGUILayout.EndVertical();
            return;
        }

        bool showDetailedMetadata = (int)Prefs.showDetailedMetadata == 1;

        if (showDetailedMetadata) {
            //Coordinates;
            Vector4 coordinates = limb.GetCoordinates();

            //Sprite boxes;
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sprite Box Start", GUILayout.Width(100));
            EditorGUILayout.SelectableLabel(coordinates.x.ToString(), EditorStyles.textField, GUILayout.Height(boxHeight));

            EditorGUILayout.LabelField("Sprite Box End", GUILayout.Width(100));
            EditorGUILayout.SelectableLabel(coordinates.y.ToString(), EditorStyles.textField, GUILayout.Height(boxHeight));
            EditorGUILayout.EndHorizontal();

            //Width / Height;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Width", GUILayout.Width(100));
            EditorGUILayout.SelectableLabel(coordinates.z.ToString(), EditorStyles.textField, GUILayout.Height(boxHeight));

            EditorGUILayout.LabelField("Height", GUILayout.Width(100));
            EditorGUILayout.SelectableLabel(coordinates.w.ToString(), EditorStyles.textField, GUILayout.Height(boxHeight));
            
            EditorGUILayout.EndHorizontal();

            //Coordinates;
            Vector2 pivot = limb.GetPivot();
            //Pivot;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Pivot X", GUILayout.Width(100));
            EditorGUILayout.SelectableLabel(pivot.x.ToString(), EditorStyles.textField, GUILayout.Height(boxHeight));

            EditorGUILayout.LabelField("Pivot Y", GUILayout.Width(100));
            EditorGUILayout.SelectableLabel(pivot.y.ToString(), EditorStyles.textField, GUILayout.Height(boxHeight));
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();
        }

        //Texture Mask;
        //ShowTextureInField(limb.GetMaskReference(), "Mask Texture Reference", rectSize);

        //Mask Colors;
        ShowMaskColorManagement(limb);

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();
    }

    public static void ShowTextureInField(Texture2D texture, string textureName, float rectSize){
        bool showMaskTextureMetadata = (int) Prefs.showMaskColorMetadata == 1;
        if(!showMaskTextureMetadata) return;

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        // Display the label and read-only texture field
        EditorGUILayout.PrefixLabel(textureName);
        Rect objectFieldRect = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth * rectSize, EditorGUIUtility.fieldWidth * rectSize);

        EditorGUI.BeginDisabledGroup(true);
        EditorGUI.ObjectField(objectFieldRect, GUIContent.none, texture, typeof(Texture2D), false);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }


    public static float rectangleSize = 20f;
    public static float colorRectangleSize = 60f;
    public static int thresholdInputValue = 0;
    public static Color colorInputValue = new Color();
    public static List<LimbColor> clipboardColorList = new List<LimbColor>();

    public static void ShowMaskColorManagement(Limb limb) {
        //If the GUI should not check for color metadata;
        bool showMaskColorMetadata = (int) Prefs.showMaskTextureMetadata == 1;
        if (!showMaskColorMetadata) return;

        //Folder;
        limb.SetColorsFolder(EditorGUILayout.Foldout(limb.GetColorsFolder(), "Mask Color Management"));
        if (!limb.GetColorsFolder()) return;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Pass Entire Texture as a Mask?");
        limb.SetPassLimbAsMask(EditorGUILayout.Toggle(limb.GetPassLimbAsMask()));
        EditorGUILayout.EndHorizontal();

        if (limb.GetPassLimbAsMask()) return;
        EditorGUILayout.HelpBox("Assign colors to the list below to indicate different parts of the texture. Python will use these colors to separate the masks accordingly.", MessageType.Info);

        //Mask Colors;
        EditorGUILayout.Space();
        List<LimbColor> colorList = limb.GetMaskColors();

        if(colorList.Count > 0) EditorGUILayout.LabelField("Color & Threshold");

        for(int i = 0; i < colorList.Count; i++) {
            LimbColor limbColor = limb.GetMaskColor(i);
            EditorGUILayout.BeginHorizontal();

            limbColor.SetColor(EditorGUILayout.ColorField(limbColor.GetColor()));
            limbColor.SetThreshold(EditorGUILayout.IntField(limbColor.GetThreshold()));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Remove", GUILayout.Width(colorRectangleSize))) {
                limb.RemoveAtMaskColor(i);
            }

            //Copy color;
            if (GUILayout.Button("Copy", GUILayout.Width(colorRectangleSize))) {
                clipboardColor = limbColor.GetColor();
                clipboardThreshold = limbColor.GetThreshold();
            }

            //Paste color;
            if (GUILayout.Button("Paste", GUILayout.Width(colorRectangleSize))) {
                bool hasColor = colorList.Any(x => x.GetColor().Equals(clipboardColor));

                if(!hasColor) {
                    LimbColor referenceLimbColor = colorList.ElementAt(i);
                    referenceLimbColor.SetColor(clipboardColor);
                    referenceLimbColor.SetThreshold(clipboardThreshold);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        //Color picker and threshold setter;
        EditorGUILayout.LabelField("Input Color & Threshold");
        EditorGUILayout.BeginHorizontal();
        colorInputValue = EditorGUILayout.ColorField(new Color(colorInputValue.r, colorInputValue.g, colorInputValue.b, 1));
        thresholdInputValue = EditorGUILayout.IntField(thresholdInputValue);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Add")) {
            bool hasColor = colorList.Any(x => x.GetColor().Equals(colorInputValue));
            if(!hasColor) {
                LimbColor newLimbColor = new LimbColor(colorInputValue, thresholdInputValue);
                limb.AddMaskColor(newLimbColor);
                colorInputValue = new Color(0, 0, 0, 1);
            }
        }

        if (GUILayout.Button("Copy All")) {
            clipboardColorList = colorList;
        }

        if (GUILayout.Button("Paste New")) {
            limb.SetMaskColors(clipboardColorList);
        }

        if (GUILayout.Button("Paste Ref")) {
            limb.SetMaskColors(clipboardColorList);
        }

        if (GUILayout.Button("Clear")) {
            limb.ClearMaskColors();
        }

        EditorGUILayout.EndHorizontal();
    }

    public static void ColorAndThresholdSetter(Color color, int threshold) {
        EditorGUILayout.BeginHorizontal();
        color = EditorGUILayout.ColorField(color);
        threshold = EditorGUILayout.IntField(threshold);
        EditorGUILayout.EndHorizontal();
    }
}