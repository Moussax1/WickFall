using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WickFall.Environment;
using WickFall.Player;

namespace WickFall.UI
{
    public class GameHUDManager : MonoBehaviour
    {
        [SerializeField] private PlayerWax _playerWax;

        [Header("Health UI")]
        [SerializeField] private Image _candleIcon;
        [SerializeField] private Sprite[] _candleStages;
        [SerializeField] private Text _waxPercentText;
        [SerializeField] private Slider _waxSlider;
        [SerializeField] private Image _dangerVignette;

        [Header("Objectives & Stats Tracker")]
        [SerializeField] private Text _objectiveText;
        [SerializeField] private Text _statsTrackerText;

        [Header("Modals")]
        [SerializeField] private GameObject _victoryModal;
        [SerializeField] private GameObject _gameOverModal;
        [SerializeField] private Text _victorySummaryText;

        [Header("Buttons")]
        [SerializeField] private Button _btnVictoryReplay;
        [SerializeField] private Button _btnVictoryMenu;
        [SerializeField] private Button _btnGameOverRetry;
        [SerializeField] private Button _btnGameOverMenu;

        private int _enemiesDefeatedCount;
        private bool _isGameEnded;

        private void Awake()
        {
            if (_playerWax == null) _playerWax = Object.FindFirstObjectByType<PlayerWax>();
            if (_victoryModal != null) _victoryModal.SetActive(false);
            if (_gameOverModal != null) _gameOverModal.SetActive(false);

            if (_btnVictoryReplay != null) _btnVictoryReplay.onClick.AddListener(RestartGame);
            if (_btnVictoryMenu != null) _btnVictoryMenu.onClick.AddListener(LoadMainMenu);
            if (_btnGameOverRetry != null) _btnGameOverRetry.onClick.AddListener(RestartGame);
            if (_btnGameOverMenu != null) _btnGameOverMenu.onClick.AddListener(LoadMainMenu);
        }

        private void OnEnable()
        {
            if (_playerWax != null) _playerWax.OnWaxChanged += UpdateWaxDisplay;
            PlayerDeathSequence.OnDeathSequenceFinished += ShowGameOver;
            Brazier.OnBrazierKindled += ShowVictory;
        }

        private void OnDisable()
        {
            if (_playerWax != null) _playerWax.OnWaxChanged -= UpdateWaxDisplay;
            PlayerDeathSequence.OnDeathSequenceFinished -= ShowGameOver;
            Brazier.OnBrazierKindled -= ShowVictory;
        }

        private void Start()
        {
            if (_playerWax != null) UpdateWaxDisplay(_playerWax.CurrentWax, _playerWax.MaxWax);
            if (_objectiveText != null) _objectiveText.text = "Traverse the dark catacombs and kindle the sacred Brazier.";
            UpdateStatsTracker();
        }

        public void RegisterEnemyDefeated()
        {
            _enemiesDefeatedCount++;
            UpdateStatsTracker();
        }

        private void UpdateStatsTracker()
        {
            if (_statsTrackerText != null)
                _statsTrackerText.text = $"Shadow Thralls Cleansed: {_enemiesDefeatedCount}";
        }

        public void UpdateWaxDisplay(float current, float max)
        {
            float ratio = Mathf.Clamp01(current / Mathf.Max(1f, max));
            if (_waxSlider != null) _waxSlider.value = ratio;
            if (_waxPercentText != null) _waxPercentText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";

            if (_candleStages != null && _candleStages.Length > 0 && _candleIcon != null)
            {
                int index = Mathf.Clamp(Mathf.FloorToInt((1f - ratio) * _candleStages.Length), 0, _candleStages.Length - 1);
                _candleIcon.sprite = _candleStages[index];
            }

            if (_dangerVignette != null)
            {
                Color c = _dangerVignette.color;
                c.a = ratio < 0.25f ? (1f - ratio / 0.25f) * 0.5f : 0f;
                _dangerVignette.color = c;
            }
        }

        public void ShowVictory()
        {
            if (_isGameEnded) return;
            _isGameEnded = true;

            if (_victorySummaryText != null && _playerWax != null)
            {
                _victorySummaryText.text =
                    $"The ancient flame is rekindled!\n\n" +
                    $"• Wax Preserved: {Mathf.CeilToInt(_playerWax.CurrentWax)} / {Mathf.CeilToInt(_playerWax.MaxWax)}\n" +
                    $"• Shadow Thralls Banished: {_enemiesDefeatedCount}\n" +
                    $"• Darkness Cleansed: 100%";
            }

            if (_victoryModal != null) _victoryModal.SetActive(true);
        }

        public void ShowGameOver()
        {
            if (_isGameEnded) return;
            _isGameEnded = true;
            if (_gameOverModal != null) _gameOverModal.SetActive(true);
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
