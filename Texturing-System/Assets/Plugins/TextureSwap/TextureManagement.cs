using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using TMPro;

public enum TexturingType{
    BaseTexturesSO = 0,
    MultipleTexturesSO = 1,
}

public class TextureManagement : MonoBehaviour{
    //Data;
    [HideInInspector] public SkeletonSO skeleton;
    [HideInInspector] public TexturesSO textures;
    [HideInInspector] public MaterialStoreSO materialStoreSO;
    [HideInInspector] public TexturesSO[] texturesToReference;
    [HideInInspector] public int[] limitPerLimb;
    List<SkeletonRelationships> skeletonRelationships;
    [HideInInspector] public List<Texture2D> newTexturesToManage = new List<Texture2D>();
    [HideInInspector] public TexturingType texturingType;
    [HideInInspector] public TextMeshProUGUI[] relationshipTMToModify, texturesTMToModify;
    [HideInInspector] public string[] relationshipTMFormats, texturesTMFormats;


    //Unity Editor;
    Material material;
    MeshRenderer mesh;
    CustomMovieClipBehaviour movieClip;

    ////////////////////////////////////////////////////////////////////

    
    void Start() {
        //Extracting the material;
        mesh = GetComponent<MeshRenderer>();
        material = mesh.material;
        movieClip = GetComponent<CustomMovieClipBehaviour>();
        
        //Creating a new texture based in the original one;
        if(textures is BaseTexturesSO) {
            newTexturesToManage.Add(Utils.CloneTexture(textures.GetTexture()));
            texturingType = TexturingType.BaseTexturesSO;
        } else if (textures is MultipleTexturesSO){
            MultipleTexturesSO container = (MultipleTexturesSO) textures;
            newTexturesToManage = new List<Texture2D>(container.GetTexturesToSearch());
            texturingType = TexturingType.MultipleTexturesSO;

            // Creating a MaterialStoreSO;
            if(materialStoreSO == null){
                MaterialStoreSO instanceMaterialSO = new MaterialStoreSO();
                instanceMaterialSO.name = "Instance Material SO";
                materialStoreSO = instanceMaterialSO;
            }
        }

        //Circumvents the infinite material swapping provoked by UniSWF;
        enabled = true;
    }


    ////////////////////////////////////////////////////////////////////


    void Update(){
        if(texturingType == TexturingType.BaseTexturesSO){
            material.mainTexture = newTexturesToManage[0];
            mesh.material = material;
        }

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
                Utils.ClearTextureAt(limb.GetCoordinates(), newTexturesToManage);
                Utils.PasteTexture(limb, newTexturesToManage, 
                    textureToReference[textureType]);
            }
        }

        yield return new WaitForSeconds(4f);
        isCouroutineRunning = false;
    }*/


    ////////////////////////////////////////////////////////////////////


    /* SwapTextureProcess();
     * This is the process in charge of swapping textures. You require a
     * "SkeletonRelationships" reference and the current selected texture
     * set to point the data correctly to the texture swapping process. 
     * This fuction is used in all instances of texture swapping, if you
     * want to create a variant of the texturing process functions you
     * must invoke the "SwapTextureProcess" function in all your code 
     * snippets involving the texture swapping process. */
    public static void SwapTextureProcess(TextureManagement reference, SkeletonRelationships relationship, int selectedTextureToReference = 0){
        List<Material> materialReferences = new List<Material>();
        TexturesSO[] texturesToReference = reference.texturesToReference;

        foreach(Limb limb in relationship.GetLimbsRelated()) {
            switch(reference.texturingType){
                case TexturingType.BaseTexturesSO:
                    if(selectedTextureToReference > texturesToReference.Length) return;

                    Utils.ClearTextureAt(limb.GetCoordinates(), reference.newTexturesToManage[0]);
                    Utils.PasteTexture(limb, reference.newTexturesToManage[0], reference.texturesToReference[selectedTextureToReference]);
                break;

                case TexturingType.MultipleTexturesSO:
                    if(selectedTextureToReference > texturesToReference.Length) return;

                    // Extract the materials from the MaterialStoreSO object;
                    foreach(KeyValuePair<string, Material> kvp in reference.materialStoreSO.GetMaterials()) {
                        string key = kvp.Key;
                        Material material = kvp.Value;

                        Sprite[] sprites = Resources.LoadAll<Sprite>(key);

                        // Checking if a texture contains a relationship with a limb we need to swap;
                        foreach(Sprite sprite in sprites) {
                            if(sprite.name.Contains(relationship.GetRelationshipName()) &&
                                !materialReferences.Contains(material)){
                                // Add it to a list for future processing;
                                materialReferences.Add(material);
                            }
                        }
                    }

                    // Process and swap all limbs;
                    foreach(Material material in materialReferences) {
                        Texture2D texture = Utils.CloneTexture((Texture2D) material.GetTexture("_MainTex"));
                        
                        Utils.ClearTextureAt(limb.GetCoordinates(), texture);
                        Utils.PasteTexture(limb, texture, texturesToReference[selectedTextureToReference]);

                        material.SetTexture("_MainTex", texture);
                    }
                break;
            }                  
        }
    }

    /* SwapTextureSearchRelationship();
     * Invoke Relationship objects from the SkeletonSO reference via 
     * name and pass it to the "SwapTextureProcess" function. */
    public static void SwapTextureSearchRelationship(TextureManagement reference, string relationshipName, int selectedTextureToReference = 0){
        SkeletonRelationships relationship = SearchRelationship(reference, relationshipName);
        
        if(relationship == null) return;

        SwapTextureProcess(reference, relationship, selectedTextureToReference);
    }

    // SearchRelationship(); Search a relationship based on an input name;
    public static SkeletonRelationships SearchRelationship(TextureManagement reference, string relationshipName){
        // Searching for the parent relationship;
        SkeletonRelationships relationship = null;

        foreach (var rel in reference.skeleton.GetRelationships()){
            if (rel.GetRelationshipName() == relationshipName)
            {
                relationship = rel;
                break;
            }
        }

        return relationship;
    }

    /* SwapTextureSearchRelationshipAsync();
     * For all instances of the texture swapping process you must use this
     * function, as it minimizes slowdowns provoked by the texturing system.
     * This code will run asynchronously the texture swapping system to
     * liberate the main thread. This is not multithreading but a coroutine. */
    public static void SwapTextureSearchRelationshipAsync(TextureManagement reference, string relationshipName, int selectedTextureToReference = 0){
        SkeletonRelationships relationship = SearchRelationship(reference, relationshipName);

        if(relationship == null) return;

        reference.StartCoroutine(SwapTextureAsync(reference, relationship, selectedTextureToReference));
    }


    ////////////////////////////////////////////////////////////////////

    // SwapTextureRelationship(); Swap a texture based on an input relationship without searching it;
    public static void SwapTextureRelationship(TextureManagement reference, SkeletonRelationships relationship, int selectedTextureToReference = 0){
        if(relationship == null) return;

        SwapTextureProcess(reference, relationship, selectedTextureToReference);
    }

    /* SwapTextureRelationshipAsync();
     * For all instances of the texture swapping process you must use this
     * function, as it minimizes slowdowns provoked by the texturing system.
     * This code will run asynchronously the texture swapping system to
     * liberate the main thread. This is not multithreading but a coroutine. */
    public static void SwapTextureRelationshipAsync(TextureManagement reference, SkeletonRelationships relationship, int selectedTextureToReference = 0){
        if(relationship == null) return;

        reference.StartCoroutine(SwapTextureAsync(reference, relationship, selectedTextureToReference));
    }


    ////////////////////////////////////////////////////////////////////


    /* SwapTextureAsync();
     * The coroutine in charge of invoking the "SwapTextureProcess" function. */
    public static IEnumerator SwapTextureAsync(TextureManagement reference, SkeletonRelationships relationship, int selectedTextureToReference = 0){
        SwapTextureProcess(reference, relationship, selectedTextureToReference);
        
        yield return null;
    }


    ////////////////////////////////////////////////////////////////////

    // GUI Functions;
    [HideInInspector] public int selectedRelationshipIndex = 0, // The TextureSO component to reference for Texture Swapping;
    selectedTextureToReference = 0; // The Relationship we want to swap in and out;

    // SwapTextureRunProcess(); Runs the TextureSwapper process from an input GameObject;
    public void SwapTextureRunProcess(GameObject go){
        TextMeshProUGUI mesh = go.GetComponentInChildren<TextMeshProUGUI>();

        SwapTextureSearchRelationshipAsync(this, mesh.gameObject.name, selectedTextureToReference);
    }

    // IncreaseScrollerTextures(); Increases the index of the currently selected TextureSO in the GUI if invoked;
    public void IncreaseScrollerTextures(){
        int index = selectedTextureToReference,
        min = 0, max = limitPerLimb[selectedRelationshipIndex];

        // If the index goes out of bounds, return it to the minimum accepted value;
        if(index >= max) index = min;
        else index++;

        // Map the index to a TextureSO reference;
        selectedTextureToReference = index;
        TexturesSO selectedTexture = texturesToReference[selectedTextureToReference];
        string textureName = selectedTexture.name;

        // Change the associated TextMeshPro components in the GUI;
        ManageTexturesTextMeshPro(textureName);
    }

    // DecreaseScrollerTextures(); Decreases the index of the currently selected TextureSO in the GUI if invoked;
    public void DecreaseScrollerTextures(){
        int index = selectedTextureToReference,
        min = 0, max = limitPerLimb[selectedRelationshipIndex];

        // If the index goes out of bounds, return it to the maximum accepted value;
        if(index <= min) index = max;
        else index--;

        // Map the index to a TextureSO reference;
        selectedTextureToReference = index;
        TexturesSO selectedTexture = texturesToReference[selectedTextureToReference];
        string textureName = selectedTexture.name;

        // Change the associated TextMeshPro components in the GUI;
        ManageTexturesTextMeshPro(textureName);
    }

    /* ManageTexturesTextMeshPro(); Manages the TextMeshPro components associated
     *  with the TexturesSO components and their displayed text in the GUI. */
    public void ManageTexturesTextMeshPro(string name){
        for(int i = 0; i < texturesTMToModify.Length; i++){
            TextMeshProUGUI mesh = texturesTMToModify[i];
            string format = "";

            // Store the name of the relationship in the GameObject for future processing;
            mesh.gameObject.name = name;

            // Add arguments to TextMeshPro components to display TextureSO names;
            try{
                format = texturesTMFormats[i];

                if(format != ""){
                    mesh.text = format.Replace("[VAR]", name);
                    continue;
                }
            } catch {}
            

            mesh.text = name;
        }
    }


    ////////////////////////////////////////////////////////////////////

    // IncreaseScrollerRelationships(); Increases the index of the currently selected Relationship in the GUI if invoked;
    public void IncreaseScrollerRelationships(){
        int index = selectedRelationshipIndex,
        min = 0, max = skeleton.GetRelationships().Count - 1, 
        maxTexture = limitPerLimb[selectedRelationshipIndex];

        // If the index goes out of bounds, return it to the minimum accepted value;
        if(index >= max) index = min;
        else index++;

        /* Return the index to a preset Limb Limit if it references
         * a texture it shouldn't; */
        if(selectedTextureToReference >= maxTexture) selectedTextureToReference = 0;

        // Map the index to a Relationship reference;
        selectedRelationshipIndex = index;
        SkeletonRelationships relationship = skeleton.GetRelationships()[selectedRelationshipIndex];
        string relationshipName = relationship.GetRelationshipName();

        // Change the associated TextMeshPro components in the GUI;
        ManageRelationshipsTextMeshPro(relationshipName);
    }

    // DecreaseScrollerRelationships(); Decreases the index of the currently selected Relationship in the GUI if invoked;
    public void DecreaseScrollerRelationships(){
        int index = selectedRelationshipIndex,
        min = 0, max = skeleton.GetRelationships().Count - 1,
        maxTexture = limitPerLimb[selectedRelationshipIndex];

        // If the index goes out of bounds, return it to the maximum accepted value;
        if(index <= min) index = max;
        else index--;

        /* Return the index to a preset Limb Limit if it references
         * a texture it shouldn't; */
        if(selectedTextureToReference >= maxTexture) selectedTextureToReference = 0;

        // Map the index to a Relationship reference;
        selectedRelationshipIndex = index;
        SkeletonRelationships relationship = skeleton.GetRelationships()[selectedRelationshipIndex];
        string relationshipName = relationship.GetRelationshipName();

        // Change the associated TextMeshPro components in the GUI;
        ManageRelationshipsTextMeshPro(relationshipName);
    }

    /* ManageRelationshipsTextMeshPro(); Manages the Relationship components associated
     *  with the SkeletonSO components and their displayed text in the GUI. */
    public void ManageRelationshipsTextMeshPro(string name){
        for(int i = 0; i < relationshipTMToModify.Length; i++){
            TextMeshProUGUI mesh = relationshipTMToModify[i];
            string format = "";

            // Store the name of the relationship in the GameObject for future processing;
            mesh.gameObject.name = name;

            // Add arguments to TextMeshPro components to display TextureSO names;
            try{
                format = relationshipTMFormats[i];

                if(format != ""){
                    mesh.text = format.Replace("[VAR]", name);
                    continue;
                }
            } catch {}
            

            mesh.text = name;
        }
    }
}

////////////////////////////////////////////////////////////////////

//Custom Editor;
[CustomEditor(typeof(TextureManagement))]
public class CustomInspector : Editor {
    int selectedRelationshipIndex = 0, selectedTextureToReference = 0;
    bool folderLimitStatus;

    //Changes values in the inspector, creating a custom inspector;
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        serializedObject.Update();

        SerializedProperty materialStoreSOProperty = serializedObject.FindProperty("materialStoreSO"),
        texturesProperty = serializedObject.FindProperty("textures"),
        texturesToReferenceProperty = serializedObject.FindProperty("texturesToReference"),
        skeletonProperty = serializedObject.FindProperty("skeleton"),
        relationshipTMToModifyProperty = serializedObject.FindProperty("relationshipTMToModify"),
        relationshipTMFormatsProperty = serializedObject.FindProperty("relationshipTMFormats"),
        texturesTMToModifyProperty = serializedObject.FindProperty("texturesTMToModify"),
        texturesTMFormatsProperty = serializedObject.FindProperty("texturesTMFormats");


        TextureManagement manager = (TextureManagement) target; //Referencing the script;
        selectedRelationshipIndex = manager.selectedRelationshipIndex;
        selectedTextureToReference = manager.selectedTextureToReference;

        Separator();

        EditorGUILayout.PropertyField(skeletonProperty, new GUIContent("Skeleton"));
        
        Separator();

        if(manager.skeleton == null) return;
        List<SkeletonRelationships> relationships = manager.skeleton.GetRelationships();

        if(relationships.Count == 0){
            EditorGUILayout.HelpBox("Skeleton " + manager.skeleton.name + " has no relationships.\nPlease initialize the skeleton relationships to progress with the texture swapping system.", MessageType.Error);
            Separator();
            return;
        }

        EditorGUILayout.PropertyField(texturesProperty, new GUIContent("Textures"));

        // If the type of textures is multiple, call for the MaterialStoreSO reference;
        if (manager.textures is MultipleTexturesSO){
            GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(materialStoreSOProperty, new GUIContent("Material Store SO"));

                // Formatting the path to save the asset correctly;
                string assetPath = AssetDatabase.GetAssetPath(manager.textures);
                int dashIndex = assetPath.LastIndexOf('/');
                string formattedPath = assetPath.Substring(0, dashIndex + 1);
                formattedPath += manager.gameObject.name + ".asset";

                if(manager.materialStoreSO == null && GUILayout.Button("New", GUILayout.Width(60))){
                    // Create the MaterialStoreSO asset;
                    MaterialStoreSO newMaterialStoreSO = ScriptableObject.CreateInstance<MaterialStoreSO>();

                    // Saving the asset in the files and the object;
                    AssetDatabase.CreateAsset(newMaterialStoreSO, formattedPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    manager.materialStoreSO = newMaterialStoreSO;
                }
                
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        Separator();

        // Displaying the textures to reference list;
        EditorGUILayout.PropertyField(texturesToReferenceProperty, true);

        serializedObject.ApplyModifiedProperties();

        Separator();

        if(manager.texturesToReference.Length == 0 || manager.texturesToReference[0] == null) return;
        EditorGUILayout.Space();

        

        // Limit mapping;
        int[] limitPerLimb = manager.limitPerLimb;
        int relationshipsReferenced = relationships.Count;

        if(limitPerLimb.Length != relationshipsReferenced) {
            Array.Resize(ref manager.limitPerLimb, relationshipsReferenced);
        }

        //Folder for containing the data;
        EditorGUILayout.LabelField("Texture Set Limits Per Limb Management", EditorStyles.boldLabel);
        GUILayout.BeginVertical(GUI.skin.box);
        folderLimitStatus = EditorGUILayout.Foldout(folderLimitStatus, "Texture Limb Limits");

        if (folderLimitStatus) {
            //Naming a subgroup of folders if needed;
            EditorGUILayout.HelpBox("When dealing with texture sets that contain only one texture requiring referencing, " +
                "you can conveniently set their data limits using the following array.", MessageType.Info);
            
            serializedObject.Update();
            SerializedProperty arrayProp = serializedObject.FindProperty("limitPerLimb");

            try{
                for (int i = 0; i < arrayProp.arraySize; i++){
                    GUILayout.BeginHorizontal();
                    SerializedProperty elementProp = arrayProp.GetArrayElementAtIndex(i);

                    EditorGUILayout.LabelField($"{relationships[i].GetRelationshipName()}", GUILayout.Width(150f));
                    elementProp.intValue = InputScrollable(manager.texturesToReference[elementProp.intValue].name, 0, manager.texturesToReference.Length - 1, arrayProp.GetArrayElementAtIndex(i).intValue);
                    
                    GUILayout.EndHorizontal();
                }
            

                // If the user needs to maximize all limits at once;
                if(GUILayout.Button("Maximize Limits")) {
                    MaximizeTextureLimits(limitPerLimb, manager.texturesToReference.Length);
                }

            } catch {
                MaximizeTextureLimits(limitPerLimb, manager.texturesToReference.Length);
            }

            serializedObject.ApplyModifiedProperties();
            
        }
        GUILayout.EndVertical();
        

        //Texture Swapping;
        Separator();
        EditorGUILayout.LabelField("Texture Management", EditorStyles.boldLabel);

        GUILayout.BeginVertical(GUI.skin.box);

        SkeletonRelationships relationship = relationships[selectedRelationshipIndex];
        string relationshipName = relationship.GetRelationshipName();

        EditorGUILayout.LabelField("Body Part:");
        manager.selectedRelationshipIndex = Scrollable(relationshipName, 0, 
            relationships.Count - 1, selectedRelationshipIndex);

        manager.ManageRelationshipsTextMeshPro(relationshipName);

        EditorGUILayout.LabelField("Texture Set:");
        TexturesSO selectedTexture = manager.texturesToReference[selectedTextureToReference];
        manager.selectedTextureToReference = Scrollable(selectedTexture.name, 0, 
            limitPerLimb[selectedRelationshipIndex], selectedTextureToReference);

        manager.ManageTexturesTextMeshPro(selectedTexture.name);

        if(selectedTextureToReference > limitPerLimb[selectedRelationshipIndex]) manager.selectedTextureToReference = 0;


        EditorGUILayout.Space();

        if(GUILayout.Button("Swap " + relationshipName + " Textures")) {
            TextureManagement.SwapTextureRelationshipAsync(manager, relationship, selectedTextureToReference); 
        }

        GUILayout.EndVertical();

        Separator();


        EditorGUILayout.PropertyField(relationshipTMToModifyProperty, true);

        EditorGUILayout.PropertyField(relationshipTMFormatsProperty, true);

        Separator();

        EditorGUILayout.PropertyField(texturesTMToModifyProperty, true);

        EditorGUILayout.PropertyField(texturesTMFormatsProperty, true);

        Separator();
    }

    private void MaximizeTextureLimits(int[] limitPerLimb, int length){
        for(int i = 0; i < limitPerLimb.Length; i++) {
            limitPerLimb[i] = length - 1;
        }
    }

    private void Separator(){
        Utils.Separator();
        serializedObject.ApplyModifiedProperties();
    }


    private int Scrollable(string name, int min, int max, int index) {
        EditorGUILayout.BeginHorizontal();

        if(GUILayout.Button("<<", GUILayout.Width(40))) {
            if(index <= min) index = max;
            else index--;
        }

        GUILayout.Button($"{name}");

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