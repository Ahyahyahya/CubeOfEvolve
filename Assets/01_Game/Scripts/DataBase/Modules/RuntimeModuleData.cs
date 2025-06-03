using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using R3;
using System;
using UnityEngine;

namespace App.GameSystem.Modules
{
    /// <summary>
    /// �Q�[�����ɓ��I�ɕω����郂�W���[���f�[�^���Ǘ�����N���X�B
    /// �}�X�^�[�f�[�^ (ModuleData) ����ɏ���������A���x���␔�ʂȂǂ̏�Ԃ�ێ����܂��B
    /// </summary>
    [Serializable]
    public class RuntimeModuleData
    {
        // ----- Property (���J�v���p�e�B)
        public int Id { get; private set; } // ���W���[���̈�ӂ�ID�B

        // ----- ReactiveProperty (���A�N�e�B�u�v���p�e�B)
        private ReactiveProperty<int> _currentLevel; // ���݂̃��x�����Ǘ�����ReactiveProperty�B
        public ReadOnlyReactiveProperty<int> Level => _currentLevel; // �O�����J�p�̓ǂݎ���p���x���v���p�e�B�B
        public int CurrentLevelValue => _currentLevel.Value; // ���݂̃��x���̒��ڒl�B

        private ReactiveProperty<int> _quantity;
        public ReadOnlyReactiveProperty<int> Quantity => _quantity;
        public int CurrentQuantityValue => _quantity.Value;

        private ReactiveProperty<float> _attack;
        public ReadOnlyReactiveProperty<float> Atk => _attack;
        public float CurrentAttackValue => _attack.Value;

        private ReactiveProperty<float> _bulletSpeed;
        public ReadOnlyReactiveProperty<float> Spd => _bulletSpeed;
        public float CurrentBulletSpeedValue => _bulletSpeed.Value;

        private ReactiveProperty<float> _interval;
        public ReadOnlyReactiveProperty<float> Interval => _interval;
        public float CurrentIntervalValue => _interval.Value;

        private ReactiveProperty<float> _searchRange;
        public ReadOnlyReactiveProperty<float> SearchRange => _searchRange;
        public float CurrentSearchRangeValue => _searchRange.Value;

        // ----- Constructor (�R���X�g���N�^)
        /// <summary>
        /// ModuleData�}�X�^�[�f�[�^����RuntimeModuleData�̃C���X�^���X�����������܂��B
        /// </summary>
        /// <param name="masterData">���W���[���̃}�X�^�[�f�[�^�B</param>
        public RuntimeModuleData(ModuleData masterData)
        {
            Id = masterData.Id; // �}�X�^�[�f�[�^����ID��ݒ�B
            _currentLevel = new ReactiveProperty<int>(masterData.Level); // �������x���̓}�X�^�[�f�[�^�������B
            _quantity = new ReactiveProperty<int>(masterData.Quantity); // �������ʂ̓}�X�^�[�f�[�^�������B
            _attack = new ReactiveProperty<float>(masterData.ModuleState.Attack);
            _bulletSpeed = new ReactiveProperty<float>(masterData.ModuleState.BulletSpeed);
            _interval = new ReactiveProperty<float>(masterData.ModuleState.Interval);
            _searchRange = new ReactiveProperty<float>(masterData.ModuleState.SearchRange);
        }

        // ----- Private

        private void LevelUpBonus()
        {

        }

        // ----- Public Methods (���J���\�b�h)
        /// <summary>
        /// ���W���[���̃��x�����X�V���܂��B
        /// </summary>
        /// <param name="newLevel">�ݒ肷��V�������x���B</param>
        public void SetLevel(int newLevel)
        {
            if (newLevel < 0) newLevel = 0; // ���x�������̒l�ɂȂ�Ȃ��悤�ɐ����B
            _currentLevel.Value = newLevel; // ReactiveProperty�̒l���X�V�B
            LevelUpBonus();
        }

        /// <summary>
        /// ���W���[���̐��ʂ��X�V���܂��B
        /// </summary>
        /// <param name="newQuantity">�ݒ肷��V�������ʁB</param>
        public void SetQuantity(int newQuantity)
        {
            if (newQuantity < 0) newQuantity = 0; // ���ʂ����̒l�ɂȂ�Ȃ��悤�ɐ����B
            _quantity.Value = newQuantity; // ReactiveProperty�̒l���X�V�B
        }

        /// <summary>
        /// ���W���[���̃��x����1�グ�܂��B
        /// </summary>
        public void LevelUp() => SetLevel(_currentLevel.Value + 1);

        /// <summary>
        /// ���W���[���̐��ʂ��w�肳�ꂽ�ʂ����ύX���܂��B
        /// </summary>
        /// <param name="amount">���ʂ̑����ʁB</param>
        public void ChangeQuantity(int amount) => SetQuantity(_quantity.Value + amount);
    }
}