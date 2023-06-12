using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Prefs {
    padding = 50,
}

public class Utils{
    //Clone a reference texture;
    public static Texture2D CloneTexture(Texture2D original) {
        Texture2D clone = new Texture2D(original.width, original.height);
        clone.SetPixels(original.GetPixels());
        clone.Apply();
        return clone;
    }
    
    //Remove a specific texture at a position;
    public static void ClearTextureAt(Vector4 v, Texture2D texture) {
        //Get transparent color;
        Color32[] pixels = FillArrayColor(Color.clear, (int) v.z, (int) v.w);

        //Save the converted pixels;
        texture.SetPixels32((int) v.x, (int) v.y, (int) v.z, (int) v.w, pixels);
        texture.Apply();
    }


    //Paste a specific limb texture to the texture sheet;
    public static void PasteTexture(Limb limb, Texture2D skeletonTexture, TexturesSO reference) {
        //If there are limbs missing, return; 
        if(!reference.GetLimbs().Any(x => x.GetName().Equals(limb.GetName()))) return;

        Vector4 coordinates = limb.GetCoordinates();
        int x = (int)coordinates.x, y = (int)coordinates.y, 
            w = (int)coordinates.z, h = (int)coordinates.w;

        //Extracting the texture to paste from the limb referenced;
        Texture2D pasteTexture = reference.CallLimb(limb.GetName()).GetTexture();
        int sourceWidth = pasteTexture.width;
        int sourceHeight = pasteTexture.height;

        // Resize the source texture, if necessary;
        if (sourceWidth != w || sourceHeight != h){
            Texture2D resizedTexture = new Texture2D(w, h);
            Color32[] resizedPixels = new Color32[w * h];

            //Round the pixels of the image to fit it into the new texture;
            for (int i = 0; i < h; i++){
                for (int j = 0; j < w; j++){
                    int sourceX = Mathf.RoundToInt(j * (sourceWidth / (float)w));
                    int sourceY = Mathf.RoundToInt(i * (sourceHeight / (float)h));
                    resizedPixels[i * w + j] = pasteTexture.GetPixel(sourceX, sourceY);
                }
            }

            resizedTexture.SetPixels32(resizedPixels);
            resizedTexture.Apply();

            // Copy the resized texture to the target texture
            Graphics.CopyTexture(resizedTexture, 0, 0, 0, 0, w, h, skeletonTexture, 0, 0, x, y);
        }
        else{
            // If not, just paste it;
            Graphics.CopyTexture(pasteTexture, 0, 0, 0, 0, sourceWidth, sourceHeight, skeletonTexture, 0, 0, x, y);
        }

        skeletonTexture.Apply();
    }

    //Fill a color32 array with a specific color;
    public static Color32[] FillArrayColor(Color color, int w, int h) {
        Color32[] pixels = new Color32[w * h];

        for(int i = 0; i < pixels.Length; i++) {
            pixels[i] = color;
        }

        return pixels;
    }
}
