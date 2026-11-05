using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Quản lý Vòng đời & Trạng thái Trận đấu (Game State Machine):
///  0. TitleScreen: Màn hình khởi động - gameplay đóng băng hoàn toàn.
///  1. MatchHub: Sảnh chọn sới đấu & tướng - gameplay đóng băng hoàn toàn.
///  2. BattlePlaying: Trong trận đấu - physics, AI Bot & Item Spawner hoạt động.
///  3. RoundResetting: Tạm dừng sau Ring-Out, cộng điểm và đặt lại vị trí (dùng WaitForSecondsRealtime).
///  4. GameOver: Kết thúc trận đấu (Best of 5 chạm mốc 3 điểm hoặc hết 60s).
///  5. Paused: Tạm dừng trận đấu từ nút Pause trên Mobile HUD (lưu previousState).
/// Dự án: Mythic Bumper (KGW2026).
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        TitleScreen = 0,
        MatchHub = 1,
        BattlePlaying = 2,

        Playing = 2,

        RoundResetting = 3,
        GameOver = 4,
        Paused = 5
    }

    [Header("--- QUẢN LÝ TỈ SỐ (BEST OF 5) ---")]
    [Tooltip("Số lần bị đẩy ra ngoài để quyết định thắng trận (3 điểm)")]
    [SerializeField] private int maxScoreToWin = 3;
    private int playerScore = 0; // Điểm của VN (đẩy được đối thủ)
    private int enemyScore = 0;  // Điểm của HQ (đẩy được player)

    [Header("--- THỜI GIAN TRẬN ĐẤU ---")]
    [Tooltip("Tổng thời gian trận đấu (giây)")]
    [SerializeField] private float matchTime = 60f;
    private float currentTimer;
    private bool isFeverTime = false;

    [Header("--- VỊ TRÍ XUẤT PHÁT CỦA 2 BÊN ---")]
    [SerializeField] private Vector3 playerStartPos = new Vector3(-2.2f, 0, 0);
    [SerializeField] private Vector3 enemyStartPos = new Vector3(2.2f, 0, 0);

    [Header("--- THAM CHIẾU GIAO DIỆN (UI) ---")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TMP_Text victoryMessageText;
    [SerializeField] private TMPro.TextMeshProUGUI victoryTmpText;
    [SerializeField] private Image victoryBannerImage;
    [SerializeField] private Sprite victoryBannerSprite;
    [SerializeField] private Sprite defeatBannerSprite;

    [Header("--- NÚT BẤM MOBILE HUD ---")]
    [SerializeField] private Button btnPause;
    [SerializeField] private Button btnHome;
    [SerializeField] private Button btnRestart;
    [SerializeField] private Button btnSound;
    [SerializeField] private Button btnReplay;
    [SerializeField] private Button btnVictoryHome;

    [Header("--- DANH SÁCH BẢN ĐỒ THI ĐẤU (MAP BACKGROUNDS) ---")]
    [SerializeField] private Sprite[] mapBackgroundSprites = new Sprite[4];

    public Sprite[] MapBackgroundSprites => mapBackgroundSprites;

    public Sprite GetMapSprite(int index)
    {
        if (mapBackgroundSprites == null || mapBackgroundSprites.Length == 0) return null;
        if (index < 0 || index >= mapBackgroundSprites.Length) index = 0;
        return mapBackgroundSprites[index] != null ? mapBackgroundSprites[index] : mapBackgroundSprites[0];
    }

    private PlayerController player;
    private AIController enemy;

    // Biến trạng thái State Machine chuẩn hóa
    private GameState currentState = GameState.TitleScreen;
    private GameState previousState = GameState.TitleScreen;

    public GameState CurrentState => currentState;
    public bool IsFeverTime => isFeverTime;

    // Sự kiện thông báo thay đổi trạng thái cho MenuManager và UIStateController
    public event System.Action<GameState> OnStateChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        currentTimer = matchTime;

        // Đảm bảo MenuManager luôn tồn tại trên GameObject luôn Active (GameManager)
        if (MenuManager.Instance == null && GetComponent<MenuManager>() == null)
        {
            gameObject.AddComponent<MenuManager>();
        }
    }

    private void Start()
    {
        player = Object.FindAnyObjectByType<PlayerController>();
        enemy = Object.FindAnyObjectByType<AIController>();

        // Tự động tìm Text_Score nếu chưa gán
        if (scoreText == null)
        {
            var txtObj = MenuManager.FindSceneObject("Text_Score");
            if (txtObj != null) scoreText = txtObj.GetComponent<TMP_Text>();
        }

        // Tự động tìm và liên kết Popup Chiến Thắng
        if (victoryPanel == null)
        {
            var p = MenuManager.FindSceneObject("VictoryDefeat_Popup");
            if (p != null)
            {
                victoryPanel = p;
                victoryBannerImage = p.GetComponent<Image>();
                var txt = p.transform.Find("Text_VictoryMessage");
                if (txt != null)
                {
                    victoryTmpText = txt.GetComponent<TMPro.TextMeshProUGUI>();
                    victoryMessageText = txt.GetComponent<TMP_Text>();
                }
                victoryPanel.SetActive(false);
            }
        }

#if UNITY_EDITOR
        if (victoryBannerSprite == null)
        {
            victoryBannerSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/banner_victory.png");
        }
        if (defeatBannerSprite == null)
        {
            defeatBannerSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/banner_defeat.png");
        }
        if (mapBackgroundSprites == null || mapBackgroundSprites.Length < 4 || mapBackgroundSprites[0] == null)
        {
            mapBackgroundSprites = new Sprite[4];
            mapBackgroundSprites[0] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Maps/san1-lang.png");
            mapBackgroundSprites[1] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Maps/san-yentu.png");
            mapBackgroundSprites[2] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Maps/dan-dempho.png");
            mapBackgroundSprites[3] = mapBackgroundSprites[0]; // Dự phòng map 4 trỏ về map 1
        }
#endif

        WireButtons();
        UpdateScoreUI();

        // Khởi động ở trạng thái TitleScreen và đóng băng gameplay ngay từ đầu
        SetState(GameState.TitleScreen);
    }

    /// <summary>
    /// Chuyển đổi trạng thái Game State Machine trung tâm
    /// </summary>
    public void SetState(GameState newState)
    {
        if (currentState == newState) return;

        previousState = currentState;
        currentState = newState;

        // Đóng băng hoặc mở khóa vật lý và logic gameplay
        bool isPlaying = (currentState == GameState.BattlePlaying);
        FreezeGameplay(!isPlaying);

        OnStateChanged?.Invoke(currentState);
        Debug.Log($"<color=yellow>[GAME STATE]</color> Chuyển sang: {currentState} (Trước đó: {previousState})");
    }

    /// <summary>
    /// Đóng băng hoặc giải phóng vật lý, AI bot và spawner một cách triệt để
    /// </summary>
    public void FreezeGameplay(bool freeze)
    {
        if (player == null) player = Object.FindAnyObjectByType<PlayerController>();
        if (enemy == null) enemy = Object.FindAnyObjectByType<AIController>();
        var spawner = Object.FindAnyObjectByType<ItemSpawner>();

        if (player != null)
        {
            player.enabled = !freeze;
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                if (freeze)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
                rb.simulated = !freeze;
            }
        }

        if (enemy != null)
        {
            enemy.enabled = !freeze;
            if (freeze) enemy.StopAllCoroutines();
            var rb = enemy.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                if (freeze)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
                rb.simulated = !freeze;
            }
        }

        if (spawner != null)
        {
            spawner.enabled = !freeze;
            if (freeze) spawner.StopAllCoroutines();
        }
    }

    /// <summary>
    /// Gom toàn bộ tác vụ thiết lập lại thông số trận đấu vào 1 hàm duy nhất
    /// </summary>
    public void ResetMatch()
    {
        playerScore = 0;
        enemyScore = 0;
        currentTimer = matchTime;
        isFeverTime = false;
        ResetPositions();
        UpdateScoreUI();

        if (victoryPanel != null) victoryPanel.SetActive(false);

        var spawner = Object.FindAnyObjectByType<ItemSpawner>();
        if (spawner != null) spawner.ClearAllItems();
    }

    public void WireButtons()
    {
        if (btnPause == null) btnPause = MenuManager.FindButtonInScene("Btn_Pause");
        if (btnHome == null) btnHome = MenuManager.FindButtonInScene("Btn_Home");
        if (btnRestart == null) btnRestart = MenuManager.FindButtonInScene("Btn_Restart");
        if (btnSound == null) btnSound = MenuManager.FindButtonInScene("Btn_Sound");
        if (btnReplay == null) btnReplay = MenuManager.FindButtonInScene("Btn_Replay");
        if (btnVictoryHome == null) btnVictoryHome = MenuManager.FindButtonInScene("Btn_VictoryHome");

        if (btnPause != null)
        {
            btnPause.onClick.RemoveAllListeners();
            btnPause.onClick.AddListener(TogglePause);
        }
        if (btnHome != null)
        {
            btnHome.onClick.RemoveAllListeners();
            btnHome.onClick.AddListener(GoHome);
        }
        if (btnRestart != null)
        {
            btnRestart.onClick.RemoveAllListeners();
            btnRestart.onClick.AddListener(RestartGame);
        }
        if (btnSound != null)
        {
            btnSound.onClick.RemoveAllListeners();
            btnSound.onClick.AddListener(ToggleSound);
        }
        if (btnReplay != null)
        {
            btnReplay.onClick.RemoveAllListeners();
            btnReplay.onClick.AddListener(RestartGame);
        }
        if (btnVictoryHome != null)
        {
            btnVictoryHome.onClick.RemoveAllListeners();
            btnVictoryHome.onClick.AddListener(GoHome);
        }
    }

    private void Update()
    {
        // CHỈ đếm ngược thời gian và cập nhật UI khi đang ở đúng trạng thái BattlePlaying
        if (currentState != GameState.BattlePlaying) return;

        if (currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
            if (currentTimer <= 20f && !isFeverTime)
            {
                TriggerFeverTime();
            }

            if (currentTimer <= 0)
            {
                currentTimer = 0;
                EndMatch();
            }
        }

        UpdateScoreUI();
    }

    private void TriggerFeverTime()
    {
        isFeverTime = true;
        Debug.Log("<color=yellow>[FEVER TIME]</color> Nhịp trống rộn rã! 20 giây vàng cuối cùng!");
    }

    /// <summary>
    /// Xử lý sự kiện linh vật rơi khỏi sới đấu (Ring-Out)
    /// </summary>
    public void OnMascotRingOut(bool isPlayer)
    {
        // Guard 1: Chặn đứng khi không ở trong trạng thái thi đấu (tránh cộng điểm đôi hoặc rác ngoài hiệp)
        if (currentState != GameState.BattlePlaying) return;

        // Chuyển State sang RoundResetting ngay lập tức để khóa các va chạm tiếp theo
        SetState(GameState.RoundResetting);

        if (isPlayer)
        {
            enemyScore++;
            Debug.Log($"<color=red>Hàn Quốc ghi điểm!</color> Tỉ số: VN {playerScore} - {enemyScore} HQ");
        }
        else
        {
            playerScore++;
            Debug.Log($"<color=green>Việt Nam ghi điểm!</color> Tỉ số: VN {playerScore} - {enemyScore} HQ");
        }

        UpdateScoreUI();

        // Kiểm tra điều kiện thắng Best of 5 (chạm mốc 3 điểm)
        if (playerScore >= maxScoreToWin || enemyScore >= maxScoreToWin)
        {
            EndMatch();
        }
        else
        {
            StartCoroutine(ResetRoundRoutine());
        }
    }

    public void ResetPositions()
    {
        if (player == null) player = Object.FindAnyObjectByType<PlayerController>();
        if (enemy == null) enemy = Object.FindAnyObjectByType<AIController>();

        if (player != null)
        {
            player.transform.position = playerStartPos;
            player.transform.rotation = Quaternion.identity;
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        if (enemy != null)
        {
            enemy.transform.position = enemyStartPos;
            enemy.transform.rotation = Quaternion.identity;
            var rb = enemy.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
    }

    private IEnumerator ResetRoundRoutine()
    {
        // Dùng WaitForSecondsRealtime để nếu bị tạm dừng vẫn hoạt động chính xác
        yield return new WaitForSecondsRealtime(1.2f);

        if (currentState == GameState.RoundResetting)
        {
            ResetPositions();
            SetState(GameState.BattlePlaying);
        }
    }

    /// <summary>
    /// Kết thúc trận đấu, kiểm tra Thắng - Thua - Hòa và hiển thị Popup Vinh Danh
    /// </summary>
    private void EndMatch()
    {
        SetState(GameState.GameOver);
        string message = "";
        bool isPlayerWin = playerScore > enemyScore;
        bool isEnemyWin = enemyScore > playerScore;

        bool isKorean = LocalizationManager.Instance != null && LocalizationManager.Instance.CurrentLanguage == LocalizationManager.LanguageMode.Korean;
        string tag = isKorean ? "kr" : "vn";

        Sprite vicSprite = Resources.Load<Sprite>($"UI/Popups/banner_victory_{tag}");
        Sprite defSprite = Resources.Load<Sprite>($"UI/Popups/banner_defeat_{tag}");
#if UNITY_EDITOR
        if (vicSprite == null) vicSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/UI/Popups/banner_victory_{tag}.png");
        if (defSprite == null) defSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/UI/Popups/banner_defeat_{tag}.png");
#endif
        if (vicSprite == null) vicSprite = victoryBannerSprite;
        if (defSprite == null) defSprite = defeatBannerSprite;

        if (isPlayerWin)
        {
            message = isKorean ? "<size=32><color=#FFD700><b>우정의 승리!</b></color></size>\n\n<size=20>베트남 대표팀이 sới đấu에서 승리했습니다!\n양국이 함께 전통 음식을 나누며 우정을 축하합니다!</size>"
                               : "<size=32><color=#FFD700><b>CHIẾN THẮNG HỮU NGHỊ!</b></color></size>\n\n<size=20>Đội tuyển Việt Nam đã xuất sắc trụ vững sới đấu!\nHai bên cùng chia sẻ ẩm thực Bánh Chưng & Kimchi!</size>";
            if (victoryBannerImage != null && vicSprite != null)
            {
                victoryBannerImage.sprite = vicSprite;
            }
        }
        else if (isEnemyWin)
        {
            message = isKorean ? "<size=32><color=#4D96FF><b>아쉬운 패배!</b></color></size>\n\n<size=20>대한민국 대표팀이 승리했습니다!\n스포츠맨십과 우정의 가치를 높이 기립니다!</size>"
                               : "<size=32><color=#4D96FF><b>THẤT BẠI TIẾC NUỐI!</b></color></size>\n\n<size=20>Đội tuyển Hàn Quốc đã giành thắng lợi thuyết phục!\nHai dân tộc cùng bắt tay tôn vinh tinh thần thượng võ!</size>";
            if (victoryBannerImage != null && defSprite != null)
            {
                victoryBannerImage.sprite = defSprite;
            }
        }
        else
        {
            message = isKorean ? "<size=32><color=#FFD700><b>아름다운 화합!</b></color></size>\n\n<size=20>양국이 대등한 기량으로 우정의 축제를 빛냈습니다!</size>"
                               : "<size=32><color=#FFD700><b>HÒA HỢP HỮU NGHỊ!</b></color></size>\n\n<size=20>Trận đấu ngang tài ngang sức mang đậm nét đẹp văn hóa Việt - Hàn!</size>";
            if (victoryBannerImage != null && vicSprite != null)
            {
                victoryBannerImage.sprite = vicSprite;
            }
        }

        if (victoryPanel != null)
        {
            var repImg = victoryPanel.transform.Find("Btn_Replay")?.GetComponent<Image>();
            if (repImg != null)
            {
                Sprite sp = Resources.Load<Sprite>($"UI/Popups/btn_result_replay_{tag}");
#if UNITY_EDITOR
                if (sp == null) sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/UI/Popups/btn_result_replay_{tag}.png");
#endif
                if (sp != null) repImg.sprite = sp;
            }

            var homeImg = victoryPanel.transform.Find("Btn_Home")?.GetComponent<Image>();
            if (homeImg != null)
            {
                Sprite sp = Resources.Load<Sprite>($"UI/Popups/btn_result_close_{tag}");
#if UNITY_EDITOR
                if (sp == null) sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/UI/Popups/btn_result_close_{tag}.png");
#endif
                if (sp != null) homeImg.sprite = sp;
            }
        }

        if (victoryTmpText != null) victoryTmpText.text = message;
        if (victoryMessageText != null) victoryMessageText.text = message;
        if (victoryPanel != null) victoryPanel.SetActive(true);

        Debug.Log("<color=cyan>[KẾT THÚC TRẬN ĐẤU]</color> " + (isPlayerWin ? "Việt Nam Thắng!" : isEnemyWin ? "Hàn Quốc Thắng!" : "Hòa!"));
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            int seconds = Mathf.CeilToInt(currentTimer);
            string feverTag = isFeverTime ? " <color=yellow>[FEVER]</color>" : "";
            scoreText.text = LocalizationManager.Instance != null ? LocalizationManager.Instance.FormatScore(playerScore, enemyScore, seconds) : $"VIỆT NAM (베트남)  {playerScore}  -  {enemyScore}  HÀN QUỐC (대한민국)   ({seconds}s)";
        }
    }

    /// <summary>
    /// Cập nhật bán kính vòng đai Ring-out Collider khớp với sới đấu của map
    /// </summary>
    public void SetArenaRadius(float radius)
    {
        var arenaObj = GameObject.Find("Arena_Stadium");
        if (arenaObj != null)
        {
            var col = arenaObj.GetComponent<CircleCollider2D>();
            if (col != null)
            {
                col.radius = radius > 0 ? radius : 3.65f;
                Debug.Log($"<color=cyan>[ARENA]</color> Đã cập nhật bán kính vòng đai Ring-out: {col.radius:F2}m");
            }
        }
    }

    /// <summary>
    /// Khởi động trận đấu với bản đồ và tướng đã chọn từ Match Hub
    /// </summary>
    public void StartGameWithSettings(Sprite mapSprite, PlayerController.MascotType playerMascot, AIController.MascotType enemyMascot, float arenaRadius = 3.65f)
    {
        Time.timeScale = 1f;

        // 1. Cập nhật Sprite nền sới đấu đã chọn
        var bgObj = GameObject.Find("Full_Background");
        if (bgObj != null)
        {
            var sr = bgObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (mapSprite != null)
                {
                    sr.sprite = mapSprite;
                }
                else if (mapBackgroundSprites != null && mapBackgroundSprites.Length > 0 && mapBackgroundSprites[0] != null)
                {
                    sr.sprite = mapBackgroundSprites[0];
                }
            }
            var fsb = bgObj.GetComponent<FullScreenBackground>();
            if (fsb != null)
            {
                fsb.FitToScreen();
            }
        }

        // 2. Cập nhật kích thước vòng đai Ring-out Collider của map
        SetArenaRadius(arenaRadius);

        // 3. Cập nhật linh vật Player & Đối thủ
        if (player == null) player = Object.FindAnyObjectByType<PlayerController>();
        if (player != null) player.SwitchMascot(playerMascot);

        if (enemy == null) enemy = Object.FindAnyObjectByType<AIController>();
        if (enemy != null) enemy.SwitchMascot(enemyMascot);

        // 4. Thiết lập lại thông số trận đấu
        ResetMatch();

        // 5. Chuyển sang trạng thái thi đấu thực thụ
        SetState(GameState.BattlePlaying);
        WireButtons();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        ResetMatch();
        SetState(GameState.BattlePlaying);
        Debug.Log("<color=green>[RESTART]</color> Đã thiết lập lại ván đấu mới.");
    }

    public void GoHome()
    {
        Time.timeScale = 1f;
        ResetMatch();
        SetState(GameState.MatchHub);
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.ShowMatchHub();
        }
        Debug.Log("<color=cyan>[HOME]</color> Quay về Sảnh chọn tướng.");
    }

    public void TogglePause()
    {
        if (currentState == GameState.Paused)
        {
            Time.timeScale = 1f;
            SetState(previousState == GameState.Paused ? GameState.BattlePlaying : previousState);
            Debug.Log("<color=yellow>[RESUME]</color> Tiếp tục trận đấu.");
        }
        else if (currentState == GameState.BattlePlaying)
        {
            Time.timeScale = 0f;
            SetState(GameState.Paused);
            Debug.Log("<color=yellow>[PAUSE]</color> Đã tạm dừng trận đấu.");
        }
    }

    [SerializeField] private float bgmVolume = 1f;
    [SerializeField] private float sfxVolume = 1f;

    public float BGMVolume => bgmVolume;
    public float SFXVolume => sfxVolume;

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        AudioManager.Instance?.SetBGMVolume(bgmVolume);
        Debug.Log($"<color=cyan>[AUDIO]</color> Âm lượng Nhạc nền: {(int)(bgmVolume * 100)}%");
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        AudioManager.Instance?.SetSFXVolume(sfxVolume);
        Debug.Log($"<color=cyan>[AUDIO]</color> Âm lượng Hiệu ứng (SFX): {(int)(sfxVolume * 100)}%");
    }

    public void SetCrowdVolume(float volume)
    {
        AudioManager.Instance?.SetCrowdVolume(volume);
    }

    public void ToggleSound()
    {
        AudioListener.pause = !AudioListener.pause;
        Debug.Log(AudioListener.pause ? "<color=yellow>[SOUND]</color> Đã tắt âm thanh." : "<color=yellow>[SOUND]</color> Đã bật âm thanh.");
    }
}
