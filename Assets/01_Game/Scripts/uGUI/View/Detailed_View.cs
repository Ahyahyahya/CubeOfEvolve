using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Detailed_View : MonoBehaviour
{
    // -----
    // -----SerializeField
    [SerializeField] private Image _itemImage;
    [SerializeField] private Image _blockSizeImage;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _unitNameText;
    [SerializeField] private TextMeshProUGUI _atkText; // ��: �U����
    [SerializeField] private TextMeshProUGUI _rapidText; // ��: �U�����x
    [SerializeField] private TextMeshProUGUI _priceText; // ��: ���i

    // -----Public
    /// <summary>
    /// ���W���[������UI�ɐݒ肵�܂��B
    /// </summary>
    /// <param name="masterData">���W���[���̃}�X�^�[�f�[�^�B</param>
    /// <param name="runtimeData">���W���[���̃����^�C���f�[�^�B</param>
    public void SetInfo(ModuleData masterData, RuntimeModuleData runtimeData)
    {
        if (masterData == null || runtimeData == null)
        {
            Debug.LogError("MasterData or RuntimeData is null in Detailed_View.SetInfo.");
            return;
        }

        // �A�C�e���摜��� (ModuleData��Sprite�Ȃǂ̉摜��񂪂����)
        // if (_itemImage != null) _itemImage.sprite = masterData.IconSprite;

        // �u���b�N�T�C�Y�摜��� (���������)
        // if (_blockSizeImage != null) _blockSizeImage.sprite = masterData.BlockSprite;

        // �e�e�L�X�g�R���|�[�l���g�ɒl����
        if (_levelText != null) _levelText.text = $"Lv: {runtimeData.CurrentLevel}";
        if (_unitNameText != null) _unitNameText.text = masterData.ViewName; // �}�X�^�[�f�[�^�̕\����
        //if (_atkText != null) _atkText.text = $"ATK: {masterData.AttackPower + (runtimeData.CurrentLevel * 5)}"; // ��: �U���͂̓}�X�^�[�f�[�^�ƃ��x������v�Z
        //if (_rapidText != null) _rapidText.text = $"SPD: {masterData.Speed}"; // ��: ���x�̓}�X�^�[�f�[�^����
        if (_priceText != null) _priceText.text = $"Price: {masterData.BasePrice}";
    }
}