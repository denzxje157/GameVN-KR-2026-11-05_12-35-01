import os, re

files = [
    'Assets/Sprites/UI/banner_victory.png.meta',
    'Assets/Sprites/UI/banner_defeat.png.meta',
    'Assets/Sprites/UI/MatchHub/card_character_vn.png.meta',
    'Assets/Sprites/UI/MatchHub/card_character_kr.png.meta',
    'Assets/Sprites/UI/MatchHub/btn_vao_tran_fire.png.meta',
    'Assets/Sprites/UI/MatchHub/btn_quay_lai.png.meta',
    'Assets/Sprites/UI/arrow_red_left.png.meta',
    'Assets/Sprites/UI/arrow_yellow_right.png.meta',
    'Assets/Sprites/UI/avatar_frame_circle.png.meta',
    'Assets/Sprites/UI/avatar_frame_square.png.meta',
]

for f in files:
    if os.path.exists(f):
        with open(f, 'r', encoding='utf-8') as mf:
            m = re.search(r'guid:\s*([0-9a-fA-F]+)', mf.read())
            print(f"{os.path.basename(f)[:-5]}: {m.group(1) if m else 'None'}")
    else:
        print(f"Missing: {f}")
