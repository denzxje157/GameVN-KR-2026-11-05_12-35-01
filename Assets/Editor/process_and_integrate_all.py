import cv2, os, hashlib, sys, re
import numpy as np

sys.stdout.reconfigure(encoding='utf-8')

PROJECT_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
SPRITES_DIR = os.path.join(PROJECT_ROOT, 'Assets', 'Sprites', 'UI')
RES_DIR = os.path.join(PROJECT_ROOT, 'Assets', 'Resources', 'UI')
SCENE_PATH = os.path.join(PROJECT_ROOT, 'Assets', 'Scenes', 'MainGame.unity')

def cv2_imread(path):
    with open(path, 'rb') as f:
        bytes_arr = bytearray(f.read())
        return cv2.imdecode(np.asarray(bytes_arr, dtype=np.uint8), cv2.IMREAD_COLOR)

def cv2_imwrite(path, img):
    is_success, im_buf_arr = cv2.imencode('.png', img)
    if is_success:
        im_buf_arr.tofile(path)

def clean_cutout(crop, white_thresh=245):
    h, w = crop.shape[:2]
    is_white = (crop[:,:,0] >= white_thresh) & (crop[:,:,1] >= white_thresh) & (crop[:,:,2] >= white_thresh)
    white_mask = np.uint8(is_white * 255)
    for x in range(w):
        for y in [0, h-1]:
            if white_mask[y, x] == 255:
                cv2.floodFill(white_mask, None, (x, y), 128)
    for y in range(h):
        for x in [0, w-1]:
            if white_mask[y, x] == 255:
                cv2.floodFill(white_mask, None, (x, y), 128)
    bg_mask = (white_mask == 128)
    rgba = cv2.cvtColor(crop, cv2.COLOR_BGR2BGRA)
    rgba[bg_mask, 3] = 0
    fg = np.where(~bg_mask)
    if len(fg[0]) > 0:
        ymin, ymax = fg[0].min(), fg[0].max() + 1
        xmin, xmax = fg[1].min(), fg[1].max() + 1
        rgba = rgba[ymin:ymax, xmin:xmax]
    return rgba

def generate_meta(path, guid=None):
    if guid is None:
        rel = os.path.relpath(path, PROJECT_ROOT).replace('\\', '/')
        guid = hashlib.md5(rel.encode('utf-8')).hexdigest()
    meta_path = path + '.meta'
    meta_content = f'''fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: []
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMasterTextureLimit: 0
  doesUnassociatedAlphaWrapAround: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponentMapping: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings: []
'''
    with open(meta_path, 'w', encoding='utf-8') as f:
        f.write(meta_content)
    return guid

guid_registry = {}

def export_sprite(rel_subpath, img, custom_guid=None):
    # Save to Assets/Sprites/rel_subpath
    spr_path = os.path.join(PROJECT_ROOT, 'Assets', 'Sprites', rel_subpath)
    os.makedirs(os.path.dirname(spr_path), exist_ok=True)
    cv2_imwrite(spr_path, img)
    g = generate_meta(spr_path, custom_guid)
    guid_registry[rel_subpath] = g
    
    # Also save to Assets/Resources/rel_subpath
    res_path = os.path.join(PROJECT_ROOT, 'Assets', 'Resources', rel_subpath)
    os.makedirs(os.path.dirname(res_path), exist_ok=True)
    cv2_imwrite(res_path, img)
    generate_meta(res_path, hashlib.md5(('res_' + rel_subpath).encode('utf-8')).hexdigest())
    print(f'Exported: {rel_subpath} ({img.shape[1]}x{img.shape[0]}) -> GUID: {g}')
    return g

print('Slicing all sheets with clean transparency...')

# 1. Title Buttons VN
sheet_title_vn = cv2_imread(os.path.join(SPRITES_DIR, 'Vietnamese Festival Game Menu Buttons.png'))
export_sprite('UI/Title/btn_title_local_vn.png', clean_cutout(sheet_title_vn[137:468, 19:753]))
export_sprite('UI/Title/btn_title_pvp_vn.png', clean_cutout(sheet_title_vn[163:468, 784:1516]))
export_sprite('UI/Title/btn_title_codex_vn.png', clean_cutout(sheet_title_vn[558:861, 18:754]))
export_sprite('UI/Title/btn_title_guide_vn.png', clean_cutout(sheet_title_vn[558:863, 784:1516]))

# 2. Title Buttons KR
sheet_title_kr = cv2_imread(os.path.join(SPRITES_DIR, 'Colorful Korean Game Menu Buttons.png'))
export_sprite('UI/Title/btn_title_local_kr.png', clean_cutout(sheet_title_kr[136:468, 18:754]))
export_sprite('UI/Title/btn_title_pvp_kr.png', clean_cutout(sheet_title_kr[162:468, 784:1516]))
export_sprite('UI/Title/btn_title_codex_kr.png', clean_cutout(sheet_title_kr[558:861, 19:754]))
export_sprite('UI/Title/btn_title_guide_kr.png', clean_cutout(sheet_title_kr[558:863, 784:1516]))

# 3. Action Buttons VN
sheet_act_vn = cv2_imread(os.path.join(SPRITES_DIR, 'Vietnamese Festive Game Menu Buttons.png'))
export_sprite('UI/MatchHub/btn_vao_tran_fire_vn.png', clean_cutout(sheet_act_vn[17:366, 143:1392]))
export_sprite('UI/MatchHub/btn_replay_vn.png', clean_cutout(sheet_act_vn[383:618, 10:517]))
export_sprite('UI/MatchHub/btn_quay_lai_vn.png', clean_cutout(sheet_act_vn[383:618, 544:1051]))
export_sprite('UI/MatchHub/btn_dong_vn.png', clean_cutout(sheet_act_vn[395:612, 1082:1526]))
export_sprite('UI/MatchHub/sign_huong_dan_vn.png', clean_cutout(sheet_act_vn[633:972, 27:751]))
export_sprite('UI/MatchHub/sign_cai_dat_vn.png', clean_cutout(sheet_act_vn[633:972, 788:1511]))

# 4. Action Buttons KR
sheet_act_kr = cv2_imread(os.path.join(SPRITES_DIR, 'Korean Festive Wooden Game Buttons.png'))
export_sprite('UI/MatchHub/btn_vao_tran_fire_kr.png', clean_cutout(sheet_act_kr[22:362, 215:1321]))
export_sprite('UI/MatchHub/btn_replay_kr.png', clean_cutout(sheet_act_kr[391:616, 12:519]))
export_sprite('UI/MatchHub/btn_quay_lai_kr.png', clean_cutout(sheet_act_kr[391:614, 544:1051]))
export_sprite('UI/MatchHub/btn_dong_kr.png', clean_cutout(sheet_act_kr[401:604, 1093:1514]))
export_sprite('UI/MatchHub/sign_huong_dan_kr.png', clean_cutout(sheet_act_kr[639:959, 48:742]))
export_sprite('UI/MatchHub/sign_cai_dat_kr.png', clean_cutout(sheet_act_kr[639:961, 796:1490]))

# 5. Victory & Defeat Popups VN
sheet_vic_def_vn = cv2_imread(os.path.join(SPRITES_DIR, 'Vietnamese Festival Game UI Set.png'))
export_sprite('UI/Popups/popup_victory_vn.png', clean_cutout(sheet_vic_def_vn[18:692, 15:755]))
export_sprite('UI/Popups/popup_defeat_vn.png', clean_cutout(sheet_vic_def_vn[18:692, 770:1523]))
export_sprite('UI/Popups/btn_victory_replay_vn.png', clean_cutout(sheet_vic_def_vn[715:979, 15:755]))
export_sprite('UI/Popups/btn_victory_close_vn.png', clean_cutout(sheet_vic_def_vn[715:979, 780:1520]))

# 6. Victory & Defeat Popups KR
sheet_vic_def_kr = cv2_imread(os.path.join(SPRITES_DIR, 'Korean Fantasy Game Popup Sprite Sheet.png'))
export_sprite('UI/Popups/popup_victory_kr.png', clean_cutout(sheet_vic_def_kr[18:692, 9:750]))
export_sprite('UI/Popups/popup_defeat_kr.png', clean_cutout(sheet_vic_def_kr[18:692, 770:1527]))
export_sprite('UI/Popups/btn_victory_replay_kr.png', clean_cutout(sheet_vic_def_kr[693:959, 21:775]))
export_sprite('UI/Popups/btn_victory_close_kr.png', clean_cutout(sheet_vic_def_kr[695:947, 808:1505]))

# 7. Settings Frames VN & KR
sheet_settings_vn = cv2_imread(os.path.join(SPRITES_DIR, 'Cài Đặt Giao Diện Game Gỗ.png'))
export_sprite('UI/Popups/frame_settings_vn.png', clean_cutout(sheet_settings_vn[12:944, 0:1586]))

sheet_settings_kr = cv2_imread(os.path.join(SPRITES_DIR, 'Korean-Style Ornate Game Settings Frame.png'))
export_sprite('UI/Popups/frame_settings_kr.png', clean_cutout(sheet_settings_kr[12:944, 0:1585]))

# 8. Settings Controls (Sliders, Knobs, Icons)
sheet_controls = cv2_imread(os.path.join(SPRITES_DIR, 'Colorful Festival Game UI Sprite Sheet.png'))
export_sprite('UI/Popups/btn_settings_close_x.png', clean_cutout(sheet_controls[16:234, 169:393]))
export_sprite('UI/Popups/slider_bar_wood.png', clean_cutout(sheet_controls[251:368, 28:500]))
export_sprite('UI/Popups/knob_drum_round.png', clean_cutout(sheet_controls[381:568, 65:325]))
export_sprite('UI/Popups/knob_taegeuk_round.png', clean_cutout(sheet_controls[381:568, 435:665]))
export_sprite('UI/Popups/knob_janggu_round.png', clean_cutout(sheet_controls[391:558, 977:1228]))
export_sprite('UI/Popups/icon_music_on.png', clean_cutout(sheet_controls[565:771, 175:387]))
export_sprite('UI/Popups/icon_music_off.png', clean_cutout(sheet_controls[561:771, 496:712]))
export_sprite('UI/Popups/icon_sfx_on.png', clean_cutout(sheet_controls[561:771, 821:1042]))
export_sprite('UI/Popups/icon_sfx_off.png', clean_cutout(sheet_controls[561:776, 1145:1369]))

# 9. PvP Lobby Frames & Buttons
sheet_pvp_vn = cv2_imread(os.path.join(SPRITES_DIR, 'Vietnamese Festival PvP Game Lobby.png'))
export_sprite('UI/Popups/frame_pvp_vn.png', clean_cutout(sheet_pvp_vn[0:650, 0:979]))
export_sprite('UI/Popups/badge_room_code_vn.png', clean_cutout(sheet_pvp_vn[155:310, 980:1536]))
export_sprite('UI/Popups/btn_pvp_create_vn.png', clean_cutout(sheet_pvp_vn[675:996, 15:755]))
export_sprite('UI/Popups/btn_pvp_join_vn.png', clean_cutout(sheet_pvp_vn[675:996, 760:1523]))

sheet_pvp_kr = cv2_imread(os.path.join(SPRITES_DIR, 'Vibrant Asian Festival PvP Lobby UI.png'))
export_sprite('UI/Popups/frame_pvp_kr.png', clean_cutout(sheet_pvp_kr[15:664, 12:980]))
export_sprite('UI/Popups/btn_pvp_create_kr.png', clean_cutout(sheet_pvp_kr[705:967, 12:755]))
export_sprite('UI/Popups/btn_pvp_join_kr.png', clean_cutout(sheet_pvp_kr[705:967, 760:1523]))

# 10. Tutorial & Codex Parchment Frames
sheet_tut_vn = cv2_imread(os.path.join(SPRITES_DIR, 'Vietnamese Festival Game UI Assets.png'))
export_sprite('UI/Popups/panel_codex_vn.png', clean_cutout(sheet_tut_vn[10:505, 9:780]))
export_sprite('UI/Battle/btn_skill_fist_vn.png', clean_cutout(sheet_tut_vn[10:505, 785:1200]))
export_sprite('UI/Battle/btn_battle_pause_vn.png', clean_cutout(sheet_tut_vn[10:505, 1200:1527]))
export_sprite('UI/Battle/banner_ringout_vn.png', clean_cutout(sheet_tut_vn[711:1002, 41:1502]))

sheet_tut_kr = cv2_imread(os.path.join(SPRITES_DIR, 'East Asian Festival Game UI Sticker Sheet.png'))
export_sprite('UI/Popups/panel_codex_kr.png', clean_cutout(sheet_tut_kr[8:505, 13:780]))
export_sprite('UI/Battle/btn_skill_fist_kr.png', clean_cutout(sheet_tut_kr[8:505, 785:1200]))
export_sprite('UI/Battle/btn_battle_pause_kr.png', clean_cutout(sheet_tut_kr[8:505, 1200:1527]))
export_sprite('UI/Battle/banner_ringout_kr.png', clean_cutout(sheet_tut_kr[720:1000, 84:1448]))

# 11. Spirit Selection Panels
sheet_char = cv2_imread(os.path.join(SPRITES_DIR, 'Vietnamese and Korean Spirit Selection Panels.png'))
export_sprite('UI/MatchHub/panel_char_select_vn.png', clean_cutout(sheet_char[78:853, 13:790]))
export_sprite('UI/MatchHub/panel_char_select_kr.png', clean_cutout(sheet_char[78:853, 790:1575]))

# 12. Multilingual Cards
sheet_cards = cv2_imread(os.path.join(SPRITES_DIR, 'Festive Multilingual Game UI Frames.png'))
export_sprite('UI/MatchHub/card_confirm_red_vn.png', clean_cutout(sheet_cards[0:607, 42:577]))
export_sprite('UI/MatchHub/card_confirm_red_kr.png', clean_cutout(sheet_cards[8:607, 668:1206]))
export_sprite('UI/MatchHub/card_select_blue_vn.png', clean_cutout(sheet_cards[643:1242, 42:577]))
export_sprite('UI/MatchHub/card_select_blue_kr.png', clean_cutout(sheet_cards[642:1242, 670:1206]))

# 13. HUD Assets
sheet_match = cv2_imread(os.path.join(SPRITES_DIR, 'Colorful Match Game UI Asset Sheet.png'))
export_sprite('UI/Battle/badge_vs_fire.png', clean_cutout(sheet_match[27:544, 948:1491]))
export_sprite('UI/Battle/avatar_frame_gold_round.png', clean_cutout(sheet_match[564:953, 36:400]))

# 14. Cultural Icons
sheet_icons = cv2_imread(os.path.join(SPRITES_DIR, 'Korean and Vietnamese Game Icon Collection.png'))
export_sprite('UI/Icons/icon_kimchi_pot.png', clean_cutout(sheet_icons[30:520, 36:484]))
export_sprite('UI/Icons/icon_lotus_pot.png', clean_cutout(sheet_icons[30:526, 523:958]))
export_sprite('UI/Icons/icon_banh_chung.png', clean_cutout(sheet_icons[18:545, 955:1518]))
export_sprite('UI/Icons/icon_kimchi_dish.png', clean_cutout(sheet_icons[547:985, 78:736]))
export_sprite('UI/Icons/obstacle_mud_puddle.png', clean_cutout(sheet_icons[561:983, 803:1514]))

# 15. Arena Background
sheet_arena = cv2_imread(os.path.join(SPRITES_DIR, 'Vibrant Vietnamese-Korean Festival Arena.png'))
export_sprite('Arena/arena_festival_vn_kr.png', sheet_arena)

print('=== ALL 15 CATEGORIES SLICED AND EXPORTED SUCCESSFULLY! ===')
