using App.BaseSystem.DataStores.ScriptableObjects;
using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObjects/Data/ItemData")]
public class ItemData : BaseData
{
    // ----- Enum
    /// <summary>
    /// ���W���[���̎�ނ��`����񋓌^�ł��B
    /// </summary>
    public enum ItemType
    {
        [InspectorName("�Ȃ�")]
        None = 0,
        [InspectorName("�g������")]
        Use,
        [InspectorName("�X�e�[�^�X")]
        Status,
    }
    public ItemBase Item;
    public ItemType Type;
}
