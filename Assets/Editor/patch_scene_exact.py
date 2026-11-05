import re, sys

SCENE_PATH = r'Assets/Scenes/MainGame.unity'

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

slider_bg_guid = '1c1ecbd202b624038fe3f9dee8f51f23'
slider_handle_guid = 'e7bf1c16abcb29316475fb6ece8bd872'

with open(SCENE_PATH, 'r', encoding='utf-8') as f:
    content = f.read()

chunks = content.split('--- !u!')
header = chunks[0]
blocks = chunks[1:]

go_info = {} # go_id -> {'name': name, 'components': [fileIDs]}
comp_map = {} # comp_id -> block_text

for b in blocks:
    first_line = b.split('\n', 1)[0]
    type_id, comp_id = first_line.split('&')
    comp_id = comp_id.strip()
    if type_id.startswith('1 '):
        # GameObject
        m_name = re.search(r'm_Name: ([^\n]+)', b)
        name = m_name.group(1).strip() if m_name else ''
        comps = re.findall(r'- component: \{fileID: ([0-9]+)\}', b)
        go_info[comp_id] = {'name': name, 'components': comps}
    comp_map[comp_id] = (type_id, b)

print(f'Parsed {len(go_info)} GameObjects and {len(comp_map)} blocks.')

updated_blocks = {}

for go_id, info in go_info.items():
    name = info['name']
    if name in UPDATES:
        u = UPDATES[name]
        for cid in info['components']:
            if cid in comp_map:
                type_id, block = comp_map[cid]
                # Is it RectTransform?
                if type_id.startswith('224 ') and 'size' in u:
                    w, h = u['size']
                    block = re.sub(r'm_SizeDelta: \{x: [^,]+, y: [^}]+\}', f'm_SizeDelta: {{x: {w}, y: {h}}}', block)
                    comp_map[cid] = (type_id, block)
                    print(f'Updated size for {name}: {w}x{h}')
                # Is it Image? (guid: fe87c0e1cc204ed48ad3b37840f39efc)
                if 'guid: fe87c0e1cc204ed48ad3b37840f39efc' in block:
                    col_r, col_g, col_b, col_a = [x.strip() for x in u['color'].split(',')]
                    block = re.sub(r'm_Color: \{r: [^,]+, g: [^,]+, b: [^,]+, a: [^}]+\}',
                                   f'm_Color: {{r: {col_r}, g: {col_g}, b: {col_b}, a: {col_a}}}',
                                   block)
                    block = re.sub(r'm_Sprite: \{fileID: [0-9-]+, guid: [a-f0-9]+, type: [0-9]+\}',
                                   f'm_Sprite: {{fileID: 21300000, guid: {u["guid"]}, type: 3}}',
                                   block)
                    block = re.sub(r'm_PreserveAspect: [0-9]', 'm_PreserveAspect: 1', block)
                    comp_map[cid] = (type_id, block)
                    print(f'Updated Image for {name}: GUID {u["guid"]}')
    
    # Handle Slider backgrounds and knobs
    if name == 'Background':
        for cid in info['components']:
            if cid in comp_map:
                type_id, block = comp_map[cid]
                if 'guid: fe87c0e1cc204ed48ad3b37840f39efc' in block:
                    block = re.sub(r'm_Sprite: \{fileID: [0-9-]+, guid: [a-f0-9]+, type: [0-9]+\}',
                                   f'm_Sprite: {{fileID: 21300000, guid: {slider_bg_guid}, type: 3}}',
                                   block)
                    comp_map[cid] = (type_id, block)
                    print(f'Updated Slider Background -> {slider_bg_guid}')
    if name == 'Handle':
        for cid in info['components']:
            if cid in comp_map:
                type_id, block = comp_map[cid]
                if 'guid: fe87c0e1cc204ed48ad3b37840f39efc' in block:
                    block = re.sub(r'm_Sprite: \{fileID: [0-9-]+, guid: [a-f0-9]+, type: [0-9]+\}',
                                   f'm_Sprite: {{fileID: 21300000, guid: {slider_handle_guid}, type: 3}}',
                                   block)
                    comp_map[cid] = (type_id, block)
                    print(f'Updated Slider Handle -> {slider_handle_guid}')

# Reconstruct scene file
rebuilt = [header]
for b in blocks:
    first_line = b.split('\n', 1)[0]
    type_id, comp_id = first_line.split('&')
    comp_id = comp_id.strip()
    if comp_id in comp_map:
        _, updated_b = comp_map[comp_id]
        rebuilt.append(f'--- !u!{updated_b}')
    else:
        rebuilt.append(f'--- !u!{b}')

with open(SCENE_PATH, 'w', encoding='utf-8') as f:
    f.write(''.join(rebuilt))

print('Scene patched with 100% precision!')
