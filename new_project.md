# TÀI LIỆU DỰ ÁN: LỄ HỘI LINH VẬT: ĐẤU ĐẨY HỮU NGHỊ (MYTHIC BUMPER)

## 1. THÔNG TIN CHUNG & BỐI CẢNH DỰ ÁN
- **Mục tiêu:** Tham gia cuộc thi "Lập trình game Korea Game Week 2026 (KGW2026)".
- **Chủ đề:** Giao lưu văn hóa Việt Nam - Hàn Quốc.
- **Thời gian hoàn thiện:** 5 ngày (yêu cầu tối giản, tránh bẫy kỹ thuật, ưu tiên cơ chế vật lý vui nhộn).
- **Nền tảng mục tiêu:** Android (Mobile) & PC Standalone (để test/quay video demo).
- **Engine sử dụng:** Unity 2022 LTS (Physics 2D, Camera Orthographic nhìn từ trên xuống - Top-down 2D).
- **Chế độ chơi mạng (PvP):** Sử dụng thư viện Photon PUN 2 (hoặc Photon Fusion) để kết nối 2 người chơi qua hệ thống tạo/nhập ID phòng.

---

## 2. Ý NGHĨA VĂN HÓA & TRÁNH XUNG ĐỘT (QUY TẮC BẮT BUỘC)
- **Bối cảnh:** Lễ hội thể thao hữu nghị Việt - Hàn. Sới đấu là sự kết hợp giữa sới vật đất nện hội làng truyền thống Việt Nam và sàn đấu cát Ssireum Hàn Quốc.
- **Tính chất gameplay:** Mang tính thể thao dân gian thượng võ, vui tươi, lành mạnh (Casual/Family-friendly).
- **TUYỆT ĐỐI KHÔNG:** Không vũ khí, không đòn đánh, không máu me, không tính sát thương HP.
- **Cơ chế:** Thuần túy là "Đấu đẩy vật lý" (Bumper Arena / Sumo / Slingshot). Người nào bị đẩy ra khỏi ranh giới sới đấu sẽ thua một hiệp. Khi kết thúc, hai bên cùng ăn mừng hữu nghị.

---

## 3. HỆ THỐNG LINH VẬT & CHỈ SỐ (CHARACTERS SPECIFICATIONS)
Mọi nhân vật sử dụng chung kiến trúc `Rigidbody2D` và `CircleCollider2D`, phân biệt bằng thông số vật lý và tuyệt kỹ riêng:

### Phía Việt Nam:
1. **Trâu Vàng (Kim Ngưu - Đô vật hội làng):**
   - Vai trò: Tanker (Phòng thủ/Trụ sân).
   - Thông số: `Mass: 2.0`, `Linear Drag: 1.5`, `Lực bắn tối đa: 12`, `Độ nảy (Bounciness): 0.6`.
   - Tuyệt kỹ: "Kim Ngưu Trấn Địa" -> Dậm đất tạo sóng xung kích vòng tròn hất nhẹ kẻ địch xung quanh và tăng khối lượng (`Mass * 3`) trong 2 giây.
2. **Bé Long (Rồng thời Lý cách tân):**
   - Vai trò: Speedster (Tốc độ/Linh hoạt).
   - Thông số: `Mass: 1.0`, `Linear Drag: 1.0`, `Lực bắn tối đa: 16`, `Độ nảy (Bounciness): 0.9`.
   - Tuyệt kỹ: "Vũ Long Đằng Vân" -> Lao lướt nhanh theo đường uốn lượn, để lại vệt mây vàng làm chậm đối thủ 50% trong 2 giây nếu chạm phải.

### Phía Hàn Quốc:
3. **Hổ Baekho (Bạch Hổ Ssireum):**
   - Vai trò: Bruiser (Cân bằng/Lực đẩy mạnh).
   - Thông số: `Mass: 1.8`, `Linear Drag: 1.4`, `Lực bắn tối đa: 14`, `Độ nảy (Bounciness): 0.7`.
   - Tuyệt kỹ: "Hổ Khiếu Sơn Hà" -> Gầm thét tạo luồng khí đẩy đối diện theo đường thẳng phía trước, hất văng mọi vật thể.
4. **Haechi (Kỳ lân Seoul trừ tà):**
   - Vai trò: Trickster (Lắt léo/Độ nảy cao).
   - Thông số: `Mass: 1.1`, `Linear Drag: 1.2`, `Lực bắn tối đa: 15`, `Độ nảy (Bounciness): 0.85`.
   - Tuyệt kỹ: "Linh Khí Trừ Tà" -> Tạo khiên ánh sáng hình chuông đồng. Khi bị húc trúng, phản lại 150% lực va chạm ngược về kẻ tấn công.

---

## 4. CƠ CHẾ GAMEPLAY CỐT LÕI (CORE GAMEPLAY LOOP) & TÍNH NĂNG NÂNG CAO
1. **Điều khiển người chơi (Slingshot):**
   - Người chơi chạm/nhấp vào linh vật, kéo ngược lại để lấy lực (hiển thị tia định hướng bằng `LineRenderer`).
   - Thả tay: Truyền lực xung (`ForceMode2D.Impulse`) để nhân vật phóng đi.
   - Thêm vệt di chuyển (Trail Renderer) mang họa tiết hoa sen (VN) hoặc hoa Mugunghwa (HQ).
2. **Cơ chế phối hợp cặp đôi (Tag-Team Swap):**
   - Đội chơi mang theo 2 linh vật. Người chơi có thể bấm nút "Swap" (Hồi chiêu 10s) để đổi linh vật đang đứng trên sân nhằm thay đổi chiến thuật.
3. **Vật phẩm Văn hóa rơi ngẫu nhiên (Power-ups):**
   - Rơi ngẫu nhiên từ trên xuống sau mỗi 15-20 giây. Ai chạm trước sẽ nhận bùa lợi trong 5 giây:
     * **Bánh Chưng (VN):** Nhân đôi kích thước và trọng lượng.
     * **Bánh Gạo Tteok (HQ):** Trượt cực nhanh (giảm ma sát).
     * **Nón Lá (VN):** Tạo vòng chắn bảo vệ, kháng 1 lần bị húc văng.
     * **Quạt Lụa Buchaechum (HQ):** Quạt gió nhẹ xung quanh đẩy đối thủ ra xa.
4. **Vòng bo Lễ hội thu nhỏ (Shrinking Arena) & Fever Time:**
   - Trận đấu kéo dài 60 giây. Sau mỗi 20 giây, vòng an toàn thu nhỏ lại 15% (co Collider & Scale sàn đấu).
   - 20 giây cuối: Nhịp trống lễ hội thúc giục dồn dập (Fever Time), nhân đôi lực đẩy (2x Knockback) cho tất cả.
5. **Quy tắc tính điểm (Win/Lose Condition):**
   - Ai bị húc ra ngoài vòng sân 3 lần trước sẽ thua. Trò chơi dừng và hiển thị màn hình kết thúc hữu nghị: 2 bên cùng vui vẻ chia sẻ ẩm thực Việt-Hàn.

---

## 5. HỆ THỐNG PvP CHƠI MẠNG QUA ID PHÒNG (MULTIPLAYER NETWORK)
Sử dụng **Photon PUN 2** để làm multiplayer nhanh gọn:
- **Trạng thái kết nối (Lobby Scene):**
  - Người chơi kết nối vào máy chủ Photon (ConnectUsingSettings).
  - Có 2 ô nhập liệu: Ô "Đặt tên người chơi" và ô "Nhập ID Phòng" (Room ID).
  - Có 2 nút: "Tạo Phòng" (Create Room) và "Tham Gia Phòng" (Join Room) bằng ID phòng đã nhập.
- **Đồng bộ hóa trong game (Game Scene):**
  - Đồng bộ vị trí và lực của 2 Linh vật qua `PhotonView` và `PhotonTransformView`.
  - Master Client (Người tạo phòng) sẽ quản lý việc sinh vật phẩm ngẫu nhiên và vòng bo co lại để tránh lệch dữ liệu (Desync).
  - Khi một người chơi bị đẩy ra ngoài vòng, gửi sự kiện (RPC) để thông báo cho cả hai và trừ điểm.

---

## 6. YÊU CẦU KỸ THUẬT CHO CÁC SCRIPT C# TRONG UNITY
1. **Tính độc lập & tối giản:** Code viết gọn gàng, có chú thích tiếng Việt, kế thừa `MonoBehaviour` (hoặc `MonoBehaviourPunCallbacks` cho mạng).
2. **Khả năng tinh chỉnh Inspector:** Toàn bộ thông số vận tốc, lực kéo, thời gian trễ phải gán `[SerializeField]` hoặc `public`.
3. **Danh sách các file Script cần có:**
   - `LobbyManager.cs`: Kết nối server, tạo/tham gia phòng bằng ID phòng, chuyển Scene.
   - `PlayerController.cs`: Xử lý kéo - thả (Slingshot), vẽ LineRenderer và gửi đồng bộ qua mạng (`photonView.IsMine`).
   - `CharacterSkill.cs`: Xử lý Kích hoạt tuyệt kỹ riêng của từng linh vật khi đầy thanh năng lượng.
   - `CharacterSwap.cs`: Logic đổi người giữa 2 linh vật đã chọn.
   - `ObstacleBumper.cs`: Xử lý độ nảy cho chum/vại trên sân.
   - `PowerUpItem.cs`: Logic cho vật phẩm rơi tự do và kích hoạt buff khi va chạm.
   - `ArenaShrink.cs`: Thu nhỏ bán kính sân đấu theo từng mốc thời gian.
   - `RingOutDetector.cs`: Bắt sự kiện nhân vật văng ra khỏi vòng tròn. Gửi RPC báo điểm.
   - `GameManager.cs`: Quản lý tỉ số, trạng thái trận đấu, giao diện và nút chơi lại.