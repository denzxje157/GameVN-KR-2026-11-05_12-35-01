import os, sys
from PIL import Image
import numpy as np

sys.stdout.reconfigure(encoding='utf-8')
artifact_dir = r"C:\Users\Admin\.gemini\antigravity\brain\777661cc-5705-48ae-8e41-a23175dba7d6"

img11 = Image.open("picture/pic.11-dau-doi-khang.png")
# Save a medium preview in artifacts
img11.save(os.path.join(artifact_dir, "preview_pic11.png"))

img10 = Image.open("picture/pic.10-background-cac-access.png")
img10.save(os.path.join(artifact_dir, "preview_pic10.png"))

img12 = Image.open("picture/pic.12-background-chinh.png")
img12.save(os.path.join(artifact_dir, "preview_pic12.png"))

img_set = Image.open("picture/UI_Settings_Sheet_Complete.png")
img_set.save(os.path.join(artifact_dir, "preview_ui_settings.png"))

print("Saved previews to artifacts!")
