using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "MaterialStoreSO", menuName = "ScriptableObjects/Material Store SO", order = 1)]
public class MaterialStoreSO : ScriptableObject{
    [SerializeField] Dictionary<string, Material> materials = new Dictionary<string, Material>();

    //Making data save consistently;
    public void OnEnable() {
        EditorUtility.SetDirty(this);
    }

    public void OnValidate() {
        AssetDatabase.SaveAssets();
    }

    public Dictionary<string, Material> GetMaterials(){
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

        Debug.Log(manager.GetMaterials().Count);

        foreach(KeyValuePair<string, Material> kvp in manager.GetMaterials()){
            string key = kvp.Key;
            Material material = kvp.Value;

            int dashIndex = key.LastIndexOf('/');
            string materialName = key.Substring(dashIndex + 1); // Add 1 to start from the character after the dash


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(materialName);
            EditorGUILayout.ObjectField(material, typeof(Material), true);
            EditorGUILayout.EndHorizontal();
        }
    }

}