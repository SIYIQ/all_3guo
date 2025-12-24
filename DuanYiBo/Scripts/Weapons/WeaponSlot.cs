using System;
using UnityEngine;

public class WeaponSlot : MonoBehaviour
{
    public event Action<WeaponData> WeaponChanged;

    [SerializeField] private WeaponItem equippedItem;

    public WeaponData EquippedData => equippedItem != null ? equippedItem.data : null;

    public bool Equip(WeaponItem item)
    {
        if (item == null || item.data == null)
        {
            return false;
        }

        equippedItem = item;
        WeaponChanged?.Invoke(item.data);
        return true;
    }

    public void Equip(WeaponData data)
    {
        if (data == null)
        {
            Unequip();
            return;
        }

        equippedItem = new WeaponItem { data = data };
        WeaponChanged?.Invoke(data);
    }

    public void Unequip()
    {
        equippedItem = null;
        WeaponChanged?.Invoke(null);
    }
}
