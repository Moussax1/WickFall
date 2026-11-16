using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WickFall.UI;

namespace WickFall.EditorTools
{
    [InitializeOnLoad]
    public static class MainMenuBuilder
    {
        private const string SCENE_PATH = "Assets/Scenes/MainMenu.unity";
        private const string GAME_SCENE_PATH = "Assets/Scenes/GameScene.unity";

        private const string BG_SPRITE_PATH = "Assets/Sprites/UI/menu_background.png";
        private const string TITLE_LOGO_PATH = "Assets/Sprites/UI/title_logo.png";
        private const string MENU_FRAME_PATH = "Assets/Sprites/UI/menu_frame_reference.png";
        private const string SETTINGS_FRAME_PATH = "Assets/Sprites/UI/settings_frame_gothic.png";
        private const string PILL_HIGHLIGHT_PATH = "Assets/Sprites/UI/btn_glow_pill.png";
        private const string FRAME_CANDLE_PATH = "Assets/Sprites/UI/frame_candle.png";

        private const string BTN_STONE_NORM = "Assets/Sprites/UI/Buttons/btn_stone_slab.png";
        private const string BTN_STONE_HIGH = "Assets/Sprites/UI/Buttons/btn_stone_slab_high.png";

        private const string SLIDER_TRACK_PATH = "Assets/Sprites/UI/slider_groove.png";
        private const string SLIDER_FILL_PATH = "Assets/Sprites/UI/slider_flame_fill.png";
        private const string SLIDER_HANDLE_PATH = "Assets/Sprites/UI/slider_gem_thumb.png";

        private const string TOGGLE_BOX_PATH = "Assets/Sprites/UI/toggle_stone_box.png";
        private const string TOGGLE_CHECK_PATH = "Assets/Sprites/UI/toggle_flame_check.png";

        private const string HOVER_SFX_PATH = "Assets/Audio/menu_hover.wav";
        private const string CLICK_SFX_PATH = "Assets/Audio/menu_click.wav";
        private const string BGM_PATH = "Assets/Audio/crypt_ambient.wav";

        [MenuItem("WickFall/Build Main Menu Scene")]
        public static void BuildScene()
        {
            Debug.Log("[MainMenuBuilder] Building reference-accurate, rich WickFall Main Menu Scene...");

            // 1. Create fresh scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Camera Setup
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            Camera cam = cameraObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.04f, 0.03f, 1f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cameraObj.AddComponent<AudioListener>();
            cameraObj.transform.position = new Vector3(0, 0, -10);

            // Audio sources
            AudioSource sfxSource = cameraObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            AudioSource bgmSource = cameraObj.AddComponent<AudioSource>();
            AudioClip bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>(BGM_PATH);
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.volume = 0.6f;
            bgmSource.playOnAwake = true;

            // 3. EventSystem Setup
            GameObject eventSystemObj = new GameObject("EventSystem");
            EventSystem es = eventSystemObj.AddComponent<EventSystem>();
            InputSystemUIInputModule inputModule = eventSystemObj.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();

            // 4. Canvas Hierarchy
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
            MainMenuManager manager = canvasObj.AddComponent<MainMenuManager>();

            // 5. Full-Screen Crypt Background Image
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvasObj.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.raycastTarget = false;
            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BG_SPRITE_PATH);
            if (bgSprite != null)
            {
                bgImage.sprite = bgSprite;
                bgImage.preserveAspect = false;
            }
            else
            {
                bgImage.color = new Color(0.06f, 0.05f, 0.05f, 1f);
            }

            // 6. Prominent Gothic "WICKFALL" Title Logo (Placed directly above the menu frame)
            GameObject titleObj = new GameObject("TitleLogo");
            titleObj.transform.SetParent(canvasObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = new Vector2(-140f, 290f);
            titleRect.sizeDelta = new Vector2(560f, 190f);

            Image titleImg = titleObj.AddComponent<Image>();
            titleImg.raycastTarget = false;
            Sprite titleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TITLE_LOGO_PATH);
            if (titleSprite != null)
            {
                titleImg.sprite = titleSprite;
                titleImg.preserveAspect = true;
            }
            titleObj.AddComponent<CandleFlameFlicker>();

            // 7. Reference-Accurate Menu Frame Container
            GameObject frameObj = new GameObject("MenuOrnateFrame");
            frameObj.transform.SetParent(canvasObj.transform, false);
            RectTransform frameRect = frameObj.AddComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0.5f, 0.5f);
            frameRect.anchorMax = new Vector2(0.5f, 0.5f);
            frameRect.pivot = new Vector2(0.5f, 0.5f);
            frameRect.anchoredPosition = new Vector2(-140f, -40f);
            frameRect.sizeDelta = new Vector2(420f, 500f);

            Image frameImg = frameObj.AddComponent<Image>();
            frameImg.raycastTarget = false;
            Sprite frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(MENU_FRAME_PATH);
            if (frameSprite != null)
            {
                frameImg.sprite = frameSprite;
                frameImg.type = Image.Type.Sliced;
            }
            else
            {
                frameImg.color = new Color(0.08f, 0.07f, 0.06f, 0.95f);
            }

            // Atmospheric Candles on Frame Ledges
            Sprite candleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(FRAME_CANDLE_PATH);
            if (candleSprite != null)
            {
                CreateFrameCandle("Candle_Left", frameObj.transform, candleSprite, new Vector2(-215f, 85f));
                CreateFrameCandle("Candle_Right", frameObj.transform, candleSprite, new Vector2(215f, 5f));
            }

            // Selection Pill (Placed as sibling inside frameObj, behind buttons, NOT inside VerticalLayoutGroup)
            GameObject pillObj = new GameObject("SelectionPill");
            pillObj.transform.SetParent(frameObj.transform, false);
            RectTransform pillRect = pillObj.AddComponent<RectTransform>();
            pillRect.sizeDelta = new Vector2(340f, 64f);
            Image pillImg = pillObj.AddComponent<Image>();
            pillImg.raycastTarget = false;
            Sprite pillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PILL_HIGHLIGHT_PATH);
            if (pillSprite != null)
            {
                pillImg.sprite = pillSprite;
                pillImg.type = Image.Type.Sliced;
            }
            pillObj.AddComponent<CandleFlameFlicker>();

            // Button Container with VerticalLayoutGroup
            GameObject btnContainer = new GameObject("ButtonsGroup");
            btnContainer.transform.SetParent(frameObj.transform, false);
            RectTransform btnContainerRect = btnContainer.AddComponent<RectTransform>();
            btnContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnContainerRect.pivot = new Vector2(0.5f, 0.5f);
            btnContainerRect.anchoredPosition = new Vector2(0, 0);
            btnContainerRect.sizeDelta = new Vector2(380f, 420f);

            VerticalLayoutGroup layout = btnContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 22f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            // 4 Mockup Buttons (New Game, Continue, Settings, Quit)
            Font gothicFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Button btnNewGame = CreateMockupTextButton("Button_NewGame", btnContainer.transform, "New Game", gothicFont);
            Button btnContinue = CreateMockupTextButton("Button_Continue", btnContainer.transform, "Continue", gothicFont);
            Button btnSettings = CreateMockupTextButton("Button_Settings", btnContainer.transform, "Settings", gothicFont);
            Button btnQuit = CreateMockupTextButton("Button_Quit", btnContainer.transform, "Quit", gothicFont);

            // 8. Rich Ornate Settings Modal Panel (with working Back Button & Custom Gothic Sliders)
            GameObject settingsModal = CreateSettingsModal(canvasObj.transform, gothicFont, manager, out Button backBtn, out Slider volSlider, out Text volLabel, out Toggle fsToggle);

            // 9. Wire Inspector References to MainMenuManager
            SerializedObject so = new SerializedObject(manager);
            so.FindProperty("gameSceneName").stringValue = "GameScene";
            so.FindProperty("btnNewGame").objectReferenceValue = btnNewGame;
            so.FindProperty("btnContinue").objectReferenceValue = btnContinue;
            so.FindProperty("btnSettings").objectReferenceValue = btnSettings;
            so.FindProperty("btnQuit").objectReferenceValue = btnQuit;
            so.FindProperty("selectionPill").objectReferenceValue = pillRect;
            so.FindProperty("menuFrameContainer").objectReferenceValue = frameObj;
            so.FindProperty("settingsModal").objectReferenceValue = settingsModal;
            so.FindProperty("btnBack").objectReferenceValue = backBtn;
            so.FindProperty("volumeSlider").objectReferenceValue = volSlider;
            so.FindProperty("volumeLabel").objectReferenceValue = volLabel;
            so.FindProperty("fullscreenToggle").objectReferenceValue = fsToggle;
            so.FindProperty("sfxSource").objectReferenceValue = sfxSource;
            so.FindProperty("bgmSource").objectReferenceValue = bgmSource;
            so.FindProperty("hoverClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(HOVER_SFX_PATH);
            so.FindProperty("clickClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(CLICK_SFX_PATH);
            so.ApplyModifiedProperties();

            // 10. Save and Register
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[MainMenuBuilder] Enhanced Mockup Scene Built and Saved to {SCENE_PATH}!");
        }

        private static void CreateFrameCandle(string name, Transform parent, Sprite sprite, Vector2 localPos)
        {
            GameObject candleObj = new GameObject(name);
            candleObj.transform.SetParent(parent, false);
            RectTransform rt = candleObj.AddComponent<RectTransform>();
            rt.anchoredPosition = localPos;
            rt.sizeDelta = new Vector2(40f, 68f);

            Image img = candleObj.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;

            candleObj.AddComponent<CandleFlameFlicker>();
        }

        private static Button CreateMockupTextButton(string name, Transform parent, string label, Font font)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(340f, 64f);

            Image targetGraphic = btnObj.AddComponent<Image>();
            targetGraphic.color = new Color(0, 0, 0, 0.005f); // Transparent raycast target
            targetGraphic.raycastTarget = true;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = targetGraphic;
            btn.transition = Selectable.Transition.None;

            GameObject txtObj = new GameObject("Label");
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRt = txtObj.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;

            Text text = txtObj.AddComponent<Text>();
            text.text = label;
            text.font = font;
            text.fontSize = 28;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.94f, 0.90f, 0.82f, 1f);
            text.raycastTarget = false;

            // Subtle dark drop shadow
            Shadow shadow = txtObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.04f, 0.03f, 0.02f, 0.95f);
            shadow.effectDistance = new Vector2(2f, -2f);

            return btn;
        }

        private static GameObject CreateSettingsModal(Transform parent, Font font, MainMenuManager manager, out Button backButton, out Slider volSlider, out Text volLabel, out Toggle fsToggle)
        {
            // Dim background overlay
            GameObject modal = new GameObject("SettingsModal");
            modal.transform.SetParent(parent, false);
            RectTransform modalRect = modal.AddComponent<RectTransform>();
            modalRect.anchorMin = Vector2.zero;
            modalRect.anchorMax = Vector2.one;
            modalRect.offsetMin = Vector2.zero;
            modalRect.offsetMax = Vector2.zero;

            Image overlayImg = modal.AddComponent<Image>();
            overlayImg.color = new Color(0, 0, 0, 0.85f);

            // Rich Gothic Stone Modal Frame
            GameObject frame = new GameObject("Frame");
            frame.transform.SetParent(modal.transform, false);
            RectTransform frameRect = frame.AddComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0.5f, 0.5f);
            frameRect.anchorMax = new Vector2(0.5f, 0.5f);
            frameRect.pivot = new Vector2(0.5f, 0.5f);
            frameRect.anchoredPosition = new Vector2(0, 0);
            frameRect.sizeDelta = new Vector2(640f, 520f);

            Image frameImg = frame.AddComponent<Image>();
            frameImg.raycastTarget = false;
            Sprite settingsFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SETTINGS_FRAME_PATH);
            if (settingsFrameSprite != null)
            {
                frameImg.sprite = settingsFrameSprite;
                frameImg.type = Image.Type.Sliced;
            }
            else
            {
                frameImg.color = new Color(0.15f, 0.13f, 0.12f, 0.98f);
            }

            // Title Text inside Banner Plaque
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(frame.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 208f);
            titleRect.sizeDelta = new Vector2(400f, 45f);
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "SETTINGS";
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.font = font;
            titleText.fontSize = 28;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = new Color(1f, 0.88f, 0.55f, 1f);
            titleText.raycastTarget = false;

            // Volume Section Heading
            GameObject volLblObj = new GameObject("VolumeHeading");
            volLblObj.transform.SetParent(frame.transform, false);
            RectTransform volLblRect = volLblObj.AddComponent<RectTransform>();
            volLblRect.anchoredPosition = new Vector2(0, 90f);
            volLblRect.sizeDelta = new Vector2(400f, 30f);
            Text volLbl = volLblObj.AddComponent<Text>();
            volLbl.text = "MASTER VOLUME";
            volLbl.alignment = TextAnchor.MiddleCenter;
            volLbl.font = font;
            volLbl.fontSize = 20;
            volLbl.color = new Color(0.88f, 0.84f, 0.78f, 1f);
            volLbl.raycastTarget = false;

            // Custom Gothic Stone Volume Slider
            GameObject sliderObj = new GameObject("VolumeSlider");
            sliderObj.transform.SetParent(frame.transform, false);
            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchoredPosition = new Vector2(-40f, 45f);
            sliderRect.sizeDelta = new Vector2(340f, 26f);

            volSlider = sliderObj.AddComponent<Slider>();
            volSlider.minValue = 0f;
            volSlider.maxValue = 1f;
            volSlider.value = 1f;

            // Slider Background Track (Groove)
            GameObject sBg = new GameObject("Background");
            sBg.transform.SetParent(sliderObj.transform, false);
            RectTransform sBgRect = sBg.AddComponent<RectTransform>();
            sBgRect.anchorMin = Vector2.zero;
            sBgRect.anchorMax = Vector2.one;
            sBgRect.offsetMin = Vector2.zero;
            sBgRect.offsetMax = Vector2.zero;
            Image sBgImg = sBg.AddComponent<Image>();
            Sprite trackSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SLIDER_TRACK_PATH);
            if (trackSprite != null) { sBgImg.sprite = trackSprite; sBgImg.type = Image.Type.Sliced; }
            else { sBgImg.color = new Color(0.18f, 0.16f, 0.14f, 1f); }

            // Slider Fill Area (Liquid Molten Flame fill)
            GameObject sFillArea = new GameObject("Fill Area");
            sFillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform sFillAreaRect = sFillArea.AddComponent<RectTransform>();
            sFillAreaRect.anchorMin = Vector2.zero;
            sFillAreaRect.anchorMax = Vector2.one;
            sFillAreaRect.offsetMin = new Vector2(4, 0);
            sFillAreaRect.offsetMax = new Vector2(-4, 0);

            GameObject sFill = new GameObject("Fill");
            sFill.transform.SetParent(sFillArea.transform, false);
            RectTransform sFillRect = sFill.AddComponent<RectTransform>();
            sFillRect.sizeDelta = Vector2.zero;
            Image sFillImg = sFill.AddComponent<Image>();
            Sprite fillSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SLIDER_FILL_PATH);
            if (fillSprite != null) { sFillImg.sprite = fillSprite; sFillImg.type = Image.Type.Sliced; }
            else { sFillImg.color = new Color(0.98f, 0.58f, 0.12f, 1f); }

            // Slider Handle (Fiery Amber Diamond Gem)
            GameObject sHandleArea = new GameObject("Handle Slide Area");
            sHandleArea.transform.SetParent(sliderObj.transform, false);
            RectTransform sHandleAreaRect = sHandleArea.AddComponent<RectTransform>();
            sHandleAreaRect.anchorMin = Vector2.zero;
            sHandleAreaRect.anchorMax = Vector2.one;
            sHandleAreaRect.offsetMin = new Vector2(10, 0);
            sHandleAreaRect.offsetMax = new Vector2(-10, 0);

            GameObject sHandle = new GameObject("Handle");
            sHandle.transform.SetParent(sHandleArea.transform, false);
            RectTransform sHandleRect = sHandle.AddComponent<RectTransform>();
            sHandleRect.sizeDelta = new Vector2(32f, 40f);
            Image sHandleImg = sHandle.AddComponent<Image>();
            Sprite handleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SLIDER_HANDLE_PATH);
            if (handleSprite != null) { sHandleImg.sprite = handleSprite; sHandleImg.preserveAspect = true; }
            else { sHandleImg.color = new Color(1f, 0.88f, 0.55f, 1f); }

            volSlider.fillRect = sFillRect;
            volSlider.handleRect = sHandleRect;
            volSlider.targetGraphic = sHandleImg;

            // Volume Value Text Label
            GameObject valObj = new GameObject("ValueLabel");
            valObj.transform.SetParent(frame.transform, false);
            RectTransform valRect = valObj.AddComponent<RectTransform>();
            valRect.anchoredPosition = new Vector2(175f, 45f);
            valRect.sizeDelta = new Vector2(70f, 30f);
            volLabel = valObj.AddComponent<Text>();
            volLabel.text = "100%";
            volLabel.alignment = TextAnchor.MiddleLeft;
            volLabel.font = font;
            volLabel.fontSize = 20;
            volLabel.color = new Color(1f, 0.85f, 0.5f, 1f);
            volLabel.raycastTarget = false;

            // Fullscreen Toggle (Chiseled stone box with flame check)
            GameObject toggleObj = new GameObject("FullscreenToggle");
            toggleObj.transform.SetParent(frame.transform, false);
            RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
            toggleRect.anchoredPosition = new Vector2(0, -35f);
            toggleRect.sizeDelta = new Vector2(320f, 44f);

            fsToggle = toggleObj.AddComponent<Toggle>();

            GameObject tBox = new GameObject("Background");
            tBox.transform.SetParent(toggleObj.transform, false);
            RectTransform tBoxRect = tBox.AddComponent<RectTransform>();
            tBoxRect.anchoredPosition = new Vector2(-110f, 0);
            tBoxRect.sizeDelta = new Vector2(32f, 32f);
            Image tBoxImg = tBox.AddComponent<Image>();
            Sprite tBoxSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TOGGLE_BOX_PATH);
            if (tBoxSprite != null) { tBoxImg.sprite = tBoxSprite; tBoxImg.type = Image.Type.Sliced; }
            else { tBoxImg.color = new Color(0.2f, 0.18f, 0.16f, 1f); }

            GameObject tCheck = new GameObject("Checkmark");
            tCheck.transform.SetParent(tBox.transform, false);
            RectTransform tCheckRect = tCheck.AddComponent<RectTransform>();
            tCheckRect.anchoredPosition = Vector2.zero;
            tCheckRect.sizeDelta = new Vector2(22f, 22f);
            Image tCheckImg = tCheck.AddComponent<Image>();
            Sprite tCheckSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TOGGLE_CHECK_PATH);
            if (tCheckSprite != null) { tCheckImg.sprite = tCheckSprite; tCheckImg.preserveAspect = true; }
            else { tCheckImg.color = new Color(1f, 0.7f, 0.2f, 1f); }

            GameObject tLabel = new GameObject("Label");
            tLabel.transform.SetParent(toggleObj.transform, false);
            RectTransform tLabelRect = tLabel.AddComponent<RectTransform>();
            tLabelRect.anchoredPosition = new Vector2(25f, 0);
            tLabelRect.sizeDelta = new Vector2(220f, 30f);
            Text tLabelText = tLabel.AddComponent<Text>();
            tLabelText.text = "FULLSCREEN MODE";
            tLabelText.font = font;
            tLabelText.fontSize = 20;
            tLabelText.alignment = TextAnchor.MiddleLeft;
            tLabelText.color = new Color(0.90f, 0.86f, 0.80f, 1f);

            fsToggle.graphic = tCheckImg;
            fsToggle.targetGraphic = tBoxImg;

            // Custom Gothic Stone Slab "BACK" Button with SpriteSwap
            GameObject backObj = new GameObject("Button_Back");
            backObj.transform.SetParent(frame.transform, false);
            RectTransform backRect = backObj.AddComponent<RectTransform>();
            backRect.anchoredPosition = new Vector2(0, -145f);
            backRect.sizeDelta = new Vector2(240f, 64f);

            Image backImg = backObj.AddComponent<Image>();
            Sprite stoneNorm = AssetDatabase.LoadAssetAtPath<Sprite>(BTN_STONE_NORM);
            Sprite stoneHigh = AssetDatabase.LoadAssetAtPath<Sprite>(BTN_STONE_HIGH);
            if (stoneNorm != null)
            {
                backImg.sprite = stoneNorm;
                backImg.type = Image.Type.Sliced;
            }

            backButton = backObj.AddComponent<Button>();
            if (stoneHigh != null)
            {
                backButton.transition = Selectable.Transition.SpriteSwap;
                backButton.spriteState = new SpriteState
                {
                    highlightedSprite = stoneHigh,
                    pressedSprite = stoneHigh,
                    selectedSprite = stoneHigh
                };
            }

            // Persistent listener for Editor saving + runtime safety
            UnityEventTools.AddPersistentListener(backButton.onClick, manager.CloseSettings);

            GameObject backTxtObj = new GameObject("Text");
            backTxtObj.transform.SetParent(backObj.transform, false);
            RectTransform backTxtRect = backTxtObj.AddComponent<RectTransform>();
            backTxtRect.anchorMin = Vector2.zero;
            backTxtRect.anchorMax = Vector2.one;
            backTxtRect.offsetMin = Vector2.zero;
            backTxtRect.offsetMax = Vector2.zero;
            Text backTxt = backTxtObj.AddComponent<Text>();
            backTxt.text = "BACK";
            backTxt.alignment = TextAnchor.MiddleCenter;
            backTxt.font = font;
            backTxt.fontSize = 24;
            backTxt.fontStyle = FontStyle.Bold;
            backTxt.color = new Color(0.98f, 0.94f, 0.88f, 1f);
            backTxt.raycastTarget = false;

            Shadow backShadow = backTxtObj.AddComponent<Shadow>();
            backShadow.effectColor = new Color(0.05f, 0.04f, 0.03f, 0.9f);
            backShadow.effectDistance = new Vector2(2f, -2f);

            modal.SetActive(false);
            return modal;
        }

        private static void UpdateBuildSettings()
        {
            var scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(SCENE_PATH, true),
                new EditorBuildSettingsScene(GAME_SCENE_PATH, true)
            };

            EditorBuildSettings.scenes = scenes;
            Debug.Log("[MainMenuBuilder] Build settings updated with MainMenu and GameScene.");
        }
    }
}
