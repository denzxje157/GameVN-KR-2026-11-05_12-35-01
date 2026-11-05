import os, sys
from PIL import Image

sys.stdout.reconfigure(encoding='utf-8')

for name in ["pic.11-dau-doi-khang.png", "pic.10-background-cac-access.png", "pic.12-background-chinh.png"]:
    img = Image.open(os.path.join("picture", name))
    print(f"=== {name} ===")
    print(f"Size: {img.size}, Mode: {img.mode}")
