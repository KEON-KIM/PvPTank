using UnityEngine;
using Protocol;

namespace PvpTank
{ 
    public class InputManager : MonoBehaviour
    {
        public VirtualStick virtualStick;
        public ShotButton attackButton;
        private bool isMove = false;
        void Start()
        {
            GameManager.InGame += MobileInput;
            GameManager.InGame += AttackInput;
            GameManager.AfterInGame += SendNoMoveMessage;
        }

        void MobileInput()
        {
            if (!virtualStick)
            {

                return;
            }

            int keyCode = 0;
            isMove = false;

            if (!virtualStick.isInputEnable)
            {
#if !UNITY_EDITOR
			isMove = false;
#endif

                return;
            }

            isMove = true;

            keyCode |= KeyEventCode.MOVE;
            Vector3 moveVector = new Vector3(virtualStick.GetHorizontalValue(), 0, virtualStick.GetVerticalValue());
            moveVector = Vector3.Normalize(moveVector);


            if (keyCode <= 0)
            {
                return;
            }

            KeyMessage msg;
            msg = new KeyMessage(keyCode, moveVector);
            if (MatchManager.GetInstance().IsHost())
            {
                MatchManager.GetInstance().AddMsgToLocalQueue(msg);
            }
            else
            {
                MatchManager.GetInstance().SendDataToInGame<KeyMessage>(msg);
            }
        }

        void AttackInput()
        {
            if (!attackButton)
            {
                return;
            }
            if (!attackButton.buttondown)
            {
                return;
            }

            int keyCode = 0;
            keyCode |= KeyEventCode.ATTACK;
            
            Vector3 aimPos = new Vector3(virtualStick.GetHorizontalValue(), 0, virtualStick.GetVerticalValue());
            aimPos += WorldManager.instance.GetMyPlayerPos();
            KeyMessage msg;
            msg = new KeyMessage(keyCode, aimPos);
            if (MatchManager.GetInstance().IsHost())
            {
                MatchManager.GetInstance().AddMsgToLocalQueue(msg);
            }
            else
            {
                MatchManager.GetInstance().SendDataToInGame<KeyMessage>(msg);
            }
        }

        public void AttackInput(Vector3 pos)
        {
            if (GameManager.GetInstance().GetGameState() != GameManager.GameState.InGame)
            {
                return;
            }
            int keyCode = 0;
            keyCode |= KeyEventCode.ATTACK;

            KeyMessage msg;
            msg = new KeyMessage(keyCode, pos);
            if (MatchManager.GetInstance().IsHost())
            {
                MatchManager.GetInstance().AddMsgToLocalQueue(msg);
            }
            else
            {
                MatchManager.GetInstance().SendDataToInGame<KeyMessage>(msg);
            }
        }

        void SendNoMoveMessage()
        {
            int keyCode = 0;
            if (!isMove && WorldManager.instance.IsMyPlayerMove())
            {
                keyCode |= KeyEventCode.NO_MOVE;
            }
            if (keyCode == 0)
            {
                return;
            }
            KeyMessage msg = new KeyMessage(keyCode, Vector3.zero);

            if (MatchManager.GetInstance().IsHost())
            {
                MatchManager.GetInstance().AddMsgToLocalQueue(msg);
            }
            else
            {
                MatchManager.GetInstance().SendDataToInGame<KeyMessage>(msg);
            }
        }
    }
}