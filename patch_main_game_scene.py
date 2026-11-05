# -*- coding: utf-8 -*-
import sys, re

sys.stdout.reconfigure(encoding='utf-8')
print('=== BẮT ĐẦU PATCH TOÀN DIỆN CHO MainGame.unity ===')

scene_path = 'Assets/Scenes/MainGame.unity'
with open(scene_path, 'r', encoding='utf-8') as f:
    text = f.read()

def replace_block(text, obj_header, replacements):
    pattern = re.escape(obj_header) + r'.*?(?=\n--- !u!)'
    m = re.search(pattern, text, re.DOTALL)
    if not m:
        print(f'[WARN] Không tìm thấy block: {obj_header}')
        return text
    block = m.group(0)
    for old_val, new_val in replacements:
        if isinstance(old_val, str):
            block = block.replace(old_val, new_val)
        else:
            block = re.sub(old_val, new_val, block)
    return text[:m.start()] + block + text[m.end():]

# 1. SETTINGS POPUP: Xóa chữ tiêu đề bị đè
text = replace_block(text, '--- !u!114 &1505063466', [
    (r'm_text: "[^"]*"', 'm_text: ""'),
    (r'm_Text: "[^"]*"', 'm_Text: ""'),
])

# Settings Popup Language Toggle: Dùng nút gỗ thật, text Tiếng Việt
text = replace_block(text, '--- !u!114 &933379074', [
    (r'guid: [a-f0-9]{32}', 'guid: a6da98b4857e43d2b4f14690b8ab0127'), # btn_back_wood
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 1}'),
])
text = replace_block(text, '--- !u!224 &933379072', [
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 210, y: 44}'),
])
text = replace_block(text, '--- !u!114 &1818684053', [
    (r'm_text: "[^"]*"', 'm_text: "Ti\\u1EBFng Vi\\u1EC7t"'),
    (r'm_fontColor: \{[^\}]+\}', 'm_fontColor: {r: 0.22, g: 0.12, b: 0.05, a: 1}'),
])
text = replace_block(text, '--- !u!114 &1683853763', [
    (r'm_text: "[^"]*"', 'm_text: "Ng\\xF4n Ng\\u1EEF"'),
])

# Settings Sliders: Background là track gỗ, Fill trong suốt, Knob là trống/thái cực
# Slider BGM
text = replace_block(text, '--- !u!114 &849543639', [
    (r'guid: [a-f0-9]{32}', 'guid: 9b6c79a0fff94b7b83b19a1edc768a70'), # slider_bgm_track
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 1}'),
])
text = replace_block(text, '--- !u!114 &1523785452', [
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 0.95, g: 0.72, b: 0.22, a: 0.4}'),
])
text = replace_block(text, '--- !u!114 &337345307', [
    (r'guid: [a-f0-9]{32}', 'guid: c4c1702df75f4e33844e2d1261e9c11f'), # slider_knob_gold_drum
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 1}'),
])
text = replace_block(text, '--- !u!224 &337345306', [
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 40, y: 40}'),
])

# Audio Labels
text = replace_block(text, '--- !u!114 &1978466685', [(r'm_text: "[^"]*"', 'm_text: "Nh\\u1EA1c N\\u1EC1n"')])
text = replace_block(text, '--- !u!114 &1798914722', [(r'm_text: "[^"]*"', 'm_text: "Hi\\u1EC7u \\u1EE8ng"')])
text = replace_block(text, '--- !u!114 &1841148188', [(r'm_text: "[^"]*"', 'm_text: "Kh\\xE1n Gi\\u1EA3"')])

# Settings Popup: SizeDelta 960x620, Nút Đóng gỗ
text = replace_block(text, '--- !u!224 &1847102480', [
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 960, y: 620}'),
])
text = replace_block(text, '--- !u!224 &1396542018', [
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 200, y: 80}'),
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 0, y: -195}'),
])
text = replace_block(text, '--- !u!114 &1396542020', [
    (r'guid: [a-f0-9]{32}', 'guid: f1a2b3c4d5e60718293a4b5c6d7e8f90'),
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 1}'),
])

# 2. TITLE SCREEN 4 BUTTONS (Rộng rãi 280x125, rực rỡ, xóa chữ trùng đè)
text = replace_block(text, '--- !u!114 &1282533292', [
    (r'guid: [a-f0-9]{32}', 'guid: 2dcc8110beec00b3dd85cda2888b5ced'), # btn_title_local_vn
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 1}'),
    (r'm_PreserveAspect: \d+', 'm_PreserveAspect: 1'),
])
text = replace_block(text, '--- !u!224 &1282533290', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: -155, y: -155}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 280, y: 125}'),
])

text = replace_block(text, '--- !u!114 &1969374577', [
    (r'guid: [a-f0-9]{32}', 'guid: b9d0ecee42535602f419fa8d42adbcf9'), # btn_title_pvp_vn
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 1}'),
    (r'm_PreserveAspect: \d+', 'm_PreserveAspect: 1'),
])
text = replace_block(text, '--- !u!224 &1969374575', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 155, y: -155}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 280, y: 125}'),
])

text = replace_block(text, '--- !u!114 &862241306', [
    (r'guid: [a-f0-9]{32}', 'guid: a97b544617a669ebd79d6e496740faeb'), # btn_title_codex_vn
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 1}'),
    (r'm_PreserveAspect: \d+', 'm_PreserveAspect: 1'),
])
text = replace_block(text, '--- !u!224 &862241304', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: -155, y: -275}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 280, y: 125}'),
])

text = replace_block(text, '--- !u!114 &1916881639', [
    (r'guid: [a-f0-9]{32}', 'guid: 7df284eb0c3af30027e005d3bc4f3dea'), # btn_title_guide_vn
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 1}'),
    (r'm_PreserveAspect: \d+', 'm_PreserveAspect: 1'),
])
text = replace_block(text, '--- !u!224 &1916881637', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 155, y: -275}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 280, y: 125}'),
])

# Title Screen Settings & Sound Buttons
text = replace_block(text, '--- !u!114 &1448996592', [
    (r'guid: [a-f0-9]{32}', 'guid: 149366169e8a05f5e786cf770782546a'),
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 1}'),
])
text = replace_block(text, '--- !u!224 &1448996590', [
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 65, y: 65}'),
])

text = replace_block(text, '--- !u!114 &357200199', [
    (r'guid: [a-f0-9]{32}', 'guid: 197f2e7d2ed5fbac2ebf2b3c107edfc6'),
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 1}'),
])
text = replace_block(text, '--- !u!224 &357200197', [
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 65, y: 65}'),
])

# 3. TUTORIAL POPUP (panel_codex_vn, xóa Text_Title, chữ nâu cà phê)
text = replace_block(text, '--- !u!114 &973450860', [
    (r'guid: [a-f0-9]{32}', 'guid: d644e4c775f6b19d004225d6e8b7db63'),
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 1}'),
    (r'm_PreserveAspect: \d+', 'm_PreserveAspect: 1'),
])
text = replace_block(text, '--- !u!224 &973450859', [
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 880, y: 560}'),
])
text = replace_block(text, '--- !u!114 &556679260', [
    (r'm_Text: "[^"]*"', 'm_Text: ""'),
])
text = replace_block(text, '--- !u!114 &1304611765', [
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 0.22, g: 0.12, b: 0.05, a: 1}'),
])
text = replace_block(text, '--- !u!224 &1304611764', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 0, y: 15}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 740, y: 350}'),
])
text = replace_block(text, '--- !u!114 &357914174', [
    (r'guid: [a-f0-9]{32}', 'guid: f1a2b3c4d5e60718293a4b5c6d7e8f90'),
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 1}'),
])
text = replace_block(text, '--- !u!224 &357914172', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 0, y: -220}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 180, y: 70}'),
])

# 4. CODEX & PVP POPUPS
text = replace_block(text, '--- !u!114 &1343639007', [(r'm_Text: "[^"]*"', 'm_Text: ""')])
text = replace_block(text, '--- !u!114 &1535921528', [
    (r'guid: [a-f0-9]{32}', 'guid: f1a2b3c4d5e60718293a4b5c6d7e8f90'),
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 1}'),
])
text = replace_block(text, '--- !u!224 &1535921526', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 0, y: -220}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 180, y: 70}'),
])

# 5. MATCHHUB CHARACTER SELECTION REDESIGN
# Panel_CharSelect kích thước 640x560
text = replace_block(text, '--- !u!224 &65584978', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 320, y: 25}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 640, y: 560}'),
])
# Label_Char xóa chữ đè
text = replace_block(text, '--- !u!114 &824707641', [(r'm_Text: "[^"]*"', 'm_Text: ""')])

# Card_Player: Thẻ đình mái rồng đỏ card_confirm_red_vn
text = replace_block(text, '--- !u!114 &489964180', [
    (r'guid: [a-f0-9]{32}', 'guid: 70a9a731ef77c51a3bd3d6a578d65c24'),
    (r'm_PreserveAspect: \d+', 'm_PreserveAspect: 1'),
])
text = replace_block(text, '--- !u!224 &489964179', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: -155, y: -15}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 270, y: 325}'),
])

# Card_Enemy: Thẻ đình mái hổ xanh card_select_blue_kr
text = replace_block(text, '--- !u!114 &1435379398', [
    (r'guid: [a-f0-9]{32}', 'guid: 6993103959a1f5da7181444b953c8d6e'),
    (r'm_PreserveAspect: \d+', 'm_PreserveAspect: 1'),
])
text = replace_block(text, '--- !u!224 &1435379397', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 155, y: -15}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 270, y: 325}'),
])

# Icon_VS: Huy hiệu VS bốc lửa badge_vs_fire
text = replace_block(text, '--- !u!114 &1419472564', [
    (r'guid: [a-f0-9]{32}', 'guid: fcb31382f2dbfae0d1974c91297a2573'),
    (r'm_PreserveAspect: \d+', 'm_PreserveAspect: 1'),
])
text = replace_block(text, '--- !u!224 &1419472563', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 0, y: -15}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 105, y: 105}'),
])

# Khung Avatar tròn vàng avatar_frame_gold_round
text = replace_block(text, '--- !u!114 &1144639303', [
    (r'guid: [a-f0-9]{32}', 'guid: 006dc7da506c99ef3c30f1d3ce65d64d'),
    (r'm_PreserveAspect: \d+', 'm_PreserveAspect: 1'),
])
text = replace_block(text, '--- !u!224 &1144639301', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 0, y: 45}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 120, y: 120}'),
])

text = replace_block(text, '--- !u!114 &1407639038', [
    (r'guid: [a-f0-9]{32}', 'guid: 006dc7da506c99ef3c30f1d3ce65d64d'),
    (r'm_PreserveAspect: \d+', 'm_PreserveAspect: 1'),
])
text = replace_block(text, '--- !u!224 &1407639036', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 0, y: 45}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 120, y: 120}'),
])

# Image_PlayerAvatar & Image_EnemyAvatar
text = replace_block(text, '--- !u!224 &448847965', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 0, y: 45}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 100, y: 100}'),
])
text = replace_block(text, '--- !u!224 &333058522', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 0, y: 45}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 100, y: 100}'),
])

# Text Tướng & Vai Trò (Nâu đậm sắc nét)
text = replace_block(text, '--- !u!114 &1929265090', [
    (r'm_fontColor: \{[^\}]+\}', 'm_fontColor: {r: 0.22, g: 0.12, b: 0.05, a: 1}'),
])
text = replace_block(text, '--- !u!224 &1929265089', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 0, y: -38}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 240, y: 30}'),
])

text = replace_block(text, '--- !u!114 &534303106', [
    (r'm_fontColor: \{[^\}]+\}', 'm_fontColor: {r: 0.45, g: 0.25, b: 0.1, a: 1}'),
])
text = replace_block(text, '--- !u!224 &534303105', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 0, y: -65}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 240, y: 28}'),
])

text = replace_block(text, '--- !u!114 &918483173', [
    (r'm_fontColor: \{[^\}]+\}', 'm_fontColor: {r: 0.22, g: 0.12, b: 0.05, a: 1}'),
])
text = replace_block(text, '--- !u!224 &918483172', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 0, y: -38}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 240, y: 30}'),
])

text = replace_block(text, '--- !u!114 &153162633', [
    (r'm_fontColor: \{[^\}]+\}', 'm_fontColor: {r: 0.45, g: 0.25, b: 0.1, a: 1}'),
])
text = replace_block(text, '--- !u!224 &153162632', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 0, y: -65}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 240, y: 28}'),
])

# Ẩn nút xấu Btn_NextPlayerText và Btn_NextEnemyText (trong suốt)
text = replace_block(text, '--- !u!114 &867950096', [
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 0}'),
])
text = replace_block(text, '--- !u!224 &867950094', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: -155, y: -120}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 200, y: 55}'),
])

text = replace_block(text, '--- !u!114 &678028740', [
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 0}'),
])
text = replace_block(text, '--- !u!224 &678028738', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 155, y: -120}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 200, y: 55}'),
])

# Arrows vị trí cân đối
text = replace_block(text, '--- !u!224 &272225228', [ # Btn_PrevPlayer
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: -290, y: -15}'),
])
text = replace_block(text, '--- !u!224 &761837525', [ # Btn_NextPlayer
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: -20, y: -15}'),
])
text = replace_block(text, '--- !u!224 &910255536', [ # Btn_PrevEnemy
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 20, y: -15}'),
])
text = replace_block(text, '--- !u!224 &1952272925', [ # Btn_NextEnemy
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 290, y: -15}'),
])

# Nút Điều Hướng Dưới Cùng Của MatchHub
text = replace_block(text, '--- !u!114 &1500651798', [ # Btn_BackToTitle
    (r'guid: [a-f0-9]{32}', 'guid: 025d57b1ce8544d998d363dca218bbad'), # btn_quay_lai_vn
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 1}'),
])
text = replace_block(text, '--- !u!224 &1500651796', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: -420, y: -300}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 190, y: 70}'),
])

text = replace_block(text, '--- !u!114 &1008614015', [ # Btn_StartBattle
    (r'guid: [a-f0-9]{32}', 'guid: b86ca556531548edb4b2cb20c36391a6'), # btn_vao_tran_fire_vn
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 1}'),
])
text = replace_block(text, '--- !u!224 &1008614013', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 0, y: -295}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 320, y: 90}'),
])

text = replace_block(text, '--- !u!114 &729790346', [ # Btn_Tutorial
    (r'guid: [a-f0-9]{32}', 'guid: a37e8c3a9f02497eb1df873fa73f3fc5'), # sign_huong_dan_vn
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 1}'),
])
text = replace_block(text, '--- !u!224 &729790344', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 290, y: -300}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 180, y: 65}'),
])

text = replace_block(text, '--- !u!114 &2121404757', [ # Btn_MatchHub_Settings
    (r'guid: [a-f0-9]{32}', 'guid: afa9b89e3467406a92823635777ce4d0'), # sign_cai_dat_vn
    (r'm_Color: \{[^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 1}'),
])
text = replace_block(text, '--- !u!224 &2121404755', [
    (r'm_AnchoredPosition: \{[^\}]+\}', 'm_AnchoredPosition: {x: 460, y: -300}'),
    (r'm_SizeDelta: \{[^\}]+\}', 'm_SizeDelta: {x: 160, y: 65}'),
])

# Map Select Header (Đơn ngữ chuẩn)
text = replace_block(text, '--- !u!114 &385437812', [ # Label_Map Text
    (r'm_Text: "[^"]*"', 'm_Text: "\\u2014 CH\\u1ECCN S\\u1EDAI \\u0110\\u1EA4U (3 B\\u1EA2N \\u0110\\u1ED2) \\u2014"'),
])

with open(scene_path, 'w', encoding='utf-8') as f:
    f.write(text)

print('✓ MainGame.unity đã cập nhật toàn diện 100%!')
