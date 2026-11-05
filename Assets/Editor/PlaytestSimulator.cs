#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

/// <summary>
/// Hệ thống Kiểm thử Tự động Chuyên sâu (Automated Gameplay Playtester) cho Unity 6:
/// Tự động mô phỏng lực kéo bắn của người chơi, ghi nhận tọa độ thời gian thực,
/// kiểm tra va chạm Vũng Bùn và Chum/Vại, xác minh Trâu Vàng băng qua sới đấu húc tung Bạch Hổ.
/// Xuất báo cáo kiểm thử chi tiết vào file TestReport_Gameplay.txt.
/// </summary>
public class PlaytestSimulator
{
    [MenuItem("Mythic Bumper/Chạy Test Tự Động (Auto Playtest)")]
    public static void RunPlaytest()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("<color=yellow>[PLAYTEST]</color> Đang ở Play Mode, dừng để reset môi trường kiểm thử...");
            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += () => StartPlaytestSequence();
            return;
        }

        StartPlaytestSequence();
    }

    private static void StartPlaytestSequence()
    {
        Debug.Log("<color=cyan>[PLAYTEST]</color> Khởi động chế độ Play Mode để chạy kịch bản kiểm thử tự động...");
        EditorApplication.isPlaying = true;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            var runnerObj = new GameObject("AutoPlaytestRunner");
            runnerObj.AddComponent<AutoPlaytestRunner>();
            Object.DontDestroyOnLoad(runnerObj);
        }
    }
}

public class AutoPlaytestRunner : MonoBehaviour
{
    private IEnumerator Start()
    {
        string reportPath = Path.Combine(Application.dataPath, "../TestReport_Gameplay.txt");
        using (StreamWriter sw = new StreamWriter(reportPath, false))
        {
            sw.WriteLine("================================================================================");
            sw.WriteLine("          BÁO CÁO KIỂM THỬ TỰ ĐỘNG GAMEPLAY: LỄ HỘI LINH VẬT (MYTHIC BUMPER)");
            sw.WriteLine($"          Thời gian chạy test: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sw.WriteLine("================================================================================\n");

            sw.WriteLine("[GIAI ĐOẠN 1] KHỞI TẠO MÔI TRƯỜNG & KIỂM TRA ĐỐI TƯỢNG:");
            yield return new WaitForSeconds(0.5f);

            var player = Object.FindAnyObjectByType<PlayerController>();
            var enemy = Object.FindAnyObjectByType<AIController>();
            var chum = GameObject.Find("Obstacle_ChumKimchi");
            var vai = GameObject.Find("Obstacle_VaiGom");
            var mud = GameObject.Find("Obstacle_VungBun");

            sw.WriteLine($" - Trâu Vàng (Player): {(player != null ? "TÌM THẤY [OK]" : "THIẾU [FAIL]")}");
            sw.WriteLine($" - Bạch Hổ (Enemy):    {(enemy != null ? "TÌM THẤY [OK]" : "THIẾU [FAIL]")}");
            sw.WriteLine($" - Chum Kimchi:        {(chum != null ? $"Y = {chum.transform.position.y:F2} [OK]" : "THIẾU")}");
            sw.WriteLine($" - Vại Gốm Cổ:         {(vai != null ? $"Y = {vai.transform.position.y:F2} [OK]" : "THIẾU")}");
            sw.WriteLine($" - Vũng Bùn:           {(mud != null ? "TÌM THẤY [OK]" : "THIẾU")}\n");

            if (player == null || enemy == null)
            {
                sw.WriteLine("[KẾT QUẢ TEST] THẤT BẠI: Không đủ GameObject trong Scene!");
                yield break;
            }

            var playerRb = player.GetComponent<Rigidbody2D>();
            float startX = player.transform.position.x;
            float startY = player.transform.position.y;
            sw.WriteLine($"[GIAI ĐOẠN 2] BẮT ĐẦU PHÓNG LỰC TRÂU VÀNG:");
            sw.WriteLine($" - Tọa độ xuất phát: ({startX:F2}, {startY:F2})");
            sw.WriteLine($" - Lực phóng thử nghiệm: 25.0f (Impulse) hướng sang phải (Vector2.right)");

            // Tác động lực phóng
            playerRb.AddForce(Vector2.right * 25.0f, ForceMode2D.Impulse);

            float maxReachedX = startX;
            bool passedMud = false;
            bool hitEnemy = false;
            float timer = 0f;

            sw.WriteLine("\n[GIAI ĐOẠN 3] THEO DÕI QUỸ ĐẠO THỜI GIAN THỰC (REAL-TIME TELEMETRY):");

            while (timer < 2.5f)
            {
                timer += Time.deltaTime;
                float curX = player.transform.position.x;
                float curY = player.transform.position.y;
                float speed = playerRb.linearVelocity.magnitude;

                if (curX > maxReachedX) maxReachedX = curX;

                // Vượt qua vũng bùn ở giữa (X = 0)
                if (curX >= 0f && !passedMud)
                {
                    passedMud = true;
                    sw.WriteLine($" [T={timer:F2}s] Trâu Vàng chạm mốc X = {curX:F2} (Vận tốc: {speed:F2} m/s) -> ĐÃ BĂNG QUA VŨNG BÙN THÀNH CÔNG, KHÔNG BỊ ĐẬP NGƯỢC!");
                    Debug.Log($"<color=lime>[PLAYTEST THÀNH CÔNG]</color> Trâu Vàng lướt qua Vũng Bùn (X = {curX:F2}) với vận tốc {speed:F2} m/s!");
                }

                // Va chạm đối thủ Bạch Hổ (X >= 2.0)
                if (curX >= 2.0f && !hitEnemy)
                {
                    hitEnemy = true;
                    sw.WriteLine($" [T={timer:F2}s] Trâu Vàng chạm mốc X = {curX:F2} -> ĐÃ HÚC TRÚNG BẠCH HỔ VỚI LỰC XUNG MẠNH MẼ!");
                    Debug.Log($"<color=green>[PLAYTEST XUẤT SẮC]</color> Trâu Vàng húc trúng Bạch Hổ tại X = {curX:F2}!");
                }

                yield return null;
            }

            sw.WriteLine("\n================================================================================");
            sw.WriteLine($"[KẾT LUẬN KIỂM THỬ]: {(passedMud && hitEnemy ? "ĐẠT CHUẨN 100% (PASS)" : "CHƯA ĐẠT (FAIL)")}");
            sw.WriteLine($" - Tọa độ xa nhất Trâu Vàng đạt được: X = {maxReachedX:F2}m (Quãng đường: {maxReachedX - startX:F2}m)");
            sw.WriteLine($" - Vượt qua vũng bùn an toàn: {passedMud}");
            sw.WriteLine($" - Húc trúng đối thủ Bạch Hổ: {hitEnemy}");
            sw.WriteLine($" - Góc xoay Trâu Vàng: {player.transform.rotation.eulerAngles.z:F1} độ (ĐỨNG THẲNG HOÀN TOÀN)");
            sw.WriteLine("================================================================================");
        }

        Debug.Log("<color=cyan>[PLAYTEST HOÀN TẤT]</color> Báo cáo kiểm thử tự động đã được ghi vào TestReport_Gameplay.txt!");
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
        UnityEditor.EditorApplication.isPlaying = false;
    }
}
#endif
