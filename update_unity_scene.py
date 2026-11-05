import re

with open('Assets/Scenes/MainGame.unity', 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Clear bilingual texts
content = content.replace('m_text: "Nh\\u1EA1c N\\u1EC1n / \\uBC30\\uACBD\\uC74C"', 'm_text: "Nh\\u1EA1c N\\u1EC1n"')
content = content.replace('m_text: "Hi\\u1EC7u \\u1EE8ng / \\uD6A8\\uACFC\\uC74C"', 'm_text: "Hi\\u1EC7u \\u1EE8ng"')
content = content.replace('m_text: "Kh\\xE1n Gi\\u1EA3 / \\uAD00\\uC911 \\uD658\\uD638"', 'm_text: "Kh\\xE1n Gi\\u1EA3"')
content = content.replace('m_text: "Ng\\xF4n Ng\\u1EEF / \\uC5B8\\uC5B4"', 'm_text: "Ng\\xF4n Ng\\u1EEF"')
content = content.replace('m_text: "Song Ng\\u1EEF  (VN | KR)"', 'm_text: "Ti\\u1EBFng Vi\\u1EC7t"')
content = content.replace('m_text: "C\\xC0I \\u0110\\u1EB6T TR\\xD2 CH\\u01A0I  |  \\uAC8C\\uC784 \\uC124\\uC815"', 'm_text: ""')

# Also check for non-escaped variants
content = content.replace('Nhạc Nền / 배경음', 'Nhạc Nền')
content = content.replace('Hiệu Ứng / 효과음', 'Hiệu Ứng')
content = content.replace('Khán Giả / 관중 환호', 'Khán Giả')
content = content.replace('Ngôn Ngữ / 언어', 'Ngôn Ngữ')
content = content.replace('Song Ngữ (VN | KR)', 'Tiếng Việt')
content = content.replace('Song Ngữ  (VN | KR)', 'Tiếng Việt')
content = content.replace('CÀI ĐẶT TRÒ CHƠI  |  게임 설정', '')
content = content.replace('CÀI ĐẶT TRÒ CHƠI | 게임 설정', '')

# 2. Tutorial popup changes
# Replace sprite guid on Tutorial_Popup Image (114 &973450860)
# old guid: d644e4c775f6b19d004225d6e8b7db63
# new guid: 99fa582add6a4785aa7a1caef530c29b (panel_tutorial_bamboo_vn)
content = re.sub(
    r'--- !u!114 &973450860.*?m_Sprite: \{fileID: 21300000, guid: [0-9a-fA-F]+',
    r'--- !u!114 &973450860\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: 973450858}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}\n  m_Name: \n  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Image\n  m_Material: {fileID: 0}\n  m_Color: {r: 1, g: 1, b: 1, a: 1}\n  m_RaycastTarget: 1\n  m_RaycastPadding: {x: 0, y: 0, z: 0, w: 0}\n  m_Maskable: 1\n  m_OnCullStateChanged:\n    m_PersistentCalls:\n      m_Calls: []\n  m_Sprite: {fileID: 21300000, guid: 99fa582add6a4785aa7a1caef530c29b',
    content,
    flags=re.DOTALL
)

# 3. Clear Tutorial_Popup Text_Title
content = content.replace('m_Text: "H\\u01AF\\u1EDA NG D\\u1EAAN LU\\u1EACT TRANH T\\xC0I H\\u1EEAEU NGH\\u1EAE"', 'm_Text: ""')
content = content.replace('m_Text: "HƯỚNG DẪN LUẬT TRANH TÀI HỮU NGHỊ"', 'm_Text: ""')

# 4. Clear Btn_CloseTutorial Text
content = content.replace('m_Text: "\\u0110\\xC3 HI\\u1EC2U! V\\xC0O TRANH T\\xC0I"', 'm_Text: ""')
content = content.replace('m_Text: "ĐÃ HIỂU! VÀO TRANH TÀI"', 'm_Text: ""')

with open('Assets/Scenes/MainGame.unity', 'w', encoding='utf-8') as f:
    f.write(content)

print('Updated MainGame.unity successfully!')
