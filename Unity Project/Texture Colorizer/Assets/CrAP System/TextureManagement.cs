using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

public class TextureManagement : MonoBehaviour{
    //Data;
    public TexturesSO textures;
    public TexturesSO[] textureToReference;
    [HideInInspector] public int[] limitPerLimb;
    public SkeletonSO skeleton;
    List<SkeletonRelationships> skeletonRelationships;
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
    bool folderLimitStatus = true;

    //Changes values in the inspector, creating a custom inspector;
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        TextureManagement manager = (TextureManagement) target; //Referencing the script;
        List<SkeletonRelationships> relationships = manager.skeleton.GetRelationships();

        if(manager.textureToReference.Length == 0) return;
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // Limit mapping;
        int[] limitPerLimb = manager.limitPerLimb;
        int relationshipsReferenced = relationships.Count;

        if(limitPerLimb.Length != relationshipsReferenced) {
            Array.Resize(ref manager.limitPerLimb, relationshipsReferenced);
        }

        //Folder for containing the data;
        EditorGUILayout.LabelField("Texture-set Limits Per Limb Management", EditorStyles.boldLabel);
        folderLimitStatus = EditorGUILayout.Foldout(folderLimitStatus, "Texture Limb Limits");

        if (folderLimitStatus) {
            //Naming a subgroup of folders if needed;
            EditorGUILayout.HelpBox("When dealing with texturesets that contain only one texture requiring referencing, " +
                "you can conveniently set their data limits using the following array.", MessageType.Info);

            GUILayout.BeginVertical(GUI.skin.box);
            serializedObject.Update();
            SerializedProperty arrayProp = serializedObject.FindProperty("limitPerLimb");

            
            for (int i = 0; i < arrayProp.arraySize; i++){
                GUILayout.BeginHorizontal();
                SerializedProperty elementProp = arrayProp.GetArrayElementAtIndex(i);

                EditorGUILayout.LabelField($"{relationships[i].GetRelationshipName()} ({i})", GUILayout.Width(200f));
                elementProp.intValue = InputScrollable(manager.textureToReference[elementProp.intValue].name, 0, manager.textureToReference.Length - 1, arrayProp.GetArrayElementAtIndex(i).intValue);
                
                GUILayout.EndHorizontal();
            }

            // If the user needs to maximize all limits at once;
            if(GUILayout.Button("Maximize Limits")) {
                for(int i = 0; i < limitPerLimb.Length; i++) {
                    limitPerLimb[i] = manager.textureToReference.Length - 1;
                }
            }

            serializedObject.ApplyModifiedProperties();
            GUILayout.EndVertical();
        }
        

        //Texture Swapping;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Texture Management", EditorStyles.boldLabel);
        try {
            GUILayout.BeginVertical(GUI.skin.box);
            SkeletonRelationships relationship = relationships[currentRelationIndex];

            EditorGUILayout.LabelField("Body Part:");
            currentRelationIndex = Scrollable(relationship.GetRelationshipName(), 0, 
                relationships.Count - 1, currentRelationIndex);

            EditorGUILayout.LabelField("Texture Set:");
            TexturesSO selectedTexture = manager.textureToReference[currentSelectedTextureToReference];
            currentSelectedTextureToReference = Scrollable(selectedTexture.name, 0, 
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

        GUILayout.Button($"{name} ({index})");

        if(GUILayout.Button(">>", GUILayout.Width(40))) {
            if(index >= max) index = min;
            else index++;
        }
        EditorGUILayout.EndHorizontal();

        return index;
    }

    int scrollableSize = 30;

    private int InputScrollable(string name, int min, int max, int index) {
        if(GUILayout.Button("|<", GUILayout.Width(scrollableSize))) {
            index = min;
        }

        if(GUILayout.Button("<<", GUILayout.Width(scrollableSize))) {
            if(index <= min) index = max;
            else index--;
        }

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField(name);
        EditorGUI.EndDisabledGroup();

        if(GUILayout.Button(">>", GUILayout.Width(scrollableSize))) {
            if(index >= max) index = min;
            else index++;
        }
        if(GUILayout.Button(">|", GUILayout.Width(scrollableSize))) {
            index = max;
        }

        return index;
    }
}