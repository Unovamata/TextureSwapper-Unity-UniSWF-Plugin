using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CreateAssetMenu(fileName = "TexturesSO", menuName = "ScriptableObjects/Texture Extractor SO", order = 1)]
/* TexturesSO helps saving and extracting the limb assets used
 * by a specific texture. Serializes the data in files for
 * later reference by the skeleton. */
public class TexturesSO : ScriptableObject {
    [HideInInspector] [SerializeField] private Texture2D texture;
    public string textureToSearch;
    [HideInInspector] [SerializeField] private List<Limb> limbs = new List<Limb>();
    [SerializeField] HashSet<string> hashSet;

    //Making data save consistently;
    private void OnEnable() {
        EditorUtility.SetDirty(this);
    }

    public List<Limb> GetLimbs(){ return limbs; }
    public void ClearLimbs(){ limbs.Clear(); }
    public void AddLimb(Limb limb){ limbs.Add(limb); }
    public Limb CallLimb(string name){ return limbs.Find(limbSearcher => limbSearcher.GetName() == name); }
    public void SetLimbs(List<Limb> Limbs){ limbs = Limbs; }
    public Texture2D GetTexture() { return texture; }
    public void SetTexture(Texture2D Texture) { texture = Texture; }
}


////////////////////////////////////////////////////////////////////

/* The Limb class is mandatory for all dependencies of the 
 * texture swapping system, as they store all the pertinent
 * information to iterate over multiple limbs if needed. */
[System.Serializable]
public class Limb {
    [SerializeField] private Texture2D texture;
    [SerializeField] private string name;
    [SerializeField] private Vector4 coordinates;
    [SerializeField] private Vector2 pivot;

    public Texture2D GetTexture() { return texture; }
    public void SetTexture(Texture2D _Texture) { texture = _Texture; }
    public string GetName() { return name; }
    public void SetName(string Name) { name = Name; }
    /* 0: X; Where the sprite box starts;
     * 1: Y; Where the sprite box ends;
     * 2: Width; Size of the texture;
     * 3: Height; Size of the texture; */
    public Vector4 GetCoordinates() { return coordinates; }
    public void SetCoordinates(Vector4 Coordinates) { coordinates = Coordinates; }
    public Vector2 GetPivot() { return pivot; }
    public void SetPivot(Vector2 Pivot) { pivot = Pivot; }
}


////////////////////////////////////////////////////////////////////

//Custom Editor;
[CustomEditor(typeof(TexturesSO))] // Replace with the name of your ScriptableObject class
public class TexturesSOEditor : Editor{
    public override void OnInspectorGUI(){
        TexturesSO textures = (TexturesSO) target;
        base.OnInspectorGUI();

        // Display a label for a property
        EditorGUILayout.LabelField("Skeleton Scriptable Object Data", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(textures.GetLimbs().Count + " Limbs found");
        EditorGUILayout.HelpBox("Textures must be in the 'Resources' folder at the project's root for the texture compiling script to function properly.", MessageType.Warning);
        EditorGUILayout.Space();

        //Confirm search and load button;
        if (GUILayout.Button("Load Limb Data From Texture")){
            textures.ClearLimbs();
            textures.SetTexture(CrAPTextureManagement.LoadSkeletonData(textures.textureToSearch, textures));
        }

        EditorGUILayout.Space();

        try {
            foreach(Limb limb in textures.GetLimbs()) {
                LoadLimbInGUI(limb, 2, true);
            }
        } catch { }
    }

    //Loads all the needed data in the GUI;
    public static void LoadLimbInGUI(Limb limb, float rectSize, bool showMetadata) {
        int boxHeight = 20;
        string name = limb.GetName();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(name, EditorStyles.boldLabel);

        if(showMetadata){
            //Coordinates;
            Vector4 coordinates = limb.GetCoordinates();
            //Sprite boxes;
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
        }
        EditorGUILayout.Space();

        // Display the label and read-only texture field
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Texture");
        Rect objectFieldRect = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth * rectSize, EditorGUIUtility.fieldWidth * rectSize);

        EditorGUI.BeginDisabledGroup(true);
        EditorGUI.ObjectField(objectFieldRect, GUIContent.none, limb.GetTexture(), typeof(Texture2D), false);
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
}