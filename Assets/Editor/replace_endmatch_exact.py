with open('Assets/Scripts/GameManager.cs', 'r', encoding='utf-8') as f:
    lines = f.readlines()

start_idx = -1
end_idx = -1
for i, l in enumerate(lines):
    if 'private void EndMatch()' in l:
        start_idx = i
    if start_idx != -1 and 'private void UpdateScoreUI()' in l:
        end_idx = i
        break

clean_endmatch = '''    private void EndMatch()
    {
        SetState(GameState.GameOver);
        string message = "";
        bool isPlayerWin = playerScore > enemyScore;
        bool isEnemyWin = enemyScore > playerScore;

        bool isKorean = LocalizationManager.Instance != null && LocalizationManager.Instance.CurrentLanguage == LocalizationManager.LanguageMode.Korean;
        string tag = isKorean ? "kr" : "vn";

        Sprite vicSprite = Resources.Load<Sprite>($"UI/Popups/banner_victory_{tag}");
        Sprite defSprite = Resources.Load<Sprite>($"UI/Popups/banner_defeat_{tag}");
#if UNITY_EDITOR
        if (vicSprite == null) vicSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/UI/Popups/banner_victory_{tag}.png");
        if (defSprite == null) defSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/UI/Popups/banner_defeat_{tag}.png");
#endif
        if (vicSprite == null) vicSprite = victoryBannerSprite;
        if (defSprite == null) defSprite = defeatBannerSprite;

        if (isPlayerWin)
        {
            message = isKorean ? "<size=32><color=#FFD700><b>우정의 승리!</b></color></size>\\n\\n<size=20>베트남 대표팀이 sới đấu에서 승리했습니다!\\n양국이 함께 전통 음식을 나누며 우정을 축하합니다!</size>"
                               : "<size=32><color=#FFD700><b>CHIẾN THẮNG HỮU NGHỊ!</b></color></size>\\n\\n<size=20>Đội tuyển Việt Nam đã xuất sắc trụ vững sới đấu!\\nHai bên cùng chia sẻ ẩm thực Bánh Chưng & Kimchi!</size>";
            if (victoryBannerImage != null && vicSprite != null)
            {
                victoryBannerImage.sprite = vicSprite;
            }
        }
        else if (isEnemyWin)
        {
            message = isKorean ? "<size=32><color=#4D96FF><b>아쉬운 패배!</b></color></size>\\n\\n<size=20>대한민국 대표팀이 승리했습니다!\\n스포츠맨십과 우정의 가치를 높이 기립니다!</size>"
                               : "<size=32><color=#4D96FF><b>THẤT BẠI TIẾC NUỐI!</b></color></size>\\n\\n<size=20>Đội tuyển Hàn Quốc đã giành thắng lợi thuyết phục!\\nHai dân tộc cùng bắt tay tôn vinh tinh thần thượng võ!</size>";
            if (victoryBannerImage != null && defSprite != null)
            {
                victoryBannerImage.sprite = defSprite;
            }
        }
        else
        {
            message = isKorean ? "<size=32><color=#FFD700><b>아름다운 화합!</b></color></size>\\n\\n<size=20>양국이 대등한 기량으로 우정의 축제를 빛냈습니다!</size>"
                               : "<size=32><color=#FFD700><b>HÒA HỢP HỮU NGHỊ!</b></color></size>\\n\\n<size=20>Trận đấu ngang tài ngang sức mang đậm nét đẹp văn hóa Việt - Hàn!</size>";
            if (victoryBannerImage != null && vicSprite != null)
            {
                victoryBannerImage.sprite = vicSprite;
            }
        }

        if (victoryPanel != null)
        {
            var repImg = victoryPanel.transform.Find("Btn_Replay")?.GetComponent<Image>();
            if (repImg != null)
            {
                Sprite sp = Resources.Load<Sprite>($"UI/Popups/btn_result_replay_{tag}");
#if UNITY_EDITOR
                if (sp == null) sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/UI/Popups/btn_result_replay_{tag}.png");
#endif
                if (sp != null) repImg.sprite = sp;
            }

            var homeImg = victoryPanel.transform.Find("Btn_Home")?.GetComponent<Image>();
            if (homeImg != null)
            {
                Sprite sp = Resources.Load<Sprite>($"UI/Popups/btn_result_close_{tag}");
#if UNITY_EDITOR
                if (sp == null) sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/UI/Popups/btn_result_close_{tag}.png");
#endif
                if (sp != null) homeImg.sprite = sp;
            }
        }

        if (victoryTmpText != null) victoryTmpText.text = message;
        if (victoryMessageText != null) victoryMessageText.text = message;
        if (victoryPanel != null) victoryPanel.SetActive(true);

        Debug.Log("<color=cyan>[KẾT THÚC TRẬN ĐẤU]</color> " + (isPlayerWin ? "Việt Nam Thắng!" : isEnemyWin ? "Hàn Quốc Thắng!" : "Hòa!"));
    }

'''

lines[start_idx:end_idx] = [clean_endmatch]

with open('Assets/Scripts/GameManager.cs', 'w', encoding='utf-8') as f:
    f.writelines(lines)

print('Replaced EndMatch lines successfully.')
