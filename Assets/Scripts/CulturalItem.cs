using UnityEngine;
using System.Collections;

/// <summary>
/// Quản lý vật phẩm văn hóa xuất hiện trên sới đấu:
/// 1. Bánh Chưng (VN): Tăng Mass và lực húc.
/// 2. Bánh Gạo Tteokbokki (KR): Tăng tốc độ lướt và độ trơn.
/// 3. Nón Lá Pháp Bảo (VN): Tạo giáp bảo vệ giảm lực đẩy.
/// 4. Quạt Lửa Buchaechum (KR): Bộc phát cơn gió đẩy lùi đối thủ.
/// Dự án: Mythic Bumper (KGW2026).
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class CulturalItem : MonoBehaviour
{
    public enum ItemType
    {
        BanhChung,
        BanhGaoTteokbokki,
        NonLaPhapBao,
        QuatLuaBuchaechum
    }

    [Header("--- THÔNG TIN VẬT PHẨM ---")]
    [SerializeField] private ItemType itemType = ItemType.BanhChung;
    [SerializeField] private string itemName = "Bánh Chưng Đất Tổ";
    [SerializeField] private float duration = 6.0f;
    [SerializeField] private float bobbingSpeed = 3.0f;
    [SerializeField] private float bobbingHeight = 0.12f;

    private Vector3 basePosition;
    private bool isCollected = false;

    public void SetItemType(ItemType type)
    {
        itemType = type;
        switch (type)
        {
            case ItemType.BanhChung: itemName = "Bánh Chưng Đất Tổ"; break;
            case ItemType.BanhGaoTteokbokki: itemName = "Bánh Gạo Tteokbokki"; break;
            case ItemType.NonLaPhapBao: itemName = "Nón Lá Pháp Bảo"; break;
            case ItemType.QuatLuaBuchaechum: itemName = "Quạt Lụa Buchaechum"; break;
        }
    }

    private void Start()
    {
        basePosition = transform.position;
        var col = GetComponent<CircleCollider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        if (isCollected) return;

        // Hiệu ứng nhấp nhô lơ lửng vui tươi (Bobbing animation)
        float newY = basePosition.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
        transform.position = new Vector3(basePosition.x, newY, basePosition.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        // Kiểm tra xem là Người chơi hay Đối thủ nhặt
        var player = other.GetComponent<PlayerController>();
        var ai = other.GetComponent<AIController>();
        var rb = other.GetComponent<Rigidbody2D>();

        if (player != null || ai != null)
        {
            isCollected = true;
            ApplyBuff(other.gameObject, rb, player != null);
            PlayPickupEffect();
            Destroy(gameObject, 0.1f);
        }
    }

    private void ApplyBuff(GameObject target, Rigidbody2D rb, bool isPlayer)
    {
        string receiverName = isPlayer ? "Người chơi (Việt Nam)" : "Đối thủ (Hàn Quốc)";

        switch (itemType)
        {
            case ItemType.BanhChung:
                Debug.Log($"<color=green>[VẬT PHẨM]</color> {receiverName} nhặt được BÁNH CHƯNG! Tăng 50% Mass & Sức đẩy trong {duration}s!");
                StartCoroutine(BuffMassRoutine(rb, 1.5f));
                break;

            case ItemType.BanhGaoTteokbokki:
                Debug.Log($"<color=orange>[VẬT PHẨM]</color> {receiverName} nhặt được BÁNH GẠO TTEOKBOKKI! Tốc độ phóng siêu nhanh!");
                if (rb != null) rb.linearVelocity *= 1.4f;
                break;

            case ItemType.NonLaPhapBao:
                Debug.Log($"<color=yellow>[VẬT PHẨM]</color> {receiverName} trang bị NÓN LÁ PHÁP BẢO! Giảm lực va chạm!");
                StartCoroutine(BuffShieldRoutine(rb));
                break;

            case ItemType.QuatLuaBuchaechum:
                Debug.Log($"<color=cyan>[VẬT PHẨM]</color> {receiverName} vung QUẠT LỬA BUCHAECHUM! Thổi bay mọi đối thủ lân cận!");
                PushNearbyOpponents(target.transform.position);
                break;
        }
    }

    private IEnumerator BuffMassRoutine(Rigidbody2D rb, float multiplier)
    {
        if (rb == null) yield break;
        float original = rb.mass;
        rb.mass *= multiplier;
        yield return new WaitForSeconds(duration);
        if (rb != null) rb.mass = original;
    }

    private IEnumerator BuffShieldRoutine(Rigidbody2D rb)
    {
        if (rb == null) yield break;
        float originalDamp = rb.linearDamping;
        rb.linearDamping += 0.8f;
        yield return new WaitForSeconds(duration);
        if (rb != null) rb.linearDamping = originalDamp;
    }

    private void PushNearbyOpponents(Vector3 origin)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, 3.5f);
        foreach (var hit in hits)
        {
            if (hit.gameObject != gameObject && hit.attachedRigidbody != null)
            {
                Vector2 dir = (hit.transform.position - origin).normalized;
                hit.attachedRigidbody.AddForce(dir * 10f, ForceMode2D.Impulse);
            }
        }
    }

    private void PlayPickupEffect()
    {
        var vfx = Object.FindAnyObjectByType<VFXManager>();
        if (vfx != null)
        {
            vfx.PlayGoldenCloud(transform.position);
        }
    }
}
