using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "SkeletonSO", menuName = "ScriptableObjects/Skeleton Manager SO", order = 1)]
public class SkeletonSO : ScriptableObject{
    [SerializeField] private TexturesSO textureData;
    [SerializeField] private bool[,] relationships;

    public TexturesSO GetTextureData(){ return textureData; }
    public bool[,] GetRelationships(){ 
        int size = textureData.GetLimbs().Count;
        relationships = new bool[size, size];
        return relationships; 
    }

    public void SetRelationships(bool[,] Relationships){ 
        relationships = Relationships;
    }
}

public class SkeletonRelationshops{

}

[CustomEditor(typeof(SkeletonSO))]
public class SkeletonSOEditor : Editor {
    int rows;
    int columns;
    bool boolVariable = false;
    bool[,] relationships;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        SkeletonSO scriptableObject = (SkeletonSO) target;
        TexturesSO textures = scriptableObject.GetTextureData();

        if(relationships == null) {
            relationships = scriptableObject.GetRelationships();
        }

        // Calculate the number of rows and columns in the grid
        int xSpacing = 20, ySpacing = 20;
        rows = textures.GetLimbs().Count;
        columns = rows;
        

        // Begin the grid layout
        GUILayout.BeginVertical(GUI.skin.box);

        

        // Iterate through the columns
        for (int column = 0; column <= columns; column++){
            GUILayout.BeginHorizontal();

            // Iterate through the rows
            for (int row = 0; row <= rows; row++){
                string name = textures.GetLimbs()[Mathf.Clamp(column - 1, 0, 999)].GetName();

                if(column == 0 && row == 0) {
                    GUILayout.Label("", GUILayout.Width(xSpacing + 127.8f), GUILayout.Height(ySpacing));
                }

                else if(column == 0) {
                    GUILayout.Label((row - 1).ToString(), GUILayout.Width(xSpacing), GUILayout.Height(ySpacing));
                }

                else if(row == 0) {
                    GUILayout.Label(name + " (" + (column - 1).ToString() + ")", GUILayout.Width(xSpacing), GUILayout.Width(150));
                }

                else {
                    if(column == row) {
                        GUILayout.Label("X".ToString(), GUILayout.Width(xSpacing + 1.11f), GUILayout.Height(ySpacing));
                    } else {
                        if(row < column) {
                            bool currentRelationship = relationships[row - 1, column - 1];

                            Color originalColor = GUI.backgroundColor;
                            if(currentRelationship) GUI.backgroundColor = Color.green;

                            if(GUILayout.Button("", GUILayout.Width(xSpacing + 1.11f), GUILayout.Height(ySpacing))) {
                                relationships[row - 1, column - 1] = currentRelationship ? false : true;
                                Debug.Log(relationships[row - 1, column - 1]);
                            }

                            GUI.backgroundColor = originalColor;
                            //EditorGUILayout.Toggle(false, GUILayout.Width(xSpacing + 1.11f), GUILayout.Height(ySpacing));
                        } else relationships[row - 1, column - 1] = false;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        scriptableObject.SetRelationships(relationships);

        // End the grid layout
        GUILayout.EndVertical();
        


        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Save Relationships")) {

        }

        if(GUILayout.Button("Clear Relationships")) {

        }
        GUILayout.EndHorizontal();
    }
}