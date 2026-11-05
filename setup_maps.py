#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
setup_maps.py
Copies 3 new maps from picture/ to Assets/Sprites/Maps/, generates thumbnails in Assets/Sprites/UI/Thumbnails/,
and generates valid Unity .meta files for Sprite (2D and UI), Single sprite mode.
"""

import os
import shutil
import cv2

meta_sprite_template = """fileFormatVersion: 2
guid: {guid}
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

maps = [
    ('san1-lang.png', '7a1b2c3d4e5f60718293a4b5c6d7e801', '8a1b2c3d4e5f60718293a4b5c6d7e802'),
    ('san-yentu.png', '7b1b2c3d4e5f60718293a4b5c6d7e803', '8b1b2c3d4e5f60718293a4b5c6d7e804'),
    ('dan-dempho.png', '7c1b2c3d4e5f60718293a4b5c6d7e805', '8c1b2c3d4e5f60718293a4b5c6d7e806')
]

os.makedirs('Assets/Sprites/Maps', exist_ok=True)
os.makedirs('Assets/Sprites/UI/Thumbnails', exist_ok=True)

for fname, map_guid, thumb_guid in maps:
    src_path = os.path.join('picture', fname)
    dst_map_path = os.path.join('Assets/Sprites/Maps', fname)
    dst_thumb_path = os.path.join('Assets/Sprites/UI/Thumbnails', 'thumb_' + fname)

    # 1. Copy map
    shutil.copy2(src_path, dst_map_path)
    with open(dst_map_path + '.meta', 'w', encoding='utf-8') as mf:
        mf.write(meta_sprite_template.replace('{guid}', map_guid))
    print(f"Installed Map: {dst_map_path} (guid: {map_guid})")

    # 2. Generate clean thumbnail 480x200
    im = cv2.imread(src_path)
    # Resize to 480x268 then crop middle 200px or direct resize
    thumb = cv2.resize(im, (480, 200), interpolation=cv2.INTER_AREA)
    cv2.imwrite(dst_thumb_path, thumb)
    with open(dst_thumb_path + '.meta', 'w', encoding='utf-8') as tf:
        tf.write(meta_sprite_template.replace('{guid}', thumb_guid))
    print(f"Generated Thumbnail: {dst_thumb_path} (guid: {thumb_guid})")

print("All 3 new maps & thumbnails configured successfully!")
