import os
import hashlib
import sys

sys.stdout.reconfigure(encoding='utf-8')

SPRITE_META_TEMPLATE = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
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
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbook: 0
  ignorePngGamma: 0
  cookieLightType: 0
  platformSettings: []
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

def generate_guid(rel_path):
    h = hashlib.md5(rel_path.lower().encode('utf-8')).hexdigest()
    return h

def main():
    base_dir = "Assets/Sprites"
    created = 0
    for root, dirs, files in os.walk(base_dir):
        # Create folder metas if needed
        for d in dirs:
            dir_path = os.path.join(root, d)
            dir_meta = dir_path + ".meta"
            if not os.path.exists(dir_meta):
                guid = generate_guid(dir_path)
                with open(dir_meta, 'w', encoding='utf-8') as f:
                    f.write(f"fileFormatVersion: 2\nguid: {guid}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n")
        
        for f in files:
            if f.endswith('.png'):
                png_path = os.path.join(root, f)
                meta_path = png_path + '.meta'
                if not os.path.exists(meta_path):
                    guid = generate_guid(png_path)
                    content = SPRITE_META_TEMPLATE.format(guid=guid)
                    with open(meta_path, 'w', encoding='utf-8') as mf:
                        mf.write(content)
                    created += 1
                    print(f"Created sprite meta: {meta_path}")

    print(f"Done! Created {created} new sprite .meta files.")

if __name__ == '__main__':
    main()
