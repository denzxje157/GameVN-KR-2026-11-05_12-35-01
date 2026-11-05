import sys, re

sys.stdout.reconfigure(encoding='utf-8')

scene_path = 'Assets/Scenes/MainGame.unity'

with open(scene_path, 'r', encoding='utf-8') as f:
    text = f.read()

# Generate new unique fileIDs
# Let's find max fileID currently used
all_fids = [int(x) for x in re.findall(r'&(\d+)', text)]
max_fid = max(all_fids) if all_fids else 2140000000
next_fid = max_fid + 100

def get_next_fid():
    global next_fid
    next_fid += 1
    return next_fid

# 1. Btn_Close_X under Settings_Popup (father: 2057269603)
# 2. Btn_Music_Toggle under Settings_Popup (father: 2057269603)
# 3. Btn_SFX_Toggle under Settings_Popup (father: 2057269603)
# 4. Frame_MapOval under Panel_MapSelect (father: 913064356)

def create_ui_image_node(go_fid, rt_fid, cr_fid, img_fid, name, father_fid, pos, size, guid):
    go_str = f"""--- !u!1 &{go_fid}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {rt_fid}}}
  - component: {{fileID: {cr_fid}}}
  - component: {{fileID: {img_fid}}}
  m_Layer: 5
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
"""
    rt_str = f"""--- !u!224 &{rt_fid}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_fid}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {father_fid}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 0.5}}
  m_AnchorMax: {{x: 0.5, y: 0.5}}
  m_AnchoredPosition: {{x: {pos[0]}, y: {pos[1]}}}
  m_SizeDelta: {{x: {size[0]}, y: {size[1]}}}
  m_Pivot: {{x: 0.5, y: 0.5}}
"""
    cr_str = f"""--- !u!222 &{cr_fid}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_fid}}}
  m_CullTransparentMesh: 1
"""
    img_str = f"""--- !u!114 &{img_fid}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_fid}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Image
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: 21300000, guid: {guid}, type: 3}}
  m_Type: 0
  m_PreserveAspect: 1
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
"""
    return f"{go_str}{rt_str}{cr_str}{img_str}"

def create_ui_button_node(go_fid, rt_fid, cr_fid, img_fid, btn_fid, name, father_fid, pos, size, guid):
    base = create_ui_image_node(go_fid, rt_fid, cr_fid, img_fid, name, father_fid, pos, size, guid)
    # inject button component into go_str
    base = base.replace(f"  - component: {{fileID: {img_fid}}}\n", f"  - component: {{fileID: {img_fid}}}\n  - component: {{fileID: {btn_fid}}}\n")
    btn_str = f"""--- !u!114 &{btn_fid}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_fid}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 4e29b1a8efbd4b44bb3f3716e73f07ff, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Button
  m_Navigation:
    m_Mode: 3
    m_WrapAround: 0
    m_SelectOnUp: {{fileID: 0}}
    m_SelectOnDown: {{fileID: 0}}
    m_SelectOnLeft: {{fileID: 0}}
    m_SelectOnRight: {{fileID: 0}}
  m_Transition: 1
  m_Colors:
    m_NormalColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_HighlightedColor: {{r: 0.96, g: 0.96, b: 0.96, a: 1}}
    m_PressedColor: {{r: 0.78, g: 0.78, b: 0.78, a: 1}}
    m_SelectedColor: {{r: 0.96, g: 0.96, b: 0.96, a: 1}}
    m_DisabledColor: {{r: 0.78, g: 0.78, b: 0.78, a: 0.5}}
    m_ColorMultiplier: 1
    m_FadeDuration: 0.1
  m_SpriteState:
    m_HighlightedSprite: {{fileID: 0}}
    m_PressedSprite: {{fileID: 0}}
    m_SelectedSprite: {{fileID: 0}}
    m_DisabledSprite: {{fileID: 0}}
  m_AnimationTriggers:
    m_NormalTrigger: Normal
    m_HighlightedTrigger: Highlighted
    m_PressedTrigger: Pressed
    m_SelectedTrigger: Selected
    m_DisabledTrigger: Disabled
  m_Interactable: 1
  m_TargetGraphic: {{fileID: {img_fid}}}
  m_OnClick:
    m_PersistentCalls:
      m_Calls: []
"""
    return base + btn_str

new_blocks = []

# 1. Btn_Close_X
btn_close_x_rt = get_next_fid()
new_blocks.append(create_ui_button_node(
    get_next_fid(), btn_close_x_rt, get_next_fid(), get_next_fid(), get_next_fid(),
    "Btn_Close_X", 2057269603, (430, 245), (46, 46), "f7a8b90123456789abcdef0123456789"
))

# 2. Btn_Music_Toggle
btn_music_rt = get_next_fid()
new_blocks.append(create_ui_button_node(
    get_next_fid(), btn_music_rt, get_next_fid(), get_next_fid(), get_next_fid(),
    "Btn_Music_Toggle", 2057269603, (-50, 110), (48, 48), "b2c3d4e5f60718293a4b5c6d7e8f01a2"
))

# 3. Btn_SFX_Toggle
btn_sfx_rt = get_next_fid()
new_blocks.append(create_ui_button_node(
    get_next_fid(), btn_sfx_rt, get_next_fid(), get_next_fid(), get_next_fid(),
    "Btn_SFX_Toggle", 2057269603, (-50, 20), (48, 48), "d4e5f60718293a4b5c6d7e8f01a2b3c4"
))

# 4. Frame_MapOval
frame_oval_rt = get_next_fid()
new_blocks.append(create_ui_image_node(
    get_next_fid(), frame_oval_rt, get_next_fid(), get_next_fid(),
    "Frame_MapOval", 913064356, (0, 80), (430, 210), "adbdda5d12ae3b3cbf435696131e2dcb"
))

# Now add children fileIDs into father RectTransforms
# Father of Settings_Popup children is 2057269603
pattern_settings_father = r'(RectTransform:\s*m_ObjectHideFlags: 0[\s\S]*?&2057269603[\s\S]*?m_Children:)([\s\S]*?)(m_Father:)'

def add_children_to_settings(match):
    prefix = match.group(1)
    children = match.group(2).strip()
    suffix = match.group(3)
    new_children = f"\n  - {{fileID: {btn_close_x_rt}}}\n  - {{fileID: {btn_music_rt}}}\n  - {{fileID: {btn_sfx_rt}}}"
    return f"{prefix}\n{children}{new_children}\n  {suffix}"

text = re.sub(r'(&2057269603\nRectTransform:[\s\S]*?m_Children:)([\s\S]*?)(m_Father:)', 
              rf'\g<1>\g<2>  - {{fileID: {btn_close_x_rt}}}\n  - {{fileID: {btn_music_rt}}}\n  - {{fileID: {btn_sfx_rt}}}\n  \g<3>', 
              text)

# Father of Panel_MapSelect children is 913064356
text = re.sub(r'(&913064356\nRectTransform:[\s\S]*?m_Children:)([\s\S]*?)(m_Father:)', 
              rf'\g<1>\g<2>  - {{fileID: {frame_oval_rt}}}\n  \g<3>', 
              text)

# Append new blocks
text = text.rstrip() + '\n' + ''.join(new_blocks)

with open(scene_path, 'w', encoding='utf-8') as f:
    f.write(text)

print("Successfully injected all remaining UI nodes (Btn_Close_X, Btn_Music_Toggle, Btn_SFX_Toggle, Frame_MapOval) into MainGame.unity!")
