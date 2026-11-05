using System;
using UnityEngine;
using TMPro;

/// <summary>
/// Quản lý đa ngôn ngữ Tiếng Việt & Tiếng Hàn (Chuyển đổi hoàn toàn theo từng ngôn ngữ, không song ngữ).
/// Tự động cập nhật nội dung TextMeshPro và Sprites khi người chơi bấm nút đổi ngôn ngữ trong Cài Đặt.
/// Tự động nạp Korean Dynamic Fallback Font để hiển thị 100% chuẩn tiếng Hàn.
/// </summary>
public class LocalizationManager : MonoBehaviour
{
    private static LocalizationManager _instance;
    public static LocalizationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<LocalizationManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("LocalizationManager");
                    _instance = go.AddComponent<LocalizationManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        if (_instance == null)
        {
            var inst = Instance;
            Debug.Log("<color=green>[LANG]</color> LocalizationManager tự động kích hoạt trước khi tải Scene!");
        }
    }

    public enum LanguageMode
    {
        Vietnamese = 0,
        Korean = 1
    }

    [SerializeField] private LanguageMode currentLanguage = LanguageMode.Vietnamese;
    public LanguageMode CurrentLanguage => currentLanguage;

    public TMP_FontAsset KrTMPFontAsset { get; private set; }
    public TMP_FontAsset VnTMPFontAsset { get; private set; }
    public Font KrOSFont { get; private set; }
    public Font VnOSFont { get; private set; }

    public event Action<LanguageMode> OnLanguageChanged;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            SetupFallbackFonts();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void SetupFallbackFonts()
    {
        try
        {
            // 1. Nạp Font Tiếng Việt mặc định (BeVietnamPro-ExtraBold SDF)
            VnTMPFontAsset = Resources.Load<TMP_FontAsset>("Fonts/BeVietnamPro-ExtraBold SDF");
#if UNITY_EDITOR
            if (VnTMPFontAsset == null)
            {
                VnTMPFontAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-ExtraBold SDF.asset");
            }
#endif
            if (VnTMPFontAsset == null)
            {
                VnTMPFontAsset = TMP_Settings.defaultFontAsset;
            }

            // 2. Nạp Font Tiếng Hàn chuẩn (Ưu tiên nạp bản SDF đã nướng sẵn đầy đủ ký tự)
            KrTMPFontAsset = Resources.Load<TMP_FontAsset>("Fonts/MalgunGothic-Bold SDF");
#if UNITY_EDITOR
            if (KrTMPFontAsset == null)
            {
                KrTMPFontAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/MalgunGothic-Bold SDF.asset");
            }
#endif

            // Nạp Dynamic OS Font tiếng Hàn cho UI thông thường và tạo dynamic fallback
            KrOSFont = Font.CreateDynamicFontFromOSFont(new string[] { "Malgun Gothic", "Gulim", "Batang", "Apple SD Gothic Neo" }, 36);

            // Nếu chưa có KrTMPFontAsset tĩnh, tạo Dynamic TMP Font Asset từ KrOSFont với AtlasPopulationMode.Dynamic
            if (KrTMPFontAsset == null && KrOSFont != null)
            {
                KrTMPFontAsset = TMP_FontAsset.CreateFontAsset(KrOSFont);
                if (KrTMPFontAsset != null)
                {
                    KrTMPFontAsset.name = "Korean-Dynamic-Fallback";
                    KrTMPFontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                }
            }

            // 3. Liên kết Fallback Table toàn cục để bất kỳ văn bản nào thiếu glyph đều tự động hiển thị chữ Hàn sắc nét
            if (KrTMPFontAsset != null)
            {
                if (TMP_Settings.fallbackFontAssets == null)
                {
                    TMP_Settings.fallbackFontAssets = new System.Collections.Generic.List<TMP_FontAsset>();
                }
                if (!TMP_Settings.fallbackFontAssets.Contains(KrTMPFontAsset))
                {
                    TMP_Settings.fallbackFontAssets.Add(KrTMPFontAsset);
                }

                if (VnTMPFontAsset != null)
                {
                    if (VnTMPFontAsset.fallbackFontAssetTable == null)
                    {
                        VnTMPFontAsset.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
                    }
                    if (!VnTMPFontAsset.fallbackFontAssetTable.Contains(KrTMPFontAsset))
                    {
                        VnTMPFontAsset.fallbackFontAssetTable.Add(KrTMPFontAsset);
                    }
                }
                Debug.Log("<color=green>[LANG FONT]</color> Đã kích hoạt Font tiếng Hàn (Malgun Gothic) 100% chuẩn nét!");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[SetupFallbackFonts] " + ex.Message);
        }
    }

    /// <summary>
    /// Gán trực tiếp Font tương ứng với ngôn ngữ đang chọn cho TextMeshPro.
    /// Giải quyết triệt để lỗi ô vuông (□□□) khi chuyển sang tiếng Hàn.
    /// </summary>
    public void ApplyFont(TMPro.TMP_Text tmp)
    {
        if (tmp == null) return;
        if (currentLanguage == LanguageMode.Korean && KrTMPFontAsset != null)
        {
            tmp.font = KrTMPFontAsset;
        }
        else if (currentLanguage == LanguageMode.Vietnamese && VnTMPFontAsset != null)
        {
            tmp.font = VnTMPFontAsset;
        }
        tmp.SetAllDirty();
    }

    /// <summary>
    /// Gán trực tiếp Font tương ứng cho UnityEngine.UI.Text thông thường.
    /// Hỗ trợ hoàn hảo Unity 6 (LegacyRuntime.ttf hoặc Dynamic OS Font, không dùng Arial.ttf đã lỗi thời).
    /// </summary>
    public void ApplyFont(UnityEngine.UI.Text txt)
    {
        if (txt == null) return;
        try
        {
            if (currentLanguage == LanguageMode.Korean && KrOSFont != null)
            {
                txt.font = KrOSFont;
            }
            else if (currentLanguage == LanguageMode.Vietnamese)
            {
                if (VnOSFont == null)
                {
                    try { VnOSFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
                    if (VnOSFont == null)
                    {
                        try { VnOSFont = Font.CreateDynamicFontFromOSFont(new string[] { "Segoe UI", "Arial", "Tahoma" }, 20); } catch { }
                    }
                }
                if (VnOSFont != null) txt.font = VnOSFont;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[ApplyFont Text] " + ex.Message);
        }
    }

    /// <summary>
    /// Tự động quét và áp dụng Font tiếng tương ứng cho toàn bộ cây phân cấp GameObject.
    /// </summary>
    public void ApplyFontToHierarchy(GameObject root)
    {
        if (root == null) return;
        var tmps = root.GetComponentsInChildren<TMPro.TMP_Text>(true);
        foreach (var t in tmps)
        {
            ApplyFont(t);
        }
        var texts = root.GetComponentsInChildren<UnityEngine.UI.Text>(true);
        foreach (var t in texts)
        {
            ApplyFont(t);
        }
    }

    /// <summary>
    /// Chuyển đổi giữa Tiếng Việt và Tiếng Hàn (Không song ngữ)
    /// </summary>
    public void ToggleNextLanguage()
    {
        SetLanguage(currentLanguage == LanguageMode.Vietnamese ? LanguageMode.Korean : LanguageMode.Vietnamese);
    }

    public void SetLanguage(LanguageMode mode)
    {
        currentLanguage = mode;
        Debug.Log($"<color=green>[LANG]</color> Ngôn ngữ hiện tại: {currentLanguage}");
        OnLanguageChanged?.Invoke(currentLanguage);
    }

    public string GetLanguageDisplayName()
    {
        return currentLanguage == LanguageMode.Korean ? "한국어" : "Tiếng Việt";
    }

    // Các chuỗi hiển thị theo ngữ cảnh (Đơn ngữ 100%, không dùng gạch chéo '/')
    public string TextTitle => currentLanguage == LanguageMode.Korean ? "신화 범퍼: 한-베 친선 축제" : "LỄ HỘI LINH VẬT: ĐẤU ĐẨY HỮU NGHỊ";
    public string TextSettingsTitle => ""; // Để trống vì khung gỗ đã có sẵn chữ 3D CÀI ĐẶT / 설정 chạm trổ
    public string TextBGM => currentLanguage == LanguageMode.Korean ? "배경음" : "Nhạc Nền";
    public string TextSFX => currentLanguage == LanguageMode.Korean ? "효과음" : "Hiệu Ứng";
    public string TextCrowd => currentLanguage == LanguageMode.Korean ? "관중 환호" : "Khán Giả";
    public string TextLanguage => currentLanguage == LanguageMode.Korean ? "언어" : "Ngôn Ngữ";
    public string TextClose => currentLanguage == LanguageMode.Korean ? "닫기" : "Đóng";
    public string TextStartBattle => currentLanguage == LanguageMode.Korean ? "경기 시작" : "Bắt Đầu Trận Đấu";
    public string TextBack => currentLanguage == LanguageMode.Korean ? "뒤로 가기" : "Quay Lại";
    public string TextReplay => currentLanguage == LanguageMode.Korean ? "다시 하기" : "Chơi Lại";
    public string TextMapHeader => currentLanguage == LanguageMode.Korean ? "— 경기장 선택 (3개 맵) —" : "— CHỌN SỚI ĐẤU (3 BẢN ĐỒ) —";
    public string TextMapSelectTitle => currentLanguage == LanguageMode.Korean ? "대전 모드" : "ĐẤU ĐỐI KHÁNG";

    public string FormatScore(int vnScore, int krScore, int seconds)
    {
        if (currentLanguage == LanguageMode.Korean)
            return $"베트남  {vnScore}  -  {krScore}  대한민국   ({seconds}초)";
        return $"VIỆT NAM  {vnScore}  -  {krScore}  HÀN QUỐC   ({seconds}s)";
    }

    public string GetRandomCheer()
    {
        if (currentLanguage == LanguageMode.Korean)
        {
            string[] krCheers = { "파이팅!", "대단해!", "호랑이 포효!", "황소 돌진!", "황룡 비상!", "해치 강림!" };
            return krCheers[UnityEngine.Random.Range(0, krCheers.Length)];
        }
        string[] vnCheers = { "CỐ LÊN!", "TUYỆT VỜI!", "HỔ KHIẾU!", "KIM NGƯU!", "RỒNG VÀNG!", "HAECHI TRỪ TÀ!" };
        return vnCheers[UnityEngine.Random.Range(0, vnCheers.Length)];
    }
}
