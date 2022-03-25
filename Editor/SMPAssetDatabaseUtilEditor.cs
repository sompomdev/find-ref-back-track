using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SMPAssetDatabaseUtilEditor 
{
    public static string[] GetPrefabPathsFromSelection()
    {
        return GetAssetPaths(1);
    }
    public static string[] GetMaterialPathsFromSelection()
    {
        return GetAssetPaths(2);
    }

    private static string[] GetAssetPaths(int modeFilter)
    {
        var filter = "t:Prefab";
        if (modeFilter == 2)
        {
            filter = "t:Material";
        }
        var paths = new List<string>();
        if (UnityEditor.Selection.assetGUIDs.Length > 0)
        {
            var folderList = new List<string>();
            foreach (var assetGUID in Selection.assetGUIDs)
            {
                var folder = UnityEditor.AssetDatabase.GUIDToAssetPath(assetGUID);
                folderList.Add(folder);
            }

            folderList = ClearSub(folderList);
            
            foreach (var selectedFolder in folderList)
            {
                // Debug.Log($"IsValidateFolder : {AssetDatabase.IsValidFolder(selectedFolder)}");
                if (AssetDatabase.IsValidFolder(selectedFolder))
                {
                    var results = AssetDatabase.FindAssets(filter, new[] { selectedFolder });
                    foreach (var g in results)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(g);
                        // Debug.Log($"{path}");
                        paths.Add(path);
                    }
                }
                else
                {
                    //folder is material
                    paths.Add(selectedFolder);
                    // Debug.Log($"{selectedFolder}");
                }
                
            }
        }
        return paths.ToArray();
    }

    private static List<string> ClearSub(List<string> list)
    {
        var cleanList = new List<string>();
        for(int i = 0; i < list.Count; i++)
        {
            var walker = list[i];
            bool isSub = false;
            for(int j = 0; j < list.Count ; j++)
            {
                var sitter = list[j];
                isSub = IsParent(sitter, walker);
                if (isSub) break;
            }

            if (!isSub)
            {
                cleanList.Add(walker);
            }
        }

        return cleanList;
    }
    
    private static bool IsParent(string parent, string sub)
    {
        DirectoryInfo di1 = new DirectoryInfo(parent);
        DirectoryInfo di2 = new DirectoryInfo(sub);
        if (di2.Parent == null)
        {
            return false;
        }
        bool isParent = di2.Parent.FullName == di1.FullName;

        return isParent;
    }
}
