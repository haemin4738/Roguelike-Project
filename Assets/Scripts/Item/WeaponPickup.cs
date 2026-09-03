using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponPickup : MonoBehaviour
{
    public WeaponData data;

    bool _playerInRange;
    GameObject _playerObj;

    void Update()
    {
        if (!_playerInRange) return;
        if (Keyboard.current.eKey.wasPressedThisFrame) Pickup();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = true;
        _playerObj = other.gameObject;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = false;
    }

    void Pickup()
    {
        var combat = _playerObj.GetComponent<PlayerCombat>();
        if (combat == null) return;
        combat.EquipWeapon(data);
        EventBus.Publish(new ItemPickedEvent { ItemName = data.weaponName });
        Destroy(gameObject);
    }
}
