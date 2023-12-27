using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public interface ITexturesSO {
    public Texture2D LoadTextureData(TexturesSO textures, bool saveAsPng);
    public void SetPath();
}

/* TexturesSO helps saving and extracting the limb assets used
 * by a specific texture. Serializes the data in files for
 * later reference by the skeleton. */

public class TexturesSO : ScriptableObject, ITexturesSO {
    [HideInInspector] [SerializeField] protected Texture2D texture;
    [SerializeField] private string subGroupRoute = "";
    [HideInInspector] [SerializeField] private List<Limb> limbs = new List<Limb>();
    [SerializeField] HashSet<string> hashSet;
    [HideInInspector] [SerializeField] protected string path = "";

    //Making data save consistently;
    public void OnEnable() {
        EditorUtility.SetDirty(this);
    }

    public void OnValidate() {
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
    public string GetSubGroupRoute(){ return subGroupRoute; }
    public void SetSubGroupRoute(string SubGroupRoute){ subGroupRoute = SubGroupRoute; }
    public string GetPath(){ return path; }
    public string GetTexturePath(Texture2D texture){ return AssetDatabase.GetAssetPath(texture); }

    public void SaveTexture(string route, Texture2D texture, Sprite sprite, Limb limb, bool saveAsPng) {
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

    public Texture2D LoadTextureData(TexturesSO textures, bool saveAsPng){
        return new Texture2D(1, 1);
    }

    public void SetPath(){}
}
