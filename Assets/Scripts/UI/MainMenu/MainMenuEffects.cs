using UnityEngine;

public class MainMenuEffects : MonoBehaviour {
    [Header("Particles")]
    public ParticleSystem ambientParticles;
    public ParticleSystem buttonGlow;

    [Header("Camera Movement")]
    public float cameraSpeed = 0.5f;
    public float cameraAmplitude = 0.5f;

    private Vector3 cameraStartPos;
    private float time;

    private void Start() {
        cameraStartPos = Camera.main.transform.position;

        if (ambientParticles != null) {
            ambientParticles.Play();
        }
    }

    private void Update() {
        // Плавное движение камеры
        time += Time.deltaTime * cameraSpeed;
        float x = Mathf.Sin(time) * cameraAmplitude;
        Camera.main.transform.position = cameraStartPos + new Vector3(x, 0, 0);
    }

    public void OnButtonHover() {
        if (buttonGlow != null) {
            buttonGlow.Play();
        }
    }
}