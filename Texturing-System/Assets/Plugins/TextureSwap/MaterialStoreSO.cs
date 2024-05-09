using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "MaterialStoreSO", menuName = "ScriptableObjects/Material Store SO", order = 1)]
[System.Serializable]
public class MaterialStoreSO : ScriptableObject{
    [SerializeField] Dictionary<string, Material> materials;

    //Making data save consistently;
    private void OnEnable() { 
        EditorUtility.SetDirty(this);
    }

    private void OnValidate() {
        AssetDatabase.SaveAssets();
    }

    public Dictionary<string, Material> GetMaterials(){
        try{
            int count = materials.Count;
        } catch {
            materials = new Dictionary<string, Material>();
        }
        
        
        return materials;
    }

    public void SetMaterials(Dictionary<string, Material> Materials){
        materials = Materials;
    }

}

//Custom Editor;
[CustomEditor(typeof(MaterialStoreSO))]
public class MaterialStoreSOEditor : Editor{
    

    public override void OnInspectorGUI(){
        DrawDefaultInspector();

        MaterialStoreSO manager = (MaterialStoreSO) target; //Referencing the script;

        try{
            foreach(KeyValuePair<string, Material> kvp in manager.GetMaterials()){
                string key = kvp.Key;
                Material material = kvp.Value;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(key);
                EditorGUILayout.ObjectField(material, typeof(Material), true);
                EditorGUILayout.EndHorizontal();
            }
        } catch {}

        Separator();

        GUILayout.Label("The base materials dictionary contains " +  manager.GetMaterials().Count + " material/s.");

        Separator();
    }

    private void Separator(){
        Utils.Separator();
        serializedObject.ApplyModifiedProperties();
    }

    public static void LoadLimbInGUI(Material material, float rectSize) {
        int boxHeight = 20;
        string name = material.name;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(name, EditorStyles.boldLabel);

        // Display the label and read-only texture field
        Utils.ShowTextureInField((Texture2D) material.GetTexture("_MainTex"), "Main Texture", rectSize);
        
        EditorGUILayout.EndVertical();

        //Texture Mask;
        //ShowTextureInField(limb.GetMaskReference(), "Mask Texture Reference", rectSize);

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();
    }

}