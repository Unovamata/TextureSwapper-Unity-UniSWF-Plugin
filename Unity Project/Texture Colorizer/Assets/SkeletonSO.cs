using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "SkeletonSO", menuName = "ScriptableObjects/SkeletonData", order = 1)]
public class SkeletonSO : ScriptableObject {
    public List<Limb> limbs = new List<Limb>();
}

public class Limb {
    Texture2D texture;
    string name;
    Vector4 coordinates;
    Vector2 pivot;

    public Texture2D GetTexture() { return texture; }
    public void SetTexture(Texture2D _Texture) { texture = _Texture; }
    public string GetName() { return name; }
    public void SetName(string Name) { name = Name; }
    public Vector4 GetCoordinates() { return coordinates; }
    public void SetCoordinates(Vector4 Coordinates) { coordinates = Coordinates; }
    public Vector2 GetPivot() { return pivot; }
    public void SetPivot(Vector2 Pivot) { pivot = Pivot; }
}

[CustomEditor(typeof(SkeletonSO))] // Replace with the name of your ScriptableObject class
public class SkeletonSOEditor : Editor{
    public override void OnInspectorGUI(){
        SkeletonSO scriptableObject = (SkeletonSO) target;
        DrawDefaultInspector();

        // Display a label for a property
        EditorGUILayout.LabelField("Skeleton Scriptable Object Data", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(scriptableObject.limbs.Count + " Limbs found");
        EditorGUILayout.Space();

        // Display a button
        if (GUILayout.Button("Load Limb Data From Texture")){
            CrAPTextureManagement.LoadSkeletonData("SkeletonData", scriptableObject);
        }


        try {
            foreach(Limb limb in scriptableObject.limbs) {
                string name = limb.GetName();
                Vector4 coordinates = limb.GetCoordinates();
                int x = (int)coordinates.x;
                int y = (int)coordinates.y;
                int w = (int)coordinates.z;
                int h = (int)coordinates.w;
            }
        } catch { }

        
    }
}