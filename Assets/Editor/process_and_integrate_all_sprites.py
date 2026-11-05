import cv2, os, hashlib, sys
import numpy as np

def cv2_imread(path):
    with open(path, 'rb') as f:
        bytes_arr = bytearray(f.read())
        np_arr = np.asarray(bytes_arr, dtype=np.uint8)
        return cv2.imdecode(np_arr, cv2.IMREAD_COLOR)

def cv2_imwrite(path, img):
    is_success, im_buf_arr = cv2.imencode('.png', img)
    if is_success:
        im_buf_arr.tofile(path)

def slice_and_trim(img_rgb, roi, bg_tol=22):
    # roi: (x1, y1, x2, y2)
    crop = img_rgb[roi[1]:roi[3], roi[0]:roi[2]]
    h, w = crop.shape[:2]
    mask = np.zeros((h+2, w+2), np.uint8)
    diff = (bg_tol, bg_tol, bg_tol)
    for pt in [(0, 0), (w-1, 0), (0, h-1), (w-1, h-1), (w//2, 0), (w//2, h-1), (0, h//2), (w-1, h//2)]:
        if mask[pt[1]+1, pt[0]+1] == 0:
            cv2.floodFill(crop, mask, pt, (0, 255, 0), diff, diff, flags=8 | (255 << 8) | cv2.FLOODFILL_MASK_ONLY)
    bg_mask = mask[1:h+1, 1:w+1] == 255
    rgba = cv2.cvtColor(crop, cv2.COLOR_BGR2BGRA)
    rgba[bg_mask, 3] = 0
    fg = np.where(~bg_mask)
    if len(fg[0]) > 0:
        ymin, ymax = fg[0].min(), fg[0].max() + 1
        xmin, xmax = fg[1].min(), fg[1].max() + 1
        rgba = rgba[ymin:ymax, xmin:xmax]
    return rgba

def generate_meta(path):
    guid = hashlib.md5(path.replace('\\', '/').encode('utf-8')).hexdigest()
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
  swizzle: 0
  cookieLightType: 0
  platformSettings: []
'''
    with open(meta_path, 'w', encoding='utf-8') as f:
        f.write(meta_content)

def save_and_meta(path, img):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    cv2_imwrite(path, img)
    generate_meta(path)
    print(f'Saved: {path} ({img.shape[1]}x{img.shape[0]})')

def process_all():
    ui_dir = r'Assets\Sprites\UI'

    # 1. Title Buttons (Index 16 & 17)
    print('--- Processing Title Buttons ---')
    for tag, fname in [('vn', 'Vietnamese Festival Game Menu Buttons.png'), ('kr', 'Colorful Korean Game Menu Buttons.png')]:
        path = os.path.join(ui_dir, fname)
        img = cv2_imread(path)
        w, h = img.shape[1], img.shape[0]
        # 4 quadrants
        save_and_meta(f'Assets/Sprites/UI/Title/btn_title_local_{tag}.png', slice_and_trim(img, (0, 0, w//2, h//2)))
        save_and_meta(f'Assets/Sprites/UI/Title/btn_title_pvp_{tag}.png', slice_and_trim(img, (w//2, 0, w, h//2)))
        save_and_meta(f'Assets/Sprites/UI/Title/btn_title_codex_{tag}.png', slice_and_trim(img, (0, h//2, w//2, h)))
        save_and_meta(f'Assets/Sprites/UI/Title/btn_title_guide_{tag}.png', slice_and_trim(img, (w//2, h//2, w, h)))

    # 2. Action Buttons & Hanging Signs (Index 14 & 15)
    print('--- Processing MatchHub Action Buttons & Signs ---')
    for tag, fname in [('vn', 'Vietnamese Festive Game Menu Buttons.png'), ('kr', 'Korean Festive Wooden Game Buttons.png')]:
        path = os.path.join(ui_dir, fname)
        img = cv2_imread(path)
        save_and_meta(f'Assets/Sprites/UI/MatchHub/btn_vao_tran_fire_{tag}.png', slice_and_trim(img, (150, 20, 1380, 400)))
        save_and_meta(f'Assets/Sprites/UI/MatchHub/btn_choi_lai_{tag}.png', slice_and_trim(img, (10, 420, 510, 610)))
        save_and_meta(f'Assets/Sprites/UI/MatchHub/btn_quay_lai_{tag}.png', slice_and_trim(img, (520, 420, 1030, 610)))
        save_and_meta(f'Assets/Sprites/UI/MatchHub/btn_dong_{tag}.png', slice_and_trim(img, (1040, 420, 1530, 610)))
        save_and_meta(f'Assets/Sprites/UI/MatchHub/sign_huong_dan_{tag}.png', slice_and_trim(img, (40, 630, 770, 990)))
        save_and_meta(f'Assets/Sprites/UI/MatchHub/sign_cai_dat_{tag}.png', slice_and_trim(img, (760, 630, 1490, 990)))

    # 3. Victory & Defeat Banners (Index 6 & 7)
    print('--- Processing Victory/Defeat Sheets ---')
    for tag, fname in [('vn', 'Vietnamese Festival Game UI Set.png'), ('kr', 'Korean Fantasy Game Popup Sprite Sheet.png')]:
        path = os.path.join(ui_dir, fname)
        img = cv2_imread(path)
        w, h = img.shape[1], img.shape[0]
        save_and_meta(f'Assets/Sprites/UI/Popups/banner_victory_{tag}.png', slice_and_trim(img, (0, 0, w//2, 680)))
        save_and_meta(f'Assets/Sprites/UI/Popups/banner_defeat_{tag}.png', slice_and_trim(img, (w//2, 0, w, 680)))
        save_and_meta(f'Assets/Sprites/UI/Popups/btn_result_replay_{tag}.png', slice_and_trim(img, (0, 680, w//2, h)))
        save_and_meta(f'Assets/Sprites/UI/Popups/btn_result_close_{tag}.png', slice_and_trim(img, (w//2, 680, w, h)))

    # 4. Settings Frames (Index 12 & 13)
    print('--- Processing Settings Frames ---')
    img_set_vn = cv2_imread(os.path.join(ui_dir, 'Cài Đặt Giao Diện Game Gỗ.png'))
    save_and_meta('Assets/Sprites/UI/Popups/frame_settings_vn.png', slice_and_trim(img_set_vn, (0, 0, img_set_vn.shape[1], img_set_vn.shape[0])))
    img_set_kr = cv2_imread(os.path.join(ui_dir, 'Korean-Style Ornate Game Settings Frame.png'))
    save_and_meta('Assets/Sprites/UI/Popups/frame_settings_kr.png', slice_and_trim(img_set_kr, (0, 0, img_set_kr.shape[1], img_set_kr.shape[0])))

    # 5. Character Selection Panels & Cards (Index 10 & 9)
    print('--- Processing Char Select Panels & Cards ---')
    img_panels = cv2_imread(os.path.join(ui_dir, 'Vietnamese and Korean Spirit Selection Panels.png'))
    wp, hp = img_panels.shape[1], img_panels.shape[0]
    save_and_meta('Assets/Sprites/UI/MatchHub/panel_char_select_vn.png', slice_and_trim(img_panels, (0, 0, wp//2, hp)))
    save_and_meta('Assets/Sprites/UI/MatchHub/panel_char_select_kr.png', slice_and_trim(img_panels, (wp//2, 0, wp, hp)))

    img_cards = cv2_imread(os.path.join(ui_dir, 'Festive Multilingual Game UI Frames.png'))
    wc, hc = img_cards.shape[1], img_cards.shape[0]
    save_and_meta('Assets/Sprites/UI/MatchHub/card_mascot_vn_confirm.png', slice_and_trim(img_cards, (0, 0, wc//2, hc//2)))
    save_and_meta('Assets/Sprites/UI/MatchHub/card_mascot_kr_confirm.png', slice_and_trim(img_cards, (wc//2, 0, wc, hc//2)))
    save_and_meta('Assets/Sprites/UI/MatchHub/card_mascot_vn_select.png', slice_and_trim(img_cards, (0, hc//2, wc//2, hc)))
    save_and_meta('Assets/Sprites/UI/MatchHub/card_mascot_kr_select.png', slice_and_trim(img_cards, (wc//2, hc//2, wc, hc)))

    # 6. MatchHub Accessories Sheet (Index 8)
    print('--- Processing MatchHub Accessories ---')
    img_acc = cv2_imread(os.path.join(ui_dir, 'Colorful Match Game UI Asset Sheet.png'))
    save_and_meta('Assets/Sprites/UI/MatchHub/icon_vs_drum_fire.png', slice_and_trim(img_acc, (950, 40, 1530, 460)))
    save_and_meta('Assets/Sprites/UI/MatchHub/avatar_frame_gold.png', slice_and_trim(img_acc, (20, 500, 450, 980)))
    save_and_meta('Assets/Sprites/UI/MatchHub/arrow_red_left.png', slice_and_trim(img_acc, (900, 560, 1080, 940)))
    save_and_meta('Assets/Sprites/UI/MatchHub/arrow_yellow_right.png', slice_and_trim(img_acc, (1090, 560, 1280, 940)))
    save_and_meta('Assets/Sprites/UI/MatchHub/arrow_blue_right.png', slice_and_trim(img_acc, (1290, 560, 1530, 940)))

    # 7. Battle HUD & Codex / Tutorial (Index 4 & 5)
    print('--- Processing Battle HUD & Codex ---')
    for tag, fname in [('vn', 'Vietnamese Festival Game UI Assets.png'), ('kr', 'East Asian Festival Game UI Sticker Sheet.png')]:
        path = os.path.join(ui_dir, fname)
        img = cv2_imread(path)
        save_and_meta(f'Assets/Sprites/UI/Popups/panel_codex_{tag}.png', slice_and_trim(img, (20, 20, 970, 480)))
        save_and_meta(f'Assets/Sprites/UI/Battle/btn_skill_fist_{tag}.png', slice_and_trim(img, (980, 20, 1340, 480)))
        save_and_meta(f'Assets/Sprites/UI/Battle/btn_battle_pause_{tag}.png', slice_and_trim(img, (1340, 80, 1530, 450)))
        save_and_meta(f'Assets/Sprites/UI/Battle/bar_score_{tag}.png', slice_and_trim(img, (20, 490, 1530, 680)))
        save_and_meta(f'Assets/Sprites/UI/Battle/callout_ringout_{tag}.png', slice_and_trim(img, (20, 690, 1530, 980)))

    # 8. Cultural Items & Obstacles (Index 3)
    print('--- Processing Cultural Items ---')
    img_items = cv2_imread(os.path.join(ui_dir, 'Korean and Vietnamese Game Icon Collection.png'))
    save_and_meta('Assets/Sprites/Items/obstacle_chum_kimchi.png', slice_and_trim(img_items, (20, 40, 500, 500)))
    save_and_meta('Assets/Sprites/Items/obstacle_vai_gom.png', slice_and_trim(img_items, (510, 40, 980, 500)))
    save_and_meta('Assets/Sprites/Items/item_banh_chung.png', slice_and_trim(img_items, (990, 40, 1530, 500)))
    save_and_meta('Assets/Sprites/Items/item_dia_kimchi.png', slice_and_trim(img_items, (40, 500, 760, 980)))
    save_and_meta('Assets/Sprites/Items/obstacle_vung_bun.png', slice_and_trim(img_items, (770, 500, 1530, 980)))

    # 9. PvP Lobby (Index 1 & 2)
    print('--- Processing PvP Lobby ---')
    for tag, fname in [('vn', 'Vietnamese Festival PvP Game Lobby.png'), ('kr', 'Vibrant Asian Festival PvP Lobby UI.png')]:
        path = os.path.join(ui_dir, fname)
        img = cv2_imread(path)
        save_and_meta(f'Assets/Sprites/UI/Popups/frame_pvp_{tag}.png', slice_and_trim(img, (20, 20, 1020, 600)))
        save_and_meta(f'Assets/Sprites/UI/Popups/slots_pvp_pin_{tag}.png', slice_and_trim(img, (1020, 20, 1530, 600)))
        save_and_meta(f'Assets/Sprites/UI/Popups/btn_pvp_create_{tag}.png', slice_and_trim(img, (20, 610, 760, 980)))
        save_and_meta(f'Assets/Sprites/UI/Popups/btn_pvp_join_{tag}.png', slice_and_trim(img, (760, 610, 1530, 980)))

    # 10. Settings Components (Index 11)
    print('--- Processing Settings Components ---')
    img_comp = cv2_imread(os.path.join(ui_dir, 'Colorful Festival Game UI Sprite Sheet.png'))
    save_and_meta('Assets/Sprites/UI/Popups/btn_settings_close_x.png', slice_and_trim(img_comp, (20, 20, 300, 200)))
    save_and_meta('Assets/Sprites/UI/Popups/plaque_header_gold.png', slice_and_trim(img_comp, (320, 20, 1500, 200)))
    save_and_meta('Assets/Sprites/UI/Popups/slider_bar_wood.png', slice_and_trim(img_comp, (510, 210, 1020, 320)))
    save_and_meta('Assets/Sprites/UI/Popups/knob_drum_round.png', slice_and_trim(img_comp, (200, 330, 500, 480)))
    save_and_meta('Assets/Sprites/UI/Popups/knob_taegeuk_round.png', slice_and_trim(img_comp, (520, 330, 850, 480)))
    save_and_meta('Assets/Sprites/UI/Popups/icon_music_on.png', slice_and_trim(img_comp, (200, 490, 450, 660)))
    save_and_meta('Assets/Sprites/UI/Popups/icon_music_off.png', slice_and_trim(img_comp, (460, 490, 710, 660)))
    save_and_meta('Assets/Sprites/UI/Popups/icon_sfx_on.png', slice_and_trim(img_comp, (720, 490, 970, 660)))
    save_and_meta('Assets/Sprites/UI/Popups/icon_sfx_off.png', slice_and_trim(img_comp, (980, 490, 1250, 660)))

    # 11. Grand Festival Arena Background (Index 0)
    print('--- Processing Arena Background ---')
    img_arena = cv2_imread(os.path.join(ui_dir, 'Vibrant Vietnamese-Korean Festival Arena.png'))
    save_and_meta('Assets/Sprites/Arenas/arena_viet_han_grand_festival.png', img_arena)

    print('=== ALL 18 SHEETS PROCESSED SUCCESSFULLY! ===')

if __name__ == '__main__':
    process_all()
