with open('Assets/Scripts/MenuManager.cs', 'r', encoding='utf-8') as f:
    code = f.read()

# 1. Add LoadSpriteSafe method if not present
load_sprite_safe = '''    public static Sprite LoadSpriteSafe(string resourceSubPath)
    {
        Sprite sp = Resources.Load<Sprite>(resourceSubPath);
        if (sp == null)
        {
            var all = Resources.LoadAll<Sprite>(resourceSubPath);
            if (all != null && all.Length > 0) sp = all[0];
        }
#if UNITY_EDITOR
        if (sp == null)
        {
            string p1 = "Assets/Resources/" + resourceSubPath + ".png";
            string p2 = "Assets/Sprites/" + resourceSubPath + ".png";
            foreach (var path in new[] { p1, p2 })
            {
                sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sp != null) break;
                var subAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var a in subAssets)
                {
                    if (a is Sprite spr) { sp = spr; break; }
                }
                if (sp != null) break;
            }
        }
#endif
        return sp;
    }
'''

if 'public static Sprite LoadSpriteSafe' not in code:
    # insert before UpdateLocalizedSprites
    code = code.replace('    public void UpdateLocalizedSprites()', load_sprite_safe + '\n    public void UpdateLocalizedSprites()')

# 2. Update SwapImage to use LoadSpriteSafe
old_swap = '''        void SwapImage(string objName, string resourceSubPath)
        {
            var obj = FindSceneObject(objName);
            if (obj != null)
            {
                var img = obj.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    Sprite sp = Resources.Load<Sprite>(resourceSubPath);
#if UNITY_EDITOR
                    if (sp == null)
                    {
                        sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/" + resourceSubPath + ".png");
                    }
#endif
                    if (sp != null) img.sprite = sp;
                }
            }
        }'''

new_swap = '''        void SwapImage(string objName, string resourceSubPath)
        {
            var obj = FindSceneObject(objName);
            if (obj != null)
            {
                var img = obj.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    Sprite sp = LoadSpriteSafe(resourceSubPath);
                    if (sp != null) img.sprite = sp;
                }
            }
        }'''

code = code.replace(old_swap, new_swap)

# 3. Update MatchHub cards in UpdateLocalizedSprites to be transparent
old_cards = '''        // 2. MatchHub Cards & VS Badge
        SwapImage("Card_Player", $"UI/MatchHub/card_confirm_red_{tag}");
        SwapImage("Card_Enemy", $"UI/MatchHub/card_select_blue_{tag}");
        SwapImage("Icon_VS", "UI/Battle/badge_vs_fire");
        SwapImage("Image_PlayerAvatarFrame", "UI/Battle/avatar_frame_gold_round");
        SwapImage("Image_EnemyAvatarFrame", "UI/Battle/avatar_frame_gold_round");'''

new_cards = '''        // 2. MatchHub Cards & VS Badge - Giữ Card_Player và Card_Enemy trong suốt để lộ trống đồng & Taegeuk trên Panel_CharSelect
        SwapImage("Panel_CharSelect", $"UI/MatchHub/panel_char_select_{tag}");
        var cardP = FindSceneObject("Card_Player")?.GetComponent<UnityEngine.UI.Image>();
        if (cardP != null) cardP.color = new Color(1f, 1f, 1f, 0f);
        var cardE = FindSceneObject("Card_Enemy")?.GetComponent<UnityEngine.UI.Image>();
        if (cardE != null) cardE.color = new Color(1f, 1f, 1f, 0f);

        SwapImage("Icon_VS", "UI/Battle/badge_vs_fire");
        SwapImage("Image_PlayerAvatarFrame", "UI/Battle/avatar_frame_gold_round");
        SwapImage("Image_EnemyAvatarFrame", "UI/Battle/avatar_frame_gold_round");'''

code = code.replace(old_cards, new_cards)

# 4. Update Settings popup sprite swap to use LoadSpriteSafe
old_settings_swap = '''                Sprite sp = Resources.Load<Sprite>($"UI/Popups/frame_settings_{tag}");
#if UNITY_EDITOR
                if (sp == null) sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/UI/Popups/frame_settings_{tag}.png");
#endif'''

new_settings_swap = '''                Sprite sp = LoadSpriteSafe($"UI/Popups/frame_settings_{tag}");'''

code = code.replace(old_settings_swap, new_settings_swap)

with open('Assets/Scripts/MenuManager.cs', 'w', encoding='utf-8') as f:
    f.write(code)

print('Updated MenuManager.cs successfully')
