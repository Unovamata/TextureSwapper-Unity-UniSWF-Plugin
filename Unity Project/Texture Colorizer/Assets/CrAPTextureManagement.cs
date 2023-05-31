using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CrAPTextureManagement : MonoBehaviour{
    //Data;
    public TexturesSO textures, textureToReference;
    public SkeletonSO skeleton;
    [HideInInspector] public Texture2D newTextureToManage;

    //Unity Editor;
    Material material;
    MeshRenderer mesh;


    ////////////////////////////////////////////////////////////////////

    
    void Start() {
        //Extracting the material;
        mesh = GetComponent<MeshRenderer>();
        material = mesh.material;
        
        //Creating a new texture based in the original one;
        newTextureToManage = CloneTexture(textures.GetTexture());
        
        //Assigning the new texture;
        material.mainTexture = newTextureToManage;
        mesh.material = material;

        //Circumvents the infinite material swapping provoked by UniSWF;
        enabled = false;
    }

    //Clone a reference texture;
    private Texture2D CloneTexture(Texture2D original) {
        Texture2D clone = new Texture2D(original.width, original.height);
        clone.SetPixels(original.GetPixels());
        clone.Apply();
        return clone;
    }


    ////////////////////////////////////////////////////////////////////


    void Update(){
        material.mainTexture = newTextureToManage;
        mesh.material = material;
    }
    
    //Loads data into a SkeletonSO object;
    public static Texture2D LoadSkeletonData(string route, TexturesSO skeleton) {
        //Sprite has to be located in the "Resources" folder;
        UnityEngine.Sprite[] sprites = Resources.LoadAll<UnityEngine.Sprite>(route);
        string path = CreateFolder(skeleton.name);

        //Extracting limb data;
        foreach(UnityEngine.Sprite sprite in sprites) {
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
            skeleton.AddLimb(limb);
        }

        return Resources.Load<Texture2D>(route);
    }

    //Create a folder to hold assets;
    public static string CreateFolder(string skeletonName) {
        string baseRoute = "Assets/Textures";
        string folderPath = baseRoute + "/" + skeletonName;

        //If the folder has a valid path and name, create it;
        if (!AssetDatabase.IsValidFolder(folderPath)) {
            AssetDatabase.CreateFolder(baseRoute, skeletonName);
            Debug.Log("Folder Created: " + skeletonName);
        }

        return folderPath;
    }

    //Remove a specific texture at a position;
    public void ClearTextureAt(int x, int y, int w, int h, Texture2D texture) {
        //Get transparent color;
        Color32[] pixels = FillArrayColor(Color.clear, w, h);

        //Save the converted pixels;
        texture.SetPixels32(x, y, w, h, pixels);
        texture.Apply();
    }


    //Paste a specific limb texture to the texture sheet;
    public void PasteTexture(Limb limb, Texture2D skeletonTexture, TexturesSO reference) {
        Vector4 coordinates = limb.GetCoordinates();
        int x = (int)coordinates.x, y = (int)coordinates.y, w = (int)coordinates.z, h = (int)coordinates.w;

        //Extracting the texture to paste from the limb referenced;
        Texture2D pasteTexture = reference.CallLimb(limb.GetName()).GetTexture();
        int sourceWidth = pasteTexture.width;
        int sourceHeight = pasteTexture.height;

        // Resize the source texture, if necessary;
        if (sourceWidth != w || sourceHeight != h){
            Texture2D resizedTexture = new Texture2D(w, h);
            Color32[] resizedPixels = new Color32[w * h];

            //Round the pixels of the image to fit it into the new texture;
            for (int i = 0; i < h; i++){
                for (int j = 0; j < w; j++){
                    int sourceX = Mathf.RoundToInt(j * (sourceWidth / (float)w));
                    int sourceY = Mathf.RoundToInt(i * (sourceHeight / (float)h));
                    resizedPixels[i * w + j] = pasteTexture.GetPixel(sourceX, sourceY);
                }
            }

            resizedTexture.SetPixels32(resizedPixels);
            resizedTexture.Apply();

            // Copy the resized texture to the target texture
            Graphics.CopyTexture(resizedTexture, 0, 0, 0, 0, w, h, skeletonTexture, 0, 0, x, y);
        }
        else{
            // If not, just paste it;
            Graphics.CopyTexture(pasteTexture, 0, 0, 0, 0, sourceWidth, sourceHeight, skeletonTexture, 0, 0, x, y);
        }

        skeletonTexture.Apply();
    }

    //Fill a color32 array with a specific color;
    public static Color32[] FillArrayColor(Color color, int w, int h) {
        Color32[] pixels = new Color32[w * h];
        for(int i = 0; i < pixels.Length; i++) {
            pixels[i] = color;
        }

        return pixels;
    }
}

////////////////////////////////////////////////////////////////////

//Custom Editor;
[CustomEditor(typeof(CrAPTextureManagement))]
public class CustomInspector : Editor {
    int currentRelationIndex = 0;

    //Changes values in the inspector, creating a custom inspector;
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        CrAPTextureManagement manager = (CrAPTextureManagement) target; //Referencing the script;
        List<SkeletonRelationships> relationships = manager.skeleton.GetRelationships();


        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Texture Management", EditorStyles.boldLabel);

        try {
            GUILayout.BeginVertical(GUI.skin.box);
            SkeletonRelationships relationship = relationships[currentRelationIndex];

            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("<<", GUILayout.Width(40))) {
                if(currentRelationIndex <= 0) currentRelationIndex = relationships.Count - 1;
                else currentRelationIndex--;
            }

            if(GUILayout.Button(relationship.GetRelationshipName())) {
                //manager.ClearTextureAt(x, y, w, h, manager.newTextureToManage);
            }

            if(GUILayout.Button(">>", GUILayout.Width(40))) {
                if(currentRelationIndex >= relationships.Count - 1) currentRelationIndex = 0;
                else currentRelationIndex++;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if(GUILayout.Button("Clear " + relationship.GetRelationshipName() + " Texture Group")) {
                foreach(Limb limb in relationship.GetLimbsRelated()) {
                    string name = limb.GetName();
                    Vector4 coordinates = limb.GetCoordinates();
                    int x = (int)coordinates.x;
                    int y = (int)coordinates.y;
                    int w = (int)coordinates.z;
                    int h = (int)coordinates.w;

                    manager.ClearTextureAt(x, y, w, h, manager.newTextureToManage);
                }
            }

            if(GUILayout.Button("Paste " + relationship.GetRelationshipName() + " Texture Group")) {
                foreach(Limb limb in relationship.GetLimbsRelated()) {
                    string name = limb.GetName();
                    Vector4 coordinates = limb.GetCoordinates();
                    int x = (int)coordinates.x;
                    int y = (int)coordinates.y;
                    int w = (int)coordinates.z;
                    int h = (int)coordinates.w;

                    manager.PasteTexture(limb, manager.newTextureToManage, manager.textureToReference);
                }
            }

            /*foreach(SkeletonRelationships relationship in relationships) {
                if(GUILayout.Button(relationship.GetRelationshipName())) {
                    //manager.ClearTextureAt(x, y, w, h, manager.newTextureToManage);
                }

                string name = limb.GetName();
                Vector4 coordinates = limb.GetCoordinates();
                int x = (int)coordinates.x;
                int y = (int)coordinates.y;
                int w = (int)coordinates.z;
                int h = (int)coordinates.w;

                if(GUILayout.Button("Delete " + name)) {
                    manager.ClearTextureAt(x, y, w, h, manager.newTextureToManage);
                }

                if(GUILayout.Button("Paste " + name)) {
                    manager.PasteTexture(limb, manager.newTextureToManage, manager.textureToReference);
                }
            }*/
            GUILayout.EndVertical();
        } catch { }
    }
}