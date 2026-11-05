using UnityEngine;
using System.Collections;

/// <summary>
/// Xử lý độ nảy cực mạnh (Bounciness = 1.2) cho Chum Kimchi & Vại gốm cổ.
/// Đóng vai trò như Bumper trong bàn Pinball, tạo cảm giác va chạm nảy đã tay.
/// Dự án: Mythic Bumper (KGW2026).
/// </summary>
public class ObstacleBumper : MonoBehaviour
{
    [Header("--- THÔNG SỐ ĐỘ NẢY BUMPER ---")]
    [Tooltip("Hệ số nhân lực phản chấn khi bị va chạm")]
    [SerializeField] private float bounceMultiplier = 1.35f;

    [Tooltip("Lực nảy tối thiểu đảm bảo linh vật luôn bị hất văng")]
    [SerializeField] private float minimumBounceForce = 8.0f;

    [Header("--- HIỆU ỨNG RUNG NẢY (SQUASH & STRETCH) ---")]
    [SerializeField] private float bounceScaleIntensity = 1.25f;
    [SerializeField] private float bounceDuration = 0.2f;

    private Vector3 originalScale;
    private Coroutine bounceRoutine;

    private void Awake()
    {
        originalScale = transform.localScale;

        // Đảm bảo Collider có PhysicsMaterial2D siêu nảy
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            var mat = new PhysicsMaterial2D($"{gameObject.name}_BumperMat")
            {
                bounciness = 1.2f, // Độ nảy cực cao theo PROJECT_CONTEXT.md
                friction = 0.05f
            };
            col.sharedMaterial = mat;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D hitRb = collision.rigidbody;
        if (hitRb != null)
        {
            // Vector pháp tuyến từ tâm bumper hướng ra ngoài
            Vector2 normal = (collision.transform.position - transform.position).normalized;
            if (normal == Vector2.zero) normal = -collision.relativeVelocity.normalized;

            // Tính toán lực phản xung
            float incomingSpeed = collision.relativeVelocity.magnitude;
            float bounceForce = Mathf.Max(incomingSpeed * bounceMultiplier, minimumBounceForce);

            // Tác động lực xung hất văng linh vật
            hitRb.AddForce(normal * bounceForce, ForceMode2D.Impulse);

            // Kích hoạt hoạt ảnh rung nảy
            if (bounceRoutine != null) StopCoroutine(bounceRoutine);
            bounceRoutine = StartCoroutine(BounceAnimationRoutine());
        }
    }

    private IEnumerator BounceAnimationRoutine()
    {
        float timer = 0f;
        while (timer < bounceDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / bounceDuration;
            // Hiệu ứng phồng to rồi co về ban đầu
            float scale = Mathf.Lerp(bounceScaleIntensity, 1f, progress);
            transform.localScale = originalScale * scale;
            yield return null;
        }
        transform.localScale = originalScale;
    }
}
