using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using System.IO;
using DependenciesHunter;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = System.Random;

public class EditItem : EditorWindow
{
    private TextField m_TextPackName;
    private Label m_TextEffectName;
    private TextField m_TargetUsed;
    private TextField m_UsedFor;
    private TextField m_UrpMoveStatus;
    private TextField m_ScriptRefCount;
    private TextField m_AsseSourceRef;
    private Label m_PrefabPath;
    private EffectModel m_EffectModel;

    private List<PackModel> m_PackList;
    
    private void OnEnable()
    {
        var root = rootVisualElement;
        m_TextPackName = new TextField("Pack Name:");
        m_TextPackName.isReadOnly = true;
        m_TextEffectName = new Label("Effect Name:");
        m_PrefabPath = new Label("Prefab path:");
        
        m_TargetUsed = new TextField("Target used:");
        m_UsedFor = new TextField("Used for:");
        m_UrpMoveStatus = new TextField("URP move status:");
        m_ScriptRefCount = new TextField("Scrip ref count:");
        m_AsseSourceRef = new TextField("Asset source ref:");
        
        //m_TextPackName.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
            
        root.Add(m_TextPackName);
        root.Add(m_TextEffectName);
        root.Add(m_PrefabPath);
        
        root.Add(m_TargetUsed);
        root.Add(m_UsedFor);
        root.Add(m_UrpMoveStatus);
        root.Add((m_ScriptRefCount));
        root.Add(m_AsseSourceRef);
        
        
        var saveButton = new Button();
        saveButton.text = "Save";
        saveButton.clicked += UpdateEffectModel;
        
        root.Add(saveButton);
    }
    void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        foreach (var packModel in m_PackList)
        {
            evt.menu.AppendAction(packModel.packName, OnMenuAction, DropdownMenuAction.AlwaysEnabled);   
        }
    }
 
    void OnMenuAction(DropdownMenuAction action)
    {
        Debug.Log(action.name);
        foreach (var packModel in m_PackList)
        {
            if (packModel.packName == action.name)
            {
                m_TextPackName.value = packModel.packName;
            }
        }
    }
    private void UpdateEffectModel()
    {
        m_EffectModel.targetUsed = m_TargetUsed.value;
        m_EffectModel.usedFor = m_UsedFor.value;
        m_EffectModel.moveUrpStatus = m_UrpMoveStatus.value;
        m_EffectModel.scriptRefCount = int.Parse(m_ScriptRefCount.value);
        m_EffectModel.assetSourceRef = m_AsseSourceRef.value;

        var list = m_PackList;//EffectTrackingEditor.LoadFromFile();
        var found = list.Find(t => t.packId == m_EffectModel.packId);
        if (found != null)
        {
            var foundEffect = found.effects.Find(e => e.effectName == m_EffectModel.effectName);
            if (foundEffect != null)
            {
                foundEffect.targetUsed = m_EffectModel.targetUsed;
                foundEffect.usedFor = m_EffectModel.usedFor;
                foundEffect.moveUrpStatus = m_EffectModel.moveUrpStatus;
                foundEffect.scriptRefCount = m_EffectModel.scriptRefCount;
                foundEffect.assetSourceRef = m_EffectModel.assetSourceRef;
                
                EffectTrackingEditor.SaveToFile(list);   
            }
        }

        //switching pack
        if (found != null && m_TextPackName.value != found.packName)
        {
            var foundNewPack = list.Find(t => t.packName == m_TextPackName.value);
            if (foundNewPack != null )
            {
                var foundEffect = found.effects.Find(e => e.effectName == m_EffectModel.effectName);
                if (foundEffect != null)
                {
                    found.effects.Remove(foundEffect);
                    foundNewPack.effects.Add(foundEffect);
                    foundEffect.packId = foundNewPack.packId;
                    EffectTrackingEditor.SaveToFile(list);
                }
            }
        }
        
        var window = GetWindow<EffectTrackingEditor>();
        window.ReloadPacks();
    }

    private PackModel GetPackById(string packId)
    {
        return m_PackList.Find(p => p.packId == packId);
    }
    public void SetEffect(EffectModel effectModel, List<PackModel> packs)
    {
        m_EffectModel = effectModel;
        m_PackList = packs;
        
        var pack = GetPackById(effectModel.packId);
        m_TextPackName.value = pack.packName;
        m_TextEffectName.text = "Effect : " + effectModel.effectName;
        m_PrefabPath.text = "Path : " + effectModel.prefabPath;
        
        m_TargetUsed.value = effectModel.targetUsed;
        m_UsedFor.value = effectModel.usedFor;
        m_UrpMoveStatus.value = effectModel.moveUrpStatus;
        m_ScriptRefCount.value = effectModel.scriptRefCount.ToString();
        m_AsseSourceRef.value = effectModel.assetSourceRef;
        
        
        
    }
}

public class EditPack : EditorWindow
{
    private TextField m_TextPackName;
    private TextField m_TextStoreUrl;
    private TextField m_TextUpdatedDate;
    private TextField m_TextChangeToUrpBy;
    private Toggle m_UrpSupport;
    private Toggle m_BuildInSupport;
    private PackModel m_PackModel;
    private void OnEnable()
    {
        var root = rootVisualElement;
        var labelPackName = new Label("Pack Name:");
        m_TextPackName = new TextField();
        m_TextPackName.isReadOnly = true;
        root.Add(labelPackName);
        root.Add(m_TextPackName);
        
        var labelStoreUrl = new Label("Store Url:");
        m_TextStoreUrl = new TextField();
        root.Add(labelStoreUrl);
        root.Add(m_TextStoreUrl);
        
        var labelUpdatedDate = new Label("Updated Date:");
        m_TextUpdatedDate = new TextField();
        root.Add(labelUpdatedDate);
        root.Add(m_TextUpdatedDate);

        var labelChangeToUrpBy = new Label("Change to URP by:");
        m_TextChangeToUrpBy = new TextField();
        root.Add(labelChangeToUrpBy);
        root.Add(m_TextChangeToUrpBy);
        
        m_UrpSupport = new Toggle("URP Support");
        root.Add(m_UrpSupport);
        
        m_BuildInSupport = new Toggle("Build in Support");
        root.Add(m_BuildInSupport);
        
        
        var saveButton = new Button();
        saveButton.text = "Save";
        saveButton.clicked += UpdatePackModel;
        
        // var deleteButton = new Button();
        // deleteButton.text = "Delete";
        // deleteButton.clicked += Delete;
        
        root.Add(saveButton);
        // root.Add(deleteButton);
    }

    private void Delete()
    {
        if (m_PackModel != null)
        {
            var list = EffectTrackingEditor.LoadFromFile();
            var found = list.Find(t => t.packId == m_PackModel.packId);
            if (found != null)
            {
                var yes = EditorUtility.DisplayDialog("Confirmation",
                    "Do you really want to delete this pack?", "Delete", "Do Not Delete");
                if (yes)
                {
                    list.Remove(found);
                    EffectTrackingEditor.SaveToFile(list);
                    var window = GetWindow<EffectTrackingEditor>();
                    window.ReloadPacks();    
                }
            }
        }
    }
    private void UpdatePackModel()
    {
        if (m_PackModel != null)
        {
            m_PackModel.packName = m_TextPackName.value;
            m_PackModel.packStoreUrl = m_TextStoreUrl.value;
            m_PackModel.dateUpdated = m_TextUpdatedDate.value;
            m_PackModel.changeToUrpBy = m_TextChangeToUrpBy.value;
            m_PackModel.urpSupport = m_UrpSupport.value;
            m_PackModel.buildInSupport = m_BuildInSupport.value;

            var list = EffectTrackingEditor.LoadFromFile();
            var found = list.Find(t => t.packId == m_PackModel.packId);
            if (found != null)
            {
                found.packName = m_TextPackName.value;
                found.packStoreUrl = m_TextStoreUrl.value;
                found.dateUpdated = m_TextUpdatedDate.value;
                found.changeToUrpBy = m_TextChangeToUrpBy.value;
                found.urpSupport = m_UrpSupport.value;
                found.buildInSupport = m_BuildInSupport.value;
            
                EffectTrackingEditor.SaveToFile(list);
                
                var window = GetWindow<EffectTrackingEditor>();
                window.ReloadPacks();
            }
        }
        else
        {
            var list = EffectTrackingEditor.LoadFromFile();
            var newPack = new PackModel()
            {
                packId = GenerateID(),
                packName = m_TextPackName.value,
                packStoreUrl = m_TextStoreUrl.value,
                dateUpdated = m_TextUpdatedDate.value,
                changeToUrpBy = m_TextChangeToUrpBy.value,
                urpSupport = m_UrpSupport.value,
                buildInSupport = m_BuildInSupport.value,
                effects = new List<EffectModel>()
            };
            list.Add(newPack);
            EffectTrackingEditor.SaveToFile(list);
            
            var window = GetWindow<EffectTrackingEditor>();
            window.ReloadPacks();
        }
    }
    public  static string GenerateID()
    {
        return Guid.NewGuid().ToString("N");
    }
    public void SetPack(PackModel packModel)
    {
        m_TextPackName.value = packModel.packName;
        m_TextStoreUrl.value = packModel.packStoreUrl;
        m_TextUpdatedDate.value = packModel.dateUpdated;
        m_TextChangeToUrpBy.value = packModel.changeToUrpBy;
        m_UrpSupport.value = packModel.urpSupport;
        m_BuildInSupport.value = packModel.buildInSupport;

        m_PackModel = packModel;
    }
}

public class EffectTrackingEditor : EditorWindow
{
    private SelectedAssetsAnalysisUtilities _service;
    private ScrollView m_ScrollViewPackage;
    private ScrollView m_ScrollViewItem;
    private ScrollView m_ScrollViewPackDetails;
    
    private List<PackModel> m_ListOfPack;

    private TextField m_TextSearch;

    [MenuItem("EffectTrackings/Open")]
    public static void ShowWindow()
    {
        // Opens the window, otherwise focuses it if it’s already open.
        var window = GetWindow<EffectTrackingEditor>();

        // Adds a title to the window.
        window.titleContent = new GUIContent("Effect Trakings");

        // Sets a minimum size to the window.
        window.minSize = new Vector2(250, 50);
    }

    private void OnEnable()                                                                                               
    {
        Debug.Log("OnEanble");
        if (_service == null)
        {
            _service = new SelectedAssetsAnalysisUtilities();
        }
        
        // Reference to the root of the window.
        var root = rootVisualElement;

        //// Associates a stylesheet to our root. Thanks to inheritance, all root’s
        //// children will have access to it.
        //root.styleSheets.Add(Resources.Load<StyleSheet>("QuickTool_Style"));

        //// Loads and clones our VisualTree (eg. our UXML structure) inside the root.
        //var quickToolVisualTree = Resources.Load<VisualTreeAsset>("QuickTool_Main");
        //quickToolVisualTree.CloneTree(root);

        //// Queries all the buttons (via type) in our root and passes them
        //// in the SetupButton method.
        //var toolButtons = root.Query<Button>();
        //toolButtons.ForEach(SetupButton);

        root.style.flexDirection = FlexDirection.Row;

        var packageBox = new Box();
        packageBox.style.flexGrow = 1f;
        packageBox.style.flexShrink = 0f;
        packageBox.style.flexBasis = 0f;
        packageBox.style.flexDirection = FlexDirection.Column;
        
        var rightListBox = new Box();
        rightListBox.style.flexGrow = 3f;
        rightListBox.style.flexShrink = 0f;
        rightListBox.style.flexBasis = 0f;
        rightListBox.style.flexDirection = FlexDirection.Column;

        var itemsListBox = new Box();
        itemsListBox.style.flexGrow = 3f;
        itemsListBox.style.flexShrink = 0f;
        itemsListBox.style.flexBasis = 0f;
        itemsListBox.style.flexDirection = FlexDirection.Column;

        var packDetailsBox = new Box();
        packDetailsBox.style.flexGrow = 1f;
        packDetailsBox.style.flexShrink = 0f;
        packDetailsBox.style.flexBasis = 0f;
        packDetailsBox.style.flexDirection = FlexDirection.Column;
        
        rightListBox.Add(itemsListBox);
        rightListBox.Add(packDetailsBox);
        
        root.Add(packageBox);
        root.Add(rightListBox);

        SetupPackageList(packageBox);
        SetupItemList(itemsListBox);
        SetupPackDetails(packDetailsBox);

        PreloadAssetMap();
        LoadPacks();
    }

    private void PreloadAssetMap()
    {
        _service.Preload();
    }
    #region ItemDetails
    private void ShowItemDetails(EffectModel effect)
    {
        
        // var objs = AssetDatabase.LoadAllAssetsAtPath(effect.prefabPath);
        // var lastResults = _service.GetReferences(objs);
        // for (int i = 0; i < lastResults.Count; i++)
        // {
        //     var results = lastResults[objs[i]];
        //     foreach (var result in results)
        //     {
        //         Debug.Log("=> " +result);
        //     }    
        // }
        UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(effect.prefabPath, typeof(UnityEngine.Object));
        Selection.objects = new[] { obj };
        SelectedAssetsReferencesWindow.FindReferences(_service);
        
        var pack = m_ListOfPack.Find(p => p.packId == effect.packId);
        m_ScrollViewPackDetails.Clear();
        m_ScrollViewPackDetails.Add(new Label("Effect name: " + effect.effectName));
        m_ScrollViewPackDetails.Add(new Label("Pack name: " + pack.packName));
        m_ScrollViewPackDetails.Add(new Label("Target used: " + effect.targetUsed));
        m_ScrollViewPackDetails.Add(new Label("URP status: " + effect.moveUrpStatus));
        m_ScrollViewPackDetails.Add(new Label("Script ref: " + effect.scriptRefCount));
        m_ScrollViewPackDetails.Add(new Label("Asset ref: " + effect.assetSourceRef));

        var materialPath = GetMaterialPath(effect.prefabPath);
        if (materialPath != null)
        {
            m_ScrollViewPackDetails.Add(new Label("Material path: " + materialPath));
        }
    }

    private string GetMaterialPath(string prefabPath)
    {
        var item = AssetDatabase.LoadAssetAtPath<ParticleSystemRenderer>(prefabPath);
        if (item != null && item.sharedMaterial != null)
        {
            // var shaderName = item.;
            // Debug.Log("Test material null : " + (item.sharedMaterial == null));
            // Debug.Log("Test material name : " + (item.sharedMaterial.name));
            // Debug.Log("Test shader name : " + (item.sharedMaterial.shader.name));
            var list = AssetDatabase.FindAssets($"{item.sharedMaterial.name} t:Material");
            foreach (var t in list)
            {
                var path = AssetDatabase.GUIDToAssetPath(t);
                var eles = path.Split('/');
                var fileName = eles[eles.Length - 1];
                // Debug.Log("Found : " + path);
                if (fileName == $"{item.sharedMaterial.name}.mat")
                {
                    return path;   
                }
                return null;
            }
            return null;
        }
        else
        {
            // if (item != null && item.material != null)
            // {
            //     Debug.Log("=== > item.material : " + item.material.name);
            //     var list = AssetDatabase.FindAssets($"{item.material.name} t:Material");
            //     foreach (var t in list)
            //     {
            //         var path = AssetDatabase.GUIDToAssetPath(t);
            //         var eles = path.Split('/');
            //         var fileName = eles[eles.Length - 1];
            //         // Debug.Log("Found : " + path);
            //         if (fileName == $"{item.material.name}.mat")
            //         {
            //             return path;   
            //         }
            //         return null;
            //     }
            // }
            return null;
        }
    }
    #endregion
    #region PackDetails
    private void SetupPackDetails(Box parent)
    {
        var listLabel = new Label("Details");
        listLabel.style.alignSelf = Align.Center;
        listLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        parent.Add(listLabel);

        m_ScrollViewPackDetails = new ScrollView();
        m_ScrollViewPackDetails.showHorizontal = false;
        m_ScrollViewPackDetails.style.flexGrow = 1f;
        parent.Add(m_ScrollViewPackDetails);
    }
    private void ShowPackDetails(string packId)
    {
        m_ScrollViewPackDetails.Clear();
        
        var packFound = m_ListOfPack.Find(p => p.packId == packId);
        if (packFound != null)
        {
            var labelPackName = new Label("Pack name: " + packFound.packName);
            var labelStoreUrl = new Label("Store: " + packFound.packStoreUrl);
            var labelUpdated = new Label("Updated: " + packFound.dateUpdated);
            var labelBuildInSupport = new Label("Build in: " + packFound.buildInSupport);
            var labelURPSupport = new Label("URP: " + packFound.urpSupport);
            var changeToUrpBy = new Label("Change to URP by: " + packFound.changeToUrpBy);
            
            var labelTotalEffects = new Label("Total Effects: " + packFound.effects.Count);

            // labelStoreUrl.style.color = new StyleColor(Color.blue);
            labelStoreUrl.RegisterCallback<MouseDownEvent>(evt =>
            {
                Application.OpenURL(packFound.packStoreUrl);
            });
            
            m_ScrollViewPackDetails.Add(labelPackName);
            m_ScrollViewPackDetails.Add(labelStoreUrl);
            m_ScrollViewPackDetails.Add(labelUpdated);
            m_ScrollViewPackDetails.Add(labelBuildInSupport);
            m_ScrollViewPackDetails.Add(labelURPSupport);
            m_ScrollViewPackDetails.Add(changeToUrpBy);
            m_ScrollViewPackDetails.Add(labelTotalEffects);
        }
    }
    #endregion
    #region Item List
    private void SetupItemList(Box parent)
    {
        var listLabel = new Label("Item List");
        listLabel.style.alignSelf = Align.Center;
        listLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        parent.Add(listLabel);
        
        m_TextSearch = new TextField();
        parent.Add(m_TextSearch);

        var buttonSearch = new Button();
        buttonSearch.text = "Search";
        buttonSearch.clicked += SearchingItem;
        parent.Add(buttonSearch);
        
        var buttonSearchNoRef = new Button();
        buttonSearchNoRef.text = "Search No Ref";
        buttonSearchNoRef.clicked += SearchingItemWithoutAnyReference;
        parent.Add(buttonSearchNoRef);

        m_ScrollViewItem = new ScrollView();
        m_ScrollViewItem.showHorizontal = false;
        m_ScrollViewItem.style.flexGrow = 1f;
        parent.Add(m_ScrollViewItem);
    }

    private void CreateListItem(EffectModel itemData)
    {
        var itemElement = new VisualElement();
        itemElement.style.flexDirection = FlexDirection.Row;
        itemElement.focusable = true;

        var buttonEdit = new Button();
        buttonEdit.text = "E";
        buttonEdit.clicked += () => {
            OpenEditItemWindow(itemData);
        };
        itemElement.Add(buttonEdit);
        
        var nameButton = new Button();
        nameButton.text = itemData.prefabPath;
        nameButton.style.unityTextAlign = new StyleEnum<TextAnchor>();
        nameButton.style.flexGrow = 1f;
        nameButton.clicked += () => {
            // Load object
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(itemData.prefabPath, typeof(UnityEngine.Object));

            EditorUtility.FocusProjectWindow();

            // Select the object in the project folder
            Selection.activeObject = obj;

            // Also flash the folder yellow to highlight it
            EditorGUIUtility.PingObject(obj);
            
            ShowItemDetails(itemData);
        };
        itemElement.Add(nameButton);

        m_ScrollViewItem.contentContainer.Add(itemElement);
    }

    private void LoadItems(string packId)
    {
        var packFound = m_ListOfPack.Find(p => p.packId == packId);
        if (packFound != null)
        {
            m_ScrollViewItem.contentContainer.Clear();
            foreach (var e in packFound.effects)
            {
                CreateListItem(e);
            }
        }
    }

    private void SearchingItem()
    {
        var query = m_TextSearch.value;
        m_ScrollViewItem.contentContainer.Clear();
        foreach (var pack in m_ListOfPack)
        {
            var foundEffect = pack.effects.Find(e => e.effectName.Contains(query));
            if (foundEffect != null)
            {
                CreateListItem(foundEffect);
            }
        }
    }
    private void SearchingItemWithoutAnyReference()
    {
        var query = m_TextSearch.value;
        m_ScrollViewItem.contentContainer.Clear();
        foreach (var pack in m_ListOfPack)
        {
            foreach (var effect in pack.effects)
            {
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(effect.prefabPath, typeof(UnityEngine.Object));
                var references = _service.GetReferences(new[] { obj });
                var countRef = references[obj].Count;
                if (countRef <= 0)
                {
                    CreateListItem(effect);
                }
            }
        }
    }
    #endregion

    #region Package
    private void SetupPackageList(Box parent)
    {
        var listLabel = new Label("Packages");
        listLabel.style.alignSelf = Align.Center;
        listLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        parent.Add(listLabel);

        m_ScrollViewPackage = new ScrollView();
        m_ScrollViewPackage.showHorizontal = false;
        m_ScrollViewPackage.style.flexGrow = 1f;
        parent.Add(m_ScrollViewPackage);
        
        /*var buttonAdd = new Button();
        buttonAdd.text = "New Pack";
        buttonAdd.clicked += () => {
            OpenEditPackWindow(null);
        };
        parent.Add(buttonAdd);*/
    }
    
    private void CreateListPackage(PackModel pack)
    {
        var packName = pack.packName;
        var itemElement = new VisualElement();
        itemElement.style.flexDirection = FlexDirection.Row;
        itemElement.focusable = true;

        var buttonEdit = new Button();
        buttonEdit.text = "E";
        buttonEdit.clicked += () => {
            OpenEditPackWindow(pack.packId);
        };
        itemElement.Add(buttonEdit);

        var nameButton = new Button();
        nameButton.text = packName;
        nameButton.style.unityTextAlign = new StyleEnum<TextAnchor>();
        nameButton.style.flexGrow = 1f;
        nameButton.clicked += () =>
        {
            LoadItems(pack.packId);
            ShowPackDetails(pack.packId);
        };
        itemElement.Add(nameButton);

        m_ScrollViewPackage.contentContainer.Add(itemElement);
    }

    #endregion

    #region Edit

    private void OpenEditPackWindow(string packNameOrigin)
    {
        if (packNameOrigin == null)
        {
            var window = GetWindow<EditPack>();
            window.titleContent = new GUIContent("New Pack");
            window.minSize = new Vector2(250, 50);
        }
        else
        {
            var packFound = m_ListOfPack.Find(p => p.packId == packNameOrigin);
            if (packFound != null)
            {
                var window = GetWindow<EditPack>();
                window.titleContent = new GUIContent("Edit Pack " + packNameOrigin);
                window.minSize = new Vector2(250, 50);
                window.SetPack(packFound);
            }
        }
    }
    private void OpenEditItemWindow(EffectModel effectModel)
    {
        var window = GetWindow<EditItem>();
        window.titleContent = new GUIContent("Edit Effect " + effectModel.effectName);
        window.minSize = new Vector2(250, 50);
        window.SetEffect(effectModel, m_ListOfPack);
    }

    #endregion

    private void SetupButton(Button button)
    {
        // Reference to the VisualElement inside the button that serves
        // as the button’s icon.
        var buttonIcon = button.Q(className: "quicktool-button-icon");

        // Icon’s path in our project.
        string iconPath = $"Icons/{button.parent.name}_icon";

        // Loads the actual asset from the above path.
        var iconAsset = Resources.Load<Texture2D>(iconPath);

        // Applies the above asset as a background image for the icon.
        buttonIcon.style.backgroundImage = iconAsset;

        // Instantiates our primitive object on a left click.
        button.clickable.clicked += () => CreateObject(button.parent.name);

        // Sets a basic tooltip to the button itself.
        button.tooltip = button.parent.name;
    }

    private void CreateObject(string primitiveTypeName)
    {
        var pt = (PrimitiveType)Enum.Parse
                     (typeof(PrimitiveType), primitiveTypeName, true);
        var go = ObjectFactory.CreatePrimitive(pt);
        go.transform.position = Vector3.zero;
    }

    private void FindAllParticleAutomatically()
    {
        var list = AssetDatabase.FindAssets("t:Prefab");
        var listOfPack = new List<PackModel>();

        if (m_ListOfPack != null)
        {
            for (int i = 0; i < m_ListOfPack.Count; i++)
            {
                var p = m_ListOfPack[i];
                p.effects.Clear();
                listOfPack.Add(p);
            }
        }
        
        var progress = 0.0f;
        var total = list.Length;
        try
        {
            
            foreach (string guid in list)
            {
                progress++;
                EditorUtility.DisplayProgressBar("Effect Tracking", "Collecting all effect prefabs...", progress/total);
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var materialPath = GetMaterialPath(path) ?? "";
                
                Object thisObject = AssetDatabase.LoadAssetAtPath<SMPEffectProperty>(path);
                if (thisObject == null)
                {
                    thisObject = AssetDatabase.LoadAssetAtPath<ParticleSystem>(path);
                }
                if (thisObject != null)
                {
                    char[] sep = { '/' };
                    var splits = path.Split(sep);
                    var pack = splits[1];
                    var effectName = splits[splits.Length - 1];
                    
                    

                    
                    if (effectName.Contains("CFX2") || effectName.Contains("CFXM2") || materialPath.Contains("CFX2") || materialPath.Contains("CFXM2"))
                    {
                        pack = "Cartoon FX Pack 2";
                    }
                    else if (effectName.Contains("CFX3") || effectName.Contains("CFXM3") || materialPath.Contains("CFX3") || materialPath.Contains("CFXM3"))
                    {
                        pack = "Cartoon FX Pack 3";
                    }
                    else if (effectName.Contains("CFX4") || effectName.Contains("CFXM4") || materialPath.Contains("CFX4") || materialPath.Contains("CFXM4"))
                    {
                        pack = "Cartoon FX Pack 4";
                    }
                    else if (effectName.Contains("CFX") || effectName.Contains("CFX1") || effectName.Contains("CFXM1") || materialPath.Contains("CFX") || materialPath.Contains("CFX1") || materialPath.Contains("CFXM1"))
                    {
                        pack = "Cartoon FX Pack 1";
                    }
                    else if (pack.Contains("JMO Assets"))
                    {
                        pack = splits[2];
                    }
                    else if (effectName.Contains("Epic Toon FX") || materialPath.Contains("Epic Toon FX"))
                    {
                        pack = "Epic Toon FX";
                    }
                    else if (effectName.Contains("PinwheelFantasyEffect") || materialPath.Contains("PinwheelFantasyEffect"))
                    {
                        pack = "PinwheelFantasyEffect";
                    }
                    else if (effectName.Contains("Hovl Studio") || materialPath.Contains("Hovl Studio"))
                    {
                        pack = "Hovl Studio";
                    }
                    else if (effectName.Contains("ShFX") || materialPath.Contains("ShFX"))
                    {
                        pack = "Shape FX";
                    }
                    

                    var packFound = listOfPack.Find(pk => pk.packName == pack);
                    if (packFound == null)
                    {
                        var packId = EditPack.GenerateID();
                        listOfPack.Add(new PackModel()
                        {
                            packName = pack,
                            packId = packId,
                            effects = new List<EffectModel>()
                            {
                                new EffectModel()
                                {
                                    prefabPath = path,
                                    packId = packId,
                                    effectName = effectName
                                }
                            }
                        });
                    }
                    else
                    {
                        packFound.effects.Add(new EffectModel()
                        {
                            prefabPath = path,
                            packId = packFound.packId,
                            effectName = effectName
                        });
                    }
                }
            }
            
            SaveToFile(listOfPack);
            
            EditorUtility.ClearProgressBar();
        
        }
        catch (Exception e)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError(e.ToString());
        }
    }
    
    private void SavePacksByAutomicallyCheck()
    {
        var listOfPack = new List<PackModel>();
        var list = AssetDatabase.FindAssets("t:Prefab");
        Debug.Log("Found list : " + list.Length);
        foreach(var p in list)
        {
            var path = AssetDatabase.GUIDToAssetPath(p);
            
            char[] sep = { '/' };
            var splits = path.Split(sep);
            var pack = splits[1];
            var effectName = splits[splits.Length - 1];

            var packFound = listOfPack.Find(pk => pk.packName == pack);
            if (packFound == null)
            {
                listOfPack.Add(new PackModel()
                {
                    packName = pack,
                    packId = pack,
                    effects = new List<EffectModel>()
                    {
                        new EffectModel()
                        {
                            prefabPath = path,
                            effectName = effectName
                        }
                    }
                });
            }
            else
            {
                packFound.effects.Add(new EffectModel()
                {
                    prefabPath = path,
                    effectName = effectName
                });
            }
        }

        SaveToFile(listOfPack);
    }
    private void LoadPacks()
    {
        m_ListOfPack = LoadFromFile();
        FindAllParticleAutomatically();
        if (m_ListOfPack == null || m_ListOfPack.Count <= 0)
        {
            m_ListOfPack = LoadFromFile();
        }
        
        foreach (var pack in m_ListOfPack)
        {
            CreateListPackage(pack);
        }
    }

    private void ReloadPackAndCreateList()
    {
        m_ListOfPack.Clear();
        m_ListOfPack = LoadFromFile();
        foreach (var pack in m_ListOfPack)
        {
            CreateListPackage(pack);
        }
    }

    public void ReloadPacks()
    {
        m_ScrollViewItem.Clear();
        m_ScrollViewPackage.Clear();
        m_ScrollViewPackDetails.Clear();
        
        m_ListOfPack.Clear();
        
        ReloadPackAndCreateList();
    }


    public static void SaveToFile(List<PackModel> listOfPack)
    {
        var json = JsonUtility.ToJson(new SavePackModel(){packs = listOfPack}, true);
        File.WriteAllText(Application.dataPath + "/Editor/EffectTrackings/Resources/packs.json", json);
    }
    public static List<PackModel> LoadFromFile()
    {
        var path = Application.dataPath + "/Editor/EffectTrackings/Resources/packs.json";
        
        if (File.Exists((path)))
        {
            var json = File.ReadAllText(path);
            var savePackModel = JsonUtility.FromJson<SavePackModel>(json);
            var listOfPack = savePackModel.packs;
            return listOfPack;    
        }

        return new List<PackModel>();
    }
    
    public static string GenerateName(int len)
    { 
        Random r = new Random();
        string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x" };
        string[] vowels = { "a", "e", "i", "o", "u", "ae", "y" };
        string Name = "";
        Name += consonants[r.Next(consonants.Length)].ToUpper();
        Name += vowels[r.Next(vowels.Length)];
        int b = 2; //b tells how many times a new letter has been added. It's 2 right now because the first two letters are already in the name.
        while (b < len)
        {
            Name += consonants[r.Next(consonants.Length)];
            b++;
            Name += vowels[r.Next(vowels.Length)];
            b++;
        }

        return Name;


    }
    
}

#region Model

[Serializable]
public class SavePackModel
{
    public List<PackModel> packs;
}

[Serializable]
public class PackModel
{
    public string packId;
    public string packName;
    public string packStoreUrl;
    public string dateUpdated;
    public bool urpSupport;
    public bool buildInSupport;
    public string changeToUrpBy;
    public List<EffectModel> effects;
}

[Serializable]
public class EffectModel
{
    public string targetUsed;
    public string usedFor;
    public string effectName;
    public string packId;
    public string moveUrpStatus;
    public int scriptRefCount;
    public string prefabPath;
    public string assetSourceRef;
}

#endregion