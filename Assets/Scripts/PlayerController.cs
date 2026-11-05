using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Quản lý điều khiển linh vật người chơi.
/// Hỗ trợ cả 2 chế độ điều khiển:
///  1. Drag_TowardsTarget (Kéo về phía đối thủ để phóng thẳng tới - Kiểu Bắn Bi / Bida).
///  2. Slingshot_PullBack (Kéo lùi về sau để bắn tới - Kiểu Ná Cao Su).
/// Thuộc dự án: Mythic Bumper (KGW2026).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
// Updated for Unity 6.6 Playtest Verified
public class PlayerController : MonoBehaviour
{
    public enum ControlMode
    {
        Drag_TowardsTarget, // Kéo về hướng nào -> Bắn thẳng về hướng đó (Rất trực quan cho di động)
        Slingshot_PullBack  // Kéo lùi về sau -> Bắn về phía trước (Kiểu ná cao su)
    }

    public enum MascotType
    {
        Custom,
        TrauVang,   // 0. Đô vật hội làng VN (Tanker: Mass 2.0, Drag 1.2, Force 22, Bounce 0.6)
        BeLong,     // 1. Rồng thời Lý VN (Speedster: Mass 1.0, Drag 0.9, Force 26, Bounce 0.9)
        LanSu,      // 2. Lân Sư May Mắn VN (Disruptor: Mass 1.4, Drag 1.1, Force 24, Bounce 0.8)
        BeTre,      // 3. Bé Tre Lũy Tre VN (Defender: Mass 1.7, Drag 1.4, Force 21, Bounce 0.5)
        ChimLac,    // 4. Chim Lạc Đông Sơn VN (Agile: Mass 1.1, Drag 0.9, Force 25, Bounce 0.85)
        HoBaekho,   // HQ 0. Bạch Hổ Ssireum KR
        Haechi      // HQ 1. Kỳ lân trừ tà KR
    }

    [Header("--- CHẾ ĐỘ ĐIỀU KHIỂN ---")]
    [Tooltip("Drag_TowardsTarget: Kéo về phía đối thủ là bắn sang đối thủ.\nSlingshot_PullBack: Kéo lùi về sau để nạp lực bắn tới.")]
    [SerializeField] private ControlMode controlMode = ControlMode.Drag_TowardsTarget;

    [Header("--- CẤU HÌNH LINH VẬT ---")]
    [SerializeField] private MascotType mascotType = MascotType.TrauVang;

    [Header("--- THÔNG SỐ VẬT LÝ ---")]
    [SerializeField] private float mass = 2.0f;
    [SerializeField] private float linearDrag = 0.8f;
    [SerializeField] private float maxShootForce = 25.0f; // Lực bắn mạnh mẽ đủ để lướt vèo qua sân
    [SerializeField] [Range(0f, 1.2f)] private float bounciness = 0.6f;

    [Header("--- THÔNG SỐ KÉO THẢ ---")]
    [Tooltip("Khoảng cách kéo tối đa để đạt 100% lực")]
    [SerializeField] private float maxDragDistance = 2.2f;

    [Tooltip("Vận tốc tối thiểu để cho phép bắn lượt tiếp theo")]
    [SerializeField] private float stopVelocityThreshold = 0.25f;

    [Header("--- HIỆU ỨNG VỆT LƯỚT (TRAIL) ---")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Color trailColor = new Color(1f, 0.85f, 0.2f, 0.85f); // Vàng kim Trâu Vàng

    [Tooltip("Độ nhạy vùng chạm xung quanh linh vật")]
    [SerializeField] private float touchRadiusMultiplier = 1.6f;

    [Header("--- HIỂN THỊ TIA ĐỊNH HƯỚNG ---")]
    [SerializeField] private LineRenderer aimLine;
    [SerializeField] private float visualAimLength = 3.5f;

    [Header("--- SỰ KIỆN ---")]
    public UnityEvent onShootStart;
    public UnityEvent onShootRelease;

    private Rigidbody2D rb;
    private CircleCollider2D col;
    private Animator animator;
    private Camera mainCamera;
    private PhysicsMaterial2D dynamicMaterial;

    private bool isDragging = false;
    private Vector2 dragCurrentPos;
    private Vector2 shootDirection;
    private float currentPullPercent;

    public bool IsDragging => isDragging;
    public bool CanShoot
    {
        get
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (rb == null) return false;
            if (!Application.isPlaying) return true;
            return rb.linearVelocity.magnitude <= 0.65f;
        }
    }

    private void OnValidate()
    {
        ApplyMascotStats();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;

        ApplyMascotStats();

        if (aimLine == null) aimLine = GetComponent<LineRenderer>();
        if (aimLine != null)
        {
            aimLine.positionCount = 2;
            aimLine.enabled = false;
        }

        SetupTrailRenderer();
    }

    private void Start()
    {
        BindSwapButton();
    }

    private void OnEnable()
    {
        BindSwapButton();
    }

    public void BindSwapButton()
    {
        var btn = MenuManager.FindButtonInScene("Btn_Swap");
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(SwapTagTeam);
        }
    }

    private void OnDisable()
    {
        isDragging = false;
        if (aimLine != null) aimLine.enabled = false;
        StopAllCoroutines();
    }

    public MascotType CurrentMascot => mascotType;

    /// <summary>
    /// Đổi linh vật Tag-Team giữa Trâu Vàng (Tanker) và Bé Long (Speedster)
    /// </summary>
    public void SwapTagTeam()
    {
        // Chặn đổi tướng khi không ở trong trận đấu thi đấu (BattlePlaying)
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.BattlePlaying)
        {
            return;
        }

        MascotType next = (mascotType == MascotType.TrauVang) ? MascotType.BeLong : MascotType.TrauVang;
        SwitchMascot(next);
    }

    /// <summary>
    /// Chuyển đổi linh vật theo chỉ định, nạp lại Sprite, thông số vật lý và kỹ năng
    /// </summary>
    public void SwitchMascot(MascotType newType)
    {
        mascotType = newType;
        ApplyMascotStats();

        // Đổi Sprite tương ứng
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            string spriteName = (newType == MascotType.TrauVang) ? "trau_vang_idle.png" :
                                (newType == MascotType.BeLong) ? "rong_vang_idle.png" :
                                (newType == MascotType.LanSu) ? "lan_su_idle.png" :
                                (newType == MascotType.BeTre) ? "be_tre_idle.png" :
                                (newType == MascotType.ChimLac) ? "chim_lac_idle.png" :
                                (newType == MascotType.HoBaekho) ? "ho_baekho_idle.png" : "haechi_idle.png";
#if UNITY_EDITOR
            var newSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Characters/{spriteName}");
            if (newSprite != null) sr.sprite = newSprite;
#endif
            sr.sortingLayerName = "Gameplay";
            if (sr.sortingOrder < 2) sr.sortingOrder = 5;
        }

        // Cập nhật Animator Controller tương ứng
        if (animator == null) animator = GetComponent<Animator>();
        if (animator != null)
        {
            string acName = (newType == MascotType.TrauVang) ? "AC_TrauVang" :
                            (newType == MascotType.BeLong) ? "AC_RongVang" :
                            (newType == MascotType.LanSu) ? "AC_LanSu" :
                            (newType == MascotType.BeTre) ? "AC_BeTre" :
                            (newType == MascotType.ChimLac) ? "AC_ChimLac" :
                            (newType == MascotType.HoBaekho) ? "AC_HoBaekho" : "AC_Haechi";
#if UNITY_EDITOR
            var newAc = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"Assets/Animations/Mascots/{acName}.controller");
            if (newAc != null) animator.runtimeAnimatorController = newAc;
#endif
        }

        // Cập nhật màu TrailRenderer
        if (trailRenderer != null)
        {
            trailColor = (newType == MascotType.TrauVang) ? new Color(1f, 0.85f, 0.2f, 0.85f) :
                         (newType == MascotType.BeLong) ? new Color(0.2f, 0.95f, 0.4f, 0.85f) :
                         (newType == MascotType.LanSu) ? new Color(1f, 0.25f, 0.2f, 0.85f) :
                         (newType == MascotType.BeTre) ? new Color(0.35f, 0.85f, 0.35f, 0.85f) :
                         (newType == MascotType.ChimLac) ? new Color(0.3f, 0.8f, 0.95f, 0.85f) :
                         (newType == MascotType.HoBaekho) ? new Color(0.2f, 0.85f, 1f, 0.85f) :
                         new Color(0.9f, 0.3f, 0.9f, 0.85f);

            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(trailColor, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.85f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            trailRenderer.colorGradient = grad;
        }

        // Hiệu ứng mây vàng khói nổ
        var vfx = Object.FindAnyObjectByType<VFXManager>();
        if (vfx != null)
        {
            vfx.PlayGoldenCloud(transform.position);
        }

        // Cập nhật tuyệt kỹ theo linh vật
        var skill = GetComponent<CharacterSkill>();
        if (skill != null)
        {
            skill.SetMascotSkill(newType);
        }

        Debug.Log($"<color=yellow>[TAG TEAM]</color> Người chơi đã xuất trận với: {newType}!");
    }

    private void SetupTrailRenderer()
    {
        if (trailRenderer == null) trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer == null)
        {
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
            trailRenderer.time = 0.35f;
            trailRenderer.startWidth = 0.65f;
            trailRenderer.endWidth = 0.05f;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null) trailRenderer.material = new Material(shader);
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(trailColor, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.85f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            trailRenderer.colorGradient = grad;
            trailRenderer.sortingLayerName = "Gameplay";
            trailRenderer.sortingOrder = 5;
            trailRenderer.emitting = false;
        }
    }

    public void ApplyMascotStats()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (col == null) col = GetComponent<CircleCollider2D>();

        if (mascotType != MascotType.Custom)
        {
            SetStatsByPreset(mascotType);
        }

        if (rb != null)
        {
            rb.mass = mass;
            rb.linearDamping = linearDrag;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Khóa cứng góc xoay đứng thẳng
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        if (col != null)
        {
            dynamicMaterial = new PhysicsMaterial2D($"{gameObject.name}_Mat")
            {
                bounciness = bounciness,
                friction = 0.15f
            };
            col.sharedMaterial = dynamicMaterial;
        }
    }

    private void SetStatsByPreset(MascotType type)
    {
        switch (type)
        {
            case MascotType.TrauVang:
                mass = 2.0f;
                linearDrag = 0.8f; // Ma sát sàn vừa phải để lướt mượt mà qua sân
                maxShootForce = 25.0f; // Lực bắn mạnh mẽ đủ để băng qua sân húc bay đối thủ
                bounciness = 0.6f;
                break;
            case MascotType.BeLong:
                mass = 1.0f;
                linearDrag = 0.9f;
                maxShootForce = 26.0f;
                bounciness = 0.9f;
                break;
            case MascotType.LanSu:
                mass = 1.4f;
                linearDrag = 1.0f;
                maxShootForce = 25.0f;
                bounciness = 0.8f;
                break;
            case MascotType.BeTre:
                mass = 1.7f;
                linearDrag = 1.3f;
                maxShootForce = 22.0f;
                bounciness = 0.5f;
                break;
            case MascotType.ChimLac:
                mass = 1.1f;
                linearDrag = 0.85f;
                maxShootForce = 26.0f;
                bounciness = 0.85f;
                break;
            case MascotType.HoBaekho:
                mass = 1.8f;
                linearDrag = 1.1f;
                maxShootForce = 24.0f;
                bounciness = 0.7f;
                break;
            case MascotType.Haechi:
                mass = 1.1f;
                linearDrag = 1.0f;
                maxShootForce = 25.0f;
                bounciness = 0.85f;
                break;
        }
    }

    private void Update()
    {
        HandleInput();

        // Phím tắt bàn phím tiện lợi khi thử nghiệm
        if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.Tab))
        {
            SwapTagTeam();
        }

        if (trailRenderer != null && rb != null)
        {
            trailRenderer.emitting = rb.linearVelocity.magnitude > 1.5f;
        }
    }

    private void FixedUpdate()
    {
        // Khi linh vật chạy chậm lại dưới ngưỡng, dừng hẳn để sẵn sàng lượt kéo bắn mới ngay lập tức
        if (rb != null && !isDragging && rb.linearVelocity.magnitude < 0.25f && rb.linearVelocity.magnitude > 0.0001f)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void HandleInput()
    {
        // Chặn điều khiển nếu không ở trạng thái BattlePlaying
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.BattlePlaying)
        {
            if (isDragging)
            {
                isDragging = false;
                if (aimLine != null) aimLine.enabled = false;
            }
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryStartDrag();
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            OnDragging();
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            ReleaseAndShoot();
        }
    }

    private void TryStartDrag()
    {
        // Chặn chạm khi bấm vào nút UI
        if (EventSystem.current != null)
        {
            if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                return;
            if (EventSystem.current.IsPointerOverGameObject())
                return;
        }

        Vector2 touchWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // Bán kính chạm hào phóng (2.8 đơn vị thế giới) hoặc chạm nửa sân của người chơi
        float touchDist = Vector2.Distance(touchWorldPos, transform.position);
        bool isNearPlayer = touchDist <= 2.8f;
        bool isPlayerHalf = touchWorldPos.x <= 0.8f && touchWorldPos.magnitude <= 5.5f;

        if (isNearPlayer || isPlayerHalf)
        {
            // Nếu đang di chuyển chậm, dừng lại ngay để bắt đầu ngắm bắn
            if (rb != null && rb.linearVelocity.magnitude <= 0.8f)
            {
                rb.linearVelocity = Vector2.zero;
            }

            if (!CanShoot) return;

            isDragging = true;
            dragCurrentPos = touchWorldPos;

            if (aimLine != null) aimLine.enabled = true;
            onShootStart?.Invoke();
            Debug.Log("<color=green>[PLAYER]</color> Bắt đầu kéo ngắm bắn linh vật!");
        }
    }

    private void OnDragging()
    {
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        dragCurrentPos = mouseWorldPos;

        Vector2 rawVector;
        if (controlMode == ControlMode.Drag_TowardsTarget)
        {
            // Kéo về đâu -> Bắn về đó (Kéo sang phải hướng tới đối thủ -> Bắn sang phải)
            rawVector = dragCurrentPos - (Vector2)transform.position;
        }
        else
        {
            // Slingshot: Kéo lùi về sau để nạp lực
            rawVector = (Vector2)transform.position - dragCurrentPos;
        }

        float dragDistance = Mathf.Clamp(rawVector.magnitude, 0f, maxDragDistance);
        currentPullPercent = dragDistance / maxDragDistance;

        if (rawVector.sqrMagnitude > 0.001f)
        {
            shootDirection = rawVector.normalized;
        }
        else
        {
            shootDirection = Vector2.zero;
        }

        UpdateAimLineVisual(shootDirection, currentPullPercent);
    }

    private void ReleaseAndShoot()
    {
        isDragging = false;
        if (aimLine != null) aimLine.enabled = false;

        if (currentPullPercent > 0.05f && shootDirection != Vector2.zero)
        {
            float appliedForce = currentPullPercent * maxShootForce;
            // Triệt tiêu quán tính cũ trước khi bắn để đảm bảo đường bay chính xác tuyệt đối
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(shootDirection * appliedForce, ForceMode2D.Impulse);
            }
            if (animator == null) animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("OnAttack");
                animator.SetTrigger("OnDash");
            }
            onShootRelease?.Invoke();
            Debug.Log($"<color=green>[PLAYER]</color> Phóng linh vật! Lực: {appliedForce:F1} | Hướng: {shootDirection}");
        }

        currentPullPercent = 0f;
        shootDirection = Vector2.zero;
    }

    private void UpdateAimLineVisual(Vector2 direction, float percent)
    {
        if (aimLine == null || !aimLine.enabled) return;

        Vector3 startPoint = transform.position;
        Vector3 endPoint = startPoint + (Vector3)(direction * (visualAimLength * percent));

        aimLine.SetPosition(0, startPoint);
        aimLine.SetPosition(1, endPoint);
    }

    public void ApplyMudSlowdown(float factor = 0.5f)
    {
        if (rb != null)
        {
            rb.linearVelocity *= factor;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        AudioManager.Instance?.PlayCollisionHit();
        if (collision.relativeVelocity.magnitude > 2.0f && VFXManager.Instance != null)
        {
            // Rung màn hình và tạo cảm giác va chạm nảy lửa khi 2 linh vật đụng nhau hoặc đụng vại gốm/chum
            VFXManager.Instance.TriggerCameraShake(0.15f, 0.12f);
        }
    }

    private void LateUpdate()
    {
        // Khóa tuyệt đối góc xoay đứng thẳng, ngăn mọi va chạm hay lộn ngược đầu
        transform.rotation = Quaternion.identity;
    }

    private void OnDestroy()
    {
        if (dynamicMaterial != null) Destroy(dynamicMaterial);
    }
}
