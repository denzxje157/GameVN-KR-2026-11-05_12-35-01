import sys, re

sys.stdout.reconfigure(encoding='utf-8')

scene_path = 'Assets/Scenes/MainGame.unity'

with open(scene_path, 'r', encoding='utf-8') as f:
    text = f.read()

docs = text.split('--- !u!')
print(f"Total YAML blocks: {len(docs)}")

# Map fileID -> doc text
doc_map = {}
for d in docs:
    m = re.match(r'(\d+)\s+&(\d+)', d)
    if m:
        doc_map[m.group(2)] = d

def get_name(d):
    m = re.search(r'm_Name:\s*(.+)', d)
    return m.group(1).strip() if m else None

# Let's inspect objects of interest
for fid, d in doc_map.items():
    name = get_name(d)
    if name in ['Panel_MapSelect', 'Panel_CharSelect', 'Card_Player', 'Card_Enemy', 
                'Btn_PrevMap', 'Btn_NextMap', 'Btn_PrevPlayer', 'Btn_NextPlayer',
                'Btn_PrevEnemy', 'Btn_NextEnemy', 'Btn_NextPlayerText', 'Btn_NextEnemyText',
                'Icon_VS', 'Btn_StartBattle', 'Btn_BackToTitle', 'Btn_Tutorial', 'Btn_Sound',
                'Settings_Popup', 'Btn_CloseSettings', 'Slider_BGM', 'Slider_SFX']:
        print(f"Object: {name} (&{fid})")
