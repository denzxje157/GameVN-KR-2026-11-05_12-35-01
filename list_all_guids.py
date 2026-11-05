import os, sys, re

sys.stdout.reconfigure(encoding='utf-8')

sprites_dir = 'Assets/Sprites'
sprite_guids = {}

for root, dirs, files in os.walk(sprites_dir):
    for f in files:
        if f.endswith('.png.meta') or f.endswith('.jpg.meta'):
            meta_path = os.path.join(root, f)
            sprite_path = meta_path[:-5]
            with open(meta_path, 'r', encoding='utf-8') as mp:
                m = re.search(r'guid:\s*([0-9a-fA-F]{32})', mp.read())
                if m:
                    rel_p = os.path.relpath(sprite_path, 'Assets').replace('\\', '/')
                    sprite_guids[rel_p] = m.group(1)

print(f"Total sprites with meta: {len(sprite_guids)}")
for k in sorted(sprite_guids.keys()):
    if any(term in k for term in ['UI', 'Popups', 'MatchHub', 'Title', 'Maps', 'Avatars']):
        print(f"  {k}: {sprite_guids[k]}")
