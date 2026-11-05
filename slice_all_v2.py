import cv2
import numpy as np
import os
import sys

sys.stdout.reconfigure(encoding='utf-8')

def extract_alpha_sprite(crop, bg_color, threshold=24, blur_radius=3):
    """
    Tách nền tối bằng Distance Metric và FloodFill từ các góc,
    tạo kênh Alpha mượt mà không bị răng cưa.
    """
    h, w = crop.shape[:2]
    if h <= 4 or w <= 4:
        return None

    # Đo khoảng cách màu so với màu nền
    diff = np.linalg.norm(crop.astype(float) - bg_color.astype(float), axis=2)
    is_bg = (diff < threshold).astype(np.uint8)

    # Floodfill từ 4 cạnh để chỉ xóa nền ngoại vi
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

    # Làm mượt nhẹ kênh Alpha
    if blur_radius > 0:
        alpha = cv2.GaussianBlur(alpha, (blur_radius, blur_radius), 0)

    # Bounding box crop sát viền
    ys, xs = np.where(alpha > 15)
    if len(ys) == 0 or len(xs) == 0:
        return None

    y_min, y_max = ys.min(), ys.max() + 1
    x_min, x_max = xs.min(), xs.max() + 1

    rgba = cv2.cvtColor(crop, cv2.COLOR_BGR2BGRA)
    rgba[:, :, 3] = alpha
    return rgba[y_min:y_max, x_min:x_max]


def process_pic8_vietnam():
    print("--> Đang xử lý pic.8-hieu-ung-viet-nam.png...")
    img = cv2.imread('picture/pic.8-hieu-ung-viet-nam.png')
    h, w = img.shape[:2]
    out_dir = 'Assets/Sprites/VFX/Vietnam'
    os.makedirs(out_dir, exist_ok=True)

    # 3x3 grid (bỏ header top 70px)
    top_y = 70
    grid_h = h - top_y
    cell_h = grid_h // 3
    cell_w = w // 3

    vn_effects = [
        [("vfx_song_chan_dong_trong_dong.png", 0, 0),
         ("vfx_manh_vo_lua_vang.png", 0, 1),
         ("vfx_vet_luot_may_la_de.png", 0, 2)],
        [("vfx_loc_xoay_thang_long.png", 1, 0),
         ("vfx_vu_no_phao_hoa_phao_tep.png", 1, 1),
         ("vfx_bo_luy_tre_nga.png", 1, 2)],
        [("vfx_la_tre_bay_xao_xac.png", 2, 0),
         ("vfx_hao_quang_trong_dong.png", 2, 1),
         ("vfx_gio_loc_dong_son.png", 2, 2)]
    ]

    bg_color = np.array([32, 32, 32], dtype=np.uint8)

    for r in range(3):
        for c in range(3):
            fname, row, col = vn_effects[r][c]
            y1 = top_y + r * cell_h + 20 # Bỏ text nhãn trong cell
            y2 = top_y + (r + 1) * cell_h - 10
            x1 = c * cell_w + 10
            x2 = (c + 1) * cell_w - 10
            crop = img[y1:y2, x1:x2]
            sprite = extract_alpha_sprite(crop, bg_color, threshold=22)
            if sprite is not None:
                p = os.path.join(out_dir, fname)
                cv2.imwrite(p, sprite)
                print(f"  + Xuất {fname} ({sprite.shape[1]}x{sprite.shape[0]}px)")


def process_pic9_korea():
    print("--> Đang xử lý pic.9-hieu-ung-han-quoc.png...")
    img = cv2.imread('picture/pic.9-hieu-ung-han-quoc.png')
    h, w = img.shape[:2]
    out_dir = 'Assets/Sprites/VFX/Korea'
    os.makedirs(out_dir, exist_ok=True)

    top_y = 70
    grid_h = h - top_y
    cell_h = grid_h // 3
    cell_w = w // 3

    kr_effects = [
        [("vfx_luong_am_thanh_gam_thet.png", 0, 0),
         ("vfx_chan_dong_vuot_ho.png", 0, 1),
         ("vfx_khien_chuong_dong_dancheong.png", 0, 2)],
        [("vfx_canh_hoa_mugunghwa.png", 1, 0),
         ("vfx_song_phep_chay_gai.png", 1, 1),
         ("vfx_dom_ma_troi_dokkaebi.png", 1, 2)],
        [("vfx_dia_chan_nen_tay_thao_duoc.png", 2, 0),
         ("vfx_canh_chim_lua_thai_cuc.png", 2, 1),
         ("vfx_hao_quang_thai_cuc.png", 2, 2)]
    ]

    bg_color = np.array([34, 34, 34], dtype=np.uint8)

    for r in range(3):
        for c in range(3):
            fname, row, col = kr_effects[r][c]
            y1 = top_y + r * cell_h + 20
            y2 = top_y + (r + 1) * cell_h - 10
            x1 = c * cell_w + 10
            x2 = (c + 1) * cell_w - 10
            crop = img[y1:y2, x1:x2]
            sprite = extract_alpha_sprite(crop, bg_color, threshold=22)
            if sprite is not None:
                p = os.path.join(out_dir, fname)
                cv2.imwrite(p, sprite)
                print(f"  + Xuất {fname} ({sprite.shape[1]}x{sprite.shape[0]}px)")


def process_pic6_environment_and_ui():
    print("--> Đang xử lý pic.6-boicanh.png...")
    img = cv2.imread('picture/pic.6-boicanh.png')
    env_dir = 'Assets/Sprites/Environment'
    ui_dir = 'Assets/Sprites/UI'
    os.makedirs(env_dir, exist_ok=True)
    os.makedirs(ui_dir, exist_ok=True)

    bg_color = np.array([38, 38, 38], dtype=np.uint8)

    # Danh mục định nghĩa chính xác từ pic.6
    items = [
        # (Thư mục, tên file, x, y, w, h)
        (env_dir, "prop_cong_lang_ngoi_do_vietnam.png", 19, 119, 233, 120),
        (env_dir, "prop_cung_dien_hanok_korea.png", 262, 117, 226, 122),
        (ui_dir, "btn_pause_round.png", 534, 115, 41, 42),
        (env_dir, "prop_hang_rao_cọc_tre_co_ngu_sac.png", 593, 157, 79, 111),
        (env_dir, "prop_cay_phong_la_do_korea.png", 770, 116, 101, 150),
        (env_dir, "prop_den_long_cheongsa_chorong.png", 901, 117, 101, 149),
        (env_dir, "prop_dai_den_long_hoi_an.png", 520, 180, 68, 76),
        (env_dir, "prop_khan_dai_co_vu_vietnam.png", 143, 247, 219, 128),
        (ui_dir, "btn_sound_round.png", 601, 280, 44, 45),
        (ui_dir, "icon_victory_trophy.png", 682, 276, 47, 46),
        (env_dir, "prop_luy_tre_xanh_vietnam.png", 260, 390, 225, 136),
        (ui_dir, "ui_bar_energy_health.png", 527, 350, 146, 68),
        (ui_dir, "ui_panel_victory_defeat.png", 781, 381, 226, 148),
        (env_dir, "prop_khan_dai_co_vu_korea.png", 21, 424, 204, 101),
        (ui_dir, "btn_replay.png", 529, 431, 74, 74),
        (ui_dir, "btn_home.png", 627, 412, 101, 107)
    ]

    for target_dir, fname, x, y, w, h in items:
        crop = img[y:y+h, x:x+w]
        sprite = extract_alpha_sprite(crop, bg_color, threshold=20)
        if sprite is not None:
            p = os.path.join(target_dir, fname)
            cv2.imwrite(p, sprite)
            print(f"  + Xuất {fname} ({sprite.shape[1]}x{sprite.shape[0]}px) -> {target_dir}")


def process_pic7_background():
    print("--> Đang xử lý pic.7-background.png...")
    img = cv2.imread('picture/pic.7-background.png')
    out_path = 'Assets/Sprites/Arenas/background_festival_panoramic.png'
    # Lưu ảnh nền phong cảnh lễ hội góc rộng
    cv2.imwrite(out_path, img)
    print(f"  + Xuất bối cảnh nền panorama: {out_path} ({img.shape[1]}x{img.shape[0]}px)")


if __name__ == '__main__':
    process_pic8_vietnam()
    process_pic9_korea()
    process_pic6_environment_and_ui()
    process_pic7_background()
    print("===> HOÀN TẤT TRÍCH XUẤT TOÀN BỘ SPRITES SẠCH NỀN V2!")
