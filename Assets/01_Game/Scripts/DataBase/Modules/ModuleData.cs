using System.Collections.Generic;
using UnityEngine;

namespace App.BaseSystem.DataStores.ScriptableObjects.Modules
{
    /// <summary>
    /// ���W���[���f�[�^���`����ScriptableObject�N���X�B
    /// �e���W���[���̎�ށA�\�����A�����A��{���i�A���x���A���ʂȂǂ�ێ����܂��B
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObjects/Data/Module")]
    public class ModuleData : BaseData
    {
        // ----- Enum
        /// <summary>
        /// ���W���[���̎�ނ��`����񋓌^�ł��B
        /// </summary>
        public enum MODULE_TYPE
        {
            [InspectorName("�Ȃ�")]
            None = 0, // ���W���[���̎�ނ��ݒ肳��Ă��Ȃ���Ԃ������܂��B
            [InspectorName("����")]
            Weapons,  // ����^�C�v�̃��W���[���������܂��B
            [InspectorName("�I�v�V����")]
            Options,  // �I�v�V�����^�C�v�̃��W���[���������܂��B
        }

        /// <summary>
        /// MODULE_TYPE�񋓌^�Ƃ���ɑΉ�������{�ꕶ����̃}�b�s���O�B
        /// </summary>
        private static readonly Dictionary<MODULE_TYPE, string> _moduleTypeMapping = new Dictionary<MODULE_TYPE, string>()
        {
            {MODULE_TYPE.None, "�Ȃ�"},     // ���W���[���^�C�v�u�Ȃ��v�̕\����
            {MODULE_TYPE.Weapons, "�E�F�|��"}, // ���W���[���^�C�v�u����v�̕\����
            {MODULE_TYPE.Options, "�I�v�V����"}, // ���W���[���^�C�v�u�I�v�V�����v�̕\����
        };

        // ----- SerializeField
        // �s��
        [SerializeField] private MODULE_TYPE _moduleType; // ���W���[���̎�ނ�ݒ肵�܂��B
        [SerializeField] private string _viewName = "�\����"; // ���W���[���̕\������ݒ肵�܂��B
        [SerializeField] private string _description = "������"; // ���W���[���̏ڍׂȐ�������ݒ肵�܂��B
        [SerializeField] private int _basePrice = 100; // ���W���[���̊�{���i��ݒ肵�܂��B
        //[SerializeField] private BaseWeapon _weaponData; // ���퓙�̃f�[�^�B�U���͂ȂǕϐ��ƁA�_���[�W�����̋L�q�������Ă�B

        // ��
        [SerializeField] private int _level = 0; // ���W���[���̌��݂̃��x����ݒ肵�܂��B
        [SerializeField] private int _quantity = 0; // ���W���[���̌��݂̐��ʂ�ݒ肵�܂��B

        // ----- Property
        /// <summary>
        /// ���W���[���^�C�v�Ƃ��̕\�����̓ǂݎ���p�������擾���܂��B
        /// </summary>
        public static IReadOnlyDictionary<MODULE_TYPE, string> ModuleTypeMapping => _moduleTypeMapping;

        /// <summary>
        /// ���̃��W���[���̎�ނ��擾���܂��B
        /// </summary>
        public MODULE_TYPE ModuleType => _moduleType;
        /// <summary>
        /// ���̃��W���[���̕\�������擾���܂��B
        /// </summary>
        public string ViewName => _viewName;
        /// <summary>
        /// ���̃��W���[���̐��������擾���܂��B
        /// </summary>
        public string Description => _description;
        /// <summary>
        /// ���̃��W���[���̊�{���i���擾���܂��B
        /// </summary>
        public int BasePrice => _basePrice;

        /// <summary>
        /// ���̃��W���[���̌��݂̃��x�����擾���܂��B
        /// </summary>
        public int Level => _level;
        /// <summary>
        /// ���̃��W���[���̌��݂̐��ʂ��擾���܂��B
        /// </summary>
        public int Quantity => _quantity;
        //public BaseWeapon WeaponData => _weaponData; // ����f�[�^�Ɋւ���v���p�e�B�B�R�����g�A�E�g����Ă��܂��B
    }
}