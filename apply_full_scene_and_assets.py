import sys, re

sys.stdout.reconfigure(encoding='utf-8')

scene_path = 'Assets/Scenes/MainGame.unity'

with open(scene_path, 'r', encoding='utf-8') as f:
    text = f.read()

docs = text.split('--- !u!')
header = docs[0]
doc_list = docs[1:]

doc_map = {}
for i, d in enumerate(doc_list):
    m = re.match(r'(\d+)\s+&(\d+)', d)
    if m:
        doc_map[m.group(2)] = (i, d)

def replace_field(doc_str, field, value):
    pattern = rf'({field}:\s*).*'
    return re.sub(pattern, rf'\g<1>{value}', doc_str)

def set_sprite_and_color(doc_str, guid):
    # Replace m_Color
    doc_str = re.sub(r'm_Color:\s*\{[^}]*\}', 'm_Color: {r: 1, g: 1, b: 1, a: 1}', doc_str)
    # Replace m_Sprite
    doc_str = re.sub(r'm_Sprite:\s*\{[^}]*\}', f'm_Sprite: {{fileID: 21300000, guid: {guid}, type: 3}}', doc_str)
    return doc_str

def set_rect(doc_str, pos_x, pos_y, width, height):
    doc_str = re.sub(r'm_AnchoredPosition:\s*\{[^}]*\}', f'm_AnchoredPosition: {{x: {pos_x}, y: {pos_y}}}', doc_str)
    doc_str = re.sub(r'm_SizeDelta:\s*\{[^}]*\}', f'm_SizeDelta: {{x: {width}, y: {height}}}', doc_str)
    return doc_str

# 1. Panel_MapSelect (&913064357 is Image, &913064356 is RectTransform)
idx, d = doc_map['913064357']
doc_list[idx] = set_sprite_and_color(d, '77f16e8302b3d63d218828b37c814f12')

# 2. Btn_PrevMap (&961367641 is Image, &961367639 is RectTransform)
idx, d = doc_map['961367641']
doc_list[idx] = set_sprite_and_color(d, '653580e3c5ca7564c985c30ab29e3459')
idx, d = doc_map['961367639']
doc_list[idx] = set_rect(d, -130, -175, 52, 52)

# 3. Btn_NextMap (&1399508781 is Image, &1399508779 is RectTransform)
idx, d = doc_map['1399508781']
doc_list[idx] = set_sprite_and_color(d, 'f6c487f65a3af084793bda898b83f51b')
idx, d = doc_map['1399508779']
doc_list[idx] = set_rect(d, 130, -175, 52, 52)

# 4. Panel_CharSelect (&1349784647 is Image, &1349784646 is RectTransform)
idx, d = doc_map['1349784647']
doc_list[idx] = set_sprite_and_color(d, 'eda5159d5b90c681569e739aed502b24')
idx, d = doc_map['1349784646']
doc_list[idx] = set_rect(d, 380, 20, 620, 520)

# 5. Card_Player (&433538158 is Image, &433538157 is RectTransform)
idx, d = doc_map['433538158']
doc_list[idx] = set_sprite_and_color(d, '3853b00010fe92e3c48783f1820d492d')
idx, d = doc_map['433538157']
doc_list[idx] = set_rect(d, -150, 45, 230, 280)

# 6. Btn_PrevPlayer (&1646757769 is Image, &1646757767 is RectTransform)
idx, d = doc_map['1646757769']
doc_list[idx] = set_sprite_and_color(d, '653580e3c5ca7564c985c30ab29e3459')
idx, d = doc_map['1646757767']
doc_list[idx] = set_rect(d, -280, 45, 46, 46)

# 7. Btn_NextPlayer (&1939139190 is Image, &1939139188 is RectTransform)
idx, d = doc_map['1939139190']
doc_list[idx] = set_sprite_and_color(d, 'f6c487f65a3af084793bda898b83f51b')
idx, d = doc_map['1939139188']
doc_list[idx] = set_rect(d, -20, 45, 46, 46)

# 8. Btn_NextPlayerText (&785262823 is Image, &785262821 is RectTransform)
idx, d = doc_map['785262823']
doc_list[idx] = set_sprite_and_color(d, '54f561619c200c76cd4a6a4090ed34b7')
idx, d = doc_map['785262821']
doc_list[idx] = set_rect(d, -150, -140, 180, 46)

# 9. Card_Enemy (&1439241751 is Image, &1439241750 is RectTransform)
idx, d = doc_map['1439241751']
doc_list[idx] = set_sprite_and_color(d, '20903d2ef1bfd39772d4758110c1ba59')
idx, d = doc_map['1439241750']
doc_list[idx] = set_rect(d, 150, 45, 230, 280)

# 10. Btn_PrevEnemy (&252689967 is Image, &252689965 is RectTransform)
idx, d = doc_map['252689967']
doc_list[idx] = set_sprite_and_color(d, '653580e3c5ca7564c985c30ab29e3459')
idx, d = doc_map['252689965']
doc_list[idx] = set_rect(d, 20, 45, 46, 46)

# 11. Btn_NextEnemy (&2141801379 is Image, &2141801377 is RectTransform)
idx, d = doc_map['2141801379']
doc_list[idx] = set_sprite_and_color(d, '2d9ed93032198b74183497f09b474adf')
idx, d = doc_map['2141801377']
doc_list[idx] = set_rect(d, 280, 45, 46, 46)

# 12. Btn_NextEnemyText (&1617997804 is Image, &1617997802 is RectTransform)
idx, d = doc_map['1617997804']
doc_list[idx] = set_sprite_and_color(d, '4fa1bb24cb8e1e5aa808ab8530868cd7')
idx, d = doc_map['1617997802']
doc_list[idx] = set_rect(d, 150, -140, 180, 46)

# 13. Btn_StartBattle (&1042492377 is Image, &1042492375 is RectTransform)
idx, d = doc_map['1042492377']
doc_list[idx] = set_sprite_and_color(d, 'a036e4c43a3c3238bfe6676767cc1f02')
idx, d = doc_map['1042492375']
doc_list[idx] = set_rect(d, 0, -380, 360, 115)

# 14. Btn_BackToTitle (&2135314115 is Image, &2135314113 is RectTransform)
idx, d = doc_map['2135314115']
doc_list[idx] = set_sprite_and_color(d, '2b619472eae67ce31a722f8439b2b431')
idx, d = doc_map['2135314113']
doc_list[idx] = set_rect(d, -420, -380, 170, 72)

# 15. Btn_Tutorial on MatchHub (&884519566 is Image, &884519564 is RectTransform)
idx, d = doc_map['884519566']
doc_list[idx] = set_sprite_and_color(d, 'e1ecbb231b961ea83ec0e84d94890ce8')
idx, d = doc_map['884519564']
doc_list[idx] = set_rect(d, 320, -380, 190, 60)

# 16. Btn_Sound on MatchHub (&1272012451 is Image, &1272012449 is RectTransform)
idx, d = doc_map['1272012451']
doc_list[idx] = set_sprite_and_color(d, '557d6b213d48caa9b0cf913f3345d626')
idx, d = doc_map['1272012449']
doc_list[idx] = set_rect(d, 520, -380, 180, 60)

# 17. Slider_SFX handle (&1371512848 is Image)
idx, d = doc_map['1371512848']
doc_list[idx] = set_sprite_and_color(d, 'd4e5f60718293a4b5c6d7e8f01a2b3c5')

# Re-assemble scene
new_scene_text = header + '--- !u!' + '--- !u!'.join(doc_list)

with open(scene_path, 'w', encoding='utf-8') as f:
    f.write(new_scene_text)

print("Scene successfully patched with all authentic UI assets!")
