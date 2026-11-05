with open('Assets/Editor/ProjectAutoSetup.cs', 'r', encoding='utf-8') as f:
    content = f.read()

safe_font_method = '''    private static Font GetSafeDefaultFont()
    {
        Font font = null;
        try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
        if (font == null)
        {
            try { font = Font.CreateDynamicFontFromOSFont(new string[] { "Segoe UI", "Malgun Gothic", "Tahoma" }, 20); } catch { }
        }
        return font;
    }
'''

old_font_call = 'Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf")'

content = content.replace(old_font_call, 'GetSafeDefaultFont()')

if 'private static Font GetSafeDefaultFont()' not in content:
    content = content.replace('private static GameObject BuildCleanMobileHUD', safe_font_method + '\n    private static GameObject BuildCleanMobileHUD')

# Update BuildSettingsPopup initial strings
content = content.replace('"CÀI ĐẶT TRÒ CHƠI  |  게임 설정"', '""')
content = content.replace('"Nhạc Nền / 배경음"', '"Nhạc Nền"')
content = content.replace('"Hiệu Ứng / 효과음"', '"Hiệu Ứng"')
content = content.replace('"Khán Giả / 관중 환호"', '"Khán Giả"')
content = content.replace('"Ngôn Ngữ / 언어"', '"Ngôn Ngữ"')
content = content.replace('"Song Ngữ  (VN | KR)"', '"Tiếng Việt"')

with open('Assets/Editor/ProjectAutoSetup.cs', 'w', encoding='utf-8') as f:
    f.write(content)

print('Updated ProjectAutoSetup.cs successfully')
