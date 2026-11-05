import cv2, os, hashlib, sys
import numpy as np
from PIL import Image

def cv2_imread(path):
    with open(path, 'rb') as f:
        bytes_arr = bytearray(f.read())
        np_arr = np.asarray(bytes_arr, dtype=np.uint8)
        return cv2.imdecode(np_arr, cv2.IMREAD_COLOR)

def cv2_imwrite(path, img):
    is_success, im_buf_arr = cv2.imencode('.png', img)
    if is_success:
        im_buf_arr.tofile(path)

def make_transparent_and_trim(crop, bg_tol=22):
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

def generate_meta(path, guid=None):
    if guid is None:
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
  swizzle: 50462976
  cookieLightType: 0
  platformSettings: []
'''
    with open(meta_path, 'w', encoding='utf-8') as f:
        f.write(meta_content)
    return guid

def save_sprite(path, img, guid=None):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    cv2_imwrite(path, img)
    g = generate_meta(path, guid)
    # Also copy to Resources/UI so it is loadable anywhere
    res_path = path.replace('Assets/Sprites/UI/', 'Assets/Resources/UI/')
    if res_path != path:
        os.makedirs(os.path.dirname(res_path), exist_ok=True)
        cv2_imwrite(res_path, img)
        generate_meta(res_path, hashlib.md5(res_path.replace('\\', '/').encode('utf-8')).hexdigest())
    print(f'Saved: {path} ({img.shape[1]}x{img.shape[0]}) -> GUID: {g}')
    return g

print('Master reprocessor module loaded.')
