using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemDrop : MonoBehaviour
{
    [Serializable]
    private class DropItem
    {
        public ItemData data;
        public int value;
    }
    // ---------------------------- SerializeField
    [Header("�h���b�v�������")]
    [SerializeField, Tooltip("")] private DropItem[] _dropItemList;

    [Header("������ԗ�")]
    [SerializeField, Tooltip("��")] private float _forceHeightPower;
    [SerializeField, Tooltip("��")] private float _forceHorizontalPower;

    // ---------------------------- PublicMethod
    /// <summary>
    /// �o���l�𗎂Ƃ�����
    /// </summary>
    public void DropItemProcess()
    {
        foreach (var dropItem in _dropItemList)
        {
            for (int i = 0; i < dropItem.value; i++)
            {
                DropAnimation(dropItem.data.Item);
            }
        }
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// �A�C�e���𐶐����Ĕ�΂�����
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="dropObj"></param>
    private void DropAnimation(ItemBase dropObj)
    {
        var obj = Instantiate(dropObj);
        obj.transform.position = transform.position;

        if (obj.GetComponent<Rigidbody>() == null)
            obj.gameObject.AddComponent<Rigidbody>();

        Rigidbody rb = obj.GetComponent<Rigidbody>();

        // 360�x���璊�I
        float spawnAngle = Random.Range(0, 8) * 45;
        // ���W�A���p�ɕύX
        float radians = spawnAngle * Mathf.Deg2Rad;
        // ����
        Vector3 direction = new Vector3(Mathf.Sin(radians), _forceHeightPower, Mathf.Cos(radians));

        // ��΂�
        rb.AddForce(_forceHorizontalPower * direction, ForceMode.Impulse);
    }
}
