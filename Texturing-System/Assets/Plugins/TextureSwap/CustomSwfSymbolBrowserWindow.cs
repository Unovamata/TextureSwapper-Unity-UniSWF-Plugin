// Decompiled with JetBrains decompiler
// Type: SwfSymbolBrowserWindow
// Assembly: LibUniSWFEditor, Version=1.0.6.1, Culture=neutral, PublicKeyToken=null
// MVID: E5E06B76-33DC-4FE2-8F13-C02519642EE7
// Assembly location: C:\Users\Admin\Documents\GitHub\Unity-UniSWF-Texture-Manager\Texturing-System\Assets\Plugins\uniSWF\DLLs\Editor\LibUniSWFEditor.dll

using pumpkin.editor;
using pumpkin.swf;
using pumpkin.utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#nullable disable
public class CustomSwfSymbolBrowserWindow : SwfSymbolBrowserWindow
{
  public SwfSymbolBrowserWindow.SelectCallback selectedCallack;
  public SwfSymbolBrowserWindow.SelectCallbackWithInfo selectedCallackWithInfo;
  public UnityEngine.Object applyTo;
  public bool copyToClipboard = false;
  public bool showExportSharedOnly = false;
  protected static int swfId = 0;
  protected bool m_Closed = false;
  protected bool m_ClosedDone = false;
  protected int m_IgnoreNextFrame = 0;
  protected Hashtable m_Symbols = new Hashtable();
  protected Vector2 scrollPos;
  public string m_Search = "";
  public SwfAssetExportOptions autoExpandAssetInfo;
  private List<UnityEngine.Object> m_AssetListCache;
  private Dictionary<int, bool> m_ExpandStates = new Dictionary<int, bool>();
  private List<SwfAssetExportOptions> m_SwfAssetExportOptionsCache = new List<SwfAssetExportOptions>();
  private int lastSyncTime = 0;

  protected void OnSelectionChange() => this.Close();

  private void OnGUI()
  {
    SwfExportOptionsPanel.onHeaderGUI();
    if (this.m_AssetListCache == null)
    {
      this.m_AssetListCache = SwfSymbolBrowserWindow.getAllSwfAssets();
      this.m_SwfAssetExportOptionsCache.Clear();
      for (int index = 0; index < this.m_AssetListCache.Count; ++index)
        this.m_SwfAssetExportOptionsCache.Add(SwfAssetExportOptions.getSwfAssetInfo(this.m_AssetListCache[index]));
    }
    if (SwfSymbolBrowserWindow.swfId < 0 || SwfSymbolBrowserWindow.swfId >= this.m_AssetListCache.Count)
      SwfSymbolBrowserWindow.swfId = 0;
    Exception exception;
    try
    {
      if (!SwfConverterClient.startConverterProcess())
      {
        Debug.LogError((object) "Exporter process failed to start");
        return;
      }
    }
    catch (Exception ex)
    {
      exception = ex;
      GUILayout.Label("Exporter process failed to start", new GUILayoutOption[0]);
      return;
    }
    this.m_Search = EditorGUILayout.TextField(this.m_Search, new GUILayoutOption[0]);
    string lower = this.m_Search.ToLower();
    this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, new GUILayoutOption[0]);
    EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
    for (int index1 = 0; index1 < this.m_SwfAssetExportOptionsCache.Count; ++index1)
    {
      SwfAssetExportOptions assetInfo = this.m_SwfAssetExportOptionsCache[index1];
      if (assetInfo != null && !string.IsNullOrEmpty(assetInfo.swfName) && (string.IsNullOrEmpty(this.m_Search) || assetInfo.swfName.ToLower().Contains(lower)))
      {
        bool flag1 = this.m_ExpandStates.ContainsKey(index1) && this.m_ExpandStates[index1];
        if (this.autoExpandAssetInfo != null && this.autoExpandAssetInfo.swfAssetFilename == assetInfo.swfAssetFilename)
          flag1 = true;
        bool flag2 = EditorGUILayout.Foldout(flag1, assetInfo.swfName);
        this.m_ExpandStates[index1] = flag2;
        if (flag2)
        {
          SwfSymbolBrowserWindow.swfId = index1;
          string absoluteSwfFilename = assetInfo.getAbsoluteSwfFilename();
          string[] strArray = (string[]) null;
          if (this.m_Symbols.ContainsKey((object) absoluteSwfFilename))
          {
            strArray = (string[]) this.m_Symbols[(object) absoluteSwfFilename];
          }
          else
          {
            try
            {
              strArray = SwfConverterClient.getSwfSymbols(absoluteSwfFilename);
              this.m_Symbols[(object) absoluteSwfFilename] = (object) strArray;
            }
            catch (Exception ex)
            {
              exception = ex;
              this.m_Symbols[(object) absoluteSwfFilename] = (object) new string[0];
            }
          }
          if (strArray != null)
          {
            for (int index2 = 0; index2 < strArray.Length; ++index2)
            {
              EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
              EditorGUILayout.Space();
              if (GUILayout.Button(strArray[index2], new GUILayoutOption[0]))
              {
                CustomMovieClipBehaviour applyTo = (CustomMovieClipBehaviour) this.applyTo;
                applyTo.swf = assetInfo.getResourceSwfFilename();
                applyTo.symbolName = strArray[index2];
                
                SwfSymbolBrowserWindow.refreshComponent(this.applyTo);
                if (this.selectedCallack != null)
                  this.selectedCallack(assetInfo);
                if (this.selectedCallackWithInfo != null)
                  this.selectedCallackWithInfo(assetInfo, this);
                if (this.copyToClipboard)
                {
                  string str = assetInfo.getResourceSwfFilename() + ":" + strArray[index2];
                  Debug.Log((object) str);
                  EditorGUIUtility.systemCopyBuffer = "\"" + str + "\"";
                }
                this.Close();
                return;
              }
              EditorGUILayout.Space();
              EditorGUILayout.Space();
              EditorGUILayout.EndHorizontal();
            }
          }
        }
      }
    }
    EditorGUILayout.EndVertical();
    EditorGUILayout.EndScrollView();
    if (!GUILayout.Button("Refresh", new GUILayoutOption[0]))
      return;
    this.m_SwfAssetExportOptionsCache.Clear();
    this.m_AssetListCache.Clear();
    this.m_Symbols.Clear();
    this.m_AssetListCache.Clear();
    this.m_AssetListCache = (List<UnityEngine.Object>) null;
  }

  public static List<UnityEngine.Object> getAllSwfAssets()
  {
    List<FileInfo> fileInfoList = PathUtils.searchFilesRecursive(new DirectoryInfo(Application.dataPath), "*.*");
    List<UnityEngine.Object> allSwfAssets = new List<UnityEngine.Object>();
    foreach (FileInfo fileInfo in fileInfoList)
    {
      if (!fileInfo.Name.StartsWith(".") && fileInfo.Name.EndsWith(".swf"))
        allSwfAssets.Add(AssetDatabase.LoadMainAssetAtPath(SwfSymbolBrowserWindow.getRelativeAssetPath(fileInfo.FullName)));
    }
    return allSwfAssets;
  }

  public static List<string> getAllSwfAssetPaths()
  {
    List<FileInfo> fileInfoList = PathUtils.searchFilesRecursive(new DirectoryInfo(Application.dataPath), "*.*");
    List<string> allSwfAssetPaths = new List<string>();
    foreach (FileInfo fileInfo in fileInfoList)
    {
      if (!fileInfo.Name.StartsWith(".") && fileInfo.Name.EndsWith(".swf"))
        allSwfAssetPaths.Add(SwfSymbolBrowserWindow.getRelativeAssetPath(fileInfo.FullName));
    }
    return allSwfAssetPaths;
  }

  public static string getRelativeAssetPath(string pathName)
  {
    return PathUtils.fixSlashes(pathName).Replace(Application.dataPath, "Assets");
  }

  public static void refreshComponent(UnityEngine.Object component)
  {
    switch (component)
    {
      case MovieClipBehaviour _:
        MovieClipBehaviour mcb = component as MovieClipBehaviour;
        MovieClipPlayer.clearContextCache(true);
        MovieClipBehaviourEditor.refreshMovieClipBehaviour(mcb);
        break;
      case StaticMovieClipBehaviour _:
        StaticMovieClipBehaviour movieClipBehaviour = component as StaticMovieClipBehaviour;
        MovieClipPlayer.clearContextCache(true);
        bool editorPreview = movieClipBehaviour.editorPreview;
        try
        {
          movieClipBehaviour.editorPreview = true;
          movieClipBehaviour.Awake();
        }
        finally
        {
          movieClipBehaviour.editorPreview = editorPreview;
        }
        movieClipBehaviour.renderFrame();
        break;
    }
  }

  public delegate void SelectCallback(SwfAssetExportOptions assetInfo);

  public delegate void SelectCallbackWithInfo(
    SwfAssetExportOptions assetInfo,
    SwfSymbolBrowserWindow window);
}
