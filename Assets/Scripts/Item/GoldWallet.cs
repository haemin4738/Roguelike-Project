public static class GoldWallet
{
    public static int Gold { get; private set; }

    public static void Add(int amount)
    {
        Gold += amount;
        EventBus.Publish(new GoldChangedEvent { Total = Gold });
    }

    public static bool Spend(int amount)
    {
        if (Gold < amount) return false;
        Gold -= amount;
        EventBus.Publish(new GoldChangedEvent { Total = Gold });
        return true;
    }

    public static void Reset()
    {
        Gold = 0;
        EventBus.Publish(new GoldChangedEvent { Total = 0 });
    }
}
