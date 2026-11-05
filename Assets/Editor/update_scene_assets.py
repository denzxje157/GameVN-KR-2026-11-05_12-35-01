import re, os, sys

SCENE_PATH = r'Assets/Scenes/MainGame.unity'

# Map of GameObject name -> new sprite GUID, color (r,g,b,a), and optional (w, h)
UPDATES = {
    'Btn_Title_LocalPlay': {'guid': '2dcc8110beec00b3dd85cda2888b5ced', 'color': '1, 1, 1, 1', 'size': (430, 190)},
    'Btn_Title_PvP': {'guid': 'b9d0ecee42535602f419fa8d42adbcf9', 'color': '1, 1, 1, 1', 'size': (430, 180)},
    'Btn_Title_Codex': {'guid': 'a97b544617a669ebd79d6e496740faeb', 'color': '1, 1, 1, 1', 'size': (430, 180)},
    'Btn_Title_Tutorial': {'guid': '7df284eb0c3af30027e005d3bc4f3dea', 'color': '1, 1, 1, 1', 'size': (430, 180)},
    'Settings_Popup': {'guid': '81bdceba2c6ac81b6ba01d900a2cda29', 'color': '1, 1, 1, 1', 'size': (1050, 620)},
    'Btn_CloseSettings': {'guid': '15d74f73007b5bb1357c114e6ac79e1d', 'color': '1, 1, 1, 1', 'size': (240, 120)},
    'Btn_Close_X': {'guid': '2d2888f7b3c5a306880bb8dfa804ab48', 'color': '1, 1, 1, 1', 'size': (70, 70)},
    'PvP_Popup': {'guid': '61c799aff6f226db37029fa8a873064c', 'color': '1, 1, 1, 1', 'size': (950, 630)},
    'Btn_ClosePvP': {'guid': '15d74f73007b5bb1357c114e6ac79e1d', 'color': '1, 1, 1, 1', 'size': (240, 120)},
    'Tutorial_Popup': {'guid': 'd644e4c775f6b19d004225d6e8b7db63', 'color': '1, 1, 1, 1', 'size': (900, 570)},
    'Btn_CloseTutorial': {'guid': '15d74f73007b5bb1357c114e6ac79e1d', 'color': '1, 1, 1, 1', 'size': (240, 120)},
    'Codex_Popup': {'guid': 'd644e4c775f6b19d004225d6e8b7db63', 'color': '1, 1, 1, 1', 'size': (900, 570)},
    'Btn_CloseCodex': {'guid': '15d74f73007b5bb1357c114e6ac79e1d', 'color': '1, 1, 1, 1', 'size': (240, 120)},
    'Panel_CharSelect': {'guid': 'bd2de4f112e212ce47f6efc449c4c827', 'color': '1, 1, 1, 1', 'size': (750, 750)},
    'Btn_StartBattle': {'guid': '6c2b35f2c62781ef15e628ef30d213f9', 'color': '1, 1, 1, 1', 'size': (480, 135)},
    'Btn_BackToTitle': {'guid': '4c0ae8798cdb3f3c7687724de49e1f00', 'color': '1, 1, 1, 1', 'size': (230, 105)},
    'Btn_Tutorial': {'guid': '7380fb0ace513827ffb7466ad35a662d', 'color': '1, 1, 1, 1', 'size': (230, 105)},
    'Btn_MatchHub_Settings': {'guid': '8b552a3ae832eb1aba8a69098f619cde', 'color': '1, 1, 1, 1', 'size': (230, 105)},
    'VictoryDefeat_Popup': {'guid': 'dea893e64c8cd35ef3f584e8b851e1c6', 'color': '1, 1, 1, 1', 'size': (750, 680)}
}

with open(SCENE_PATH, 'r', encoding='utf-8') as f:
    scene_text = f.read()

# Build GO ID map to RectTransform ID and Image ID
go_to_rt = {}
go_to_img = {}
go_names = {}

for m in re.finditer(r'--- !u!1 &([0-9]+)\nGameObject:.*?m_Name: ([^\n]+)', scene_text, re.DOTALL):
    go_id = m.group(1)
    name = m.group(2).strip()
    go_names[go_id] = name

# Find all RectTransforms
for m in re.finditer(r'--- !u!224 &([0-9]+)\nRectTransform:.*?m_GameObject: \{fileID: ([0-9]+)\}', scene_text, re.DOTALL):
    rt_id = m.group(1)
    go_id = m.group(2)
    go_to_rt[go_id] = rt_id

# Find all Images
for m in re.finditer(r'--- !u!114 &([0-9]+)\nMonoBehaviour:.*?m_GameObject: \{fileID: ([0-9]+)\}.*?m_Script: \{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3\}', scene_text, re.DOTALL):
    img_id = m.group(1)
    go_id = m.group(2)
    go_to_img[go_id] = img_id

print(f'Parsed {len(go_names)} GameObjects, {len(go_to_rt)} RectTransforms, {len(go_to_img)} Images.')

# Also update Slider Backgrounds and Handles
slider_bg_guid = '1c1ecbd202b624038fe3f9dee8f51f23' # slider_bar_wood
slider_handle_guid = 'e7bf1c16abcb29316475fb6ece8bd872' # drum knob

modified_count = 0

for go_id, name in go_names.items():
    if name in UPDATES:
        info = UPDATES[name]
        # Update Image
        if go_id in go_to_img:
            img_id = go_to_img[go_id]
            # Replace sprite guid and color in this Image component
            pattern = re.compile(rf'(--- !u!114 &{img_id}\nMonoBehaviour:.*?'
                                 rf'm_Color: \{{r: [^,]+, g: [^,]+, b: [^,]+, a: [^}}]+\}}.*?'
                                 rf'm_Sprite: \{{fileID: [0-9-]+, guid: )([a-f0-9]+)(, type: [0-9]+\}}.*?'
                                 rf'm_PreserveAspect: )([0-9])', re.DOTALL)
            
            def repl_img(match):
                prefix = match.group(1)
                col_r, col_g, col_b, col_a = [x.strip() for x in info['color'].split(',')]
                prefix_mod = re.sub(r'm_Color: \{r: [^,]+, g: [^,]+, b: [^,]+, a: [^}]+\}',
                                    f'm_Color: {{r: {col_r}, g: {col_g}, b: {col_b}, a: {col_a}}}',
                                    prefix)
                return f"{prefix_mod}{info['guid']}{match.group(3)}1"
            
            new_text, n = pattern.subn(repl_img, scene_text)
            if n > 0:
                scene_text = new_text
                modified_count += 1
                print(f'Updated Image on {name} -> GUID {info["guid"]}, Color ({info["color"]})')
            else:
                pattern_simple = re.compile(rf'(--- !u!114 &{img_id}\nMonoBehaviour:.*?'
                                            rf'm_Sprite: \{{fileID: [0-9-]+, guid: )([a-f0-9]+)(, type: [0-9]+\}})', re.DOTALL)
                def repl_simple(match):
                    return f"{match.group(1)}{info['guid']}{match.group(3)}"
                new_text, n = pattern_simple.subn(repl_simple, scene_text)
                if n > 0:
                    scene_text = new_text
                    modified_count += 1
                    print(f'Updated Image (simple) on {name} -> GUID {info["guid"]}')

        # Update RectTransform sizeDelta if requested
        if 'size' in info and go_id in go_to_rt:
            rt_id = go_to_rt[go_id]
            w, h = info['size']
            rt_pattern = re.compile(rf'(--- !u!224 &{rt_id}\nRectTransform:.*?'
                                    rf'm_SizeDelta: \{{x: )([^,]+)(, y: )([^}}]+)(\}})', re.DOTALL)
            def repl_rt(match):
                return f"{match.group(1)}{w}{match.group(3)}{h}{match.group(5)}"
            new_text, n = rt_pattern.subn(repl_rt, scene_text)
            if n > 0:
                scene_text = new_text
                print(f'Updated RectTransform size on {name} -> {w}x{h}')

# Update Slider Backgrounds (name: Background) and Handles (name: Handle) inside Slider_BGM, Slider_SFX, Slider_Crowd
for go_id, name in go_names.items():
    if name == 'Background' and go_id in go_to_img:
        img_id = go_to_img[go_id]
        pattern = re.compile(rf'(--- !u!114 &{img_id}\nMonoBehaviour:.*?'
                             rf'm_Sprite: \{{fileID: [0-9-]+, guid: )([a-f0-9]+)(, type: [0-9]+\}})', re.DOTALL)
        def repl_bg(match):
            return f"{match.group(1)}{slider_bg_guid}{match.group(3)}"
        scene_text, n = pattern.subn(repl_bg, scene_text)
        if n > 0:
            print(f'Updated Slider Background (Image {img_id}) -> {slider_bg_guid}')
    elif name == 'Handle' and go_id in go_to_img:
        img_id = go_to_img[go_id]
        pattern = re.compile(rf'(--- !u!114 &{img_id}\nMonoBehaviour:.*?'
                             rf'm_Sprite: \{{fileID: [0-9-]+, guid: )([a-f0-9]+)(, type: [0-9]+\}})', re.DOTALL)
        def repl_handle(match):
            return f"{match.group(1)}{slider_handle_guid}{match.group(3)}"
        scene_text, n = pattern.subn(repl_handle, scene_text)
        if n > 0:
            print(f'Updated Slider Handle (Image {img_id}) -> {slider_handle_guid}')

with open(SCENE_PATH, 'w', encoding='utf-8') as f:
    f.write(scene_text)

print(f'Scene update complete! Total target updates: {modified_count}')
