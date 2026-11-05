import os
import shutil

meta_template = """fileFormatVersion: 2
guid: GUID_PLACEHOLDER
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {}
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
  ignoreMipmapLimit: 0
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
    wrapU: 0
    wrapV: 0
    wrapW: 0
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {x: 0.5, y: 0.5}
  spritePixelsToUnits: 100
  spriteBorder: {x: 0, y: 0, z: 0, w: 0}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationMethod: 0
  spriteTessellationDetail: -1
  atlasTags: []
  spritePackingTag: ''
  userData: ''
  assetBundleName: ''
  assetBundleVariant: ''
"""

files_to_copy = [
    ('panel_settings_large_clean.png', 'Assets/Sprites/UI/Popups/panel_settings_large.png', 'e9a1b2c3d4e5f60718293a4b5c6d7e8f'),
    ('btn_settings_dong_clean.png', 'Assets/Sprites/UI/Popups/btn_settings_dong.png', 'f1a2b3c4d5e60718293a4b5c6d7e8f90'),
    ('slider_bar_clean.png', 'Assets/Sprites/UI/Popups/slider_bar_wood.png', 'a1b2c3d4e5f60718293a4b5c6d7e8f01'),
    ('btn_music_on_clean.png', 'Assets/Sprites/UI/Popups/btn_music_on.png', 'b2c3d4e5f60718293a4b5c6d7e8f01a2'),
    ('btn_music_off_clean.png', 'Assets/Sprites/UI/Popups/btn_music_off.png', 'c3d4e5f60718293a4b5c6d7e8f01a2b3'),
    ('btn_sfx_on_clean.png', 'Assets/Sprites/UI/Popups/btn_sfx_on.png', 'd4e5f60718293a4b5c6d7e8f01a2b3c4'),
    ('btn_sfx_off_clean.png', 'Assets/Sprites/UI/Popups/btn_sfx_off.png', 'e5f60718293a4b5c6d7e8f01a2b3c4d5'),
]

os.makedirs('Assets/Sprites/UI/Popups', exist_ok=True)
for src, dst, guid in files_to_copy:
    if os.path.exists(src):
        shutil.copy2(src, dst)
        meta_file = dst + '.meta'
        with open(meta_file, 'w', encoding='utf-8') as mf:
            mf.write(meta_template.replace('GUID_PLACEHOLDER', guid))
        print(f'Installed: {dst} (guid: {guid})')
    else:
        print(f'Warning: {src} not found!')
