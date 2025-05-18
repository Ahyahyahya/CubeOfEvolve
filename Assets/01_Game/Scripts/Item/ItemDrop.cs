using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    // �V���O���g��
    public static ItemDrop Instance;

    // ---------------------------- SerializeField
    [Header("�v���C���[")]
    [SerializeField, Tooltip("�v���C���[")] private GameObject _playerObj;

    [Header("�h���b�v�������")]
    [SerializeField, Tooltip("�o���l")] private GameObject _expObj;
    [SerializeField, Tooltip("����")] private GameObject _money;

    [Header("������ԗ�")]
    [SerializeField, Tooltip("��")] private float _forceHeightPower;
    [SerializeField, Tooltip("��")] private float _forceHorizontalPower;

    // ---------------------------- Property
    public GameObject PlayerObj => _playerObj;

    // ---------------------------- UnityMessage
    private void Awake()
    {
        // �V���O���g��
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ---------------------------- PublicMethod
    public void DropExp(Vector3 pos, int value)
    {
        for (int i = 0; i < value; i++)
        {
            DropAnimation(pos, _expObj);
        }
    }
    public void DropMoney(Vector3 pos, int value)
    {
        for (int i = 0; i < value; i++)
        {
            DropAnimation(pos, _money);
        }
    }

    // ---------------------------- PrivateMethod
    private void DropAnimation(Vector3 pos, GameObject dropObj)
    {
        GameObject obj = Instantiate(dropObj);
        obj.transform.position = pos;

        if (obj.GetComponent<Rigidbody>() == null)
            obj.AddComponent<Rigidbody>();

        Rigidbody rb = obj.GetComponent<Rigidbody>();

        // 360�x���璊�I
        float spawnAngle = Random.Range(0, 360);
        // ���W�A���p�ɕύX
        float radians = spawnAngle * Mathf.Deg2Rad;
        // ����
        Vector3 direction = new Vector3(Mathf.Sin(radians), _forceHeightPower, Mathf.Cos(radians));

        // ��΂�
        rb.AddForce(_forceHorizontalPower * direction, ForceMode.Impulse);
    }
}
