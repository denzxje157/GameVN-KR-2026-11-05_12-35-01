import re

path = 'Assets/Scripts/MenuManager.cs'
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Update Start() to register OnLanguageChanged
old_start_needle = '''        else
        {
            ShowTitleScreen();
        }'''

new_start_needle = '''        else
        {
            ShowTitleScreen();
        }

        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= OnLangChanged;
            LocalizationManager.Instance.OnLanguageChanged += OnLangChanged;
        }
        UpdateLocalizedTexts();'''

if old_start_needle in content and 'OnLangChanged' not in content:
    content = content.replace(old_start_needle, new_start_needle)
    print('Updated Start() in MenuManager.cs')

# 2. Update UpdateLocalizedTexts and add UpdateLocalizedSprites
old_update_pattern = r'    public void UpdateLocalizedTexts\(\)\s*\{.*?UpdateMascotUI\(\);\s*\}'

new_update_impl = '''    private void OnLangChanged(LocalizationManager.LanguageMode mode)
    {
        UpdateLocalizedTexts();
    }

    public void UpdateLocalizedTexts()
    {
        if (LocalizationManager.Instance == null) return;
        var loc = LocalizationManager.Instance;

        var langText = FindSceneObject("Text_Value_Lang")?.GetComponent<TMPro.TextMeshProUGUI>();
        if (langText != null) langText.text = loc.GetLanguageDisplayName();

        var titleText = FindSceneObject("Text_Title")?.GetComponent<TMPro.TextMeshProUGUI>();
        if (titleText != null) titleText.text = loc.TextSettingsTitle;

        void FixLabel(TMPro.TextMeshProUGUI label, string txt)
        {
            if (label == null) return;
            label.text = txt;
            label.enableAutoSizing = true;
            label.fontSizeMin = 13;
            label.fontSizeMax = 18;
            label.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        }

        var bgmLabel = FindSceneObject("Text_Label_BGM")?.GetComponent<TMPro.TextMeshProUGUI>();
        FixLabel(bgmLabel, loc.TextBGM);

        var sfxLabel = FindSceneObject("Text_Label_SFX")?.GetComponent<TMPro.TextMeshProUGUI>();
        FixLabel(sfxLabel, loc.TextSFX);

        var crowdLabel = FindSceneObject("Text_Label_Crowd")?.GetComponent<TMPro.TextMeshProUGUI>();
        FixLabel(crowdLabel, loc.TextCrowd);

        var langLabel = FindSceneObject("Text_Label_Lang")?.GetComponent<TMPro.TextMeshProUGUI>();
        FixLabel(langLabel, loc.TextLanguage);

        UpdateMapUI();
        UpdateMascotUI();
        UpdateLocalizedSprites();
    }

    public void UpdateLocalizedSprites()
    {
        if (LocalizationManager.Instance == null) return;
        bool isKorean = LocalizationManager.Instance.CurrentLanguage == LocalizationManager.LanguageMode.Korean;
        string tag = isKorean ? "kr" : "vn";

        void SwapImage(string objName, string resourceSubPath)
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
        }

        // 1. Title Screen 4 Buttons
        SwapImage("Btn_Title_LocalPlay", $"UI/Title/btn_title_local_{tag}");
        SwapImage("Btn_Title_PvP", $"UI/Title/btn_title_pvp_{tag}");
        SwapImage("Btn_Title_Codex", $"UI/Title/btn_title_codex_{tag}");
        SwapImage("Btn_Title_Tutorial", $"UI/Title/btn_title_guide_{tag}");

        // 2. MatchHub Action Buttons & Signs
        SwapImage("Btn_StartBattle", $"UI/MatchHub/btn_vao_tran_fire_{tag}");
        SwapImage("Btn_BackToTitle", $"UI/MatchHub/btn_quay_lai_{tag}");
        SwapImage("Btn_Back", $"UI/MatchHub/btn_quay_lai_{tag}");
        SwapImage("Btn_MatchHub_Settings", $"UI/MatchHub/sign_cai_dat_{tag}");
        SwapImage("Btn_Sound", $"UI/MatchHub/sign_cai_dat_{tag}");
        SwapImage("Btn_Tutorial", $"UI/MatchHub/sign_huong_dan_{tag}");

        // 3. MatchHub Character Select Board
        SwapImage("Panel_CharSelect", $"UI/MatchHub/panel_char_select_{tag}");

        // 4. Settings Popup
        SwapImage("Panel_Settings", $"UI/Popups/frame_settings_{tag}");
        SwapImage("Btn_CloseSettings", $"UI/MatchHub/btn_dong_{tag}");
        SwapImage("Btn_Close", $"UI/MatchHub/btn_dong_{tag}");
        SwapImage("Btn_Close_X", "UI/Popups/btn_settings_close_x");

        // 5. Popups: Tutorial, Codex, PvP
        SwapImage("Tutorial_Popup", $"UI/Popups/panel_codex_{tag}");
        SwapImage("Codex_Popup", $"UI/Popups/panel_codex_{tag}");
        SwapImage("PvP_Popup", $"UI/Popups/frame_pvp_{tag}");
        SwapImage("Btn_PvP_Create", $"UI/Popups/btn_pvp_create_{tag}");
        SwapImage("Btn_PvP_Join", $"UI/Popups/btn_pvp_join_{tag}");

        // 6. In-Game Battle HUD
        SwapImage("Btn_Skill", $"UI/Battle/btn_skill_fist_{tag}");
        SwapImage("Btn_Pause", $"UI/Battle/btn_battle_pause_{tag}");

        // 7. Victory / Defeat Popup Buttons
        SwapImage("Btn_Replay", $"UI/Popups/btn_result_replay_{tag}");
        SwapImage("Btn_Home", $"UI/Popups/btn_result_close_{tag}");
    }'''

content = re.sub(old_update_pattern, new_update_impl, content, flags=re.DOTALL)

with open(path, 'w', encoding='utf-8') as f:
    f.write(content)

print('MenuManager.cs updated successfully via script.')
