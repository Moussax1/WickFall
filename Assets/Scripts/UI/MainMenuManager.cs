using UnityEngine;
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

        private void Awake()
        {
            if (btnNewGame != null) btnNewGame.onClick.AddListener(OnNewGameClicked);
            if (btnContinue != null) btnContinue.onClick.AddListener(OnContinueClicked);
            if (btnSettings != null) btnSettings.onClick.AddListener(OpenSettings);
            if (btnQuit != null) btnQuit.onClick.AddListener(QuitGame);
            if (btnBack != null) btnBack.onClick.AddListener(CloseSettings);

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
        }

        public void CloseSettings()
        {
            PlayClickSound();
            if (settingsModal != null) settingsModal.SetActive(false);
            if (menuFrameContainer != null) menuFrameContainer.SetActive(true);
        }

        private void PlayClickSound()
        {
            if (sfxSource != null && clickClip != null) sfxSource.PlayOneShot(clickClip);
        }
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
