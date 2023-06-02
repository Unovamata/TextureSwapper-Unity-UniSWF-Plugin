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

    //Remove a specific texture at a position;
    public void ClearTextureAt(Vector4 v, Texture2D texture) {
        //Get transparent color;
        Color32[] pixels = FillArrayColor(Color.clear, (int) v.z, (int) v.w);

        //Save the converted pixels;
        texture.SetPixels32((int) v.x, (int) v.y, (int) v.z, (int) v.w, pixels);
        texture.Apply();
    }


    //Paste a specific limb texture to the texture sheet;
    public void PasteTexture(Limb limb, Texture2D skeletonTexture, TexturesSO reference) {
        //If there are limbs missing, return; 
        if(!reference.GetLimbs().Any(x => x.GetName().Equals(limb.GetName()))) return;

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
                    manager.ClearTextureAt(limb.GetCoordinates(), manager.newTextureToManage);
                }
            }

            if(GUILayout.Button("Paste " + relationship.GetRelationshipName() + " Texture Group")) {
                foreach(Limb limb in relationship.GetLimbsRelated()) {
                    manager.PasteTexture(limb, manager.newTextureToManage, manager.textureToReference);
                }
            }
            GUILayout.EndVertical();
        } catch { }
    }
}