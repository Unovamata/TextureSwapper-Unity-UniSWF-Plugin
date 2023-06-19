using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;

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
    
    public static Texture2D CreateTransparent2DTexture(int w, int h) {
        //Generating the textures & mapping it to a transparent color;
        Texture2D texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color32[] emptyTexturePixels = new Color32[w * h];
        Color32 transparentColor = new Color32(0, 0, 0, 0);
        System.Array.Fill(emptyTexturePixels, transparentColor);
        texture.SetPixels32(emptyTexturePixels);
        texture.Apply();
        return texture;
    }

    public static void SaveTexture(Texture2D textureToSave, string savePath){
        byte[] bytes = textureToSave.EncodeToPNG();
        File.WriteAllBytes(savePath, bytes);
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
            Texture2D resizedTexture = BisharpScalingInterpolation(pasteTexture, w, h, sourceWidth, sourceHeight);
            Graphics.CopyTexture(resizedTexture, 0, 0, 0, 0, w, h, skeletonTexture, 0, 0, x, y);
        }
        else{
            // If not, just paste it;
            Graphics.CopyTexture(pasteTexture, 0, 0, 0, 0, sourceWidth, sourceHeight, skeletonTexture, 0, 0, x, y);
        }

        skeletonTexture.Apply();
    }

    //Use bicubic interpolation to scale the texture;
    public static Texture2D BisharpScalingInterpolation(Texture2D pasteTexture, int w, int h, int sourceWidth, int sourceHeight) {
        Texture2D resizedTexture = new Texture2D(w, h);
        Color32[] resizedPixels = new Color32[w * h];

        // Bisharp scaling algorithm
        float scaleX = (float)sourceWidth / w;
        float scaleY = (float)sourceHeight / h;

        for (int i = 0; i < h; i++){
            for (int j = 0; j < w; j++){
                float sourceX = j * scaleX;
                float sourceY = i * scaleY;

                int left = Mathf.FloorToInt(sourceX);
                int top = Mathf.FloorToInt(sourceY);
                int right = Mathf.CeilToInt(sourceX);
                int bottom = Mathf.CeilToInt(sourceY);

                float blendX = sourceX - left;
                float blendY = sourceY - top;

                Color32 pixel00 = pasteTexture.GetPixel(left, top);
                Color32 pixel01 = pasteTexture.GetPixel(right, top);
                Color32 pixel10 = pasteTexture.GetPixel(left, bottom);
                Color32 pixel11 = pasteTexture.GetPixel(right, bottom);

                Color32 blendedPixel = new Color32();
                blendedPixel.r = (byte)(pixel00.r * (1 - blendX) * (1 - blendY) +
                    pixel01.r * blendX * (1 - blendY) +
                    pixel10.r * (1 - blendX) * blendY +
                    pixel11.r * blendX * blendY);
                blendedPixel.g = (byte)(pixel00.g * (1 - blendX) * (1 - blendY) +
                    pixel01.g * blendX * (1 - blendY) +
                    pixel10.g * (1 - blendX) * blendY +
                    pixel11.g * blendX * blendY);
                blendedPixel.b = (byte)(pixel00.b * (1 - blendX) * (1 - blendY) +
                    pixel01.b * blendX * (1 - blendY) +
                    pixel10.b * (1 - blendX) * blendY +
                    pixel11.b * blendX * blendY);
                blendedPixel.a = (byte)(pixel00.a * (1 - blendX) * (1 - blendY) +
                    pixel01.a * blendX * (1 - blendY) +
                    pixel10.a * (1 - blendX) * blendY +
                    pixel11.a * blendX * blendY);

                resizedPixels[i * w + j] = blendedPixel;
            }
        }

        resizedTexture.SetPixels32(0, 0, w, h, resizedPixels);
        resizedTexture.Apply();

        return resizedTexture;
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
