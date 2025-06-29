using System.Collections.Generic;
using UnityEngine;

public class EnemyCollective : MonoBehaviour
{
    // ---------------------------- Field
    private List<EnemyStatus> _enemyList = new();

    // ---------------------------- UnityMessage
    private void Start()
    {
        var targetObj = PlayerMonitoring.Instance.PlayerObj;

        // �G����Ώۂւ̃x�N�g�����擾
        var moveForward = targetObj.transform.position - transform.position;

        // �����͒ǂ�Ȃ�
        moveForward.y = 0;

        // �L�����N�^�[�̌�����i�s�����Ɍ�����
        if (moveForward != Vector3.zero)
        {
            // �����x�N�g�����擾
            Vector3 direction = targetObj.transform.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

            // Y���̉�]�̂ݎ擾
            transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        }


        // ���g�̎q�I�u�W�F�N�g���X�L�������ă��X�g�ɒǉ�
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            var status = child.GetComponent<EnemyStatus>();

            // �L���� EnemyStatus �����X�g�ɒǉ�
            if (status != null && !_enemyList.Contains(status))
            {
                _enemyList.Add(status);
            }
        }

        // �e�G���o���E�e�q�֌W������
        foreach (EnemyStatus status in _enemyList)
        {
            // �G�̏���������
            status.EnemySpawn();

            // �G�����̃I�u�W�F�N�g����O��
            status.transform.SetParent(null);
        }
    }
}
