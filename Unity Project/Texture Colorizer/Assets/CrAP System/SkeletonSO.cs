using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

[CreateAssetMenu(fileName = "SkeletonSO", menuName = "ScriptableObjects/Skeleton Manager SO", order = 1)]
[System.Serializable]
/* SkeletonSO is the scriptable object containing all the
 * relationships between the data; Is what connects the limbs
 * to a specific group; it groups the relationships together; */ 
public class SkeletonSO : ScriptableObject{
    [SerializeField] TexturesSO textureData;
    [HideInInspector] [SerializeField] List<SkeletonRelationships> relationships;

    //Making data save consistently;
    private void OnEnable() { 
        EditorUtility.SetDirty(this);
    }

    private void OnValidate() {
        AssetDatabase.SaveAssets();
    }

    public TexturesSO GetTextureData(){ return textureData; }
    public List<SkeletonRelationships> GetRelationships(){ return relationships; }
    public void AddRelationship(SkeletonRelationships SkeletonRelationship){ relationships.Add(SkeletonRelationship); }
    public void RemoveLastRelationship(){ relationships.RemoveAt(relationships.Count - 1); }
    public void ClearRelationships(){ relationships.Clear(); }
}


////////////////////////////////////////////////////////////////////


/* SkeletonRelationships are the relationships themselves,
 * they are the nodes of the data where everything is stored;
 * is the group of data related to eachother; */

[System.Serializable]
public class SkeletonRelationships{
    [SerializeField] string relationshipName;
    [SerializeField] List<Limb> limbsRelated;
    [SerializeField] bool folder;

    public SkeletonRelationships(string RelationshipName) {
        relationshipName = RelationshipName;
        limbsRelated = new List<Limb>();
    }
    public string GetRelationshipName(){ return relationshipName; }
    public void SetRelationshipName(string RelationshipName){ relationshipName = RelationshipName; }
    public List<Limb> GetLimbsRelated(){ return limbsRelated; }
    public void SetLimbsRelated(List<Limb> LimbsRelated){ limbsRelated = LimbsRelated; }
    public void AddLimbRelated(Limb limb){ limbsRelated.Add(limb); }
    public void RemoveLimbRelated(){ limbsRelated.RemoveAt(limbsRelated.Count - 1); }
    public void ClearLimbsRelated(){ limbsRelated = new List<Limb>(); }

    //GUI Management;
    public bool GetFolder(){ return folder; }
    public void SetFolder(bool Folder){ folder = Folder; }
}


////////////////////////////////////////////////////////////////////


[CustomEditor(typeof(SkeletonSO))]
public class SkeletonSOEditor : Editor {
    //Memory consistent values (Text fields);
    private string textFieldRelationName = "";
    private string renameFieldRelationName = "";

    public override void OnInspectorGUI() {
        //Skeleton, Textures, and Relationships references;
        base.OnInspectorGUI();
        SkeletonSO skeleton = (SkeletonSO) target;
        List<SkeletonRelationships> relationships = skeleton.GetRelationships();
        TexturesSO textures = skeleton.GetTextureData();

        if(textures == null) return;

        //Relationship box;
        EditorGUILayout.Space();
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Create a New Relation in the Skeleton:", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Relation Name:");

        //Text field for the relationship's name;
        textFieldRelationName = GUILayout.TextField(textFieldRelationName, GUILayout.ExpandWidth(true));
        string relationName = textFieldRelationName; //Data normalization;

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        //***********************************

        // Create a button to open the window
        EditorGUILayout.BeginHorizontal();
        
        //Is name only letters either lower or upper case?
        bool isNameValid = Regex.IsMatch(textFieldRelationName, "^[a-zA-Z]+$");
        bool isExisting = relationships.Any(r => r.GetRelationshipName().ToUpper().Equals(relationName.ToUpper()));

        //Adding, Removing, and Clearing Relationships;
        if (GUILayout.Button("Add New")) {
            AddNewRelationship(isNameValid, isExisting, textures, skeleton, relationName);
        }
        RemoveRenameAndClearRelationships(skeleton);
        EditorGUILayout.EndHorizontal();
        
        RemoveAndRenameRelationships(relationships, isExisting);
        EditorGUILayout.Space();
        
        //Loads all similar named relationships;
        if(GUILayout.Button("Load All Relationships")) {
            skeleton.ClearRelationships();
            List<string> sameGroupNames = new List<string>();

            foreach(Limb limb in textures.GetLimbs()) {
                string identificator = limb.GetName().Split(' ')[0];

                if (!sameGroupNames.Contains(identificator)) {
                    UnityEngine.Debug.Log(identificator);
                    sameGroupNames.Add(identificator);

                    AddNewRelationship(true, false, textures, skeleton, identificator);
                }
            }
        }

        // Create a button to open the window
        if (GUILayout.Button("Open List Window")){
            SkeletonEditorWindow window = new SkeletonEditorWindow(skeleton, new SkeletonRelationships(""), false);
            SkeletonEditorWindow.OpenWindow(window);
        }
        
        //***********************************

        //Save and remove relationships from a specific class;
        ManageRelationships(skeleton, relationships);
    }

    //Adds a new button in the GUI to create a new relationship between data;
    private void AddNewRelationship(bool isNameValid, bool isExisting, TexturesSO textures, 
        SkeletonSO skeleton, string relationName) {
        //If the input name is not valid, get out of the script;
        if(!isNameValid || isExisting){
            UnityEngine.Debug.Log("The name " + textFieldRelationName + "already exists in the relationships list or is not valid."); 
            return;
        }

        //Create an empty relationship;
        SkeletonRelationships relationship = new SkeletonRelationships(textFieldRelationName);
        List<Limb> limbs = textures.GetLimbs();

        /*Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();*/

        //For each matched limb, add it in the relationship object;
        foreach(Limb limb in limbs) {
            bool isNameInList = limb.GetName().ToUpper().StartsWith(relationName.ToUpper());
            if (isNameInList){
                relationship.SetRelationshipName(relationName);
                relationship.AddLimbRelated(limb);
            }
        }

        /*stopwatch.Stop();
        UnityEngine.Debug.Log(stopwatch.Elapsed.ToString());*/

        skeleton.AddRelationship(relationship);
    }

    //Creates the buttons in the GUI to remove or clear relationships between data;
    private void RemoveRenameAndClearRelationships(SkeletonSO skeleton){
        if (GUILayout.Button("Remove Last")){
            try { skeleton.RemoveLastRelationship(); } catch { }
        }

        if (GUILayout.Button("Clear All")){
            skeleton.ClearRelationships();
        }
    }

    //Remove and rename specific relationships between data;
    private void RemoveAndRenameRelationships(List<SkeletonRelationships> relationships, bool isExistingOutside) {
        if(!isExistingOutside || textFieldRelationName.Equals("")) return;

        //Renaming specific relationships;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Renaming Relationships:", EditorStyles.boldLabel);
        renameFieldRelationName = GUILayout.TextField(renameFieldRelationName, GUILayout.ExpandWidth(true), GUILayout.ExpandWidth(true));
        GUILayout.EndScrollView();

        if(renameFieldRelationName.Equals("")) return;

        //Is name only letters either lower or upper case?
        bool isNameValid = Regex.IsMatch(textFieldRelationName, "^[a-zA-Z]+$");
        bool isExisting = relationships.Any(r => r.GetRelationshipName().ToUpper().Equals(renameFieldRelationName.ToUpper()));

        //Button with renaming variables;
        if (GUILayout.Button("Rename " + textFieldRelationName + " to " + renameFieldRelationName)){
            try {
                //Find the first element with 'x' name and replace it;
                SkeletonRelationships renameRelationship = relationships.FirstOrDefault(x => x.GetRelationshipName().Equals(textFieldRelationName));
                renameRelationship.SetRelationshipName(renameFieldRelationName);
            } catch { }
        }
    }

    private void ManageRelationships(SkeletonSO skeleton, List<SkeletonRelationships> relationships) {
        foreach(SkeletonRelationships relationship in relationships) { 
            //Storing the data inside a GUI "folder;"
            GUILayout.BeginVertical(GUI.skin.box);
            bool folderRelationship = relationship.GetFolder();
            relationship.SetFolder(folderRelationship = EditorGUILayout.Foldout(folderRelationship, relationship.GetRelationshipName()));

            //If the folder is closed, exit the script;
            if (!folderRelationship) {
                GUILayout.EndVertical();
                continue;
            }

            //Load the limbs in the GUI;
            foreach(Limb limb in relationship.GetLimbsRelated()) {
                TexturesSOEditor.LoadLimbInGUI(limb, 1, false);

                //Add the remove relationship button;
                if (GUILayout.Button("Remove " + limb.GetName() + " Relationship")){
                    relationship.GetLimbsRelated().Remove(limb);
                }
            }

            //Differentiating from remove and add relationships buttons;
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;

            //Open the window to select limbs and select new relationships from there;
            if (GUILayout.Button("Add New " + relationship.GetRelationshipName() + " Relationship")){
                SkeletonEditorWindow window = new SkeletonEditorWindow(skeleton, relationship, true);
                SkeletonEditorWindow.OpenWindow(window);
            }

            GUI.backgroundColor = Color.red;

            //Removing specific relationships;
            if (GUILayout.Button("Remove " + relationship.GetRelationshipName())){
                try { relationships.RemoveAll(x => x.GetRelationshipName().Equals(relationship.GetRelationshipName())); } catch { }
            }
            
            GUI.backgroundColor = originalColor;
            GUILayout.EndVertical();
        }
    }
}


////////////////////////////////////////////////////////////////////


//Limb Select Window;
public class SkeletonEditorWindow : EditorWindow {
    //References;
    SkeletonSO skeleton;
    SkeletonRelationships relationships;

    public SkeletonEditorWindow(SkeletonSO Skeleton, SkeletonRelationships Relationships, bool ButtonCanActivateActions) {
        skeleton = Skeleton;
        relationships = Relationships;
        buttonCanActivateActions = ButtonCanActivateActions;
    }

    public static void OpenWindow(SkeletonEditorWindow window) {
        window.titleContent = new GUIContent("Available Limb List");
        window.Show();
    }

    //Window;
    bool buttonCanActivateActions = false;
    string searchQuery = "";
    Vector2 scrollPosition;

    public void OnGUI() {
        EditorGUILayout.LabelField("Limb References: ", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        GUILayout.BeginVertical(GUI.skin.box);

        //Get the limbs in the TextureSO;
        List<Limb> limbList = skeleton.GetTextureData().GetLimbs();

        //Get the limb relations inside one group;
        List<Limb> limbRelations = relationships.GetLimbsRelated();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        searchQuery = EditorGUILayout.TextField(searchQuery);

        if(limbList != null) {
            /*Draw all limbs in the window,
             * if a limb is already in a relation, then, remove it from the list of visible limbs,
             * if the search query is not empty, then, filter the limbs; */
            foreach(Limb limb in limbList) {
                string name = limb.GetName();
                bool isAlreadyRelated = limbRelations.Any(r => r.GetName().Equals(name));
                bool isUserSearching = name.Contains(searchQuery); //"" filters all limbs;
                
                if (isUserSearching && !isAlreadyRelated) {
                    if (GUILayout.Button(name) && buttonCanActivateActions) {
                        relationships.AddLimbRelated(limb);
                    }
                }
            }
        }
        GUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }
}