using UnityEngine;

public class Item_RecoveryHp : ItemBase
{
    [SerializeField] private int _value;

    public override void UseItem(PlayerCore playerCore)
    {
        Debug.Log("�񕜁I�I�I");
        playerCore.RecoveryHp(_value);
    }
}