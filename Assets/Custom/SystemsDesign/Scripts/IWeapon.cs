using UnityEngine;

public interface IWeapon
{
    void Shoot();
    void Reload(int ammo);
    int GetCurrentAmmo();
    int GetMaxAmmo();
}
