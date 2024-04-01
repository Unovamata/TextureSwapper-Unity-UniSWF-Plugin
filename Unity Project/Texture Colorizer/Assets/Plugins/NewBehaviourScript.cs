// Decompiled with JetBrains decompiler
// Type: pumpkin.displayInternal.GraphicsMeshGenerator
// Assembly: LibUniSWF, Version=1.1.0.4, Culture=neutral, PublicKeyToken=null
// MVID: ECA667DE-E663-4CF8-BB22-A8C2F545850C
// Assembly location: C:\Users\Admin\Documents\GitHub\Unity-UniSWF-Texture-Manager\Unity Project\Texture Colorizer\Assets\Plugins\uniSWF\DLLs\LibUniSWF.dll
/*
using pumpkin.display;
using pumpkin.geom;
using pumpkin.swf;
using pumpkin.utils;
using System;
using UnityEngine;

#nullable disable
namespace pumpkin.displayInternal
{
  [Serializable]
  public class GraphicsMeshGenerator : IGraphicsGenerator, IGraphicsGeneratorDataPool
  {
    public const int PHASE_SCAN = 0;
    public const int PHASE_BUILD = 1;
    public const int PHASE_NULL = 2;
    public Material currentMaterial;
    private int subMeshId = 0;
    public bool generateNormals = false;
    public float zSpace = 0.0001f;
    public float zContainerSpace = 0.0f;
    public bool enableCache = true;
    public bool simpleGeneration = true;
    public Vector3[] vertices = new Vector3[0];
    public Vector3[] normals = new Vector3[0];
    public Vector2[] UVs = new Vector2[0];
    public Color[] colors = new Color[0];
    public IGraphicsGeneratorDataPool dataPool;
    public FastList<Material> materialList = new FastList<Material>();
    public FastList<int[]> submeshIndices = new FastList<int[]>();
    protected float zDrawOffset = 0.0f;
    protected int quadCount;
    protected int phaseId = 0;
    protected int numVerts = 0;
    protected int numUVs = 0;
    protected int numColours = 0;
    protected int numIndex = 0;
    protected int VertId = 0;
    protected int UVId = 0;
    protected int ColourId = 0;
    protected int triId = 0;
    protected Stage stage;
    public int updateCounter = -2;
    public Mesh mesh_0;
    public Mesh mesh_1;
    private bool m_meshSwitch = true;
    private SwfGraphicsRenderState renderState = new SwfGraphicsRenderState();
    private Vector2 uvPos = new Vector2();
    private Vector2 tmpUv;
    private Vector2 lowerLeftUV;
    private Vector2 UVDimensions;
    private Color white = Color.white;
    private MeshGeneratorOptions m_Options = new MeshGeneratorOptions();
    public SimpleStageRenderResult m_SimpleMeshResult = new SimpleStageRenderResult();
    private GenericUseOrderCyclingArrayPool<int> indexPool = new GenericUseOrderCyclingArrayPool<int>();
    private GenericUseOrderCyclingArrayPool<Vector3> vertexPool = new GenericUseOrderCyclingArrayPool<Vector3>();
    private GenericUseOrderCyclingArrayPool<Vector2> uvPool = new GenericUseOrderCyclingArrayPool<Vector2>();
    private GenericUseOrderCyclingArrayPool<Color> colorPool = new GenericUseOrderCyclingArrayPool<Color>();

    public GraphicsMeshGenerator() => this.dataPool = (IGraphicsGeneratorDataPool) this;

    public bool renderStage(Stage stage)
    {
      this.stage = stage;
      this.submeshIndices.Clear();
      this.materialList.Clear();
      this.dataPool.preCycleAllocation((object) this);
      this.currentMaterial = (Material) null;
      this.subMeshId = -1;
      this.quadCount = 0;
      this.zDrawOffset = 0.0f;
      this.phaseId = 0;
      this.numVerts = 0;
      this.numUVs = 0;
      this.numColours = 0;
      this.numIndex = 0;
      this.renderState.blendMode = 0;
      this.renderState.colorTransform.r = this.renderState.colorTransform.g = this.renderState.colorTransform.b = this.renderState.colorTransform.a = 1f;
      this.renderState.parentHasClipRect = false;
      this.renderState.clipRectParent = (DisplayObject) null;
      this.renderState.clipRectCached = false;
      this.renderDisplayObjectContainer((DisplayObjectContainer) stage);
      this.materialList.Add(this.currentMaterial);
      this.submeshIndices.Add(this.dataPool.allocIndexArray(this.numIndex));
      this.getCurrentMesh().Clear();
      if (this.vertices.Length != this.numVerts)
        this.vertices = this.dataPool.allocVertexArray(this.numVerts);
      if (this.UVs.Length != this.numVerts)
        this.UVs = this.dataPool.allocUVArray(this.numVerts);
      if (this.colors.Length != this.numVerts)
        this.colors = this.dataPool.allocColorArray(this.numVerts);
      this.currentMaterial = (Material) null;
      this.subMeshId = -1;
      this.quadCount = 0;
      this.zDrawOffset = 0.0f;
      this.phaseId = 1;
      this.VertId = 0;
      this.UVId = 0;
      this.ColourId = 0;
      this.triId = 0;
      this.renderState.blendMode = 0;
      this.renderState.colorTransform.r = this.renderState.colorTransform.g = this.renderState.colorTransform.b = this.renderState.colorTransform.a = 1f;
      this.renderState.parentHasClipRect = false;
      this.renderState.clipRectParent = (DisplayObject) null;
      this.renderState.clipRectCached = false;
      this.renderDisplayObjectContainer((DisplayObjectContainer) stage);
      this.updateCounter = Time.frameCount;
      this.dataPool.preCycleAllocation((object) this);
      for (int i = 0; i < this.submeshIndices.Count; ++i)
        this.dataPool.releaseIndexArray(this.submeshIndices[i]);
      if (this.vertices != null)
        this.dataPool.releaseVertexArray(this.vertices);
      if (this.colors != null)
        this.dataPool.releaseColorArray(this.colors);
      if (this.UVs != null)
        this.dataPool.releaseUVArray(this.UVs);
      if (this.m_Options.doubleBuffered)
        this.m_meshSwitch = !this.m_meshSwitch;
      return true;
    }

    public Mesh applyToMeshRenderer(MeshRenderer meshRenderer)
    {
      if (this.vertices == null || this.vertices.Length == 0)
        return (Mesh) null;
      Mesh currentMesh = this.getCurrentMesh();
      currentMesh.vertices = this.vertices;
      if (this.generateNormals)
        currentMesh.normals = this.normals;
      currentMesh.uv = this.UVs;
      currentMesh.colors = this.colors;
      if (Object.op_Implicit((Object) meshRenderer))
      {
        this.materialList.resizeToCount();
        ((Renderer) meshRenderer).materials = this.materialList.m_Buffer;
      }
      currentMesh.subMeshCount = this.submeshIndices.Count;
      for (int i = 0; i < this.submeshIndices.Count; ++i)
      {
        int[] submeshIndex = this.submeshIndices[i];
        currentMesh.SetTriangles(submeshIndex, i);
      }
      return currentMesh;
    }

    private void renderDisplayObjectContainer(DisplayObjectContainer parent)
    {
      if (this.phaseId == 0 || this.phaseId != 1)
        ;
      if (!parent.visible)
        return;
      int blendMode = this.renderState.blendMode;
      if (parent.blendMode != 0)
        this.renderState.blendMode = parent.blendMode;
      Color colorTransform = this.renderState.colorTransform;
      this.renderState.colorTransform = Color.op_Multiply(parent.colorTransform, this.renderState.colorTransform);
      for (int id = 0; id < parent.numChildren; ++id)
      {
        DisplayObject childAt = parent.getChildAt(id);
        switch (childAt)
        {
          case Sprite _:
            this.renderSprite(childAt as Sprite);
            break;
          case DisplayObjectContainer _:
            this.renderDisplayObjectContainer(childAt as DisplayObjectContainer);
            break;
        }
      }
      this.renderState.colorTransform = colorTransform;
      this.renderState.blendMode = blendMode;
    }

    private void renderSprite(Sprite sprite)
    {
      if (!sprite.visible)
        return;
      Graphics graphics = sprite.graphics;
      bool parentHasClipRect = this.renderState.parentHasClipRect;
      if (sprite.hasClipRect)
      {
        this.renderState.parentHasClipRect = true;
        this.renderState.clipRectParent = (DisplayObject) sprite;
        this.renderState.clipRectCached = false;
      }
      Sprite parent = sprite;
      int[] numArray = (int[]) null;
      if (!parent.visible)
        return;
      int count = graphics.drawOPs.Count;
      for (int index1 = 0; index1 < count; ++index1)
      {
        GraphicsDrawOP drawOp = graphics.drawOPs[index1];
        if (!Object.op_Equality((Object) drawOp.material, (Object) null))
        {
          if (this.renderState.blendMode != 0 && Object.op_Inequality((Object) drawOp.material.shader, (Object) TextureManager.baseBitmapAddShader))
            drawOp.material = TextureManager.instance.createConvertToAdditive(drawOp.material);
          if (this.phaseId == 0)
          {
            if (Object.op_Inequality((Object) this.currentMaterial, (Object) drawOp.material))
            {
              ++this.subMeshId;
              if (Object.op_Implicit((Object) this.currentMaterial))
              {
                this.materialList.Add(this.currentMaterial);
                this.submeshIndices.Add(this.dataPool.allocIndexArray(this.numIndex));
                this.triId = 0;
                this.numIndex = 0;
              }
              this.currentMaterial = drawOp.material;
            }
            if (drawOp.simpleVectorShape != null)
            {
              int length = drawOp.simpleVectorShape.vertices.Length;
              this.numVerts += length;
              this.numUVs += length;
              this.numColours += length;
              if (drawOp.simpleVectorShape.cachedTriangulatedIndex == null)
                drawOp.simpleVectorShape.cachedTriangulatedIndex = Triangulator.triangulate(drawOp.simpleVectorShape.vertices);
              this.numIndex += drawOp.simpleVectorShape.cachedTriangulatedIndex.Length;
              this.quadCount += length;
            }
            else
            {
              this.numVerts += 4;
              this.numUVs += 4;
              this.numColours += 4;
              this.numIndex += 6;
              ++this.quadCount;
            }
            this.zDrawOffset += this.zSpace;
          }
          else if (this.phaseId == 1)
          {
            if (Object.op_Inequality((Object) this.currentMaterial, (Object) drawOp.material))
            {
              ++this.subMeshId;
              this.currentMaterial = drawOp.material;
              this.triId = 0;
              numArray = this.submeshIndices[this.subMeshId];
            }
            Matrix fullMatrixRef = parent._fastGetFullMatrixRef();
            if (drawOp.simpleVectorShape != null)
            {
              Color color = Color.op_Multiply(this.renderState.colorTransform, drawOp.color);
              for (int index2 = 0; index2 < drawOp.simpleVectorShape.vertices.Length; ++index2)
              {
                Vector2 vertex = drawOp.simpleVectorShape.vertices[index2];
                this.vertices[this.VertId++] = fullMatrixRef.transformVector3Static(vertex.x, vertex.y, this.zDrawOffset);
                this.UVs[this.UVId++] = drawOp.simpleVectorShape.uv[index2];
                this.colors[this.ColourId++] = Color.op_Multiply(drawOp.simpleVectorShape.colors[index2], color);
              }
              numArray = this.submeshIndices[this.subMeshId];
              for (int index3 = 0; index3 < drawOp.simpleVectorShape.cachedTriangulatedIndex.Length; ++index3)
                numArray[this.triId++] = drawOp.simpleVectorShape.cachedTriangulatedIndex[index3] + this.quadCount;
              this.quadCount += drawOp.simpleVectorShape.vertices.Length;
            }
            else
            {
              float x = drawOp.x;
              float y = drawOp.y;
              float pW = drawOp.drawWidth;
              float pH = drawOp.drawHeight;
              Rect drawSrcRect = drawOp.drawSrcRect;
              if (this.stage.m_HasClipRect && this.renderState.parentHasClipRect)
              {
                Rect clipRect;
                if (!this.renderState.clipRectCached)
                {
                  clipRect = this.renderState.clipRect = this.renderState.clipRectParent.getInheritedClipRect();
                  this.renderState.clipRectCached = true;
                }
                else
                  clipRect = this.renderState.clipRect;
                if ((double) ((Rect) ref clipRect).width != 0.0 && (double) ((Rect) ref clipRect).height != 0.0)
                {
                  Rect rect = drawOp.calcClipping(parent, clipRect);
                  UVPixelRect uvPixelRect = new UVPixelRect(x, y, pW, pH, ((Rect) ref drawSrcRect).x, ((Rect) ref drawSrcRect).y, ((Rect) ref drawSrcRect).width, ((Rect) ref drawSrcRect).height);
                  double num1 = (double) uvPixelRect.setWidthPixels(((Rect) ref rect).width);
                  double num2 = (double) uvPixelRect.setHeightPixels(((Rect) ref rect).height);
                  double num3 = (double) uvPixelRect.setXPixels(((Rect) ref rect).x);
                  double num4 = (double) uvPixelRect.setYPixels(((Rect) ref rect).y);
                  x = ((Rect) ref rect).x;
                  y = ((Rect) ref rect).y;
                  pW = ((Rect) ref rect).width;
                  pH = ((Rect) ref rect).height;
                  ((Rect) ref drawSrcRect).x = uvPixelRect.uX;
                  ((Rect) ref drawSrcRect).y = uvPixelRect.uY;
                  ((Rect) ref drawSrcRect).width = uvPixelRect.uW;
                  ((Rect) ref drawSrcRect).height = uvPixelRect.uH;
                }
              }
              this.lowerLeftUV.x = ((Rect) ref drawSrcRect).x;
              this.lowerLeftUV.y = 1f - ((Rect) ref drawSrcRect).y;
              this.UVDimensions.x = ((Rect) ref drawSrcRect).width;
              this.UVDimensions.y = ((Rect) ref drawSrcRect).height;
              Vector2 vector2;
              if (drawOp.matrix != null && (double) drawOp.matrixScale.x == 0.0)
              {
                float num5 = 0.0f;
                float num6 = 0.0f;
                Texture mainTexture = drawOp.material.mainTexture;
                if (Object.op_Implicit((Object) mainTexture))
                {
                  num5 = (float) mainTexture.width;
                  num6 = (float) mainTexture.height;
                }
                float num7 = x + pW;
                float num8 = y + 0.0f;
                float inPos_x1 = num7 - drawOp.tilePosX;
                float inPos_y1 = num8 - drawOp.tilePosY;
                vector2 = drawOp.matrix.transformPointStatic(inPos_x1, inPos_y1);
                this.uvPos.x = vector2.x / num5;
                this.uvPos.y = (float) (1.0 - (double) vector2.y / (double) num6);
                this.UVs[this.UVId++] = this.uvPos;
                float num9 = x + pW;
                float num10 = y + pH;
                float inPos_x2 = num9 - drawOp.tilePosX;
                float inPos_y2 = num10 - drawOp.tilePosY;
                vector2 = drawOp.matrix.transformPointStatic(inPos_x2, inPos_y2);
                this.uvPos.x = vector2.x / num5;
                this.uvPos.y = (float) (1.0 - (double) vector2.y / (double) num6);
                this.UVs[this.UVId++] = this.uvPos;
                float num11 = x + 0.0f;
                float num12 = y + pH;
                float inPos_x3 = num11 - drawOp.tilePosX;
                float inPos_y3 = num12 - drawOp.tilePosY;
                vector2 = drawOp.matrix.transformPointStatic(inPos_x3, inPos_y3);
                this.uvPos.x = vector2.x / num5;
                this.uvPos.y = (float) (1.0 - (double) vector2.y / (double) num6);
                this.UVs[this.UVId++] = this.uvPos;
                float num13 = x + 0.0f;
                float num14 = y + 0.0f;
                float inPos_x4 = num13 - drawOp.tilePosX;
                float inPos_y4 = num14 - drawOp.tilePosY;
                vector2 = drawOp.matrix.transformPointStatic(inPos_x4, inPos_y4);
                this.uvPos.x = vector2.x / num5;
                this.uvPos.y = (float) (1.0 - (double) vector2.y / (double) num6);
                this.UVs[this.UVId++] = this.uvPos;
              }
              else if (drawOp.matrix != null && (double) drawOp.matrixScale.x != 0.0)
              {
                float num15 = 0.0f;
                float num16 = 0.0f;
                Texture mainTexture = drawOp.material.mainTexture;
                if (Object.op_Implicit((Object) mainTexture))
                {
                  num15 = (float) mainTexture.width;
                  num16 = (float) mainTexture.height;
                }
                float num17 = x + pW;
                float num18 = y + 0.0f;
                float inPos_x5 = num17 - drawOp.tilePosX;
                float inPos_y5 = num18 - drawOp.tilePosY;
                vector2 = Matrix.transformPointStaticWithInvertedPostScale(drawOp.matrix, inPos_x5, inPos_y5, drawOp.matrixScale.x, drawOp.matrixScale.y);
                this.uvPos.x = vector2.x / num15;
                this.uvPos.y = (float) (1.0 - (double) vector2.y / (double) num16);
                this.UVs[this.UVId++] = this.uvPos;
                float num19 = x + pW;
                float num20 = y + pH;
                float inPos_x6 = num19 - drawOp.tilePosX;
                float inPos_y6 = num20 - drawOp.tilePosY;
                vector2 = Matrix.transformPointStaticWithInvertedPostScale(drawOp.matrix, inPos_x6, inPos_y6, drawOp.matrixScale.x, drawOp.matrixScale.y);
                this.uvPos.x = vector2.x / num15;
                this.uvPos.y = (float) (1.0 - (double) vector2.y / (double) num16);
                this.UVs[this.UVId++] = this.uvPos;
                float num21 = x + 0.0f;
                float num22 = y + pH;
                float inPos_x7 = num21 - drawOp.tilePosX;
                float inPos_y7 = num22 - drawOp.tilePosY;
                vector2 = Matrix.transformPointStaticWithInvertedPostScale(drawOp.matrix, inPos_x7, inPos_y7, drawOp.matrixScale.x, drawOp.matrixScale.y);
                this.uvPos.x = vector2.x / num15;
                this.uvPos.y = (float) (1.0 - (double) vector2.y / (double) num16);
                this.UVs[this.UVId++] = this.uvPos;
                float num23 = x + 0.0f;
                float num24 = y + 0.0f;
                float inPos_x8 = num23 - drawOp.tilePosX;
                float inPos_y8 = num24 - drawOp.tilePosY;
                vector2 = Matrix.transformPointStaticWithInvertedPostScale(drawOp.matrix, inPos_x8, inPos_y8, drawOp.matrixScale.x, drawOp.matrixScale.y);
                this.uvPos.x = vector2.x / num15;
                this.uvPos.y = (float) (1.0 - (double) vector2.y / (double) num16);
                this.UVs[this.UVId++] = this.uvPos;
              }
              else
              {
                this.tmpUv.x = this.lowerLeftUV.x + this.UVDimensions.x;
                this.tmpUv.y = this.lowerLeftUV.y;
                this.UVs[this.UVId++] = this.tmpUv;
                this.tmpUv.x = this.lowerLeftUV.x + this.UVDimensions.x;
                this.tmpUv.y = this.lowerLeftUV.y - this.UVDimensions.y;
                this.UVs[this.UVId++] = this.tmpUv;
                this.tmpUv.x = this.lowerLeftUV.x;
                this.tmpUv.y = this.lowerLeftUV.y - this.UVDimensions.y;
                this.UVs[this.UVId++] = this.tmpUv;
                this.tmpUv.x = this.lowerLeftUV.x;
                this.tmpUv.y = this.lowerLeftUV.y;
                this.UVs[this.UVId++] = this.tmpUv;
              }
              if (!this.simpleGeneration)
              {
                this.colors[this.ColourId++] = this.white;
                this.colors[this.ColourId++] = this.white;
                this.colors[this.ColourId++] = this.white;
                this.colors[this.ColourId++] = this.white;
              }
              else
              {
                Color color = Color.op_Multiply(this.renderState.colorTransform, drawOp.color);
                this.colors[this.ColourId++] = color;
                this.colors[this.ColourId++] = color;
                this.colors[this.ColourId++] = color;
                this.colors[this.ColourId++] = color;
              }
              bool flipped = drawOp.flipped;
              int vertId = this.VertId;
              if (flipped)
              {
                this.vertices[this.VertId++] = fullMatrixRef.transformVector3Static(x + 0.0f, y + 0.0f, this.zDrawOffset);
                this.vertices[this.VertId++] = fullMatrixRef.transformVector3Static(x + 0.0f, y + pH, this.zDrawOffset);
                this.vertices[this.VertId++] = fullMatrixRef.transformVector3Static(x + pW, y + pH, this.zDrawOffset);
                this.vertices[this.VertId++] = fullMatrixRef.transformVector3Static(x + pW, y + 0.0f, this.zDrawOffset);
              }
              else
              {
                this.vertices[this.VertId++] = fullMatrixRef.transformVector3Static(x + pW, y + 0.0f, this.zDrawOffset);
                this.vertices[this.VertId++] = fullMatrixRef.transformVector3Static(x + pW, y + pH, this.zDrawOffset);
                this.vertices[this.VertId++] = fullMatrixRef.transformVector3Static(x + 0.0f, y + pH, this.zDrawOffset);
                this.vertices[this.VertId++] = fullMatrixRef.transformVector3Static(x + 0.0f, y + 0.0f, this.zDrawOffset);
              }
              if (this.generateNormals)
              {
                Vector3[] normals1 = this.normals;
                int index4 = vertId;
                int num25 = index4 + 1;
                normals1[index4] = Vector3.forward;
                Vector3[] normals2 = this.normals;
                int index5 = num25;
                int num26 = index5 + 1;
                normals2[index5] = Vector3.forward;
                Vector3[] normals3 = this.normals;
                int index6 = num26;
                int num27 = index6 + 1;
                normals3[index6] = Vector3.forward;
                Vector3[] normals4 = this.normals;
                int index7 = num27;
                int num28 = index7 + 1;
                normals4[index7] = Vector3.forward;
              }
              if (!this.simpleGeneration && ((double) parent.rotateX != 0.0 || (double) parent.rotateY != 0.0 || (double) parent.rotateZ != 0.0))
              {
                Matrix4x4 matrix4x4 = Utils.MatrixFromQuaternion(Quaternion.Euler(parent.rotateX, parent.rotateY, parent.rotateZ));
                this.vertices[this.VertId - 4] = ((Matrix4x4) ref matrix4x4).MultiplyVector(this.vertices[this.VertId - 4]);
                this.vertices[this.VertId - 3] = ((Matrix4x4) ref matrix4x4).MultiplyVector(this.vertices[this.VertId - 3]);
                this.vertices[this.VertId - 2] = ((Matrix4x4) ref matrix4x4).MultiplyVector(this.vertices[this.VertId - 2]);
                this.vertices[this.VertId - 1] = ((Matrix4x4) ref matrix4x4).MultiplyVector(this.vertices[this.VertId - 1]);
              }
              if ((double) parent.zWorldOffset != 0.0)
              {
                float zWorldOffset = parent.zWorldOffset;
                this.vertices[this.VertId - 4].z += zWorldOffset;
                this.vertices[this.VertId - 3].z += zWorldOffset;
                this.vertices[this.VertId - 2].z += zWorldOffset;
                this.vertices[this.VertId - 1].z += zWorldOffset;
              }
              if (numArray == null)
                numArray = this.submeshIndices[this.subMeshId];
              int num = this.quadCount * 4;
              numArray[this.triId++] = num;
              numArray[this.triId++] = num + 1;
              numArray[this.triId++] = num + 3;
              numArray[this.triId++] = num + 3;
              numArray[this.triId++] = num + 1;
              numArray[this.triId++] = num + 2;
              ++this.quadCount;
            }
            this.zDrawOffset += this.zSpace;
          }
        }
      }
      this.renderDisplayObjectContainer((DisplayObjectContainer) sprite);
      if (!parentHasClipRect && this.renderState.parentHasClipRect)
      {
        this.renderState.parentHasClipRect = false;
        this.renderState.clipRectParent = (DisplayObject) null;
        this.renderState.clipRectCached = false;
      }
      this.zDrawOffset += this.zContainerSpace;
    }

    public void drawMeshNow(Matrix4x4 camMatrix, Vector3 drawOffset, Vector3 drawScale)
    {
    }

    public MeshGeneratorOptions meshGeneratorOptions
    {
      get => this.m_Options;
      set
      {
        this.m_Options = value;
        if (this.m_Options == null)
          this.m_Options = new MeshGeneratorOptions();
        if (this.m_Options.customDataPoolAllocator != null)
          this.dataPool = this.m_Options.customDataPoolAllocator;
        else
          this.dataPool = (IGraphicsGeneratorDataPool) this;
      }
    }

    public Mesh getCurrentMesh()
    {
      if (this.m_meshSwitch)
      {
        if (Object.op_Equality((Object) this.mesh_0, (Object) null))
        {
          this.mesh_0 = new Mesh();
          ((Object) this.mesh_0).name = "mesh_0";
        }
        return this.mesh_0;
      }
      if (Object.op_Equality((Object) this.mesh_1, (Object) null))
      {
        this.mesh_1 = new Mesh();
        ((Object) this.mesh_1).name = "mesh_1";
      }
      return this.mesh_1;
    }

    public SimpleStageRenderResult getSimpleStageRenderResult()
    {
      Mesh currentMesh = this.getCurrentMesh();
      currentMesh.Clear();
      currentMesh.vertices = this.vertices;
      if (this.generateNormals)
        currentMesh.normals = this.normals;
      currentMesh.uv = this.UVs;
      currentMesh.colors = this.colors;
      currentMesh.subMeshCount = this.submeshIndices.Count;
      for (int i = 0; i < this.submeshIndices.Count; ++i)
      {
        int[] submeshIndex = this.submeshIndices[i];
        currentMesh.SetTriangles(submeshIndex, i);
      }
      this.m_SimpleMeshResult.materials = this.materialList.ToArray();
      this.m_SimpleMeshResult.mesh = currentMesh;
      return this.m_SimpleMeshResult;
    }

    public void preCycleAllocation(object context)
    {
    }

    public int[] allocIndexArray(int size)
    {
      return this.m_Options.enableInternalMemoryPooling ? this.indexPool.popArray(size) : new int[size];
    }

    public Vector3[] allocVertexArray(int size)
    {
      return this.m_Options.enableInternalMemoryPooling ? this.vertexPool.popArray(size) : new Vector3[size];
    }

    public Vector2[] allocUVArray(int size)
    {
      return this.m_Options.enableInternalMemoryPooling ? this.uvPool.popArray(size) : new Vector2[size];
    }

    public Color[] allocColorArray(int size)
    {
      return this.m_Options.enableInternalMemoryPooling ? this.colorPool.popArray(size) : new Color[size];
    }

    public void releaseIndexArray(int[] data)
    {
      if (!this.m_Options.enableInternalMemoryPooling)
        return;
      this.indexPool.addArray(data);
    }

    public void releaseVertexArray(Vector3[] data)
    {
    }

    public void releaseUVArray(Vector2[] data)
    {
    }

    public void releaseColorArray(Color[] data)
    {
    }

    public void postCycleAllocation(object context)
    {
      if (!this.m_Options.enableInternalMemoryPooling)
        return;
      this.indexPool.cycle();
      this.vertexPool.cycle();
      this.uvPool.cycle();
      this.colorPool.cycle();
    }
  }
}
*/