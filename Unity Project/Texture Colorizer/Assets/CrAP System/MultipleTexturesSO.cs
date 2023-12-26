using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CreateAssetMenu(fileName = "MultipleTexturesSO", menuName = "ScriptableObjects/Multiple Texture Extractor SO", order = 1)]
/* TexturesSO helps saving and extracting the limb assets used
 * by a specific texture. Serializes the data in files for
 * later reference by the skeleton. */
public class MultipleTexturesSO : ScriptableObject {
    [HideInInspector] [SerializeField] private Texture2D texture;
    [SerializeField] private Texture2D[] texturesToSearch;
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
    public Texture2D GetSingleTexture(int index) { return texturesToSearch[index]; }
    public void SetSingleTexture(int index, Texture2D texture) { texturesToSearch[index] = texture; }
    public Texture2D[] GetTexturesToSearch() { return texturesToSearch; }
    /*public void SetTextureToSearch(int index) { texture = texturesToSearch[index]; }*/
    public string GetSubGroupRoute(){ return subGroupRoute; }
    public void SetSubGroupRoute(string SubGroupRoute){ subGroupRoute = SubGroupRoute; }
    public string GetPath(){ return path; }
    public string GetTexturePath(Texture2D texture){ return AssetDatabase.GetAssetPath(texture); }
    public void SetPath(){
        if(texturesToSearch.Length <= 0) return; 
        
        path = AssetDatabase.GetAssetPath(texturesToSearch[0]); 
    }


    //***********************************

    //System Management;
    public Texture2D LoadTextureData(MultipleTexturesSO textures, bool saveAsPng) {
        string path = CreateFolder(textures.name, textures.GetSubGroupRoute());
        
        foreach(Texture2D spriteTexture in textures.GetTexturesToSearch()){
            //Sprite has to be located in the "Resources" folder;
            Sprite[] sprites = Resources.LoadAll<Sprite>(FormatRoute(GetTexturePath(spriteTexture)));
            
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
                 * 3: Height; Size of the texture;*/

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

            
        }

        return Resources.Load<Texture2D>(GetPath());
        //return Resources.Load<Texture2D>(route);
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