import re

scene_path = 'Assets/Scenes/MainGame.unity'
with open(scene_path, 'r', encoding='utf-8') as f:
    text = f.read()

def update_block(block_text, pattern, replacement):
    return re.sub(pattern, replacement, block_text, flags=re.MULTILINE)

def update_object_by_id(text, obj_id, updates):
    # Find block starting with --- !u!xxx &obj_id
    pattern = r'(--- !u!\d+ &' + str(obj_id) + r'\n[\s\S]*?)(?=\n--- !u|\Z)'
    match = re.search(pattern, text)
    if not match:
        print(f"Warning: Object &{obj_id} not found!")
        return text
    block = match.group(1)
    for p, r in updates:
        block = re.sub(p, r, block)
    return text[:match.start()] + block + text[match.end():]

print("Applying harmonized layout to MainGame.unity...")

# 1. Settings_Popup Text_Title: Hide it
text = update_object_by_id(text, '323371618', [
    (r'm_IsActive: \d+', 'm_IsActive: 0')
])
text = update_object_by_id(text, '323371620', [
    (r'm_text: [^\n]+', 'm_text: ""')
])

# Settings Labels: Monolingual Vietnamese default
text = re.sub(
    r'(m_Name: Text_Label_BGM[\s\S]*?m_text: )[^\n]+',
    r'\1"Nh\\u1EA1c N\\u1EC1n"',
    text
)
text = re.sub(
    r'(m_Name: Text_Label_SFX[\s\S]*?m_text: )[^\n]+',
    r'\1"Hi\\u1EC7u \\u1EE8ng"',
    text
)
text = re.sub(
    r'(m_Name: Text_Label_Crowd[\s\S]*?m_text: )[^\n]+',
    r'\1"Kh\\xE1n Gi\\u1EA3"',
    text
)
text = re.sub(
    r'(m_Name: Text_Label_Lang[\s\S]*?m_text: )[^\n]+',
    r'\1"Ng\\xF4n Ng\\u1EEF"',
    text
)
text = re.sub(
    r'(m_Name: Text_Value_Lang[\s\S]*?m_text: )[^\n]+',
    r'\1"Ti\\u1EBFng Vi\\u1EC7t"',
    text
)

# 2. Panel_MapSelect (Left board)
text = update_object_by_id(text, '1648662339', [
    (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: -305, y: 25}'),
    (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 570, y: 520}')
])

# 3. Panel_CharSelect (Right board)
text = update_object_by_id(text, '421533815', [
    (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: 305, y: 25}'),
    (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 570, y: 520}')
])

# 4. Card_Player (Transparent container)
text = update_object_by_id(text, '2125129853', [
    (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: -142, y: 20}'),
    (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 250, y: 340}')
])
text = update_object_by_id(text, '2125129854', [
    (r'm_Color: \{r: [^,]+, g: [^,]+, b: [^,]+, a: [^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 0}')
])

# 5. Card_Enemy (Transparent container)
text = update_object_by_id(text, '495753692', [
    (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: 142, y: 20}'),
    (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 250, y: 340}')
])
text = update_object_by_id(text, '495753693', [
    (r'm_Color: \{r: [^,]+, g: [^,]+, b: [^,]+, a: [^\}]+\}', 'm_Color: {r: 1, g: 1, b: 1, a: 0}')
])

# 6. Avatars & Frames
# Player Avatar
text = update_object_by_id(text, '905536917', [
    (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: 0, y: 50}'),
    (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 112, y: 112}')
])
text = update_object_by_id(text, '1518471550', [
    (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: 0, y: 50}'),
    (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 126, y: 126}')
])

# Enemy Avatar
text = update_object_by_id(text, '1327812025', [
    (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: 0, y: 50}'),
    (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 112, y: 112}')
])
text = update_object_by_id(text, '74896012', [
    (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: 0, y: 50}'),
    (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 126, y: 126}')
])

# 7. Mascot Texts
# Player Mascot Name & Role
text = update_object_by_id(text, '1508804959', [
    (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: 0, y: -45}'),
    (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 240, y: 32}')
])
text = update_object_by_id(text, '2047222741', [
    (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: 0, y: -88}'),
    (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 240, y: 46}')
])

# Enemy Mascot Name & Role
text = update_object_by_id(text, '1321985671', [
    (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: 0, y: -45}'),
    (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 240, y: 32}')
])
text = update_object_by_id(text, '1737165572', [
    (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: 0, y: -88}'),
    (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 240, y: 46}')
])

# 8. Mascot Navigation Arrows
# Btn_PrevPlayer (left arrow of player)
text = update_object_by_id(text, '653580e3c5ca7564c985c30ab29e3459', []) # check
# Find RectTransform of Btn_PrevPlayer
m_bp = re.search(r'm_Name: Btn_PrevPlayer[\s\S]*?m_Component:\n\s+- component: \{fileID: (\d+)\}', text)
if m_bp:
    rt_id = m_bp.group(1)
    text = update_object_by_id(text, rt_id, [
        (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: -217, y: 70}'),
        (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 44, y: 44}')
    ])

# Btn_NextPlayer (right arrow of player)
m_bn = re.search(r'm_Name: Btn_NextPlayer[\s\S]*?m_Component:\n\s+- component: \{fileID: (\d+)\}', text)
if m_bn:
    rt_id = m_bn.group(1)
    text = update_object_by_id(text, rt_id, [
        (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: -67, y: 70}'),
        (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 44, y: 44}')
    ])

# Icon_VS (center badge)
text = update_object_by_id(text, '1662286597', [
    (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: 0, y: 65}'),
    (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 82, y: 115}')
])

# Btn_PrevEnemy (left arrow of enemy)
m_pe = re.search(r'm_Name: Btn_PrevEnemy[\s\S]*?m_Component:\n\s+- component: \{fileID: (\d+)\}', text)
if m_pe:
    rt_id = m_pe.group(1)
    text = update_object_by_id(text, rt_id, [
        (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: 67, y: 70}'),
        (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 44, y: 44}')
    ])

# Btn_NextEnemy (right arrow of enemy)
m_ne = re.search(r'm_Name: Btn_NextEnemy[\s\S]*?m_Component:\n\s+- component: \{fileID: (\d+)\}', text)
if m_ne:
    rt_id = m_ne.group(1)
    text = update_object_by_id(text, rt_id, [
        (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: 217, y: 70}'),
        (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 44, y: 44}')
    ])

# 9. Bottom Row Buttons
# Btn_BackToTitle: Wooden carved back button
text = update_object_by_id(text, '1843109395', [
    (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: -420, y: -365}'),
    (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 170, y: 70}')
])
text = update_object_by_id(text, '1843109397', [
    (r'guid: [a-f0-9]+', 'guid: 5632aff58d675147ecdc4857256b723b')
])

# Btn_StartBattle
text = update_object_by_id(text, '75217495', [
    (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: 0, y: -360}'),
    (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 310, y: 95}')
])

# Btn_Tutorial
text = update_object_by_id(text, '2087633876', [
    (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: 315, y: -365}'),
    (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 170, y: 70}')
])

# Btn_MatchHub_Settings
text = update_object_by_id(text, '88473707', [
    (r'm_AnchoredPosition: \{x: [^,]+, y: [^\}]+\}', 'm_AnchoredPosition: {x: 485, y: -365}'),
    (r'm_SizeDelta: \{x: [^,]+, y: [^\}]+\}', 'm_SizeDelta: {x: 170, y: 70}')
])

with open(scene_path, 'w', encoding='utf-8') as f:
    f.write(text)

print("MainGame.unity updated successfully!")
