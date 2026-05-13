using System;
using UnityEngine;

namespace Misc
{
    public class FlashBlink : MonoBehaviour
    {
        [SerializeField]
        private MonoBehaviour _damagebleObject;

        [SerializeField]
        private Material _blinkMaterial;

        [SerializeField]
        private float _blinkDuration = 0.2f;

        private float blinkTimer;
        private Material defaultMaterial;
        private SpriteRenderer spriteRenderer;
        private bool isBlinking;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            defaultMaterial = spriteRenderer.material;

            isBlinking = true;
        }

        private void Start()
        {
            if (_damagebleObject is Player)
            {
                (_damagebleObject as Player).OnFlashBlink += DamagebleObject_OnFlashBlink;
            }
        }

        private void DamagebleObject_OnFlashBlink(object sender, EventArgs e)
        {
            SetBlinkingMaterial();
        }

        private void Update()
        {
            if (isBlinking)
            {
                blinkTimer -= Time.deltaTime;
                if (blinkTimer < 0)
                {
                    SetDefaultMaterial();
                }
            }
        }

        private void SetBlinkingMaterial()
        {
            blinkTimer = _blinkDuration;
            spriteRenderer.material = _blinkMaterial;
        }

        private void SetDefaultMaterial()
        {
            spriteRenderer.material = defaultMaterial;
        }

        public void StopBlinking()
        {
            SetDefaultMaterial();
            isBlinking = false;
        }

        private void OnDestroy()
        {
            if (_damagebleObject is Player)
            {
                (_damagebleObject as Player).OnFlashBlink -= DamagebleObject_OnFlashBlink;
            }
        }
    }
}
