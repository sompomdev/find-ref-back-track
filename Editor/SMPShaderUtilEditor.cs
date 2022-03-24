using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SMPShaderUtilEditor 
{
    public static string[] GetMaterialPathsFromSelection()
    {
        var isSingleObjectOnly = false;
        var paths = new List<string>();
        
        if (Selection.activeObject != null)
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var obj = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (obj == null)
            {
                isSingleObjectOnly = false;
            }
            else
            {
                paths.Add(path);
                isSingleObjectOnly = true;
            }
        }

        if (!isSingleObjectOnly && UnityEditor.Selection.assetGUIDs.Length > 0)
        {
            var selectedFolder = UnityEditor.Selection.assetGUIDs[0];
            selectedFolder = UnityEditor.AssetDatabase.GUIDToAssetPath(selectedFolder);
            var results = AssetDatabase.FindAssets("t:Material", new[] { selectedFolder });
            foreach (var g in results)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                paths.Add(path);
            }
        }
        return paths.ToArray();
    }
}
