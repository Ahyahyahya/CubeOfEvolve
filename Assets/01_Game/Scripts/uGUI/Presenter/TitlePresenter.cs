// AT
// �^�C�g����ʂł̏��������s����B

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MVRP.AT.Presenter
{
    public class TitlePresenter : MonoBehaviour
    {
        // -----
        // -----SerializeField
        [Header("CheckCanvas")]
        [SerializeField] private CheckCanvasCtrl _checkCanvasCtrl;

        [SerializeField] private Button _resetButton;
        [SerializeField] private CheckDialogConfig _resetEvent;

        [SerializeField] private Button _quitButton;
        [SerializeField] private CheckDialogConfig _quitEvent;

        // -----UnityMessage
        private void Start()
        {
            if (!Initialize()) enabled = false;
        }

        // -----private
        private bool Initialize()
        {
            var isSuccess = false;

            // �ˑ��`�F�b�N
            if (_checkCanvasCtrl == null || _resetEvent == null || _quitEvent == null)
                Debug.Log($"TitlePresenter: �ˑ����s���̂��ߏ������~");
            else isSuccess = true;

            if(_resetButton == null) Debug.Log($"TitlePresenter: _resetButton ��null �ł�");
            else
            {
                _resetButton.onClick.AddListener(() => _checkCanvasCtrl.ShowCheckCanvas(_resetEvent, _resetButton));
                isSuccess = true;
            }

            if (_quitButton == null) Debug.Log($"TitlePresenter: _quitButton ��null �ł�");
            else
            {
                _quitButton.onClick.AddListener(() => _checkCanvasCtrl.ShowCheckCanvas(_quitEvent, _quitButton));
                isSuccess = true;
            }

            Debug.Log($"TitlePresenter:�����������H=>{isSuccess}");
            return isSuccess;
        }

    }
}