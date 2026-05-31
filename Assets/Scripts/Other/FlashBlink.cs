using UnityEngine;

public class FlashBlink : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private Material _blinkMaterial;
    [SerializeField] private float _blinkDuration = 0.2f;

    private float _blinkTimer;
    private Material _defaultMaterial;
    private SpriteRenderer _spriteRenderer;
    private bool _isBlinking;

    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null) {
            _defaultMaterial = _spriteRenderer.material;
        }
        else {
            Debug.LogError($"SpriteRenderer not found on {name}");
        }
    }

    private void Start() {
        SubscribeToDamageable();
    }

    private void SubscribeToDamageable() {
        // Ищем EnemyAI на родителе или текущем объекте
        var enemyAI = GetComponentInParent<EnemyAI>();
        if (enemyAI == null) enemyAI = GetComponent<EnemyAI>();

        if (enemyAI != null) {
            enemyAI.OnFlashBlink += OnDamageTaken;
            Debug.Log($"FlashBlink subscribed to EnemyAI on {enemyAI.name}");
            return;
        }

        // Ищем Player на родителе или текущем объекте
        var player = GetComponentInParent<Player>();
        if (player == null) player = GetComponent<Player>();

        if (player != null) {
            player.OnFlashBlink += OnDamageTaken;
            Debug.Log($"FlashBlink subscribed to Player on {player.name}");
            return;
        }

        Debug.LogWarning($"No EnemyAI or Player found for FlashBlink on {name}");
    }

    private void OnDamageTaken(object sender, System.EventArgs e) {
        Blink();
    }

    private void Update() {
        if (_isBlinking) {
            _blinkTimer -= Time.deltaTime;
            if (_blinkTimer <= 0f) {
                SetDefaultMaterial();
                _isBlinking = false;
            }
        }
    }

    private void Blink() {
        if (_spriteRenderer == null) return;
        if (_blinkMaterial == null) {
            Debug.LogWarning("Blink material not assigned!");
            return;
        }
        SetBlinkingMaterial();
    }

    private void SetBlinkingMaterial() {
        _blinkTimer = _blinkDuration;
        _spriteRenderer.material = _blinkMaterial;
        _isBlinking = true;
    }

    private void SetDefaultMaterial() {
        if (_spriteRenderer != null)
            _spriteRenderer.material = _defaultMaterial;
    }

    public void StopBlinking() {
        SetDefaultMaterial();
        _isBlinking = false;
    }

    private void OnDestroy() {
        // Отписываемся
        var enemyAI = GetComponentInParent<EnemyAI>();
        if (enemyAI == null) enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null) {
            enemyAI.OnFlashBlink -= OnDamageTaken;
            return;
        }

        var player = GetComponentInParent<Player>();
        if (player == null) player = GetComponent<Player>();
        if (player != null) {
            player.OnFlashBlink -= OnDamageTaken;
        }
    }
}