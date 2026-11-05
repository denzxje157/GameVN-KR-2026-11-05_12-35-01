using UnityEngine;

/// <summary>
/// Trí tuệ nhân tạo (AI) điều khiển đối thủ Hàn Quốc (5 Linh vật: Bạch Hổ, Haechi, Dokkaebi, Gấu Dangoon, Samjoko).
/// Cơ chế AI thông minh (Smart AI):
///  1. Dự đoán hướng di chuyển của người chơi (Trajectory Prediction).
///  2. Chống tự sát mép sân (Edge Avoidance) - tự động chỉnh góc vào tâm sân khi ở gần rìa.
///  3. Né vũng bùn & vật cản (Obstacle Awareness).
///  4. Báo hiệu nạp lực (Telegraphing) và kích hoạt tuyệt kỹ đặc trưng theo từng tướng.
/// Dự án: Mythic Bumper (KGW2026).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class AIController : MonoBehaviour
{
    public enum MascotType
    {
        Custom,
        HoBaekho,   // HQ 0. Bạch Hổ Ssireum (Bruiser - Sức mạnh)
        Haechi,     // HQ 1. Kỳ Lân Seoul (Trickster - Phản đòn/Lắt léo)
        Dokkaebi,   // HQ 2. Yêu Tinh Dokkaebi (Disruptor - Gây nhiễu/Chùy phép)
        GauDangoon, // HQ 3. Gấu Thần Dangoon (Heavy Tank - Sức nặng/Địa chấn)
        Samjoko,    // HQ 4. Chim Ba Chân Samjoko (Speedster - Tốc độ/Băng vút)
        TrauVang,   // VN 0
        BeLong      // VN 1
    }

    [Header("--- CẤU HÌNH LINH VẬT BOT ---")]
    [SerializeField] private MascotType mascotType = MascotType.HoBaekho;
    [SerializeField] private float mass = 1.8f;
    [SerializeField] private float linearDrag = 0.8f;
    [SerializeField] private float maxShootForce = 24.0f;
    [SerializeField] [Range(0f, 1.2f)] private float bounciness = 0.7f;

    [Header("--- MỤC TIÊU & TẦN SUẤT TẤN CÔNG ---")]
    [Tooltip("Mục tiêu người chơi cần ngắm bắn")]
    [SerializeField] private Transform targetPlayer;

    [Tooltip("Thời gian giữa mỗi lần ngắm bắn (giây)")]
    [SerializeField] private float attackCooldown = 2.4f;

    [Tooltip("Độ lệch góc ngẫu nhiên (độ) để bot chơi tự nhiên")]
    [SerializeField] private float randomAngleDeviation = 8.0f;

    [Tooltip("Ngưỡng vận tốc tối thiểu để AI coi là đã đứng yên")]
    [SerializeField] private float stopVelocityThreshold = 0.25f;

    [Header("--- TRẠNG THÁI NẠP LỰC (TELEGRAPH) ---")]
    [Tooltip("Thời gian bot dừng lại chuẩn bị phóng (báo hiệu cho người chơi né)")]
    [SerializeField] private float chargeDuration = 0.45f;

    [Header("--- HIỆU ỨNG VỆT LƯỚT (TRAIL) ---")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Color trailColor = new Color(0.2f, 0.85f, 1f, 0.85f);

    private Rigidbody2D rb;
    private CircleCollider2D col;
    private Animator animator;
    private PhysicsMaterial2D dynamicMaterial;
    private float timer = 0f;
    private bool isCharging = false;
    private float chargeTimer = 0f;
    private Vector2 pendingDirection;
    private float pendingForce;
    private int shootCounter = 0;

    public bool IsStopped => rb != null && rb.linearVelocity.magnitude <= stopVelocityThreshold;
    public MascotType CurrentMascot => mascotType;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        animator = GetComponent<Animator>();

        ApplyStats();

        if (targetPlayer == null)
        {
            var player = Object.FindAnyObjectByType<PlayerController>();
            if (player != null) targetPlayer = player.transform;
        }

        SetupTrailRenderer();
    }

    public void SwitchMascot(MascotType newType)
    {
        mascotType = newType;
        SetStatsByPreset(newType);
        ApplyStats();

        // Cập nhật Sprite
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            string spriteName = (newType == MascotType.HoBaekho) ? "ho_baekho_idle.png" :
                                (newType == MascotType.Haechi) ? "haechi_idle.png" :
                                (newType == MascotType.Dokkaebi) ? "dokkaebi_idle.png" :
                                (newType == MascotType.GauDangoon) ? "gau_dangoon_idle.png" :
                                (newType == MascotType.Samjoko) ? "samjoko_idle.png" : "ho_baekho_idle.png";
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
            string acName = (newType == MascotType.HoBaekho) ? "AC_HoBaekho" :
                            (newType == MascotType.Haechi) ? "AC_Haechi" :
                            (newType == MascotType.Dokkaebi) ? "AC_Dokkaebi" :
                            (newType == MascotType.GauDangoon) ? "AC_GauDangoon" :
                            (newType == MascotType.Samjoko) ? "AC_Samjoko" : "AC_HoBaekho";
#if UNITY_EDITOR
            var newAc = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"Assets/Animations/Mascots/{acName}.controller");
            if (newAc != null) animator.runtimeAnimatorController = newAc;
#endif
        }

        // Cập nhật màu TrailRenderer
        if (trailRenderer != null)
        {
            trailColor = (newType == MascotType.HoBaekho) ? new Color(0.2f, 0.85f, 1f, 0.85f) :
                         (newType == MascotType.Haechi) ? new Color(0.8f, 0.3f, 0.95f, 0.85f) :
                         (newType == MascotType.Dokkaebi) ? new Color(0.6f, 0.2f, 0.85f, 0.85f) :
                         (newType == MascotType.GauDangoon) ? new Color(0.9f, 0.6f, 0.2f, 0.85f) :
                         new Color(1f, 0.35f, 0.1f, 0.85f);

            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(trailColor, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.85f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            trailRenderer.colorGradient = grad;
        }

        Debug.Log($"<color=cyan>[BOT HQ]</color> Đối thủ AI đã chuyển sang linh vật: {newType}!");
    }

    private void SetStatsByPreset(MascotType type)
    {
        switch (type)
        {
            case MascotType.HoBaekho:
                mass = 1.8f;
                linearDrag = 0.8f;
                maxShootForce = 24.0f;
                bounciness = 0.7f;
                break;
            case MascotType.Haechi:
                mass = 1.2f;
                linearDrag = 0.9f;
                maxShootForce = 25.0f;
                bounciness = 0.85f;
                break;
            case MascotType.Dokkaebi:
                mass = 1.3f;
                linearDrag = 0.9f;
                maxShootForce = 25.0f;
                bounciness = 0.8f;
                break;
            case MascotType.GauDangoon:
                mass = 2.2f;
                linearDrag = 1.2f;
                maxShootForce = 21.0f;
                bounciness = 0.55f;
                break;
            case MascotType.Samjoko:
                mass = 1.0f;
                linearDrag = 0.85f;
                maxShootForce = 27.0f;
                bounciness = 0.95f;
                break;
        }
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

    private void OnValidate()
    {
        ApplyStats();
    }

    private void ApplyStats()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (col == null) col = GetComponent<CircleCollider2D>();

        if (rb != null)
        {
            rb.mass = mass;
            rb.linearDamping = linearDrag;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        if (col != null)
        {
            dynamicMaterial = new PhysicsMaterial2D($"{gameObject.name}_AIMat")
            {
                bounciness = bounciness,
                friction = 0.15f
            };
            col.sharedMaterial = dynamicMaterial;
        }
    }

    private void OnDisable()
    {
        isCharging = false;
        chargeTimer = 0f;
        transform.localScale = Vector3.one;
        StopAllCoroutines();
    }

    private void FixedUpdate()
    {
        // Khi bot di chuyển chậm lại dưới ngưỡng, triệt tiêu vi sai để dừng hẳn
        if (rb != null && !isCharging && rb.linearVelocity.magnitude < 0.25f && rb.linearVelocity.magnitude > 0.0001f)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void Update()
    {
        // CHỈ thực thi AI khi đang ở đúng trạng thái BattlePlaying
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.BattlePlaying)
        {
            if (isCharging)
            {
                isCharging = false;
                chargeTimer = 0f;
                transform.localScale = Vector3.one;
            }
            return;
        }

        if (trailRenderer != null && rb != null)
        {
            trailRenderer.emitting = rb.linearVelocity.magnitude > 2.0f;
        }

        if (targetPlayer == null)
        {
            var player = Object.FindAnyObjectByType<PlayerController>();
            if (player != null) targetPlayer = player.transform;
            return;
        }

        // Báo hiệu nạp lực (Telegraphing pulse)
        if (isCharging)
        {
            chargeTimer += Time.deltaTime;
            float pulse = 1f + Mathf.Sin(chargeTimer * 22f) * 0.08f;
            transform.localScale = new Vector3(pulse, pulse, 1f);

            if (chargeTimer >= chargeDuration)
            {
                ExecuteShoot();
            }
            return;
        }

        // Chỉ nạp lượt bắn mới khi bot đã dừng lại
        if (IsStopped)
        {
            timer += Time.deltaTime;
            if (timer >= attackCooldown)
            {
                StartChargingAttack();
            }
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }

    private void StartChargingAttack()
    {
        timer = 0f;
        isCharging = true;
        chargeTimer = 0f;

        Vector2 myPos = transform.position;
        Vector2 targetPos = targetPlayer.position;

        // 1. Dự đoán vị trí di chuyển của Player (Trajectory Prediction)
        var playerRb = targetPlayer.GetComponent<Rigidbody2D>();
        if (playerRb != null && playerRb.linearVelocity.magnitude > 0.5f)
        {
            float dist = Vector2.Distance(myPos, targetPos);
            float leadTime = Mathf.Clamp(dist / maxShootForce, 0.1f, 0.4f);
            targetPos += playerRb.linearVelocity * leadTime;
        }

        // 2. Tính vector ngắm tới mục tiêu
        Vector2 aimDir = (targetPos - myPos).normalized;

        // 3. Cơ chế chống tự sát mép sàn (Edge Avoidance)
        float distFromCenter = myPos.magnitude;
        if (distFromCenter > 2.2f)
        {
            // Nếu AI đang đứng gần rìa ngoài sới đấu (bán kính 3.65f)
            // Kiểm tra xem hướng bắn có đang đẩy bot ra ngoài vòng bo không
            Vector2 dirToCenter = -myPos.normalized;
            float dot = Vector2.Dot(aimDir, myPos.normalized);
            if (dot > 0.1f)
            {
                // Hướng bắn nguy hiểm! Bẻ lái 70% vào tâm sới đấu để giữ mạng
                aimDir = Vector2.Lerp(aimDir, dirToCenter, 0.75f).normalized;
            }
        }

        // 4. Né vũng bùn ở giữa sân (Obstacle Avoidance)
        // Nếu đường ngắm đi ngang qua (0,0) trong vòng 0.8m, thêm độ lệch né tránh
        float distToMud = Vector2.Distance(Vector2.zero, myPos);
        if (distToMud > 1.2f && Vector2.Dot(aimDir, -myPos.normalized) > 0.8f)
        {
            Vector2 perp = new Vector2(-aimDir.y, aimDir.x);
            aimDir = (aimDir + perp * 0.35f).normalized;
        }

        // 5. Thêm độ lệch ngẫu nhiên nhẹ để AI tự nhiên
        float deviation = Random.Range(-randomAngleDeviation, randomAngleDeviation);
        float angleRad = deviation * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);
        Vector2 finalDir = new Vector2(aimDir.x * cos - aimDir.y * sin, aimDir.x * sin + aimDir.y * cos).normalized;

        pendingDirection = finalDir;
        pendingForce = Random.Range(0.85f, 1.0f) * maxShootForce;
    }

    private void ExecuteShoot()
    {
        isCharging = false;
        transform.localScale = Vector3.one;

        if (rb != null)
        {
            rb.AddForce(pendingDirection * pendingForce, ForceMode2D.Impulse);
        }

        if (animator == null) animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("OnAttack");
            animator.SetTrigger("OnDash");
        }

        shootCounter++;
        // Kích hoạt chiêu thức khi người chơi ở gần hoặc mỗi 2 lượt bắn
        float distToPlayer = targetPlayer != null ? Vector2.Distance(transform.position, targetPlayer.position) : 99f;
        if ((distToPlayer < 2.5f || shootCounter % 2 == 0) && VFXManager.Instance != null)
        {
            TriggerMascotSkill();
        }
    }

    private void TriggerMascotSkill()
    {
        if (VFXManager.Instance == null) return;

        switch (mascotType)
        {
            case MascotType.HoBaekho:
                Debug.Log("<color=cyan>[TUYỆT KỸ BOT]</color> Bạch Hổ xuất chiêu: HỔ KHIẾU SƠN HÀ!");
                VFXManager.Instance.PlayHoKhieuSonHa(transform.position, pendingDirection);
                break;
            case MascotType.Haechi:
                Debug.Log("<color=cyan>[TUYỆT KỸ BOT]</color> Haechi xuất chiêu: LINH KHÍ TRỪ TÀ!");
                VFXManager.Instance.PlayGoldenCloud(transform.position);
                break;
            case MascotType.Dokkaebi:
                Debug.Log("<color=cyan>[TUYỆT KỸ BOT]</color> Dokkaebi xuất chiêu: CHÙY PHÉP MA TRỜI!");
                VFXManager.Instance.PlayTigerClaw(transform.position);
                break;
            case MascotType.GauDangoon:
                Debug.Log("<color=cyan>[TUYỆT KỸ BOT]</color> Gấu Dangoon xuất chiêu: ĐỊA CHẤN THẦN THOẠI!");
                VFXManager.Instance.PlayKimNguuShockwave(transform.position);
                break;
            case MascotType.Samjoko:
                Debug.Log("<color=cyan>[TUYỆT KỸ BOT]</color> Samjoko xuất chiêu: LIỆT HỎA BĂNG VÚT!");
                VFXManager.Instance.PlayLotusPetals(transform.position);
                break;
            default:
                VFXManager.Instance.PlayHoKhieuSonHa(transform.position, pendingDirection);
                break;
        }
    }

    public void ApplyMudSlowdown(float factor = 0.5f)
    {
        if (rb != null)
        {
            rb.linearVelocity *= factor;
        }
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }

    private void OnDestroy()
    {
        if (dynamicMaterial != null) Destroy(dynamicMaterial);
    }
}
