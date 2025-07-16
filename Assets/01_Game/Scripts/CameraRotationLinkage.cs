using UnityEngine;

namespace Assets.AT.CameraCtrl
{
    public class CameraRotationLinkage : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera; // ��������鑤�̃J����
        [SerializeField] private Camera referenceCamera; // ��ƂȂ�J����

        private void Start()
        {
            if (targetCamera == null || referenceCamera == null) enabled = false;
        }

        private void LateUpdate()
        {
            // ��J������Z���̉�]�p�x���擾
            Vector3 referenceRotation = referenceCamera.transform.eulerAngles;
            targetCamera.transform.rotation = Quaternion.Euler(referenceRotation);
        }
    }
}