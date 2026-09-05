using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }
    [SerializeField] WeaponData[] startingWeapons;

    public List<WeaponData> Weapons { get; } = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (startingWeapons == null) return;
        foreach (var w in startingWeapons) Add(w);
    }

    public void Add(WeaponData w) { if (!Weapons.Contains(w)) Weapons.Add(w); }
    public void Remove(WeaponData w) => Weapons.Remove(w);

    public void Swap(WeaponData incoming, WeaponData outgoing)
    {
        var idx = Weapons.IndexOf(incoming);
        if (idx >= 0) Weapons[idx] = outgoing;
        else Add(outgoing);
    }
}
