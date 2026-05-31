using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponSlotUI : MonoBehaviour {
    [Header("Slots")]
    public Image[] weaponIcons;
    public GameObject[] weaponBorders;

    [Header("Weapon Info (опционально)")]
    public TMP_Text weaponNameText;
    public TMP_Text weaponDamageText;

    private WeaponInventory _weaponInventory;

    private void Start() {
        _weaponInventory = GetComponentInParent<WeaponInventory>();
        if (_weaponInventory == null) {
            _weaponInventory = FindAnyObjectByType<WeaponInventory>();
        }
        UpdateUI();
    }

    private void Update() {
        UpdateUI();
    }

    public void UpdateUI() {
        if (_weaponInventory == null) return;

        var weapons = _weaponInventory.GetWeapons();
        int currentIndex = _weaponInventory.GetCurrentIndex();

        for (int i = 0; i < weaponIcons.Length; i++) {
            if (i < weapons.Count && weapons[i] != null) {
                // Ищем SpriteRenderer на оружии
                var sr = weapons[i].GetComponentInChildren<SpriteRenderer>();
                if (sr != null && weaponIcons[i] != null) {
                    weaponIcons[i].sprite = sr.sprite;
                    weaponIcons[i].color = Color.white;
                }
            }
            else {
                if (weaponIcons[i] != null) {
                    weaponIcons[i].sprite = null;
                    weaponIcons[i].color = Color.clear;
                }
            }

            // Рамка активного оружия
            if (weaponBorders.Length > i && weaponBorders[i] != null) {
                weaponBorders[i].SetActive(i == currentIndex);
            }
        }

        // Информация о текущем оружии
        var current = _weaponInventory.GetCurrentWeapon();
        if (current != null && weaponNameText != null) {
            weaponNameText.text = current.name;
        }
    }
}