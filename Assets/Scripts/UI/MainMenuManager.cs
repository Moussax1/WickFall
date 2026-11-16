using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WickFall.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private string gameSceneName = "GameScene";

        [Header("Buttons")]
        [SerializeField] private Button btnNewGame;
        [SerializeField] private Button btnContinue;
        [SerializeField] private Button btnSettings;
        [SerializeField] private Button btnQuit;
        [SerializeField] private RectTransform selectionPill;

        [Header("Panels")]
        [SerializeField] private GameObject menuFrameContainer;
        [SerializeField] private GameObject settingsModal;

        [Header("Settings Controls")]
        [SerializeField] private Button btnBack;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Text volumeLabel;
        [SerializeField] private Toggle fullscreenToggle;

        [Header("Audio")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioClip hoverClip;
        [SerializeField] private AudioClip clickClip;

        private float _targetPillY;
        private bool _isPillTracking;
        private int _currentHoverIndex = -1;
        private readonly List<Button> _buttons = new List<Button>();
        private readonly List<Text> _labels = new List<Text>();
        private readonly Color _normalColor = new Color(0.94f, 0.90f, 0.82f, 1f);
        private readonly Color _hoverColor = new Color(1.0f, 0.82f, 0.23f, 1f); // Warm radiant gold

        private void Awake()
        {
            EnsureCursorActive();

            RegisterButton(btnNewGame, OnNewGameClicked);
            RegisterButton(btnContinue, OnContinueClicked);
            RegisterButton(btnSettings, OpenSettings);
            RegisterButton(btnQuit, QuitGame);

            if (btnBack != null)
            {
                btnBack.onClick.AddListener(CloseSettings);
                var backLabel = btnBack.GetComponentInChildren<Text>();
                if (backLabel != null) backLabel.raycastTarget = false;
            }

            if (volumeSlider != null)
            {
                volumeSlider.value = AudioListener.volume;
                volumeSlider.onValueChanged.AddListener(v => {
                    AudioListener.volume = v;
                    if (volumeLabel != null) volumeLabel.text = $"{Mathf.RoundToInt(v * 100f)}%";
                });
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = Screen.fullScreen;
                fullscreenToggle.onValueChanged.AddListener(fs => Screen.fullScreen = fs);
            }

            if (settingsModal != null) settingsModal.SetActive(false);
            if (menuFrameContainer != null) menuFrameContainer.SetActive(true);

            // Ensure selection pill does not block mouse raycasts
            if (selectionPill != null)
            {
                var pillImg = selectionPill.GetComponent<Image>();
                if (pillImg != null) pillImg.raycastTarget = false;
            }
        }

        private void Start()
        {
            EnsureCursorActive();

            // Set initial selection and snap selection pill to first option
            if (btnNewGame != null)
            {
                OnButtonHovered(btnNewGame, 0, playSound: false);
                if (selectionPill != null)
                {
                    Vector2 p = selectionPill.anchoredPosition;
                    p.y = _targetPillY;
                    selectionPill.anchoredPosition = p;
                }

                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(btnNewGame.gameObject);
                }
            }
        }

        private void Update()
        {
            EnsureCursorActive();

            // Smoothly glide the selection pill to the hovered button
            if (selectionPill != null && _isPillTracking)
            {
                Vector2 cur = selectionPill.anchoredPosition;
                float newY = Mathf.Lerp(cur.y, _targetPillY, Time.unscaledDeltaTime * 20f);
                selectionPill.anchoredPosition = new Vector2(cur.x, newY);
            }

            // Keyboard/Gamepad: escape closes settings
            if (Input.GetKeyDown(KeyCode.Escape) && settingsModal != null && settingsModal.activeSelf)
            {
                CloseSettings();
            }
        }

        private void EnsureCursorActive()
        {
            if (!Cursor.visible) Cursor.visible = true;
            if (Cursor.lockState != CursorLockMode.None) Cursor.lockState = CursorLockMode.None;
            if (Time.timeScale <= 0f) Time.timeScale = 1f;
        }

        private void RegisterButton(Button btn, UnityEngine.Events.UnityAction onClick)
        {
            if (btn == null) return;
            int idx = _buttons.Count;
            _buttons.Add(btn);
            btn.onClick.AddListener(onClick);

            // Ensure child text does not block raycasts
            Text label = btn.GetComponentInChildren<Text>();
            _labels.Add(label);
            if (label != null) label.raycastTarget = false;

            // Ensure button itself is a clean raycast target
            Image img = btn.GetComponent<Image>();
            if (img != null) img.raycastTarget = true;

            // Add hover trigger
            var trigger = btn.gameObject.GetComponent<MenuHoverTrigger>();
            if (trigger == null) trigger = btn.gameObject.AddComponent<MenuHoverTrigger>();
            trigger.Init(this, btn, idx);
        }

        public void OnButtonHovered(Button btn, int index, bool playSound = true)
        {
            if (btn == null) return;

            // Calculate pixel-perfect Y coordinate relative to menu frame
            if (menuFrameContainer != null && selectionPill != null)
            {
                Vector3 localInFrame = menuFrameContainer.transform.InverseTransformPoint(btn.transform.position);
                _targetPillY = localInFrame.y;
                _isPillTracking = true;
            }

            // Play hover sound only when transitioning to a new button
            if (playSound && _currentHoverIndex != index)
            {
                PlayHoverSound();
            }
            _currentHoverIndex = index;

            // Update text label highlight colors
            for (int i = 0; i < _labels.Count; i++)
            {
                if (_labels[i] != null)
                {
                    _labels[i].color = (i == index) ? _hoverColor : _normalColor;
                }
            }
        }

        public void OnButtonUnhovered(int index)
        {
            if (index >= 0 && index < _labels.Count && _labels[index] != null)
            {
                // Only revert if not currently selected
                if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject != _buttons[index].gameObject)
                {
                    _labels[index].color = _normalColor;
                }
            }
        }

        public void OnNewGameClicked()
        {
            PlayClickSound();
            SceneManager.LoadScene(gameSceneName);
        }

        public void OnContinueClicked()
        {
            PlayClickSound();
            SceneManager.LoadScene(gameSceneName);
        }

        public void OpenSettings()
        {
            PlayClickSound();
            if (settingsModal != null) settingsModal.SetActive(true);
            if (menuFrameContainer != null) menuFrameContainer.SetActive(false);

            if (btnBack != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(btnBack.gameObject);
            }
        }

        public void CloseSettings()
        {
            PlayClickSound();
            if (settingsModal != null) settingsModal.SetActive(false);
            if (menuFrameContainer != null) menuFrameContainer.SetActive(true);

            if (btnSettings != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(btnSettings.gameObject);
                OnButtonHovered(btnSettings, _buttons.IndexOf(btnSettings), playSound: false);
            }
        }

        private void PlayHoverSound()
        {
            if (sfxSource != null && hoverClip != null) sfxSource.PlayOneShot(hoverClip);
        }

        private void PlayClickSound()
        {
            if (sfxSource != null && clickClip != null) sfxSource.PlayOneShot(clickClip);
        }

        public void QuitGame()
        {
            PlayClickSound();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    /// <summary>
    /// Lightweight UI pointer and navigation trigger forwarding hover and selection events to MainMenuManager.
    /// </summary>
    public class MenuHoverTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        private MainMenuManager _manager;
        private Button _button;
        private int _index;

        public void Init(MainMenuManager manager, Button button, int index)
        {
            _manager = manager;
            _button = button;
            _index = index;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_manager != null) _manager.OnButtonHovered(_button, _index, playSound: true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_manager != null) _manager.OnButtonUnhovered(_index);
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (_manager != null) _manager.OnButtonHovered(_button, _index, playSound: true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (_manager != null) _manager.OnButtonUnhovered(_index);
        }
    }
}
