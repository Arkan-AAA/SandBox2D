using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour {
    public TextMeshProUGUI textComponent;
    public float typingSpeed = 0.05f;
    public float delayBeforeStart = 0.5f;

    private string fullText;

    private void Start() {
        fullText = textComponent.text;
        textComponent.text = "";
        StartCoroutine(TypeText());
    }

    private IEnumerator TypeText() {
        yield return new WaitForSeconds(delayBeforeStart);

        for (int i = 0; i <= fullText.Length; i++) {
            textComponent.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}