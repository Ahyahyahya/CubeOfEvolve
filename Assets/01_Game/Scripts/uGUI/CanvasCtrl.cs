// �쐬���F 250508
// �X�V���F 250508 �@�\����
//              250820 �@�\�g�� ����{�^���𕡐��ݒ�\�ɁB
// �쐬�ҁF ���� ���l

// �T�v����(AI�ɂ��쐬)�F

// �g���������F

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AT.uGUI
{
    public class CanvasCtrl : MonoBehaviour
    {
        // ---------------------------- SerializeField
        [Header("�{�^��")]
        [SerializeField] private List<Button> _openBtnList = new List<Button> ();
        //[SerializeField] private Button _closeBtn;
        [SerializeField] private List<Button> _closeBtnList = new List<Button> ();
        // ---------------------------- Field
        private Button _saveBtn; // �ꎞ�ϐ�
        private Canvas _canvas;
        // ---------------------------- button
        private void OnOpenCanvas(Button clickedButton) // �����Ƃ��ĉ����ꂽ�{�^�����󂯎��
        {
            Debug.Log("�{�^�������Fin - " + clickedButton.name); // �ǂ̃{�^���������ꂽ���m�F
            _canvas.enabled = true;
            _closeBtnList[0].Select();
            _saveBtn = clickedButton;
        }
        private void OnCloseCanvas()
        {
            Debug.Log("�{�^�������Fout");
            _canvas.enabled = false;
            if (_saveBtn != null) // _saveBtn ��null�łȂ����m�F
            {
                _saveBtn.Select();
            }
        }

        // ---------------------------- UnityMessage

        private void Start()
        {
            _canvas = GetComponent<Canvas>();
            if (_openBtnList == null)
                return;
            else
                foreach (var button in _openBtnList)
                {
                    // �����_�����g���āA�{�^�����N���b�N���ꂽ�Ƃ��� OnOpenCanvas ���\�b�h�Ɏ��g��n��
                    button.onClick.AddListener(() => OnOpenCanvas(button));
                }
            if(_closeBtnList == null)
                return ;
            else
                foreach (var button in _closeBtnList)
                {
                    button.onClick.AddListener(() => OnCloseCanvas());
                }
        }

        // ---------------------------- PublicMethod

        // ---------------------------- PrivateMethod

    }
}
