using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CrAPTextureManagement : MonoBehaviour{
    public static CrAPTextureManagement Instance;

    private void Awake() {
        Instance = this;
    }

    //Data;
    public SkeletonSO skeleton;
    public Texture2D skeletonData;
    [HideInInspector] public Texture2D temporalTexture;

    //Unity Editor;
    Material material;
    MeshRenderer mesh;
    [HideInInspector] public int x, y, w, h;
    public Limb limb;

    //Callers;
    [SerializeField] string limbName = "Head";

    // Start is called before the first frame update
    void Start() {
        mesh = GetComponent<MeshRenderer>();
        material = mesh.material;
        //LoadSkeletonData("SkeletonData", skeleton);

        //Texture to edit;
        temporalTexture = CloneTexture(skeletonData);

        try {
            limb = skeleton.CallLimb(limbName);
            Vector4 coordinates = limb.GetCoordinates();
            x = (int)coordinates.x;
            y = (int)coordinates.y;
            w = (int)coordinates.z;
            h = (int)coordinates.w;
        } catch { 
            Debug.LogError("There's no limb with the name: " + limbName);
        }
        
        material.mainTexture = temporalTexture;
        mesh.material = material;
        enabled = false;
        //AssetDatabase.CreateAsset(mesh, "Mesh");
        //AssetDatabase.SaveAssets();
    }

    // Update is called once per frame
    void Update(){
        material.mainTexture = temporalTexture;
        mesh.material = material;
    }

    public static void LoadSkeletonData(string route, SkeletonSO skeleton) {
        //Sprite has to be located in the "Resources" folder;
        UnityEngine.Sprite[] sprites = Resources.LoadAll<UnityEngine.Sprite>(route);
        string path = CreateFolder(skeleton.name);

        //Extracting limb data;
        foreach(UnityEngine.Sprite sprite in sprites) {
            //Formatting the limb;
            Limb limb = new Limb();
            limb.SetName(sprite.name);
            int w = (int) sprite.rect.width, h = (int) sprite.rect.height;

            limb.SetCoordinates(new Vector4(sprite.rect.x, sprite.rect.y, w, h));
            /* 0: X; Where the sprite box starts;
             * 1: Y; Where the sprite box ends;
             * 2: Width; Size of the texture;
             * 3: Height; Size of the texture; */

            //Sprite center;
            limb.SetPivot(new Vector2(sprite.pivot.x / w, sprite.pivot.y / h));

            //Generating the textures & mapping it to a transparent color;
            Texture2D texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color32[] emptyTexturePixels = new Color32[w * h];
            Color32 transparentColor = new Color32(0, 0, 0, 0);
            System.Array.Fill(emptyTexturePixels, transparentColor);
            texture.SetPixels32(emptyTexturePixels);
            texture.Apply();

            //Mapping the limb to the texture;
            Rect rect = sprite.textureRect;
            Color[] pixels = sprite.texture.GetPixels((int) rect.x, (int) rect.y, (int) rect.width, (int) rect.height);

            texture.SetPixels(pixels);
            texture.Apply();
            
            // Saving the texture as an asset
            string texturePath = path + "/" + sprite.name + ".asset"; // Specify the desired path and filename
            AssetDatabase.CreateAsset(texture, texturePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            limb.SetTexture(AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));

            //Saving the data;
            skeleton.AddLimb(limb);
        }
    }

    public static string CreateFolder(string skeletonName) {
        string baseRoute = "Assets/Textures";
        string folderPath = baseRoute + "/" + skeletonName;

        if (!AssetDatabase.IsValidFolder(folderPath)) {
            AssetDatabase.CreateFolder(baseRoute, skeletonName);
            Debug.Log("Folder Created: " + skeletonName);
        }

        return folderPath;
    }

    private Texture2D CloneTexture(Texture2D original) {
        Texture2D clone = new Texture2D(original.width, original.height);
        clone.SetPixels(original.GetPixels());
        clone.Apply();
        return clone;
    }

    public void ClearTextureAt(int x, int y, int w, int h) {
        Color[] c = FillArrayColor(Color.clear, w, h);
        temporalTexture.SetPixels(x, y, w, h, c);
        temporalTexture.Apply();
    }

    public void PasteTexture(Limb limb, Texture2D skeletonTexture) {
        Vector4 coordinates = limb.GetCoordinates();
        int x = (int)coordinates.x, y = (int)coordinates.y, w = (int)coordinates.z, h = (int)coordinates.w;
        Color[] colors = limb.GetTexture().GetPixels();
        skeletonTexture.SetPixels(x, y, w, h, colors);
        skeletonTexture.Apply();
    }

    public static Color[] FillArrayColor(Color color, int w, int h) {
        return Enumerable.Repeat(color, w * h).ToArray();
    }
}

//Custom Editor;
[CustomEditor(typeof(CrAPTextureManagement))]
public class CustomInspector : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        CrAPTextureManagement manager = (CrAPTextureManagement) target; //Referencing the script;

        EditorGUILayout.LabelField("Texture Management");

        try {
            foreach(Limb limb in manager.skeleton.GetLimbs()) {
                string name = limb.GetName();
                Vector4 coordinates = limb.GetCoordinates();
                int x = (int)coordinates.x;
                int y = (int)coordinates.y;
                int w = (int)coordinates.z;
                int h = (int)coordinates.w;

                if(GUILayout.Button("Delete " + name)) {
                    manager.ClearTextureAt(x, y, w, h);
                }

                if(GUILayout.Button("Paste " + name)) {
                    manager.PasteTexture(limb, manager.temporalTexture);
                }
            }
        } catch { }
    }
}