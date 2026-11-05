import cv2
import numpy as np
import os

img = cv2.imread('picture/UI_Accessories_Sheet.png')
bg_color = np.array([53.5, 53.5, 53.5])

def extract_rgba(crop, threshold=14.0, feather=16.0):
    diff = np.linalg.norm(crop.astype(float) - bg_color, axis=2)
    # Smooth alpha feathering
    alpha = np.clip((diff - threshold) * (255.0 / feather), 0, 255).astype(np.uint8)
    b, g, r = cv2.split(crop)
    rgba = cv2.merge([b, g, r, alpha])
    return rgba

# Crop definitions based on exact bounds:
items = {
    'banner_victory': (21, 10, 397, 254),
    'banner_defeat': (27, 263, 384, 524),
    'card_character_vn': (427, 23, 564, 206),
    'card_character_kr': (626, 23, 763, 206),
    'avatar_frame_circle': (425, 220, 528, 323),
    'avatar_frame_octagon': (541, 220, 644, 323),
    'avatar_frame_square': (659, 221, 760, 322),
    'btn_vao_tran_fire': (440, 350, 664, 522),
    'btn_quay_lai': (683, 448, 750, 509),
    'arrow_red_left': (432, 343, 495, 406),
    'arrow_blue_right': (609, 343, 672, 406),
    'arrow_yellow_right': (692, 343, 755, 406)
}

os.makedirs('Assets/Sprites/UI/MatchHub', exist_ok=True)

for name, (x1, y1, x2, y2) in items.items():
    crop = img[y1:y2, x1:x2].copy()
    rgba = extract_rgba(crop)
    
    # Target paths
    if name in ['card_character_vn', 'card_character_kr', 'btn_vao_tran_fire', 'btn_quay_lai']:
        out_path = f'Assets/Sprites/UI/MatchHub/{name}.png'
    else:
        out_path = f'Assets/Sprites/UI/{name}.png'
        
    cv2.imwrite(out_path, rgba)
    print(f"Saved: {out_path} ({rgba.shape[1]}x{rgba.shape[0]}px)")

# Also copy full sheet into Assets/Sprites/UI/
import shutil
shutil.copy('picture/UI_Accessories_Sheet.png', 'Assets/Sprites/UI/UI_Accessories_Sheet.png')
print("Copied full sheet to Assets/Sprites/UI/UI_Accessories_Sheet.png")
