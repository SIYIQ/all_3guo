public interface IWeaponEquippable
{
    bool CanEquip(WeaponData data);
    void EquipWeapon(WeaponData data);
    void UnequipWeapon();
}
