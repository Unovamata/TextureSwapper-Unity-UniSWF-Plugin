using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CreateAssetMenu(fileName = "BaseTexturesSO", menuName = "ScriptableObjects/Texture Extractor SO", order = 1)]
/* BaseTexturesSO helps saving and extracting the limb assets used
 * by a specific texture. Serializes the data in files for
 * later reference by the skeleton. */
public class BaseTexturesSO : TexturesSO {
    [SerializeField] private Texture2D textureToSearch;

    //Making data save consistently;
    private void OnEnable() {
        base.OnEnable();
    }

    private void OnValidate() {
        base.OnValidate();
    }

    public Texture2D GetTextureToSearch() { return textureToSearch; }
    public void SetPath(){ 
        
        texture = textureToSearch;
        path = AssetDatabase.GetAssetPath(textureToSearch); 
    }


    //***********************************

    //System Management;
    public Texture2D LoadTextureData(BaseTexturesSO textures, bool saveAsPng) {
        string route = textures.GetPath();
        
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
}


////////////////////////////////////////////////////////////////////