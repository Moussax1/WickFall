using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WickFall.UI
{
    /// <summary>
    /// Comprehensive Main Menu Manager matching the WICKFALL design reference mockup.
    /// Handles 4-button navigation (New Game, Continue, Settings, Quit), glowing highlight pill,
    /// keyboard/gamepad navigation, atmospheric audio, and settings overlay with reliable Back button.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Scene Configuration")]
        [SerializeField] private string gameSceneName = "GameScene";

        [Header("Menu Buttons")]
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

        private List<Button> menuButtons = new List<Button>();
        private int currentSelectedIndex = 0;

        private void Awake()
        {
            menuButtons.Clear();
            if (btnNewGame != null) menuButtons.Add(btnNewGame);
            if (btnContinue != null) menuButtons.Add(btnContinue);
            if (btnSettings != null) menuButtons.Add(btnSettings);
            if (btnQuit != null) menuButtons.Add(btnQuit);

            // Setup primary button event listeners
            if (btnNewGame != null) btnNewGame.onClick.AddListener(OnNewGameClicked);
            if (btnContinue != null) btnContinue.onClick.AddListener(OnContinueClicked);
            if (btnSettings != null) btnSettings.onClick.AddListener(OpenSettings);
            if (btnQuit != null) btnQuit.onClick.AddListener(QuitGame);

            // Settings Back button setup (defensive runtime binding if persistent listener missing)
            if (btnBack != null)
            {
                bool hasListener = false;
                for (int i = 0; i < btnBack.onClick.GetPersistentEventCount(); i++)
                {
                    if (btnBack.onClick.GetPersistentMethodName(i) == nameof(CloseSettings))
                    {
                        hasListener = true;
                        break;
                    }
                }
                if (!hasListener)
                {
                    btnBack.onClick.AddListener(CloseSettings);
                }
            }

            // Setup hover triggers for each button
            for (int i = 0; i < menuButtons.Count; i++)
            {
                int index = i;
                EventTrigger trigger = menuButtons[i].gameObject.GetComponent<EventTrigger>();
                if (trigger == null) trigger = menuButtons[i].gameObject.AddComponent<EventTrigger>();

                EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                entry.callback.AddListener((data) => { SelectButtonByIndex(index, playSound: true); });
                trigger.triggers.Add(entry);
            }

            // Settings initialization
            if (settingsModal != null) settingsModal.SetActive(false);
            if (menuFrameContainer != null) menuFrameContainer.SetActive(true);

            if (volumeSlider != null)
            {
                volumeSlider.value = AudioListener.volume;
                volumeSlider.onValueChanged.AddListener(SetVolume);
                UpdateVolumeLabel(volumeSlider.value);
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = Screen.fullScreen;
                fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            }
        }

        private void Start()
        {
            // Initial selection on New Game
            StartCoroutine(InitialSelectionCoroutine());

            if (bgmSource != null && !bgmSource.isPlaying)
            {
                bgmSource.loop = true;
                bgmSource.Play();
            }
        }

        private IEnumerator InitialSelectionCoroutine()
        {
            yield return null; // wait 1 frame for layout to settle
            SelectButtonByIndex(0, playSound: false);
        }

        private void Update()
        {
            // When Settings modal is open, Escape closes it
            if (settingsModal != null && settingsModal.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    CloseSettings();
                }
                return;
            }

            // Keyboard / Gamepad Navigation (W/S or Up/Down arrows)
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                int next = (currentSelectedIndex - 1 + menuButtons.Count) % menuButtons.Count;
                SelectButtonByIndex(next, playSound: true);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                int next = (currentSelectedIndex + 1) % menuButtons.Count;
                SelectButtonByIndex(next, playSound: true);
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                if (currentSelectedIndex >= 0 && currentSelectedIndex < menuButtons.Count)
                {
                    menuButtons[currentSelectedIndex].onClick.Invoke();
                }
            }
        }

        public void SelectButtonByIndex(int index, bool playSound)
        {
            if (index < 0 || index >= menuButtons.Count) return;

            currentSelectedIndex = index;
            Button selectedBtn = menuButtons[index];

            // Snap the glowing pill directly behind the selected button
            if (selectionPill != null && selectedBtn != null)
            {
                selectionPill.gameObject.SetActive(true);
                selectionPill.position = selectedBtn.transform.position;
            }

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(selectedBtn.gameObject);
            }

            if (playSound && sfxSource != null && hoverClip != null)
            {
                sfxSource.PlayOneShot(hoverClip);
            }
        }

        private void OnNewGameClicked()
        {
            PlayClickSound();
            Debug.Log("[MainMenu] Starting New Game run...");
            PlayerPrefs.DeleteKey("WickFall_CheckpointX");
            PlayerPrefs.DeleteKey("WickFall_CheckpointY");
            PlayerPrefs.Save();
            SceneManager.LoadScene(gameSceneName);
        }

        private void OnContinueClicked()
        {
            PlayClickSound();
            Debug.Log("[MainMenu] Continuing from checkpoint or starting run...");
            SceneManager.LoadScene(gameSceneName);
        }

        public void OpenSettings()
        {
            PlayClickSound();
            Debug.Log("[MainMenu] Opening Settings Modal...");
            if (settingsModal != null) settingsModal.SetActive(true);
            if (menuFrameContainer != null) menuFrameContainer.SetActive(false);

            // Clear selected GameObject so submit/enter doesn't immediately trigger Back button
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void CloseSettings()
        {
            PlayClickSound();
            Debug.Log("[MainMenu] Closing Settings Modal...");
            if (settingsModal != null) settingsModal.SetActive(false);
            if (menuFrameContainer != null) menuFrameContainer.SetActive(true);
            SelectButtonByIndex(currentSelectedIndex, playSound: false);
        }

        public void QuitGame()
        {
            PlayClickSound();
            Debug.Log("[MainMenu] Quitting application...");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        public void SetVolume(float volume)
        {
            AudioListener.volume = Mathf.Clamp01(volume);
            UpdateVolumeLabel(volume);
        }

        private void UpdateVolumeLabel(float volume)
        {
            if (volumeLabel != null)
            {
                volumeLabel.text = $"{Mathf.RoundToInt(volume * 100f)}%";
            }
        }

        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            Debug.Log($"[MainMenu] Fullscreen set to: {isFullscreen}");
        }

        private void PlayClickSound()
        {
            if (sfxSource != null && clickClip != null)
            {
                sfxSource.PlayOneShot(clickClip);
            }
        }
    }
}
