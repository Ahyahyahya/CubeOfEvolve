using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using UnityEngine;

public class OptionBase : MonoBehaviour, IModuleID
{
    // ---------------------------- SerializeField
    [Header("�f�[�^")]
    [SerializeField, Tooltip("�f�[�^")] protected ModuleData _data;
    [SerializeField, Tooltip("�o�t�f�[�^")] private StatusEffectData[] _statusEffectData;

    // ---------------------------- Field
    protected float _attack;
    protected float _currentInterval;

    // ---------------------------- Property
    /// <summary>
    /// ID
    /// </summary>
    public int Id => _data.Id;

    // ---------------------------- AbstractMethod
    /// <summary>
    /// �������ꂽ�Ƃ��̏���
    /// </summary>
    public void WhenEquipped()
    {
        foreach (var item in _statusEffectData)
        {
            RuntimeModuleManager.Instance.AddOption(item);
        }
    }

    /// <summary>
    /// �O���ꂽ�Ƃ��̏���
    /// </summary>
    public void ProcessingWhenRemoved()
    {
        foreach (var item in _statusEffectData)
        {
            RuntimeModuleManager.Instance.RemoveOption(item);
        }
    }
}
