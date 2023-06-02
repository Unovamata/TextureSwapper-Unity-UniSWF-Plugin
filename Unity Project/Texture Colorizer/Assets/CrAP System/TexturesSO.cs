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
    [SerializeField] private Texture2D textureToSearch;
    [SerializeField] private string subGroupRoute = "";
    [HideInInspector] [SerializeField] private List<Limb> limbs = new List<Limb>();
    [SerializeField] HashSet<string> hashSet;

    //Making data save consistently;
    private void OnEnable() {
        EditorUtility.SetDirty(this);
    }

    private void OnValidate() {
        AssetDatabase.SaveAssets();
    }

    public List<Limb> GetLimbs(){ return limbs; }
    public void ClearLimbs(){ limbs.Clear(); }
    public void AddLimb(Limb limb){ limbs.Add(limb); }
    public Limb CallLimb(string name){ return limbs.Find(limbSearcher => limbSearcher.GetName() == name); }
    public void SetLimbs(List<Limb> Limbs){ limbs = Limbs; }
    public Texture2D GetTexture() { return texture; }
    public void SetTexture(Texture2D Texture) { texture = Texture; }
    public Texture2D GetTextureToSearch() { return textureToSearch; }
    public void SetTextureToSearch(Texture2D TextureToSearch) { textureToSearch = TextureToSearch; }
    public string GetSubGroupRoute(){ return subGroupRoute; }
    public void SetSubGroupRoute(string SubGroupRoute){ subGroupRoute = SubGroupRoute; }

    //***********************************

    //System Management;
    public Texture2D LoadTextureData(string route, TexturesSO textures) {
        //Sprite has to be located in the "Resources" folder;
        Sprite[] sprites = Resources.LoadAll<Sprite>(FormatRoute(route));
        string path = CreateFolder(textures.name, textures.GetSubGroupRoute());

        //Extracting limb data;
        foreach(Sprite sprite in sprites) {
            //Formatting the limb;
            Limb limb = new Limb();
            limb.SetName(sprite.name);
            int w = (int) sprite.rect.width, h = (int) sprite.rect.height;

            limb.SetCoordinates(new Vector4(sprite.rect.x, sprite.rect.y, w, h));
            /* 0: X; Where the sprite box starts;
             * 1: Y; Where the sprite box ends;
             * 2: Width; Size of the texture;
             * 3: Height; Size of the texture; */

            //Sprite center;
            limb.SetPivot(new Vector2(sprite.pivot.x / w, sprite.pivot.y / h));

            //Generating the textures & mapping it to a transparent color;
            Texture2D texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color32[] emptyTexturePixels = new Color32[w * h];
            Color32 transparentColor = new Color32(0, 0, 0, 0);
            System.Array.Fill(emptyTexturePixels, transparentColor);
            texture.SetPixels32(emptyTexturePixels);
            texture.Apply();

            //Mapping the limb to the texture;
            Rect rect = sprite.textureRect;
            Color[] pixels = sprite.texture.GetPixels((int) rect.x, (int) rect.y, (int) rect.width, (int) rect.height);
            texture.SetPixels(pixels);
            
            texture.Apply();

            // Saving the texture as an asset for later reference;
            string texturePath = path + "/" + sprite.name + ".asset"; // Specify the desired path and filename
            AssetDatabase.CreateAsset(texture, texturePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //Saving the data;
            limb.SetTexture(AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
            textures.AddLimb(limb);
        }

        return Resources.Load<Texture2D>(route);
    }

    //Create a readable route by the asset database;
    public string FormatRoute(string route) {
        //Removing the first part of the route;
        route = route.Replace("Assets/Resources/", "");

        //Removing the .extension part of the route;
        route = route.Substring(0, route.IndexOf("."));

        return route;
    }

    //Create a folder to hold assets;
    public string CreateFolder(string textureName, string groupRoute){
        string baseRoute = "Assets/Resources/Skeletons";
        string[] groupSplit;
        string folderPath = "";

        // If the group doesn't have a route
        if (groupRoute.Equals("")){
            groupSplit = textureName.Split("/");
        }
        else{
            groupSplit = (groupRoute + "/" + textureName).Split("/");
        }

        foreach (string folderName in groupSplit){
            folderPath += "/" + folderName;

            // If the folder has a valid path and name, create it
            string relativeFolderPath = baseRoute + folderPath;
            if (!AssetDatabase.IsValidFolder(relativeFolderPath)){
                AssetDatabase.CreateFolder(baseRoute + folderPath.Substring(0, folderPath.LastIndexOf('/')), folderName);
                Debug.Log("Folder Created: " + folderName);
            }
        }

        Debug.Log(baseRoute + folderPath);
        return baseRoute + folderPath;
    }
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
        string subGroupName = textures.GetSubGroupRoute();

        //Naming a subgroup of folders if needed;
        if (!subGroupName.Equals("") && textures.GetLimbs().Count == 0) {
            EditorGUILayout.HelpBox("Specify this field ONLY IF the texture belongs to a subgroup of a main group.", MessageType.Info);
        }

        //Removing the "/" character as it can generate conflicts between routes;
        if (subGroupName.EndsWith("/")){
            textures.SetSubGroupRoute(subGroupName.Substring(0, subGroupName.Length - 1));
        }

        // Display a label for a property
        EditorGUILayout.LabelField("Skeleton Scriptable Object Data", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(textures.GetLimbs().Count + " Limbs found");
        EditorGUILayout.HelpBox("Ensure the texture is placed in the 'Resources' folder at the project's root for the script to work correctly.", MessageType.Warning);
        EditorGUILayout.Space();

        //Confirm search and load button;
        if (GUILayout.Button("Load Limb Data From Texture")){
            textures.ClearLimbs();
            textures.SetTexture(textures.LoadTextureData(AssetDatabase.GetAssetPath(textures.GetTextureToSearch()), textures));
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