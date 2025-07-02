using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.View
{
    public class ViewInfo : MonoBehaviour
    {
        // ----- SerializedField
        [SerializeField] private Image _itemImage; // ���W���[���̃A�C�R���摜�B
        [SerializeField] private Image _blockSizeImage; // ���W���[���̃u���b�N�T�C�Y�������摜�B
        [SerializeField] private TextMeshProUGUI _levelText; // ���W���[���̃��x����\������e�L�X�g�B
        [SerializeField] private TextMeshProUGUI _quantityText; // ���W���[���̃��x����\������e�L�X�g�B
        [SerializeField] private TextMeshProUGUI _unitNameText; // ���W���[���̖��O��\������e�L�X�g�B
        [SerializeField] private TextMeshProUGUI _atkText; // ���W���[���̍U���͂�\������e�L�X�g�i��j�B
        [SerializeField] private TextMeshProUGUI _rapidText; // ���W���[���̍U�����x��\������e�L�X�g�i��j�B
        [SerializeField] private TextMeshProUGUI _priceText; // ���W���[���̉��i��\������e�L�X�g�i��j�B

        // ----- Public
        /// <summary>
        /// ���W���[������UI�ɐݒ肵�܂��B
        /// </summary>
        /// <param name="masterData">���W���[���̃}�X�^�[�f�[�^�B</param>
        /// <param name="runtimeData">���W���[���̃����^�C���f�[�^�B</param>
        public void SetInfo(ModuleData masterData, RuntimeModuleData runtimeData)
        {
            if (masterData == null || runtimeData == null)
            {
                Debug.LogError("Detailed_View.SetInfo��MasterData�܂���RuntimeData��null�ł��B");
                return;
            }

            // �e�e�L�X�g�R���|�[�l���g�ɒl����
            if (_levelText != null) _levelText.text = $"{runtimeData.CurrentLevelValue}";
            if (_quantityText != null) _quantityText.text = $"{runtimeData.CurrentQuantityValue}";
            if (_unitNameText != null) _unitNameText.text = masterData.ViewName; // �}�X�^�[�f�[�^�̕\�����B
            // ATK�̌v�Z��: �U���͂̓}�X�^�[�f�[�^�ƃ��x������v�Z����܂��B
            // if (_atkText != null) _atkText.text = $"ATK: {masterData.AttackPower + (runtimeData.CurrentLevelValue * 5)}";
            // SPD�̗�: ���x�̓}�X�^�[�f�[�^����擾����܂��B
            // if (_rapidText != null) _rapidText.text = $"SPD: {masterData.Speed}";
            if (_priceText != null) _priceText.text = $"{masterData.BasePrice}";

            // �A�C�e���摜�ƃu���b�N�T�C�Y�摜�́A`ModuleData` �� `Sprite` �Ȃǂ̉摜��񂪊܂܂�Ă���ꍇ�ɐݒ肵�܂��B
            if (_itemImage != null) _itemImage.sprite = masterData.MainSprite;
            // if (_blockSizeImage != null) _blockSizeImage.sprite = masterData.BlockSprite;
        }
    }
}