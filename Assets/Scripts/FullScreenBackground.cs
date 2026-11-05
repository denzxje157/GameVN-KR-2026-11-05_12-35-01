using UnityEngine;

/// <summary>
/// Tự động căn chỉnh Scale của ảnh nền để luôn lấp đầy (Aspect Fill) góc nhìn Camera Orthographic,
/// loại bỏ hoàn toàn viền đen 4 góc trên mọi tỉ lệ màn hình điện thoại (16:9, 18:9, 19.5:9, 21:9).
/// Có bổ sung 10% lề an toàn để khi rung Camera (Screen Shake) không bị lộ mép.
/// Dự án: Mythic Bumper (KGW2026).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[ExecuteAlways]
public class FullScreenBackground : MonoBehaviour
{
    [SerializeField] private float safetyMargin = 1.12f; // 12% lề an toàn chống viền đen khi rung lắc

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        FitToScreen();
    }

    private void Start()
    {
        FitToScreen();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
        {
            FitToScreen();
        }
    }
#endif

    public void FitToScreen()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
        if (cam == null || !cam.orthographic) return;

        // Chiều cao và chiều rộng góc nhìn của Camera trong không gian thế giới
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        camHeight *= safetyMargin;
        camWidth *= safetyMargin;

        // Kích thước thế giới thực của Sprite
        Vector2 spriteSize = sr.sprite.rect.size / sr.sprite.pixelsPerUnit;
        if (spriteSize.x <= 0 || spriteSize.y <= 0) return;

        float scaleX = camWidth / spriteSize.x;
        float scaleY = camHeight / spriteSize.y;

        // Lấy hệ số phóng to lớn hơn để lấp đầy cả chiều ngang lẫn dọc (Aspect Fill)
        float maxScale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(maxScale, maxScale, 1f);
        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
    }
}
