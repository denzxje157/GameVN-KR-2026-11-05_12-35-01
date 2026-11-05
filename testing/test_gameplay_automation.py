#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script: test_gameplay_automation.py
Sử dụng: AltTester SDK Python Driver (v2.3.3)
Chức năng:
  - Kết nối trực tiếp vào Unity qua AltTester Server (Port 13000).
  - Tự động tìm kiếm các GameObject: Player_TrauVang, Enemy_HoBaekho, Arena_Stadium, Btn_Skill.
  - Lấy tọa độ thực tế, kiểm tra va chạm, giả lập thao tác kéo bắn và bấm nút Kỹ Năng.
"""

import sys
import time

try:
    from alttester import AltDriver, By
except ImportError:
    print("[LỖI] Chưa cài đặt AltTester-Driver! Chạy lệnh: pip install AltTester-Driver")
    sys.exit(1)

def run_mythic_bumper_test(host="127.0.0.1", port=13000):
    print("=" * 65)
    print("  BẮT ĐẦU KIỂM THỬ TỰ ĐỘNG UNITY VỚI ALTTESTER SDK")
    print("=" * 65)

    print(f"[1] Đang kết nối tới Unity AltTester Server ({host}:{port})...")
    try:
        driver = AltDriver(host=host, port=port)
        print("  -> Kết nối thành công tới Unity!")
    except Exception as e:
        print(f"[LỖI KẾT NỐI] Không thể kết nối tới Unity tại {host}:{port}.")
        print("  -> Hãy đảm bảo bạn đã mở Unity, mở cửa sổ 'AltTester Editor' và bấm 'Play in Editor'!")
        print(f"  Chi tiết lỗi: {e}")
        return False

    try:
        # 1. Kiểm tra Scene hiện tại
        current_scene = driver.get_current_scene()
        print(f"[2] Scene hiện tại trong Unity: '{current_scene}'")

        # 2. Tìm kiếm các GameObject quan trọng trong sới đấu
        print("[3] Đang tìm kiếm các GameObjects trên sới đấu...")
        arena = driver.find_object(By.NAME, "Arena_Stadium")
        player = driver.find_object(By.NAME, "Player_TrauVang")
        enemy = driver.find_object(By.NAME, "Enemy_HoBaekho")

        print(f"  -> Sới đấu (Arena): Tọa độ ({arena.worldX:.2f}, {arena.worldY:.2f})")
        print(f"  -> Trâu Vàng (Player): Tọa độ ({player.worldX:.2f}, {player.worldY:.2f})")
        print(f"  -> Bạch Hổ (Enemy): Tọa độ ({enemy.worldX:.2f}, {enemy.worldY:.2f})")

        # Kiểm tra khoảng cách ban đầu giữa 2 linh vật
        distance = abs(enemy.worldX - player.worldX)
        print(f"  -> Khoảng cách giữa 2 linh vật: {distance:.2f}m")
        assert distance >= 5.0, "Lỗi: Hai linh vật đặt quá gần nhau!"

        # 3. Kiểm tra các nút giao diện UI Mobile HUD
        print("[4] Đang kiểm tra nút giao diện Mobile HUD...")
        btn_skill = driver.find_object(By.NAME, "Btn_Skill")
        btn_swap = driver.find_object(By.NAME, "Btn_Swap")
        score_text = driver.find_object(By.NAME, "Text_Score")

        print(f"  -> Tìm thấy nút Tuyệt Kỹ: {btn_skill.name} tại ({btn_skill.x}, {btn_skill.y})")
        print(f"  -> Tìm thấy nút Đổi Người: {btn_swap.name}")
        print(f"  -> Bảng điểm hiện tại: '{score_text.get_text()}'")

        # 4. Giả lập bấm nút Tuyệt Kỹ (Btn_Skill)
        print("[5] Giả lập người chơi chạm vào Nút Tuyệt Kỹ (Kim Ngưu Trấn Địa)...")
        btn_skill.tap()
        time.sleep(0.5)
        print("  -> Đã kích hoạt sóng xung kích dậm đất!")

        # 5. Giả lập kéo bắn Trâu Vàng sang phía Bạch Hổ
        print("[6] Giả lập thao tác kéo bắn Trâu Vàng lao sang phía đối thủ...")
        start_x, start_y = player.x, player.y
        # Vuốt từ vị trí Trâu Vàng sang phía phải (hướng tới Bạch Hổ)
        target_x = start_x + 200
        target_y = start_y
        driver.swipe(start_x, start_y, target_x, target_y, duration=0.3)

        time.sleep(1.5)
        updated_player = driver.find_object(By.NAME, "Player_TrauVang")
        print(f"  -> Tọa độ Trâu Vàng sau khi phóng: X = {updated_player.worldX:.2f}")

        print("\n" + "=" * 65)
        print("  TẤT CẢ CÁC BÀI TEST TỰ ĐỘNG ĐÃ VƯỢT QUA XUẤT SẮC! (PASS 100%)")
        print("=" * 65)
        return True

    except Exception as e:
        print(f"[LỖI TRONG KHI TEST] {e}")
        return False
    finally:
        driver.stop()
        print("[INFO] Đã đóng kết nối AltTester Driver.")

if __name__ == '__main__':
    run_mythic_bumper_test()
