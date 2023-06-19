using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TextureManagement : MonoBehaviour{
    //Data;
    public TexturesSO textures;
    public TexturesSO[] textureToReference;
    public SkeletonSO skeleton;
    List<SkeletonRelationships> skeletonRelationships;
    [HideInInspector] public Texture2D newTextureToManage;

    //Unity Editor;
    Material material;
    MeshRenderer mesh;
    Vector2[] originalUVs;

    ////////////////////////////////////////////////////////////////////

    
    void Start() {
        //Extracting the material;
        mesh = GetComponent<MeshRenderer>();
        material = mesh.material;
        
        //Creating a new texture based in the original one;
        newTextureToManage = Utils.CloneTexture(textures.GetTexture());
        
        //Assigning the new texture;
        material.mainTexture = newTextureToManage;
        mesh.material = material;

        //Circumvents the infinite material swapping provoked by UniSWF;
        enabled = false;
    }


    ////////////////////////////////////////////////////////////////////


    void Update(){
        material.mainTexture = newTextureToManage;
        mesh.material = material;

        //PREVIEW CODE ONLY, DO NOT USE IN PRODUCTION, IT IS VERY TAXING ON MEMORY;
        /*SkeletonRelationships Head, Eyes, Ears, Mouth, Nose, Tuft;

        if(!isCouroutineRunning) {
            TexturesSO texture = textureToReference[Random.Range(0, textureToReference.Length)];
            Head = skeleton.GetRelationships().FirstOrDefault(x => x.GetRelationshipName() == "Head");
            Eyes = skeleton.GetRelationships().FirstOrDefault(x => x.GetRelationshipName() == "Eyes");
            Ears = skeleton.GetRelationships().FirstOrDefault(x => x.GetRelationshipName() == "Ears");
            Mouth = skeleton.GetRelationships().FirstOrDefault(x => x.GetRelationshipName() == "Mouth");
            Nose = skeleton.GetRelationships().FirstOrDefault(x => x.GetRelationshipName() == "Nose");
            Tuft = skeleton.GetRelationships().FirstOrDefault(x => x.GetRelationshipName() == "Tuft");

            StartCoroutine(RandomTextureSwap(Head, Ears, Mouth, Nose, Tuft, Eyes));
        }*/
    }

    //PREVIEW CODE ONLY, DO NOT USE IN PRODUCTION, IT IS VERY TAXING ON MEMORY;
    /*private bool isCouroutineRunning = false;

    private IEnumerator RandomTextureSwap(params SkeletonRelationships[] limbs){
        isCouroutineRunning = true;

        for(int i = 0; i < limbs.Length; i++) {
            int textureType = Random.Range(0, textureToReference.Length - 1);

            foreach(Limb limb in limbs[i].GetLimbsRelated()) {
                Utils.ClearTextureAt(limb.GetCoordinates(), newTextureToManage);
                Utils.PasteTexture(limb, newTextureToManage, 
                    textureToReference[textureType]);
            }
        }

        yield return new WaitForSeconds(4f);
        isCouroutineRunning = false;
    }*/
}

////////////////////////////////////////////////////////////////////

//Custom Editor;
[CustomEditor(typeof(TextureManagement))]
public class CustomInspector : Editor {
    int currentRelationIndex = 0;
    int currentSelectedTextureToReference = 0;

    //Changes values in the inspector, creating a custom inspector;
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        TextureManagement manager = (TextureManagement) target; //Referencing the script;
        List<SkeletonRelationships> relationships = manager.skeleton.GetRelationships();


        EditorGUILayout.Space();

        if(manager.textureToReference.Length == 0) return;

        EditorGUILayout.LabelField("Texture Management", EditorStyles.boldLabel);

        try {
            GUILayout.BeginVertical(GUI.skin.box);
            SkeletonRelationships relationship = relationships[currentRelationIndex];

            EditorGUILayout.LabelField("Body Part:");
            currentRelationIndex = Scrollable(relationship.GetRelationshipName(), 0, 
                relationships.Count - 1, currentRelationIndex);

            EditorGUILayout.LabelField("Texture Set:");
            TexturesSO selectedTexture = manager.textureToReference[currentSelectedTextureToReference];
            string selectedName = selectedTexture.name + " (" + currentSelectedTextureToReference + ")";
            currentSelectedTextureToReference = Scrollable(selectedName, 0, 
                manager.textureToReference.Length - 1, currentSelectedTextureToReference);


            EditorGUILayout.Space();

            if(GUILayout.Button("Swap " + relationship.GetRelationshipName() + " Textures")) {
                foreach(Limb limb in relationship.GetLimbsRelated()) {
                    Utils.ClearTextureAt(limb.GetCoordinates(), manager.newTextureToManage);
                    Utils.PasteTexture(limb, manager.newTextureToManage, 
                        manager.textureToReference[currentSelectedTextureToReference]);
                }
            }

            GUILayout.EndVertical();
        } catch { }
    }

    private int Scrollable(string name, int min, int max, int index) {
        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("<<", GUILayout.Width(40))) {
            if(index <= min) index = max;
            else index--;
        }

        GUILayout.Button(name);

        if(GUILayout.Button(">>", GUILayout.Width(40))) {
            if(index >= max) index = min;
            else index++;
        }
        EditorGUILayout.EndHorizontal();

        return index;
    }
}