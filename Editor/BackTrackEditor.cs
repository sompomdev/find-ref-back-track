using System;
using System.Collections.Generic;
 using UnityEngine;
 using System.Linq;
using UnityEditor;
 public class BacktraceReference : EditorWindow
 {
     private static BacktraceReference _myWindow;
     
     /// <summary> The result </summary>
     public static List<Component> ReferencingSelection = new List<Component>();
     /// <summary> allComponents in the scene that will be searched to see if they contain the reference </summary>
     private static Component[] allComponents;
     /// <summary> Selection of gameobjects the user made </summary>
     private static GameObject[] selections;
     /// <summary>
     /// Adds context menu to hierarchy window https://answers.unity.com/questions/22947/adding-to-the-context-menu-of-the-hierarchy-tab.html
     /// </summary>
     [UnityEditor.MenuItem("GameObject/Find Objects Referencing This", false, 48)]
     public static void InitHierarchy()
     {
         selections = UnityEditor.Selection.gameObjects;
         BacktraceSelection(selections);
         // GetWindow(typeof(BacktraceReference));
         
         if (_myWindow == null)
         {
             _myWindow = ScriptableObject.CreateInstance(typeof(BacktraceReference)) as BacktraceReference;
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
             GUILayout.Label("Select source object/s from scene Hierarchy panel.");
             return;
         }
         // display reference that is being checked
         GUILayout.Label(string.Join(", ", selections.Where(go => go != null).Select(go => go.name).ToArray()));
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
         if (selections == null || selections.Length < 1)
             return;
         allComponents = GetAllActiveInScene();
         if (allComponents == null) return;
         ReferencingSelection.Clear();
         foreach (GameObject selection in selections)
         {
             foreach (Component cOfSelection in selection.GetComponents(typeof(Component)))
             {
                 FindObjectsReferencing(cOfSelection);
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
                Debug.Log($"objectReferenceValue : {prop.name} {prop.objectReferenceInstanceIDValue}");
             
             bool isObjectField = prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue != null;
             if (isObjectField ) 
                 //&& prop.objectReferenceValue == references)
             {
                 // Debug.Log($"objectReferenceValue : {prop.name} - {prop.objectReferenceInstanceIDValue}");
                 if (prop.objectReferenceValue == references)
                 {
                     Debug.Log($"Found ref : {prop.name}");
                     if (!ReferencingSelection.Contains(component))
                     {
                         ReferencingSelection.Add(component);
                     }
                 }
                 else if (prop.objectReferenceInstanceIDValue == references.gameObject.GetInstanceID())
                 {
                     Debug.Log($"Found ref : {prop.name}");
                     if (!ReferencingSelection.Contains(component))
                     {
                         ReferencingSelection.Add(component);
                     }
                 }
                 //ReferencingSelection.Add(component);
             }
         }
     }
 }