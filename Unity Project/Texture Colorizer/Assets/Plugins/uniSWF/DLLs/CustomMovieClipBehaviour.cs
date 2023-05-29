// Decompiled with JetBrains decompiler
// Type: MovieClipBehaviour
// Assembly: LibUniSWF, Version=1.1.0.4, Culture=neutral, PublicKeyToken=null
// MVID: ECA667DE-E663-4CF8-BB22-A8C2F545850C

using pumpkin.display;
using pumpkin.displayInternal;
using pumpkin.events;
using pumpkin.swf;
using System;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("uniSWF/CustomMovieClipBehaviour")]
public class CustomMovieClipBehaviour : MonoBehaviour{
    public static Vector2 defaultDrawScale = new Vector2(0.01f, 0.01f);
    public static bool defaultUseAccurateTiming = true;
    public string swf = null;
    public string symbolName = null;
    public string gotoAndStopLabel = null;
    public int gotoAndStopFrame = 0;
    protected bool enableCache = false;
    public bool billboard = false;
    public Camera billboardCamera = null;
    public bool loop = true;
    public bool flipY = false;
    public Vector2 drawScale = Vector2.zero;
    public bool editorPreview = true;
    public MovieClipBlendMode blendMode = MovieClipBlendMode.Normal;
    public int fps = 30;
    public Color colorTransform = Color.white;
    public bool staticRemoveOnStart = false;
    public bool useFastRenderer = false;
    public bool useSmoothTime = false;
    public bool useAccurateTiming = defaultUseAccurateTiming;
    public bool renderOnAwake = true;
    [NonSerialized]
    public bool enableMeshRenderer = true;
    [Obsolete("has no effect, use meshGeneratorOptions")]
    [HideInInspector]
    public bool doubleBufferMesh = false;
    [NonSerialized]
    public MovieClip movieClip;
    [NonSerialized]
    public Stage stage;
    [NonSerialized]
    public IGraphicsGenerator gfxGenerator;
    public MeshGeneratorOptions meshGeneratorOptions = new MeshGeneratorOptions();
    public bool readOnlyMovieClip = false;
    protected MeshFilter meshFilter;
    protected MeshRenderer meshRenderer;
    protected bool m_Is3D = true;
    protected double lastInterval;
    protected double updateInterval;
    protected DisplayObject currentMouseOver;
    protected bool lastWasDown = false;
    protected Vector2 lastMousePos;
    protected bool m_EnableMouse = true;
    protected bool lastTouchWasDown = false;
    protected DisplayObject lastUnderMouse;
    private string lastMovieClip = null;
    private string lastSymbolName = null;
    private CEvent m_EnterFrameEvent;
    protected bool _enableRender = true;
    private double t;
    private double frameDrift = 0.0;
    public bool drawMeshMode = false;
    private SimpleStageRenderResult m_LastRenderMesh;
    private float m_DrawMeshZSpace = 0f;
    protected Vector3 m_TmpVector = default(Vector3);

    public int currentFrame {
        get {
            return movieClip.getCurrentFrame();
        }
        set {
            movieClip.setFrame(value);
        }
    }

    public float zDrawDisplayObjectSpace {
        get {
            if (gfxGenerator is GraphicsMeshGenerator) {
                return ((GraphicsMeshGenerator)gfxGenerator).zSpace;
            }

            if (gfxGenerator is GraphicsDrawMeshGenerator) {
                return ((GraphicsDrawMeshGenerator)gfxGenerator).zSpace;
            }

            return 0f;
        }
        set {
            if (gfxGenerator is GraphicsMeshGenerator) {
                ((GraphicsMeshGenerator)gfxGenerator).zSpace = value;
            }

            if (gfxGenerator is GraphicsDrawMeshGenerator) {
                ((GraphicsDrawMeshGenerator)gfxGenerator).zSpace = value;
            }
        }
    }

    public float zDrawDisplayObjectContainerSpace {
        get {
            if (gfxGenerator is GraphicsMeshGenerator) {
                return ((GraphicsMeshGenerator)gfxGenerator).zContainerSpace;
            }

            if (gfxGenerator is GraphicsDrawMeshGenerator) {
                return ((GraphicsDrawMeshGenerator)gfxGenerator).zContainerSpace;
            }

            return 0f;
        }
        set {
            if (gfxGenerator is GraphicsMeshGenerator) {
                ((GraphicsMeshGenerator)gfxGenerator).zContainerSpace = value;
            }

            if (gfxGenerator is GraphicsDrawMeshGenerator) {
                ((GraphicsDrawMeshGenerator)gfxGenerator).zContainerSpace = value;
            }
        }
    }

    public Mesh renderMesh {
        get {
            if (drawMeshMode) {
                if (m_LastRenderMesh != null) {
                    return m_LastRenderMesh.mesh;
                }
            }
            else if ((bool)meshFilter) {
                return meshFilter.mesh;
            }

            return null;
        }
    }

    public virtual void Awake() {
        if (!Application.isPlaying && !editorPreview) {
            return;
        }

        enableCache = false;
        m_EnterFrameEvent = new CEvent(CEvent.ENTER_FRAME, bubbles: false, cancelable: false);
        meshFilter = (MeshFilter)base.gameObject.GetComponent(typeof(MeshFilter));
        meshRenderer = (MeshRenderer)base.gameObject.GetComponent(typeof(MeshRenderer));
        setDrawMeshMode(drawMeshMode);
        gfxGenerator = instanceGfxGenerator();
        if (gfxGenerator != null) {
            setMeshGeneratorOptions(meshGeneratorOptions);
        }

        if (drawScale == Vector2.zero) {
            drawScale = getDefaultDrawScale();
        }

        stage = new Stage();
        stage.host = this;
        stage.scaleX = drawScale.x;
        if (flipY) {
            stage.scaleY = drawScale.y;
        }
        else {
            stage.scaleY = 0f - drawScale.y;
        }

        SwfURI swfURI = new SwfURI();
        swfURI.swf = swf;
        swfURI.linkage = symbolName;
        movieClip = (readOnlyMovieClip ? new ReadonlyMovieClip(swfURI) : new MovieClip(swfURI));
        stage.addChild(movieClip);
        if (gotoAndStopLabel != null && gotoAndStopLabel.Length > 0) {
            movieClip.gotoAndStop(gotoAndStopLabel);
        }

        if (gotoAndStopFrame > 0) {
            movieClip.gotoAndStop(gotoAndStopFrame);
        }

        movieClip.looping = loop;
        movieClip.colorTransform = colorTransform;
        movieClip.alpha = colorTransform.a;
        updateInterval = 1f / (float)fps;
        lastInterval = 0.0;
        movieClip.blendMode = (int)blendMode;
        m_EnableMouse = true;
        if (Application.platform == RuntimePlatform.IPhonePlayer) {
            m_EnableMouse = false;
        }

        lastMovieClip = swf;
        lastSymbolName = symbolName;
        if (Application.isEditor) {
            UniSWFSharedAssetManagerBehaviour.checkPreloadExecutionOrder();
        }

        if (renderOnAwake) {
            generateMesh();
            if (!useAccurateTiming) {
                lastInterval = updateInterval;
            }
        }
    }

    public MovieClip loadMovieClip(string linkage) {
        stage.removeAllChildren();
        SwfURI swfURI = new SwfURI(linkage);
        swf = swfURI.swf;
        symbolName = swfURI.linkage;
        movieClip = (readOnlyMovieClip ? new ReadonlyMovieClip(swfURI) : new MovieClip(swfURI));
        stage.addChild(movieClip);
        return movieClip;
    }

    protected virtual IGraphicsGenerator instanceGfxGenerator() {
        IGraphicsGenerator graphicsGenerator = null;
        if (useFastRenderer) {
            graphicsGenerator = new FastGraphicsDrawMeshGenerator();
            FastGraphicsDrawMeshGenerator fastGraphicsDrawMeshGenerator = graphicsGenerator as FastGraphicsDrawMeshGenerator;
            fastGraphicsDrawMeshGenerator.zSpace = 0.0001f;
        }
        else {
            graphicsGenerator = new GraphicsMeshGenerator();
            GraphicsMeshGenerator graphicsMeshGenerator = graphicsGenerator as GraphicsMeshGenerator;
            graphicsMeshGenerator.enableCache = enableCache;
        }

        return graphicsGenerator;
    }

    public virtual void Update() {
        bool isPlaying = Application.isPlaying;
        if (Application.isEditor && !isPlaying) {
            if (!editorPreview) {
                return;
            }

            if ((lastMovieClip != null && swf != null && lastMovieClip.CompareTo(swf) != 0) || (lastSymbolName != null && symbolName != null && lastSymbolName.CompareTo(symbolName) != 0)) {
                Awake();
            }
        }

        if (enableMeshRenderer != _enableRender) {
            _enableRender = enableMeshRenderer;
            if ((bool)meshRenderer) {
                meshRenderer.enabled = _enableRender;
            }
        }

        if (movieClip != null) {
            bool flag = false;
            if (!useAccurateTiming) {
                float realtimeSinceStartup = Time.realtimeSinceStartup;
                if ((double)realtimeSinceStartup > lastInterval + updateInterval) {
                    renderFrame();
                    flag = true;
                }
            }
            else {
                float num = useSmoothTime ? Time.smoothDeltaTime : Time.deltaTime;
                t += num;
                if (t >= updateInterval) {
                    frameDrift = t - updateInterval;
                    t = frameDrift;
                    if (t > updateInterval) {
                        t = updateInterval;
                    }

                    renderFrame();
                    flag = true;
                }
            }

            if (!isPlaying) {
                return;
            }

            if (flag) {
                handleUserInput();
            }
        }

        if (billboard) {
            Transform transform = (billboardCamera != null) ? billboardCamera.transform : findMainCamera().transform;
            base.transform.eulerAngles = transform.eulerAngles;
        }

        if (Application.isPlaying && staticRemoveOnStart) {
            UnityEngine.Object.Destroy(this);
        }
        else if (drawMeshMode && _enableRender && m_LastRenderMesh != null && (bool)m_LastRenderMesh.mesh) {
            for (int i = 0; i < m_LastRenderMesh.mesh.subMeshCount; i++) {
                m_TmpVector.x = 0f;
                m_TmpVector.y = 0f;
                m_TmpVector.z = (0f - m_DrawMeshZSpace) * (float)i;
                Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
                Matrix4x4 rhs = default(Matrix4x4);
                rhs.SetTRS(m_TmpVector, Quaternion.identity, Vector3.one);
                UnityEngine.Graphics.DrawMesh(m_LastRenderMesh.mesh, localToWorldMatrix * rhs, m_LastRenderMesh.materials[i], base.gameObject.layer, null, i);
            }
        }
    }

    protected Camera findMainCamera() {
        if (Camera.main != null) {
            return Camera.main;
        }

        Camera camera = (Camera)UnityEngine.Object.FindObjectOfType(typeof(Camera));
        if (camera == null) {
            throw new Exception("Failed to find a Camera in the scene");
        }

        return camera;
    }

    protected virtual void handleUserInput() {
    }

    public void calcOthoScale(Camera camera) {
        float orthographicSize = camera.orthographicSize;
        float num = Screen.height / 2;
        float num2 = 1f / num * orthographicSize;
        base.transform.localScale = new Vector3(num2, num2, num2);
    }

    public virtual Vector2 getDefaultDrawScale() {
        return defaultDrawScale;
    }

    public void setUri(string uri) {
        stage.removeChild(movieClip);
        SwfURI swfURI = new SwfURI(uri);
        swf = swfURI.swf;
        symbolName = swfURI.linkage;
        gotoAndStopLabel = (string)swfURI.label;
        movieClip = (readOnlyMovieClip ? new ReadonlyMovieClip(uri) : new MovieClip(swfURI));
        stage.addChild(movieClip);
    }

    public void renderFrame() {
        stage.updateFrame(m_EnterFrameEvent);
        lastInterval = Time.realtimeSinceStartup;
        if (drawMeshMode && Application.isPlaying) {
            if (m_Is3D && gfxGenerator != null && gfxGenerator.renderStage(stage)) {
                m_LastRenderMesh = gfxGenerator.getSimpleStageRenderResult();
            }
        }
        else if (m_Is3D && (bool)meshFilter && gfxGenerator != null && gfxGenerator.renderStage(stage)) {
            meshFilter.mesh = gfxGenerator.applyToMeshRenderer(meshRenderer);
        }
    }

    public Mesh generateMesh() {
        lastInterval = Time.realtimeSinceStartup;
        if (m_Is3D && (bool)meshFilter && gfxGenerator != null && gfxGenerator.renderStage(stage)) {
            meshFilter.mesh = gfxGenerator.applyToMeshRenderer(meshRenderer);
            return meshFilter.sharedMesh;
        }

        return null;
    }

    public void setFps(float fps) {
        this.fps = (int)fps;
        updateInterval = 1f / fps;
        lastInterval = 0.0;
        frameDrift = 0.0;
        t = 0.0;
    }

    public virtual void setDrawMeshMode(bool setting) {
        drawMeshMode = setting;
        if (drawMeshMode) {
            if (Application.isPlaying) {
                if ((bool)meshFilter) {
                    UnityEngine.Object.DestroyImmediate(meshFilter);
                    meshFilter = null;
                }

                if ((bool)meshRenderer) {
                    UnityEngine.Object.DestroyImmediate(meshRenderer);
                    meshRenderer = null;
                }
            }

            m_LastRenderMesh = new SimpleStageRenderResult();
        }
        else {
            if (!meshFilter) {
                base.gameObject.AddComponent<MeshFilter>();
            }

            if (!meshRenderer) {
                base.gameObject.AddComponent<MeshRenderer>();
            }

            meshFilter = (MeshFilter)base.gameObject.GetComponent(typeof(MeshFilter));
            meshRenderer = (MeshRenderer)base.gameObject.GetComponent(typeof(MeshRenderer));
            m_LastRenderMesh = null;
        }
    }

    public void setMeshGeneratorOptions(MeshGeneratorOptions meshGeneratorOptions) {
        this.meshGeneratorOptions = meshGeneratorOptions;
        gfxGenerator.meshGeneratorOptions = meshGeneratorOptions;
    }
}