using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/* The Limb class is mandatory for all dependencies of the 
 * texture swapping system, as they store all the pertinent
 * information to iterate over multiple limbs if needed. */
[System.Serializable]
public class Limb {
    [SerializeField] private Texture2D texture;
    [SerializeField] private string name;
    [SerializeField] private Vector4 coordinates;
    [SerializeField] private Vector2 pivot;
    //The colors folder will always be closed on opening as it is very memory intensive;
    private bool colorsFolder = false;
    //If the limb has different colors to mask, then false. If not, true;
    [SerializeField] private bool passLimbAsMask = true;
    [SerializeField] private List<LimbColor> maskColors = new List<LimbColor>();
    [SerializeField] private string maskRouteReference;
    [SerializeField] private Texture2D maskTextureReference;
    [SerializeField] private List<Texture2D> maskTextures;
    [SerializeField] private Texture2D sourceTexture;

    public Texture2D GetTexture() { return texture; }
    public void SetTexture(Texture2D _Texture) { texture = _Texture; }
    public string GetName() { return name; }
    public void SetName(string Name) { name = Name; }
    /* x: X; Where the sprite box starts;
     * y: Y; Where the sprite box ends;
     * z: Width; Size of the texture;
     * w: Height; Size of the texture; */
    public Vector4 GetCoordinates() { return coordinates; }
    public void SetCoordinates(Vector4 Coordinates) { coordinates = Coordinates; }
    public int GetX() { return (int) coordinates.x; }
    public void SetX(int X) { coordinates.x = X; }
    public int GetY() { return (int) coordinates.y; }
    public void SetY(int Y) { coordinates.y = Y; }
    public int GetWidth() { return (int) coordinates.z; }
    public void SetWidth(int Width) { coordinates.z = Width; }
    public int GetHeight() { return (int) coordinates.w; }
    public void SetHeight(int Height) { coordinates.w = Height; }
    public Vector2 GetPivot() { return pivot; }
    public void SetPivot(Vector2 Pivot) { pivot = Pivot; }
    public bool GetColorsFolder() { return colorsFolder; }
    public void SetColorsFolder(bool ColorsFolder) { colorsFolder = ColorsFolder; }
    public bool GetPassLimbAsMask() { return passLimbAsMask; }
    public void SetPassLimbAsMask(bool PassLimbAsMask) { passLimbAsMask = PassLimbAsMask; }
    public List<LimbColor> GetMaskColors() { return maskColors; }
    public void SetMaskColors(List<LimbColor> MaskColor) { maskColors = MaskColor; }
    public LimbColor GetMaskColor(int Index) { return maskColors[Index]; }
    public void SetMaskColor(int Index, LimbColor MaskColor) { maskColors[Index] = MaskColor; }
    public void AddMaskColor(LimbColor @Color) { maskColors.Add(@Color); }
    public void RemoveMaskColor(LimbColor @Color) { maskColors.Remove(@Color); }
    public void RemoveAtMaskColor(int Index) { maskColors.RemoveAt(Index); }
    public void ClearMaskColors() { maskColors = new List<LimbColor>(); }
    public string GetMaskRouteReference() { return maskRouteReference; }
    public void SetMaskRouteReference(string MaskRouteReference) { maskRouteReference = MaskRouteReference; }
    public Texture2D GetMaskReference() { return maskTextureReference; }
    public void SetMaskReference(Texture2D MaskTextureReference) { maskTextureReference = MaskTextureReference; }
    public void UpdateMaskTextureReference() {
        byte[] fileData = File.ReadAllBytes(maskRouteReference);
        maskTextureReference = new Texture2D(GetWidth(), GetHeight());
        maskTextureReference.LoadImage(fileData);
    }
    public List<Texture2D> GetMaskTextures() { return maskTextures; }
    public void SetMaskTextures(List<Texture2D> MaskTextures) { maskTextures = MaskTextures; }
    public void AddMaskTexture(Texture2D texture){ maskTextures.Add(texture); }
    public void RemoveMaskTexture(int index){ maskTextures.RemoveAt(index); }
    public void RemoveMaskTexture(Texture2D texture){ maskTextures.Remove(texture); }
    public void ClearMaskTextures(){ maskTextures = new List<Texture2D>(); }
    public void SetSourceTexture(Texture2D SourceTexture){ sourceTexture = SourceTexture; }
    public Texture2D GetSourceTexture(){ return sourceTexture; }
}

public class LimbColor {
    [SerializeField] private Color color;
    [SerializeField] private int threshold;

    public LimbColor(Color @Color, int Threshold) {
        color = @Color;
        threshold = Threshold;
    }
    public Color GetColor() { return color; }
    public void SetColor(Color @Color) { color = @Color; }
    public int GetThreshold() { return  threshold; }
    public void SetThreshold(int Threshold) { threshold = Threshold; }
}