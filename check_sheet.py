import cv2
import numpy as np

img = cv2.imread('picture/UI_Accessories_Sheet.png')
bg_color = np.array([53.5, 53.5, 53.5])
diff = np.linalg.norm(img.astype(float) - bg_color, axis=2)
mask = (diff > 18).astype(np.uint8)

# Check btn_vao_tran flame bounds around x: 440..665, y: 350..525
vao_tran_mask = mask[350:525, 440:665]
ys, xs = np.where(vao_tran_mask > 0)
print(f"btn_vao_tran exact bounds: x: {440 + xs.min()}..{440 + xs.max()}, y: {350 + ys.min()}..{350 + ys.max()}")

# Check banner_victory bounds around x: 10..400, y: 10..270
vic_mask = mask[10:255, 10:400]
ys, xs = np.where(vic_mask > 0)
print(f"banner_victory exact bounds: x: {10 + xs.min()}..{10 + xs.max()}, y: {10 + ys.min()}..{10 + ys.max()}")

# Check banner_defeat bounds around x: 10..400, y: 260..540
def_mask = mask[260:525, 10:400]
ys, xs = np.where(def_mask > 0)
print(f"banner_defeat exact bounds: x: {10 + xs.min()}..{10 + xs.max()}, y: {260 + ys.min()}..{260 + ys.max()}")

# Check card_vn bounds around x: 420..565, y: 20..210
cvn_mask = mask[20:210, 420:565]
ys, xs = np.where(cvn_mask > 0)
print(f"card_vn exact bounds: x: {420 + xs.min()}..{420 + xs.max()}, y: {20 + ys.min()}..{20 + ys.max()}")

# Check card_kr bounds around x: 625..765, y: 20..210
ckr_mask = mask[20:210, 625:765]
ys, xs = np.where(ckr_mask > 0)
print(f"card_kr exact bounds: x: {625 + xs.min()}..{625 + xs.max()}, y: {20 + ys.min()}..{20 + ys.max()}")

# Check avatar frames bounds
c_circ = mask[210:325, 420:530]
ys, xs = np.where(c_circ > 0)
print(f"avatar_circle bounds: x: {420 + xs.min()}..{420 + xs.max()}, y: {210 + ys.min()}..{210 + ys.max()}")

c_oct = mask[210:325, 540:645]
ys, xs = np.where(c_oct > 0)
print(f"avatar_octagon bounds: x: {540 + xs.min()}..{540 + xs.max()}, y: {210 + ys.min()}..{210 + ys.max()}")

c_sq = mask[210:325, 655:765]
ys, xs = np.where(c_sq > 0)
print(f"avatar_square bounds: x: {655 + xs.min()}..{655 + xs.max()}, y: {210 + ys.min()}..{210 + ys.max()}")

# Check btn_quay_lai bounds
c_ql = mask[420:510, 675:755]
ys, xs = np.where(c_ql > 0)
print(f"btn_quay_lai bounds: x: {675 + xs.min()}..{675 + xs.max()}, y: {420 + ys.min()}..{420 + ys.max()}")

# Check arrow buttons bounds (4 arrows)
arr_red = mask[330:410, 425:500]
ys, xs = np.where(arr_red > 0)
print(f"arrow_red bounds: x: {425 + xs.min()}..{425 + xs.max()}, y: {330 + ys.min()}..{330 + ys.max()}")

arr_blue = mask[330:410, 600:675]
ys, xs = np.where(arr_blue > 0)
print(f"arrow_blue bounds: x: {600 + xs.min()}..{600 + xs.max()}, y: {330 + ys.min()}..{330 + ys.max()}")

arr_yellow = mask[330:410, 685:760]
ys, xs = np.where(arr_yellow > 0)
print(f"arrow_yellow bounds: x: {685 + xs.min()}..{685 + xs.max()}, y: {330 + ys.min()}..{330 + ys.max()}")
