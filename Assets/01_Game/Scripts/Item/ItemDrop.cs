using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemDrop : MonoBehaviour
{
    [Serializable]
    private class DropItem
    {
        // �A�C�e���f�[�^
        public ItemData data;

        // ���Ƃ���
        public int value = 1;

        // �m�� 0�`100��
        public float rate = 100;
    }

    // ---------------------------- SerializeField
    [Header("�h���b�v�������")]
    [SerializeField, Tooltip("")] private DropItem[] _dropItemList;

    [Header("������ԗ�")]
    [SerializeField, Tooltip("��")] private float _forceHeightPower;
    [SerializeField, Tooltip("��")] private float _forceHorizontalPower;

    // ---------------------------- PublicMethod
    /// <summary>
    /// �A�C�e���𗎂Ƃ�����
    /// </summary>
    public void DropItemProcess()
    {
        foreach (var dropItem in _dropItemList)
        {
            var random = Random.Range(0f, 1f) * 100;

            if (random <= dropItem.rate)
            {
                for (int i = 0; i < dropItem.value; i++)
                {
                    DropAnimation(dropItem.data.Item);
                }
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
