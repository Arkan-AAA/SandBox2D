using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class UIInputEnabler : MonoBehaviour {
    public GameObject defaultSelectedButton; // перетащите первую кнопку меню (Play)

    private PlayerInputActions _inputActions;

    private void Awake() {
        _inputActions = new PlayerInputActions();
        // Включаем UI action map, чтобы навигация работала
        _inputActions.UI.Enable();
    }

    private void OnEnable() {
        if (defaultSelectedButton != null)
            EventSystem.current.SetSelectedGameObject(defaultSelectedButton);
    }

    private void OnDisable() {
        _inputActions.UI.Disable();
    }

    // Опционально: обработать Cancel (кнопка B на геймпаде или Escape)
    private void Start() {
        _inputActions.UI.Cancel.performed += OnCancelPerformed;
    }

    private void OnCancelPerformed(InputAction.CallbackContext context) {
        // Если открыта панель настроек – закрыть её
        if (GameManager.Instance != null && GameManager.Instance.optionsPanel != null) {
            var cg = GameManager.Instance.optionsPanel.GetComponent<CanvasGroup>();
            if (cg != null && cg.alpha > 0.5f) {
                GameManager.Instance.ToggleOptions();
                return;
            }
        }
        // Иначе – выйти из игры или показать диалог
        Debug.Log("Cancel pressed – Quit?");
        // Application.Quit();
    }

    private void OnDestroy() {
        _inputActions.UI.Cancel.performed -= OnCancelPerformed;
    }
}