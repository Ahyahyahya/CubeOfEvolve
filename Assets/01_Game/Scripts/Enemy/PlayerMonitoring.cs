using UnityEngine;

public class PlayerMonitoring : MonoBehaviour
{
    // �V���O���g��
    public static PlayerMonitoring Instance;

    // ---------------------------- SerializeField
    [Header("�v���C���[")]
    [SerializeField, Tooltip("�v���C���[")] private GameObject _playerObj;

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
}
