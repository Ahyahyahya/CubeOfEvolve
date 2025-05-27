// App.GameSystem.Modules/RuntimeModuleData.cs
using R3; // R3���g�p
using System; // Serializable���g�p����ꍇ
using UnityEngine; // ScriptableObject�ȂǁAUnity�̌^���g�p����ꍇ
using App.BaseSystem.DataStores.ScriptableObjects.Modules; // ModuleData���Q�Ƃ��邽��

namespace App.GameSystem.Modules
{
    [Serializable] // Unity Inspector�ŕ\���������ꍇ�Ȃ�
    public class RuntimeModuleData
    {
        public int Id { get; private set; }

        // ���݂̃��x����ReactiveProperty�Ō��J
        [SerializeField]
        private ReactiveProperty<int> _currentLevel;
        public ReadOnlyReactiveProperty<int> Level => _currentLevel;
        public int CurrentLevelValue => _currentLevel.Value; // ���ڒl���擾���邽�߂̃v���p�e�B

        // ���݂̐��ʂ�ReactiveProperty�Ō��J
        [SerializeField]
        private ReactiveProperty<int> _quantity;
        public ReadOnlyReactiveProperty<int> Quantity => _quantity;
        public int CurrentQuantityValue => _quantity.Value; // ���ڒl���擾���邽�߂̃v���p�e�B

        // �R���X�g���N�^ (MasterData���珉����)
        public RuntimeModuleData(ModuleData masterData)
        {
            Id = masterData.Id;
            _currentLevel = new ReactiveProperty<int>(0); // �������x����0
            _quantity = new ReactiveProperty<int>(0); // �������ʂ�0
        }

        // ���x�����X�V����������\�b�h
        public void SetLevel(int newLevel)
        {
            if (newLevel < 0) newLevel = 0;
            _currentLevel.Value = newLevel;
        }

        // ���ʂ��X�V����������\�b�h
        public void SetQuantity(int newQuantity)
        {
            if (newQuantity < 0) newQuantity = 0;
            _quantity.Value = newQuantity;
        }

        // Convenience methods for changing level/quantity (�ʏ��RuntimeModuleManager�o�R�ŌĂ΂��)
        public void LevelUp() => SetLevel(_currentLevel.Value + 1);
        public void ChangeQuantity(int amount) => SetQuantity(_quantity.Value + amount);
    }
}