using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemDrop : MonoBehaviour
{
    [Serializable]
    private class DropItem
    {
        public int value;
        public GameObject obj;
    }
    // ---------------------------- SerializeField
    [Header("�h���b�v�������")]
    [SerializeField, Tooltip("�o���l")] private DropItem _expItem;
    [SerializeField, Tooltip("����")] private DropItem _moneyItem;

    [Header("������ԗ�")]
    [SerializeField, Tooltip("��")] private float _forceHeightPower;
    [SerializeField, Tooltip("��")] private float _forceHorizontalPower;

    // ---------------------------- PublicMethod
    /// <summary>
    /// �o���l�𗎂Ƃ�����
    /// </summary>
    public void DropExp()
    {
        for (int i = 0; i < _expItem.value; i++)
        {
            DropAnimation(_expItem.obj);
        }
    }

    /// <summary>
    /// �����𗎂Ƃ�����
    /// </summary>
    public void DropMoney()
    {
        for (int i = 0; i < _moneyItem.value; i++)
        {
            DropAnimation(_moneyItem.obj);
        }
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// �A�C�e���𐶐����Ĕ�΂�����
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="dropObj"></param>
    private void DropAnimation(GameObject dropObj)
    {
        GameObject obj = Instantiate(dropObj);
        obj.transform.position = transform.position;

        if (obj.GetComponent<Rigidbody>() == null)
            obj.AddComponent<Rigidbody>();

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
