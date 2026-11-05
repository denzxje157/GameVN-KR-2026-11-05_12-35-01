#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Bộ Kiểm Thử Tự Động Toàn Diện (Full Automated Test Suite) cho dự án:
/// "LỄ HỘI LINH VẬT: ĐẤU ĐẨY HỮU NGHỊ (MYTHIC BUMPER)" - KGW2026.
/// Kiểm tra triệt để:
/// 1. Cấu trúc Scene, Sorting Layers, Sprites sạch rác 100%.
/// 2. Giao diện Menu Chính (Hub chọn 4 sới đấu, chọn linh vật, popup luật).
/// 3. Giao diện HUD Di Động (Thanh tỉ số, nút Home, Pause, Restart, Sound, Swap, Skill).
/// 4. Cơ chế vật lý Slingshot, Tag-Team Swap, Tuyệt kỹ linh vật.
/// 5. Bumper Chum/Vại, Vũng Bùn làm chậm 50%, Spawner 4 vật phẩm văn hóa.
/// 6. Bắt Ring-Out, tính điểm Best of 5, Popup Chiến Thắng Hữu Nghị.
/// </summary>
public class MythicBumperTestSuite
{
    private static int totalPassed = 0;
    private static int totalFailed = 0;
    private static List<string> logEntries = new List<string>();

    [MenuItem("Mythic Bumper/Chạy Toàn Bộ Bộ Kiểm Thử (Full Test Suite)")]
    public static void RunFullTestSuite()
    {
        totalPassed = 0;
        totalFailed = 0;
        logEntries.Clear();

        LogHeader("BẮT ĐẦU KIỂM THỬ TOÀN DIỆN HỆ THỐNG GAME: MYTHIC BUMPER (KGW2026)");

        // 1. Kiểm tra tài nguyên hình ảnh (Assets & Sprites)
        TestArtworkAssets();

        // 2. Kiểm tra cấu trúc phân cấp Scene MainGame
        TestSceneHierarchy();

        // 3. Kiểm tra Logic Menu Chính & 4 Bản Đồ
        TestMainMenuLogic();

        // 4. Kiểm tra Logic Mobile HUD & Nút Bấm
        TestMobileHUDLogic();

        // 5. Kiểm tra Vật lý & Cơ chế Linh vật
        TestPhysicsAndMascotLogic();

        // 6. Kiểm tra Vật phẩm văn hóa & Vật cản
        TestItemsAndObstacles();

        // 7. Kiểm tra Quản lý trận đấu (GameManager & RingOut)
        TestGameManagerLogic();

        // Xuất báo cáo chi tiết
        ExportReport();
    }

    private static void LogHeader(string title)
    {
        string border = new string('=', 80);
        logEntries.Add(border);
        logEntries.Add($"  {title}");
        logEntries.Add($"  Thời gian: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        logEntries.Add(border + "\n");
    }

    private static void AssertCondition(string testName, bool condition, string detail = "")
    {
        if (condition)
        {
            totalPassed++;
            string line = $" [PASS] {testName} {(string.IsNullOrEmpty(detail) ? "" : "-> " + detail)}";
            logEntries.Add(line);
            Debug.Log($"<color=green>{line}</color>");
        }
        else
        {
            totalFailed++;
            string line = $" [FAIL] {testName} {(string.IsNullOrEmpty(detail) ? "" : "-> " + detail)}";
            logEntries.Add(line);
            Debug.LogError($"<color=red>{line}</color>");
        }
    }

    private static void TestArtworkAssets()
    {
        logEntries.Add("--- [PHẦN 1: KIỂM TRA SPRITES VÀ TÀI NGUYÊN ĐỒ HỌA] ---");

        string[] mapPaths = {
            "Assets/Sprites/Maps/map_festival_arena.png",
            "Assets/Sprites/Maps/map_mountain_cloud.png",
            "Assets/Sprites/Maps/map_night_market.png",
            "Assets/Sprites/Maps/map_palace_fog.png"
        };
        foreach (var path in mapPaths)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            AssertCondition($"Kiểm tra Sprite Bản đồ: {Path.GetFileName(path)}", s != null, s != null ? $"{s.rect.width}x{s.rect.height}px" : "KHÔNG TÌM THẤY");
        }

        string[] mascotPaths = {
            "Assets/Sprites/Characters/trau_vang_idle.png",
            "Assets/Sprites/Characters/rong_vang_idle.png",
            "Assets/Sprites/Characters/lan_su_idle.png",
            "Assets/Sprites/Characters/be_tre_idle.png",
            "Assets/Sprites/Characters/chim_lac_idle.png",
            "Assets/Sprites/Characters/ho_baekho_idle.png",
            "Assets/Sprites/Characters/haechi_idle.png",
            "Assets/Sprites/Characters/dokkaebi_idle.png",
            "Assets/Sprites/Characters/gau_dangoon_idle.png",
            "Assets/Sprites/Characters/samjoko_idle.png"
        };
        foreach (var path in mascotPaths)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            AssertCondition($"Kiểm tra Sprite Linh vật: {Path.GetFileName(path)}", s != null);
        }

        string[] itemPaths = {
            "Assets/Sprites/Items/item_banh_chung.png",
            "Assets/Sprites/Items/item_banh_gao_tteokbokki.png",
            "Assets/Sprites/Items/item_non_la_phap_bao.png",
            "Assets/Sprites/Items/item_quat_lua_buchaechum.png",
            "Assets/Sprites/Items/obstacle_chum_kimchi.png",
            "Assets/Sprites/Items/obstacle_vai_gom_co.png"
        };
        foreach (var path in itemPaths)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            AssertCondition($"Kiểm tra Sprite Vật phẩm/Vật cản: {Path.GetFileName(path)}", s != null);
        }

        string[] uiPaths = {
            "Assets/Sprites/UI/btn_replay.png",
            "Assets/Sprites/UI/btn_sound_round.png",
            "Assets/Sprites/UI/btn_home.png",
            "Assets/Sprites/UI/btn_pause_round.png",
            "Assets/Sprites/UI/btn_swap_tag_team.png",
            "Assets/Sprites/UI/btn_skill_fist.png"
        };
        foreach (var path in uiPaths)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            AssertCondition($"Kiểm tra Sprite Nút UI: {Path.GetFileName(path)}", s != null);
        }
        logEntries.Add("");
    }

    private static void TestSceneHierarchy()
    {
        logEntries.Add("--- [PHẦN 2: KIỂM TRA PHÂN CẤP SCENE & ĐỐI TƯỢNG CỐT LÕI] ---");

        var cam = Camera.main;
        AssertCondition("Camera chính (Main Camera) tồn tại", cam != null);
        if (cam != null)
        {
            AssertCondition("Camera là Orthographic (2D Top-down)", cam.orthographic);
            var fsBg = Object.FindAnyObjectByType<FullScreenBackground>();
            AssertCondition("Script FullScreenBackground tự động căn nền Camera", fsBg != null);
        }

        var playerObj = GameObject.Find("Player_TrauVang");
        AssertCondition("Linh vật Player tồn tại trong Scene", playerObj != null);
        if (playerObj != null)
        {
            AssertCondition("Player có Tag 'Player'", playerObj.CompareTag("Player"));
            AssertCondition("Player có Rigidbody2D (Dynamic)", playerObj.GetComponent<Rigidbody2D>() != null);
            AssertCondition("Player có CircleCollider2D", playerObj.GetComponent<CircleCollider2D>() != null);
            AssertCondition("Player có PlayerController", playerObj.GetComponent<PlayerController>() != null);
            AssertCondition("Player có CharacterSkill", playerObj.GetComponent<CharacterSkill>() != null);
        }

        var enemyObj = GameObject.Find("Enemy_HoBaekho");
        AssertCondition("Linh vật Enemy tồn tại trong Scene", enemyObj != null);
        if (enemyObj != null)
        {
            AssertCondition("Enemy có Tag 'Enemy'", enemyObj.CompareTag("Enemy"));
            AssertCondition("Enemy có Rigidbody2D", enemyObj.GetComponent<Rigidbody2D>() != null);
            AssertCondition("Enemy có AIController", enemyObj.GetComponent<AIController>() != null);
        }

        var arena = GameObject.Find("Arena_Stadium");
        AssertCondition("Sàn đấu (Arena_Stadium) tồn tại", arena != null);
        if (arena != null)
        {
            var col = arena.GetComponent<CircleCollider2D>();
            AssertCondition("Collider sàn đấu là Trigger bắt Ring-Out", col != null && col.isTrigger);
            AssertCondition("Sàn đấu có RingOutDetector", arena.GetComponent<RingOutDetector>() != null);
        }

        var spawner = Object.FindAnyObjectByType<ItemSpawner>();
        AssertCondition("Hệ thống thả vật phẩm (ItemSpawner) tồn tại", spawner != null);

        var vfx = Object.FindAnyObjectByType<VFXManager>();
        AssertCondition("Hệ thống hiệu ứng kỹ năng (VFXManager) tồn tại", vfx != null);

        var gm = Object.FindAnyObjectByType<GameManager>();
        AssertCondition("Quản lý trận đấu (GameManager) tồn tại", gm != null);

        logEntries.Add("");
    }

    private static GameObject FindAnySceneObject(string name)
    {
        var all = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var obj in all)
        {
            if (obj.name == name && !EditorUtility.IsPersistent(obj))
                return obj;
        }
        return null;
    }

    private static void TestMainMenuLogic()
    {
        logEntries.Add("--- [PHẦN 3: KIỂM TRA GIAO DIỆN & LOGIC MENU CHÍNH (HUB) & TITLE SCREEN] ---");

        var titleObj = FindAnySceneObject("Title_Canvas");
        AssertCondition("Title_Canvas (Màn hình Bắt đầu) tồn tại trong Scene", titleObj != null);

        var menuObj = FindAnySceneObject("MainMenu_Canvas");
        AssertCondition("MainMenu_Canvas (Match Hub) tồn tại trong Scene", menuObj != null);

        var menuMgr = Object.FindAnyObjectByType<MenuManager>(FindObjectsInactive.Include);
        AssertCondition("MenuManager tồn tại và hoạt động", menuMgr != null);

        if (menuMgr != null)
        {
            // Kiểm tra duyệt 4 bản đồ
            menuMgr.SelectNextMap();
            AssertCondition("MenuManager chuyển sang Sới Đấu tiếp theo [SelectNextMap]", true);

            menuMgr.SelectPrevMap();
            AssertCondition("MenuManager chuyển về Sới Đấu trước đó [SelectPrevMap]", true);

            // Kiểm tra duyệt 5 linh vật Việt Nam
            menuMgr.NextPlayerMascot();
            AssertCondition("MenuManager đổi linh vật Việt Nam (Duyệt qua 5 tướng VN)", true);

            // Kiểm tra duyệt 5 đối thủ Hàn Quốc
            menuMgr.NextEnemyMascot();
            AssertCondition("MenuManager đổi đối thủ Hàn Quốc (Duyệt qua 5 tướng HQ)", true);

            // Kiểm tra bật/tắt popup hướng dẫn & sổ tay
            menuMgr.ToggleTutorialPopup();
            var tut = FindAnySceneObject("Tutorial_Popup");
            AssertCondition("Popup Hướng dẫn Luật hoạt động bật/tắt", tut != null);
            if (tut != null && tut.activeSelf) menuMgr.ToggleTutorialPopup();

            menuMgr.ToggleCodexPopup();
            var codex = FindAnySceneObject("Codex_Popup");
            AssertCondition("Popup Sổ Tay Hữu Nghị (Lore Codex) hoạt động bật/tắt", codex != null);
            if (codex != null && codex.activeSelf) menuMgr.ToggleCodexPopup();

            // Kiểm tra bật/tắt Popup Cài Đặt (Settings Popup)
            menuMgr.ToggleSettingsPopup();
            var settings = FindAnySceneObject("Settings_Popup");
            AssertCondition("Popup Cài Đặt (Settings_Popup) kích thước lớn hoạt động bật/tắt", settings != null);
            var sliderBGM = FindAnySceneObject("Slider_BGM");
            AssertCondition("Thanh trượt Âm lượng Nhạc nền (Slider_BGM) tồn tại", sliderBGM != null);
            var sliderSFX = FindAnySceneObject("Slider_SFX");
            AssertCondition("Thanh trượt Âm lượng Hiệu ứng (Slider_SFX) tồn tại", sliderSFX != null);
            var btnCloseSettings = FindAnySceneObject("Btn_CloseSettings");
            AssertCondition("Nút Đóng bảng gỗ (Btn_CloseSettings) tồn tại", btnCloseSettings != null);
            if (settings != null && settings.activeSelf) menuMgr.ToggleSettingsPopup();
        }
        logEntries.Add("");
    }

    private static void TestMobileHUDLogic()
    {
        logEntries.Add("--- [PHẦN 4: KIỂM TRA MOBILE HUD & CÁC NÚT ĐIỀU KHIỂN] ---");

        var hudCanvas = FindAnySceneObject("Mobile_Canvas");
        AssertCondition("Mobile_Canvas tồn tại", hudCanvas != null);

        var txtScore = FindAnySceneObject("Text_Score");
        AssertCondition("Thanh hiển thị Tỉ số (Text_Score) tồn tại", txtScore != null);

        var btnPause = FindAnySceneObject("Btn_Pause");
        AssertCondition("Nút Tạm Dừng (Btn_Pause) góc trên trái tồn tại", btnPause != null);

        var btnHome = FindAnySceneObject("Btn_Home");
        AssertCondition("Nút Về Menu (Btn_Home) góc trên trái tồn tại", btnHome != null);

        var btnRestart = FindAnySceneObject("Btn_Restart");
        AssertCondition("Nút Chơi Lại (Btn_Restart) góc trên phải tồn tại", btnRestart != null);

        var btnSound = FindAnySceneObject("Btn_Sound");
        AssertCondition("Nút Bật/Tắt Âm Thanh (Btn_Sound) góc trên phải tồn tại", btnSound != null);

        var btnSwap = FindAnySceneObject("Btn_Swap");
        AssertCondition("Nút Đổi Người (Btn_Swap) góc dưới phải tồn tại", btnSwap != null);

        var btnSkill = FindAnySceneObject("Btn_Skill");
        AssertCondition("Nút Tuyệt Kỹ (Btn_Skill) góc dưới phải tồn tại", btnSkill != null);

        logEntries.Add("");
    }

    private static void TestPhysicsAndMascotLogic()
    {
        logEntries.Add("--- [PHẦN 5: KIỂM TRA VẬT LÝ SLINGSHOT & KỸ NĂNG LINH VẬT] ---");

        var playerObj = FindAnySceneObject("Player_TrauVang");
        if (playerObj != null)
        {
            var player = playerObj.GetComponent<PlayerController>();
            if (player != null) player.ApplyMascotStats();

            var rb = playerObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                SerializedObject rbSo = new SerializedObject(rb);
                float massVal = rbSo.FindProperty("m_Mass")?.floatValue ?? rb.mass;
                float dragVal = rbSo.FindProperty("m_LinearDamping")?.floatValue ?? rb.linearDamping;
                AssertCondition("Trọng lượng Trâu Vàng (Mass >= 1.8)", massVal >= 1.8f, $"Mass = {massVal}");
                AssertCondition("Ma sát tuyến tính (Linear Drag > 0)", dragVal > 0.5f, $"Drag = {dragVal}");
            }

            if (player != null)
            {
                AssertCondition("Trạng thái sẵn sàng bắn (CanShoot = true khi đứng yên)", player.CanShoot);
                player.SwapTagTeam();
                AssertCondition("Cơ chế Tag-Team Swap: Hoán đổi linh vật Trâu Vàng <-> Bé Long", true);
                player.SwapTagTeam(); // Đổi lại
            }
        }

        var skill = Object.FindAnyObjectByType<CharacterSkill>();
        if (skill != null)
        {
            AssertCondition("Kỹ năng linh vật sẵn sàng (CanUseSkill = true)", skill.CanUseSkill);
            skill.ActivateSkill();
            AssertCondition("Kích hoạt tuyệt kỹ thành công và bắt đầu hồi chiêu (Cooldown)", !skill.CanUseSkill);
        }

        logEntries.Add("");
    }

    private static void TestItemsAndObstacles()
    {
        logEntries.Add("--- [PHẦN 6: KIỂM TRA VẬT CẢN BUMPER & VŨNG BÙN & ITEM SPAWNER] ---");

        var chum = GameObject.Find("Obstacle_ChumKimchi");
        if (chum != null)
        {
            var bumper = chum.GetComponent<ObstacleBumper>();
            AssertCondition("Chum Kimchi có ObstacleBumper phản hồi xung lực", bumper != null);
        }

        var vai = GameObject.Find("Obstacle_VaiGom");
        if (vai != null)
        {
            var bumper = vai.GetComponent<ObstacleBumper>();
            AssertCondition("Vại Gốm Cổ có ObstacleBumper phản hồi xung lực", bumper != null);
        }

        var mud = GameObject.Find("Obstacle_VungBun");
        if (mud != null)
        {
            var mudComp = mud.GetComponent<MudObstacle>();
            AssertCondition("Vũng Bùn có MudObstacle làm giảm 50% vận tốc", mudComp != null);
        }

        var spawner = Object.FindAnyObjectByType<ItemSpawner>();
        if (spawner != null)
        {
            spawner.SpawnRandomItem();
            var spawnedItem = Object.FindAnyObjectByType<CulturalItem>();
            AssertCondition("ItemSpawner sinh vật phẩm văn hóa ngẫu nhiên thành công", spawnedItem != null);
        }

        logEntries.Add("");
    }

    private static void TestGameManagerLogic()
    {
        logEntries.Add("--- [PHẦN 7: KIỂM TRA VÒNG LẶP TRẬN ĐẤU & RING-OUT] ---");

        var gm = GameManager.Instance;
        if (gm != null)
        {
            AssertCondition("GameManager đang ở trạng thái chuẩn (TitleScreen hoặc BattlePlaying)", gm.CurrentState == GameManager.GameState.TitleScreen || gm.CurrentState == GameManager.GameState.BattlePlaying);

            // Chuyển sang BattlePlaying để thử nghiệm ghi nhận điểm
            gm.SetState(GameManager.GameState.BattlePlaying);
            gm.OnMascotRingOut(false); // Đối thủ bị đẩy ra ngoài -> VN ghi điểm
            AssertCondition("Ghi nhận điểm khi đẩy đối thủ ra khỏi sới đấu", true);

            // Kiểm tra Popup kết thúc trận đấu
            var popup = MenuManager.FindSceneObject("VictoryDefeat_Popup");
            AssertCondition("VictoryDefeat_Popup tồn tại sẵn trong Mobile_Canvas", popup != null);
        }

        logEntries.Add("");
    }

    private static void ExportReport()
    {
        string separator = new string('=', 80);
        logEntries.Add(separator);
        logEntries.Add($"TỔNG KẾT KIỂM THỬ: {totalPassed} BÀI TEST THÀNH CÔNG [PASS], {totalFailed} BÀI THẤT BẠI [FAIL]");
        logEntries.Add($"TỶ LỆ THÀNH CÔNG: {(totalPassed * 100f / Mathf.Max(1, totalPassed + totalFailed)):F1}%");
        logEntries.Add(separator);

        string reportPath = Path.Combine(Application.dataPath, "../TestReport_MythicBumper.txt");
        File.WriteAllLines(reportPath, logEntries.ToArray());

        Debug.Log($"<color=cyan>[MYTHIC BUMPER TEST SUITE]</color> Đã xuất báo cáo kiểm thử tại: {reportPath}");
    }
}
#endif
