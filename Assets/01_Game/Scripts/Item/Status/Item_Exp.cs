using UnityEngine;

public class Item_Exp : ItemBase
{
    [SerializeField] private int _value;

    public override void UseItem(PlayerCore playerCore)
    {
        playerCore.ReceiveExp(_value);
    }
}