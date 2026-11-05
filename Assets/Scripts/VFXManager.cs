using UnityEngine;
using System.Collections;

/// <summary>
/// Quản lý toàn bộ Hiệu ứng Kỹ năng (VFX System) cho Lễ hội Linh vật:
/// - Sóng chấn động Trống Đồng & Mảnh vụn lúa vàng (Trâu Vàng)
/// - Luồng âm thanh gầm thét & Chấn động vuốt hổ (Bạch Hổ)
/// - Rung lắc màn hình (Screen Shake)
/// - Vệt lướt tốc độ cao (TrailRenderer)
/// </summary>
public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("--- SPRITES HIỆU ỨNG VIỆT NAM ---")]
    [SerializeField] private Sprite shockwaveSprite;
    [SerializeField] private Sprite riceDebrisSprite;
    [SerializeField] private Sprite bambooLeavesSprite;
    [SerializeField] private Sprite lotusTrailSprite;

    [Header("--- SPRITES HIỆU ỨNG HÀN QUỐC ---")]
    [SerializeField] private Sprite tigerRoarSprite;
    [SerializeField] private Sprite tigerClawSprite;
    [SerializeField] private Sprite mugunghwaPetalSprite;

    private Camera targetCamera;
    private Vector3 originalCamPos;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        targetCamera = Camera.main;
        if (targetCamera != null) originalCamPos = targetCamera.transform.position;

        LoadDefaultSpritesIfNull();
    }

    private void LoadDefaultSpritesIfNull()
    {
#if UNITY_EDITOR
        if (shockwaveSprite == null)
            shockwaveSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/VFX/Vietnam/vfx_song_chan_dong_trong_dong.png");
        if (riceDebrisSprite == null)
            riceDebrisSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/VFX/Vietnam/vfx_manh_vo_lua_vang.png");
        if (bambooLeavesSprite == null)
            bambooLeavesSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/VFX/Vietnam/vfx_la_tre_bay_xao_xac.png");
        if (lotusTrailSprite == null)
            lotusTrailSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/VFX/Vietnam/vfx_vet_luot_may_la_de.png");

        if (tigerRoarSprite == null)
            tigerRoarSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/VFX/Korea/vfx_luong_am_thanh_gam_thet.png");
        if (tigerClawSprite == null)
            tigerClawSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/VFX/Korea/vfx_chan_dong_vuot_ho.png");
        if (mugunghwaPetalSprite == null)
            mugunghwaPetalSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/VFX/Korea/vfx_canh_hoa_mugunghwa.png");
#endif
    }

    /// <summary>
    /// Kích hoạt kỹ năng Kim Ngưu Trấn Địa (Trâu Vàng)
    /// </summary>
    public void PlayKimNguuShockwave(Vector3 center)
    {
        // 1. Sinh sóng chấn động Trống Đồng dưới chân
        if (shockwaveSprite != null)
        {
            GameObject waveObj = new GameObject("VFX_Shockwave_TrongDong");
            waveObj.transform.position = new Vector3(center.x, center.y, 0.05f);
            var sr = waveObj.AddComponent<SpriteRenderer>();
            sr.sprite = shockwaveSprite;
            sr.sortingLayerName = "Gameplay";
            sr.sortingOrder = 5;
            sr.color = new Color(1f, 0.95f, 0.5f, 1f);
            StartCoroutine(ScaleAndFadeRoutine(waveObj, 0.4f, 2.6f, 0.45f));
        }

        // 2. Bắn tung các mảnh vụn lúa vàng & lá tre xung quanh
        SpawnDebrisBurst(center, riceDebrisSprite, count: 6, speed: 4.5f);
        SpawnDebrisBurst(center, bambooLeavesSprite, count: 4, speed: 3.5f);

        // 3. Rung màn hình uy lực
        TriggerCameraShake(0.35f, 0.22f);
    }

    /// <summary>
    /// Kích hoạt kỹ năng Hổ Khiếu Sơn Hà (Bạch Hổ)
    /// </summary>
    public void PlayHoKhieuSonHa(Vector3 center, Vector2 direction)
    {
        if (direction == Vector2.zero) direction = Vector2.left;
        direction.Normalize();

        // 1. Luồng âm thanh gầm thét bay thẳng
        if (tigerRoarSprite != null)
        {
            GameObject roarObj = new GameObject("VFX_TigerRoarWave");
            roarObj.transform.position = center + (Vector3)(direction * 0.8f);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            roarObj.transform.rotation = Quaternion.Euler(0, 0, angle);

            var sr = roarObj.AddComponent<SpriteRenderer>();
            sr.sprite = tigerRoarSprite;
            sr.sortingLayerName = "Gameplay";
            sr.sortingOrder = 6;
            sr.color = new Color(0.6f, 0.9f, 1f, 0.95f);

            var rb = roarObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            var col = roarObj.AddComponent<CircleCollider2D>();
            col.radius = 0.8f;
            col.isTrigger = true;

            StartCoroutine(ProjectileFlyAndFade(roarObj, direction, speed: 7.0f, duration: 0.8f));
        }

        // 2. Chấn động vuốt hổ
        if (tigerClawSprite != null)
        {
            GameObject clawObj = new GameObject("VFX_TigerClaw");
            clawObj.transform.position = center + (Vector3)(direction * 0.5f);
            var sr = clawObj.AddComponent<SpriteRenderer>();
            sr.sprite = tigerClawSprite;
            sr.sortingLayerName = "Gameplay";
            sr.sortingOrder = 7;
            StartCoroutine(ScaleAndFadeRoutine(clawObj, 0.6f, 1.8f, 0.35f));
        }

        // 3. Rung màn hình
        TriggerCameraShake(0.28f, 0.16f);
    }

    /// <summary>
    /// Hiệu ứng cào vuốt hổ chớp nhoáng
    /// </summary>
    public void PlayTigerClaw(Vector3 center)
    {
        if (tigerClawSprite != null)
        {
            GameObject clawObj = new GameObject("VFX_TigerClaw");
            clawObj.transform.position = center;
            var sr = clawObj.AddComponent<SpriteRenderer>();
            sr.sprite = tigerClawSprite;
            sr.sortingLayerName = "Gameplay";
            sr.sortingOrder = 7;
            StartCoroutine(ScaleAndFadeRoutine(clawObj, 0.6f, 1.8f, 0.35f));
        }
        TriggerCameraShake(0.25f, 0.15f);
    }

    /// <summary>
    /// Hiệu ứng cánh hoa sen / Mugunghwa bay tung tóe
    /// </summary>
    public void PlayLotusPetals(Vector3 center)
    {
        Sprite s = lotusTrailSprite != null ? lotusTrailSprite : mugunghwaPetalSprite;
        if (s != null)
        {
            SpawnDebrisBurst(center, s, count: 6, speed: 3.5f);
        }
        TriggerCameraShake(0.18f, 0.12f);
    }

    /// <summary>
    /// Kích hoạt hiệu ứng khói mây vàng khi đổi người Tag-Team hoặc nhặt vật phẩm
    /// </summary>
    public void PlayGoldenCloud(Vector3 center)
    {
        Sprite cloudSprite = null;
#if UNITY_EDITOR
        cloudSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/vfx_cloud_gold.png");
#endif
        if (cloudSprite == null) cloudSprite = shockwaveSprite;
        if (cloudSprite != null)
        {
            GameObject cloudObj = new GameObject("VFX_GoldenCloud");
            cloudObj.transform.position = new Vector3(center.x, center.y, 0.05f);
            var sr = cloudObj.AddComponent<SpriteRenderer>();
            sr.sprite = cloudSprite;
            sr.sortingLayerName = "Gameplay";
            sr.sortingOrder = 8;
            sr.color = new Color(1f, 0.9f, 0.3f, 0.95f);
            StartCoroutine(ScaleAndFadeRoutine(cloudObj, 0.5f, 1.8f, 0.5f));
        }
    }

    private void SpawnDebrisBurst(Vector3 center, Sprite sprite, int count, float speed)
    {
        if (sprite == null) return;
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            GameObject debris = new GameObject("VFX_Debris");
            debris.transform.position = center + (Vector3)(dir * 0.3f);
            debris.transform.localScale = Vector3.one * Random.Range(0.25f, 0.45f);

            var sr = debris.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingLayerName = "Gameplay";
            sr.sortingOrder = 8;

            StartCoroutine(DebrisFlyRoutine(debris, dir * (speed * Random.Range(0.7f, 1.3f)), duration: 0.5f));
        }
    }

    private IEnumerator DebrisFlyRoutine(GameObject obj, Vector2 velocity, float duration)
    {
        float timer = 0f;
        var sr = obj.GetComponent<SpriteRenderer>();
        Vector3 curPos = obj.transform.position;

        while (timer < duration && obj != null)
        {
            timer += Time.deltaTime;
            curPos += (Vector3)(velocity * Time.deltaTime);
            velocity *= 0.92f; // Giảm tốc tự nhiên
            obj.transform.position = curPos;

            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, timer / duration);
                sr.color = c;
            }
            yield return null;
        }

        if (obj != null) Destroy(obj);
    }

    private IEnumerator ProjectileFlyAndFade(GameObject obj, Vector2 direction, float speed, float duration)
    {
        float timer = 0f;
        var sr = obj.GetComponent<SpriteRenderer>();

        while (timer < duration && obj != null)
        {
            timer += Time.deltaTime;
            obj.transform.position += (Vector3)(direction * (speed * Time.deltaTime));

            // Đẩy văng nhân vật đối diện nếu chạm
            Collider2D[] hits = Physics2D.OverlapCircleAll(obj.transform.position, 0.8f);
            foreach (var h in hits)
            {
                if (h.GetComponent<PlayerController>() != null || h.GetComponent<AIController>() != null)
                {
                    var rb = h.attachedRigidbody;
                    if (rb != null) rb.AddForce(direction * 12.0f, ForceMode2D.Impulse);
                }
            }

            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, timer / duration);
                sr.color = c;
            }
            yield return null;
        }

        if (obj != null) Destroy(obj);
    }

    private IEnumerator ScaleAndFadeRoutine(GameObject obj, float startScale, float endScale, float duration)
    {
        float timer = 0f;
        var sr = obj.GetComponent<SpriteRenderer>();

        while (timer < duration && obj != null)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            float s = Mathf.Lerp(startScale, endScale, progress);
            obj.transform.localScale = new Vector3(s, s, 1f);

            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, progress);
                sr.color = c;
            }
            yield return null;
        }

        if (obj != null) Destroy(obj);
    }

    public void TriggerCameraShake(float duration = 0.3f, float magnitude = 0.2f)
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null) return;

        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(CameraShakeRoutine(duration, magnitude));
    }

    private IEnumerator CameraShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        Vector3 basePos = new Vector3(0, -0.1f, -10f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float damper = 1.0f - Mathf.Clamp01(elapsed / duration);
            float x = Random.Range(-1f, 1f) * magnitude * damper;
            float y = Random.Range(-1f, 1f) * magnitude * damper;

            targetCamera.transform.position = new Vector3(basePos.x + x, basePos.y + y, basePos.z);
            yield return null;
        }

        targetCamera.transform.position = basePos;
        shakeCoroutine = null;
    }
}
