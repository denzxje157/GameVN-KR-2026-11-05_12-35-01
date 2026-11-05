import sys, re

sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Scenes/MainGame.unity', 'r', encoding='utf-8') as f:
    text = f.read()

docs = text.split('--- !u!')
doc_map = {}
for d in docs:
    m = re.match(r'(\d+)\s+&(\d+)', d)
    if m:
        doc_map[m.group(2)] = d

def inspect(fid, name):
    d = doc_map.get(str(fid), '')
    comps = re.findall(r'component:\s*\{fileID:\s*(\d+)\}', d)
    print(f"=== {name} (&{fid}) ===")
    for cid in comps:
        cdoc = doc_map.get(cid, '')
        lines = cdoc.split('\n')
        c_type = lines[0]
        # check sprite, color, rect
        info = []
        for l in lines:
            if any(k in l for k in ['m_Sprite:', 'm_Color:', 'm_SizeDelta:', 'm_AnchoredPosition:']):
                info.append(l.strip())
        print(f"  Comp &{cid} ({c_type[:15]}): {', '.join(info)}")

targets = [
    (913064355, 'Panel_MapSelect'),
    (961367638, 'Btn_PrevMap'),
    (1399508778, 'Btn_NextMap'),
    (1349784645, 'Panel_CharSelect'),
    (433538156, 'Card_Player'),
    (1646757766, 'Btn_PrevPlayer'),
    (1939139187, 'Btn_NextPlayer'),
    (785262820, 'Btn_NextPlayerText'),
    (1439241749, 'Card_Enemy'),
    (252689964, 'Btn_PrevEnemy'),
    (2141801376, 'Btn_NextEnemy'),
    (1617997801, 'Btn_NextEnemyText'),
    (1042492374, 'Btn_StartBattle'),
    (2135314112, 'Btn_BackToTitle'),
    (884519563, 'Btn_Tutorial'),
    (621654318, 'Btn_Sound'),
    (1272012448, 'Btn_Sound'),
    (2057269600, 'Settings_Popup'),
    (135490717, 'Slider_BGM'),
    (1335363155, 'Slider_SFX'),
    (1908103593, 'Btn_CloseSettings'),
]

for fid, name in targets:
    inspect(fid, name)
