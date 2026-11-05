#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Tự động đóng gói Asset chuẩn và dựng Scene thi đấu chuẩn:
/// - Khởi động với Title Screen phong cách Lễ Hội Hữu Nghị (Trâu Vàng & Bạch Hổ nâng cúp).
/// - 4 Nút chính: ĐẤU ĐỐI KHÁNG, PvP CHƠI MẠNG, SỔ TAY HỮU NGHỊ, HƯỚNG DẪN.
/// - Match Hub chọn 4 Sới Đấu và Đầy Đủ 10 Tướng (5 Việt Nam vs 5 Hàn Quốc).
/// - Khắc phục 100% lỗi nút bấm (targetGraphic gán đầy đủ, raycastTarget text = false).
/// - Nền sới đấu sạch bóng rác (pic.12-background-chinh), không vẽ đè hai lần sàn đấu.
/// - AI thông minh (Smart AI) né mép sân, né bùn, dự đoán đường đi của người chơi.
/// </summary>
public class ProjectAutoSetup
{
    [MenuItem("Mythic Bumper/Cấu hình Chuẩn Game Di Động & Dựng Toàn Bộ Game")]
    [MenuItem("Mythic Bumper/Dọn dẹp Toàn Bộ Đối tượng Menu & Sắp Xếp Lại Scene")]
    public static void SetupFullGame()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("<color=yellow>[MYTHIC BUMPER]</color> Đang ở Play Mode, tự động dừng Play để dựng Scene an toàn...");
            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += SetupFullGame;
            return;
        }

        // Cấu hình màn hình ngang Landscape
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

        TMPFontAssetBuilder.GenerateFontAssets();
        EnsureTags();
        EnsureSortingLayers();
        ConvertTexturesToSprites();
        EnsurePrefabFolders();
        CreateVFXPrefabs();
        CreateMapPrefabs();
        RemoveUnwantedObjectsFromActiveScene();
        BuildBalancedGameScene();
    }

    private static void EnsureTags()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        string[] requiredTags = { "Enemy", "Player" };
        foreach (string reqTag in requiredTags)
        {
            bool found = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                if (t.stringValue == reqTag)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
                newTag.stringValue = reqTag;
            }
        }
        tagManager.ApplyModifiedProperties();
    }

    private static void EnsureSortingLayers()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty sortingLayers = tagManager.FindProperty("m_SortingLayers");

        string[] requiredLayers = { "Background", "Gameplay", "UI" };
        foreach (string layerName in requiredLayers)
        {
            bool exists = false;
            for (int i = 0; i < sortingLayers.arraySize; i++)
            {
                SerializedProperty element = sortingLayers.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative("name").stringValue == layerName)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                sortingLayers.InsertArrayElementAtIndex(sortingLayers.arraySize);
                SerializedProperty newLayer = sortingLayers.GetArrayElementAtIndex(sortingLayers.arraySize - 1);
                newLayer.FindPropertyRelative("name").stringValue = layerName;
                newLayer.FindPropertyRelative("uniqueID").intValue = 1000 + sortingLayers.arraySize;
            }
        }
        tagManager.ApplyModifiedProperties();
    }

    private static void EnsurePrefabFolders()
    {
        string[] folders = {
            "Assets/Prefabs",
            "Assets/Prefabs/Characters",
            "Assets/Prefabs/Items",
            "Assets/Prefabs/Arenas",
            "Assets/Prefabs/Maps",
            "Assets/Prefabs/UI",
            "Assets/Prefabs/VFX",
            "Assets/PhysicsMaterials"
        };
        foreach (var f in folders)
        {
            if (!Directory.Exists(f)) Directory.CreateDirectory(f);
        }
        AssetDatabase.Refresh();
    }

    private static void CreateVFXPrefabs()
    {
        CreateSingleVFXPrefab("VFX_KimNguu_Shockwave", "Assets/Sprites/VFX/Vietnam/vfx_song_chan_dong_trong_dong.png", 5);
        CreateSingleVFXPrefab("VFX_HoKhieu_SonHa", "Assets/Sprites/VFX/Korea/vfx_luong_am_thanh_gam_thet.png", 6);
        CreateSingleVFXPrefab("VFX_TigerClaw", "Assets/Sprites/VFX/Korea/vfx_chan_dong_vuot_ho.png", 7);
        CreateSingleVFXPrefab("VFX_Debris_Rice", "Assets/Sprites/VFX/Vietnam/vfx_manh_vo_lua_vang.png", 8);
        CreateSingleVFXPrefab("VFX_Debris_Bamboo", "Assets/Sprites/VFX/Vietnam/vfx_la_tre_bay_xao_xac.png", 8);
    }

    private static void CreateSingleVFXPrefab(string name, string spritePath, int order)
    {
        string prefabPath = $"Assets/Prefabs/VFX/{name}.prefab";
        if (File.Exists(prefabPath)) return;

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null) return;

        GameObject tempObj = new GameObject(name);
        var sr = tempObj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Gameplay";
        sr.sortingOrder = order;

        PrefabUtility.SaveAsPrefabAsset(tempObj, prefabPath);
        Object.DestroyImmediate(tempObj);
    }

    private static void CreateMapPrefabs()
    {
        CreateSingleMapPrefab("Map_FestivalArena", "Assets/Sprites/Maps/san1-lang.png", 3.65f);
        CreateSingleMapPrefab("Map_MountainCloud", "Assets/Sprites/Maps/san-yentu.png", 3.65f);
        CreateSingleMapPrefab("Map_NightMarket", "Assets/Sprites/Maps/dan-dempho.png", 3.25f);
        CreateSingleMapPrefab("Map_PalaceFog", "Assets/Sprites/Maps/san1-lang.png", 3.65f);
    }

    private static void CreateSingleMapPrefab(string mapName, string bgSpritePath, float arenaRadius = 3.65f)
    {
        string prefabPath = $"Assets/Prefabs/Maps/{mapName}.prefab";
        if (File.Exists(prefabPath)) return;

        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgSpritePath);
        if (bgSprite == null) return;

        GameObject mapObj = new GameObject(mapName);

        GameObject bgObj = new GameObject("Full_Background");
        bgObj.transform.SetParent(mapObj.transform, false);
        bgObj.transform.position = new Vector3(0, 0, 2f);
        var sr = bgObj.AddComponent<SpriteRenderer>();
        sr.sprite = bgSprite;
        sr.sortingLayerName = "Background";
        sr.sortingOrder = -10;
        bgObj.AddComponent<FullScreenBackground>();

        GameObject arenaObj = new GameObject("Arena_Stadium");
        arenaObj.transform.SetParent(mapObj.transform, false);
        arenaObj.transform.position = Vector3.zero;

        var ringOutCol = arenaObj.AddComponent<CircleCollider2D>();
        ringOutCol.isTrigger = true;
        ringOutCol.radius = arenaRadius;
        arenaObj.AddComponent<RingOutDetector>();

        PrefabUtility.SaveAsPrefabAsset(mapObj, prefabPath);
        Object.DestroyImmediate(mapObj);
    }

    private static void RemoveUnwantedObjectsFromActiveScene()
    {
        var allObjs = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var obj in allObjs)
        {
            if (obj == null) continue;
            string n = obj.name.ToLower();

            bool isUnwanted = n.Contains("select") || n.Contains("chontuong") || n.Contains("heroselect")
                || n.Contains("khungavatar") || n.Contains("saphoa") || n.Contains("backdrop_panorama")
                || (n.StartsWith("vfx_") && obj.transform.parent == null);

            if (isUnwanted)
            {
                Debug.Log($"<color=orange>[DỌN DẸP]</color> Đã xóa đối tượng thừa khỏi Scene: {obj.name}");
                Object.DestroyImmediate(obj);
            }
        }
    }

    private static void ConvertTexturesToSprites()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites" });
        bool changed = false;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && (importer.textureType != TextureImporterType.Sprite || !importer.alphaIsTransparency))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
                changed = true;
            }
        }

        if (changed) AssetDatabase.Refresh();
    }

    private static void BuildBalancedGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // A. Camera 2D Orthographic
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.0f;
        cam.transform.position = new Vector3(0, 0, -10);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.10f, 0.16f);
        camObj.AddComponent<AudioListener>();

        // B. NỀN TOÀN MÀN HÌNH (Full_Background - Map 1 Sạch 100%)
        GameObject bgObj = new GameObject("Full_Background");
        bgObj.transform.position = new Vector3(0, 0, 2f);
        var bgSr = bgObj.AddComponent<SpriteRenderer>();
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Maps/san1-lang.png");
        bgSr.sprite = bgSprite;
        bgSr.sortingLayerName = "Background";
        bgSr.sortingOrder = -10;
        bgObj.AddComponent<FullScreenBackground>();

        // C. Phân cấp Decorations
        new GameObject("Decorations");

        // D. Sới đấu Đất Nện & Cát Ssireum (Arena_Stadium)
        GameObject arenaObj = new GameObject("Arena_Stadium");
        arenaObj.transform.position = Vector3.zero;
        var ringOutCol = arenaObj.AddComponent<CircleCollider2D>();
        ringOutCol.isTrigger = true;
        ringOutCol.radius = 3.65f;
        arenaObj.AddComponent<RingOutDetector>();
        PrefabUtility.SaveAsPrefabAsset(arenaObj, "Assets/Prefabs/Arenas/Arena_Stadium.prefab");

        // E. Chướng ngại vật: Chum Kimchi, Vại Gốm, Vũng Bùn
        GameObject chumObj = CreateBumper("Obstacle_ChumKimchi", "Assets/Sprites/Items/obstacle_chum_kimchi.png", new Vector3(0, 2.3f, 0), 0.75f, 0.42f);
        PrefabUtility.SaveAsPrefabAsset(chumObj, "Assets/Prefabs/Items/Obstacle_ChumKimchi.prefab");

        GameObject vaiObj = CreateBumper("Obstacle_VaiGom", "Assets/Sprites/Items/obstacle_vai_gom_co.png", new Vector3(0, -2.3f, 0), 0.75f, 0.42f);
        PrefabUtility.SaveAsPrefabAsset(vaiObj, "Assets/Prefabs/Items/Obstacle_VaiGom.prefab");

        GameObject mudObj = CreateMud("Obstacle_VungBun", "Assets/Sprites/Items/obstacle_vung_bun.png", Vector3.zero, 1.0f, 0.75f);
        PrefabUtility.SaveAsPrefabAsset(mudObj, "Assets/Prefabs/Items/Obstacle_VungBun.prefab");

        // F. Người chơi: Trâu Vàng (Việt Nam)
        GameObject playerObj = new GameObject("Player_TrauVang");
        playerObj.tag = "Player";
        playerObj.transform.position = new Vector3(-2.2f, 0, 0);
        playerObj.transform.localScale = new Vector3(0.75f, 0.75f, 1f);

        var playerRenderer = playerObj.AddComponent<SpriteRenderer>();
        Sprite playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Characters/trau_vang_idle.png");
        playerRenderer.sprite = playerSprite;
        playerRenderer.sortingLayerName = "Gameplay";
        playerRenderer.sortingOrder = 5;

        Shader outlineShader = Shader.Find("Custom/SpriteOutline") ?? Shader.Find("Sprites/Default");
        Material playerMat = new Material(outlineShader);
        playerMat.SetColor("_OutlineColor", new Color(1f, 0.85f, 0.2f, 1f));
        playerMat.SetFloat("_OutlineSize", 2.0f);
        playerRenderer.material = playerMat;

        var playerCol = playerObj.AddComponent<CircleCollider2D>();
        playerCol.radius = 0.65f;

        var playerRb = playerObj.AddComponent<Rigidbody2D>();
        playerRb.gravityScale = 0f;
        playerRb.mass = 2.0f;
        playerRb.linearDamping = 0.8f;
        playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        SerializedObject pRbSo = new SerializedObject(playerRb);
        pRbSo.FindProperty("m_Mass").floatValue = 2.0f;
        pRbSo.FindProperty("m_LinearDamping").floatValue = 0.8f;
        pRbSo.FindProperty("m_GravityScale").floatValue = 0f;
        pRbSo.ApplyModifiedProperties();

        var lineRenderer = playerObj.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.08f;
        lineRenderer.endWidth = 0.08f;
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new Color(1f, 0.9f, 0.2f);
        lineRenderer.endColor = new Color(1f, 0.2f, 0.2f);
        lineRenderer.sortingLayerName = "Gameplay";
        lineRenderer.sortingOrder = 6;

        var playerCtrl = playerObj.AddComponent<PlayerController>();
        SerializedObject so = new SerializedObject(playerCtrl);
        so.FindProperty("mass").floatValue = 2.0f;
        so.FindProperty("linearDrag").floatValue = 0.8f;
        so.FindProperty("aimLine").objectReferenceValue = lineRenderer;
        so.ApplyModifiedProperties();

        playerObj.AddComponent<CharacterSkill>();

        var pAnim = playerObj.AddComponent<Animator>();
        var pAc = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Mascots/AC_TrauVang.controller");
        if (pAc != null) pAnim.runtimeAnimatorController = pAc;

        PrefabUtility.SaveAsPrefabAsset(playerObj, "Assets/Prefabs/Characters/Player_TrauVang.prefab");

        // G. Đối thủ: Bạch Hổ Baekho (Hàn Quốc)
        GameObject enemyObj = new GameObject("Enemy_HoBaekho");
        enemyObj.tag = "Enemy";
        enemyObj.transform.position = new Vector3(2.2f, 0, 0);
        enemyObj.transform.localScale = new Vector3(0.75f, 0.75f, 1f);

        var enemyRenderer = enemyObj.AddComponent<SpriteRenderer>();
        Sprite enemySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Characters/ho_baekho_idle.png");
        enemyRenderer.sprite = enemySprite;
        enemyRenderer.sortingLayerName = "Gameplay";
        enemyRenderer.sortingOrder = 5;

        Material enemyMat = new Material(outlineShader);
        enemyMat.SetColor("_OutlineColor", new Color(0.2f, 0.75f, 1f, 1f));
        enemyMat.SetFloat("_OutlineSize", 2.0f);
        enemyRenderer.material = enemyMat;

        var enemyCol = enemyObj.AddComponent<CircleCollider2D>();
        enemyCol.radius = 0.65f;

        var enemyRb = enemyObj.AddComponent<Rigidbody2D>();
        enemyRb.gravityScale = 0f;
        enemyRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        enemyRb.mass = 1.8f;
        enemyRb.linearDamping = 0.8f;
        enemyRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        SerializedObject eRbSo = new SerializedObject(enemyRb);
        eRbSo.FindProperty("m_Mass").floatValue = 1.8f;
        eRbSo.FindProperty("m_LinearDamping").floatValue = 0.8f;
        eRbSo.FindProperty("m_GravityScale").floatValue = 0f;
        eRbSo.ApplyModifiedProperties();

        var aiCtrl = enemyObj.AddComponent<AIController>();
        SerializedObject eCtrlSo = new SerializedObject(aiCtrl);
        eCtrlSo.FindProperty("mass").floatValue = 1.8f;
        eCtrlSo.FindProperty("linearDrag").floatValue = 0.8f;
        eCtrlSo.FindProperty("targetPlayer").objectReferenceValue = playerObj.transform;
        eCtrlSo.ApplyModifiedProperties();

        var eAnim = enemyObj.AddComponent<Animator>();
        var eAc = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Mascots/AC_HoBaekho.controller");
        if (eAc != null) eAnim.runtimeAnimatorController = eAc;

        PrefabUtility.SaveAsPrefabAsset(enemyObj, "Assets/Prefabs/Characters/Enemy_HoBaekho.prefab");

        // H. Quản lý Hiệu ứng & Spawner & GameManager
        GameObject vfxObj = new GameObject("VFX_Manager");
        vfxObj.AddComponent<VFXManager>();

        GameObject spawnerObj = new GameObject("Item_Spawner");
        spawnerObj.AddComponent<ItemSpawner>();

        GameObject gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<GameManager>();
        SerializedObject gmSo = new SerializedObject(gm);
        SerializedProperty mapBgProp = gmSo.FindProperty("mapBackgroundSprites");
        if (mapBgProp != null)
        {
            mapBgProp.arraySize = 4;
            mapBgProp.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Maps/san1-lang.png");
            mapBgProp.GetArrayElementAtIndex(1).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Maps/san-yentu.png");
            mapBgProp.GetArrayElementAtIndex(2).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Maps/dan-dempho.png");
            mapBgProp.GetArrayElementAtIndex(3).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Maps/san1-lang.png");
            gmSo.ApplyModifiedProperties();
        }

        // EventSystem
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // I. Giao diện Mobile HUD trong trận
        GameObject mobileCanvasObj = BuildCleanMobileHUD(gm);

        // J. Giao diện Menu Chính (Match Hub chọn sới & tướng)
        GameObject hubCanvasObj = BuildMainMenuUI();

        // K. Giao diện Popups Độc Lập (Luật, Sổ tay, PvP, Cài đặt - Layer UI, Order 30)
        GameObject popupCanvasObj = BuildPopupCanvas();

        // L. Giao diện Khởi Động (Title Screen từ background-batdaugame.png)
        GameObject titleCanvasObj = BuildTitleCanvas();

        // M. UI Manager độc lập (Luôn Active trong Scene để lắng nghe và điều phối toàn bộ Canvas)
        GameObject uiMgrObj = new GameObject("UI_Manager");
        var menuMgr = uiMgrObj.AddComponent<MenuManager>();
        SetupMenuManagerReferences(menuMgr, titleCanvasObj, hubCanvasObj, mobileCanvasObj, popupCanvasObj);

        // Mặc định: Title Canvas bật, Menu Hub và Mobile HUD ẩn, Popup Canvas bật nhưng các popup con ẩn
        titleCanvasObj.SetActive(true);
        hubCanvasObj.SetActive(false);
        mobileCanvasObj.SetActive(false);
        popupCanvasObj.SetActive(true);
        uiMgrObj.SetActive(true);

        // Lưu Scene và mở lại trực tiếp trên Unity Editor
        string scenePath = "Assets/Scenes/MainGame.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<color=green>[MYTHIC BUMPER]</color> Đã dựng hoàn tất Game Toàn Diện & Menu Hub tại: {scenePath}");
        MythicBumperTestSuite.RunFullTestSuite();
    }

    private static GameObject BuildTitleCanvas()
    {
        GameObject canvasObj = new GameObject("Title_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingLayerName = "UI";
        canvas.sortingOrder = 20;
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Ảnh nền Full Title Screen
        GameObject bgObj = new GameObject("Image_TitleBackground");
        bgObj.transform.SetParent(canvasObj.transform, false);
        RectTransform bgrt = bgObj.AddComponent<RectTransform>();
        bgrt.anchorMin = Vector2.zero;
        bgrt.anchorMax = Vector2.one;
        bgrt.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        Sprite titleBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title/background_title_screen.png");
        if (titleBg != null) bgImg.sprite = titleBg;
        bgImg.raycastTarget = false;

        // Logo Tiêu Đề Chính Lễ Hội Linh Vật
        GameObject logoObj = new GameObject("Image_TitleLogo");
        logoObj.transform.SetParent(canvasObj.transform, false);
        RectTransform logoRt = logoObj.AddComponent<RectTransform>();
        logoRt.anchorMin = new Vector2(0.5f, 0.5f);
        logoRt.anchorMax = new Vector2(0.5f, 0.5f);
        logoRt.pivot = new Vector2(0.5f, 0.5f);
        logoRt.anchoredPosition = new Vector2(0, 310);
        logoRt.sizeDelta = new Vector2(860, 290);
        Image logoImg = logoObj.AddComponent<Image>();
        Sprite logoSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title/title_banner_logo.png");
        if (logoSprite != null) logoImg.sprite = logoSprite;
        logoImg.preserveAspect = true;
        logoImg.raycastTarget = false;

        // Bục Trâu Vàng & Bạch Hổ Nâng Cúp Hữu Nghị
        GameObject podiumObj = new GameObject("Image_TitlePodium");
        podiumObj.transform.SetParent(canvasObj.transform, false);
        RectTransform podRt = podiumObj.AddComponent<RectTransform>();
        podRt.anchorMin = new Vector2(0.5f, 0.5f);
        podRt.anchorMax = new Vector2(0.5f, 0.5f);
        podRt.pivot = new Vector2(0.5f, 0.5f);
        podRt.anchoredPosition = new Vector2(0, -10);
        podRt.sizeDelta = new Vector2(580, 480);
        Image podImg = podiumObj.AddComponent<Image>();
        Sprite podSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title/title_podium_mascots.png");
        if (podSprite != null) podImg.sprite = podSprite;
        podImg.preserveAspect = true;
        podImg.raycastTarget = false;

        // 4 Nút chính ở nửa dưới màn hình
        Sprite btnPlaySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title/btn_title_local_play.png");
        Sprite btnPvPSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title/btn_title_pvp.png");
        Sprite btnCodexSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title/btn_title_codex.png");
        Sprite btnTutSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title/btn_title_tutorial.png");

        GameObject btnPlay = CreateTitleButton(canvasObj.transform, "Btn_Title_LocalPlay", btnPlaySprite, new Vector2(430, 125), new Vector2(-230, -320));
        GameObject btnPvP = CreateTitleButton(canvasObj.transform, "Btn_Title_PvP", btnPvPSprite, new Vector2(430, 125), new Vector2(230, -320));
        GameObject btnCodex = CreateTitleButton(canvasObj.transform, "Btn_Title_Codex", btnCodexSprite, new Vector2(430, 100), new Vector2(-230, -440));
        GameObject btnTut = CreateTitleButton(canvasObj.transform, "Btn_Title_Tutorial", btnTutSprite, new Vector2(430, 100), new Vector2(230, -440));

        // Nút Cài Đặt & Âm Thanh ở góc trên bên trái
        Sprite gearSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title/btn_title_settings.png");
        Sprite sndSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title/btn_title_sound.png");
        GameObject btnSettings = CreateTitleButton(canvasObj.transform, "Btn_Title_Settings", gearSprite, new Vector2(90, 90), new Vector2(-880, 470));
        GameObject btnTitleSnd = CreateTitleButton(canvasObj.transform, "Btn_Title_Sound", sndSprite, new Vector2(90, 90), new Vector2(-770, 470));

        // Gán sự kiện trực tiếp cho các nút trên Title Screen
        btnPlay.GetComponent<Button>().onClick.AddListener(() => MenuManager.Instance?.OnTitleLocalPlayClicked());
        btnPvP.GetComponent<Button>().onClick.AddListener(() => MenuManager.Instance?.OnTitlePvPClicked());
        btnCodex.GetComponent<Button>().onClick.AddListener(() => MenuManager.Instance?.OnTitleCodexClicked());
        btnTut.GetComponent<Button>().onClick.AddListener(() => MenuManager.Instance?.ToggleTutorialPopup());
        btnSettings.GetComponent<Button>().onClick.AddListener(() => MenuManager.Instance?.ToggleSettingsPopup());
        btnTitleSnd.GetComponent<Button>().onClick.AddListener(() => GameManager.Instance?.ToggleSound());

        return canvasObj;
    }

    private static GameObject CreateTitleButton(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 pos)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        if (sprite != null)
        {
            img.sprite = sprite;
            img.preserveAspect = true;
        }
        else
        {
            img.color = new Color(0.9f, 0.4f, 0.1f, 0.95f);
        }
        img.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
        cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = cb;

        return btnObj;
    }

        private static Font GetSafeDefaultFont()
    {
        Font font = null;
        try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
        if (font == null)
        {
            try { font = Font.CreateDynamicFontFromOSFont(new string[] { "Segoe UI", "Malgun Gothic", "Tahoma" }, 20); } catch { }
        }
        return font;
    }

    private static GameObject BuildCleanMobileHUD(GameManager gm)
    {
        GameObject canvasObj = new GameObject("Mobile_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingLayerName = "UI";
        canvas.sortingOrder = 0;
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        Font font = GetSafeDefaultFont();

        // 1. Thanh hiển thị Tỉ số & Thời gian ở mép trên cùng
        GameObject topBanner = new GameObject("Top_HUD");
        topBanner.transform.SetParent(canvasObj.transform, false);
        RectTransform topRect = topBanner.AddComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0.5f, 1f);
        topRect.anchorMax = new Vector2(0.5f, 1f);
        topRect.pivot = new Vector2(0.5f, 1f);
        topRect.anchoredPosition = new Vector2(0, -15);
        topRect.sizeDelta = new Vector2(720, 60);

        Image bgImg = topBanner.AddComponent<Image>();
        bgImg.color = new Color(0.12f, 0.08f, 0.16f, 0.90f);

        GameObject textObj = new GameObject("Text_Score");
        textObj.transform.SetParent(topBanner.transform, false);
        TextMeshProUGUI scoreText = AddTMP(textObj, "VIỆT NAM (베트남)  0  -  0   HÀN QUỐC (대한민국)   (60s)", 24, new Color(1f, 0.95f, 0.8f), TextAlignmentOptions.Center, true);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(700, 60);

        // 2. Góc trên bên trái: Quay Về Menu (Quay Lại) & Tạm Dừng
        Sprite homeIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/btn_quay_lai.png");
        GameObject btnHome = CreateUIButton(canvasObj.transform, "Btn_Home", homeIcon, new Vector2(130, 58), new Vector2(0, 1), new Vector2(85, -50));

        Sprite pauseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/btn_pause_round.png");
        GameObject btnPause = CreateUIButton(canvasObj.transform, "Btn_Pause", pauseSprite, new Vector2(65, 65), new Vector2(0, 1), new Vector2(195, -50));

        // 3. Góc trên bên phải: Âm Thanh & Chơi Lại (↺)
        Sprite soundSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title/btn_title_sound.png");
        GameObject btnSound = CreateUIButton(canvasObj.transform, "Btn_Sound", soundSprite, new Vector2(65, 65), new Vector2(1, 1), new Vector2(-150, -50));

        Sprite restartSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/btn_replay.png");
        GameObject btnRestart = CreateUIButton(canvasObj.transform, "Btn_Restart", restartSprite, new Vector2(65, 65), new Vector2(1, 1), new Vector2(-65, -50));

        // 4. Góc dưới bên phải: Đổi Người (Swap) & Tuyệt Kỹ (Skill)
        Sprite swapSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/btn_swap_tag_team.png");
        CreateUIButton(canvasObj.transform, "Btn_Swap", swapSprite, new Vector2(110, 110), new Vector2(1, 0), new Vector2(-250, 100));

        Sprite skillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/btn_skill_fist.png");
        CreateUIButton(canvasObj.transform, "Btn_Skill", skillSprite, new Vector2(140, 140), new Vector2(1, 0), new Vector2(-110, 110));

        // 5. Dòng text hướng dẫn ở dưới cùng
        GameObject hintObj = new GameObject("Text_Hint");
        hintObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI hintText = AddTMP(hintObj, "Kéo để phóng thẳng tới • Bấm nắm đấm để dậm đất! (상대 방향으로 당겨 돌진하세요!)", 20, new Color(1f, 1f, 1f, 0.85f), TextAlignmentOptions.Left, true);
        RectTransform hintRect = hintObj.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0, 0);
        hintRect.anchorMax = new Vector2(0, 0);
        hintRect.pivot = new Vector2(0, 0);
        hintRect.anchoredPosition = new Vector2(40, 40);
        hintRect.sizeDelta = new Vector2(750, 50);

        // 6. Popup Kết Thúc Trận Đấu
        GameObject victoryPopup = BuildVictoryPopup(canvasObj.transform, font);

        // Gán tham chiếu nút bấm vào GameManager
        if (gm != null)
        {
            SerializedObject soGm = new SerializedObject(gm);
            soGm.FindProperty("scoreText").objectReferenceValue = scoreText;
            soGm.FindProperty("victoryPanel").objectReferenceValue = victoryPopup;
            soGm.FindProperty("btnPause").objectReferenceValue = btnPause.GetComponent<Button>();
            soGm.FindProperty("btnHome").objectReferenceValue = btnHome.GetComponent<Button>();
            soGm.FindProperty("btnRestart").objectReferenceValue = btnRestart.GetComponent<Button>();
            soGm.FindProperty("btnSound").objectReferenceValue = btnSound.GetComponent<Button>();
            soGm.FindProperty("btnReplay").objectReferenceValue = victoryPopup.transform.Find("Btn_Replay")?.GetComponent<Button>();
            soGm.FindProperty("btnVictoryHome").objectReferenceValue = victoryPopup.transform.Find("Btn_Home")?.GetComponent<Button>();
            soGm.ApplyModifiedProperties();
        }

        PrefabUtility.SaveAsPrefabAsset(canvasObj, "Assets/Prefabs/UI/Mobile_HUD.prefab");
        return canvasObj;
    }

    private static GameObject BuildVictoryPopup(Transform parent, Font font)
    {
        GameObject popupObj = new GameObject("VictoryDefeat_Popup");
        popupObj.transform.SetParent(parent, false);

        RectTransform pRect = popupObj.AddComponent<RectTransform>();
        pRect.anchorMin = new Vector2(0.5f, 0.5f);
        pRect.anchorMax = new Vector2(0.5f, 0.5f);
        pRect.pivot = new Vector2(0.5f, 0.5f);
        pRect.anchoredPosition = new Vector2(0, 20);
        pRect.sizeDelta = new Vector2(680, 440);

        Image pImg = popupObj.AddComponent<Image>();
        Sprite popupSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/banner_victory.png");
        if (popupSprite != null)
        {
            pImg.sprite = popupSprite;
            pImg.type = Image.Type.Simple;
            pImg.color = Color.white;
            pImg.preserveAspect = true;
        }
        else
        {
            pImg.color = new Color(0.1f, 0.08f, 0.15f, 0.95f);
        }

        GameObject msgObj = new GameObject("Text_VictoryMessage");
        msgObj.transform.SetParent(popupObj.transform, false);
        TextMeshProUGUI msgText = AddTMP(msgObj, "<size=28><color=#FFD700><b>CHIẾN THẮNG HỮU NGHỊ! / 우정의 승리!</b></color></size>\n\n<size=19>Trâu Vàng đã xuất sắc trụ vững sới đấu!\nHai bên cùng chia sẻ Bánh Chưng & Kimchi!\n(황소와 백호가 반쯩과 김치를 함께 나눕니다!)</size>", 20, Color.white, TextAlignmentOptions.Center, true);
        RectTransform msgRect = msgObj.GetComponent<RectTransform>();
        msgRect.anchoredPosition = new Vector2(0, 20);
        msgRect.sizeDelta = new Vector2(480, 160);

        // Nút CHƠI LẠI (Btn_Replay)
        Sprite replaySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/btn_replay.png");
        CreateUIButton(popupObj.transform, "Btn_Replay", replaySprite, new Vector2(170, 60), new Vector2(0.5f, 0.5f), new Vector2(-120, -165));

        // Nút QUAY LẠI (Btn_Home) từ UI_Accessories_Sheet
        Sprite homeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/btn_quay_lai.png");
        if (homeSprite == null) homeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/btn_home.png");
        CreateUIButton(popupObj.transform, "Btn_Home", homeSprite, new Vector2(170, 60), new Vector2(0.5f, 0.5f), new Vector2(120, -165));

        popupObj.SetActive(false);
        return popupObj;
    }

    private static GameObject CreateUIButton(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 anchor, Vector2 pos)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        btn.colors = cb;

        return btnObj;
    }

    private static GameObject CreateIconButton(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 pos)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = cb;

        return btnObj;
    }

    private static GameObject CreateBumper(string name, string spritePath, Vector3 pos, float scale = 1.0f, float radius = 0.55f)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = pos;
        obj.transform.localScale = new Vector3(scale, scale, 1f);
        var r = obj.AddComponent<SpriteRenderer>();
        r.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        r.sortingLayerName = "Gameplay";
        r.sortingOrder = 1;

        var col = obj.AddComponent<CircleCollider2D>();
        col.radius = radius;
        var rb = obj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        obj.AddComponent<ObstacleBumper>();

        return obj;
    }

    private static GameObject BuildMainMenuUI()
    {
        GameObject canvasObj = new GameObject("MainMenu_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingLayerName = "UI";
        canvas.sortingOrder = 10;
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        Font font = GetSafeDefaultFont();

        // 1. Tấm màn mờ tối
        GameObject backdrop = new GameObject("Menu_Backdrop");
        backdrop.transform.SetParent(canvasObj.transform, false);
        RectTransform bRect = backdrop.AddComponent<RectTransform>();
        bRect.anchorMin = Vector2.zero;
        bRect.anchorMax = Vector2.one;
        bRect.sizeDelta = Vector2.zero;
        Image bImg = backdrop.AddComponent<Image>();
        bImg.color = new Color(0.06f, 0.04f, 0.08f, 0.88f);
        bImg.raycastTarget = false;

        // 2. Tiêu đề chính
        GameObject titleObj = new GameObject("Menu_Title");
        titleObj.transform.SetParent(canvasObj.transform, false);
        RectTransform tRect = titleObj.AddComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0.5f, 1f);
        tRect.anchorMax = new Vector2(0.5f, 1f);
        tRect.pivot = new Vector2(0.5f, 1f);
        tRect.anchoredPosition = new Vector2(0, -35);
        tRect.sizeDelta = new Vector2(1200, 120);

        TextMeshProUGUI titleText = AddTMP(titleObj, "LỄ HỘI LINH VẬT: ĐẤU ĐẨY HỮU NGHỊ\n<size=22><color=#FFD700>KOREA GAME WEEK 2026 • MYTHIC BUMPER • 한-베 친선 축제</color></size>", 34, new Color(1f, 0.88f, 0.25f), TextAlignmentOptions.Center, true);

        // 3. Khung Chọn Sới Đấu ở bên trái (Panel_MapSelect)
        GameObject mapPanel = new GameObject("Panel_MapSelect");
        mapPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform mpRect = mapPanel.AddComponent<RectTransform>();
        mpRect.anchorMin = new Vector2(0.5f, 0.5f);
        mpRect.anchorMax = new Vector2(0.5f, 0.5f);
        mpRect.pivot = new Vector2(0.5f, 0.5f);
        mpRect.anchoredPosition = new Vector2(-380, 20);
        mpRect.sizeDelta = new Vector2(580, 520);

        Image mpBg = mapPanel.AddComponent<Image>();
        Sprite boardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/panel_match_hub_board.png");
        if (boardSprite != null)
        {
            mpBg.sprite = boardSprite;
            mpBg.color = Color.white;
            mpBg.preserveAspect = false;
        }
        else
        {
            mpBg.color = new Color(0.14f, 0.10f, 0.18f, 0.95f);
        }

        // Nhãn phụ chọn sới đấu (nằm bên dưới biển gỗ ĐẤU ĐỐI KHÁNG, không đè lên chữ)
        GameObject mapLabelObj = new GameObject("Label_Map");
        mapLabelObj.transform.SetParent(mapPanel.transform, false);
        TextMeshProUGUI mapLabel = AddTMP(mapLabelObj, "— CHỌN SỚI ĐẤU (3 BẢN ĐỒ) / 경기장 선택 —", 18, new Color(1f, 0.85f, 0.35f), TextAlignmentOptions.Center, true);
        mapLabel.raycastTarget = false;
        RectTransform mlRect = mapLabelObj.GetComponent<RectTransform>();
        mlRect.anchoredPosition = new Vector2(0, 160);
        mlRect.sizeDelta = new Vector2(480, 30);

        // Khung Map Preview hình chữ nhật rõ nét (hiển thị trọn vẹn vẻ đẹp của sàn đấu)
        GameObject mapImgObj = new GameObject("Image_MapPreview");
        mapImgObj.transform.SetParent(mapPanel.transform, false);
        Image mapImg = mapImgObj.AddComponent<Image>();
        Sprite defaultThumb = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Thumbnails/thumb_san1-lang.png");
        mapImg.sprite = defaultThumb;
        mapImg.preserveAspect = true;
        mapImg.raycastTarget = false;
        RectTransform miRect = mapImgObj.GetComponent<RectTransform>();
        miRect.anchoredPosition = new Vector2(0, 45);
        miRect.sizeDelta = new Vector2(360, 200);

        // Tên Bản Đồ
        GameObject mapTitleObj = new GameObject("Text_MapTitle");
        mapTitleObj.transform.SetParent(mapPanel.transform, false);
        TextMeshProUGUI mapTitle = AddTMP(mapTitleObj, "1. Sới Đấu Lễ Hội Dân Gian (민속 축제 경기장)", 20, new Color(1f, 0.90f, 0.40f), TextAlignmentOptions.Center, true);
        mapTitle.raycastTarget = false;
        RectTransform mtRect = mapTitleObj.GetComponent<RectTransform>();
        mtRect.anchoredPosition = new Vector2(0, -75);
        mtRect.sizeDelta = new Vector2(500, 32);

        // Mô tả bản đồ
        GameObject mapDescObj = new GameObject("Text_MapDesc");
        mapDescObj.transform.SetParent(mapPanel.transform, false);
        TextMeshProUGUI mapDesc = AddTMP(mapDescObj, "Sới đấu đất nện kết hợp sàn cát Ssireum hữu nghị giữa hội làng Việt - Hàn.", 15, new Color(0.95f, 0.95f, 0.95f), TextAlignmentOptions.Center, false);
        RectTransform mdRect = mapDescObj.GetComponent<RectTransform>();
        mdRect.anchoredPosition = new Vector2(0, -112);
        mdRect.sizeDelta = new Vector2(480, 45);

        // Hai nút mũi tên điều hướng bản đồ ở hai bên hình Map
        Sprite arrowRedLeft = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/arrow_red_left.png");
        Sprite arrowYellowRight = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/arrow_yellow_right.png");
        Sprite arrowBlueRight = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/arrow_blue_right.png");

        GameObject btnPrevMap = CreateIconButton(mapPanel.transform, "Btn_PrevMap", arrowRedLeft, new Vector2(48, 48), new Vector2(-205, 45));
        GameObject btnNextMap = CreateIconButton(mapPanel.transform, "Btn_NextMap", arrowYellowRight, new Vector2(48, 48), new Vector2(205, 45));

        // 4. Khung Chọn Linh Vật ở bên phải (Panel_CharSelect: 5 VN vs 5 KR)
        GameObject charPanel = new GameObject("Panel_CharSelect");
        charPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform cpRect = charPanel.AddComponent<RectTransform>();
        cpRect.anchorMin = new Vector2(0.5f, 0.5f);
        cpRect.anchorMax = new Vector2(0.5f, 0.5f);
        cpRect.pivot = new Vector2(0.5f, 0.5f);
        cpRect.anchoredPosition = new Vector2(380, 20);
        cpRect.sizeDelta = new Vector2(620, 520);

        Image cpBg = charPanel.AddComponent<Image>();
        Sprite splitBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/character_select_split_bg.png");
        if (splitBg != null)
        {
            cpBg.sprite = splitBg;
            cpBg.color = Color.white;
            cpBg.preserveAspect = false;
        }
        else
        {
            cpBg.color = new Color(0.14f, 0.10f, 0.18f, 0.0f);
        }

        // Nhãn giữ tên đúng quy chuẩn kiểm thử
        GameObject charLabelObj = new GameObject("Label_Char");
        charLabelObj.transform.SetParent(charPanel.transform, false);
        TextMeshProUGUI charLabel = AddTMP(charLabelObj, "— CHỌN LINH VẬT TRANH TÀI (5 vs 5) / 출전 캐릭터 —", 18, new Color(1f, 0.85f, 0.35f), TextAlignmentOptions.Center, true);

        // Player Section (Việt Nam)
        GameObject pCard = new GameObject("Card_Player");
        pCard.transform.SetParent(charPanel.transform, false);
        RectTransform pcRect = pCard.AddComponent<RectTransform>();
        pcRect.anchoredPosition = new Vector2(-155, 30);
        pcRect.sizeDelta = new Vector2(230, 280);
        Image pcImg = pCard.AddComponent<Image>();
        Sprite cardVnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/card_character_vn.png");
        if (cardVnSprite != null) { pcImg.sprite = cardVnSprite; pcImg.color = Color.white; }
        else { pcImg.color = new Color(0.20f, 0.14f, 0.25f, 0.9f); }

        GameObject pAvObj = new GameObject("Image_PlayerAvatar");
        pAvObj.transform.SetParent(pCard.transform, false);
        Image pAvImg = pAvObj.AddComponent<Image>();
        Sprite defaultPAv = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Avatars/avatar_trau_vang.png");
        pAvImg.sprite = defaultPAv;
        pAvImg.preserveAspect = true;
        pAvImg.raycastTarget = false;
        RectTransform pavRect = pAvObj.GetComponent<RectTransform>();
        pavRect.anchoredPosition = new Vector2(0, 42);
        pavRect.sizeDelta = new Vector2(88, 88);

        // Viền Avatar Tròn Trống Đồng (tâm rỗng trong suốt)
        GameObject pAvFrameObj = new GameObject("Image_PlayerAvatarFrame");
        pAvFrameObj.transform.SetParent(pCard.transform, false);
        Image pAvFrame = pAvFrameObj.AddComponent<Image>();
        Sprite circFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/avatar_frame_circle.png");
        if (circFrameSprite != null) pAvFrame.sprite = circFrameSprite;
        pAvFrame.preserveAspect = true;
        pAvFrame.raycastTarget = false;
        RectTransform pavfRect = pAvFrameObj.GetComponent<RectTransform>();
        pavfRect.anchoredPosition = new Vector2(0, 42);
        pavfRect.sizeDelta = new Vector2(104, 104);

        GameObject pTextObj = new GameObject("Text_PlayerMascot");
        pTextObj.transform.SetParent(pCard.transform, false);
        TextMeshProUGUI pText = AddTMP(pTextObj, "VN: Trâu Vàng (Kim Ngưu) / 황소", 18, new Color(1f, 0.90f, 0.50f), TextAlignmentOptions.Center, true); // Vàng kim sáng rực rỡ
        pText.raycastTarget = false;
        RectTransform ptRect = pTextObj.GetComponent<RectTransform>();
        ptRect.anchoredPosition = new Vector2(0, -32);
        ptRect.sizeDelta = new Vector2(220, 28);

        GameObject pRoleObj = new GameObject("Text_PlayerRole");
        pRoleObj.transform.SetParent(pCard.transform, false);
        TextMeshProUGUI pRole = AddTMP(pRoleObj, "Tanker - Trụ Sân • Chiêu: Kim Ngưu Trận Địa", 14, new Color(0.90f, 0.90f, 0.90f), TextAlignmentOptions.Center, false);
        RectTransform prRect = pRoleObj.GetComponent<RectTransform>();
        prRect.anchoredPosition = new Vector2(0, -60);
        prRect.sizeDelta = new Vector2(220, 25);

        // Nút mũi tên chuyển tướng Việt Nam
        GameObject btnPrevP = CreateIconButton(charPanel.transform, "Btn_PrevPlayer", arrowRedLeft, new Vector2(44, 44), new Vector2(-285, 30));
        GameObject btnNextP = CreateIconButton(charPanel.transform, "Btn_NextPlayer", arrowYellowRight, new Vector2(44, 44), new Vector2(-25, 30));

        // Nút Đổi tướng VN (XÁC NHẬN / CONFIRM sạch đẹp không text đè)
        Sprite btnConfirmRed = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/btn_confirm_red.png");
        GameObject btnSwapP = CreateIconButton(charPanel.transform, "Btn_NextPlayerText", btnConfirmRed, new Vector2(165, 50), new Vector2(-155, -135));
        btnSwapP.GetComponent<Button>().onClick.AddListener(() => btnNextP.GetComponent<Button>().onClick.Invoke());

        // BIỂU TƯỢNG VS LỬA Ở CHÍNH GIỮA
        GameObject vsObj = new GameObject("Icon_VS");
        vsObj.transform.SetParent(charPanel.transform, false);
        Image vsImg = vsObj.AddComponent<Image>();
        Sprite vsSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/icon_fire_vs.png");
        if (vsSprite != null) vsImg.sprite = vsSprite;
        vsImg.preserveAspect = true;
        vsImg.raycastTarget = false;
        RectTransform vsRect = vsObj.GetComponent<RectTransform>();
        vsRect.anchoredPosition = new Vector2(0, 30);
        vsRect.sizeDelta = new Vector2(80, 115);

        // Enemy Section (Hàn Quốc)
        GameObject eCard = new GameObject("Card_Enemy");
        eCard.transform.SetParent(charPanel.transform, false);
        RectTransform ecRect = eCard.AddComponent<RectTransform>();
        ecRect.anchoredPosition = new Vector2(155, 30);
        ecRect.sizeDelta = new Vector2(230, 280);
        Image ecImg = eCard.AddComponent<Image>();
        Sprite cardKrSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/card_character_kr.png");
        if (cardKrSprite != null) { ecImg.sprite = cardKrSprite; ecImg.color = Color.white; }
        else { ecImg.color = new Color(0.20f, 0.14f, 0.25f, 0.9f); }

        GameObject eAvObj = new GameObject("Image_EnemyAvatar");
        eAvObj.transform.SetParent(eCard.transform, false);
        Image eAvImg = eAvObj.AddComponent<Image>();
        Sprite defaultEAv = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Avatars/avatar_ho_baekho.png");
        eAvImg.sprite = defaultEAv;
        eAvImg.preserveAspect = true;
        eAvImg.raycastTarget = false;
        RectTransform eavRect = eAvObj.GetComponent<RectTransform>();
        eavRect.anchoredPosition = new Vector2(0, 42);
        eavRect.sizeDelta = new Vector2(88, 88);

        GameObject eAvFrameObj = new GameObject("Image_EnemyAvatarFrame");
        eAvFrameObj.transform.SetParent(eCard.transform, false);
        Image eAvFrame = eAvFrameObj.AddComponent<Image>();
        if (circFrameSprite != null) eAvFrame.sprite = circFrameSprite;
        eAvFrame.preserveAspect = true;
        eAvFrame.raycastTarget = false;
        RectTransform eavfRect = eAvFrameObj.GetComponent<RectTransform>();
        eavfRect.anchoredPosition = new Vector2(0, 42);
        eavfRect.sizeDelta = new Vector2(104, 104);

        GameObject eTextObj = new GameObject("Text_EnemyMascot");
        eTextObj.transform.SetParent(eCard.transform, false);
        TextMeshProUGUI eText = AddTMP(eTextObj, "HQ: Bạch Hổ Baekho / 백호", 18, new Color(1f, 0.90f, 0.50f), TextAlignmentOptions.Center, true);
        eText.raycastTarget = false;
        RectTransform etRect = eTextObj.GetComponent<RectTransform>();
        etRect.anchoredPosition = new Vector2(0, -32);
        etRect.sizeDelta = new Vector2(220, 28);

        GameObject eRoleObj = new GameObject("Text_EnemyRole");
        eRoleObj.transform.SetParent(eCard.transform, false);
        TextMeshProUGUI eRole = AddTMP(eRoleObj, "Bruiser - Sức Mạnh • Chiêu: Hổ Khiếu Sơn Hà", 14, new Color(0.90f, 0.90f, 0.90f), TextAlignmentOptions.Center, false);
        RectTransform erRect = eRoleObj.GetComponent<RectTransform>();
        erRect.anchoredPosition = new Vector2(0, -60);
        erRect.sizeDelta = new Vector2(220, 25);

        // Nút mũi tên chuyển tướng Hàn Quốc
        GameObject btnPrevE = CreateIconButton(charPanel.transform, "Btn_PrevEnemy", arrowRedLeft, new Vector2(44, 44), new Vector2(25, 30));
        GameObject btnNextE = CreateIconButton(charPanel.transform, "Btn_NextEnemy", arrowBlueRight, new Vector2(44, 44), new Vector2(285, 30));

        // Nút Đổi tướng HQ (CHỌN TƯỚNG xanh sạch đẹp không text đè)
        Sprite btnSelectBlue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/btn_select_char_blue.png");
        GameObject btnSwapE = CreateIconButton(charPanel.transform, "Btn_NextEnemyText", btnSelectBlue, new Vector2(165, 50), new Vector2(155, -135));
        btnSwapE.GetComponent<Button>().onClick.AddListener(() => btnNextE.GetComponent<Button>().onClick.Invoke());

        // 5. NÚT ĐIỀU HƯỚNG DƯỚI CÙNG
        // Nút Quay lại (Gỗ QUAY LẠI chuẩn)
        Sprite quayLaiSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/btn_quay_lai.png");
        GameObject btnBack = CreateIconButton(canvasObj.transform, "Btn_BackToTitle", quayLaiSprite, new Vector2(160, 65), new Vector2(-420, -380));

        // Nút Bắt đầu tranh tài (VÀO TRẬN LỬA chuẩn)
        Sprite vaoTranSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/btn_vao_tran_fire.png");
        GameObject btnStart = CreateIconButton(canvasObj.transform, "Btn_StartBattle", vaoTranSprite, new Vector2(300, 95), new Vector2(0, -380));

        // Biển gỗ treo Hướng Dẫn & Cài Đặt (có sẵn chữ vẽ tay sắc nét)
        Sprite signTutSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/sign_huong_dan.png");
        GameObject btnTut = CreateIconButton(canvasObj.transform, "Btn_Tutorial", signTutSprite, new Vector2(165, 68), new Vector2(320, -380));

        Sprite signSetSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/sign_cai_dat.png");
        GameObject btnSnd = CreateIconButton(canvasObj.transform, "Btn_MatchHub_Settings", signSetSprite, new Vector2(180, 72), new Vector2(500, -380));
        btnSnd.GetComponent<Button>()?.onClick.AddListener(() => MenuManager.Instance?.ToggleSettingsPopup());

        return canvasObj;
    }
private static GameObject BuildPopupCanvas()
    {
        GameObject canvasObj = new GameObject("Popup_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingLayerName = "UI";
        canvas.sortingOrder = 30; // Nổi lên trên tất cả các canvas khác
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        Font font = GetSafeDefaultFont();
        BuildTutorialPopup(canvasObj.transform, font);
        BuildCodexPopup(canvasObj.transform, font);
        BuildPvPPopup(canvasObj.transform, font);
        BuildSettingsPopup(canvasObj.transform, font);

        canvasObj.SetActive(true);
        return canvasObj;
    }

    private static void SetupMenuManagerReferences(MenuManager menuMgr, GameObject titleCanvas, GameObject hubCanvas, GameObject mobileCanvas, GameObject popupCanvas)
    {
        SerializedObject so = new SerializedObject(menuMgr);
        so.FindProperty("titleCanvas").objectReferenceValue = titleCanvas;
        so.FindProperty("menuRootCanvas").objectReferenceValue = hubCanvas;
        so.FindProperty("mobileCanvas").objectReferenceValue = mobileCanvas;
        so.FindProperty("popupCanvas").objectReferenceValue = popupCanvas;

        // UI references in hubCanvas
        var mapTitle = hubCanvas.transform.Find("Panel_MapSelect/Text_MapTitle")?.GetComponent<TMP_Text>();
        var mapDesc = hubCanvas.transform.Find("Panel_MapSelect/Text_MapDesc")?.GetComponent<TMP_Text>();
        var mapImg = hubCanvas.transform.Find("Panel_MapSelect/Image_MapPreview")?.GetComponent<Image>();
        var pText = hubCanvas.transform.Find("Panel_CharSelect/Card_Player/Text_PlayerMascot")?.GetComponent<TMP_Text>();
        var pRole = hubCanvas.transform.Find("Panel_CharSelect/Card_Player/Text_PlayerRole")?.GetComponent<TMP_Text>();
        var pAvImg = hubCanvas.transform.Find("Panel_CharSelect/Card_Player/Image_PlayerAvatar")?.GetComponent<Image>();
        var eText = hubCanvas.transform.Find("Panel_CharSelect/Card_Enemy/Text_EnemyMascot")?.GetComponent<TMP_Text>();
        var eRole = hubCanvas.transform.Find("Panel_CharSelect/Card_Enemy/Text_EnemyRole")?.GetComponent<TMP_Text>();
        var eAvImg = hubCanvas.transform.Find("Panel_CharSelect/Card_Enemy/Image_EnemyAvatar")?.GetComponent<Image>();

        if (mapTitle != null) so.FindProperty("mapTitleText").objectReferenceValue = mapTitle;
        if (mapDesc != null) so.FindProperty("mapDescText").objectReferenceValue = mapDesc;
        if (mapImg != null) so.FindProperty("mapPreviewImage").objectReferenceValue = mapImg;
        if (pText != null) so.FindProperty("playerMascotText").objectReferenceValue = pText;
        if (pRole != null) so.FindProperty("playerRoleText").objectReferenceValue = pRole;
        if (pAvImg != null) so.FindProperty("playerAvatarImage").objectReferenceValue = pAvImg;
        if (eText != null) so.FindProperty("enemyMascotText").objectReferenceValue = eText;
        if (eRole != null) so.FindProperty("enemyRoleText").objectReferenceValue = eRole;
        if (eAvImg != null) so.FindProperty("enemyAvatarImage").objectReferenceValue = eAvImg;

        // Popups in popupCanvas
        var tut = popupCanvas.transform.Find("Tutorial_Popup")?.gameObject;
        var codex = popupCanvas.transform.Find("Codex_Popup")?.gameObject;
        var pvp = popupCanvas.transform.Find("PvP_Popup")?.gameObject;
        var settings = popupCanvas.transform.Find("Settings_Popup")?.gameObject;

        if (tut != null) so.FindProperty("tutorialPopup").objectReferenceValue = tut;
        if (codex != null) so.FindProperty("codexPopup").objectReferenceValue = codex;
        if (pvp != null) so.FindProperty("pvpPopup").objectReferenceValue = pvp;
        if (settings != null) so.FindProperty("settingsPopup").objectReferenceValue = settings;

        // Cấu hình 5 Avatars Việt Nam
        SerializedProperty vnAvProp = so.FindProperty("vnAvatars");
        vnAvProp.ClearArray();
        string[] vnAvPaths = {
            "Assets/Sprites/UI/Avatars/avatar_trau_vang.png",
            "Assets/Sprites/UI/Avatars/avatar_rong_vang.png",
            "Assets/Sprites/UI/Avatars/avatar_lan_su.png",
            "Assets/Sprites/UI/Avatars/avatar_be_tre.png",
            "Assets/Sprites/UI/Avatars/avatar_chim_lac.png"
        };
        for (int i = 0; i < vnAvPaths.Length; i++)
        {
            vnAvProp.InsertArrayElementAtIndex(i);
            vnAvProp.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(vnAvPaths[i]);
        }

        // Cấu hình 5 Avatars Hàn Quốc
        SerializedProperty krAvProp = so.FindProperty("krAvatars");
        krAvProp.ClearArray();
        string[] krAvPaths = {
            "Assets/Sprites/UI/Avatars/avatar_ho_baekho.png",
            "Assets/Sprites/UI/Avatars/avatar_haechi.png",
            "Assets/Sprites/UI/Avatars/avatar_dokkaebi.png",
            "Assets/Sprites/UI/Avatars/avatar_gau_dangoon.png",
            "Assets/Sprites/UI/Avatars/avatar_samjoko.png"
        };
        for (int i = 0; i < krAvPaths.Length; i++)
        {
            krAvProp.InsertArrayElementAtIndex(i);
            krAvProp.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(krAvPaths[i]);
        }

        // Cấu hình danh sách Sới Đấu (3 bản đồ chính thức + 1 fallback an toàn)
        SerializedProperty mapsProp = so.FindProperty("maps");
        mapsProp.ClearArray();
        string[,] mapConfigs = new string[,] {
            { "1. Sới Đấu Lễ Hội Dân Gian", "Sới đấu đất nện kết hợp sàn cát Ssireum hữu nghị giữa hội làng Việt - Hàn.", "Assets/Sprites/Maps/san1-lang.png", "Assets/Sprites/UI/Thumbnails/thumb_san1-lang.png", "3.65" },
            { "2. Sơn Cảnh Kỳ Quan (Yên Tử - Seoraksan)", "Sới đấu bồng bềnh giữa mây ngàn và đỉnh núi thiêng liêng, gió lộng thử thách bản lĩnh.", "Assets/Sprites/Maps/san-yentu.png", "Assets/Sprites/UI/Thumbnails/thumb_san-yentu.png", "3.65" },
            { "3. Phố Đêm Rực Rỡ (Hội An - Myeongdong)", "Lung linh ánh đèn lồng phố đêm và các sạp ẩm thực truyền thống rực rỡ sắc màu.", "Assets/Sprites/Maps/dan-dempho.png", "Assets/Sprites/UI/Thumbnails/thumb_dan-dempho.png", "3.25" },
            { "4. Hoàng Cung Cổ Kính (Tạm khóa)", "Cố đô trầm mặc cổ kính trong sương huyền bí. Đang thi công, tạm sử dụng sới Lễ Hội.", "Assets/Sprites/Maps/san1-lang.png", "Assets/Sprites/UI/Thumbnails/thumb_san1-lang.png", "3.65" }
        };
        for (int mIdx = 0; mIdx < 4; mIdx++)
        {
            mapsProp.InsertArrayElementAtIndex(mIdx);
            SerializedProperty mElem = mapsProp.GetArrayElementAtIndex(mIdx);
            mElem.FindPropertyRelative("mapName").stringValue = mapConfigs[mIdx, 0];
            mElem.FindPropertyRelative("description").stringValue = mapConfigs[mIdx, 1];
            mElem.FindPropertyRelative("mapSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(mapConfigs[mIdx, 2]);
            mElem.FindPropertyRelative("thumbnailSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(mapConfigs[mIdx, 3]);
            float radius = float.TryParse(mapConfigs[mIdx, 4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float r) ? r : 3.65f;
            mElem.FindPropertyRelative("arenaRadius").floatValue = radius;
        }

        so.ApplyModifiedProperties();
    }

        private static GameObject CreateButtonWithSpriteAndText(Transform parent, string name, string text, Sprite sprite, Vector2 size, Vector2 pos, Font font, int fontSize)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        if (sprite != null)
        {
            img.sprite = sprite;
            img.color = Color.white;
            img.preserveAspect = false;
        }
        else
        {
            img.color = new Color(0.25f, 0.18f, 0.32f);
        }
        img.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        if (!string.IsNullOrEmpty(text))
        {
            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            Text t = txtObj.AddComponent<Text>();
            t.text = text;
            t.font = font;
            t.fontSize = fontSize;
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.raycastTarget = false;
            RectTransform trt = txtObj.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;
        }

        return btnObj;
    }

private static GameObject CreateButtonWithText(Transform parent, string name, string text, Vector2 size, Vector2 pos, Font font, int fontSize)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.25f, 0.18f, 0.32f);
        img.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.35f, 0.28f, 0.45f);
        cb.pressedColor = new Color(0.18f, 0.12f, 0.24f);
        btn.colors = cb;

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        Text t = txtObj.AddComponent<Text>();
        t.text = text;
        t.font = font;
        t.fontSize = fontSize;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.raycastTarget = false;
        RectTransform trt = txtObj.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;

        return btnObj;
    }

    private static GameObject BuildTutorialPopup(Transform parent, Font font)
    {
        GameObject popup = new GameObject("Tutorial_Popup");
        popup.transform.SetParent(parent, false);
        RectTransform rt = popup.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(860, 600);

        Image bg = popup.AddComponent<Image>();
        bg.color = new Color(0.10f, 0.08f, 0.14f, 0.98f);

        GameObject title = new GameObject("Text_Title");
        title.transform.SetParent(popup.transform, false);
        Text tt = title.AddComponent<Text>();
        tt.text = "HƯỚNG DẪN LUẬT TRANH TÀI HỮU NGHỊ";
        tt.font = font;
        tt.fontSize = 28;
        tt.fontStyle = FontStyle.Bold;
        tt.alignment = TextAnchor.MiddleCenter;
        tt.color = new Color(1f, 0.85f, 0.25f);
        tt.raycastTarget = false;
        RectTransform trt = title.GetComponent<RectTransform>();
        trt.anchoredPosition = new Vector2(0, 240);
        trt.sizeDelta = new Vector2(750, 50);

        GameObject body = new GameObject("Text_Body");
        body.transform.SetParent(popup.transform, false);
        Text bt = body.AddComponent<Text>();
        bt.text = "1. <color=#FFD700>ĐIỀU KHIỂN (SLINGSHOT):</color> Chạm vào linh vật, kéo về phía đối thủ để lấy lực và thả tay để phóng thẳng tới.\n\n" +
                  "2. <color=#FFD700>MỤC TIÊU:</color> Đẩy đối thủ ra khỏi ranh giới sới đấu 3 lần để giành thắng lợi hữu nghị (Best of 5).\n\n" +
                  "3. <color=#FFD700>5 LINH VẬT VIỆT NAM:</color> Trâu Vàng (Tanker), Bé Long (Speedster), Lân Sư (Disruptor), Bé Tre (Defender), Chim Lạc (Agile).\n\n" +
                  "4. <color=#FFD700>5 LINH VẬT HÀN QUỐC:</color> Bạch Hổ (Bruiser), Haechi (Trickster), Dokkaebi (Disruptor), Gấu Dangoon (Heavy Tank), Samjoko (Speedster).\n\n" +
                  "5. <color=#FFD700>VẬT PHẨM VĂN HÓA & VẬT CẢN:</color> Nhặt Bánh Chưng, Tteokbokki, Nón Lá trên sân để nhận sức mạnh; né vũng bùn giảm tốc!";
        bt.font = font;
        bt.fontSize = 19;
        bt.lineSpacing = 1.25f;
        bt.alignment = TextAnchor.UpperLeft;
        bt.color = Color.white;
        bt.raycastTarget = false;
        RectTransform brt = body.GetComponent<RectTransform>();
        brt.anchoredPosition = new Vector2(0, 30);
        brt.sizeDelta = new Vector2(760, 340);

        GameObject btnClose = CreateButtonWithText(popup.transform, "Btn_CloseTutorial", "ĐÃ HIỂU! VÀO TRANH TÀI", new Vector2(320, 60), new Vector2(0, -230), font, 20);
        btnClose.GetComponent<Button>().onClick.AddListener(() => popup.SetActive(false));

        popup.SetActive(false);
        return popup;
    }

    private static GameObject BuildCodexPopup(Transform parent, Font font)
    {
        GameObject popup = new GameObject("Codex_Popup");
        popup.transform.SetParent(parent, false);
        RectTransform rt = popup.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(920, 620);

        Image bg = popup.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.06f, 0.12f, 0.98f);

        GameObject title = new GameObject("Text_Title");
        title.transform.SetParent(popup.transform, false);
        Text tt = title.AddComponent<Text>();
        tt.text = "SỔ TAY HỮU NGHỊ VIỆT - HÀN (LORE CODEX)";
        tt.font = font;
        tt.fontSize = 28;
        tt.fontStyle = FontStyle.Bold;
        tt.alignment = TextAnchor.MiddleCenter;
        tt.color = new Color(1f, 0.85f, 0.25f);
        tt.raycastTarget = false;
        RectTransform trt = title.GetComponent<RectTransform>();
        trt.anchoredPosition = new Vector2(0, 250);
        trt.sizeDelta = new Vector2(800, 50);

        GameObject body = new GameObject("Text_Body");
        body.transform.SetParent(popup.transform, false);
        Text bt = body.AddComponent<Text>();
        bt.text = "<color=#FF6B6B>★ ĐỘI TUYỂN VIỆT NAM (5 LINH VẬT):</color>\n" +
                  "• <b>Trâu Vàng:</b> Kim Ngưu vững chãi, biểu trưng cho cần cù, trụ vững sới đấu.\n" +
                  "• <b>Bé Long:</b> Rồng thời Lý bay lượn, tốc độ thần tốc lướt gió chín tầng mây.\n" +
                  "• <b>Lân Sư:</b> Đầu lân rực rỡ mang lại may mắn, phát nổ pháo hoa hỷ khí.\n" +
                  "• <b>Bé Tre:</b> Tinh linh Lũy Tre làng, bất khuất kiên cường che chắn đồng đội.\n" +
                  "• <b>Chim Lạc:</b> Biểu tượng Trống Đồng Đông Sơn, uyển chuyển tạo sóng phong ba.\n\n" +
                  "<color=#4D96FF>★ ĐỘI TUYỂN HÀN QUỐC (5 LINH VẬT):</color>\n" +
                  "• <b>Bạch Hổ Baekho:</b> Mãnh hổ Ssireum dũng mãnh, tiếng gầm vang dội sơn hà.\n" +
                  "• <b>Kỳ Lân Haechi:</b> Linh thú trấn giữ Seoul, phân định thị phi trừ tà phản đòn.\n" +
                  "• <b>Dokkaebi:</b> Yêu tinh thần thoại tay cầm chùy phép, ma mãnh gây bão gai.\n" +
                  "• <b>Gấu Dangoon:</b> Huyền thoại dựng nước xứ Kim Chi, tạo địa chấn ngàn cân.\n" +
                  "• <b>Samjoko:</b> Chim Ba Chân thần thoại mặt trời, băng vút xé toạc không gian.";
        bt.font = font;
        bt.fontSize = 17;
        bt.lineSpacing = 1.25f;
        bt.alignment = TextAnchor.UpperLeft;
        bt.color = Color.white;
        bt.raycastTarget = false;
        RectTransform brt = body.GetComponent<RectTransform>();
        brt.anchoredPosition = new Vector2(0, 30);
        brt.sizeDelta = new Vector2(840, 360);

        GameObject btnClose = CreateButtonWithText(popup.transform, "Btn_CloseCodex", "ĐÓNG SỔ TAY", new Vector2(260, 55), new Vector2(0, -240), font, 18);
        btnClose.GetComponent<Button>().onClick.AddListener(() => popup.SetActive(false));

        popup.SetActive(false);
        return popup;
    }

    private static GameObject BuildPvPPopup(Transform parent, Font font)
    {
        GameObject popup = new GameObject("PvP_Popup");
        popup.transform.SetParent(parent, false);
        RectTransform rt = popup.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(720, 440);

        Image bg = popup.AddComponent<Image>();
        bg.color = new Color(0.10f, 0.08f, 0.16f, 0.98f);

        GameObject title = new GameObject("Text_Title");
        title.transform.SetParent(popup.transform, false);
        Text tt = title.AddComponent<Text>();
        tt.text = "PvP CHƠI MẠNG HỮU NGHỊ";
        tt.font = font;
        tt.fontSize = 28;
        tt.fontStyle = FontStyle.Bold;
        tt.alignment = TextAnchor.MiddleCenter;
        tt.color = new Color(0.3f, 0.85f, 1f);
        tt.raycastTarget = false;
        RectTransform trt = title.GetComponent<RectTransform>();
        trt.anchoredPosition = new Vector2(0, 150);
        trt.sizeDelta = new Vector2(650, 50);

        GameObject body = new GameObject("Text_Body");
        body.transform.SetParent(popup.transform, false);
        Text bt = body.AddComponent<Text>();
        bt.text = "MÃ PHÒNG GIAO LƯU (ROOM ID):\n\n" +
                  "<size=32><color=#FFD700>#KGW2026-VN-KR</color></size>\n\n" +
                  "Đang sẵn sàng kết nối qua Mạng Cục Bộ (LAN / Wi-Fi)!\nHai thiết bị kết nối chung Wi-Fi có thể thách đấu tức thì.";
        bt.font = font;
        bt.fontSize = 20;
        bt.alignment = TextAnchor.MiddleCenter;
        bt.color = Color.white;
        bt.raycastTarget = false;
        RectTransform brt = body.GetComponent<RectTransform>();
        brt.anchoredPosition = new Vector2(0, 20);
        brt.sizeDelta = new Vector2(640, 180);

        GameObject btnClose = CreateButtonWithText(popup.transform, "Btn_ClosePvP", "ĐÃ HIỂU", new Vector2(240, 55), new Vector2(0, -150), font, 18);
        btnClose.GetComponent<Button>().onClick.AddListener(() => popup.SetActive(false));

        popup.SetActive(false);
        return popup;
    }

        private static TextMeshProUGUI AddTMP(GameObject obj, string text, float fontSize, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center, bool bold = true)
    {
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        if (bold) tmp.fontStyle = FontStyles.Bold;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.raycastTarget = false;
        tmp.richText = true;

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-ExtraBold SDF.asset")
                               ?? AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (fontAsset != null) tmp.font = fontAsset;
        return tmp;
    }

private static GameObject CreateTMPText(Transform parent, string name, string text, Vector2 size, Vector2 pos, float fontSize, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Left, bool bold = true)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        if (bold) tmp.fontStyle = FontStyles.Bold;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.raycastTarget = false;
        tmp.richText = true;

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-ExtraBold SDF.asset")
                               ?? AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (fontAsset != null) tmp.font = fontAsset;

        return textObj;
    }

    private static GameObject CreateWoodSlider(Transform parent, string name, Vector2 size, Vector2 pos, string knobSpritePath = "Assets/Sprites/UI/Popups/slider_knob_wood.png", float initialValue = 1.0f)
    {
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent, false);

        RectTransform sliderRt = sliderObj.AddComponent<RectTransform>();
        sliderRt.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRt.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRt.pivot = new Vector2(0.5f, 0.5f);
        sliderRt.sizeDelta = size;
        sliderRt.anchoredPosition = pos;

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = initialValue;

        // Background Track
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        Sprite barSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Popups/slider_bar_wood.png");
        if (barSprite != null) bgImg.sprite = barSprite;
        bgImg.type = Image.Type.Simple;
        bgImg.preserveAspect = false;

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRt.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRt.offsetMin = new Vector2(15, 0);
        fillAreaRt.offsetMax = new Vector2(-15, 0);

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fillObj.AddComponent<RectTransform>();
        fillRt.sizeDelta = Vector2.zero;
        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.color = new Color(0.95f, 0.72f, 0.22f, 0.9f); // Gold/amber fill

        // Handle Slide Area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRt = handleArea.AddComponent<RectTransform>();
        handleAreaRt.anchorMin = Vector2.zero;
        handleAreaRt.anchorMax = Vector2.one;
        handleAreaRt.offsetMin = new Vector2(15, 0);
        handleAreaRt.offsetMax = new Vector2(-15, 0);

        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(handleArea.transform, false);
        RectTransform handleRt = handleObj.AddComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(35, 45);
        Image handleImg = handleObj.AddComponent<Image>();
        Sprite knobSprite = AssetDatabase.LoadAssetAtPath<Sprite>(knobSpritePath);
        if (knobSprite != null)
        {
            handleImg.sprite = knobSprite;
            handleImg.preserveAspect = true;
        }
        else
        {
            handleImg.color = new Color(0.85f, 0.55f, 0.2f, 1f);
        }

        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;

        return sliderObj;
    }

    private static GameObject BuildSettingsPopup(Transform parent, Font font)
    {
        // 1. Tấm màn mờ tối toàn màn hình (Modal Backdrop Dimmer)
        GameObject backdrop = new GameObject("Settings_Backdrop");
        backdrop.transform.SetParent(parent, false);
        RectTransform bRt = backdrop.AddComponent<RectTransform>();
        bRt.anchorMin = Vector2.zero;
        bRt.anchorMax = Vector2.one;
        bRt.sizeDelta = Vector2.zero;
        Image bImg = backdrop.AddComponent<Image>();
        bImg.color = new Color(0f, 0f, 0f, 0.70f);
        bImg.raycastTarget = true; // Chặn click xuyên xuống màn hình dưới

        GameObject popup = new GameObject("Settings_Popup");
        popup.transform.SetParent(parent, false);
        RectTransform rt = popup.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(960, 620); // Khung rộng rãi 960x620px

        Image bg = popup.AddComponent<Image>();
        Sprite windowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Popups/panel_settings_large.png");
        if (windowSprite != null)
        {
            bg.sprite = windowSprite;
            bg.color = Color.white;
            bg.preserveAspect = false;
        }
        else
        {
            bg.color = new Color(0.12f, 0.09f, 0.16f, 0.98f);
        }

        Color textDarkBrown = new Color(0.24f, 0.12f, 0.05f, 1f);

        // Nút X ở góc trên phải (trong suốt, sắc nét)
        Sprite closeXSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Popups/btn_close_x.png");
        GameObject btnCloseX = CreateIconButton(popup.transform, "Btn_Close_X", closeXSprite, new Vector2(46, 46), new Vector2(415, 250));
        if (btnCloseX != null)
        {
            btnCloseX.GetComponent<Button>()?.onClick.AddListener(() => {
                popup.SetActive(false);
                backdrop.SetActive(false);
            });
        }

        // 1. Tiêu đề cài đặt: Song Ngữ Việt - Hàn, chữ to hoàng kim có viền & bóng
        CreateTMPText(popup.transform, "Text_Title", "", new Vector2(500, 48), new Vector2(0, 248), 24, new Color(1f, 0.92f, 0.65f), TextAlignmentOptions.Center, true);

        // 2. Hàng Âm lượng Nhạc nền (BGM) - y = 120
        Sprite musicOnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Popups/btn_music_on.png");
        CreateIconButton(popup.transform, "Btn_Music_Toggle", musicOnSprite, new Vector2(48, 48), new Vector2(-235, 120));
        CreateTMPText(popup.transform, "Text_Label_BGM", "Nhạc Nền", new Vector2(170, 40), new Vector2(-120, 120), 20, textDarkBrown, TextAlignmentOptions.Left, true);
        CreateWoodSlider(popup.transform, "Slider_BGM", new Vector2(210, 36), new Vector2(85, 120), "Assets/Sprites/UI/Popups/slider_knob_wood.png", 1.0f);
        CreateTMPText(popup.transform, "Text_Value_BGM", "100%", new Vector2(60, 36), new Vector2(230, 120), 20, textDarkBrown, TextAlignmentOptions.Center, true);

        // 3. Hàng Âm lượng Hiệu ứng (SFX) - y = 45
        Sprite sfxOnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Popups/btn_sfx_on.png");
        CreateIconButton(popup.transform, "Btn_SFX_Toggle", sfxOnSprite, new Vector2(48, 48), new Vector2(-235, 45));
        CreateTMPText(popup.transform, "Text_Label_SFX", "Hiệu Ứng", new Vector2(170, 40), new Vector2(-120, 45), 20, textDarkBrown, TextAlignmentOptions.Left, true);
        CreateWoodSlider(popup.transform, "Slider_SFX", new Vector2(210, 36), new Vector2(85, 45), "Assets/Sprites/UI/Popups/slider_knob_taegeuk.png", 1.0f);
        CreateTMPText(popup.transform, "Text_Value_SFX", "100%", new Vector2(60, 36), new Vector2(230, 45), 20, textDarkBrown, TextAlignmentOptions.Center, true);

        // 4. Hàng Âm lượng Khán Giả (Crowd) - y = -30
        CreateIconButton(popup.transform, "Btn_Crowd_Toggle", sfxOnSprite, new Vector2(48, 48), new Vector2(-235, -30));
        CreateTMPText(popup.transform, "Text_Label_Crowd", "Khán Giả", new Vector2(170, 40), new Vector2(-120, -30), 20, textDarkBrown, TextAlignmentOptions.Left, true);
        CreateWoodSlider(popup.transform, "Slider_Crowd", new Vector2(210, 36), new Vector2(85, -30), "Assets/Sprites/UI/Popups/slider_knob_side_drum.png", 1.0f);
        CreateTMPText(popup.transform, "Text_Value_Crowd", "100%", new Vector2(60, 36), new Vector2(230, -30), 20, textDarkBrown, TextAlignmentOptions.Center, true);

        // 5. Hàng Ngôn ngữ Tương Tác (Chuyển đổi Tiếng Việt & Hàn Quốc) - y = -105
        CreateTMPText(popup.transform, "Text_Label_Lang", "Ngôn Ngữ", new Vector2(170, 40), new Vector2(-120, -105), 20, textDarkBrown, TextAlignmentOptions.Left, true);
        
        GameObject btnLang = new GameObject("Btn_Lang_Toggle");
        btnLang.transform.SetParent(popup.transform, false);
        RectTransform blRt = btnLang.AddComponent<RectTransform>();
        blRt.anchorMin = new Vector2(0.5f, 0.5f);
        blRt.anchorMax = new Vector2(0.5f, 0.5f);
        blRt.sizeDelta = new Vector2(250, 48);
        blRt.anchoredPosition = new Vector2(85, -105);
        Image blImg = btnLang.AddComponent<Image>();
        blImg.color = new Color(0.92f, 0.85f, 0.70f, 0.95f); // Khung giấy thẻ bài sáng
        Button blBtn = btnLang.AddComponent<Button>();
        blBtn.targetGraphic = blImg;
        blBtn.onClick.AddListener(() => MenuManager.Instance?.OnLanguageButtonClicked());

        GameObject langTextObj = CreateTMPText(btnLang.transform, "Text_Value_Lang", "Tiếng Việt", new Vector2(240, 40), Vector2.zero, 18, new Color(0.48f, 0.16f, 0.06f, 1f), TextAlignmentOptions.Center, true);

        // 6. Nút Đóng gỗ truyền thống ("ĐÓNG / 닫기") - y = -180
        GameObject btnClose = new GameObject("Btn_CloseSettings");
        btnClose.transform.SetParent(popup.transform, false);
        RectTransform btnCloseRt = btnClose.AddComponent<RectTransform>();
        btnCloseRt.anchorMin = new Vector2(0.5f, 0.5f);
        btnCloseRt.anchorMax = new Vector2(0.5f, 0.5f);
        btnCloseRt.pivot = new Vector2(0.5f, 0.5f);
        btnCloseRt.anchoredPosition = new Vector2(0, -180);
        btnCloseRt.sizeDelta = new Vector2(230, 78);

        Image btnImg = btnClose.AddComponent<Image>();
        Sprite closeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Popups/btn_settings_dong.png");
        if (closeSprite != null)
        {
            btnImg.sprite = closeSprite;
            btnImg.color = Color.white;
            btnImg.preserveAspect = true;
        }
        else
        {
            btnImg.color = new Color(0.8f, 0.3f, 0.1f);
        }
        btnImg.raycastTarget = true;

        Button btn = btnClose.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(() => {
            popup.SetActive(false);
            backdrop.SetActive(false);
        });

        popup.SetActive(false);
        backdrop.SetActive(false);
        return popup;
    }
private static GameObject CreateMud(string name, string spritePath, Vector3 pos, float scale = 1.0f, float radius = 0.70f)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = pos;
        obj.transform.localScale = new Vector3(scale, scale, 1f);

        var sr = obj.AddComponent<SpriteRenderer>();
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (s != null) sr.sprite = s;
        sr.sortingLayerName = "Gameplay";
        sr.sortingOrder = 1;

        var col = obj.AddComponent<CircleCollider2D>();
        col.radius = radius;
        col.isTrigger = true;
        obj.AddComponent<MudObstacle>();

        return obj;
    }
}

[InitializeOnLoad]
public static class AutoRunSetupOnce
{
    static AutoRunSetupOnce()
    {
        EditorApplication.delayCall += RunOnce;
    }

    private static void RunOnce()
    {
        string flagPath = "Temp/rebuild_v12.flag";
        if (File.Exists(flagPath))
        {
            try
            {
                File.Delete(flagPath);
                Debug.Log("<color=green>[MYTHIC BUMPER]</color> Đang tự động dựng hoàn tất Game Toàn Diện & Title Screen...");
                ProjectAutoSetup.SetupFullGame();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[AutoRunSetupOnce] " + ex.Message);
            }
        }
    }
}
#endif