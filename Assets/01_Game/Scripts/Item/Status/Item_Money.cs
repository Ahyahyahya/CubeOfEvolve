using UnityEngine;

public class Item_Money : ItemBase
{
    [SerializeField] private int _value;

    public override void UseItem(PlayerCore playerCore)
    {
        Debug.Log("�������l��");
        playerCore.ReceiveMoney(_value);
    }
}