using App.GameSystem.Modules;
using UnityEngine;

public class aaaDrop : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Cube"))
        {
            if (RuntimeModuleManager.Instance != null)
            {
                RuntimeModuleManager.Instance.TriggerDropUI();
            }
            else
            {
                Debug.LogWarning("RuntimeModuleManager.Instance �����݂��܂���B�h���b�vUI��\���ł��܂���B", this);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cube"))
        {
            if (RuntimeModuleManager.Instance != null)
            {
                RuntimeModuleManager.Instance.TriggerDropUI();
            }
            else
            {
                Debug.LogWarning("RuntimeModuleManager.Instance �����݂��܂���B�h���b�vUI��\���ł��܂���B", this);
            }
        }
    }
}
