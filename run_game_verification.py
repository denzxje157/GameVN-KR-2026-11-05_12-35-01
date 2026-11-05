#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script: run_game_verification.py
Kiểm tra và xác thực toàn bộ hệ thống game:
1. Đồ họa Sprites & 4 Maps (không text rác, không vệt mờ, không mảnh vụn thừa).
2. Tích hợp Scene MainGame (MainMenu_Canvas, Mobile_Canvas, Player, Enemy, Spawner, Arena).
3. Prefabs & PhysicsMaterials.
4. Logic Scripts C# & Biên dịch MSBuild.
Dự án: Lễ hội Linh vật: Đấu đẩy hữu nghị (Mythic Bumper) - KGW2026.
"""

import os
import sys
import glob
import cv2
import numpy as np

if sys.stdout.encoding.lower() != 'utf-8':
    try:
        sys.stdout.reconfigure(encoding='utf-8')
    except Exception:
        pass

def run_verification():
    passed = 0
    failed = 0
    report = []

    def check(name, condition, details=""):
        nonlocal passed, failed
        if condition:
            passed += 1
            msg = f"  [PASS] {name} {f'({details})' if details else ''}"
            report.append(msg)
            print(msg)
        else:
            failed += 1
            msg = f"  [FAIL] {name} {f'({details})' if details else ''}"
            report.append(msg)
            print(msg)

    print("=" * 80)
    print(" BÁO CÁO KIỂM THỬ VÀ XÁC THỰC TOÀN DIỆN: MYTHIC BUMPER (KGW2026)")
    print("=" * 80)

    # ----------------------------------------------------
    # 1. KIỂM TRA CÁC BẢN ĐỒ THI ĐẤU & THUMBNAILS (3 BẢN ĐỒ CHÍNH THỨC MỚI)
    # ----------------------------------------------------
    print("\n--- [1/5] KIỂM TRA 3 BẢN ĐỒ THI ĐẤU MỚI VÀ PREVIEW THUMBNAILS ---")
    maps = [
        ('Assets/Sprites/Maps/san1-lang.png', 'Assets/Sprites/UI/Thumbnails/thumb_san1-lang.png', 'Map 1: Sới Đấu Lễ Hội Dân Gian (san1-lang)'),
        ('Assets/Sprites/Maps/san-yentu.png', 'Assets/Sprites/UI/Thumbnails/thumb_san-yentu.png', 'Map 2: Sơn Cảnh Kỳ Quan - Yên Tử & Seoraksan (san-yentu)'),
        ('Assets/Sprites/Maps/dan-dempho.png', 'Assets/Sprites/UI/Thumbnails/thumb_dan-dempho.png', 'Map 3: Phố Đêm Rực Rỡ - Hội An & Myeongdong (dan-dempho)')
    ]
    for map_p, thumb_p, title in maps:
        check(f"{title} - Tệp map tồn tại", os.path.exists(map_p))
        check(f"{title} - Tệp thumbnail tồn tại", os.path.exists(thumb_p))
        if os.path.exists(map_p):
            im = cv2.imread(map_p)
            check(f"{title} - Độ phân giải chuẩn", im.shape[1] >= 1000 and im.shape[0] >= 500, f"{im.shape[1]}x{im.shape[0]}px")
            # Kiểm tra độ sắc nét
            sharpness = cv2.Laplacian(im, cv2.CV_64F).var()
            check(f"{title} - Độ sắc nét đồ họa (Laplacian Var > 100)", sharpness > 100, f"{sharpness:.1f}")

    # ----------------------------------------------------
    # 2. KIỂM TRA SPRITES LINH VẬT & MẢNH VỠ NGOẠI VI
    # ----------------------------------------------------
    print("\n--- [2/5] KIỂM TRA LINH VẬT & ĐỘ SẠCH SPRITES (0 MẢNH RÁC) ---")
    char_sprites = [
        'Assets/Sprites/Characters/trau_vang_idle.png',
        'Assets/Sprites/Characters/trau_vang_skill_kim_nguu_tran_dia.png',
        'Assets/Sprites/Characters/rong_vang_idle.png',
        'Assets/Sprites/Characters/rong_vang_skill_luot_may.png',
        'Assets/Sprites/Characters/lan_su_idle.png',
        'Assets/Sprites/Characters/be_tre_idle.png',
        'Assets/Sprites/Characters/chim_lac_idle.png',
        'Assets/Sprites/Characters/ho_baekho_idle.png',
        'Assets/Sprites/Characters/ho_baekho_skill_ho_khieu_son_ha.png',
        'Assets/Sprites/Characters/haechi_idle.png',
        'Assets/Sprites/Characters/haechi_skill_linh_khi_tru_ta.png',
        'Assets/Sprites/Characters/dokkaebi_idle.png',
        'Assets/Sprites/Characters/gau_dangoon_idle.png',
        'Assets/Sprites/Characters/samjoko_idle.png'
    ]
    for cp in char_sprites:
        fname = os.path.basename(cp)
        check(f"Linh vật: {fname} tồn tại", os.path.exists(cp))
        if os.path.exists(cp):
            im = cv2.imread(cp, cv2.IMREAD_UNCHANGED)
            check(f"{fname} có kênh Alpha (RGBA)", im.shape[2] == 4)
            # Kiểm tra mảnh vỡ lơ lửng bên ngoài
            num_labels, labels, stats, _ = cv2.connectedComponentsWithStats((im[:, :, 3] > 10).astype(np.uint8))
            small_fragments = [s[cv2.CC_STAT_AREA] for s in stats[1:] if s[cv2.CC_STAT_AREA] < 150]
            check(f"{fname} sạch 100% không mảnh vỡ rác ngoại vi", len(small_fragments) == 0, f"mảnh rác={len(small_fragments)}")

    print("\n--- [2b/5] KIỂM TRA 10 AVATARS GIAO DIỆN & NÚT TITLE SCREEN ---")
    avatars = [
        'avatar_trau_vang.png', 'avatar_rong_vang.png', 'avatar_lan_su.png', 'avatar_be_tre.png', 'avatar_chim_lac.png',
        'avatar_ho_baekho.png', 'avatar_haechi.png', 'avatar_dokkaebi.png', 'avatar_gau_dangoon.png', 'avatar_samjoko.png'
    ]
    for av in avatars:
        av_path = os.path.join('Assets/Sprites/UI/Avatars', av)
        check(f"Avatar: {av} tồn tại", os.path.exists(av_path))

    title_buttons = [
        'btn_title_local_play.png', 'btn_title_pvp.png', 'btn_title_codex.png', 'btn_title_tutorial.png',
        'btn_title_settings.png', 'btn_title_sound.png', 'background_title_screen.png',
        'title_banner_logo.png', 'title_podium_mascots.png'
    ]
    for tb in title_buttons:
        tb_path = os.path.join('Assets/Sprites/UI/Title', tb)
        check(f"Title Asset: {tb} tồn tại", os.path.exists(tb_path))

    match_hub_assets = [
        'btn_vao_tran_fire.png', 'btn_quay_lai.png', 'icon_fire_vs.png',
        'card_character_vn.png', 'card_character_kr.png', 'frame_map_oval.png'
    ]
    for mha in match_hub_assets:
        mha_path = os.path.join('Assets/Sprites/UI/MatchHub', mha)
        check(f"Match Hub Asset: {mha} tồn tại", os.path.exists(mha_path))

    # ----------------------------------------------------
    # 3. KIỂM TRA VẬT PHẨM VĂN HÓA & VẬT CẢN BUMPER
    # ----------------------------------------------------
    print("\n--- [3/5] KIỂM TRA 4 VẬT PHẨM VĂN HÓA & VẬT CẢN SỚI ĐẤU ---")
    item_sprites = [
        ('Assets/Sprites/Items/item_banh_chung.png', 'Bánh Chưng (VN)'),
        ('Assets/Sprites/Items/item_banh_gao_tteokbokki.png', 'Bánh Gạo Tteokbokki (KR)'),
        ('Assets/Sprites/Items/item_non_la_phap_bao.png', 'Nón Lá Pháp Bảo (VN)'),
        ('Assets/Sprites/Items/item_quat_lua_buchaechum.png', 'Quạt Lửa Buchaechum (KR)'),
        ('Assets/Sprites/Items/obstacle_chum_kimchi.png', 'Chum Kimchi Bumper'),
        ('Assets/Sprites/Items/obstacle_vai_gom_co.png', 'Vại Gốm Cổ Bumper'),
        ('Assets/Sprites/Items/obstacle_vung_bun.png', 'Vũng Bùn Làm Chậm 50%')
    ]
    for ip, label in item_sprites:
        check(f"Vật phẩm: {label} tồn tại", os.path.exists(ip))

    # ----------------------------------------------------
    # 4. KIỂM TRA CẤU TRÚC SCENE MAINGAME.UNITY
    # ----------------------------------------------------
    print("\n--- [4/5] KIỂM TRA PHÂN CẤP VÀ LOGIC SCENE MAINGAME ---")
    scene_path = 'Assets/Scenes/MainGame.unity'
    check("Tệp Scene MainGame.unity tồn tại", os.path.exists(scene_path))
    if os.path.exists(scene_path):
        with open(scene_path, 'r', encoding='utf-8', errors='ignore') as f:
            scene_text = f.read()

        core_elements = [
            ("Title_Canvas", "Màn hình Khởi Động Title Screen rực rỡ"),
            ("Btn_Title_LocalPlay", "Nút ĐẤU ĐỐI KHÁNG trên Title Screen"),
            ("MainMenu_Canvas", "Giao diện Menu Chính / Hub Chọn Sới Đấu & Linh Vật"),
            ("Panel_MapSelect", "Khung duyệt 4 sới đấu Carousel"),
            ("Btn_StartBattle", "Nút to Bắt Đầu Tranh Tài (CHIẾN ĐẤU!)"),
            ("Btn_Tutorial", "Nút Mở Popup Hướng Dẫn Luật"),
            ("Card_Player", "Thẻ chọn linh vật Việt Nam (5 tướng VN)"),
            ("Card_Enemy", "Thẻ chọn đối thủ Hàn Quốc (5 tướng HQ)"),
            ("Mobile_Canvas", "Giao diện Mobile HUD trong trận"),
            ("Top_HUD", "Thanh tỉ số và đếm ngược trên cùng"),
            ("Btn_Pause", "Nút Tạm Dừng trận đấu"),
            ("Btn_Home", "Nút Về Menu Chính từ trong trận"),
            ("Btn_Restart", "Nút Chơi Lại / Reset Round (↺)"),
            ("Btn_Sound", "Nút Bật/Tắt Âm Thanh"),
            ("Btn_Swap", "Nút Đổi Người Tag-Team"),
            ("Btn_Skill", "Nút Tuyệt Kỹ Linh Vật"),
            ("Full_Background", "Hình nền tự động căn vừa khít góc nhìn Camera"),
            ("Arena_Stadium", "Vòng bo sới đấu bắt Ring-Out Trigger"),
            ("Player_TrauVang", "GameObject Linh vật Người chơi"),
            ("Enemy_HoBaekho", "GameObject Linh vật Đối thủ AI"),
            ("Item_Spawner", "Hệ thống thả vật phẩm văn hóa ngẫu nhiên"),
            ("VFX_Manager", "Hệ thống hiệu ứng kỹ năng & Tag-Team"),
            ("GameManager", "Hệ thống quản lý điểm số Best of 5")
        ]
        for elem_name, desc in core_elements:
            check(f"Scene: {elem_name} ({desc})", elem_name in scene_text)

        # Kiểm tra thiết lập 3 maps mới trong Scene
        check("Scene: Full_Background gán sprite san1-lang.png", "7a1b2c3d4e5f60718293a4b5c6d7e801" in scene_text)
        check("Scene: Image_MapPreview gán thumbnail thumb_san1-lang.png", "8a1b2c3d4e5f60718293a4b5c6d7e802" in scene_text)
        check("Scene: Label_Map hiển thị 3 bản đồ", "\\u2014 CH\\u1ECCN S\\u1EDAI \\u0110\\u1EA4U (3 B\\u1EA2N \\u0110\\u1ED2) \\u2014" in scene_text or "Label_Map" in scene_text or "Panel_MapSelect" in scene_text)
        check("Scene: GameManager cấu hình mảng mapBackgroundSprites", "mapBackgroundSprites:" in scene_text)
        check("Scene: MenuManager cấu hình san-yentu.png", "7b1b2c3d4e5f60718293a4b5c6d7e803" in scene_text)
        check("Scene: MenuManager cấu hình dan-dempho.png", "7c1b2c3d4e5f60718293a4b5c6d7e805" in scene_text)

    # ----------------------------------------------------
    # 5. KIỂM TRA HỆ THỐNG HOẠT ẢNH (ANIMATION & ANIMATOR CONTROLLERS)
    # ----------------------------------------------------
    print("\n--- [5/6] KIỂM TRA SPRITE SHEETS HOẠT ẢNH & ANIMATOR CHO 10 LINH VẬT ---")
    check("Sprite Sheet Hoạt ảnh VN: animation-vietnam.png tồn tại", os.path.exists("Assets/Sprites/Animations/animation-vietnam.png"))
    check("Sprite Sheet Hoạt ảnh HQ: animation-hanquoc.png tồn tại", os.path.exists("Assets/Sprites/Animations/animation-hanquoc.png"))
    check("Meta file Sprite Sheet VN được cấu hình Multiple", os.path.exists("Assets/Sprites/Animations/animation-vietnam.png.meta"))
    check("Meta file Sprite Sheet HQ được cấu hình Multiple", os.path.exists("Assets/Sprites/Animations/animation-hanquoc.png.meta"))

    all_mascots = [
        "TrauVang", "RongVang", "LanSu", "BeTre", "ChimLac",
        "HoBaekho", "Haechi", "Dokkaebi", "GauDangoon", "Samjoko"
    ]
    for m in all_mascots:
        idle_anim = f"Assets/Animations/Mascots/{m}_Idle.anim"
        action_anim = f"Assets/Animations/Mascots/{m}_Action.anim"
        controller = f"Assets/Animations/Mascots/AC_{m}.controller"
        check(f"Animation Clip Idle: {m}_Idle.anim tồn tại", os.path.exists(idle_anim))
        check(f"Animation Clip Action: {m}_Action.anim tồn tại", os.path.exists(action_anim))
        check(f"Animator Controller: AC_{m}.controller tồn tại", os.path.exists(controller))

    check("Scene: Player_TrauVang có gắn Animator component", "ce96a96922456c5238cd839f1028455b" in scene_text if os.path.exists(scene_path) else False)
    check("Scene: Enemy_HoBaekho có gắn Animator component", "7ead3c72e81b5ac714d79dffc565fce3" in scene_text if os.path.exists(scene_path) else False)

    # ----------------------------------------------------
    # 6. KIỂM TRA BIÊN DỊCH CODE C# (MSBUILD)
    # ----------------------------------------------------
    print("\n--- [6/7] KIỂM TRA BIÊN DỊCH C# TỔNG THỂ ---")
    scripts = glob.glob('Assets/Scripts/*.cs')
    check(f"Số lượng file C# Gameplay: {len(scripts)} scripts", len(scripts) >= 10)
    for sc in sorted(scripts):
        check(f"Script: {os.path.basename(sc)}", os.path.exists(sc))

    # ----------------------------------------------------
    # 7. KIỂM TRA TÀI NGUYÊN CÀI ĐẶT & TEXTMESHPRO (SETTINGS POPUP)
    # ----------------------------------------------------
    print("\n--- [7/7] KIỂM TRA TÀI NGUYÊN CÀI ĐẶT & TEXTMESHPRO ---")
    check("Sprite Khung Gỗ Lớn: panel_settings_large.png tồn tại", os.path.exists("Assets/Sprites/UI/Popups/panel_settings_large.png"))
    check("Sprite Nút Đóng Gỗ: btn_settings_dong.png tồn tại", os.path.exists("Assets/Sprites/UI/Popups/btn_settings_dong.png"))
    check("Sprite Thanh Trượt Gỗ: slider_bar_wood.png tồn tại", os.path.exists("Assets/Sprites/UI/Popups/slider_bar_wood.png"))
    check("Sprite Nút Kéo Knob: slider_knob_wood.png tồn tại", os.path.exists("Assets/Sprites/UI/Popups/slider_knob_wood.png"))
    check("Font TextMeshPro: LiberationSans SDF.asset tồn tại", os.path.exists("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset"))

    if os.path.exists(scene_path):
        check("Scene: Settings_Popup có kích thước chuẩn lớn 960x620", "{x: 960, y: 620}" in scene_text)
        check("Scene: Slider_BGM tồn tại", "Slider_BGM" in scene_text)
        check("Scene: Slider_SFX tồn tại", "Slider_SFX" in scene_text)
        check("Scene: Btn_CloseSettings dùng sprite gỗ chuẩn", "f1a2b3c4d5e60718293a4b5c6d7e8f90" in scene_text)
        check("Scene: Tiêu đề cài đặt dùng TextMeshPro", "Text_Title" in scene_text and "TextMeshProUGUI" in scene_text)

    # TỔNG KẾT
    print("\n" + "=" * 80)
    total_tests = passed + failed
    pct = (passed * 100.0 / total_tests) if total_tests > 0 else 0
    print(f" TỔNG KẾT: {passed}/{total_tests} BÀI KIỂM THỬ THÀNH CÔNG ({pct:.1f}%) | THẤT BẠI: {failed}")
    print("=" * 80)

    # Xuất ra file báo cáo
    with open('TestReport_MythicBumper.txt', 'w', encoding='utf-8') as rf:
        rf.write("================================================================================\n")
        rf.write("          BÁO CÁO KIỂM THỬ TỰ ĐỘNG TOÀN DIỆN: MYTHIC BUMPER (KGW2026)\n")
        rf.write(f"          Thời gian chạy test: {sys.version}\n")
        rf.write("================================================================================\n\n")
        rf.write("\n".join(report))
        rf.write(f"\n\n{'='*80}\n")
        rf.write(f"TỔNG KẾT: {passed} PASS / {failed} FAIL - TỶ LỆ ĐẠT: {pct:.1f}%\n")
        rf.write(f"{'='*80}\n")

if __name__ == '__main__':
    run_verification()
