using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Quản lý kích hoạt Tuyệt Kỹ của linh vật khi người chơi nhấn nút Kỹ Năng trên Mobile HUD.
/// Trâu Vàng: "Kim Ngưu Trấn Địa" -> Dậm đất tạo sóng xung kích hất kẻ địch xung quanh + Mass x3 trong 2s.
/// Dự án: Mythic Bumper (KGW2026).
/// </summary>
public class CharacterSkill : MonoBehaviour
{
    [Header("--- TUYỆT KỸ KIM NGƯU TRẤN ĐỊA ---")]
    [Tooltip("Thời gian hồi chiêu (giây)")]
    [SerializeField] private float cooldownTime = 8.0f;

    [Tooltip("Bán kính sóng xung kích hất kẻ địch")]
    [SerializeField] private float shockwaveRadius = 3.5f;

    [Tooltip("Lực hất văng của sóng xung kích")]
    [SerializeField] private float shockwaveForce = 12.0f;

    [Tooltip("Thời gian tăng khối lượng (Mass x3)")]
    [SerializeField] private float buffDuration = 2.0f;

    [Header("--- SPRITE THAY ĐỔI KHI TUNG CHIÊU ---")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite skillSprite;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float currentCooldown = 0f;
    private bool isSkillActive = false;
    private float originalMass;

    public bool CanUseSkill => currentCooldown <= 0f && !isSkillActive;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (rb != null) originalMass = rb.mass;

        // Tự động tìm Sprite nếu chưa gán
        if (idleSprite == null && sr != null) idleSprite = sr.sprite;
        if (skillSprite == null)
        {
#if UNITY_EDITOR
            skillSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Characters/trau_vang_skill_kim_nguu_tran_dia.png");
#endif
        }
    }

    private void Start()
    {
        BindSkillButton();
    }

    private void OnEnable()
    {
        BindSkillButton();
    }

    public void BindSkillButton()
    {
        var btn = MenuManager.FindButtonInScene("Btn_Skill");
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(ActivateSkill);
        }
    }

    private void Update()
    {
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
        }

        // Hỗ trợ phím Space trên PC để test nhanh
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ActivateSkill();
        }
    }

    public void SetMascotSkill(PlayerController.MascotType mascotType)
    {
        currentCooldown = 0f;
        isSkillActive = false;

        string idleName = (mascotType == PlayerController.MascotType.TrauVang) ? "trau_vang_idle.png" :
                          (mascotType == PlayerController.MascotType.BeLong) ? "rong_vang_idle.png" :
                          (mascotType == PlayerController.MascotType.LanSu) ? "lan_su_idle.png" :
                          (mascotType == PlayerController.MascotType.BeTre) ? "be_tre_idle.png" :
                          (mascotType == PlayerController.MascotType.ChimLac) ? "chim_lac_idle.png" :
                          (mascotType == PlayerController.MascotType.HoBaekho) ? "ho_baekho_idle.png" : "haechi_idle.png";

        string skillName = (mascotType == PlayerController.MascotType.TrauVang) ? "trau_vang_skill_kim_nguu_tran_dia.png" :
                           (mascotType == PlayerController.MascotType.BeLong) ? "rong_vang_skill_luot_may.png" :
                           (mascotType == PlayerController.MascotType.LanSu) ? "lan_su_idle.png" :
                           (mascotType == PlayerController.MascotType.BeTre) ? "be_tre_idle.png" :
                           (mascotType == PlayerController.MascotType.ChimLac) ? "chim_lac_idle.png" :
                           (mascotType == PlayerController.MascotType.HoBaekho) ? "ho_baekho_skill_ho_khieu_son_ha.png" : "haechi_skill_linh_khi_tru_ta.png";

#if UNITY_EDITOR
        idleSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Characters/{idleName}");
        skillSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Characters/{skillName}");
#endif
        if (sr != null && idleSprite != null) sr.sprite = idleSprite;
        if (rb != null) originalMass = rb.mass;
    }

    private void OnDisable()
    {
        isSkillActive = false;
        StopAllCoroutines();
        if (sr != null && idleSprite != null) sr.sprite = idleSprite;
        if (rb != null && originalMass > 0f) rb.mass = originalMass;
    }

    /// <summary>
    /// Kích hoạt tuyệt kỹ linh vật
    /// </summary>
    public void ActivateSkill()
    {
        AudioManager.Instance?.PlaySkillBurst();
        // Chặn tuyệt kỹ nếu không ở trong trận đấu (BattlePlaying)
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.BattlePlaying)
            return;

        if (!CanUseSkill) return;

        var anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("OnAttack");
            anim.SetTrigger("OnDash");
        }

        var playerCtrl = GetComponent<PlayerController>();
        var mascot = playerCtrl != null ? playerCtrl.CurrentMascot : PlayerController.MascotType.TrauVang;

        switch (mascot)
        {
            case PlayerController.MascotType.BeLong:
                StartCoroutine(ExecuteLongDuCuuTieu());
                break;
            case PlayerController.MascotType.LanSu:
                StartCoroutine(ExecuteLanSuHyKhi());
                break;
            case PlayerController.MascotType.BeTre:
                StartCoroutine(ExecuteBeTreHienLuy());
                break;
            case PlayerController.MascotType.ChimLac:
                StartCoroutine(ExecuteChimLacPhongBa());
                break;
            default:
                StartCoroutine(ExecuteKimNguuTranDia());
                break;
        }
    }

    private IEnumerator ExecuteLanSuHyKhi()
    {
        isSkillActive = true;
        currentCooldown = cooldownTime;
        Debug.Log("<color=red>[TUYỆT KỸ]</color> Lân Sư May Mắn kích hoạt: HỶ KHÍ PHÁO HOA!");
        if (VFXManager.Instance != null) VFXManager.Instance.PlayLotusPetals(transform.position);

        // Hất văng kẻ địch xung quanh và tăng tốc độ di chuyển
        PushNearbyEnemies(4.0f, 14.0f);
        yield return new WaitForSeconds(1.5f);
        isSkillActive = false;
    }

    private IEnumerator ExecuteBeTreHienLuy()
    {
        isSkillActive = true;
        currentCooldown = cooldownTime;
        Debug.Log("<color=green>[TUYỆT KỸ]</color> Bé Tre kích hoạt: LŨY TRE BẤT KHẢ XÂM PHẠM!");
        if (VFXManager.Instance != null) VFXManager.Instance.PlayGoldenCloud(transform.position);

        if (rb != null)
        {
            rb.mass = originalMass * 4.0f; // Siêu trụ sàn không thể lay chuyển
            rb.linearVelocity = Vector2.zero;
        }
        yield return new WaitForSeconds(buffDuration + 0.5f);
        if (rb != null) rb.mass = originalMass;
        isSkillActive = false;
    }

    private IEnumerator ExecuteChimLacPhongBa()
    {
        isSkillActive = true;
        currentCooldown = cooldownTime;
        Debug.Log("<color=yellow>[TUYỆT KỸ]</color> Chim Lạc kích hoạt: PHONG BA ĐÔNG SƠN!");
        if (VFXManager.Instance != null) VFXManager.Instance.PlayKimNguuShockwave(transform.position);

        PushNearbyEnemies(4.5f, 16.0f);
        yield return new WaitForSeconds(1.5f);
        isSkillActive = false;
    }

    private void PushNearbyEnemies(float radius, float force)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hit in hitColliders)
        {
            if (hit.gameObject != gameObject && hit.CompareTag("Enemy"))
            {
                var enemyRb = hit.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    Vector2 pushDir = (hit.transform.position - transform.position).normalized;
                    if (pushDir == Vector2.zero) pushDir = Vector2.right;
                    enemyRb.AddForce(pushDir * force, ForceMode2D.Impulse);
                }
            }
        }
    }

    private IEnumerator ExecuteLongDuCuuTieu()
    {
        isSkillActive = true;
        currentCooldown = cooldownTime;

        Debug.Log("<color=cyan>[TUYỆT KỸ]</color> Bé Long (Rồng Vàng) kích hoạt: LONG DU CỬU TIÊU (LƯỚT MÂY)!");

        if (sr != null && skillSprite != null) sr.sprite = skillSprite;
        if (VFXManager.Instance != null) VFXManager.Instance.PlayGoldenCloud(transform.position);

        // Lao vút về phía đối thủ với tốc độ cao
        var enemy = Object.FindAnyObjectByType<AIController>();
        if (enemy != null && rb != null)
        {
            Vector2 dashDir = (enemy.transform.position - transform.position).normalized;
            rb.AddForce(dashDir * 28f, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(1.5f);

        if (sr != null && idleSprite != null) sr.sprite = idleSprite;
        isSkillActive = false;
    }

    private IEnumerator ExecuteKimNguuTranDia()
    {
        isSkillActive = true;
        currentCooldown = cooldownTime;

        Debug.Log("<color=orange>[TUYỆT KỸ]</color> Trâu Vàng kích hoạt: KIM NGƯU TRẤN ĐỊA!");

        // 1. Đổi sprite sang trạng thái bốc lửa dậm đất
        if (sr != null && skillSprite != null)
        {
            sr.sprite = skillSprite;
        }

        // Kích hoạt hiệu ứng hình ảnh Sóng Chấn Động Trống Đồng & Rung màn hình
        if (VFXManager.Instance != null)
        {
            VFXManager.Instance.PlayKimNguuShockwave(transform.position);
        }

        // 2. Tăng khối lượng x3 (Trụ sân cực vững chắc)
        if (rb != null)
        {
            rb.mass = originalMass * 3f;
        }

        // 3. Tạo sóng xung kích hất văng mọi đối thủ trong bán kính xung quanh
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, shockwaveRadius);
        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject && hit.attachedRigidbody != null)
            {
                Vector2 pushDir = (hit.transform.position - transform.position).normalized;
                if (pushDir == Vector2.zero) pushDir = Vector2.right;

                hit.attachedRigidbody.AddForce(pushDir * shockwaveForce, ForceMode2D.Impulse);
            }
        }

        // Duy trì hiệu ứng trong 2 giây
        yield return new WaitForSeconds(buffDuration);

        // Khôi phục khối lượng và Sprite ban đầu
        if (rb != null) rb.mass = originalMass;
        if (sr != null && idleSprite != null) sr.sprite = idleSprite;

        isSkillActive = false;
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ vòng tròn bán kính sóng xung kích trong Scene View
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shockwaveRadius);
    }
}
