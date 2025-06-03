using UnityEngine;

public abstract class ItemBase : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField] private ItemData _itemData;

    // ---------------------------- Property
    public ItemData Data => _itemData;

    // ---------------------------- UnityMassage
    private void Start()
    {
        Initialize();
    }

    // ---------------------------- virtual
    /// <summary>
    /// ������
    /// </summary>
    public virtual void Initialize()
    {
    }

    /// <summary>
    /// �A�C�e�����g������
    /// </summary>
    public abstract void UseItem(PlayerCore playerCore);
}
