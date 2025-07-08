// �쐬���F 250508
// �X�V���F 250508 �@�\����
//              250820 �@�\�g�� ����{�^���𕡐��ݒ�\�ɁB
// �쐬�ҁF ���� ���l

// �T�v����(AI�ɂ��쐬)�F

// �g���������F

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AT.uGUI
{
    public class CanvasCtrl : MonoBehaviour
    {
        // ---------------------------- SerializeField
        [Header("�{�^��")]
        [SerializeField] private List<Button> _openBtnList = new List<Button>();
        [SerializeField] private List<Button> _closeBtnList = new List<Button>();
        [Header("�C�x���g")]
        [SerializeField] private UnityEvent _openEvent = new();
        [SerializeField] private UnityEvent _closeEvent = new();
        // ---------------------------- Field
        private Button _saveBtn; // �ꎞ�ϐ�
        private Canvas _canvas;
        // ---------------------------- button
        public void OnOpenCanvas(Button clickedButton = null) // �����Ƃ��ĉ����ꂽ�{�^�����󂯎��
        {
            if (clickedButton == null) Debug.LogWarning("null�Ȃ̂ł���\");
            _canvas.enabled = true;
            if (_closeBtnList.Count != 0) _closeBtnList[0].Select();
            if (clickedButton != null) _saveBtn = clickedButton;
            _openEvent?.Invoke();
        }
        public void OnCloseCanvas()
        {
            _canvas.enabled = false;
            if (_saveBtn != null) _saveBtn.Select();
            _closeEvent?.Invoke();
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
                    button.onClick.AddListener(() => OnOpenCanvas(button));
                }

            if (_closeBtnList == null)
                return;
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
