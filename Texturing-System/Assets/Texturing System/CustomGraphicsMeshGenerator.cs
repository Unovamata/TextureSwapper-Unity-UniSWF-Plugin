// Decompiled with JetBrains decompiler
// Type: pumpkin.displayInternal.GraphicsMeshGenerator
// Assembly: LibUniSWF, Version=1.1.0.4, Culture=neutral, PublicKeyToken=null
// MVID: ECA667DE-E663-4CF8-BB22-A8C2F545850C
// Assembly location: C:\Users\Admin\Documents\GitHub\Unity-UniSWF-Texture-Manager\Unity Project\Texture Colorizer\Assets\Plugins\uniSWF\DLLs\LibUniSWF.dll

using pumpkin.display;
using pumpkin.geom;
using pumpkin.swf;
using pumpkin.utils;
using System;
using UnityEngine;
using System.Collections.Generic;

#nullable disable
namespace pumpkin.displayInternal
{
  [Serializable]
  public class CustomGraphicsMeshGenerator : IGraphicsGenerator, IGraphicsGeneratorDataPool{
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

    string diffuseShaderName = "Transparent/CustomDiffuseDoubleSided";
    Shader diffuseShader;

    public CustomGraphicsMeshGenerator(){
        this.dataPool = this;
  
        diffuseShader = Shader.Find(diffuseShaderName);
    }

    private pumpkin.geom.Matrix matrixReference = new pumpkin.geom.Matrix();

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
      if (meshRenderer != null)
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

    public List<int> layersToRemove = new List<int>();

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

      this.renderState.colorTransform = new Color(
        parent.colorTransform.r * this.renderState.colorTransform.r,
        parent.colorTransform.g * this.renderState.colorTransform.g,
        parent.colorTransform.b * this.renderState.colorTransform.b,
        parent.colorTransform.a * this.renderState.colorTransform.a
      );

      for (int id = 0; id < parent.numChildren; id += 1)
      {
        if(layersToRemove.Contains(id)) continue;

        DisplayObject childAt = parent.getChildAt(id);
        switch (childAt)
        {
          case pumpkin.display.Sprite _:
            this.renderSprite(childAt as pumpkin.display.Sprite);
            break;
          case DisplayObjectContainer _:
            this.renderDisplayObjectContainer(childAt as DisplayObjectContainer);
            break;
        }
      }

      this.renderState.colorTransform = colorTransform;
      this.renderState.blendMode = blendMode;
    }

    public Material createConvertToAdditive(Material existingMaterial){
      string str = (string) null;

      foreach (KeyValuePair<string, Material> material in TextureManager.instance.materials){
        if (material.Value == existingMaterial){
          str = material.Key;
          break;
        }
      }

      if (!string.IsNullOrEmpty(str)){

        string key = str;

        if(!key.Contains("_A")) key += "_A";

        if (TextureManager.instance.materials.ContainsKey(key))
        {
          Material material = TextureManager.instance.materials[key];
          material.shader = diffuseShader;
          if (material != null)
            return material;
        }
        else
        {
          Texture mainTexture = existingMaterial.mainTexture;
          Material convertToAdditive = new Material(TextureManager.baseBitmapAddShader);
          convertToAdditive.mainTexture = mainTexture;
          TextureManager.instance.materials[key] = convertToAdditive;
          return convertToAdditive;
        }
      }
      return new Material(TextureManager.baseBitmapAddShader)
      {
        mainTexture = existingMaterial.mainTexture
      };
    }

    public Dictionary<string, Material> materials = new Dictionary<string, Material>();

    public Material CreateMaterialDuplicate(Material existingMaterial){
      string key = existingMaterial.mainTexture.name;

      if(!key.Contains("_C")) key += "_C";

      if(materials.ContainsKey(key)) return materials[key];
      else {
        Texture mainTexture = existingMaterial.mainTexture;
        Material convertToAdditive = new Material(diffuseShader);
        convertToAdditive.mainTexture = mainTexture;
        TextureManager.instance.materials[key] = convertToAdditive;
        return convertToAdditive;
      }



      /*foreach (KeyValuePair<string, Material> material in TextureManager.instance.materials){
        if (material.Value == existingMaterial){
          str = material.Key;
          Debug.Log("Material Name: " + existingMaterial.name + " ~ " + material.mainTexture.name);
          break;
        }
      }

      if (!string.IsNullOrEmpty(str))
      {
        string key = str;

        if(!key.Contains("_C")) key += "_C";
        
        if (TextureManager.instance.materials.ContainsKey(key))
        {
          Material material = TextureManager.instance.materials[key];
          //material.shader = diffuseShader;
          if (material != null)
            return material;
        }
        else
        {
          Texture mainTexture = existingMaterial.mainTexture;
          Material convertToAdditive = new Material(diffuseShader);
          convertToAdditive.mainTexture = mainTexture;
          TextureManager.instance.materials[key] = convertToAdditive;
          return convertToAdditive;
        }
      }

      
      return new Material(diffuseShader)
      {
        mainTexture = existingMaterial.mainTexture
      };*/
    }

    private void renderSprite(pumpkin.display.Sprite sprite)
    {
      if (!sprite.visible)
        return;
      pumpkin.display.Graphics graphics = sprite.graphics;

      bool parentHasClipRect = this.renderState.parentHasClipRect;
      if (sprite.hasClipRect)
      {
        this.renderState.parentHasClipRect = true;
        this.renderState.clipRectParent = (DisplayObject) sprite;
        this.renderState.clipRectCached = false;
      }
      pumpkin.display.Sprite parent = sprite;
      int[] numArray = (int[]) null;
      /*if (!parent.visible)
        return;*/
      int count = graphics.drawOPs.Count;

      for (int index1 = 0; index1 < count; ++index1){
        GraphicsDrawOP drawOp = graphics.drawOPs[index1];

        if (drawOp.material != null){
          switch(this.renderState.blendMode){
            // Normal Rendering;
            case 0:
            break;
            // Additive Rendering;
            case 1:
              try{
                if (drawOp.material.shader != TextureManager.baseBitmapAddShader)
                  drawOp.material = createConvertToAdditive(drawOp.material); //TextureManager.instance.createConvertToAdditive(drawOp.material);
              } catch {}
            break;
            // Custom Rendering;
            case 2:
              if (drawOp.material.shader != TextureManager.baseBitmapAddShader)
                drawOp.material = CreateMaterialDuplicate(drawOp.material);
            break;
          }

          
          if (this.phaseId == 0)
          {
            if (this.currentMaterial != drawOp.material)
            {
              ++this.subMeshId;
              if (this.currentMaterial != null)
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
            if (this.currentMaterial != drawOp.material)
            {
              ++this.subMeshId;
              this.currentMaterial = drawOp.material;
              this.triId = 0;
              numArray = this.submeshIndices[this.subMeshId];
            }
            pumpkin.geom.Matrix fullMatrixRef = parent._fastGetFullMatrixRef();
            matrixReference = fullMatrixRef;
            if (drawOp.simpleVectorShape != null)
            {
              Color color = new Color(
                this.renderState.colorTransform.r * drawOp.color.r,
                this.renderState.colorTransform.g * drawOp.color.g,
                this.renderState.colorTransform.b * drawOp.color.b,
                this.renderState.colorTransform.a * drawOp.color.a
              );
              for (int index2 = 0; index2 < drawOp.simpleVectorShape.vertices.Length; ++index2)
              {
                Vector2 vertex = drawOp.simpleVectorShape.vertices[index2];
                this.vertices[this.VertId++] = matrixReference.transformVector3Static(vertex.x, vertex.y, this.zDrawOffset);
                this.UVs[this.UVId++] = drawOp.simpleVectorShape.uv[index2];
                this.colors[this.ColourId++] = new Color(
                  drawOp.simpleVectorShape.colors[index2].r * color.r,
                  drawOp.simpleVectorShape.colors[index2].g * color.g,
                  drawOp.simpleVectorShape.colors[index2].b * color.b,
                  drawOp.simpleVectorShape.colors[index2].a * color.a
                );
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
                if ((double)clipRect.width != 0.0 && (double)clipRect.height != 0.0)
                {
                    Rect rect = drawOp.calcClipping(parent, clipRect);
                    Rect tempDrawSrcRect = drawSrcRect; // Temporary variable
                    UVPixelRect uvPixelRect = new UVPixelRect(x, y, pW, pH, tempDrawSrcRect.x, tempDrawSrcRect.y, tempDrawSrcRect.width, tempDrawSrcRect.height);
                    double num1 = (double)uvPixelRect.setWidthPixels(rect.width);
                    double num2 = (double)uvPixelRect.setHeightPixels(rect.height);
                    double num3 = (double)uvPixelRect.setXPixels(rect.x);
                    double num4 = (double)uvPixelRect.setYPixels(rect.y);
                    x = rect.x;
                    y = rect.y;
                    pW = rect.width;
                    pH = rect.height;
                    tempDrawSrcRect.x = uvPixelRect.uX;
                    tempDrawSrcRect.y = uvPixelRect.uY;
                    tempDrawSrcRect.width = uvPixelRect.uW;
                    tempDrawSrcRect.height = uvPixelRect.uH;
                    drawSrcRect = tempDrawSrcRect; // Assign back to drawSrcRect
                }

              }
              this.lowerLeftUV.x = drawSrcRect.x;
              this.lowerLeftUV.y = 1f - drawSrcRect.y;
              this.UVDimensions.x = drawSrcRect.width;
              this.UVDimensions.y = drawSrcRect.height;
              Vector2 vector2;
              if (drawOp.matrix != null && (double) drawOp.matrixScale.x == 0.0)
              {

                float num5 = 0.0f;
                float num6 = 0.0f;
                Texture mainTexture = drawOp.material.mainTexture;
                if (mainTexture != null)
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
                if (mainTexture != null)
                {
                  num15 = (float) mainTexture.width;
                  num16 = (float) mainTexture.height;
                }
                float num17 = x + pW;
                float num18 = y + 0.0f;
                float inPos_x5 = num17 - drawOp.tilePosX;
                float inPos_y5 = num18 - drawOp.tilePosY;
                vector2 = pumpkin.geom.Matrix.transformPointStaticWithInvertedPostScale(drawOp.matrix, inPos_x5, inPos_y5, drawOp.matrixScale.x, drawOp.matrixScale.y);
                this.uvPos.x = vector2.x / num15;
                this.uvPos.y = (float) (1.0 - (double) vector2.y / (double) num16);
                this.UVs[this.UVId++] = this.uvPos;
                float num19 = x + pW;
                float num20 = y + pH;
                float inPos_x6 = num19 - drawOp.tilePosX;
                float inPos_y6 = num20 - drawOp.tilePosY;
                vector2 = pumpkin.geom.Matrix.transformPointStaticWithInvertedPostScale(drawOp.matrix, inPos_x6, inPos_y6, drawOp.matrixScale.x, drawOp.matrixScale.y);
                this.uvPos.x = vector2.x / num15;
                this.uvPos.y = (float) (1.0 - (double) vector2.y / (double) num16);
                this.UVs[this.UVId++] = this.uvPos;
                float num21 = x + 0.0f;
                float num22 = y + pH;
                float inPos_x7 = num21 - drawOp.tilePosX;
                float inPos_y7 = num22 - drawOp.tilePosY;
                vector2 = pumpkin.geom.Matrix.transformPointStaticWithInvertedPostScale(drawOp.matrix, inPos_x7, inPos_y7, drawOp.matrixScale.x, drawOp.matrixScale.y);
                this.uvPos.x = vector2.x / num15;
                this.uvPos.y = (float) (1.0 - (double) vector2.y / (double) num16);
                this.UVs[this.UVId++] = this.uvPos;
                float num23 = x + 0.0f;
                float num24 = y + 0.0f;
                float inPos_x8 = num23 - drawOp.tilePosX;
                float inPos_y8 = num24 - drawOp.tilePosY;
                vector2 = pumpkin.geom.Matrix.transformPointStaticWithInvertedPostScale(drawOp.matrix, inPos_x8, inPos_y8, drawOp.matrixScale.x, drawOp.matrixScale.y);
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
                Color color = new Color(
                  this.renderState.colorTransform.r * drawOp.color.r,
                  this.renderState.colorTransform.g * drawOp.color.g,
                  this.renderState.colorTransform.b * drawOp.color.b,
                  this.renderState.colorTransform.a * drawOp.color.a
                );

                this.colors[this.ColourId++] = color;
                this.colors[this.ColourId++] = color;
                this.colors[this.ColourId++] = color;
                this.colors[this.ColourId++] = color;
              }
              bool flipped = drawOp.flipped;
              int vertId = this.VertId;
              if (flipped)
              {
                this.vertices[this.VertId++] = matrixReference.transformVector3Static(x + 0.0f, y + 0.0f, this.zDrawOffset);
                this.vertices[this.VertId++] = matrixReference.transformVector3Static(x + 0.0f, y + pH, this.zDrawOffset);
                this.vertices[this.VertId++] = matrixReference.transformVector3Static(x + pW, y + pH, this.zDrawOffset);
                this.vertices[this.VertId++] = matrixReference.transformVector3Static(x + pW, y + 0.0f, this.zDrawOffset);
              }
              else
              {
                this.vertices[this.VertId++] = matrixReference.transformVector3Static(x + pW, y + 0.0f, this.zDrawOffset);
                this.vertices[this.VertId++] = matrixReference.transformVector3Static(x + pW, y + pH, this.zDrawOffset);
                this.vertices[this.VertId++] = matrixReference.transformVector3Static(x + 0.0f, y + pH, this.zDrawOffset);
                this.vertices[this.VertId++] = matrixReference.transformVector3Static(x + 0.0f, y + 0.0f, this.zDrawOffset);
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
                Matrix4x4 matrix4x4 = pumpkin.utils.Utils.MatrixFromQuaternion(Quaternion.Euler(parent.rotateX, parent.rotateY, parent.rotateZ));
                this.vertices[this.VertId - 4] = matrix4x4.MultiplyVector(this.vertices[this.VertId - 4]);
                this.vertices[this.VertId - 3] = matrix4x4.MultiplyVector(this.vertices[this.VertId - 3]);
                this.vertices[this.VertId - 2] = matrix4x4.MultiplyVector(this.vertices[this.VertId - 2]);
                this.vertices[this.VertId - 1] = matrix4x4.MultiplyVector(this.vertices[this.VertId - 1]);

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
        if (this.mesh_0 == null)
        {
          this.mesh_0 = new Mesh();
          ((UnityEngine.Object) this.mesh_0).name = "mesh_0";
        }
        return this.mesh_0;
      }
      if (this.mesh_1 == null)
      {
        this.mesh_1 = new Mesh();
        ((UnityEngine.Object) this.mesh_1).name = "mesh_1";
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