using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Добавь на корневой объект любой UI-панели.
/// Автоматически вешает UISelectOnHover на все Selectable внутри.
/// </summary>
public class UISelectOnHoverInstaller : MonoBehaviour {
    private void Awake() {
        foreach (var selectable in GetComponentsInChildren<Selectable>(true)) {
            if (selectable.GetComponent<UISelectOnHover>() == null)
                selectable.gameObject.AddComponent<UISelectOnHover>();
        }
    }
}
