// RuntimeModuleData.cs
using System;
using App.BaseSystem.DataStores.ScriptableObjects.Modules; // ModuleData �̖��O���

/// <summary>
/// �v���C���[�����L����P��̃��W���[���̃����^�C���f�[�^�B�Q�[�����ɏ�Ԃ��ω�����B
/// </summary>
[Serializable] // ���̃N���X��JSON�ȂǂŃV���A���C�Y�\�ɂ���
public class RuntimeModuleData
{
    // ------------------ �}�X�^�[�f�[�^����擾�����s�ςȏ�� (�ŏ����ɗ��߂�)
    public int Id { get; private set; } // ���W���[���̃}�X�^�[�f�[�^ID
    public string Name { get; set; } // ���W���[���̖��O (�\���p�A�}�X�^�[�f�[�^����擾)

    // ------------------ �Q�[�����ɕω������� (RuntimeModuleData �݂̂��ێ�����)
    public int CurrentLevel { get; set; } = 0; // ���W���[���̌��݂̃��x��
    public int Quantity { get; set; } = 0; // ������ (�v���C���[���L�̊T�O)

    // ------------------ �R���X�g���N�^
    /// <summary>
    /// ModuleData�i�}�X�^�[�f�[�^�j����ɐV���������^�C�����W���[�������������܂��B
    /// </summary>
    /// <param name="masterData">���̃����^�C�����W���[���̌��ƂȂ�ModuleData�B</param>
    public RuntimeModuleData(ModuleData masterData)
    {
        // �}�X�^�[�f�[�^����ID�Ɩ��O���R�s�[�i���邢�̓L���b�V���j
        Id = masterData.Id;
        Name = masterData.Name; // BaseData �� Name �v���p�e�B

        // �����^�C���f�[�^�̏����l��ݒ�
        // masterData �� _initialLevel ������΂���𗘗p
        // �����masterData�Ƀ��x�����Ȃ��̂ŁA�������x����0�Ƃ��܂�
        CurrentLevel = masterData.Level;
        Quantity = masterData.Quantity; // �v���C���[�����W���[�������L�����珉������1�Ƃ����
    }

    /// <summary>
    /// �Z�[�u�f�[�^�iModuleSaveState�j����Ƀ����^�C�����W���[���𕜌����܂��B
    /// </summary>
    /// <param name="state">���[�h���郂�W���[���̃Z�[�u�f�[�^�B</param>
    public RuntimeModuleData(ModuleSaveState state)
    {
        Id = state.id;
        CurrentLevel = state.level;
        Quantity = state.quantity;
        // Name �̓��[�h���� RuntimeModuleManager ����}�X�^�[�f�[�^���Q�Ƃ��Đݒ肳��邩�A
        // �K�v�ł���΃Z�[�u�f�[�^�Ɋ܂߂邱�Ƃ��\�ł��B�iID������Βʏ�͕s�v�j
    }

    // ------------------ �Z�[�u�f�[�^�\�� (�l�X�g���ꂽ�N���X�Ƃ��Ē�`)
    // ���̃N���X�� RuntimeModuleData �̏�Ԃ�ۑ����邽�߂Ɏg����
    [Serializable]
    public class ModuleSaveState
    {
        public int id;
        public int level;
        public int quantity;
    }
}