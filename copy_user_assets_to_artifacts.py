import os, sys
from PIL import Image

sys.stdout.reconfigure(encoding='utf-8')
artifact_dir = r"C:\Users\Admin\.gemini\antigravity\brain\777661cc-5705-48ae-8e41-a23175dba7d6"

files_to_check = [
    "UI_Settings_Sheet_Complete.png",
    "Settings_Panel_Large.png",
    "UI_Accessories_Sheet.png",
    "cac-access của màn hình bắt đầu khi vào game.png",
    "pic.10-background-cac-access.png",
    "pic.11-dau-doi-khang.png",
    "pic.12-background-chinh.png"
]

for fn in files_to_check:
    src = os.path.join("picture", fn)
    if os.path.exists(src):
        dst = os.path.join(artifact_dir, fn.replace(" ", "_"))
        img = Image.open(src)
        img.save(dst)
        print(f"Saved {fn} -> {dst} ({img.size})")
    else:
        print(f"Not found: {src}")
