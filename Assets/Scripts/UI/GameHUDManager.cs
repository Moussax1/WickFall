using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WickFall.Environment;
using WickFall.Player;

namespace WickFall.UI
{
    /// <summary>
    /// Manages the top-level HUD, 8-stage candle health display, objective tracker,
    /// and Victory / Game Over modal overlays.
    /// </summary>
    public class GameHUDManager : MonoBehaviour
    {
        [Header("Player Reference")]
        [SerializeField] private PlayerWax _playerWax;

        [Header("Health & Candle UI")]
        [SerializeField] private Image _candleIcon;
        [SerializeField] private Sprite[] _candleStages; // 1 to 8 (full to melted)
        [SerializeField] private Text _waxPercentText;
        [SerializeField] private Slider _waxSlider;
        [SerializeField] private Image _dangerVignette;

        [Header("Objective & Tracker")]
        [SerializeField] private Text _objectiveText;
        [SerializeField] private Text _statsTrackerText;

        [Header("Modals")]
        [SerializeField] private GameObject _victoryModal;
        [SerializeField] private GameObject _gameOverModal;
        [SerializeField] private Text _victorySummaryText;

        [Header("Modal Buttons")]
        [SerializeField] private Button _btnVictoryReplay;
        [SerializeField] private Button _btnVictoryMenu;
        [SerializeField] private Button _btnGameOverRetry;
        [SerializeField] private Button _btnGameOverMenu;

        private int _enemiesDefeatedCount;
        private float _initialMaxWax;
        private bool _isGameEnded;

        private void Awake()
        {
            if (_playerWax == null)
            {
                _playerWax = Object.FindFirstObjectByType<PlayerWax>();
            }

            if (_victoryModal != null) _victoryModal.SetActive(false);
            if (_gameOverModal != null) _gameOverModal.SetActive(false);

            // Wire button listeners
            if (_btnVictoryReplay != null) _btnVictoryReplay.onClick.AddListener(RestartGame);
            if (_btnVictoryMenu != null) _btnVictoryMenu.onClick.AddListener(LoadMainMenu);
            if (_btnGameOverRetry != null) _btnGameOverRetry.onClick.AddListener(RestartGame);
            if (_btnGameOverMenu != null) _btnGameOverMenu.onClick.AddListener(LoadMainMenu);
        }

        private void OnEnable()
        {
            if (_playerWax != null)
            {
                _playerWax.OnWaxChanged += UpdateWaxDisplay;
            }

            PlayerDeathSequence.OnDeathSequenceFinished += ShowGameOver;
            Brazier.OnBrazierKindled += ShowVictory;
        }

        private void OnDisable()
        {
            if (_playerWax != null)
            {
                _playerWax.OnWaxChanged -= UpdateWaxDisplay;
            }

            PlayerDeathSequence.OnDeathSequenceFinished -= ShowGameOver;
            Brazier.OnBrazierKindled -= ShowVictory;
        }

        private void Start()
        {
            if (_playerWax != null)
            {
                _initialMaxWax = _playerWax.MaxWax;
                UpdateWaxDisplay(_playerWax.CurrentWax, _playerWax.MaxWax);
            }

            if (_objectiveText != null)
            {
                _objectiveText.text = "Objective: Traverse the dark catacombs and kindle the sacred Brazier.";
            }

            UpdateStatsTracker();
        }

        private void Update()
        {
            if (_dangerVignette == null || _isGameEnded) return;

            float ratio = _playerWax != null ? _playerWax.WaxRatio : 1f;
            if (ratio < 0.25f && ratio > 0f)
            {
                float pulse = (Mathf.Sin(Time.time * 5.5f) + 1f) * 0.5f;
                _dangerVignette.color = new Color(0.85f, 0.12f, 0.12f, pulse * 0.38f);
            }
            else
            {
                _dangerVignette.color = Color.clear;
            }
        }

        public void RegisterEnemyDefeated()
        {
            _enemiesDefeatedCount++;
            UpdateStatsTracker();
        }

        private void UpdateStatsTracker()
        {
            if (_statsTrackerText != null)
            {
                _statsTrackerText.text = $"Enemies Cleansed: {_enemiesDefeatedCount}";
            }
        }

        private void UpdateWaxDisplay(float current, float max)
        {
            if (max <= 0f) return;

            float ratio = Mathf.Clamp01(current / max);

            if (_waxPercentText != null)
            {
                _waxPercentText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            }

            if (_waxSlider != null)
            {
                _waxSlider.value = ratio;
            }

            // Update 8-stage candle graphic (Stage 0 = 100%, Stage 7 = 0%)
            if (_candleStages != null && _candleStages.Length > 0 && _candleIcon != null)
            {
                int stageIndex = Mathf.FloorToInt((1f - ratio) * _candleStages.Length);
                stageIndex = Mathf.Clamp(stageIndex, 0, _candleStages.Length - 1);

                if (_candleStages[stageIndex] != null)
                {
                    _candleIcon.sprite = _candleStages[stageIndex];
                }
            }
        }

        private void ShowVictory()
        {
            if (_isGameEnded) return;
            _isGameEnded = true;

            StartCoroutine(VictorySequenceRoutine());
        }

        private IEnumerator VictorySequenceRoutine()
        {
            yield return new WaitForSeconds(1.2f);

            if (_victoryModal != null)
            {
                _victoryModal.SetActive(true);
            }

            if (_victorySummaryText != null && _playerWax != null)
            {
                _victorySummaryText.text =
                    $"The ancient flame is rekindled!\n\n" +
                    $"• Wax Preserved: {Mathf.CeilToInt(_playerWax.CurrentWax)} / {Mathf.CeilToInt(_playerWax.MaxWax)}\n" +
                    $"• Shadow Thralls Banished: {_enemiesDefeatedCount}\n" +
                    $"• Darkness Cleansed: 100%";
            }
        }

        public void ShowGameOver()
        {
            if (_isGameEnded) return;
            _isGameEnded = true;

            StartCoroutine(GameOverSequenceRoutine());
        }

        private IEnumerator GameOverSequenceRoutine()
        {
            yield return new WaitForSeconds(0.4f);

            if (_gameOverModal != null)
            {
                _gameOverModal.SetActive(true);
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
