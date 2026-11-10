using System.Collections.Generic;
using UnityEngine;

namespace WickFall.Player
{
    /// <summary>
    /// Spawns physical melting wax decals onto the catacomb stones.
    /// When walking: drops alternating wax footstep droplets that fade over time.
    /// When standing: expands a molten "lake of wax" under the Golem that grows
    /// larger the longer the player remains stationary, then sets into hard wax.
    /// </summary>
    [RequireComponent(typeof(PlayerWax))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerWaxTrail : MonoBehaviour
    {
        [Header("Footstep Drops (Walking)")]
        [SerializeField] private Sprite _footstepSprite;
        [SerializeField] private float _stepDistanceThreshold = 0.38f;
        [SerializeField] private float _footstepLifetime = 5.0f;
        [SerializeField] private float _footstepOffset = 0.14f;

        [Header("Molten Wax Lake (Stationary)")]
        [SerializeField] private Sprite _puddleSprite;
        [SerializeField] private float _idleThreshold = 0.35f;
        [SerializeField] private float _maxPuddleScale = 1.35f;
        [SerializeField] private float _puddleGrowthSpeed = 0.28f;
        [SerializeField] private float _puddleLifetime = 12.0f;

        [Header("Decal Pool Limits")]
        [SerializeField] private int _maxActiveDecals = 30;

        private Rigidbody2D _rb;
        private PlayerWax _playerWax;
        private Vector2 _lastStepPosition;
        private Vector2 _lastFramePos;
        private bool _stepSide; // Alternates left/right foot
        private float _idleTimer;
        private GameObject _currentActivePuddle;
        private SpriteRenderer _currentPuddleSR;
        private float _currentPuddleScale = 0.3f;

        private readonly Queue<GameObject> _activeDecals = new Queue<GameObject>();
        private Transform _decalContainer;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _playerWax = GetComponent<PlayerWax>();
            _lastStepPosition = transform.position;
            _lastFramePos = transform.position;

            GameObject container = new GameObject("Wax_Trail_Decals");
            _decalContainer = container.transform;
        }

        private void OnDestroy()
        {
            if (_decalContainer != null)
            {
                Destroy(_decalContainer.gameObject);
            }
        }

        private void Update()
        {
            if (_playerWax != null && _playerWax.IsDead)
            {
                DetachCurrentPuddle();
                return;
            }

            float deltaPos = Vector2.Distance(transform.position, _lastFramePos);
            float computedSpeed = Time.deltaTime > 0f ? (deltaPos / Time.deltaTime) : 0f;
            float speed = Mathf.Max(_rb != null ? _rb.linearVelocity.magnitude : 0f, computedSpeed);
            _lastFramePos = transform.position;

            if (speed > 0.15f)
            {
                // Player is walking -> drop footsteps
                _idleTimer = 0f;
                DetachCurrentPuddle();

                float distMoved = Vector2.Distance(transform.position, _lastStepPosition);
                if (distMoved >= _stepDistanceThreshold)
                {
                    DropFootstep();
                    _lastStepPosition = transform.position;
                }
            }
            else
            {
                // Player is standing still -> grow molten lake of wax
                _idleTimer += Time.deltaTime;
                if (_idleTimer >= _idleThreshold)
                {
                    UpdateMoltenWaxLake();
                }
            }
        }

        private void DropFootstep()
        {
            if (_footstepSprite == null) return;

            // Calculate lateral offset perpendicular to movement
            Vector2 moveDir = ((Vector2)transform.position - _lastStepPosition).normalized;
            if (moveDir.sqrMagnitude < 0.01f) moveDir = Vector2.right;
            Vector2 lateral = new Vector2(-moveDir.y, moveDir.x) * (_stepSide ? _footstepOffset : -_footstepOffset);
            _stepSide = !_stepSide;

            Vector3 stepPos = (Vector3)( (Vector2)transform.position + lateral + new Vector2(0f, -0.2f) );
            stepPos.z = 0.05f; // Above floor tiles

            GameObject stepObj = new GameObject("WaxFootstep");
            stepObj.transform.position = stepPos;
            stepObj.transform.SetParent(_decalContainer);

            // Subtle random scale & rotation for organic feel
            float rot = Random.Range(-25f, 25f);
            stepObj.transform.rotation = Quaternion.Euler(0f, 0f, rot);
            float scale = Random.Range(1.35f, 1.7f);
            stepObj.transform.localScale = new Vector3(scale, scale, 1f);

            var sr = stepObj.AddComponent<SpriteRenderer>();
            sr.sprite = _footstepSprite;
            sr.sortingOrder = 3; // Above floor (0), below player (5)
            sr.color = new Color(1.0f, 0.94f, 0.78f, 0.96f);

            RegisterDecal(stepObj);
            StartCoroutine(FadeAndDestroy(stepObj, sr, _footstepLifetime));
        }

        private void UpdateMoltenWaxLake()
        {
            if (_puddleSprite == null) return;

            if (_currentActivePuddle == null)
            {
                // Spawn new molten puddle under feet
                _currentActivePuddle = new GameObject("MoltenWaxLake");
                Vector3 puddlePos = transform.position + new Vector3(0f, -0.25f, 0.04f);
                _currentActivePuddle.transform.position = puddlePos;
                _currentActivePuddle.transform.SetParent(_decalContainer);

                _currentPuddleSR = _currentActivePuddle.AddComponent<SpriteRenderer>();
                _currentPuddleSR.sprite = _puddleSprite;
                _currentPuddleSR.sortingOrder = 2; // Floor decal
                _currentPuddleSR.color = new Color(0.95f, 0.88f, 0.72f, 0.95f);

                _currentPuddleScale = 0.35f;
                _currentActivePuddle.transform.localScale = new Vector3(_currentPuddleScale, _currentPuddleScale, 1f);
                RegisterDecal(_currentActivePuddle);
            }

            // Smoothly expand puddle scale while standing
            if (_currentPuddleScale < _maxPuddleScale)
            {
                _currentPuddleScale += Time.deltaTime * _puddleGrowthSpeed;
                _currentPuddleScale = Mathf.Min(_currentPuddleScale, _maxPuddleScale);
            }

            // Subtle fluid breathing pulsation
            float pulse = 1f + (Mathf.Sin(Time.time * 4f) * 0.03f);
            _currentActivePuddle.transform.localScale = new Vector3(_currentPuddleScale * pulse, _currentPuddleScale * 0.72f * pulse, 1f);
        }

        private void DetachCurrentPuddle()
        {
            if (_currentActivePuddle != null)
            {
                // Puddle solidifies in place on stone floor and fades out over time
                if (_currentPuddleSR != null)
                {
                    StartCoroutine(FadeAndDestroy(_currentActivePuddle, _currentPuddleSR, _puddleLifetime));
                }
                _currentActivePuddle = null;
                _currentPuddleSR = null;
                _currentPuddleScale = 0.35f;
            }
        }

        private void RegisterDecal(GameObject decal)
        {
            _activeDecals.Enqueue(decal);
            if (_activeDecals.Count > _maxActiveDecals)
            {
                var oldest = _activeDecals.Dequeue();
                if (oldest != null)
                {
                    Destroy(oldest);
                }
            }
        }

        private System.Collections.IEnumerator FadeAndDestroy(GameObject target, SpriteRenderer sr, float lifetime)
        {
            float elapsed = 0f;
            Color startColor = sr.color;

            // Wait for duration before fading
            float fadeStart = lifetime * 0.6f;
            float fadeDuration = lifetime - fadeStart;

            while (elapsed < lifetime)
            {
                if (target == null || sr == null) yield break;

                elapsed += Time.deltaTime;
                if (elapsed >= fadeStart)
                {
                    float t = (elapsed - fadeStart) / fadeDuration;
                    sr.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0f, t));
                }
                yield return null;
            }

            if (target != null)
            {
                Destroy(target);
            }
        }
    }
}
