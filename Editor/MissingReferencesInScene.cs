using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MissingReferencesInScene : EditorWindow
{

    enum FindMode
    {
        FIND_MISSING_REF,
        FIND_EMPTY_REF
    }

    private static FindMode _findMode;

    private static MissingReferencesInScene _myWindow;
    
/// <summary> The result </summary>
     public static List<Component> ReferencingSelection = new List<Component>();
     /// <summary> allComponents in the scene that will be searched to see if they contain the reference </summary>
     private static Component[] allComponents;
     /// <summary> Selection of gameobjects the user made </summary>
     private static GameObject[] selections;
     /// <summary>
     /// Adds context menu to hierarchy window https://answers.unity.com/questions/22947/adding-to-the-context-menu-of-the-hierarchy-tab.html
     /// </summary>
     [UnityEditor.MenuItem("GameObject/Find missing references", false, 48)]
     public static void InitHierarchy()
     {
         _findMode = FindMode.FIND_MISSING_REF;
         selections = UnityEditor.Selection.gameObjects;
         BacktraceSelection(selections);
         if (_myWindow == null)
         {
             _myWindow = ScriptableObject.CreateInstance(typeof(MissingReferencesInScene)) as MissingReferencesInScene;
             if (_myWindow != null)
             {
                 _myWindow.ShowUtility();
             }
         }
         else
         {
             _myWindow.Focus();
         }
     }
     [UnityEditor.MenuItem("GameObject/Find empty references", false, 48)]
     public static void FindEmptyRefs()
     {
         _findMode = FindMode.FIND_EMPTY_REF;
         selections = UnityEditor.Selection.gameObjects;
         BacktraceSelection(selections);
         if (_myWindow == null)
         {
             _myWindow = ScriptableObject.CreateInstance(typeof(MissingReferencesInScene)) as MissingReferencesInScene;
             if (_myWindow != null)
             {
                 _myWindow.ShowUtility();
             }
         }
         else
         {
             _myWindow.Focus();
         }
     }
     /// <summary>
     /// Display referenced by components in window
     /// </summary>
     public void OnGUI()
     {
         if (selections == null || selections.Length < 1)
         {
             if (_findMode != FindMode.FIND_EMPTY_REF && _findMode != FindMode.FIND_MISSING_REF)
             {
                 GUILayout.Label("Select source object/s from scene Hierarchy panel.");
                 return;    
             }
         }
         else
         {
             // display reference that is being checked
             GUILayout.Label(string.Join(", ", selections.Where(go => go != null).Select(go => go.name).ToArray()));
         }
         
         
         // handle no references
         if (ReferencingSelection == null || ReferencingSelection.Count == 0)
         {
             GUILayout.Label("is not referenced by any gameobjects in the scene");
             return;    
         }
         // display list of references using their component name as the label
         foreach (var item in ReferencingSelection)
         {
             EditorGUILayout.ObjectField(item.GetType().ToString(), item, typeof(GameObject), allowSceneObjects: true);
         }
     }
     // This script finds all objects in scene
     private static Component[] GetAllActiveInScene()
     {
         // Use new version of Resources.FindObjectsOfTypeAll(typeof(Component)) as per https://forum.unity.com/threads/editorscript-how-to-get-all-gameobjects-in-scene.224524/
         var rootObjects = UnityEngine.SceneManagement.SceneManager
             .GetActiveScene()
             .GetRootGameObjects();
         List<Component> result = new List<Component>();
         foreach (var rootObject in rootObjects)
         {
             result.AddRange(rootObject.GetComponentsInChildren<Component>());
         }
         return result.ToArray();
     }
     private static void BacktraceSelection(GameObject[] selections)
     {
         allComponents = GetAllActiveInScene();
         if (allComponents == null) return;
         ReferencingSelection.Clear();
         
         if (selections == null || selections.Length < 1)
         {
             FindObjectsReferencing(allComponents[0]);
         }
         else
         {
             foreach (GameObject selection in selections)
             {
                 foreach (Component cOfSelection in selection.GetComponents(typeof(Component)))
                 {
                     FindObjectsReferencing(cOfSelection);
                 }
             }
         }

         
         
         
         
     }
     private static void FindObjectsReferencing<T>(T cOfSelection) where T : Component
     {
         foreach (Component sceneComponent in allComponents)
         {
             try
             {
                 componentReferences(sceneComponent, cOfSelection);
             }
             catch (Exception e)
             {
                 Console.WriteLine(e);
             }
         }
     }
     /// <summary>
     /// Determines if the component makes any references to the second "references" component in any of its inspector fields
     /// </summary>
     private static void componentReferences(Component component, Component references)
     {
         // find all fields exposed in the editor as per https://answers.unity.com/questions/1333022/how-to-get-every-public-variables-from-a-script-in.html
         SerializedObject serObj = new SerializedObject(component);
         SerializedProperty prop = serObj.GetIterator();
         
         //Debug.Log($"Dest references value : {references.name} {references.GetInstanceID()}");
         while (prop.NextVisible(true))
         {
             if (prop.propertyType == SerializedPropertyType.ObjectReference)
             {
                 if (_findMode == FindMode.FIND_MISSING_REF)
                 {
                     //Debug.Log($"objectReferenceValue : {prop.name} {prop.objectReferenceValue == null}");
                     if (prop.objectReferenceValue == null && prop.objectReferenceInstanceIDValue != 0)
                     {
                         if (!ReferencingSelection.Contains(component))
                         {
                             Debug.Log($"Found missing ref : {component.GetType().ToString()} : {prop.name}");
                             ReferencingSelection.Add(component);   
                         }
                     }
                 } else if (_findMode == FindMode.FIND_EMPTY_REF)
                 {
                     if (prop.objectReferenceValue == null && prop.objectReferenceInstanceIDValue == 0)
                     {
                         if (!ReferencingSelection.Contains(component))
                         {
                             Debug.Log($"Found empty ref : {component.GetType().ToString()} : {prop.name}");
                             ReferencingSelection.Add(component);   
                         }
                     }
                 }
                 
             }
                
             
             // bool isObjectField = prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue != null;
             // if (isObjectField )
             // {
             //     // Debug.Log($"objectReferenceValue : {prop.name} - {prop.objectReferenceInstanceIDValue}");
             //     if (prop.objectReferenceValue == references)
             //     {
             //         Debug.Log($"Found ref : {prop.name}");
             //         if (!ReferencingSelection.Contains(component))
             //         {
             //             ReferencingSelection.Add(component);
             //         }
             //     }
             //     else if (prop.objectReferenceInstanceIDValue == references.gameObject.GetInstanceID())
             //     {
             //         Debug.Log($"Found ref : {prop.name}");
             //         if (!ReferencingSelection.Contains(component))
             //         {
             //             ReferencingSelection.Add(component);
             //         }
             //     }
             //     
             // }
         }
     }
}
