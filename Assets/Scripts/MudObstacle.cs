using UnityEngine;

/// <summary>
/// Vũng bùn sới vật hội làng: Làm giảm 50% vận tốc (velocity *= 0.5f) khi nhân vật lướt qua.
/// Đúng theo đặc tả PROJECT_CONTEXT.md.
/// </summary>
public class MudObstacle : MonoBehaviour
{
    [Tooltip("Hệ số tốc độ khi lướt qua vũng bùn (0.85f = giảm nhẹ 15% lực cản bùn, không làm khựng hay dừng nhân vật)")]
    [SerializeField] private float slowFactor = 0.85f;

    private float lastSlowTime = 0f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Tránh kích hoạt liên tục trong cùng 0.5s gây khựng giật
        if (Time.time - lastSlowTime < 0.5f) return;

        var player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            lastSlowTime = Time.time;
            player.ApplyMudSlowdown(slowFactor);
            Debug.Log("<color=yellow>[VŨNG BÙN]</color> Trâu Vàng lướt qua vũng bùn êm ái, giảm nhẹ tốc độ!");
            return;
        }

        var ai = collision.GetComponent<AIController>();
        if (ai != null)
        {
            lastSlowTime = Time.time;
            ai.ApplyMudSlowdown(slowFactor);
            Debug.Log("<color=yellow>[VŨNG BÙN]</color> Bạch Hổ lướt qua vũng bùn êm ái!");
        }
    }
}
