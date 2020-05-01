public class PlayerItem
{
    public ItemType Item { get; private set; }

    public PlayerItem(ItemType item)
    {
        Item = item;
    }
}
