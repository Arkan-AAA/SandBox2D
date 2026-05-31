using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour {
    [Header("Slots")]
    public Image[] weaponSlots;
    public GameObject[] slotBorders;

    [Header("Weapon Info")]
    public TMP_Text weaponNameText;
    public TMP_Text weaponDamageText;
    public TMP_Text weaponDescriptionText;

    [Header("Close Button")]
    public Button closeButton;

    private WeaponInventory _weaponInventory;
    private GameObject _inventoryPanel;

    private void Start() {
        _weaponInventory = FindAnyObjectByType<WeaponInventory>();
        _inventoryPanel = gameObject;

        if (closeButton != null) {
            closeButton.onClick.AddListener(CloseInventory);
        }

        _inventoryPanel.SetActive(false);
        UpdateUI();
    }

    private void OnEnable() {
        UpdateUI();
    }

    public void UpdateUI() {
        if (_weaponInventory == null) return;

        var weapons = _weaponInventory.GetWeapons();
        int currentIndex = _weaponInventory.GetCurrentIndex();

        for (int i = 0; i < weaponSlots.Length; i++) {
            if (i < weapons.Count && weapons[i] != null) {
                var sr = weapons[i].GetComponentInChildren<SpriteRenderer>();
                if (sr != null && weaponSlots[i] != null) {
                    weaponSlots[i].sprite = sr.sprite;
                    weaponSlots[i].color = Color.white;
                }
            }
            else {
                if (weaponSlots[i] != null) {
                    weaponSlots[i].sprite = null;
                    weaponSlots[i].color = Color.clear;
                }
            }

            if (slotBorders.Length > i && slotBorders[i] != null) {
                slotBorders[i].SetActive(i == currentIndex);
            }
        }

        var current = _weaponInventory.GetCurrentWeapon();
        if (current != null && weaponNameText != null) {
            weaponNameText.text = current.name;
        }
    }

    public void OnWeaponSlotClick(int slotIndex) {
        _weaponInventory?.EquipWeapon(slotIndex);
        UpdateUI();
    }

    public void CloseInventory() {
        _inventoryPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}