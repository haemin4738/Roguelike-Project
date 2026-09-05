using UnityEngine;
using UnityEngine.InputSystem;

public class TownZone : MonoBehaviour
{
    public enum ZoneType { Shop, Ability, CharacterSelect }

    [SerializeField] ZoneType zoneType;
    [SerializeField] GameObject hintObject;

    bool _playerInRange;

    public void Init(ZoneType type) => zoneType = type;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = true;
        hintObject?.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = false;
        hintObject?.SetActive(false);
    }

    void Update()
    {
        if (!_playerInRange || Keyboard.current == null) return;
        if (!Keyboard.current.eKey.wasPressedThisFrame) return;

        if (zoneType == ZoneType.Shop) ShopUI.Instance?.Open();
        else if (zoneType == ZoneType.Ability) AbilityUI.Instance?.Open();
        else CharacterSelectUI.Instance?.Open();
    }
}
