// �쐬���F   250522
// �X�V���F   250522
// �쐬�ҁF ���� ���l

using System.Collections.Generic;
using UnityEngine;

namespace App.BaseSystem.DataStores.ScriptableObjects.Modules
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Data/Module")]
    public class ModuleData : BaseData
    {
        // -----
        // -----Enum

        public enum MODULE_TYPE
        {
            [InspectorName("�Ȃ�")]
            None = 0,
            [InspectorName("����")]
            Weapons,
            [InspectorName("�I�v�V����")]
            Options,
        }
        private static readonly Dictionary<MODULE_TYPE, string> _moduleTypeMapping = new Dictionary<MODULE_TYPE, string>()
        {
            {MODULE_TYPE.None, "�Ȃ�"},
            {MODULE_TYPE.Weapons, "�E�F�|��"},
            {MODULE_TYPE.Options, "�I�v�V����"},
        };

        // -----SerializeField

        // �s��
        [SerializeField] private MODULE_TYPE _moduleType;
        [SerializeField] private string _viewName = "�\����";
        [SerializeField] private string _description = "������";
        [SerializeField] private int _basePrice = 100;
        //[SerializeField] private BaseWeapon _weaponData; <- ���퓙�̃f�[�^�B�U���͂ȂǕϐ��ƁA�_���[�W�����̋L�q�������Ă�B

        // ��
        [SerializeField] private int _level = 0;
        [SerializeField] private int _quantity = 0;

        

        // -----Property
        public static IReadOnlyDictionary<MODULE_TYPE,string> ModuleTypeMapping => _moduleTypeMapping;

        public MODULE_TYPE ModuleType => _moduleType;
        public string ViewName => _viewName;
        public string Description => _description;
        public int BasePrice => _basePrice;

        public int Level => _level;
        public int Quantity => _quantity;
        //public BaseWeapon WeaponData => _weaponData;
    }
}
