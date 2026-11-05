import os, sys, hashlib

sys.stdout.reconfigure(encoding='utf-8')

ROOT = r'c:\Users\Admin\Downloads\All-Project\GameVN-KR'
FRAMES_DIR = os.path.join(ROOT, 'Assets', 'Sprites', 'Characters', 'Frames')
ANIM_DIR = os.path.join(ROOT, 'Assets', 'Animations', 'Mascots')
ANIM_SHEET_DIR = os.path.join(ROOT, 'Assets', 'Sprites', 'Animations')

os.makedirs(FRAMES_DIR, exist_ok=True)
os.makedirs(ANIM_DIR, exist_ok=True)
os.makedirs(ANIM_SHEET_DIR, exist_ok=True)

def get_guid(rel_path):
    return hashlib.md5(rel_path.replace('\\', '/').lower().encode('utf-8')).hexdigest()

def write_meta(path, guid, is_sprite=True, is_sheet=False, sprites=None):
    meta_path = path + '.meta'
    if is_sheet and sprites:
        lines = [
            'fileFormatVersion: 2',
            f'guid: {guid}',
            'TextureImporter:',
            '  internalIDToNameTable: []',
            '  externalObjects: {}',
            '  serializedVersion: 13',
            '  mipmaps:',
            '    mipMapMode: 0',
            '    enableMipMap: 0',
            '    sRGBTexture: 1',
            '    linearTexture: 0',
            '    fadeOut: 0',
            '    borderMipMap: 0',
            '    mipMapsPreserveCoverage: 0',
            '    alphaTestReferenceValue: 0.5',
            '    mipMapFadeDistanceStart: 1',
            '    mipMapFadeDistanceEnd: 3',
            '  bumpmap:',
            '    convertToNormalMap: 0',
            '    externalNormalMap: 0',
            '    heightScale: 0.25',
            '    normalMapFilter: 0',
            '    flipGreenChannel: 0',
            '  isReadable: 0',
            '  streamingMipmaps: 0',
            '  streamingMipmapsPriority: 0',
            '  vTOnly: 0',
            '  ignoreMasterTextureLimit: 0',
            '  globalMipmapSizeLimit: 0',
            '  textureFormat: 1',
            '  maxTextureSize: 2048',
            '  textureSettings:',
            '    serializedVersion: 2',
            '    filterMode: 1',
            '    aniso: 1',
            '    mipBias: 0',
            '    wrapU: 1',
            '    wrapV: 1',
            '    wrapW: 1',
            '  nPOTScale: 0',
            '  lightmap: 0',
            '  compressionQuality: 100',
            '  spriteMode: 2',
            '  spriteExtrude: 1',
            '  spriteMeshType: 1',
            '  alignment: 0',
            '  spritePivot: {x: 0.5, y: 0.5}',
            '  spritePixelsToUnits: 100',
            '  spriteBorder: {x: 0, y: 0, z: 0, w: 0}',
            '  spriteGenerateFallbackPhysicsShape: 1',
            '  alphaUsage: 1',
            '  alphaIsTransparency: 1',
            '  spriteTessellationDetail: -1',
            '  textureType: 8',
            '  textureShape: 1',
            '  singleChannelComponent: 0',
            '  flipbook: 0',
            '  ignorePngGamma: 0',
            '  cookieLightType: 0',
            '  ignoreMipmapLimit: 0',
            '  platformSettings: []',
            '  spriteSheet:',
            '    serializedVersion: 2',
            '    sprites:'
        ]
        for idx, s in enumerate(sprites):
            s_guid = get_guid(f"{path}/{s['name']}")
            lines.extend([
                '    - serializedVersion: 2',
                f"      name: {s['name']}",
                '      rect:',
                '        serializedVersion: 2',
                f"        x: {s['rect'][0]}",
                f"        y: {s['rect'][1]}",
                f"        width: {s['rect'][2]}",
                f"        height: {s['rect'][3]}",
                '      alignment: 0',
                '      pivot: {x: 0.5, y: 0.5}',
                '      border: {x: 0, y: 0, z: 0, w: 0}',
                '      outline: []',
                '      physicsShape: []',
                '      tessellationDetail: 0',
                '      bones: []',
                f'      spriteID: {s_guid}',
                f"      internalID: {s['internal_id']}",
                '      vertices: []',
                '      indices: ',
                '      edges: []',
                '      weights: []'
            ])
        lines.extend([
            '  spritePackingTag: ',
            '  pSDRemoveMatte: 0',
            '  pSDShowGrid: 1',
            '  userData: ',
            '  assetBundleName: ',
            '  assetBundleVariant: '
        ])
        with open(meta_path, 'w', encoding='utf-8') as f:
            f.write('\n'.join(lines) + '\n')
    elif is_sprite:
        lines = [
            'fileFormatVersion: 2',
            f'guid: {guid}',
            'TextureImporter:',
            '  internalIDToNameTable: []',
            '  externalObjects: {}',
            '  serializedVersion: 13',
            '  mipmaps:',
            '    mipMapMode: 0',
            '    enableMipMap: 0',
            '    sRGBTexture: 1',
            '    linearTexture: 0',
            '    fadeOut: 0',
            '    borderMipMap: 0',
            '    mipMapsPreserveCoverage: 0',
            '    alphaTestReferenceValue: 0.5',
            '    mipMapFadeDistanceStart: 1',
            '    mipMapFadeDistanceEnd: 3',
            '  bumpmap:',
            '    convertToNormalMap: 0',
            '    externalNormalMap: 0',
            '    heightScale: 0.25',
            '    normalMapFilter: 0',
            '    flipGreenChannel: 0',
            '  isReadable: 0',
            '  streamingMipmaps: 0',
            '  streamingMipmapsPriority: 0',
            '  vTOnly: 0',
            '  ignoreMasterTextureLimit: 0',
            '  globalMipmapSizeLimit: 0',
            '  textureFormat: 1',
            '  maxTextureSize: 2048',
            '  textureSettings:',
            '    serializedVersion: 2',
            '    filterMode: 1',
            '    aniso: 1',
            '    mipBias: 0',
            '    wrapU: 1',
            '    wrapV: 1',
            '    wrapW: 1',
            '  nPOTScale: 0',
            '  lightmap: 0',
            '  compressionQuality: 100',
            '  spriteMode: 1',
            '  spriteExtrude: 1',
            '  spriteMeshType: 1',
            '  alignment: 0',
            '  spritePivot: {x: 0.5, y: 0.5}',
            '  spritePixelsToUnits: 100',
            '  spriteBorder: {x: 0, y: 0, z: 0, w: 0}',
            '  spriteGenerateFallbackPhysicsShape: 1',
            '  alphaUsage: 1',
            '  alphaIsTransparency: 1',
            '  spriteTessellationDetail: -1',
            '  textureType: 8',
            '  textureShape: 1',
            '  singleChannelComponent: 0',
            '  flipbook: 0',
            '  ignorePngGamma: 0',
            '  cookieLightType: 0',
            '  ignoreMipmapLimit: 0',
            '  platformSettings: []',
            '  spriteSheet:',
            '    serializedVersion: 2',
            '    sprites: []',
            '  spritePackingTag: ',
            '  pSDRemoveMatte: 0',
            '  pSDShowGrid: 1',
            '  userData: ',
            '  assetBundleName: ',
            '  assetBundleVariant: '
        ]
        with open(meta_path, 'w', encoding='utf-8') as f:
            f.write('\n'.join(lines) + '\n')
    else:
        lines = [
            'fileFormatVersion: 2',
            f'guid: {guid}',
            'DefaultImporter:',
            '  externalObjects: {}',
            '  userData: ',
            '  assetBundleName: ',
            '  assetBundleVariant: '
        ]
        with open(meta_path, 'w', encoding='utf-8') as f:
            f.write('\n'.join(lines) + '\n')

# 1. Write metadata for sheets
vn_slices = [
    { 'name': 'TrauVang_Idle_0', 'rect': (199, 432, 101, 116), 'internal_id': 21300002 },
    { 'name': 'TrauVang_Idle_1', 'rect': (375, 432, 125, 126), 'internal_id': 21300004 },
    { 'name': 'TrauVang_Action_0', 'rect': (552, 432, 158, 95), 'internal_id': 21300006 },
    { 'name': 'TrauVang_Action_1', 'rect': (754, 432, 158, 110), 'internal_id': 21300008 },
    { 'name': 'RongVang_Idle_0', 'rect': (178, 308, 154, 116), 'internal_id': 21300010 },
    { 'name': 'RongVang_Idle_1', 'rect': (353, 308, 117, 117), 'internal_id': 21300012 },
    { 'name': 'RongVang_Action_0', 'rect': (537, 307, 159, 118), 'internal_id': 21300014 },
    { 'name': 'RongVang_Action_1', 'rect': (726, 308, 124, 117), 'internal_id': 21300016 },
    { 'name': 'LanSu_Idle_0', 'rect': (199, 197, 99, 101), 'internal_id': 21300018 },
    { 'name': 'LanSu_Idle_1', 'rect': (373, 197, 107, 103), 'internal_id': 21300020 },
    { 'name': 'LanSu_Action_0', 'rect': (550, 197, 120, 110), 'internal_id': 21300022 },
    { 'name': 'LanSu_Action_1', 'rect': (748, 196, 142, 100), 'internal_id': 21300024 },
    { 'name': 'BeTre_Idle_0', 'rect': (195, 96, 101, 100), 'internal_id': 21300026 },
    { 'name': 'BeTre_Idle_1', 'rect': (370, 96, 101, 100), 'internal_id': 21300028 },
    { 'name': 'BeTre_Action_0', 'rect': (560, 94, 120, 97), 'internal_id': 21300030 },
    { 'name': 'BeTre_Action_1', 'rect': (750, 94, 140, 102), 'internal_id': 21300032 },
    { 'name': 'ChimLac_Idle_0', 'rect': (209, 2, 83, 89), 'internal_id': 21300034 },
    { 'name': 'ChimLac_Idle_1', 'rect': (377, 2, 91, 90), 'internal_id': 21300036 },
    { 'name': 'ChimLac_Action_0', 'rect': (548, 2, 142, 92), 'internal_id': 21300038 },
    { 'name': 'ChimLac_Action_1', 'rect': (750, 2, 140, 92), 'internal_id': 21300040 },
    { 'name': 'ChimLac_Action_2', 'rect': (890, 20, 102, 74), 'internal_id': 21300042 }
]

kr_slices = [
    { 'name': 'HoBaekho_Idle_0', 'rect': (212, 434, 113, 109), 'internal_id': 21300102 },
    { 'name': 'HoBaekho_Idle_1', 'rect': (410, 434, 120, 108), 'internal_id': 21300104 },
    { 'name': 'HoBaekho_Action_0', 'rect': (590, 434, 120, 117), 'internal_id': 21300106 },
    { 'name': 'HoBaekho_Action_1', 'rect': (862, 434, 111, 109), 'internal_id': 21300108 },
    { 'name': 'Haechi_Idle_0', 'rect': (213, 295, 113, 118), 'internal_id': 21300110 },
    { 'name': 'Haechi_Idle_1', 'rect': (397, 295, 122, 121), 'internal_id': 21300112 },
    { 'name': 'Haechi_Action_0', 'rect': (607, 285, 236, 143), 'internal_id': 21300114 },
    { 'name': 'Haechi_Action_1', 'rect': (872, 295, 112, 118), 'internal_id': 21300116 },
    { 'name': 'Dokkaebi_Idle_0', 'rect': (213, 116, 81, 150), 'internal_id': 21300118 },
    { 'name': 'Dokkaebi_Idle_1', 'rect': (304, 110, 96, 132), 'internal_id': 21300120 },
    { 'name': 'Dokkaebi_Action_0', 'rect': (402, 116, 87, 117), 'internal_id': 21300122 },
    { 'name': 'GauDangoon_Idle_0', 'rect': (550, 114, 120, 162), 'internal_id': 21300124 },
    { 'name': 'GauDangoon_Action_0', 'rect': (680, 114, 153, 159), 'internal_id': 21300126 },
    { 'name': 'GauDangoon_Action_1', 'rect': (870, 107, 120, 118), 'internal_id': 21300128 },
    { 'name': 'Samjoko_Idle_0', 'rect': (216, 6, 100, 101), 'internal_id': 21300130 },
    { 'name': 'Samjoko_Idle_1', 'rect': (377, 6, 104, 101), 'internal_id': 21300132 },
    { 'name': 'Samjoko_Action_0', 'rect': (546, 6, 107, 99), 'internal_id': 21300134 },
    { 'name': 'Samjoko_Action_1', 'rect': (690, 6, 110, 99), 'internal_id': 21300136 },
    { 'name': 'Samjoko_Action_2', 'rect': (880, 6, 101, 101), 'internal_id': 21300138 }
]

vn_sheet_path = os.path.join(ANIM_SHEET_DIR, 'animation-vietnam.png')
kr_sheet_path = os.path.join(ANIM_SHEET_DIR, 'animation-hanquoc.png')

write_meta(vn_sheet_path, get_guid('Assets/Sprites/Animations/animation-vietnam.png'), is_sheet=True, sprites=vn_slices)
write_meta(kr_sheet_path, get_guid('Assets/Sprites/Animations/animation-hanquoc.png'), is_sheet=True, sprites=kr_slices)

# 2. Write metadata for all 40 frame files
frame_guids = {}
for root, dirs, files in os.walk(FRAMES_DIR):
    for f in files:
        if f.endswith('.png'):
            fp = os.path.join(root, f)
            rel = os.path.relpath(fp, ROOT).replace('\\', '/')
            guid = get_guid(rel)
            write_meta(fp, guid, is_sprite=True)
            frame_name = os.path.splitext(f)[0]
            frame_guids[frame_name] = guid

print(f'Wrote .meta for {len(frame_guids)} frames')

# 3. Define the 10 mascots and their animation frames
mascots = [
    {
        'name': 'TrauVang',
        'idle': ['TrauVang_Idle_0', 'TrauVang_Idle_1'],
        'action': ['TrauVang_Action_0', 'TrauVang_Action_1']
    },
    {
        'name': 'RongVang',
        'idle': ['RongVang_Idle_0', 'RongVang_Idle_1'],
        'action': ['RongVang_Action_0', 'RongVang_Action_1']
    },
    {
        'name': 'LanSu',
        'idle': ['LanSu_Idle_0', 'LanSu_Idle_1'],
        'action': ['LanSu_Action_0', 'LanSu_Action_1']
    },
    {
        'name': 'BeTre',
        'idle': ['BeTre_Idle_0', 'BeTre_Idle_1'],
        'action': ['BeTre_Action_0', 'BeTre_Action_1']
    },
    {
        'name': 'ChimLac',
        'idle': ['ChimLac_Idle_0', 'ChimLac_Idle_1'],
        'action': ['ChimLac_Action_0', 'ChimLac_Action_1', 'ChimLac_Action_2']
    },
    {
        'name': 'HoBaekho',
        'idle': ['HoBaekho_Idle_0', 'HoBaekho_Idle_1'],
        'action': ['HoBaekho_Action_0', 'HoBaekho_Action_1']
    },
    {
        'name': 'Haechi',
        'idle': ['Haechi_Idle_0', 'Haechi_Idle_1'],
        'action': ['Haechi_Action_0', 'Haechi_Action_1']
    },
    {
        'name': 'Dokkaebi',
        'idle': ['Dokkaebi_Idle_0', 'Dokkaebi_Idle_1'],
        'action': ['Dokkaebi_Action_0']
    },
    {
        'name': 'GauDangoon',
        'idle': ['GauDangoon_Idle_0'],
        'action': ['GauDangoon_Action_0', 'GauDangoon_Action_1']
    },
    {
        'name': 'Samjoko',
        'idle': ['Samjoko_Idle_0', 'Samjoko_Idle_1'],
        'action': ['Samjoko_Action_0', 'Samjoko_Action_1', 'Samjoko_Action_2']
    }
]

from generate_mascot_animations_helpers import build_anim_clip, build_animator_controller

for m in mascots:
    m_name = m['name']
    idle_guids = [frame_guids[f] for f in m['idle'] if f in frame_guids]
    action_guids = [frame_guids[f] for f in m['action'] if f in frame_guids]
    
    # 1. Idle anim clip
    idle_clip_name = f"{m_name}_Idle"
    idle_clip_path = os.path.join(ANIM_DIR, f"{idle_clip_name}.anim")
    idle_clip_rel = f"Assets/Animations/Mascots/{idle_clip_name}.anim"
    idle_clip_guid = get_guid(idle_clip_rel)
    with open(idle_clip_path, 'w', encoding='utf-8') as f:
        f.write(build_anim_clip(idle_clip_name, idle_guids, sample_rate=8, loop=True))
    write_meta(idle_clip_path, idle_clip_guid, is_sprite=False)
    
    # 2. Action anim clip
    action_clip_name = f"{m_name}_Action"
    action_clip_path = os.path.join(ANIM_DIR, f"{action_clip_name}.anim")
    action_clip_rel = f"Assets/Animations/Mascots/{action_clip_name}.anim"
    action_clip_guid = get_guid(action_clip_rel)
    with open(action_clip_path, 'w', encoding='utf-8') as f:
        f.write(build_anim_clip(action_clip_name, action_guids, sample_rate=8, loop=False))
    write_meta(action_clip_path, action_clip_guid, is_sprite=False)
    
    # 3. Animator Controller
    ac_name = f"AC_{m_name}"
    ac_path = os.path.join(ANIM_DIR, f"{ac_name}.controller")
    ac_rel = f"Assets/Animations/Mascots/{ac_name}.controller"
    ac_guid = get_guid(ac_rel)
    with open(ac_path, 'w', encoding='utf-8') as f:
        f.write(build_animator_controller(ac_name, idle_clip_guid, action_clip_guid))
    write_meta(ac_path, ac_guid, is_sprite=False)
    
    print(f"Generated assets for {m_name}: Idle ({len(idle_guids)} frames), Action ({len(action_guids)} frames), Controller: {ac_name}")

print("\nAll 20 Animation Clips and 10 Animator Controllers successfully generated!")
