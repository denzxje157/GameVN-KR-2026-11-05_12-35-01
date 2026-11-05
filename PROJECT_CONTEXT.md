# TÀI LIỆU DỰ ÁN: LỄ HỘI LINH VẬT: ĐẤU ĐẨY HỮU NGHỊ (MYTHIC BUMPER)

## 1. THÔNG TIN CHUNG & BỐI CẢNH DỰ ÁN
- **Mục tiêu:** Tham gia cuộc thi "Lập trình game Korea Game Week 2026 (KGW2026)".
- **Chủ đề:** Giao lưu văn hóa Việt Nam - Hàn Quốc.
- **Thời gian hoàn thiện:** 5 ngày (yêu cầu tối giản, tránh bẫy kỹ thuật, không làm phức tạp hóa vấn đề).
- **Nền tảng mục tiêu:** Android (Mobile) & PC Standalone (để test/quay video demo).
- **Engine sử dụng:** Unity 2022 LTS (Physics 2D, Camera Orthographic nhìn từ trên xuống - Top-down).

---

## 2. Ý NGHĨA VĂN HÓA & TRÁNH XUNG ĐỘT (QUY TẮC BẮT BUỘC)
- **Bối cảnh:** Lễ hội thể thao hữu nghị Việt - Hàn. Sới đấu là sự kết hợp giữa sới vật đất nện hội làng truyền thống Việt Nam và sàn đấu cát Ssireum Hàn Quốc.
- **Tính chất gameplay:** Mang tính thể thao dân gian thượng võ, vui tươi, lành mạnh (Casual/Family-friendly).
- **TUYỆT ĐỐI KHÔNG:** Không vũ khí, không đòn đánh, không máu me, không tính sát thương HP.
- **Cơ chế:** Thuần túy là "Đấu đẩy vật lý" (Bumper Arena / Sumo / Slingshot). Người nào bị đẩy ra khỏi ranh giới sới đấu sẽ thua một hiệp. Khi kết thúc, hai bên cùng ăn mừng hữu nghị.

---

## 3. HỆ THỐNG LINH VẬT (CHARACTERS SPECIFICATIONS)
Mọi nhân vật sử dụng chung kiến trúc `Rigidbody2D` và `CircleCollider2D`, phân biệt bằng thông số vật lý:

### Phía Việt Nam:
1. **Trâu Vàng (Kim Ngưu - Đô vật hội làng):**
   - Vai trò: Tanker (Phòng thủ/Trụ sân).
   - Thông số: `Mass: 2.0`, `Linear Drag: 1.5`, `Lực bắn tối đa: 12`, `Độ nảy (Bounciness): 0.6`.
2. **Bé Long (Rồng thời Lý cách tân):**
   - Vai trò: Speedster (Tốc độ/Linh hoạt).
   - Thông số: `Mass: 1.0`, `Linear Drag: 1.0`, `Lực bắn tối đa: 16`, `Độ nảy (Bounciness): 0.9`.

### Phía Hàn Quốc:
3. **Hổ Baekho (Bạch Hổ Ssireum):**
   - Vai trò: Bruiser (Cân bằng/Lực đẩy mạnh).
   - Thông số: `Mass: 1.8`, `Linear Drag: 1.4`, `Lực bắn tối đa: 14`, `Độ nảy (Bounciness): 0.7`.
4. **Haechi (Kỳ lân Seoul trừ tà):**
   - Vai trò: Trickster (Lắt léo/Độ nảy cao).
   - Thông số: `Mass: 1.1`, `Linear Drag: 1.2`, `Lực bắn tối đa: 15`, `Độ nảy (Bounciness): 0.85`.

---

## 4. CƠ CHẾ GAMEPLAY CỐT LÕI (CORE GAMEPLAY LOOP)
1. **Điều khiển người chơi (Slingshot):**
   - Người chơi chạm/nhấp vào linh vật, kéo ngược lại để lấy lực (hiển thị tia định hướng bằng `LineRenderer`).
   - Thả tay: Truyền lực xung (`ForceMode2D.Impulse`) để nhân vật phóng đi.
2. **Trí tuệ nhân tạo (AI Đối thủ):**
   - Tự động xoay về hướng người chơi sau mỗi khoảng thời gian $T$ giây (có dao động ngẫu nhiên để không quá hoàn hảo).
   - Tự nạp lực và phóng về phía người chơi.
3. **Chướng ngại vật ngẫu nhiên trên sân:**
   - **Vại gốm cổ / Chum Kimchi:** Đứng yên, độ nảy cực cao (`Bounciness = 1.2`), đóng vai trò như Bumper trong bàn Pinball.
   - **Vũng bùn hội làng:** Làm giảm 50% vận tốc (`velocity *= 0.5f`) khi đi qua.
4. **Quy tắc tính điểm (Win/Lose Condition):**
   - Có một vùng Trigger bên ngoài sới đấu (`RingOutArea`).
   - Ai chạm vào vùng này 3 lần trước sẽ thua cuộc.
   - Hiển thị màn hình kết thúc với thông điệp giao lưu văn hóa hữu nghị.

---

## 5. YÊU CẦU KỸ THUẬT CHO CÁC SCRIPT C# TRONG UNITY
Khi viết hoặc sửa code cho dự án này, Antigravity CẦN TUÂN THỦ:
1. **Tính độc lập & tối giản:** Code viết gọn gàng, kế thừa từ `MonoBehaviour`, không dùng plugin ngoài phức tạp.
2. **Khả năng tinh chỉnh Inspector:** Toàn bộ thông số vận tốc, lực kéo, thời gian trễ phải gán `[SerializeField]` hoặc `public`.
3. **Vật lý 2D chuẩn:** Sử dụng hàm va chạm `OnCollisionEnter2D` và `OnTriggerEnter2D`.
4. **Quản lý danh sách Script cần có:**
   - `PlayerController.cs`: Xử lý kéo - thả (Slingshot) và vẽ LineRenderer.
   - `AIController.cs`: Logic cho bot tự ngắm và lao vào người chơi.
   - `ObstacleBumper.cs`: Xử lý độ nảy cho chum/vại trên sân.s
   - `RingOutDetector.cs`: Bắt sự kiện nhân vật văng ra khỏi vòng tròn.
   - `GameManager.cs`: Quản lý tỉ số, trạng thái trận đấu (Start, Playing, Win, Lose), nút Restart.