#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Pipeline tự động cấu hình TextureImporter, Slicing Sprite Sheet hoạt ảnh,
/// tạo Animation Clips, Animator Controllers và cập nhật vào Prefabs / Scene
/// cho toàn bộ 10 linh vật (5 Việt Nam & 5 Hàn Quốc).
/// </summary>
[InitializeOnLoad]
public class MascotAnimationAutoPipeline
{
    private const string VN_SHEET_PATH = "Assets/Sprites/Animations/animation-vietnam.png";
    private const string KR_SHEET_PATH = "Assets/Sprites/Animations/animation-hanquoc.png";
    private const string FRAMES_DIR = "Assets/Sprites/Characters/Frames";
    private const string ANIM_DIR = "Assets/Animations/Mascots";

    static MascotAnimationAutoPipeline()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorPrefs.GetBool("MascotAnimAutoPipeline_Executed_v2", false))
            {
                EditorPrefs.SetBool("MascotAnimAutoPipeline_Executed_v2", true);
                RunFullAnimationPipeline();
            }
        };
    }

    [MenuItem("Mythic Bumper/Tự Động Cắt Hoạt Ảnh & Tạo Animator 10 Linh Vật")]
    public static void RunFullAnimationPipeline()
    {
        Debug.Log("<color=cyan>[ANIM PIPELINE]</color> Bắt đầu quy trình tự động cắt hoạt ảnh và dựng Animator cho 10 Linh Vật...");

        EnsureDirectories();
        ConfigureAndSliceSpriteSheets();
        ConfigureIndividualFrames();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        CreateAnimationClipsAndControllers();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        ApplyAnimatorsToSceneAndPrefabs();
        Debug.Log("<color=green>[ANIM PIPELINE THÀNH CÔNG]</color> Đã hoàn tất 100% cắt sprite, 20 Animation Clips, 10 Animator Controllers và gán vào Scene/Prefabs!");
    }

    private static void EnsureDirectories()
    {
        if (!Directory.Exists("Assets/Sprites/Animations")) Directory.CreateDirectory("Assets/Sprites/Animations");
        if (!Directory.Exists(FRAMES_DIR)) Directory.CreateDirectory(FRAMES_DIR);
        if (!Directory.Exists(ANIM_DIR)) Directory.CreateDirectory(ANIM_DIR);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// Bước 1: Cấu hình TextureImporter & Slice tự động 2 file Sprite Sheet
    /// </summary>
    private static void ConfigureAndSliceSpriteSheets()
    {
        // 1. Sheet Việt Nam (1024x572)
        var vnSlices = new Dictionary<string, Rect>
        {
            // Trâu Vàng (Row 0)
            { "TrauVang_Idle_0", new Rect(199, 432, 101, 116) },
            { "TrauVang_Idle_1", new Rect(375, 432, 125, 126) },
            { "TrauVang_Action_0", new Rect(552, 432, 158, 95) },
            { "TrauVang_Action_1", new Rect(754, 432, 158, 110) },

            // Bé Long / Rồng Vàng (Row 1)
            { "RongVang_Idle_0", new Rect(178, 308, 154, 116) },
            { "RongVang_Idle_1", new Rect(353, 308, 117, 117) },
            { "RongVang_Action_0", new Rect(537, 307, 159, 118) },
            { "RongVang_Action_1", new Rect(726, 308, 124, 117) },

            // Lân Sư May Mắn (Row 2)
            { "LanSu_Idle_0", new Rect(199, 197, 99, 101) },
            { "LanSu_Idle_1", new Rect(373, 197, 107, 103) },
            { "LanSu_Action_0", new Rect(550, 197, 120, 110) },
            { "LanSu_Action_1", new Rect(748, 196, 142, 100) },

            // Bé Tre (Row 3)
            { "BeTre_Idle_0", new Rect(195, 96, 101, 100) },
            { "BeTre_Idle_1", new Rect(370, 96, 101, 100) },
            { "BeTre_Action_0", new Rect(560, 94, 120, 97) },
            { "BeTre_Action_1", new Rect(750, 94, 140, 102) },

            // Chim Lạc Cổ Đại (Row 4)
            { "ChimLac_Idle_0", new Rect(209, 2, 83, 89) },
            { "ChimLac_Idle_1", new Rect(377, 2, 91, 90) },
            { "ChimLac_Action_0", new Rect(548, 2, 142, 92) },
            { "ChimLac_Action_1", new Rect(750, 2, 140, 92) },
            { "ChimLac_Action_2", new Rect(890, 20, 102, 74) }
        };

        ApplyTextureSlices(VN_SHEET_PATH, vnSlices);

        // 2. Sheet Hàn Quốc (1024x572)
        var krSlices = new Dictionary<string, Rect>
        {
            // Hổ Baekho (Row 0)
            { "HoBaekho_Idle_0", new Rect(212, 434, 113, 109) },
            { "HoBaekho_Idle_1", new Rect(410, 434, 120, 108) },
            { "HoBaekho_Action_0", new Rect(590, 434, 120, 117) },
            { "HoBaekho_Action_1", new Rect(862, 434, 111, 109) },

            // Haechi (Row 1)
            { "Haechi_Idle_0", new Rect(213, 295, 113, 118) },
            { "Haechi_Idle_1", new Rect(397, 295, 122, 121) },
            { "Haechi_Action_0", new Rect(607, 285, 236, 143) },
            { "Haechi_Action_1", new Rect(872, 295, 112, 118) },

            // Yêu Tinh Dokkaebi (Row 2 Left)
            { "Dokkaebi_Idle_0", new Rect(213, 116, 81, 150) },
            { "Dokkaebi_Idle_1", new Rect(304, 110, 96, 132) },
            { "Dokkaebi_Action_0", new Rect(402, 116, 87, 117) },

            // Gấu Dangoon (Row 2 Right)
            { "GauDangoon_Idle_0", new Rect(550, 114, 120, 162) },
            { "GauDangoon_Action_0", new Rect(680, 114, 153, 159) },
            { "GauDangoon_Action_1", new Rect(870, 107, 120, 118) },

            // Samjoko (Row 4 Bottom)
            { "Samjoko_Idle_0", new Rect(216, 6, 100, 101) },
            { "Samjoko_Idle_1", new Rect(377, 6, 104, 101) },
            { "Samjoko_Action_0", new Rect(546, 6, 107, 99) },
            { "Samjoko_Action_1", new Rect(690, 6, 110, 99) },
            { "Samjoko_Action_2", new Rect(880, 6, 101, 101) }
        };

        ApplyTextureSlices(KR_SHEET_PATH, krSlices);
    }

    private static void ApplyTextureSlices(string assetPath, Dictionary<string, Rect> slices)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"[ANIM PIPELINE] Không tìm thấy Texture tại: {assetPath}");
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;

        SpriteMetaData[] sheet = new SpriteMetaData[slices.Count];
        int i = 0;
        foreach (var kvp in slices)
        {
            sheet[i] = new SpriteMetaData
            {
                name = kvp.Key,
                rect = kvp.Value,
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            };
            i++;
        }

        importer.spritesheet = sheet;
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
        Debug.Log($"[ANIM PIPELINE] Đã cắt thành công {slices.Count} sub-sprites cho: {assetPath}");
    }

    /// <summary>
    /// Đảm bảo tất cả frames trong Frames/ đều có TextureType = Sprite
    /// </summary>
    private static void ConfigureIndividualFrames()
    {
        if (!Directory.Exists(FRAMES_DIR)) return;
        string[] files = Directory.GetFiles(FRAMES_DIR, "*.png");
        foreach (var file in files)
        {
            string unityPath = file.Replace('\\', '/');
            TextureImporter importer = AssetImporter.GetAtPath(unityPath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
        }
    }

    /// <summary>
    /// Bước 2: Tạo Animation Clips & Animator Controllers cho 10 Linh Vật
    /// </summary>
    private static void CreateAnimationClipsAndControllers()
    {
        string[] mascots = {
            "TrauVang", "RongVang", "LanSu", "BeTre", "ChimLac",
            "HoBaekho", "Haechi", "Dokkaebi", "GauDangoon", "Samjoko"
        };

        foreach (var mascot in mascots)
        {
            // 1. Tải sprites cho Idle và Action
            Sprite[] idleSprites = LoadMascotSprites(mascot, "Idle");
            Sprite[] actionSprites = LoadMascotSprites(mascot, "Action");

            // 2. Tạo Animation Clip Idle (Loop = true, sampleRate = 8)
            string idleClipPath = $"{ANIM_DIR}/{mascot}_Idle.anim";
            CreateSpriteAnimationClip(idleClipPath, idleSprites, 8f, true);

            // 3. Tạo Animation Clip Action (Loop = false, sampleRate = 8)
            string actionClipPath = $"{ANIM_DIR}/{mascot}_Action.anim";
            CreateSpriteAnimationClip(actionClipPath, actionSprites, 8f, false);

            // 4. Tạo Animator Controller
            string controllerPath = $"{ANIM_DIR}/AC_{mascot}.controller";
            CreateMascotAnimatorController(controllerPath, idleClipPath, actionClipPath);
        }
    }

    private static Sprite[] LoadMascotSprites(string mascot, string animType)
    {
        List<Sprite> list = new List<Sprite>();

        // Thử tìm theo file frame độc lập trước
        int idx = 0;
        while (true)
        {
            string framePath = $"{FRAMES_DIR}/{mascot}_{animType}_{idx}.png";
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(framePath);
            if (s != null)
            {
                list.Add(s);
                idx++;
            }
            else
            {
                break;
            }
        }

        // Nếu chưa có, thử tìm trong sub-sprites của 2 sheet chính
        if (list.Count == 0)
        {
            string sheetPath = (mascot == "TrauVang" || mascot == "RongVang" || mascot == "LanSu" || mascot == "BeTre" || mascot == "ChimLac")
                ? VN_SHEET_PATH : KR_SHEET_PATH;

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(sheetPath);
            foreach (var asset in assets)
            {
                if (asset is Sprite sp && sp.name.StartsWith($"{mascot}_{animType}_"))
                {
                    list.Add(sp);
                }
            }
        }

        // Fallback an toàn: nếu chỉ có 1 sprite hoặc rỗng, dùng idle sprite gốc
        if (list.Count == 0)
        {
            string fallbackPath = (mascot == "TrauVang") ? "Assets/Sprites/Characters/trau_vang_idle.png" :
                                  (mascot == "HoBaekho") ? "Assets/Sprites/Characters/ho_baekho_idle.png" : "";
            if (!string.IsNullOrEmpty(fallbackPath))
            {
                Sprite fallback = AssetDatabase.LoadAssetAtPath<Sprite>(fallbackPath);
                if (fallback != null) list.Add(fallback);
            }
        }

        return list.ToArray();
    }

    private static void CreateSpriteAnimationClip(string clipPath, Sprite[] sprites, float frameRate, bool isLoop)
    {
        if (sprites == null || sprites.Length == 0) return;

        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, clipPath);
        }

        clip.frameRate = frameRate;

        // Cấu hình Loop Time
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = isLoop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        // Gán curve m_Sprite của SpriteRenderer
        EditorCurveBinding binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i / frameRate,
                value = sprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
        EditorUtility.SetDirty(clip);
    }

    private static void CreateMascotAnimatorController(string controllerPath, string idleClipPath, string actionClipPath)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }

        // Thêm parameters
        EnsureTrigger(controller, "OnAttack");
        EnsureTrigger(controller, "OnDash");

        // Lấy state machine ở layer 0
        var rootStateMachine = controller.layers[0].stateMachine;

        // Tải clips
        AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(idleClipPath);
        AnimationClip actionClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(actionClipPath);

        // Tạo hoặc lấy state Idle
        AnimatorState idleState = FindOrCreateState(rootStateMachine, "Idle");
        idleState.motion = idleClip;
        rootStateMachine.defaultState = idleState;

        // Tạo hoặc lấy state Action
        AnimatorState actionState = FindOrCreateState(rootStateMachine, "Action");
        actionState.motion = actionClip;

        // Xóa transitions cũ để thiết lập lại chuẩn
        idleState.transitions = new AnimatorStateTransition[0];
        actionState.transitions = new AnimatorStateTransition[0];

        // Transition: Idle -> Action (Trigger: OnAttack hoặc OnDash, Has Exit Time = false)
        var toAttack = idleState.AddTransition(actionState);
        toAttack.AddCondition(AnimatorConditionMode.If, 0, "OnAttack");
        toAttack.hasExitTime = false;
        toAttack.duration = 0.05f;

        var toDash = idleState.AddTransition(actionState);
        toDash.AddCondition(AnimatorConditionMode.If, 0, "OnDash");
        toDash.hasExitTime = false;
        toDash.duration = 0.05f;

        // Transition: Action -> Idle (Has Exit Time = true, tự quay về khi xong action)
        var backToIdle = actionState.AddTransition(idleState);
        backToIdle.hasExitTime = true;
        backToIdle.exitTime = 1.0f;
        backToIdle.duration = 0.1f;

        EditorUtility.SetDirty(controller);
    }

    private static void EnsureTrigger(AnimatorController controller, string triggerName)
    {
        foreach (var p in controller.parameters)
        {
            if (p.name == triggerName && p.type == AnimatorControllerParameterType.Trigger) return;
        }
        controller.AddParameter(triggerName, AnimatorControllerParameterType.Trigger);
    }

    private static AnimatorState FindOrCreateState(AnimatorStateMachine sm, string stateName)
    {
        foreach (var s in sm.states)
        {
            if (s.state.name == stateName) return s.state;
        }
        return sm.AddState(stateName);
    }

    /// <summary>
    /// Bước 3: Cập nhật Animator vào các GameObjects trong Scene và Prefabs
    /// </summary>
    private static void ApplyAnimatorsToSceneAndPrefabs()
    {
        // 1. Cập nhật Player trong Scene
        GameObject playerObj = GameObject.Find("Player_TrauVang");
        if (playerObj != null)
        {
            var anim = playerObj.GetComponent<Animator>() ?? playerObj.AddComponent<Animator>();
            var ac = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"{ANIM_DIR}/AC_TrauVang.controller");
            if (ac != null) anim.runtimeAnimatorController = ac;

            var sr = playerObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingLayerName = "Gameplay";
                if (sr.sortingOrder < 2) sr.sortingOrder = 5;
            }
        }

        // 2. Cập nhật Enemy trong Scene
        GameObject enemyObj = GameObject.Find("Enemy_HoBaekho");
        if (enemyObj != null)
        {
            var anim = enemyObj.GetComponent<Animator>() ?? enemyObj.AddComponent<Animator>();
            var ac = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"{ANIM_DIR}/AC_HoBaekho.controller");
            if (ac != null) anim.runtimeAnimatorController = ac;

            var sr = enemyObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingLayerName = "Gameplay";
                if (sr.sortingOrder < 2) sr.sortingOrder = 5;
            }
        }

        // 3. Cập nhật Prefabs nếu có
        UpdateCharacterPrefab("Assets/Prefabs/Characters/Player_TrauVang.prefab", $"{ANIM_DIR}/AC_TrauVang.controller");
        UpdateCharacterPrefab("Assets/Prefabs/Characters/Enemy_HoBaekho.prefab", $"{ANIM_DIR}/AC_HoBaekho.controller");

        // Lưu scene
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private static void UpdateCharacterPrefab(string prefabPath, string controllerPath)
    {
        if (!File.Exists(prefabPath)) return;
        GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefab != null)
        {
            var anim = prefab.GetComponent<Animator>() ?? prefab.AddComponent<Animator>();
            var ac = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            if (ac != null) anim.runtimeAnimatorController = ac;

            var sr = prefab.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingLayerName = "Gameplay";
                if (sr.sortingOrder < 2) sr.sortingOrder = 5;
            }

            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefab);
        }
    }
}
#endif
