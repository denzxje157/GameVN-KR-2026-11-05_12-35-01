import os
from PIL import Image

def composite_match_hub_preview():
    # Base canvas: 1280x720
    canvas_w, canvas_h = 1280, 720
    bg_path = 'Assets/Sprites/Maps/san1-lang.png'
    if os.path.exists(bg_path):
        bg = Image.open(bg_path).convert('RGBA').resize((canvas_w, canvas_h))
    else:
        bg = Image.new('RGBA', (canvas_w, canvas_h), (30, 20, 40, 255))

    # Dim overlay
    dim = Image.new('RGBA', (canvas_w, canvas_h), (0, 0, 0, 100))
    bg.alpha_composite(dim)

    # Left board: Panel_MapSelect (570, 520) at x=-305, y=25
    # Unity coords: (0,0) is center (640, 360). x=-305 => 640 - 305 = 335. y=25 => 360 - 25 = 335
    map_panel_path = 'Assets/Sprites/UI/MatchHub/frame_settings_window.png'
    # Or panel_map
    # Right board: panel_char_select_vn (570, 520) at x=305, y=25 => center 640 + 305 = 945, 335
    char_panel_path = 'Assets/Sprites/UI/MatchHub/panel_char_select_vn.png'
    if os.path.exists(char_panel_path):
        char_board = Image.open(char_panel_path).convert('RGBA').resize((380, 350))
        # paste onto bg
        bx = 945 - 380//2
        by = 335 - 350//2
        bg.paste(char_board, (bx, by), char_board)

        # Place Player avatar at center (-142, 50) relative to char_board center
        # In char_board coords (w=380, h=350): center is (190, 175)
        # x = 190 - 95 = 95, y = 175 - 35 = 140
        av_path = 'Assets/Resources/UI/Avatars/avatar_trau_vang.png'
        if os.path.exists(av_path):
            av = Image.open(av_path).convert('RGBA').resize((75, 75))
            frame_path = 'Assets/Resources/UI/Battle/avatar_frame_gold_round.png'
            fr = Image.open(frame_path).convert('RGBA').resize((85, 85))
            ax = bx + 95 - 75//2
            ay = by + 140 - 75//2
            bg.paste(av, (ax, ay), av)
            bg.paste(fr, (ax - 5, ay - 5), fr)

        # Place Enemy avatar at center (+142, 50)
        en_path = 'Assets/Resources/UI/Avatars/avatar_ho_baekho.png'
        if os.path.exists(en_path):
            en_av = Image.open(en_path).convert('RGBA').resize((75, 75))
            fr = Image.open(frame_path).convert('RGBA').resize((85, 85))
            eax = bx + 285 - 75//2
            eay = by + 140 - 75//2
            bg.paste(en_av, (eax, eay), en_av)
            bg.paste(fr, (eax - 5, eay - 5), fr)

        # VS badge in center
        vs_path = 'Assets/Sprites/UI/Battle/badge_vs_fire.png'
        if os.path.exists(vs_path):
            vs = Image.open(vs_path).convert('RGBA').resize((55, 75))
            bg.paste(vs, (bx + 190 - 27, by + 130), vs)

    # Bottom buttons: y = -365 => 360 + 260 = 620
    btn_back_path = 'Assets/Resources/UI/MatchHub/btn_quay_lai_vn.png'
    if os.path.exists(btn_back_path):
        bb = Image.open(btn_back_path).convert('RGBA').resize((120, 50))
        bg.paste(bb, (640 - 320, 620), bb)

    btn_start_path = 'Assets/Resources/UI/MatchHub/btn_vao_tran_fire_vn.png'
    if os.path.exists(btn_start_path):
        bs = Image.open(btn_start_path).convert('RGBA').resize((200, 65))
        bg.paste(bs, (640 - 100, 610), bs)

    btn_tut_path = 'Assets/Resources/UI/MatchHub/sign_huong_dan_vn.png'
    if os.path.exists(btn_tut_path):
        bt = Image.open(btn_tut_path).convert('RGBA').resize((120, 50))
        bg.paste(bt, (640 + 130, 620), bt)

    btn_set_path = 'Assets/Resources/UI/MatchHub/sign_cai_dat_vn.png'
    if os.path.exists(btn_set_path):
        bset = Image.open(btn_set_path).convert('RGBA').resize((120, 50))
        bg.paste(bset, (640 + 270, 620), bset)

    out_path = 'C:/Users/Admin/.gemini/antigravity/brain/777661cc-5705-48ae-8e41-a23175dba7d6/match_hub_preview.png'
    bg.save(out_path)
    print('Saved match_hub_preview.png')

composite_match_hub_preview()
