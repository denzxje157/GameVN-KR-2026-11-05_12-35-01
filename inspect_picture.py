import os, sys
from PIL import Image

sys.stdout.reconfigure(encoding='utf-8')

pic_dir = 'picture'
files = [f for f in os.listdir(pic_dir) if f.lower().endswith(('.png', '.jpg', '.jpeg'))]

print(f"Total images in {pic_dir}: {len(files)}")
for f in sorted(files):
    p = os.path.join(pic_dir, f)
    try:
        with Image.open(p) as img:
            print(f"{f}: {img.size}, mode={img.mode}, {os.path.getsize(p)} bytes")
    except Exception as e:
        print(f"{f}: Error {e}")
