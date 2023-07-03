using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

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
    [HideInInspector] [SerializeField] private string path = "";

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
    public void SetTexture(Texture2D Texture) { 
        texture = Texture;
    }
    public Texture2D GetTextureToSearch() { return textureToSearch; }
    public void SetTextureToSearch() { texture = textureToSearch; }
    public string GetSubGroupRoute(){ return subGroupRoute; }
    public void SetSubGroupRoute(string SubGroupRoute){ subGroupRoute = SubGroupRoute; }
    public string GetPath(){ return path; }
    public void SetPath(){ 
        path = AssetDatabase.GetAssetPath(textureToSearch); 
    }


    //***********************************

    //System Management;
    public Texture2D LoadTextureData(string route, TexturesSO textures, bool saveAsPng) {
        //Sprite has to be located in the "Resources" folder;
        Sprite[] sprites = Resources.LoadAll<Sprite>(FormatRoute(route));
        string path = CreateFolder(textures.name, textures.GetSubGroupRoute());
        
        //Extracting limb data;
        foreach(Sprite sprite in sprites) {
            //Formatting the limb;
            Limb limb = new Limb();
            limb.SetName(sprite.name);

            Rect rect = sprite.rect;
            int x = (int) rect.x, y = (int) rect.y,
                w = (int) rect.width, h = (int) rect.height;

            limb.SetCoordinates(new Vector4(x, y, w, h));
            /* 0: X; Where the sprite box starts;
             * 1: Y; Where the sprite box ends;
             * 2: Width; Size of the texture;
             * 3: Height; Size of the texture; */

            //Sprite center;
            limb.SetPivot(new Vector2(sprite.pivot.x / w, sprite.pivot.y / h));
            //Generating the textures & mapping it to a transparent color;
            Texture2D texture = Utils.CreateTransparent2DTexture(w, h);

            //Mapping the limb to the texture;
            Color[] pixels = sprite.texture.GetPixels(x, y, w, h);
            texture.SetPixels(pixels);
            
            texture.Apply();

            // Saving the texture as an asset for later reference;
            string texturePath = path + "/" + sprite.name; // Specify the desired path and filename
            string assetPath = texturePath + ".asset";
            
            if(saveAsPng) SaveTexture(texturePath, texture, sprite, limb, saveAsPng);
            AssetDatabase.CreateAsset(texture, assetPath);

            //Saving the data;
            limb.SetTexture(AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath));
            textures.AddLimb(limb);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return Resources.Load<Texture2D>(route);
    }

    private void SaveTexture(string route, Texture2D texture, Sprite sprite, Limb limb, bool saveAsPng) {
        route += ".png";
        Texture2D png = Utils.CreateTransparent2DTexture(texture.width, texture.height);

        //Mapping the limb to the texture;
        Rect rect = sprite.rect;
        Color[] pixels = sprite.texture.GetPixels((int) rect.x, (int) rect.y, 
                                                  (int) rect.width, (int) rect.height);
        png.SetPixels(pixels);
        png.Apply();

        byte[] bytes = png.EncodeToPNG();
        File.Create(route).Dispose();
        File.WriteAllBytes(route, bytes);

        if(saveAsPng) limb.SetMaskRouteReference(route);
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

        for(int i = 0; i < groupSplit.Length; i++) {
            string folderName = groupSplit[i];
            folderPath += "/" + folderName;
            

            // If the folder has a valid path and name, create it
            string relativeFolderPath = baseRoute + folderPath;
            string folderCreationRoute = baseRoute + folderPath.Substring(0, folderPath.LastIndexOf('/'));
            string folderToReplace = folderCreationRoute + "/" + textureName;

            if (AssetDatabase.IsValidFolder(relativeFolderPath) && i == groupSplit.Length - 1) {
                AssetDatabase.DeleteAsset(folderToReplace);
            }

            if (!AssetDatabase.IsValidFolder(relativeFolderPath)){
                AssetDatabase.CreateFolder(folderCreationRoute, folderName);
                Debug.Log("Folder Created: " + folderName);
            }
        }

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
    private Color guiColor;
    //The colors folder will always be closed on opening as it is very memory intensive;
    private bool colorsFolder = false;
    //If the limb has different colors to mask, then false. If not, true;
    [SerializeField] private bool passLimbAsMask = true;
    [SerializeField] private List<Color> maskColors = new List<Color>();
    [SerializeField] private string maskRouteReference;
    [SerializeField] private Texture2D maskTextureReference;
    [SerializeField] private List<Texture2D> maskTextures;

    public Texture2D GetTexture() { return texture; }
    public void SetTexture(Texture2D _Texture) { texture = _Texture; }
    public string GetName() { return name; }
    public void SetName(string Name) { name = Name; }
    /* x: X; Where the sprite box starts;
     * y: Y; Where the sprite box ends;
     * z: Width; Size of the texture;
     * w: Height; Size of the texture; */
    public Vector4 GetCoordinates() { return coordinates; }
    public void SetCoordinates(Vector4 Coordinates) { coordinates = Coordinates; }
    public int GetX() { return (int) coordinates.x; }
    public void SetX(int X) { coordinates.x = X; }
    public int GetY() { return (int) coordinates.y; }
    public void SetY(int Y) { coordinates.y = Y; }
    public int GetWidth() { return (int) coordinates.z; }
    public void SetWidth(int Width) { coordinates.z = Width; }
    public int GetHeight() { return (int) coordinates.w; }
    public void SetHeight(int Height) { coordinates.w = Height; }
    public Vector2 GetPivot() { return pivot; }
    public void SetPivot(Vector2 Pivot) { pivot = Pivot; }
    public Color GetGUIColor() { return guiColor; }
    public void SetGUIColor(Color GUIColor) { guiColor = GUIColor; }
    public bool GetColorsFolder() { return colorsFolder; }
    public void SetColorsFolder(bool ColorsFolder) { colorsFolder = ColorsFolder; }
    public bool GetPassLimbAsMask() { return passLimbAsMask; }
    public void SetPassLimbAsMask(bool PassLimbAsMask) { passLimbAsMask = PassLimbAsMask; }
    public List<Color> GetMaskColors() { return maskColors; }
    public void SetMaskColors(List<Color> MaskColor) { maskColors = MaskColor; }
    public Color GetMaskColor(int Index) { return maskColors[Index]; }
    public void SetMaskColor(int Index, Color MaskColor) { maskColors[Index] = MaskColor; }
    public void AddMaskColor(Color @Color) { maskColors.Add(@Color); }
    public void RemoveMaskColor(Color @Color) { maskColors.Remove(@Color); }
    public void ClearMaskColors() { maskColors = new List<Color>(); }
    public string GetMaskRouteReference() { return maskRouteReference; }
    public void SetMaskRouteReference(string MaskRouteReference) { maskRouteReference = MaskRouteReference; }
    public Texture2D GetMaskReference() { return maskTextureReference; }
    public void SetMaskReference(Texture2D MaskTextureReference) { maskTextureReference = MaskTextureReference; }
    public void UpdateMaskTextureReference() {
        byte[] fileData = File.ReadAllBytes(maskRouteReference);
        maskTextureReference = new Texture2D(GetWidth(), GetHeight());
        maskTextureReference.LoadImage(fileData);
    }
    public List<Texture2D> GetMaskTextures() { return maskTextures; }
    public void SetMaskTextures(List<Texture2D> MaskTextures) { maskTextures = MaskTextures; }
    public void AddMaskTexture(Texture2D texture){ maskTextures.Add(texture); }
    public void RemoveMaskTexture(int index){ maskTextures.RemoveAt(index); }
    public void RemoveMaskTexture(Texture2D texture){ maskTextures.Remove(texture); }
    public void ClearMaskTextures(){ maskTextures = new List<Texture2D>(); }
}