using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;

[CreateAssetMenu(fileName = "SkeletonSO", menuName = "ScriptableObjects/Skeleton Manager SO", order = 1)]
public class SkeletonSO : ScriptableObject{
    [SerializeField] TexturesSO textureData;
    [SerializeField] List<SkeletonRelationships> relationships;

    public TexturesSO GetTextureData(){ return textureData; }
    public List<SkeletonRelationships> GetRelationships(){ return relationships; }
    public void AddRelationship(SkeletonRelationships SkeletonRelationship){ 
        relationships.Add(SkeletonRelationship); 
        SkeletonRelationship = new SkeletonRelationships("");
    }
    public void RemoveLastRelationship(){ relationships.RemoveAt(relationships.Count - 1); }
    public void ClearRelationships(){ relationships.Clear(); }
}

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
    public bool GetFolder(){ return folder; }
    public void SetFolder(bool Folder){ folder = Folder; }
}

[CustomEditor(typeof(SkeletonSO))]
public class SkeletonSOEditor : Editor {
    private SerializedProperty textureDataProperty;
    private string textFieldRelationName = "";

    private void OnEnable(){
        textureDataProperty = serializedObject.FindProperty("textureData");
    }

    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();
        SkeletonSO scriptableObject = (SkeletonSO) target;
        List<SkeletonRelationships> relationships = scriptableObject.GetRelationships();
        TexturesSO textures = scriptableObject.GetTextureData();
        
        serializedObject.Update();

        EditorGUILayout.PropertyField(textureDataProperty, true);
        EditorGUILayout.Space();

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Create a New Relation", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Relation Name");
        textFieldRelationName = GUILayout.TextField(textFieldRelationName, GUILayout.Width(400));
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        // Create a button to open the window
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add New")){
            string relationNameUpper = textFieldRelationName.ToUpper();
            bool isExisting = relationships.Any(item => item.GetRelationshipName().Equals(relationNameUpper));
            bool isNameValid = Regex.IsMatch(textFieldRelationName, "^[a-zA-Z]+$");

            if (isNameValid) {
                if (!isExisting) {
                    SkeletonRelationships relationship = new SkeletonRelationships(textFieldRelationName);

                    foreach(Limb limb in textures.GetLimbs()) {
                        if (limb.GetName().ToUpper().StartsWith(relationNameUpper)) {
                            relationship.AddLimbRelated(limb);
                        }
                    }

                    scriptableObject.AddRelationship(relationship);

                } else { Debug.LogWarning("The name " + textFieldRelationName + "already exists in the relationships list."); }
            } else { Debug.LogWarning("The name " + textFieldRelationName + "is not valid to create a relationship, be sure to only use letters for its name."); }
        }

        if (GUILayout.Button("Remove Last")){
            try {
                scriptableObject.RemoveLastRelationship();
            } catch { }
        }

        if (GUILayout.Button("Clear All")){
            scriptableObject.ClearRelationships();
        }

        EditorGUILayout.EndHorizontal();
        if (!textFieldRelationName.Equals("")) {
            if (GUILayout.Button("Remove " + textFieldRelationName)){
                try {
                    relationships.RemoveAll(x => x.GetRelationshipName().Equals(textFieldRelationName));
                } catch { }
            }
        }
        

        EditorGUILayout.Space();
        
        
        // Create a button to open the window
        if (GUILayout.Button("Open List Window")){
            SkeletonEditorWindow window = new SkeletonEditorWindow(scriptableObject, new SkeletonRelationships(""));
            SkeletonEditorWindow.OpenWindow(window);
        }
        
        foreach(SkeletonRelationships relationship in relationships) { 
            GUILayout.BeginVertical(GUI.skin.box);
            bool folderRelationship = relationship.GetFolder();
            relationship.SetFolder(folderRelationship = EditorGUILayout.Foldout(folderRelationship, relationship.GetRelationshipName()));

            if (folderRelationship) {
                foreach(Limb limb in relationship.GetLimbsRelated()) {
                    TexturesSOEditor.LoadLimbInGUI(limb, 1, false);

                    if (GUILayout.Button("Remove " + limb.GetName() + " Relationship")){
                        relationship.GetLimbsRelated().Remove(limb);
                    }
                }

                Color originalColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.green;

                if (GUILayout.Button("Add New " + relationship.GetRelationshipName() + " Relationship")){
                    GUI.backgroundColor = originalColor;
                    SkeletonEditorWindow window = new SkeletonEditorWindow(scriptableObject, relationship);
                    SkeletonEditorWindow.OpenWindow(window);
                }

            }
            
            GUILayout.EndVertical();
        }
    }
}

public class SkeletonEditorWindow : EditorWindow {
    private SkeletonSO skeleton;
    SkeletonRelationships relationships;
    private string searchQuery = "";

    public SkeletonEditorWindow(SkeletonSO Skeleton, SkeletonRelationships Relationships) {
        skeleton = Skeleton;
        relationships = Relationships;
    }

    public static void OpenWindow(SkeletonEditorWindow window) {
        window.titleContent = new GUIContent("Available Limb List");
        window.Show();
    }

    public void OnGUI() {
        EditorGUILayout.LabelField("Limb References: ");

        List<Limb> limbList = skeleton.GetTextureData().GetLimbs();
        List<Limb> limbRelations = relationships.GetLimbsRelated();

        searchQuery = EditorGUILayout.TextField(searchQuery);

        if(limbList != null) {
            foreach(Limb limb in limbList) {
                bool isAlreadyRelated = limbRelations.Any(r => r.GetName().Equals(limb.GetName()));
                
                if (limb.GetName().Contains(searchQuery) && !isAlreadyRelated) {
                    if (GUILayout.Button(limb.GetName())) {
                        relationships.AddLimbRelated(limb);
                    }
                }
            }
        }
    }
}