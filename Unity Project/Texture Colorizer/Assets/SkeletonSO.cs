using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "SkeletonSO", menuName = "ScriptableObjects/Skeleton Manager SO", order = 1)]
public class SkeletonSO : ScriptableObject{
    [SerializeField] TexturesSO textureData;
    [SerializeField] List<SkeletonRelationships> relationships;

    public TexturesSO GetTextureData(){ return textureData; }
    public List<SkeletonRelationships> GetRelationships(){ return relationships; }
    public void AddRelationship(SkeletonRelationships SkeletonRelationship){ relationships.Add(SkeletonRelationship); }
}

[System.Serializable]
public class SkeletonRelationships{
    [SerializeField] string relationshipName;
    [SerializeField] List<Limb> limbsRelated;
    bool fold;

    public string GetRelationshipName(){ return relationshipName; }
    public void SetRelationshipName(string RelationshipName){ relationshipName = RelationshipName; }
    public List<Limb> GetLimbsRelated(){ return limbsRelated; }
    public void SetLimbsRelated(List<Limb> LimbsRelated){ limbsRelated = LimbsRelated; }
    public void AddLimbRelated(Limb limb){ limbsRelated.Add(limb); }
    public void RemoveLimbRelated(){ limbsRelated.RemoveAt(limbsRelated.Count - 1); }
    public void ClearLimbsRelated(){ limbsRelated = new List<Limb>(); }
}

[CustomEditor(typeof(SkeletonSO))]
public class SkeletonSOEditor : Editor {
    private SerializedProperty textureDataProperty;

    private void OnEnable(){
        textureDataProperty = serializedObject.FindProperty("textureData");
    }

    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();
        SkeletonSO scriptableObject = (SkeletonSO) target;
        TexturesSO textures = scriptableObject.GetTextureData();

        
        serializedObject.Update();

        EditorGUILayout.PropertyField(textureDataProperty, true);

        // Add buttons to the list
        EditorGUILayout.Space();
        
        // Create a button to open the window
        if (GUILayout.Button("Open List Window")){
            SkeletonEditorWindow window = new SkeletonEditorWindow(scriptableObject);
            SkeletonEditorWindow.OpenWindow(window);
        }

        /*GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.EndVertical();
        foreach(SkeletonRelationships relationship in scriptableObject.GetRelationships()) { }*/
    }
}

public class SkeletonEditorWindow : EditorWindow {
    private SkeletonSO skeleton;
    private string searchQuery = "";

    public SkeletonEditorWindow(SkeletonSO Skeleton) {
        skeleton = Skeleton;
    }

    public static void OpenWindow(SkeletonEditorWindow window) {
        window.titleContent = new GUIContent("Available Limb List");
        window.Show();
    }

    public void OnGUI() {
        EditorGUILayout.LabelField("Limb References: ");

        List<Limb> limbList = skeleton.GetTextureData().GetLimbs();

        searchQuery = EditorGUILayout.TextField(searchQuery);

        if(limbList != null) {
            foreach(Limb limb in limbList) {
                if (limb.GetName().Contains(searchQuery)) {
                    if (GUILayout.Button(limb.GetName())) {

                    }
                }
            }
        }
    }
}