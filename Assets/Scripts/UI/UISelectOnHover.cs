using Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class UISelectOnHover : MonoBehaviour, IPointerEnterHandler, ISelectHandler, ISubmitHandler, IPointerClickHandler {

    public void OnPointerEnter(PointerEventData eventData) {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(gameObject);
        // звук hover уже сыграет через OnSelect который вызовется автоматически
    }

    public void OnSelect(BaseEventData eventData) {
        UINavigationSounds.Instance?.PlayHover();
    }

    public void OnSubmit(BaseEventData eventData) {
        UINavigationSounds.Instance?.PlayClick();
    }

    public void OnPointerClick(PointerEventData eventData) {
        UINavigationSounds.Instance?.PlayClick();
    }
}