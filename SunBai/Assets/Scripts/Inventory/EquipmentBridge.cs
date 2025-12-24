using UnityEngine;

public class EquipmentBridge : MonoBehaviour
{
    public static EquipmentBridge Instance;

    [Header("Game (source)")]
    public WeaponSlot gameWeaponSlot;

    [Header("UI (target)")]
    public InventoryUI inventoryUI;
    public EquipSlot weaponEquipSlotUI; // usually inventoryUI.weaponSlot
    public StatusBar attackBar;
    public StatusBar rangeBar;
    public StatusBar cooldownBar;

    [Header("Status bar caps (tweak in inspector)")]
    public float maxAttack = 50f;
    public float maxRange = 5f;
    public float maxCooldown = 2f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        if (gameWeaponSlot != null)
            gameWeaponSlot.WeaponChanged += OnGameWeaponChanged;

        // initial sync
        if (gameWeaponSlot != null)
            OnGameWeaponChanged(gameWeaponSlot.EquippedData);
    }

    private void OnDisable()
    {
        if (gameWeaponSlot != null)
            gameWeaponSlot.WeaponChanged -= OnGameWeaponChanged;
    }

    private void OnGameWeaponChanged(WeaponData data)
    {
        UpdateWeaponSlotVisual(data);
        UpdateStatusBars(data);
    }

    private void UpdateWeaponSlotVisual(WeaponData data)
    {
        if (weaponEquipSlotUI == null) return;

        if (data == null)
        {
            // clear visual
            weaponEquipSlotUI.SetItem(null);
            if (weaponEquipSlotUI.icon != null)
            {
                weaponEquipSlotUI.icon.sprite = weaponEquipSlotUI.emptySprite;
                weaponEquipSlotUI.icon.color = new Color(1f, 1f, 1f, 0.6f);
            }
            return;
        }

        // Try to find a matching ItemData in inventory by icon or name for a cleaner UI binding.
        ItemData matched = null;
        if (inventoryUI != null && inventoryUI.inventoryItems != null)
        {
            foreach (var it in inventoryUI.inventoryItems)
            {
                if (it == null) continue;
                if (it.icon == data.icon || it.itemName == data.displayName)
                {
                    matched = it;
                    break;
                }
            }
        }

        if (matched != null)
        {
            weaponEquipSlotUI.SetItem(matched);
        }
        else
        {
            // No ItemData to bind, just update icon visually
            weaponEquipSlotUI.SetItem(null);
            if (weaponEquipSlotUI.icon != null)
            {
                weaponEquipSlotUI.icon.sprite = data.icon;
                weaponEquipSlotUI.icon.color = Color.white;
            }
        }
    }

    private void UpdateStatusBars(WeaponData data)
    {
        float attack = data != null ? data.attackPower : 0f;
        float range = data != null ? data.attackRange : 0f;
        float cooldown = data != null ? data.attackCooldown : 0f;

        if (attackBar != null)
        {
            attackBar.SetValue(attack, Mathf.Max(1f, maxAttack));
            attackBar.SetLabel(attack.ToString());
        }

        if (rangeBar != null)
        {
            rangeBar.SetValue(range, Mathf.Max(1f, maxRange));
            rangeBar.SetLabel(range.ToString("F1"));
        }

        if (cooldownBar != null)
        {
            // For cooldown display we show numeric cooldown and use an inverted fill (smaller cooldown -> fuller bar)
            float inv = Mathf.Clamp(maxCooldown - cooldown, 0f, maxCooldown);
            cooldownBar.SetValue(inv, Mathf.Max(1f, maxCooldown));
            cooldownBar.SetLabel(cooldown.ToString("F2"));
        }
    }
}


