using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureSwapperTrigger : MonoBehaviour{
    [SerializeField] TextureManagement textureManagementReference;
    [SerializeField] string relationshipName = "";
    [SerializeField] int selectedTextureSet = 0;

    public void SwapTextureRunProcess(){
        TextureManagement.SwapTextureSearchRelationshipAsync(textureManagementReference, relationshipName, selectedTextureSet);
    }
}
