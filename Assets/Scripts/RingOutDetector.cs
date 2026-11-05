using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Bắt sự kiện linh vật bị húc văng ra khỏi vòng tròn sới đấu (Ring-Out).
/// Dự án: Mythic Bumper (KGW2026).
/// </summary>
public class RingOutDetector : MonoBehaviour
{
    [System.Serializable]
    public class RingOutEvent : UnityEvent<string> { }

    [Header("--- SỰ KIỆN RING OUT ---")]
    [Tooltip("Sự kiện kích hoạt khi có nhân vật rơi khỏi sân (truyền tên tag hoặc loại: Player / Enemy)")]
    public RingOutEvent onRingOut;

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Guard 1: Chặn đứng mọi va chạm nếu không ở trạng thái thi đấu (BattlePlaying)
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.BattlePlaying)
            return;

        // Khi linh vật vượt ra ngoài vòng tròn Collider của đấu trường
        if (collision.GetComponent<PlayerController>() != null || collision.CompareTag("Player"))
        {
            Debug.Log("<color=red>[RING OUT]</color> Người chơi (Trâu Vàng) bị đẩy ra khỏi sới đấu!");
            onRingOut?.Invoke("Player");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMascotRingOut(isPlayer: true);
            }
        }
        else if (collision.GetComponent<AIController>() != null || collision.CompareTag("Enemy"))
        {
            Debug.Log("<color=green>[RING OUT]</color> Đối thủ (Bạch Hổ) bị đẩy ra khỏi sới đấu!");
            onRingOut?.Invoke("Enemy");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMascotRingOut(isPlayer: false);
            }
        }
    }
}
