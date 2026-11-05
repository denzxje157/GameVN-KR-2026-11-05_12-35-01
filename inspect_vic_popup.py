with open('Assets/Scenes/MainGame.unity', 'r', encoding='utf-8') as f:
    lines = f.readlines()

for i, l in enumerate(lines):
    if 'm_Name: VictoryDefeat_Popup' in l:
        for j in range(max(0, i-10), min(len(lines), i+80)):
            print(f"{j}: {lines[j].strip()}")
        break
