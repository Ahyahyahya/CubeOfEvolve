using App.GameSystem.Modules;
using UnityEngine;
using UnityEngine.UI;

public class OptionBase : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Tooltip("�f�[�^")] private StatusEffectData[] _data;
    [SerializeField, Tooltip("ID")] private int _id = -1;

    // ---------------------------- Field
    protected float _attack;
    protected float _currentInterval;

    // ---------------------------- AbstractMethod
    /// <summary>
    /// �������ꂽ�Ƃ��̏���
    /// </summary>
    public void WhenEquipped()
    {
        foreach (var item in _data)
        {
            RuntimeModuleManager.Instance.AddOption(item);
        }
    }

    /// <summary>
    /// �O���ꂽ�Ƃ��̏���
    /// </summary>
    public void ProcessingWhenRemoved()
    {
        foreach (var item in _data)
        {
            RuntimeModuleManager.Instance.RemoveOption(item);
        }
    }
}
