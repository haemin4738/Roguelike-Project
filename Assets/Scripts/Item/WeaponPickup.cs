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
        if (Keyboard.current.fKey.wasPressedThisFrame) Pickup();
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
        if (PlayerInventory.Instance == null) return;
        PlayerInventory.Instance.Add(data);
        EventBus.Publish(new ItemPickedEvent { ItemName = data.weaponName });
        Destroy(gameObject);
    }
}
