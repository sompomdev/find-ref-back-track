using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SMPMultipleMaterialSelectEditor : Editor
{
    [MenuItem("Assets/Sompom/Update Enemy Shaders to Original Spine")]
    public static void UpdateEnemyShaderSpineOriginal()
    {
        try
        {
            var paths = GetShaderPaths();
            UpdateShaderAtPaths(paths, 0);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        catch { }
        EditorUtility.ClearProgressBar();
    }
    
    [MenuItem("Assets/Sompom/Update Enemy Shaders to Support Distortion")]
    
    public static void UpdateEnemyShaderSupportDistortion()
    {
        try
        {
            var paths = GetShaderPaths();
            UpdateShaderAtPaths(paths, 1);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        catch { }
        EditorUtility.ClearProgressBar();
    }

    private static string[] GetShaderPaths()
    {
        var isSingleObjectOnly = true;
        var paths = new List<string>();
        
        if (Selection.activeObject != null)
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (obj == null)
            {
                isSingleObjectOnly = false;
            }
            else
            {
                paths.Add(path);
            }
        }
        if (!isSingleObjectOnly && UnityEditor.Selection.assetGUIDs.Length > 0)
        {
            var selectedFolder = UnityEditor.Selection.assetGUIDs[0];
            selectedFolder = UnityEditor.AssetDatabase.GUIDToAssetPath(selectedFolder);
            var results = AssetDatabase.FindAssets("t:Prefab", new[] { selectedFolder });
            foreach (var g in results)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                paths.Add(path);
            }
        }
        return paths.ToArray();
    }
    private static void UpdateShaderAtPaths(string[] paths, int mode)
    {
        var progress = 0;
        foreach (var path in paths)
        {
            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (mode == 1)
            {
                UpdateShaderToSupportDistortion(obj);    
            }
            else
            {
                UpdateShaderToOriginal(obj);
            }
            
            progress++;
            EditorUtility.DisplayProgressBar($"Progress({progress}/{paths.Length})", "Switching enemy shaders",
                (float)progress / paths.Length);
        }
    }
    
    private static void UpdateShaderToOriginal(GameObject obj)
    {
        var mat = obj.GetComponent<MeshRenderer>().sharedMaterial;
        var shader = Shader.Find("Spine/Skeleton");
        mat.shader = shader;
        mat.SetFloat("_Mode", 2);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHA_CLIP");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.SetFloat("_Cutoff", 0.1f);
        mat.renderQueue = -1;
        
        RemoveUnusedProperties(mat,"m_SavedProperties.m_TexEnvs", PropertyType.TexEnv);
        RemoveUnusedProperties(mat,"m_SavedProperties.m_Ints", PropertyType.Int);
        RemoveUnusedProperties(mat,"m_SavedProperties.m_Floats", PropertyType.Float);
        RemoveUnusedProperties(mat,"m_SavedProperties.m_Colors", PropertyType.Color);
    }
    private static void UpdateShaderToSupportDistortion(GameObject obj)
    {
        var mat = obj.GetComponent<MeshRenderer>().sharedMaterial;
        var shader = Shader.Find("Spine/Sprite/Unlit");
        mat.shader = shader;
        mat.SetFloat("_Mode", 2);
        // mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstAlpha);//DstAlpha
        // mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);//DstAlpha
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        
        mat.SetInt("_ZWrite", 1);
        // mat.DisableKeyword("_ALPHATEST_ON");
        // mat.DisableKeyword("_ALPHABLEND_ON");
        // mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        // mat.renderQueue = 3000;
        
        
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.EnableKeyword("_ALPHA_CLIP");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.SetFloat("_Cutoff", 0.5f);
        mat.renderQueue = 2450;
    }
    
    private static void RemoveUnusedProperties(Material mat, string path, PropertyType type)
    {
        var m_serializedObjects = new SerializedObject(mat);
        var properties = m_serializedObjects.FindProperty(path);
        if (properties != null && properties.isArray)
        {
            for (int j = properties.arraySize - 1; j >= 0; j--)
            {
                string propName = GetName(properties.GetArrayElementAtIndex(j));
                bool exists = ShaderHasProperty(mat, propName, type);
 
                if (!exists)
                {
                    Debug.Log("Removed " + type + " Property: " + propName);
                    properties.DeleteArrayElementAtIndex(j);
                    m_serializedObjects.ApplyModifiedProperties();
                }
            }
        }
        
    }
    
    private enum PropertyType { TexEnv, Int, Float, Color }
    private static string GetName(SerializedProperty property)
    {
        return property.FindPropertyRelative("first").stringValue; //return property.displayName;
    }
    private static bool ShaderHasProperty(Material mat, string name, PropertyType type)
    {
        switch (type)
        {
            case PropertyType.TexEnv:
                return mat.HasProperty(name);
            case PropertyType.Int:
                return mat.HasProperty(name);
            case PropertyType.Float:
                return mat.HasProperty(name);
            case PropertyType.Color:
                return mat.HasProperty(name);
        }
        return false;
    }
   
}
