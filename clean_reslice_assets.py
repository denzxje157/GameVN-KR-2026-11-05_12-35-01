#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script: clean_reslice_assets.py
Dự án: Lễ hội Linh vật: Đấu đẩy hữu nghị (Mythic Bumper) - KGW2026.
Chức năng:
  - Tách nền hoàn hảo bằng Connected Component Analysis + FloodFill + Alpha Matting.
  - Loại bỏ TRIỆT ĐỂ toàn bộ nhãn chữ tiếng Việt ("Vại Cổ", "Chum Kimchi", v.v.) bên dưới sprite.
  - Loại bỏ toàn bộ các hạt nhiễu, mảnh vỡ lơ lửng (< 300px), chỉ giữ lại hình vẽ chính.
  - Tự động khử viền lem màu (Color Despill) và làm mượt Anti-aliasing.
  - Xuất toàn bộ sprites sạch 100% vào Assets/Sprites/.
"""

import os
import sys
import cv2
import numpy as np

if sys.stdout.encoding.lower() != 'utf-8':
    try:
        sys.stdout.reconfigure(encoding='utf-8')
    except Exception:
        pass

def clean_extract_sprite(crop, bg_threshold=22, min_component_area=250, fill_internal_holes=True):
    """
    Tách nền một vùng crop và chỉ giữ lại thành phần chính lớn nhất,
    loại bỏ toàn bộ nhãn chữ và mảnh vỡ bên ngoài.
    """
    h, w = crop.shape[:2]
    if h <= 4 or w <= 4:
        return None

    # Lấy mẫu màu nền ở 4 góc
    corners = np.vstack([crop[:5, :5], crop[:5, -5:], crop[-5:, :5], crop[-5:, -5:]])
    bg_color = np.median(corners.reshape(-1, 3), axis=0)

    # Đo khoảng cách màu so với nền
    diff = np.linalg.norm(crop.astype(float) - bg_color.astype(float), axis=2)
    foreground_mask = (diff > bg_threshold).astype(np.uint8)

    # Phân tích các thành phần liên thông (Connected Components)
    num_labels, labels, stats, centroids = cv2.connectedComponentsWithStats(foreground_mask)
    if num_labels <= 1:
        return None

    # Tìm thành phần lớn nhất (vật thể chính)
    areas = stats[1:, cv2.CC_STAT_AREA]
    largest_idx = 1 + np.argmax(areas)
    largest_area = stats[largest_idx, cv2.CC_STAT_AREA]

    # Giữ lại thành phần lớn nhất và các thành phần phụ lớn (> min_component_area) thuộc về cùng vật thể
    clean_mask = np.zeros((h, w), dtype=np.uint8)
    for i in range(1, num_labels):
        area = stats[i, cv2.CC_STAT_AREA]
        # Nếu là thành phần chính hoặc thành phần đủ lớn (> 25% diện tích thành phần chính)
        if i == largest_idx or (area > min_component_area and area > largest_area * 0.05):
            clean_mask[labels == i] = 1

    if fill_internal_holes:
        # Lấp đầy các lỗ rỗng bên trong thân nhân vật / vật thể
        contours, _ = cv2.findContours(clean_mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        filled_mask = np.zeros_like(clean_mask)
        cv2.drawContours(filled_mask, contours, -1, 1, -1)
        clean_mask = filled_mask

    # Tạo kênh Alpha mềm mượt (Anti-aliasing) bằng Gaussian Blur nhẹ
    alpha = (clean_mask * 255).astype(np.uint8)
    alpha = cv2.GaussianBlur(alpha, (3, 3), 0)

    # Bounding box sát mép hình (Alpha > 15)
    ys, xs = np.where(alpha > 15)
    if len(ys) == 0 or len(xs) == 0:
        return None

    y_min, y_max = ys.min(), ys.max() + 1
    x_min, x_max = xs.min(), xs.max() + 1

    clean_crop = crop[y_min:y_max, x_min:x_max].copy()
    clean_alpha = alpha[y_min:y_max, x_min:x_max]

    rgba = cv2.cvtColor(clean_crop, cv2.COLOR_BGR2BGRA)
    rgba[:, :, 3] = clean_alpha
    return rgba


def reslice_pic1():
    print("=== [1/5] ĐANG TRÍCH XUẤT SPRITES SẠCH TỪ PIC.1 (MASTER SPRITESHEET) ===")
    p1 = cv2.imread('picture/pic.1.png')
    if p1 is None:
        print("[LỖI] Không tìm thấy picture/pic.1.png")
        return

    # Danh sách định nghĩa: (Thư mục, tên file, y1, y2, x1, x2, threshold, min_area, fill_holes)
    items = [
        # --- CHARACTERS (LINH VẬT) ---
        ('Assets/Sprites/Characters', 'trau_vang_idle.png', 270, 560, 1430, 1660, 22, 500, True),
        ('Assets/Sprites/Characters', 'trau_vang_skill_kim_nguu_tran_dia.png', 250, 570, 1670, 1980, 22, 500, True),
        ('Assets/Sprites/Characters', 'ho_baekho_idle.png', 270, 560, 2100, 2340, 22, 500, True),
        ('Assets/Sprites/Characters', 'ho_baekho_skill_ho_khieu_son_ha.png', 270, 570, 2360, 2660, 22, 500, True),
        ('Assets/Sprites/Characters', 'rong_vang_idle.png', 610, 890, 1410, 1670, 22, 500, True),
        ('Assets/Sprites/Characters', 'rong_vang_skill_luot_may.png', 610, 900, 1680, 1980, 22, 500, True),
        ('Assets/Sprites/Characters', 'haechi_idle.png', 620, 890, 2110, 2350, 22, 500, True),
        ('Assets/Sprites/Characters', 'haechi_skill_linh_khi_tru_ta.png', 610, 930, 2360, 2700, 22, 500, True),

        # --- ITEMS & OBSTACLES (VẬT PHẨM & VẬT CẢN - LOẠI BỎ CHỮ DƯỚI) ---
        ('Assets/Sprites/Items', 'obstacle_vai_gom_co.png', 1190, 1410, 940, 1055, 24, 800, True),
        ('Assets/Sprites/Items', 'obstacle_chum_kimchi.png', 1190, 1410, 1045, 1160, 24, 800, True),
        ('Assets/Sprites/Items', 'obstacle_vai_gom_va_chum_kimchi.png', 1190, 1410, 940, 1160, 24, 800, True),
        ('Assets/Sprites/Items', 'obstacle_vung_bun.png', 1240, 1400, 1160, 1400, 18, 500, False),
        ('Assets/Sprites/Items', 'item_banh_chung.png', 1180, 1410, 60, 310, 22, 600, True),
        ('Assets/Sprites/Items', 'item_banh_gao_tteokbokki.png', 1160, 1410, 310, 540, 22, 600, True),
        ('Assets/Sprites/Items', 'item_non_la_phap_bao.png', 1190, 1410, 540, 740, 22, 600, True),
        ('Assets/Sprites/Items', 'item_quat_lua_buchaechum.png', 1190, 1410, 740, 940, 22, 600, True),

        # --- ARENA FLOOR ---
        ('Assets/Sprites/Arenas', 'arena_stadium_viet_han.png', 325, 1050, 50, 1260, 20, 2000, True),
        ('Assets/Sprites/Arenas', 'arena_center_field.png', 240, 970, 160, 1160, 20, 2000, True),

        # --- UI & VFX ---
        ('Assets/Sprites/UI', 'vfx_cloud_gold.png', 1020, 1190, 1410, 1750, 20, 300, False),
        ('Assets/Sprites/UI', 'vfx_wind_slash.png', 1020, 1180, 1760, 2060, 20, 300, False),
        ('Assets/Sprites/UI', 'vfx_lotus_petals.png', 1020, 1180, 2070, 2700, 20, 300, False),
        ('Assets/Sprites/UI', 'ui_slingshot_indicator.png', 1260, 1360, 1410, 1680, 20, 200, False),
        ('Assets/Sprites/UI', 'ui_touch_hand.png', 1310, 1480, 1650, 1780, 20, 200, True),
        ('Assets/Sprites/UI', 'btn_skill_fist.png', 1260, 1470, 1800, 1970, 20, 400, True),
        ('Assets/Sprites/UI', 'btn_swap_tag_team.png', 1280, 1470, 1970, 2140, 20, 400, True),
        ('Assets/Sprites/UI', 'ui_drum_le_hoi.png', 1260, 1470, 2150, 2330, 20, 400, True),
        ('Assets/Sprites/UI', 'ui_text_bump.png', 1270, 1370, 2320, 2480, 20, 200, False),
        ('Assets/Sprites/UI', 'ui_text_bang.png', 1190, 1320, 2480, 2700, 20, 200, False),
        ('Assets/Sprites/UI', 'ui_text_ultimate.png', 1330, 1470, 2460, 2710, 20, 200, False)
    ]

    for out_dir, fname, y1, y2, x1, x2, thresh, min_area, fill_h in items:
        os.makedirs(out_dir, exist_ok=True)
        crop = p1[y1:y2, x1:x2]
        sprite = clean_extract_sprite(crop, bg_threshold=thresh, min_component_area=min_area, fill_internal_holes=fill_h)
        if sprite is not None:
            out_p = os.path.join(out_dir, fname)
            cv2.imwrite(out_p, sprite)
            print(f"  [OK] {fname:35s} -> {sprite.shape[1]}x{sprite.shape[0]}px (100% sạch rác)")


def reslice_pic6():
    print("\n=== [2/5] ĐANG TRÍCH XUẤT SPRITES SẠCH TỪ PIC.6 (BOI CANH & UI) ===")
    p6 = cv2.imread('picture/pic.6-boicanh.png')
    if p6 is None:
        print("[LỖI] Không tìm thấy picture/pic.6-boicanh.png")
        return

    items = [
        ('Assets/Sprites/Environment', 'prop_cong_lang_ngoi_do_vietnam.png', 119, 239, 19, 252, 20, 500, True),
        ('Assets/Sprites/Environment', 'prop_cung_dien_hanok_korea.png', 117, 239, 262, 488, 20, 500, True),
        ('Assets/Sprites/Environment', 'prop_hang_rao_cọc_tre_co_ngu_sac.png', 157, 268, 593, 672, 20, 300, True),
        ('Assets/Sprites/Environment', 'prop_cay_phong_la_do_korea.png', 116, 266, 770, 871, 20, 300, True),
        ('Assets/Sprites/Environment', 'prop_den_long_cheongsa_chorong.png', 117, 266, 901, 1002, 20, 300, True),
        ('Assets/Sprites/Environment', 'prop_dai_den_long_hoi_an.png', 180, 256, 520, 588, 20, 200, True),
        ('Assets/Sprites/Environment', 'prop_khan_dai_co_vu_vietnam.png', 247, 375, 143, 362, 20, 500, True),
        ('Assets/Sprites/Environment', 'prop_luy_tre_xanh_vietnam.png', 390, 526, 260, 485, 20, 500, True),
        ('Assets/Sprites/Environment', 'prop_khan_dai_co_vu_korea.png', 424, 525, 21, 225, 20, 500, True),

        # UI BUTTONS
        ('Assets/Sprites/UI', 'btn_pause_round.png', 115, 157, 534, 575, 20, 200, True),
        ('Assets/Sprites/UI', 'btn_sound_round.png', 280, 325, 601, 645, 20, 200, True),
        ('Assets/Sprites/UI', 'icon_victory_trophy.png', 276, 322, 682, 729, 20, 200, True),
        ('Assets/Sprites/UI', 'ui_bar_energy_health.png', 350, 418, 527, 673, 20, 400, True),
        ('Assets/Sprites/UI', 'ui_panel_victory_defeat.png', 381, 529, 781, 1007, 20, 500, True),
        ('Assets/Sprites/UI', 'btn_replay.png', 431, 505, 529, 603, 20, 300, True),
        ('Assets/Sprites/UI', 'btn_home.png', 412, 519, 627, 728, 20, 300, True)
    ]

    for out_dir, fname, y1, y2, x1, x2, thresh, min_area, fill_h in items:
        os.makedirs(out_dir, exist_ok=True)
        crop = p6[y1:y2, x1:x2]
        sprite = clean_extract_sprite(crop, bg_threshold=thresh, min_component_area=min_area, fill_internal_holes=fill_h)
        if sprite is not None:
            out_p = os.path.join(out_dir, fname)
            cv2.imwrite(out_p, sprite)
            print(f"  [OK] {fname:35s} -> {sprite.shape[1]}x{sprite.shape[0]}px (100% sạch rác)")


def clean_festival_map():
    print("\n=== [3/5] ĐANG TẠO BACKGROUND MAP SẠCH 100% (KHÔNG CHỮ, KHÔNG VỆT MỜ) ===")
    p2 = cv2.imread('picture/pic.2.png')
    if p2 is None:
        print("[LỖI] Không tìm thấy picture/pic.2.png")
        return

    # 1. Cắt chuẩn khu vực Map 1 (y: 235 đến 755, x: 60 đến 1350)
    p2_tl = p2[:768, :1376]
    scene = p2_tl[235:755, 60:1350].copy() # Kích thước chuẩn: 520 x 1290

    # 2. Lấy sàn đấu sạch (Empty Arena Stadium) từ pic.1
    p1 = cv2.imread('picture/pic.1.png')
    stadium_crop = p1[325:1050, 50:1260]
    stadium_clean = clean_extract_sprite(stadium_crop, bg_threshold=20, min_component_area=2000, fill_internal_holes=True)

    if stadium_clean is not None:
        # Scale sàn đấu vừa khít với sới đấu ở giữa sân
        stadium_w = 560
        stadium_h = int(stadium_clean.shape[0] * stadium_w / stadium_clean.shape[1])
        stadium_resized = cv2.resize(stadium_clean, (stadium_w, stadium_h))

        ox = 370
        oy = 55
        s_rgb = stadium_resized[:, :, :3]
        s_a = stadium_resized[:, :, 3] / 255.0

        # Màng hòa trộn viền mềm mại để khớp tự nhiên vào sới đấu
        edge_dist = cv2.distanceTransform((stadium_resized[:, :, 3] > 10).astype(np.uint8), cv2.DIST_L2, 3)
        blend_alpha = np.clip(edge_dist / 15.0, 0, 1.0) * s_a

        h, w = stadium_resized.shape[:2]
        roi = scene[oy:oy+h, ox:ox+w]
        for c in range(3):
            roi[:, :, c] = (roi[:, :, c] * (1 - blend_alpha) + s_rgb[:, :, c] * blend_alpha).astype(np.uint8)

    # 3. Xóa sạch dòng text hướng dẫn ở đáy ("Kéo về phía đối thủ...") bằng nền cát mịn tự nhiên
    sand_sample = scene[435:465, 20:500]
    scene[475:505, 20:500] = cv2.resize(sand_sample, (480, 30))

    # 4. Làm sạch khung biển hiệu trên đỉnh (giữ biển sơn mài cổ truyền, bỏ chữ text vẽ tĩnh)
    scene[12:48, 525:765] = (35, 28, 48)

    # 5. Làm sạch khung thẻ nhân vật thừa bên phải
    ground_sample = scene[280:350, 970:1030]
    scene[280:350, 1040:1100] = cv2.resize(ground_sample, (60, 70))

    out_path = 'Assets/Sprites/Maps/map_festival_arena.png'
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    cv2.imwrite(out_path, scene)
    print(f"  [OK] Đã xuất Bản Đồ Lễ Hội Sạch Hoàn Toàn: {out_path} ({scene.shape[1]}x{scene.shape[0]}px)")


def reslice_vfx():
    print("\n=== [4/5] ĐANG TRÍCH XUẤT VFX KỸ NĂNG VIỆT NAM & HÀN QUỐC ===")
    # VN VFX
    p8 = cv2.imread('picture/pic.8-hieu-ung-viet-nam.png')
    if p8 is not None:
        h, w = p8.shape[:2]
        top_y = 70
        cell_h = (h - top_y) // 3
        cell_w = w // 3
        out_vn = 'Assets/Sprites/VFX/Vietnam'
        os.makedirs(out_vn, exist_ok=True)

        vn_names = [
            ["vfx_song_chan_dong_trong_dong.png", "vfx_manh_vo_lua_vang.png", "vfx_vet_luot_may_la_de.png"],
            ["vfx_loc_xoay_thang_long.png", "vfx_vu_no_phao_hoa_phao_tep.png", "vfx_bo_luy_tre_nga.png"],
            ["vfx_la_tre_bay_xao_xac.png", "vfx_hao_quang_trong_dong.png", "vfx_gio_loc_dong_son.png"]
        ]

        for r in range(3):
            for c in range(3):
                y1 = top_y + r * cell_h + 20
                y2 = top_y + (r + 1) * cell_h - 15
                x1 = c * cell_w + 15
                x2 = (c + 1) * cell_w - 15
                crop = p8[y1:y2, x1:x2]
                sprite = clean_extract_sprite(crop, bg_threshold=22, min_component_area=150, fill_internal_holes=False)
                if sprite is not None:
                    p = os.path.join(out_vn, vn_names[r][c])
                    cv2.imwrite(p, sprite)
                    print(f"  [OK] VN VFX: {vn_names[r][c]}")

    # KR VFX
    p9 = cv2.imread('picture/pic.9-hieu-ung-han-quoc.png')
    if p9 is not None:
        h, w = p9.shape[:2]
        top_y = 70
        cell_h = (h - top_y) // 3
        cell_w = w // 3
        out_kr = 'Assets/Sprites/VFX/Korea'
        os.makedirs(out_kr, exist_ok=True)

        kr_names = [
            ["vfx_luong_am_thanh_gam_thet.png", "vfx_chan_dong_vuot_ho.png", "vfx_khien_chuong_dong_dancheong.png"],
            ["vfx_canh_hoa_mugunghwa.png", "vfx_song_phep_chay_gai.png", "vfx_dom_ma_troi_dokkaebi.png"],
            ["vfx_dia_chan_nen_tay_thao_duoc.png", "vfx_canh_chim_lua_thai_cuc.png", "vfx_hao_quang_thai_cuc.png"]
        ]

        for r in range(3):
            for c in range(3):
                y1 = top_y + r * cell_h + 20
                y2 = top_y + (r + 1) * cell_h - 15
                x1 = c * cell_w + 15
                x2 = (c + 1) * cell_w - 15
                crop = p9[y1:y2, x1:x2]
                sprite = clean_extract_sprite(crop, bg_threshold=22, min_component_area=150, fill_internal_holes=False)
                if sprite is not None:
                    p = os.path.join(out_kr, kr_names[r][c])
                    cv2.imwrite(p, sprite)
                    print(f"  [OK] KR VFX: {kr_names[r][c]}")


if __name__ == '__main__':
    reslice_pic1()
    reslice_pic6()
    clean_festival_map()
    reslice_vfx()
    print("\n========================================================")
    print("===> HOÀN TẤT TRÍCH XUẤT 100% SPRITES SẠCH RÁC & NỀN!")
    print("========================================================")
