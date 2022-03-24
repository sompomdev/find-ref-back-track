using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SMPMissingReferencesFinder
{
    [MenuItem("Assets/Find material shader issue")]
    public static void FindMaterialIssue()
    {
        var issueFound = 0;
        try
        {
            var paths = SMPShaderUtilEditor.GetMaterialPathsFromSelection();
            var progress = 0;
            
            
            foreach (var path in paths)
            {
                //Debug.Log($"Path : {path}");
                var obj = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
                if (obj != null)
                {
                    //Debug.Log($"Shader is okay : {obj.name} : {obj.shader != null}");
                    if (obj.shader != null)
                    {
                        // Debug.Log($"==> Shader is okay : {obj.name} : {obj.shader.name}");
                        if (obj.shader.name.Contains("InternalErrorShader"))
                        {
                            issueFound++;
                            Debug.LogError($"Material internal error: {path}", obj);
                        }
                    }
                }
                progress++;
                EditorUtility.DisplayProgressBar($"Hold on ({progress}/{paths.Length})", "Validating material shaders...",
                    (float)progress / paths.Length);
            }
        }
        catch
        {
             
        }
        Debug.Log($"Found issue : {issueFound}");
        EditorUtility.ClearProgressBar();
    }
    [MenuItem("Assets/Find missing references")]
    public static void FindMissingReferences()
    {
        // UnityEngine.Object obj = Selection.activeObject;
        // if (obj == null)
        // {
        //     return;
        // }
        //string path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
        //string[] objs = Directory.GetFiles(path, "*.prefab");
        //var selectedFolder = UnityEditor.Selection.assetGUIDs[0];

        showInitialProgressBar("all assets");
        var objs = GetPrefabPathsFromSelection();
        var wasCancelled = false;
        var count = findMissingReferences("Project", objs, () => { wasCancelled = false; }, () => { wasCancelled = true; });
        
        //showFinishDialog(wasCancelled, count);
        EditorUtility.ClearProgressBar();
    }

    public static string[] GetPrefabPathsFromSelection()
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
            var results = AssetDatabase.FindAssets("t:Prefab", new[] { selectedFolder });
            foreach (var g in results)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                paths.Add(path);
            }
        }
        return paths.ToArray();
    }

    private static GameObject[] LoadAllPrefabs(string path)
    {
        string[] filePath = Directory.GetFiles(path, "*.prefab");//.skel.bytes
        List<GameObject> lstGO = new List<GameObject>();
        for (int j = 0; j < filePath.Length; j++)
        {
            lstGO.Add(AssetDatabase.LoadAssetAtPath(filePath[j].Replace(Application.dataPath, "Assets"), typeof(GameObject)) as GameObject);
        }
        string[] sub = Directory.GetDirectories(path);
        if (sub.Length > 0)
        {
            foreach (string s in sub)
            {
                lstGO.AddRange(LoadAllPrefabs(s + "/"));
            }
        }
        return lstGO.ToArray();
    }

    private static int findMissingReferences(string context, string[] paths, Action onFinished, Action onCanceled, float initialProgress = 0f, float progressWeight = 1f)
    {
        var count = 0;
        var wasCancelled = false;
        for (var i = 0; i < paths.Length; i++)
        {
            var obj = AssetDatabase.LoadAssetAtPath(paths[i], typeof(GameObject)) as GameObject;
            if (obj == null || !obj) continue;

            if (wasCancelled || EditorUtility.DisplayCancelableProgressBar("Searching missing references in assets.",
                                                                           $"{paths[i]}",
                                                                           initialProgress + ((i / (float)paths.Length) * progressWeight)))
            {
                onCanceled.Invoke();
                return count;
            }

            count = findMissingReferences(context, obj);
        }

        onFinished.Invoke();
        return count;
    }

    private static int findMissingReferences(string context, GameObject go, bool findInChildren = false)
    {
        var count = 0;
        var components = go.GetComponents<Component>();

        for (var j = 0; j < components.Length; j++)
        {
            var c = components[j];
            if (!c)
            {
                Debug.LogError($"Missing Component in GameObject: {FullPath(go)} in {context}", go);
                count++;
                continue;
            }

            var so = new SerializedObject(c);
            var sp = so.GetIterator();

            while (sp.NextVisible(true))
            {
                if (sp.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (sp.objectReferenceValue == null
                     && sp.objectReferenceInstanceIDValue != 0)
                    {
                        showError(context, go, c.GetType().Name, ObjectNames.NicifyVariableName(sp.name));
                        count++;
                    }
                }
            }
        }

        if (findInChildren)
        {
            foreach (Transform child in go.transform)
            {
                count += findMissingReferences(context, child.gameObject, true);
            }
        }

        return count;
    }

    private static void showError(string context, GameObject go, string componentName, string property)
    {
        Debug.LogError($"Missing REFERENCE: [{context}]{FullPath(go)}. Component: {componentName}, Property: {property}", go);
    }

    private static string FullPath(GameObject go)
    {
        var parent = go.transform.parent;
        return parent == null ? go.name : FullPath(parent.gameObject) + "/" + go.name;
    }
    private static void showInitialProgressBar(string searchContext, bool clearConsole = true)
    {
        if (clearConsole)
        {
            Debug.ClearDeveloperConsole();
        }
        EditorUtility.DisplayProgressBar("Missing References Finder", $"Preparing search in {searchContext}", 0f);
    }
    private static void showFinishDialog(bool wasCancelled, int count)
    {
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("Missing References Finder",
                                    wasCancelled ?
                                        $"Process cancelled.\n{count} missing references were found.\n Current results are shown as errors in the console." :
                                        $"Finished finding missing references.\n{count} missing references were found.\n Results are shown as errors in the console.",
                                    "Ok");
    }
}
