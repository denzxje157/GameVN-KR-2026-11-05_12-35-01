import sys

sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Editor/ProjectAutoSetup.cs', 'r', encoding='utf-8') as f:
    lines = f.readlines()

start_idx = -1
end_idx = -1

for i, l in enumerate(lines):
    if '// 3. Khung Chọn Sới Đấu ở bên trái' in l:
        start_idx = i
    if 'return canvasObj;' in l and start_idx != -1 and end_idx == -1:
        end_idx = i

print(f"start_idx: {start_idx}, end_idx: {end_idx}")

new_lines = [
"""        // 3. Khung Chọn Sới Đấu ở bên trái (Panel_MapSelect)
        GameObject mapPanel = new GameObject("Panel_MapSelect");
        mapPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform mpRect = mapPanel.AddComponent<RectTransform>();
        mpRect.anchorMin = new Vector2(0.5f, 0.5f);
        mpRect.anchorMax = new Vector2(0.5f, 0.5f);
        mpRect.pivot = new Vector2(0.5f, 0.5f);
        mpRect.anchoredPosition = new Vector2(-380, 20);
        mpRect.sizeDelta = new Vector2(580, 520);

        Image mpBg = mapPanel.AddComponent<Image>();
        Sprite boardSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/panel_match_hub_board.png");
        if (boardSprite != null)
        {
            mpBg.sprite = boardSprite;
            mpBg.color = Color.white;
            mpBg.preserveAspect = false;
        }
        else
        {
            mpBg.color = new Color(0.14f, 0.10f, 0.18f, 0.95f);
        }

        GameObject mapLabelObj = new GameObject("Label_Map");
        mapLabelObj.transform.SetParent(mapPanel.transform, false);
        Text mapLabel = mapLabelObj.AddComponent<Text>();
        mapLabel.text = "— CHỌN SỚI ĐẤU (3 BẢN ĐỒ) —";
        mapLabel.font = font;
        mapLabel.fontSize = 24;
        mapLabel.fontStyle = FontStyle.Bold;
        mapLabel.alignment = TextAnchor.MiddleCenter;
        mapLabel.color = new Color(1f, 0.85f, 0.3f);
        mapLabel.raycastTarget = false;
        RectTransform mlRect = mapLabelObj.GetComponent<RectTransform>();
        mlRect.anchoredPosition = new Vector2(0, 220);
        mlRect.sizeDelta = new Vector2(540, 40);

        // Khung Map Preview hình Oval
        GameObject mapImgObj = new GameObject("Image_MapPreview");
        mapImgObj.transform.SetParent(mapPanel.transform, false);
        Image mapImg = mapImgObj.AddComponent<Image>();
        Sprite defaultThumb = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Thumbnails/thumb_san1-lang.png");
        mapImg.sprite = defaultThumb;
        mapImg.preserveAspect = true;
        mapImg.raycastTarget = false;
        RectTransform miRect = mapImgObj.GetComponent<RectTransform>();
        miRect.anchoredPosition = new Vector2(0, 80);
        miRect.sizeDelta = new Vector2(400, 190);

        // Khung Oval gỗ viền quanh preview
        Sprite ovalFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/frame_map_oval.png");
        if (ovalFrame != null)
        {
            GameObject frameObj = new GameObject("Frame_MapOval");
            frameObj.transform.SetParent(mapPanel.transform, false);
            Image fImg = frameObj.AddComponent<Image>();
            fImg.sprite = ovalFrame;
            fImg.color = Color.white;
            fImg.preserveAspect = true;
            fImg.raycastTarget = false;
            RectTransform frt = frameObj.GetComponent<RectTransform>();
            frt.anchoredPosition = new Vector2(0, 80);
            frt.sizeDelta = new Vector2(430, 210);
        }

        GameObject mapTitleObj = new GameObject("Text_MapTitle");
        mapTitleObj.transform.SetParent(mapPanel.transform, false);
        Text mapTitle = mapTitleObj.AddComponent<Text>();
        mapTitle.text = "1. Sới Đấu Lễ Hội Dân Gian";
        mapTitle.font = font;
        mapTitle.fontSize = 22;
        mapTitle.fontStyle = FontStyle.Bold;
        mapTitle.alignment = TextAnchor.MiddleCenter;
        mapTitle.color = new Color(1f, 0.9f, 0.3f);
        mapTitle.raycastTarget = false;
        RectTransform mtRect = mapTitleObj.GetComponent<RectTransform>();
        mtRect.anchoredPosition = new Vector2(0, -45);
        mtRect.sizeDelta = new Vector2(520, 35);

        GameObject mapDescObj = new GameObject("Text_MapDesc");
        mapDescObj.transform.SetParent(mapPanel.transform, false);
        Text mapDesc = mapDescObj.AddComponent<Text>();
        mapDesc.text = "Sới đấu đất nện kết hợp sàn cát Ssireum hữu nghị giữa hội làng Việt - Hàn.";
        mapDesc.font = font;
        mapDesc.fontSize = 17;
        mapDesc.alignment = TextAnchor.MiddleCenter;
        mapDesc.color = new Color(0.9f, 0.9f, 0.9f);
        mapDesc.raycastTarget = false;
        RectTransform mdRect = mapDescObj.GetComponent<RectTransform>();
        mdRect.anchoredPosition = new Vector2(0, -100);
        mdRect.sizeDelta = new Vector2(520, 60);

        Sprite arrowRedLeft = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/arrow_red_left.png");
        Sprite arrowYellowRight = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/arrow_yellow_right.png");
        Sprite arrowBlueRight = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/arrow_blue_right.png");

        GameObject btnPrevMap = CreateIconButton(mapPanel.transform, "Btn_PrevMap", arrowRedLeft, new Vector2(52, 52), new Vector2(-130, -175));
        GameObject btnNextMap = CreateIconButton(mapPanel.transform, "Btn_NextMap", arrowYellowRight, new Vector2(52, 52), new Vector2(130, -175));

        // 4. Khung Chọn Linh Vật ở bên phải (Panel_CharSelect: 5 VN vs 5 KR)
        GameObject charPanel = new GameObject("Panel_CharSelect");
        charPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform cpRect = charPanel.AddComponent<RectTransform>();
        cpRect.anchorMin = new Vector2(0.5f, 0.5f);
        cpRect.anchorMax = new Vector2(0.5f, 0.5f);
        cpRect.pivot = new Vector2(0.5f, 0.5f);
        cpRect.anchoredPosition = new Vector2(380, 20);
        cpRect.sizeDelta = new Vector2(620, 520);

        Image cpBg = charPanel.AddComponent<Image>();
        Sprite splitBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/character_select_split_bg.png");
        if (splitBg != null)
        {
            cpBg.sprite = splitBg;
            cpBg.color = Color.white;
            cpBg.preserveAspect = false;
        }
        else
        {
            cpBg.color = new Color(0.14f, 0.10f, 0.18f, 0.0f);
        }

        GameObject charLabelObj = new GameObject("Label_Char");
        charLabelObj.transform.SetParent(charPanel.transform, false);
        Text charLabel = charLabelObj.AddComponent<Text>();
        charLabel.text = "— CHỌN LINH VẬT TRANH TÀI (5 vs 5) —";
        charLabel.font = font;
        charLabel.fontSize = 24;
        charLabel.fontStyle = FontStyle.Bold;
        charLabel.alignment = TextAnchor.MiddleCenter;
        charLabel.color = new Color(1f, 0.85f, 0.3f);
        charLabel.raycastTarget = false;
        RectTransform clRect = charLabelObj.GetComponent<RectTransform>();
        clRect.anchoredPosition = new Vector2(0, 220);
        clRect.sizeDelta = new Vector2(560, 40);

        // Player Section (Việt Nam)
        GameObject pCard = new GameObject("Card_Player");
        pCard.transform.SetParent(charPanel.transform, false);
        RectTransform pcRect = pCard.AddComponent<RectTransform>();
        pcRect.anchoredPosition = new Vector2(-150, 45);
        pcRect.sizeDelta = new Vector2(230, 280);
        Image pcImg = pCard.AddComponent<Image>();
        Sprite cardVnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/card_character_vn.png");
        if (cardVnSprite != null) { pcImg.sprite = cardVnSprite; pcImg.color = Color.white; }
        else { pcImg.color = new Color(0.20f, 0.14f, 0.25f, 0.9f); }

        GameObject pAvObj = new GameObject("Image_PlayerAvatar");
        pAvObj.transform.SetParent(pCard.transform, false);
        Image pAvImg = pAvObj.AddComponent<Image>();
        Sprite defaultPAv = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Avatars/avatar_trau_vang.png");
        pAvImg.sprite = defaultPAv;
        pAvImg.preserveAspect = true;
        pAvImg.raycastTarget = false;
        RectTransform pavRect = pAvObj.GetComponent<RectTransform>();
        pavRect.anchoredPosition = new Vector2(0, 40);
        pavRect.sizeDelta = new Vector2(92, 92);

        // Viền Avatar Tròn Trống Đồng từ UI_Accessories_Sheet
        GameObject pAvFrameObj = new GameObject("Image_PlayerAvatarFrame");
        pAvFrameObj.transform.SetParent(pCard.transform, false);
        Image pAvFrame = pAvFrameObj.AddComponent<Image>();
        Sprite circFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/avatar_frame_circle.png");
        if (circFrameSprite != null) pAvFrame.sprite = circFrameSprite;
        pAvFrame.preserveAspect = true;
        pAvFrame.raycastTarget = false;
        RectTransform pavfRect = pAvFrameObj.GetComponent<RectTransform>();
        pavfRect.anchoredPosition = new Vector2(0, 40);
        pavfRect.sizeDelta = new Vector2(104, 104);

        GameObject pTextObj = new GameObject("Text_PlayerMascot");
        pTextObj.transform.SetParent(pCard.transform, false);
        Text pText = pTextObj.AddComponent<Text>();
        pText.text = "VN: Trâu Vàng (Kim Ngưu)";
        pText.font = font;
        pText.fontSize = 17;
        pText.fontStyle = FontStyle.Bold;
        pText.alignment = TextAnchor.MiddleCenter;
        pText.color = new Color(1f, 0.9f, 0.3f);
        pText.raycastTarget = false;
        RectTransform ptRect = pTextObj.GetComponent<RectTransform>();
        ptRect.anchoredPosition = new Vector2(0, -35);
        ptRect.sizeDelta = new Vector2(220, 28);

        GameObject pRoleObj = new GameObject("Text_PlayerRole");
        pRoleObj.transform.SetParent(pCard.transform, false);
        Text pRole = pRoleObj.AddComponent<Text>();
        pRole.text = "Tanker - Trụ sàn • Kim Ngưu";
        pRole.font = font;
        pRole.fontSize = 13;
        pRole.alignment = TextAnchor.MiddleCenter;
        pRole.color = new Color(0.9f, 0.9f, 0.9f);
        pRole.raycastTarget = false;
        RectTransform prRect = pRoleObj.GetComponent<RectTransform>();
        prRect.anchoredPosition = new Vector2(0, -65);
        prRect.sizeDelta = new Vector2(220, 25);

        // Nút mũi tên chuyển tướng Việt Nam
        GameObject btnPrevP = CreateIconButton(charPanel.transform, "Btn_PrevPlayer", arrowRedLeft, new Vector2(46, 46), new Vector2(-280, 45));
        GameObject btnNextP = CreateIconButton(charPanel.transform, "Btn_NextPlayer", arrowYellowRight, new Vector2(46, 46), new Vector2(-20, 45));

        Sprite btnConfirmRed = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/btn_confirm_red.png");
        GameObject btnSwapP = CreateButtonWithSpriteAndText(charPanel.transform, "Btn_NextPlayerText", "ĐỔI TƯỚNG VN (5)", btnConfirmRed, new Vector2(180, 46), new Vector2(-150, -140), font, 15);
        btnSwapP.GetComponent<Button>().onClick.AddListener(() => btnNextP.GetComponent<Button>().onClick.Invoke());

        // BIỂU TƯỢNG VS LỬA Ở CHÍNH GIỮA
        GameObject vsObj = new GameObject("Icon_VS");
        vsObj.transform.SetParent(charPanel.transform, false);
        Image vsImg = vsObj.AddComponent<Image>();
        Sprite vsSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/icon_fire_vs.png");
        if (vsSprite != null) vsImg.sprite = vsSprite;
        vsImg.preserveAspect = true;
        vsImg.raycastTarget = false;
        RectTransform vsRect = vsObj.GetComponent<RectTransform>();
        vsRect.anchoredPosition = new Vector2(0, 45);
        vsRect.sizeDelta = new Vector2(85, 125);

        // Enemy Section (Hàn Quốc)
        GameObject eCard = new GameObject("Card_Enemy");
        eCard.transform.SetParent(charPanel.transform, false);
        RectTransform ecRect = eCard.AddComponent<RectTransform>();
        ecRect.anchoredPosition = new Vector2(150, 45);
        ecRect.sizeDelta = new Vector2(230, 280);
        Image ecImg = eCard.AddComponent<Image>();
        Sprite cardKrSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/card_character_kr.png");
        if (cardKrSprite != null) { ecImg.sprite = cardKrSprite; ecImg.color = Color.white; }
        else { ecImg.color = new Color(0.20f, 0.14f, 0.25f, 0.9f); }

        GameObject eAvObj = new GameObject("Image_EnemyAvatar");
        eAvObj.transform.SetParent(eCard.transform, false);
        Image eAvImg = eAvObj.AddComponent<Image>();
        Sprite defaultEAv = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Avatars/avatar_ho_baekho.png");
        eAvImg.sprite = defaultEAv;
        eAvImg.preserveAspect = true;
        eAvImg.raycastTarget = false;
        RectTransform eavRect = eAvObj.GetComponent<RectTransform>();
        eavRect.anchoredPosition = new Vector2(0, 40);
        eavRect.sizeDelta = new Vector2(92, 92);

        // Viền Avatar Tròn Trống Đồng Hàn Quốc
        GameObject eAvFrameObj = new GameObject("Image_EnemyAvatarFrame");
        eAvFrameObj.transform.SetParent(eCard.transform, false);
        Image eAvFrame = eAvFrameObj.AddComponent<Image>();
        if (circFrameSprite != null) eAvFrame.sprite = circFrameSprite;
        eAvFrame.preserveAspect = true;
        eAvFrame.raycastTarget = false;
        RectTransform eavfRect = eAvFrameObj.GetComponent<RectTransform>();
        eavfRect.anchoredPosition = new Vector2(0, 40);
        eavfRect.sizeDelta = new Vector2(104, 104);

        GameObject eTextObj = new GameObject("Text_EnemyMascot");
        eTextObj.transform.SetParent(eCard.transform, false);
        Text eText = eTextObj.AddComponent<Text>();
        eText.text = "HQ: Bạch Hổ Baekho";
        eText.font = font;
        eText.fontSize = 17;
        eText.fontStyle = FontStyle.Bold;
        eText.alignment = TextAnchor.MiddleCenter;
        eText.color = new Color(0.4f, 0.85f, 1f);
        eText.raycastTarget = false;
        RectTransform etRect = eTextObj.GetComponent<RectTransform>();
        etRect.anchoredPosition = new Vector2(0, -35);
        etRect.sizeDelta = new Vector2(220, 28);

        GameObject eRoleObj = new GameObject("Text_EnemyRole");
        eRoleObj.transform.SetParent(eCard.transform, false);
        Text eRole = eRoleObj.AddComponent<Text>();
        eRole.text = "Bruiser - Sức Mạnh • Hổ Khiếu";
        eRole.font = font;
        eRole.fontSize = 13;
        eRole.alignment = TextAnchor.MiddleCenter;
        eRole.color = new Color(0.9f, 0.9f, 0.9f);
        eRole.raycastTarget = false;
        RectTransform erRect = eRoleObj.GetComponent<RectTransform>();
        erRect.anchoredPosition = new Vector2(0, -65);
        erRect.sizeDelta = new Vector2(220, 25);

        // Nút mũi tên chuyển tướng Hàn Quốc
        GameObject btnPrevE = CreateIconButton(charPanel.transform, "Btn_PrevEnemy", arrowRedLeft, new Vector2(46, 46), new Vector2(20, 45));
        GameObject btnNextE = CreateIconButton(charPanel.transform, "Btn_NextEnemy", arrowBlueRight, new Vector2(46, 46), new Vector2(280, 45));

        Sprite btnSelectBlue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/btn_select_char_blue.png");
        GameObject btnSwapE = CreateButtonWithSpriteAndText(charPanel.transform, "Btn_NextEnemyText", "ĐỔI ĐỐI THỦ HQ (5)", btnSelectBlue, new Vector2(180, 46), new Vector2(150, -140), font, 15);
        btnSwapE.GetComponent<Button>().onClick.AddListener(() => btnNextE.GetComponent<Button>().onClick.Invoke());

        // 5. NÚT BẮT ĐẦU TRANH TÀI TO LỚN Ở DƯỚI CÙNG (VÀO TRẬN LỬA)
        Sprite vaoTranSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/btn_vao_tran_fire.png");
        GameObject btnStart = CreateTitleButton(canvasObj.transform, "Btn_StartBattle", vaoTranSprite, new Vector2(360, 115), new Vector2(0, -380));

        // Nút Quay lại Màn Hình Bắt Đầu (Gỗ QUAY LẠI)
        Sprite quayLaiSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/btn_quay_lai.png");
        GameObject btnBack = CreateTitleButton(canvasObj.transform, "Btn_BackToTitle", quayLaiSprite, new Vector2(170, 72), new Vector2(-420, -380));

        // Nút Hướng Dẫn & Âm Thanh ở góc dưới phải
        Sprite signTutSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/sign_huong_dan.png");
        GameObject btnTut = CreateButtonWithSpriteAndText(canvasObj.transform, "Btn_Tutorial", "HƯỚNG DẪN LUẬT", signTutSprite, new Vector2(190, 60), new Vector2(320, -380), font, 17);

        Sprite signSetSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MatchHub/sign_cai_dat.png");
        GameObject btnSnd = CreateButtonWithSpriteAndText(canvasObj.transform, "Btn_Sound", "CÀI ĐẶT / ÂM THANH", signSetSprite, new Vector2(180, 60), new Vector2(520, -380), font, 16);

"""
]

lines = lines[:start_idx] + new_lines + lines[end_idx:]

with open('Assets/Editor/ProjectAutoSetup.cs', 'w', encoding='utf-8') as f:
    f.writelines(lines)

print("Updated BuildMainMenuUI via line indexing!")
