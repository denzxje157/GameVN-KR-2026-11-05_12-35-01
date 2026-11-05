using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Quản lý Giao diện Menu Chính Toàn Diện (Title Screen & Match Hub):
///  1. Màn hình Khởi Động (Title Screen): Trâu Vàng & Bạch Hổ nâng cúp hữu nghị KGW2026.
///     Gồm: ĐẤU ĐỐI KHÁNG, PvP CHƠI MẠNG, SỔ TAY HỮU NGHỊ, HƯỚNG DẪN, CÀI ĐẶT, ÂM THANH.
///  2. Màn hình Chọn Sới Đấu & Tướng (Match Hub):
///     - Duyệt 4 Sới Đấu: Lễ Hội Dân Gian, Đỉnh Mây, Chợ Đêm, Hoàng Cung.
///     - Chọn 5 Tướng Việt Nam: Trâu Vàng, Bé Long, Lân Sư, Bé Tre, Chim Lạc.
///     - Chọn 5 Tướng Hàn Quốc: Bạch Hổ, Haechi, Dokkaebi, Gấu Dangoon, Samjoko.
///  3. Popups: Sổ Tay Hữu Nghị, Hướng Dẫn Luật, PvP Phòng Đấu, Cài Đặt.
/// Thuộc dự án: Mythic Bumper (KGW2026).
/// Tự động liên kết (Auto-Wiring) 100% nút bấm kể cả khi Canvas đang bị ẩn.
/// </summary>
public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [System.Serializable]
    public class MapData
    {
        public string mapName;
        public string description;
        public Sprite mapSprite;
        public Sprite thumbnailSprite;
        public float arenaRadius = 3.65f;
    }

    [Header("--- QUẢN LÝ CÁC CANVAS CHÍNH ---")]
    [SerializeField] private GameObject titleCanvas;
    [SerializeField] private GameObject menuRootCanvas;
    [SerializeField] private GameObject mobileCanvas;
    [SerializeField] private GameObject popupCanvas;

    [Header("--- DANH SÁCH 4 SỚI ĐẤU ---")]
    [SerializeField] private List<MapData> maps = new List<MapData>();
    private int selectedMapIndex = 0;

    [Header("--- DANH SÁCH 5 LINH VẬT VIỆT NAM ---")]
    private PlayerController.MascotType selectedPlayerMascot = PlayerController.MascotType.TrauVang;
    private int playerMascotIndex = 0;
    private readonly PlayerController.MascotType[] vnMascotList = {
        PlayerController.MascotType.TrauVang,
        PlayerController.MascotType.BeLong,
        PlayerController.MascotType.LanSu,
        PlayerController.MascotType.BeTre,
        PlayerController.MascotType.ChimLac
    };

    [Header("--- DANH SÁCH 5 LINH VẬT HÀN QUỐC ---")]
    private AIController.MascotType selectedEnemyMascot = AIController.MascotType.HoBaekho;
    private int enemyMascotIndex = 0;
    private readonly AIController.MascotType[] krMascotList = {
        AIController.MascotType.HoBaekho,
        AIController.MascotType.Haechi,
        AIController.MascotType.Dokkaebi,
        AIController.MascotType.GauDangoon,
        AIController.MascotType.Samjoko
    };

    [Header("--- THAM CHIẾU UI HUB CHỌN ĐẤU ---")]
    [SerializeField] private TMP_Text mapTitleText;
    [SerializeField] private TMP_Text mapDescText;
    [SerializeField] private Image mapPreviewImage;

    [Header("--- THAM CHIẾU CHỌN TƯỚNG ---")]
    [SerializeField] private TMP_Text playerMascotText;
    [SerializeField] private TMP_Text playerRoleText;
    [SerializeField] private Image playerAvatarImage;
    [SerializeField] private TMP_Text enemyMascotText;
    [SerializeField] private TMP_Text enemyRoleText;
    [SerializeField] private Image enemyAvatarImage;

    [Header("--- POPUPS & NÚT BẤM ---")]
    [SerializeField] private GameObject tutorialPopup;
    [SerializeField] private GameObject codexPopup;
    [SerializeField] private GameObject pvpPopup;
    [SerializeField] private GameObject settingsPopup;
    [SerializeField] private Button btnStartBattle;
    [SerializeField] private Button btnTutorial;
    [SerializeField] private Button btnSound;

    [Header("--- SPRITES AVATARS ---")]
    [SerializeField] private Sprite[] vnAvatars;
    [SerializeField] private Sprite[] krAvatars;

    [Header("--- SPRITES SETTINGS POPUP ---")]
    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;
    [SerializeField] private Sprite sfxOnSprite;
    [SerializeField] private Sprite sfxOffSprite;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
            return;
        }

        FindAllCanvasAndPopupReferences();
        LoadDefaultMapsIfEmpty();
        LoadDefaultAvatarsIfEmpty();
    }

    private void Start()
    {
        FindAllCanvasAndPopupReferences();
        BindAllButtons();
        UpdateMapUI();
        UpdateMascotUI();

        // Đăng ký nhận sự kiện chuyển trạng thái từ GameManager (State Machine)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            HandleStateChanged(GameManager.Instance.CurrentState);
        }
        else
        {
            ShowTitleScreen();
        }

        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= OnLangChanged;
            LocalizationManager.Instance.OnLanguageChanged += OnLangChanged;
        }
        UpdateLocalizedSprites();
        UpdateLocalizedTexts();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }

    private void OnEnable()
    {
        BindAllButtons();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
            GameManager.Instance.OnStateChanged += HandleStateChanged;
        }
    }

    /// <summary>
    /// Tìm kiếm GameObject trong toàn bộ Scene kể cả khi đang bị SetActive(false).
    /// </summary>
    public static GameObject FindSceneObject(string name)
    {
        var allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (var t in allTransforms)
        {
            if (t.gameObject.name == name && t.gameObject.scene.isLoaded)
            {
                return t.gameObject;
            }
        }
        return null;
    }

    /// <summary>
    /// Tìm Button trong toàn bộ Scene kể cả khi đang bị SetActive(false).
    /// </summary>
    public static Button FindButtonInScene(string name)
    {
        var allButtons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (var b in allButtons)
        {
            if (b.gameObject.name == name && b.gameObject.scene.isLoaded)
            {
                return b;
            }
        }
        return null;
    }

    /// <summary>
    /// Tìm Slider trong toàn bộ Scene kể cả khi đang bị SetActive(false).
    /// </summary>
    public static Slider FindSliderInScene(string name)
    {
        var allSliders = Resources.FindObjectsOfTypeAll<Slider>();
        foreach (var s in allSliders)
        {
            if (s.gameObject.name == name && s.gameObject.scene.isLoaded)
            {
                return s;
            }
        }
        return null;
    }

    public void FindAllCanvasAndPopupReferences()
    {
        if (titleCanvas == null) titleCanvas = FindSceneObject("Title_Canvas");
        if (menuRootCanvas == null) menuRootCanvas = FindSceneObject("MainMenu_Canvas");
        if (mobileCanvas == null) mobileCanvas = FindSceneObject("Mobile_Canvas");
        if (popupCanvas == null) popupCanvas = FindSceneObject("Popup_Canvas");

        if (tutorialPopup == null) tutorialPopup = FindSceneObject("Tutorial_Popup");
        if (codexPopup == null) codexPopup = FindSceneObject("Codex_Popup");
        if (pvpPopup == null) pvpPopup = FindSceneObject("PvP_Popup");
        if (settingsPopup == null) settingsPopup = FindSceneObject("Settings_Popup");

        // Tìm các text và image nếu chưa được gán
        if (mapTitleText == null)
        {
            var go = FindSceneObject("Text_MapTitle");
            if (go != null) mapTitleText = go.GetComponent<TMP_Text>();
        }
        if (mapDescText == null)
        {
            var go = FindSceneObject("Text_MapDesc");
            if (go != null) mapDescText = go.GetComponent<TMP_Text>();
        }
        if (mapPreviewImage == null)
        {
            var go = FindSceneObject("Image_MapPreview");
            if (go != null) mapPreviewImage = go.GetComponent<Image>();
        }

        if (playerMascotText == null)
        {
            var go = FindSceneObject("Text_PlayerMascot");
            if (go != null) playerMascotText = go.GetComponent<TMP_Text>();
        }
        if (playerRoleText == null)
        {
            var go = FindSceneObject("Text_PlayerRole");
            if (go != null) playerRoleText = go.GetComponent<TMP_Text>();
        }
        if (playerAvatarImage == null)
        {
            var go = FindSceneObject("Image_PlayerAvatar");
            if (go != null) playerAvatarImage = go.GetComponent<Image>();
        }

        if (enemyMascotText == null)
        {
            var go = FindSceneObject("Text_EnemyMascot");
            if (go != null) enemyMascotText = go.GetComponent<TMP_Text>();
        }
        if (enemyRoleText == null)
        {
            var go = FindSceneObject("Text_EnemyRole");
            if (go != null) enemyRoleText = go.GetComponent<TMP_Text>();
        }
        if (enemyAvatarImage == null)
        {
            var go = FindSceneObject("Image_EnemyAvatar");
            if (go != null) enemyAvatarImage = go.GetComponent<Image>();
        }
    }

    /// <summary>
    /// Kết nối tự động 100% sự kiện cho toàn bộ các nút bấm trong game.
    /// </summary>
    public void BindAllButtons()
    {
        // 1. NÚT TRÊN TITLE SCREEN
        Bind("Btn_Title_LocalPlay", OnTitleLocalPlayClicked);
        Bind("Btn_Title_PvP", OnTitlePvPClicked);
        Bind("Btn_Title_Codex", OnTitleCodexClicked);
        Bind("Btn_Title_Tutorial", ToggleTutorialPopup);
        Bind("Btn_Title_Settings", ToggleSettingsPopup);
        Bind("Btn_Title_Sound", () => GameManager.Instance?.ToggleSound());

        // 2. NÚT TRÊN MATCH HUB
        Bind("Btn_StartBattle", OnStartBattleClicked);
        Bind("Btn_BackToTitle", OnBackToTitleScreen);
        Bind("Btn_PrevMap", SelectPrevMap);
        Bind("Btn_NextMap", SelectNextMap);
        Bind("Btn_PrevPlayer", PrevPlayerMascot);
        Bind("Btn_NextPlayer", NextPlayerMascot);
        Bind("Btn_PrevEnemy", PrevEnemyMascot);
        Bind("Btn_NextEnemy", NextEnemyMascot);
        Bind("Btn_Tutorial", ToggleTutorialPopup);
        Bind("Btn_Sound", ToggleSettingsPopup);
        Bind("Btn_MatchHub_Settings", ToggleSettingsPopup);
        Bind("Btn_Lang_Toggle", OnLanguageButtonClicked);

        // 3. NÚT ĐÓNG CÁC POPUPS
        Bind("Btn_CloseTutorial", () => ClosePopup(tutorialPopup));
        Bind("Btn_CloseCodex", () => ClosePopup(codexPopup));
        Bind("Btn_ClosePvP", () => ClosePopup(pvpPopup));
        Bind("Btn_CloseSettings", () => ClosePopup(settingsPopup));
        Bind("Btn_Close_X", () => ClosePopup(settingsPopup));

        // 4. SLIDERS & TOGGLE TRÊN SETTINGS POPUP
        BindSlider("Slider_BGM", (val) => {
            GameManager.Instance?.SetBGMVolume(val);
            var valText = FindSceneObject("Text_Value_BGM")?.GetComponent<TMPro.TextMeshProUGUI>();
            if (valText != null) valText.text = $"{(int)(val * 100)}%";
            UpdateAudioToggleIcons();
        });
        BindSlider("Slider_Crowd", (val) => {
            AudioManager.Instance?.SetCrowdVolume(val);
            var valText = FindSceneObject("Text_Value_Crowd")?.GetComponent<TMPro.TextMeshProUGUI>();
            if (valText != null) valText.text = $"{(int)(val * 100)}%";
        });
        BindSlider("Slider_SFX", (val) => {
            GameManager.Instance?.SetSFXVolume(val);
            var valText = FindSceneObject("Text_Value_SFX")?.GetComponent<TMPro.TextMeshProUGUI>();
            if (valText != null) valText.text = $"{(int)(val * 100)}%";
            UpdateAudioToggleIcons();
        });
        Bind("Btn_Music_Toggle", () => {
            var slider = FindSliderInScene("Slider_BGM");
            if (slider != null) slider.value = slider.value > 0.05f ? 0f : 1f;
            UpdateAudioToggleIcons();
        });
        Bind("Btn_SFX_Toggle", () => {
            var slider = FindSliderInScene("Slider_SFX");
            if (slider != null) slider.value = slider.value > 0.05f ? 0f : 1f;
            UpdateAudioToggleIcons();
        });

        // 5. NÚT TRÊN MOBILE HUD (TRONG TRẬN)
        Bind("Btn_Home", ReturnToMainMenu);
        Bind("Btn_Pause", () => GameManager.Instance?.TogglePause());
        Bind("Btn_Restart", () => GameManager.Instance?.RestartGame());
        Bind("Btn_Swap", () => {
            var player = Object.FindAnyObjectByType<PlayerController>();
            if (player != null) player.SwapTagTeam();
        });
        Bind("Btn_Skill", () => {
            var skill = Object.FindAnyObjectByType<CharacterSkill>();
            if (skill != null) skill.ActivateSkill();
        });
    }

    private void Bind(string buttonName, UnityEngine.Events.UnityAction action)
    {
        var allButtons = Resources.FindObjectsOfTypeAll<Button>();
        int count = 0;
        foreach (var b in allButtons)
        {
            if (b.gameObject.name == buttonName && b.gameObject.scene.isLoaded)
            {
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(() => {
                    Debug.Log($"<color=cyan>[UI CLICK]</color> Đã nhấn nút: {buttonName}");
                    AudioManager.Instance?.PlayButtonClick();
                    action.Invoke();
                });
                count++;
            }
        }
    }

    private void BindSlider(string sliderName, UnityEngine.Events.UnityAction<float> action)
    {
        var slider = FindSliderInScene(sliderName);
        if (slider != null)
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.onValueChanged.AddListener((val) => {
                Debug.Log($"<color=cyan>[SLIDER CHANGED]</color> {sliderName}: {val:F2}");
                action.Invoke(val);
            });
        }
    }

    // --- ĐIỀU PHỐI GIAO DIỆN THEO STATE MACHINE ---

    public void HandleStateChanged(GameManager.GameState state)
    {
        FindAllCanvasAndPopupReferences();
        switch (state)
        {
            case GameManager.GameState.TitleScreen:
                if (titleCanvas != null) titleCanvas.SetActive(true);
                if (menuRootCanvas != null) menuRootCanvas.SetActive(false);
                if (mobileCanvas != null) mobileCanvas.SetActive(false);
                CloseAllPopups();
                Time.timeScale = 1f;
                Debug.Log("<color=green>[UI SCREEN]</color> Đang ở Màn hình Khởi Động (Title Screen).");
                break;

            case GameManager.GameState.MatchHub:
                if (titleCanvas != null) titleCanvas.SetActive(false);
                if (menuRootCanvas != null) menuRootCanvas.SetActive(true);
                if (mobileCanvas != null) mobileCanvas.SetActive(false);
                CloseAllPopups();
                UpdateMapUI();
                UpdateMascotUI();
                Time.timeScale = 1f;
                Debug.Log("<color=green>[UI SCREEN]</color> Đang ở Match Hub (Chọn Sới & Tướng).");
                break;

            case GameManager.GameState.BattlePlaying:
            case GameManager.GameState.RoundResetting:
                if (titleCanvas != null) titleCanvas.SetActive(false);
                if (menuRootCanvas != null) menuRootCanvas.SetActive(false);
                if (mobileCanvas != null) mobileCanvas.SetActive(true);
                CloseAllPopups();
                break;

            case GameManager.GameState.GameOver:
                if (titleCanvas != null) titleCanvas.SetActive(false);
                if (menuRootCanvas != null) menuRootCanvas.SetActive(false);
                if (mobileCanvas != null) mobileCanvas.SetActive(true);
                break;

            case GameManager.GameState.Paused:
                break;
        }
    }

    public void ShowTitleScreen()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameManager.GameState.TitleScreen);
        }
        else
        {
            HandleStateChanged(GameManager.GameState.TitleScreen);
        }
    }

    public void ShowMatchHub()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetState(GameManager.GameState.MatchHub);
        }
        else
        {
            HandleStateChanged(GameManager.GameState.MatchHub);
        }
    }

    public void OnTitleLocalPlayClicked()
    {
        ShowMatchHub();
    }

    public void OnTitlePvPClicked()
    {
        TogglePvPPopup();
    }

    public void OnTitleCodexClicked()
    {
        ToggleCodexPopup();
    }

    public void OnBackToTitleScreen()
    {
        ShowTitleScreen();
    }

    public void ReturnToMainMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoHome();
        }
        else
        {
            ShowMatchHub();
        }
    }

    public void OnStartBattleClicked()
    {
        int mapIdx = (maps.Count > 0) ? Mathf.Clamp(selectedMapIndex, 0, Mathf.Min(2, maps.Count - 1)) : 0;
        string mName = (maps.Count > mapIdx) ? maps[mapIdx].mapName : "Sới Mặc Định";
        Debug.Log($"<color=green>[BẮT ĐẦU TRANH TÀI]</color> Sới: {mName} | Player: {selectedPlayerMascot} | Enemy: {selectedEnemyMascot}");
        if (GameManager.Instance != null)
        {
            Sprite chosenMapSprite = (maps.Count > mapIdx) ? maps[mapIdx].mapSprite : null;
            float radius = (maps.Count > mapIdx && maps[mapIdx].arenaRadius > 0) ? maps[mapIdx].arenaRadius : 3.65f;
            GameManager.Instance.StartGameWithSettings(chosenMapSprite, selectedPlayerMascot, selectedEnemyMascot, radius);
        }
        else
        {
            HandleStateChanged(GameManager.GameState.BattlePlaying);
        }
    }

    // --- QUẢN LÝ POPUP ---

    private void EnsurePopupVisibility(GameObject popup)
    {
        if (popup == null) return;
        // Nếu cha của popup đang bị ẩn (khiến activeInHierarchy = false), chuyển popup sang Canvas đang hiển thị
        if (popup.transform.parent != null && !popup.transform.parent.gameObject.activeInHierarchy)
        {
            GameObject targetCanvas = null;
            if (titleCanvas != null && titleCanvas.activeInHierarchy) targetCanvas = titleCanvas;
            else if (menuRootCanvas != null && menuRootCanvas.activeInHierarchy) targetCanvas = menuRootCanvas;
            else if (mobileCanvas != null && mobileCanvas.activeInHierarchy) targetCanvas = mobileCanvas;

            if (targetCanvas != null)
            {
                popup.transform.SetParent(targetCanvas.transform, false);
                popup.transform.SetAsLastSibling();
            }
        }
    }

    private void ClosePopup(GameObject popup)
    {
        if (popup != null)
        {
            popup.SetActive(false);
            var backdrop = popup.transform.parent?.Find("Settings_Backdrop")?.gameObject;
            if (backdrop != null) backdrop.SetActive(false);
        }
    }

    public void CloseAllPopups()
    {
        if (tutorialPopup != null) tutorialPopup.SetActive(false);
        if (codexPopup != null) codexPopup.SetActive(false);
        if (pvpPopup != null) pvpPopup.SetActive(false);
        if (settingsPopup != null)
        {
            settingsPopup.SetActive(false);
            var backdrop = settingsPopup.transform.parent?.Find("Settings_Backdrop")?.gameObject;
            if (backdrop != null) backdrop.SetActive(false);
        }
    }

    public void ToggleTutorialPopup()
    {
        FindAllCanvasAndPopupReferences();
        if (tutorialPopup != null)
        {
            bool next = !tutorialPopup.activeSelf;
            CloseAllPopups();
            if (next)
            {
                EnsurePopupVisibility(tutorialPopup);
                UpdateLocalizedSprites();
                UpdateLocalizedTexts();
                tutorialPopup.SetActive(true);
            }
        }
    }

    public void ToggleCodexPopup()
    {
        FindAllCanvasAndPopupReferences();
        if (codexPopup != null)
        {
            bool next = !codexPopup.activeSelf;
            CloseAllPopups();
            if (next)
            {
                EnsurePopupVisibility(codexPopup);
                UpdateLocalizedSprites();
                UpdateLocalizedTexts();
                codexPopup.SetActive(true);
            }
        }
    }

    public void TogglePvPPopup()
    {
        FindAllCanvasAndPopupReferences();
        if (pvpPopup != null)
        {
            bool next = !pvpPopup.activeSelf;
            CloseAllPopups();
            if (next)
            {
                EnsurePopupVisibility(pvpPopup);
                UpdateLocalizedSprites();
                UpdateLocalizedTexts();
                pvpPopup.SetActive(true);
            }
        }
    }

    public void ToggleSettingsPopup()
    {
        FindAllCanvasAndPopupReferences();
        if (settingsPopup != null)
        {
            bool next = !settingsPopup.activeSelf;
            CloseAllPopups();
            if (next)
            {
                EnsurePopupVisibility(settingsPopup);
                UpdateLocalizedSprites();
                UpdateLocalizedTexts();
                settingsPopup.SetActive(true);
                var backdrop = settingsPopup.transform.parent?.Find("Settings_Backdrop")?.gameObject;
                if (backdrop != null) backdrop.SetActive(true);
                UpdateAudioToggleIcons();
                Debug.Log("<color=green>[POPUP]</color> Đã mở Cài Đặt Trò Chơi (Settings Popup).");
            }
        }
    }

    public void UpdateAudioToggleIcons()
    {
        var bgmSlider = FindSliderInScene("Slider_BGM");
        if (bgmSlider != null)
        {
            var btnMusic = FindButtonInScene("Btn_Music_Toggle");
            if (btnMusic != null && btnMusic.image != null)
            {
#if UNITY_EDITOR
                if (musicOnSprite == null) musicOnSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Popups/btn_music_on.png");
                if (musicOffSprite == null) musicOffSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Popups/btn_music_off.png");
#endif
                Sprite s = (bgmSlider.value > 0.05f && musicOnSprite != null) ? musicOnSprite : musicOffSprite;
                if (s != null) btnMusic.image.sprite = s;
            }
        }

        var sfxSlider = FindSliderInScene("Slider_SFX");
        if (sfxSlider != null)
        {
            var btnSfx = FindButtonInScene("Btn_SFX_Toggle");
            if (btnSfx != null && btnSfx.image != null)
            {
#if UNITY_EDITOR
                if (sfxOnSprite == null) sfxOnSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Popups/btn_sfx_on.png");
                if (sfxOffSprite == null) sfxOffSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Popups/btn_sfx_off.png");
#endif
                Sprite s = (sfxSlider.value > 0.05f && sfxOnSprite != null) ? sfxOnSprite : sfxOffSprite;
                if (s != null) btnSfx.image.sprite = s;
            }
        }
    }

    // --- DUYỆT 3 SỚI ĐẤU (KHÓA MAP 4 ĐANG PHÁT TRIỂN) ---

    public void SelectNextMap()
    {
        int maxMaps = Mathf.Min(3, maps.Count);
        if (maxMaps == 0) return;
        selectedMapIndex = (selectedMapIndex + 1) % maxMaps;
        UpdateMapUI();
    }

    public void SelectPrevMap()
    {
        int maxMaps = Mathf.Min(3, maps.Count);
        if (maxMaps == 0) return;
        selectedMapIndex = (selectedMapIndex - 1 + maxMaps) % maxMaps;
        UpdateMapUI();
    }

    private void UpdateMapUI()
    {
        int maxMaps = Mathf.Min(3, maps.Count);
        if (maxMaps == 0) return;
        selectedMapIndex = Mathf.Clamp(selectedMapIndex, 0, maxMaps - 1);
        var map = maps[selectedMapIndex];

        bool isKorean = LocalizationManager.Instance != null && LocalizationManager.Instance.CurrentLanguage == LocalizationManager.LanguageMode.Korean;
        if (isKorean)
        {
            string[] krMapTitles = { "1. 민속 축제 씨름 경기장", "2. 절경의 영산 (설악산 - 옌뜨)", "3. 화려한 야시장 (명동 - 호이안)" };
            string[] krMapDescs = {
                "한국과 베트남의 우정을 기념하는 전통 모래판 씨름 경기장입니다.",
                "구름과 영봉 사이에서 펼쳐지는 청정 산악 경기장입니다.",
                "등불과 전통 먹거리 장터가 화려하게 빛나는 야간 경기장입니다."
            };
            if (mapTitleText != null && selectedMapIndex < krMapTitles.Length) mapTitleText.text = krMapTitles[selectedMapIndex];
            if (mapDescText != null && selectedMapIndex < krMapDescs.Length) mapDescText.text = krMapDescs[selectedMapIndex];
        }
        else
        {
            if (mapTitleText != null) mapTitleText.text = map.mapName;
            if (mapDescText != null) mapDescText.text = map.description;
        }

        if (mapPreviewImage != null && map.thumbnailSprite != null)
        {
            mapPreviewImage.sprite = map.thumbnailSprite;
        }
    }

    // --- DUYỆT 5 LINH VẬT VIỆT NAM ---

    public void NextPlayerMascot()
    {
        playerMascotIndex = (playerMascotIndex + 1) % vnMascotList.Length;
        selectedPlayerMascot = vnMascotList[playerMascotIndex];
        UpdateMascotUI();
    }

    public void PrevPlayerMascot()
    {
        playerMascotIndex = (playerMascotIndex - 1 + vnMascotList.Length) % vnMascotList.Length;
        selectedPlayerMascot = vnMascotList[playerMascotIndex];
        UpdateMascotUI();
    }

    // --- DUYỆT 5 LINH VẬT HÀN QUỐC ---

    public void NextEnemyMascot()
    {
        enemyMascotIndex = (enemyMascotIndex + 1) % krMascotList.Length;
        selectedEnemyMascot = krMascotList[enemyMascotIndex];
        UpdateMascotUI();
    }

    public void PrevEnemyMascot()
    {
        enemyMascotIndex = (enemyMascotIndex - 1 + krMascotList.Length) % krMascotList.Length;
        selectedEnemyMascot = krMascotList[enemyMascotIndex];
        UpdateMascotUI();
    }

    private void UpdateMascotUI()
    {
        bool isKorean = LocalizationManager.Instance != null && LocalizationManager.Instance.CurrentLanguage == LocalizationManager.LanguageMode.Korean;
        if (isKorean)
        {
            string[] krNames = {
                "금우 (황금 물소)",
                "아기용 (비룡)",
                "사자춤 (길조)",
                "대나무 (방벽)",
                "락새 (고대 신조)"
            };
            string[] krRoles = {
                "탱커 - 강철 물소 • 스킬: 금우진지",
                "스피드 - 승천 돌진 • 스킬: 무룡등운",
                "방해 - 축제 폭죽 • 스킬: 희기폭화",
                "수비 - 불굴 방벽 • 스킬: 죽림결계",
                "민첩 - 동손 폭풍 • 스킬: 풍파동손"
            };

            if (playerMascotText != null) playerMascotText.text = krNames[playerMascotIndex];
            if (playerRoleText != null) playerRoleText.text = krRoles[playerMascotIndex];

            string[] enemyNames = {
                "백호 (신수)",
                "해치 (영수)",
                "도깨비 (요괴)",
                "단군 곰 (신화)",
                "삼족오 (태양새)"
            };
            string[] enemyRoles = {
                "브루저 - 호랑이 포효 • 스킬: 호효산하",
                "반격 - 퇴마 영기 • 스킬: 영기제사",
                "방해 - 도깨비불 방망이 • 스킬: 마화가시",
                "헤비 탱커 - 지진 강타 • 스킬: 신화진동",
                "스피드 - 열화 질주 • 스킬: 열화빙벽"
            };

            if (enemyMascotText != null) enemyMascotText.text = enemyNames[enemyMascotIndex];
            if (enemyRoleText != null) enemyRoleText.text = enemyRoles[enemyMascotIndex];
        }
        else
        {
            string[] vnNames = {
                "Trâu Vàng (Kim Ngưu)",
                "Bé Long (Rồng Thời Lý)",
                "Lân Sư May Mắn",
                "Bé Tre (Lũy Tre)",
                "Chim Lạc Cổ Đại"
            };
            string[] vnRoles = {
                "Đỡ đòn - Trâu Sắt • Chiêu: Kim Ngưu Trấn Địa",
                "Tốc độ - Vũ Long • Chiêu: Vũ Long Đằng Vân",
                "Quấy rối - Pháo Hoa • Chiêu: Hỷ Khí Pháo Hoa",
                "Phòng thủ - Lũy Tre • Chiêu: Lũy Tre Bất Khả",
                "Cơ động - Phong Ba • Chiêu: Phong Ba Đông Sơn"
            };

            if (playerMascotText != null) playerMascotText.text = vnNames[playerMascotIndex];
            if (playerRoleText != null) playerRoleText.text = vnRoles[playerMascotIndex];

            string[] enemyNames = {
                "Bạch Hổ Baekho",
                "Kỳ Lân Haechi",
                "Yêu Tinh Dokkaebi",
                "Gấu Thần Dangoon",
                "Samjoko (Chim 3 Chân)"
            };
            string[] enemyRoles = {
                "Đấu sĩ - Sức Mạnh • Chiêu: Hổ Khiếu Sơn Hà",
                "Phản đòn - Linh Khí • Chiêu: Linh Khí Trừ Tà",
                "Phép thuật - Chùy Gai • Chiêu: Bão Gai Ma Trời",
                "Siêu đỡ đòn - Địa Chấn • Chiêu: Chấn Động Thần Thoại",
                "Lướt nhanh - Liệt Hỏa • Chiêu: Liệt Hỏa Băng Vút"
            };

            if (enemyMascotText != null) enemyMascotText.text = enemyNames[enemyMascotIndex];
            if (enemyRoleText != null) enemyRoleText.text = enemyRoles[enemyMascotIndex];
        }

        if (playerAvatarImage != null && vnAvatars != null && playerMascotIndex < vnAvatars.Length)
        {
            playerAvatarImage.sprite = vnAvatars[playerMascotIndex];
        }
        if (enemyAvatarImage != null && krAvatars != null && enemyMascotIndex < krAvatars.Length)
        {
            enemyAvatarImage.sprite = krAvatars[enemyMascotIndex];
        }
    }

    private void LoadDefaultMapsIfEmpty()
    {
        if (maps.Count == 0)
        {
#if UNITY_EDITOR
            maps.Add(new MapData
            {
                mapName = "1. Sới Đấu Lễ Hội Dân Gian",
                description = "Sới đấu đất nện kết hợp sàn cát Ssireum hữu nghị giữa hội làng Việt - Hàn.",
                mapSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Maps/san1-lang.png"),
                thumbnailSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Thumbnails/thumb_san1-lang.png"),
                arenaRadius = 3.65f
            });

            maps.Add(new MapData
            {
                mapName = "2. Sơn Cảnh Kỳ Quan (Yên Tử - Seoraksan)",
                description = "Sới đấu bồng bềnh giữa mây ngàn và đỉnh núi thiêng liêng, gió lộng thử thách bản lĩnh.",
                mapSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Maps/san-yentu.png"),
                thumbnailSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Thumbnails/thumb_san-yentu.png"),
                arenaRadius = 3.65f
            });

            maps.Add(new MapData
            {
                mapName = "3. Phố Đêm Rực Rỡ (Hội An - Myeongdong)",
                description = "Lung linh ánh đèn lồng phố đêm và các sạp ẩm thực truyền thống rực rỡ sắc màu.",
                mapSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Maps/dan-dempho.png"),
                thumbnailSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Thumbnails/thumb_dan-dempho.png"),
                arenaRadius = 3.25f
            });

            maps.Add(new MapData
            {
                mapName = "4. Hoàng Cung Cổ Kính (Tạm khóa)",
                description = "Cố đô trầm mặc cổ kính trong sương huyền bí. Đang thi công, tạm sử dụng sới Lễ Hội.",
                mapSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Maps/san1-lang.png"),
                thumbnailSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Thumbnails/thumb_san1-lang.png"),
                arenaRadius = 3.65f
            });
#endif
        }
    }

    private void LoadDefaultAvatarsIfEmpty()
    {
#if UNITY_EDITOR
        if (vnAvatars == null || vnAvatars.Length < 5)
        {
            vnAvatars = new Sprite[] {
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Avatars/avatar_trau_vang.png"),
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Avatars/avatar_rong_vang.png"),
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Avatars/avatar_lan_su.png"),
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Avatars/avatar_be_tre.png"),
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Avatars/avatar_chim_lac.png")
            };
        }

        if (krAvatars == null || krAvatars.Length < 5)
        {
            krAvatars = new Sprite[] {
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Avatars/avatar_ho_baekho.png"),
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Avatars/avatar_haechi.png"),
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Avatars/avatar_dokkaebi.png"),
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Avatars/avatar_gau_dangoon.png"),
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Avatars/avatar_samjoko.png")
            };
        }
#endif
    }

    public void OnLanguageButtonClicked()
    {
        LocalizationManager.Instance?.ToggleNextLanguage();
        UpdateLocalizedSprites();
        UpdateLocalizedTexts();
    }

    private void OnLangChanged(LocalizationManager.LanguageMode mode)
    {
        UpdateLocalizedSprites();
        UpdateLocalizedTexts();
    }

        public void UpdateLocalizedTexts()
    {
        if (LocalizationManager.Instance == null) return;
        var loc = LocalizationManager.Instance;
        bool isKorean = loc.CurrentLanguage == LocalizationManager.LanguageMode.Korean;

        var langText = FindSceneObject("Text_Value_Lang")?.GetComponent<TMPro.TextMeshProUGUI>();
        if (langText != null)
        {
            langText.text = loc.GetLanguageDisplayName();
            loc.ApplyFont(langText);
        }

        // 1. XÓA HOÀN TOÀN CHỮ TRÙNG ĐÈ LÊN BANNER CHẠM TRỔ 3D
        var titleText = FindSceneObject("Text_Title")?.GetComponent<TMPro.TextMeshProUGUI>();
        if (titleText != null) titleText.text = "";

        void HideTitleIn(string parentName, string childName)
        {
            var p = FindSceneObject(parentName);
            if (p != null)
            {
                var tr = p.transform.Find(childName);
                if (tr != null)
                {
                    var tmp = tr.GetComponent<TMPro.TextMeshProUGUI>();
                    if (tmp != null) tmp.text = "";
                    var uText = tr.GetComponent<UnityEngine.UI.Text>();
                    if (uText != null) uText.text = "";
                    tr.gameObject.SetActive(false);
                }
            }
        }

        void ClearTextIn(string parentName, string childName)
        {
            var p = FindSceneObject(parentName);
            if (p != null)
            {
                var tmp = p.transform.Find(childName)?.GetComponent<TMPro.TextMeshProUGUI>();
                if (tmp != null) tmp.text = "";
                var uText = p.transform.Find(childName)?.GetComponent<UnityEngine.UI.Text>();
                if (uText != null) uText.text = "";
            }
        }

        HideTitleIn("Settings_Popup", "Text_Title");
        HideTitleIn("Tutorial_Popup", "Text_Title");
        HideTitleIn("Codex_Popup", "Text_Title");
        HideTitleIn("PvP_Popup", "Text_Title");
        HideTitleIn("Panel_CharSelect", "Label_Char");
        HideTitleIn("Panel_CharSelect", "Text_Title");
        ClearTextIn("Btn_NextPlayerText", "Text");
        ClearTextIn("Btn_NextEnemyText", "Text");

        // Xóa chữ con trên 4 nút Title Screen vì chữ đã được khắc chạm nổi nghệ thuật trên mặt gỗ
        ClearTextIn("Btn_Title_LocalPlay", "Text");
        ClearTextIn("Btn_Title_PvP", "Text");
        ClearTextIn("Btn_Title_Codex", "Text");
        ClearTextIn("Btn_Title_Tutorial", "Text");

        // Xóa chữ trên các nút đóng popup
        ClearTextIn("Btn_CloseSettings", "Text");
        ClearTextIn("Btn_CloseTutorial", "Text");
        ClearTextIn("Btn_CloseCodex", "Text");
        ClearTextIn("Btn_ClosePvP", "Text");

        void FixLabel(TMPro.TextMeshProUGUI label, string txt)
        {
            if (label == null) return;
            label.text = txt;
            loc.ApplyFont(label);
            label.enableAutoSizing = true;
            label.fontSizeMin = 13;
            label.fontSizeMax = 18;
            label.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        }

        var bgmLabel = FindSceneObject("Text_Label_BGM")?.GetComponent<TMPro.TextMeshProUGUI>();
        FixLabel(bgmLabel, loc.TextBGM);

        var sfxLabel = FindSceneObject("Text_Label_SFX")?.GetComponent<TMPro.TextMeshProUGUI>();
        FixLabel(sfxLabel, loc.TextSFX);

        var crowdLabel = FindSceneObject("Text_Label_Crowd")?.GetComponent<TMPro.TextMeshProUGUI>();
        FixLabel(crowdLabel, loc.TextCrowd);

        var langLabel = FindSceneObject("Text_Label_Lang")?.GetComponent<TMPro.TextMeshProUGUI>();
        FixLabel(langLabel, loc.TextLanguage);

        // Map Select Titles
        var labelMap = FindSceneObject("Label_Map")?.GetComponent<UnityEngine.UI.Text>();
        if (labelMap != null) { labelMap.text = loc.TextMapHeader; loc.ApplyFont(labelMap); }
        var labelMapTmp = FindSceneObject("Label_Map")?.GetComponent<TMPro.TextMeshProUGUI>();
        if (labelMapTmp != null) { labelMapTmp.text = loc.TextMapHeader; loc.ApplyFont(labelMapTmp); }

        var textMapTitle = FindSceneObject("Text_MapTitle")?.GetComponent<TMPro.TextMeshProUGUI>();
        if (textMapTitle != null) { textMapTitle.text = loc.TextMapSelectTitle; loc.ApplyFont(textMapTitle); }

        // Dynamic Popup Texts (Tutorial & PvP)
        var tutBody = FindSceneObject("Tutorial_Popup")?.transform.Find("Text_Body")?.GetComponent<UnityEngine.UI.Text>();
        if (tutBody != null)
        {
            loc.ApplyFont(tutBody);
            if (isKorean)
            {
                tutBody.text = @"<color=#9C3500><b>🎯 승리 목표</b></color>
경기장 밖으로 상대를 <b>3번 밀어내면</b> 우정의 황금컵 쟁탈 (5판 3선승제)!

<color=#9C3500><b>🕹️ 슬링샷 조작법</b></color>
영수를 터치 ➔ 뒤로 당겨 기력 충전 ➔ 놓아서 강력하게 돌진!

<color=#9C3500><b>⭐ 베트남 5대 영수</b></color>
금우 (강철 돌진) • 아기용 (승천) • 사자춤 (폭죽) • 대나무 (방벽) • 락새 (풍파)

<color=#9C3500><b>🐯 한국 5대 신수</b></color>
백호 (신수 포효) • 해치 (영기 반격) • 도깨비 (가시 방망이) • 단군 곰 (지진) • 삼족오 (열화)

<color=#9C3500><b>🎁 문화 아이템 & 함정</b></color>
<b>반쭝, 떡볶이, 삿갓</b>을 먹고 버프 획득 • 감속 <b>진흙 웅덩이</b>를 조심하세요!";
            }
            else
            {
                tutBody.text = @"<color=#9C3500><b>🎯 MỤC TIÊU CHIẾN ĐẤU</b></color>
Đẩy đối thủ văng khỏi sới đấu <b>3 lần</b> để giành Cúp Vàng Hữu Nghị (Best of 5)!

<color=#9C3500><b>🕹️ CÁCH ĐIỀU KHIỂN (SLINGSHOT)</b></color>
Chạm linh vật ➔ Kéo lùi lấy đà ➔ Thả tay để húc bay đối phương!

<color=#9C3500><b>⭐ 5 LINH VẬT VIỆT NAM</b></color>
Trâu Vàng (Húc Thép) • Bé Long (Vũ Long) • Lân Sư (Pháo Nổ) • Bé Tre (Lũy Bền) • Chim Lạc (Phong Ba)

<color=#9C3500><b>🐯 5 LINH VẬT HÀN QUỐC</b></color>
Bạch Hổ (Móng Thần) • Haechi (Trừ Tà) • Dokkaebi (Chùy Gai) • Gấu Dangoon (Địa Chấn) • Samjoko (Liệt Hỏa)

<color=#9C3500><b>🎁 VẬT PHẨM & CẠM BẪY</b></color>
Ăn <b>Bánh Chưng, Tteokbokki, Nón Lá</b> tăng lực cực mạnh • Né <b>Vũng Bùn Lầy</b>!";
            }
            tutBody.color = new Color(0.18f, 0.09f, 0.03f, 1f); // Rich dark espresso
            tutBody.fontSize = 17;
            tutBody.lineSpacing = 1.15f;
        }

        var pvpBody = FindSceneObject("PvP_Popup")?.transform.Find("Text_Body")?.GetComponent<UnityEngine.UI.Text>();
        if (pvpBody != null)
        {
            loc.ApplyFont(pvpBody);
            if (isKorean)
            {
                pvpBody.text = @"<color=#FFD700>친선 교류 룸 코드 (ROOM ID):</color>

<size=28><b>#KGW2026-VN-KR</b></size>

로컬 네트워크 (LAN / Wi-Fi) 대기 중!
동일한 Wi-Fi 환경에서 즉시 1대1 대전이 가능합니다.";
            }
            else
            {
                pvpBody.text = @"<color=#FFD700>MÃ PHÒNG GIAO LƯU (ROOM ID):</color>

<size=28><b>#KGW2026-VN-KR</b></size>

Đang sẵn sàng kết nối qua Mạng Cục Bộ (LAN / Wi-Fi)!
Hai thiết bị kết nối chung Wi-Fi có thể thách đấu tức thì.";
            }
            pvpBody.color = new Color(0.95f, 0.90f, 0.80f, 1f);
        }

        if (mapTitleText != null) loc.ApplyFont(mapTitleText);
        if (mapDescText != null) loc.ApplyFont(mapDescText);
        if (playerMascotText != null) loc.ApplyFont(playerMascotText);
        if (playerRoleText != null) loc.ApplyFont(playerRoleText);
        if (enemyMascotText != null) loc.ApplyFont(enemyMascotText);
        if (enemyRoleText != null) loc.ApplyFont(enemyRoleText);

        UpdateMapUI();
        UpdateMascotUI();
        UpdateLocalizedSprites();
    }

    public static Sprite LoadSpriteSafe(string resourceSubPath)
    {
        Sprite sp = Resources.Load<Sprite>(resourceSubPath);
        if (sp == null)
        {
            var all = Resources.LoadAll<Sprite>(resourceSubPath);
            if (all != null && all.Length > 0) sp = all[0];
        }
#if UNITY_EDITOR
        if (sp == null)
        {
            string p1 = "Assets/Resources/" + resourceSubPath + ".png";
            string p2 = "Assets/Sprites/" + resourceSubPath + ".png";
            foreach (var path in new[] { p1, p2 })
            {
                sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sp != null) break;
                var subAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var a in subAssets)
                {
                    if (a is Sprite spr) { sp = spr; break; }
                }
                if (sp != null) break;
            }
        }
#endif
        return sp;
    }

    public void UpdateLocalizedSprites()
    {
        if (LocalizationManager.Instance == null) return;
        bool isKorean = LocalizationManager.Instance.CurrentLanguage == LocalizationManager.LanguageMode.Korean;
        string tag = isKorean ? "kr" : "vn";

        void SwapImage(string objName, string resourceSubPath)
        {
            var obj = FindSceneObject(objName);
            if (obj != null)
            {
                var img = obj.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    Sprite sp = LoadSpriteSafe(resourceSubPath);
                    if (sp != null) img.sprite = sp;
                }
            }
        }

        // 1. Title Screen 4 Buttons
        SwapImage("Btn_Title_LocalPlay", $"UI/Title/btn_title_local_{tag}");
        SwapImage("Btn_Title_PvP", $"UI/Title/btn_title_pvp_{tag}");
        SwapImage("Btn_Title_Codex", $"UI/Title/btn_title_codex_{tag}");
        SwapImage("Btn_Title_Tutorial", $"UI/Title/btn_title_guide_{tag}");
        SwapImage("Btn_Title_Settings", "UI/Title/btn_title_settings");
        SwapImage("Btn_Title_Sound", "UI/Title/btn_title_sound");

        // 2. MatchHub Cards & VS Badge - Giữ Card_Player và Card_Enemy trong suốt để lộ trống đồng & Taegeuk trên Panel_CharSelect
        SwapImage("Panel_CharSelect", $"UI/MatchHub/panel_char_select_{tag}");
        var cardP = FindSceneObject("Card_Player")?.GetComponent<UnityEngine.UI.Image>();
        if (cardP != null) cardP.color = new Color(1f, 1f, 1f, 0f);
        var cardE = FindSceneObject("Card_Enemy")?.GetComponent<UnityEngine.UI.Image>();
        if (cardE != null) cardE.color = new Color(1f, 1f, 1f, 0f);

        SwapImage("Icon_VS", "UI/Battle/badge_vs_fire");
        SwapImage("Image_PlayerAvatarFrame", "UI/Battle/avatar_frame_gold_round");
        SwapImage("Image_EnemyAvatarFrame", "UI/Battle/avatar_frame_gold_round");

        // 3. MatchHub Action Buttons & Signs
        SwapImage("Btn_StartBattle", $"UI/MatchHub/btn_vao_tran_fire_{tag}");
        SwapImage("Btn_BackToTitle", $"UI/MatchHub/btn_quay_lai_{tag}");
        SwapImage("Btn_Back", $"UI/MatchHub/btn_quay_lai_{tag}");
        SwapImage("Btn_MatchHub_Settings", $"UI/MatchHub/sign_cai_dat_{tag}");
        SwapImage("Btn_Sound", $"UI/MatchHub/sign_cai_dat_{tag}");
        SwapImage("Btn_Tutorial", $"UI/MatchHub/sign_huong_dan_{tag}");

        // 4. MatchHub Character Select Board
        SwapImage("Panel_CharSelect", $"UI/MatchHub/panel_char_select_{tag}");

        // 5. Settings Popup (Chuyển đổi hoàn toàn CÀI ĐẶT <-> 설정)
        SwapImage("Settings_Popup", $"UI/Popups/frame_settings_{tag}");
        SwapImage("Panel_Settings", $"UI/Popups/frame_settings_{tag}");
        if (settingsPopup != null)
        {
            var img = settingsPopup.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                Sprite sp = LoadSpriteSafe($"UI/Popups/frame_settings_{tag}");
                if (sp != null) img.sprite = sp;
            }
        }
        SwapImage("Btn_CloseSettings", $"UI/MatchHub/btn_dong_{tag}");
        SwapImage("Btn_Close", $"UI/MatchHub/btn_dong_{tag}");
        SwapImage("Btn_Close_X", "UI/Popups/btn_settings_close_x");

        // 6. Popups: Tutorial (Khung trúc hoa đào VN / Khung triều đình KR), Codex, PvP
        string tutFrame = isKorean ? "UI/Popups/panel_codex_kr" : "UI/Popups/panel_tutorial_bamboo_vn";
        SwapImage("Tutorial_Popup", tutFrame);
        if (tutorialPopup != null)
        {
            var img = tutorialPopup.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                Sprite sp = Resources.Load<Sprite>(tutFrame);
#if UNITY_EDITOR
                if (sp == null) sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/{tutFrame}.png");
#endif
                if (sp != null) img.sprite = sp;
            }
        }

        SwapImage("Codex_Popup", $"UI/Popups/panel_codex_{tag}");
        if (codexPopup != null)
        {
            var img = codexPopup.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                Sprite sp = Resources.Load<Sprite>($"UI/Popups/panel_codex_{tag}");
#if UNITY_EDITOR
                if (sp == null) sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/UI/Popups/panel_codex_{tag}.png");
#endif
                if (sp != null) img.sprite = sp;
            }
        }

        SwapImage("PvP_Popup", $"UI/Popups/frame_pvp_{tag}");
        if (pvpPopup != null)
        {
            var img = pvpPopup.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                Sprite sp = Resources.Load<Sprite>($"UI/Popups/frame_pvp_{tag}");
#if UNITY_EDITOR
                if (sp == null) sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/UI/Popups/frame_pvp_{tag}.png");
#endif
                if (sp != null) img.sprite = sp;
            }
        }

        SwapImage("Btn_CloseTutorial", $"UI/MatchHub/btn_dong_{tag}");
        SwapImage("Btn_CloseCodex", $"UI/MatchHub/btn_dong_{tag}");
        SwapImage("Btn_ClosePvP", $"UI/MatchHub/btn_dong_{tag}");
        SwapImage("Btn_PvP_Create", $"UI/Popups/btn_pvp_create_{tag}");
        SwapImage("Btn_PvP_Join", $"UI/Popups/btn_pvp_join_{tag}");

        // 7. In-Game Battle HUD
        SwapImage("Btn_Skill", $"UI/Battle/btn_skill_fist_{tag}");
        SwapImage("Btn_Pause", $"UI/Battle/btn_battle_pause_{tag}");

        // 8. Victory / Defeat Popup Buttons
        SwapImage("Btn_Replay", $"UI/Popups/btn_result_replay_{tag}");
        SwapImage("Btn_Home", $"UI/Popups/btn_result_close_{tag}");
    }
}
