import re

with open('Assets/Scenes/MainGame.unity', 'r', encoding='utf-8') as f:
    content = f.read()

# Replace Image sprite on specific GameObjects by finding their Image components

def replace_sprite_for_gameobject(scene_text, go_id, new_guid):
    # Find all components of this GameObject
    # 1. find GameObject block
    go_match = re.search(r'--- !u!1 &' + str(go_id) + r'\s+GameObject:.*?(?=--- !u!|\Z)', scene_text, re.DOTALL)
    if not go_match:
        print(f"GO {go_id} not found!")
        return scene_text
    
    # find component fileIDs
    comp_ids = re.findall(r'- component: \{fileID: (\d+)\}', go_match.group(0))
    print(f"GO {go_id} components: {comp_ids}")
    
    # find the Image component (114) among them
    for cid in comp_ids:
        comp_pattern = r'(--- !u!114 &' + cid + r'\s+MonoBehaviour:.*?m_Sprite: \{fileID: 21300000, guid: )([0-9a-fA-F]+)(\})'
        if re.search(comp_pattern, scene_text, re.DOTALL):
            scene_text = re.sub(comp_pattern, r'\g<1>' + new_guid + r'\g<3>', scene_text, count=1, flags=re.DOTALL)
            print(f"Updated sprite on component {cid} of GO {go_id} to {new_guid}")
            break
            
    return scene_text

# 1. Btn_Title_LocalPlay (GO: 1768225115) -> btn_title_local_vn (2dcc8110beec00b3dd85cda2888b5ced)
content = replace_sprite_for_gameobject(content, 1768225115, '2dcc8110beec00b3dd85cda2888b5ced')

# 2. Btn_Title_PvP (GO: 411288948) -> btn_title_pvp_vn (b9d0ecee42535602f419fa8d42adbcf9)
content = replace_sprite_for_gameobject(content, 411288948, 'b9d0ecee42535602f419fa8d42adbcf9')

# 3. Btn_Title_Codex (GO: 1492532299) -> btn_title_codex_vn (a97b544617a669ebd79d6e496740faeb)
content = replace_sprite_for_gameobject(content, 1492532299, 'a97b544617a669ebd79d6e496740faeb')

# 4. Btn_Title_Tutorial (GO: 2019887799) -> btn_title_guide_vn (7df284eb0c3af30027e005d3bc4f3dea)
content = replace_sprite_for_gameobject(content, 2019887799, '7df284eb0c3af30027e005d3bc4f3dea')

# 5. PvP_Popup (GO: 121543883 or similar)
pvp_go = re.search(r'--- !u!1 &(\d+)\s+GameObject:\s+.*?m_Name: PvP_Popup', content, re.DOTALL)
if pvp_go:
    content = replace_sprite_for_gameobject(content, int(pvp_go.group(1)), '61c799aff6f226db37029fa8a873064c')

with open('Assets/Scenes/MainGame.unity', 'w', encoding='utf-8') as f:
    f.write(content)

print("Finished updating scene sprites!")
