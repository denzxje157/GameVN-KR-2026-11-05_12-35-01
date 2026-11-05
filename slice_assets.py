#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script: slice_assets.py
Dự án: Lễ hội Linh vật: Đấu đẩy hữu nghị (Mythic Bumper) - KGW2026.
Chức năng:
  - Tự động đọc ảnh Sprite Sheet từ thư mục picture/ (hoặc ../picture/).
  - Tách nền xám/nền đặc thành nền trong suốt (Alpha channel) mượt mà.
  - Cắt chính xác từng nhân vật, trạng thái tung chiêu, vật phẩm, UI, hiệu ứng.
  - Tự động lưu và phân loại vào Assets/Sprites/ (Characters, Items, Arenas, UI) của Unity.
"""

import os
import sys
import cv2
import numpy as np
from PIL import Image

if sys.stdout.encoding.lower() != 'utf-8':
    try:
        sys.stdout.reconfigure(encoding='utf-8')
    except Exception:
        pass

def find_picture_dir():
    candidates = ['picture', '../picture', os.path.join(os.path.dirname(__file__), 'picture'), os.path.join(os.path.dirname(__file__), '../picture')]
    for c in candidates:
        if os.path.isdir(c):
            return os.path.abspath(c)
    return None

def ensure_dirs(base_dir):
    categories = ['Characters', 'Items', 'Arenas', 'UI']
    paths = {}
    for cat in categories:
        p = os.path.join(base_dir, 'Assets', 'Sprites', cat)
        os.makedirs(p, exist_ok=True)
        paths[cat] = p
    return paths

def extract_clean_sprite(img, y1, y2, x1, x2, threshold=16, pad=2):
    """
    Cắt một vùng ảnh, tìm nền ngoại vi bằng FloodFill và tạo kênh Alpha trong suốt.
    Tự động crop sát theo viền nội dung (Bounding Box).
    """
    crop = img[y1:y2, x1:x2].copy()
    h, w = crop.shape[:2]
    if h <= 4 or w <= 4:
        return None

    # Lấy mẫu màu nền ở 4 góc
    corners = np.vstack([crop[:5, :5], crop[:5, -5:], crop[-5:, :5], crop[-5:, -5:]])
    bg_color = np.median(corners.reshape(-1, 3), axis=0)

    # Đo khoảng cách màu tới màu nền
    diff = np.linalg.norm(crop.astype(float) - bg_color, axis=2)
    is_bg = (diff < threshold).astype(np.uint8)

    # FloodFill từ viền ngoài để chỉ xóa nền bên ngoài, không xóa các chi tiết bên trong
    flood = np.zeros((h + 2, w + 2), np.uint8)
    for ey in range(h):
        for ex in [0, w - 1]:
            if is_bg[ey, ex] == 1:
                cv2.floodFill(is_bg, flood, (ex, ey), 2)
    for ex in range(w):
        for ey in [0, h - 1]:
            if is_bg[ey, ex] == 1:
                cv2.floodFill(is_bg, flood, (ex, ey), 2)

    ext_bg = (is_bg == 2)
    alpha = np.where(ext_bg, 0, 255).astype(np.uint8)

    # Làm mượt nhẹ viền cạnh (Anti-aliasing) bằng Distance Transform
    dist = cv2.distanceTransform((alpha > 0).astype(np.uint8), cv2.DIST_L2, 3)
    alpha = np.clip(dist * 128, 0, 255).astype(np.uint8)

    # Ghép kênh Alpha vào ảnh BGR
    bgra = cv2.cvtColor(crop, cv2.COLOR_BGR2BGRA)
    bgra[:, :, 3] = alpha

    # Tìm bounding box chính xác của vùng có nét vẽ (Alpha > 10)
    rows = np.any(alpha > 10, axis=1)
    cols = np.any(alpha > 10, axis=0)
    if not np.any(rows) or not np.any(cols):
        return None

    rmin, rmax = np.where(rows)[0][[0, -1]]
    cmin, cmax = np.where(cols)[0][[0, -1]]

    rmin = max(0, rmin - pad)
    rmax = min(h, rmax + pad + 1)
    cmin = max(0, cmin - pad)
    cmax = min(w, cmax + pad + 1)

    cropped = bgra[rmin:rmax, cmin:cmax]
    return cropped

def slice_master_spritesheet(pic1_path, out_paths):
    img = cv2.imread(pic1_path)
    if img is None:
        print(f"[LỖI] Không thể đọc ảnh: {pic1_path}")
        return []

    h, w = img.shape[:2]
    print(f"[INFO] Đang xử lý Master Sprite Sheet: {os.path.basename(pic1_path)} ({w}x{h})...")

    # Danh mục định nghĩa tọa độ chuẩn xác trên bản vẽ Master pic.1.png (2752 x 1536)
    sprite_definitions = [
        # --- 1. CHARACTERS (LINH VẬT 2D) ---
        ('Characters', 'trau_vang_idle.png', 270, 560, 1430, 1660),
        ('Characters', 'trau_vang_skill_kim_nguu_tran_dia.png', 250, 570, 1670, 1980),
        ('Characters', 'ho_baekho_idle.png', 270, 560, 2100, 2340),
        ('Characters', 'ho_baekho_skill_ho_khieu_son_ha.png', 270, 570, 2360, 2660),
        ('Characters', 'rong_vang_idle.png', 610, 890, 1410, 1670),
        ('Characters', 'rong_vang_skill_luot_may.png', 610, 900, 1680, 1980),
        ('Characters', 'haechi_idle.png', 620, 890, 2110, 2350),
        ('Characters', 'haechi_skill_linh_khi_tru_ta.png', 610, 930, 2360, 2700),

        # --- 2. ARENAS (SỚI ĐẤU TRƯỜNG) ---
        ('Arenas', 'arena_stadium_viet_han.png', 325, 1050, 50, 1260),
        ('Arenas', 'arena_center_field.png', 240, 970, 160, 1160),

        # --- 3. ITEMS & OBSTACLES (VẬT PHẨM VĂN HÓA & VẬT CẢN) ---
        ('Items', 'item_banh_chung.png', 1180, 1420, 60, 310),
        ('Items', 'item_banh_gao_tteokbokki.png', 1160, 1420, 310, 540),
        ('Items', 'item_non_la_phap_bao.png', 1190, 1420, 540, 740),
        ('Items', 'item_quat_lua_buchaechum.png', 1190, 1420, 740, 940),
        ('Items', 'obstacle_vai_gom_va_chum_kimchi.png', 1190, 1420, 940, 1160),
        ('Items', 'obstacle_vai_gom_co.png', 1190, 1420, 940, 1055),
        ('Items', 'obstacle_chum_kimchi.png', 1190, 1420, 1045, 1160),
        ('Items', 'obstacle_vung_bun.png', 1240, 1400, 1160, 1400),

        # --- 4. VFX & UI (HIỆU ỨNG & GIAO DIỆN) ---
        ('UI', 'vfx_cloud_gold.png', 1020, 1190, 1410, 1750),
        ('UI', 'vfx_wind_slash.png', 1020, 1180, 1760, 2060),
        ('UI', 'vfx_lotus_petals.png', 1020, 1180, 2070, 2700),
        ('UI', 'ui_slingshot_indicator.png', 1260, 1360, 1410, 1680),
        ('UI', 'ui_touch_hand.png', 1310, 1480, 1650, 1780),
        ('UI', 'btn_skill_fist.png', 1260, 1470, 1800, 1970),
        ('UI', 'btn_swap_tag_team.png', 1280, 1470, 1970, 2140),
        ('UI', 'ui_drum_le_hoi.png', 1260, 1470, 2150, 2330),
        ('UI', 'ui_text_bump.png', 1270, 1370, 2320, 2480),
        ('UI', 'ui_text_bang.png', 1190, 1320, 2480, 2700),
        ('UI', 'ui_text_ultimate.png', 1330, 1470, 2460, 2710)
    ]

    saved_files = []
    for category, filename, y1, y2, x1, x2 in sprite_definitions:
        sprite = extract_clean_sprite(img, y1, y2, x1, x2)
        if sprite is not None:
            out_file = os.path.join(out_paths[category], filename)
            # Dùng Pillow để lưu PNG tối ưu kênh Alpha
            rgba_img = cv2.cvtColor(sprite, cv2.COLOR_BGRA2RGBA)
            pil_img = Image.fromarray(rgba_img)
            pil_img.save(out_file, format='PNG', optimize=True)
            saved_files.append((category, filename, sprite.shape[1], sprite.shape[0]))
            print(f"  [OK] Cắt thành công: {category}/{filename} ({sprite.shape[1]}x{sprite.shape[0]})")
        else:
            print(f"  [CẢNH BÁO] Không tìm thấy nét vẽ cho: {filename}")

    return saved_files

def copy_scenery_backgrounds(pic_dir, out_paths):
    """Xử lý thêm các ảnh bối cảnh nền nếu có (pic.7, pic.6)"""
    bg_candidates = [
        ('pic.7-background.png', 'Arenas', 'background_festival_scenery.png'),
        ('pic.6-boicanh.png', 'Arenas', 'background_arena_overview.png')
    ]
    for src_name, cat, dest_name in bg_candidates:
        src_p = os.path.join(pic_dir, src_name)
        if os.path.isfile(src_p):
            dest_p = os.path.join(out_paths[cat], dest_name)
            img = cv2.imread(src_p)
            if img is not None:
                # Đổi sang RGBA và lưu
                bgra = cv2.cvtColor(img, cv2.COLOR_BGR2BGRA)
                rgba_img = cv2.cvtColor(bgra, cv2.COLOR_BGRA2RGBA)
                Image.fromarray(rgba_img).save(dest_p, format='PNG', optimize=True)
                print(f"  [OK] Đã chuyển đổi bối cảnh: {cat}/{dest_name} ({img.shape[1]}x{img.shape[0]})")

def main():
    print("=" * 65)
    print("  CHƯƠNG TRÌNH TỰ ĐỘNG CẮT SPRITE GAME ASSETS (SLICE_ASSETS)")
    print("=" * 65)

    pic_dir = find_picture_dir()
    if not pic_dir:
        print("[LỖI] Không tìm thấy thư mục 'picture/' hoặc '../picture/'!")
        sys.exit(1)

    print(f"[1] Thư mục nguồn ảnh: {pic_dir}")

    # Thư mục gốc dự án Unity (nơi chứa Assets/)
    project_root = os.path.abspath(os.path.join(os.path.dirname(__file__)))
    out_paths = ensure_dirs(project_root)
    print(f"[2] Thư mục đích Unity: {os.path.join(project_root, 'Assets', 'Sprites')}")

    # File Sprite Sheet tổng thể độ phân giải 2K
    master_sheet = os.path.join(pic_dir, 'pic.1.png')
    if not os.path.isfile(master_sheet):
        # Thử tìm file png khác
        pngs = [f for f in os.listdir(pic_dir) if f.endswith('.png')]
        if pngs:
            master_sheet = os.path.join(pic_dir, pngs[0])

    saved = slice_master_spritesheet(master_sheet, out_paths)
    copy_scenery_backgrounds(pic_dir, out_paths)

    print("\n" + "=" * 65)
    print(f" HOÀN TẤT! Đã xuất tổng cộng {len(saved)} sprites trong suốt vào Assets/Sprites/")
    print("=" * 65)

if __name__ == '__main__':
    main()
