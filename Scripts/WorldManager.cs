using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protocol;
using BackEnd;
using BackEnd.Tcp;

namespace PvpTank
{
    public class WorldManager : MonoBehaviour
    {
        static public WorldManager instance;
        const int START_COUNT = 5;
        private SessionId userPlayerIndex = SessionId.None;

        #region 플레이어
        public GameObject playerPool;
        public GameObject nickNamePrefeb;
        public GameObject particle;
        //public GameObject playerPrefeb;
        public GameObject[] playerPrefebs = new GameObject[4];
        public int numOfPlayer = 0;
        private const int MAXPLAYER = 4;
        public int alivePlayer { get; set; }
        private Dictionary<SessionId, Player> players;
        private Dictionary<SessionId, GameObject> nicknames;
        public GameObject startPointObject;
        private List<Vector4> statringPoints;
        private Stack<SessionId> gameRecord;
        public delegate void PlayerDie(SessionId index);
        public PlayerDie dieEvent;
        #endregion

        void Awake()
        {
            instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            InitializeGame();
            var matchInstance = MatchManager.GetInstance();
            if (matchInstance == null)
            {
                return;
            }
            if (matchInstance.isReconnectProcess)
            {
                InGameUiManager.GetInstance().SetStartCount(0, false);
                InGameUiManager.GetInstance().SetReconnectBoard(ServerManager.GetInstance().userNickName);
            }

        }

        public bool InitializeGame()
        {
            if (!playerPool)
            {
                Debug.Log("Player Pool Not Exist!");
                return false;
            }
            Debug.Log("게임 초기화 진행");
            gameRecord = new Stack<SessionId>();
            GameManager.OnGameOver += OnGameOver;
            GameManager.OnGameResult += OnGameResult;
            userPlayerIndex = SessionId.None;
            SetPlayerAttribute();
            OnGameStart();

            return true;
        }

        public void SetPlayerAttribute()
        {
            // 시작점
            statringPoints = new List<Vector4>();

            int num = startPointObject.transform.childCount;
            for (int i = 0; i < num; ++i)
            {
                var child = startPointObject.transform.GetChild(i);
                Vector4 point = child.transform.position;
                point.w = child.transform.rotation.eulerAngles.y;
                statringPoints.Add(point);
            }

            dieEvent += PlayerDieEvent;
        }

        private void PlayerDieEvent(SessionId index)
        {
            alivePlayer -= 1;
            //파티클
            var expObject = Instantiate(particle, players[index].GetPosition(), Quaternion.identity);

            InGameUiManager.GetInstance().SetScoreBoard(alivePlayer);
            gameRecord.Push(index);

            Debug.Log(string.Format("Player Die : " + players[index].GetNickName()));
            
            if(MatchManager.GetInstance().IsMySessionId(index))
            {
                Debug.Log("CAMERA CHANGE");
                CameraControl.GetInstance().SetPlayerDie();
            }
            // 호스트가 아니면 바로 리턴
            if (!MatchManager.GetInstance().IsHost())
            {
                return;
            }

            if (alivePlayer <= 1 )
            {
                SendGameEndOrder();
            }

            // if (BackEndMatchManager.GetInstance().nowModeType == MatchModeType.TeamOnTeam)
            // {
            //     if (alivePlayer == 2)
            //     {
            //         int remainTeamNumber = -1;
            //         SessionId remainSession = SessionId.None;
            //         foreach (var player in players)
            //         {
            //             if (player.Value.GetIsLive() == false)
            //             {
            //                 continue;
            //             }
            //             if (remainTeamNumber == -1)
            //             {
            //                 remainTeamNumber = BackEndMatchManager.GetInstance().GetTeamInfo(player.Key);
            //                 remainSession = player.Key;
            //             }
            // else if (remainTeamNumber == BackEndMatchManager.GetInstance().GetTeamInfo(player.Key))
            // {
            //     // 남은 플레이어들이 같은편이면 그대로 게임종료 메시지를 보냄
            //     gameRecord.Push(remainSession);
            //     gameRecord.Push(player.Key);
            //     SendGameEndOrder();
            //     return;
            // }
            //     }
            // }
            // }
            // 1명 이하로 플레이어가 남으면 바로 종료 체크
            /*if(alivePlayer <= 1)
            {
                Debug.Log("Game End");
                SendGameEndOrder();
            }*/
        }

        private void SendGameEndOrder()
        {
            // 게임 종료 전환 메시지는 호스트에서만 보냄
            Debug.Log("Make GameResult & Send Game End Order");
            foreach (SessionId session in MatchManager.GetInstance().sessionIdList)
            {
                if (players[session].GetIsLive() && !gameRecord.Contains(session))
                {
                    gameRecord.Push(session);
                }
            }
            GameEndMessage message = new GameEndMessage(gameRecord);
            MatchManager.GetInstance().SendDataToInGame<GameEndMessage>(message);
        }

        public SessionId GetMyPlayerIndex()
        {
            return userPlayerIndex;
        }

        public void SetPlayerInfo()
        {
            if (MatchManager.GetInstance().sessionIdList == null)
            {
                // 현재 세션ID 리스트가 존재하지 않으면, 0.5초 후 다시 실행
                Invoke("SetPlayerInfo", 0.5f);
                return;
            }
            var gamers = MatchManager.GetInstance().sessionIdList;
            int size = gamers.Count;
            if (size <= 0)
            {
                Debug.Log("No Player Exist!");
                return;
            }
            if (size > MAXPLAYER)
            {
                Debug.Log("Player Pool Exceed!");
                return;
            }

            players = new Dictionary<SessionId, Player>();
            MatchManager.GetInstance().SetPlayerSessionList(gamers);

            int index = 0;
            foreach (var sessionId in gamers)
            {
                GameObject player = Instantiate(playerPrefebs[index], new Vector3(statringPoints[index].x, statringPoints[index].y, statringPoints[index].z), Quaternion.identity, playerPool.transform);
                GameObject nickname = Instantiate(nickNamePrefeb, new Vector3(statringPoints[index].x, statringPoints[index].y+1.5f, statringPoints[index].z), Quaternion.identity, playerPool.transform);
                CameraControl.GetInstance().players.Add(sessionId, player.GetComponent<Player>());
                players.Add(sessionId, player.GetComponent<Player>());

                //InGameUiManager.GetInstance().nicknames.Add(sessionId, nickname);
                if (MatchManager.GetInstance().IsMySessionId(sessionId))
                {
                    userPlayerIndex = sessionId;
                    CameraControl.GetInstance().userPlayerIndex = sessionId;
                    players[sessionId].nameObject = nickname;
                    players[sessionId].Initialize(true, userPlayerIndex, MatchManager.GetInstance().GetNickNameBySessionId(sessionId), statringPoints[index].w);
                }
                else
                {
                    players[sessionId].nameObject = nickname;
                    players[sessionId].Initialize(false, sessionId, MatchManager.GetInstance().GetNickNameBySessionId(sessionId), statringPoints[index].w);
                }
                index += 1;
            }
            Debug.Log("Num Of Current Player : " + size);

            // 스코어 보드 설정
            alivePlayer = size;
            InGameUiManager.GetInstance().SetScoreBoard(alivePlayer);
            CameraControl.GetInstance().UserCameraInitialize();
            if (MatchManager.GetInstance().IsHost())
            {
                StartCoroutine("StartCount");
            }
        }

        public void OnGameStart()
        {
            if (MatchManager.GetInstance() == null)
            {
                // 카운트 다운 : 종료
                InGameUiManager.GetInstance().SetStartCount(0, false);
                return;
            }
            if (MatchManager.GetInstance().IsHost())
            {
                Debug.Log("플레이어 세션정보 확인");

                if (MatchManager.GetInstance().IsSessionListNull())
                {
                    Debug.Log("Player Index Not Exist!");
                    // 호스트 기준 세션데이터가 없으면 게임을 바로 종료한다.
                    foreach (var session in MatchManager.GetInstance().sessionIdList)
                    {
                        // 세션 순서대로 스택에 추가
                        gameRecord.Push(session);
                    }
                    GameEndMessage gameEndMessage = new GameEndMessage(gameRecord);
                    MatchManager.GetInstance().SendDataToInGame<GameEndMessage>(gameEndMessage);
                    return;
                }
            }
            SetPlayerInfo();
        }

        IEnumerator StartCount()
        {
            StartCountMessage msg = new StartCountMessage(START_COUNT);

            // 카운트 다운
            for (int i = 0; i < START_COUNT + 1; i++)
            {
                msg.time = START_COUNT - i;
                MatchManager.GetInstance().SendDataToInGame<StartCountMessage>(msg);
                yield return new WaitForSeconds(1); //1초 단위
            }

            // 게임 시작 메시지를 전송
            GameStartMessage gameStartMessage = new GameStartMessage();
            MatchManager.GetInstance().SendDataToInGame<GameStartMessage>(gameStartMessage);
        }
        //  characters move vector value of players
        public void PreInGame() // send to Data for players
        {
            foreach (var player in players)
            {
                player.Value.SetMoveVector(Vector3.zero);
            }
        }

        public void OnGameOver()
        {
            Debug.Log("Game End");
            if (MatchManager.GetInstance() == null)
            {
                Debug.LogError("매치매니저가 null 입니다.");
                return;
            }
            MatchManager.GetInstance().MatchGameOver(gameRecord);
        }

        public void OnGameResult()
        {
            Debug.Log("Game Result");
            //BackEndMatchManager.GetInstance().LeaveInGameRoom();

            if (GameManager.GetInstance().IsLobbyScene())
            {
                Debug.Log("Change State");
                GameManager.GetInstance().ChangeState(GameManager.GameState.MatchLobby);
            }
        }


        public void OnRecieve(MatchRelayEventArgs args)
        {
            
            if (args.BinaryUserData == null)
            {
                Debug.LogWarning(string.Format("빈 데이터가 브로드캐스팅 되었습니다.\n{0} - {1}", args.From, args.ErrInfo));
                // 데이터가 없으면 그냥 리턴
                return;
            }
            Message msg = DataParser.ReadJsonData<Message>(args.BinaryUserData);
            if (msg == null)
            {
                //Debug.Log("CHECKING_POINT#1");
                return;
            }
            // Host가 아닐 때 내가 보내는 패킷은 받지 않는다.
            if ( !MatchManager.GetInstance().IsHost() && args.From.SessionId == userPlayerIndex)
            {
                return;
            }

            if (players == null)
            {
                Debug.LogError("Players 정보가 존재하지 않습니다.");
                return;
            }
            switch (msg.type)
            {
                case Protocol.Type.StartCount:
                    StartCountMessage startCount = DataParser.ReadJsonData<StartCountMessage>(args.BinaryUserData);
                    Debug.Log("wait second : " + (startCount.time));
                    InGameUiManager.GetInstance().SetStartCount(startCount.time);
                    break;
                case Protocol.Type.GameStart:
                    InGameUiManager.GetInstance().SetStartCount(0, false);
                    GameManager.GetInstance().ChangeState(GameManager.GameState.InGame);
                    break;
                case Protocol.Type.GameEnd:
                    GameEndMessage endMessage = DataParser.ReadJsonData<GameEndMessage>(args.BinaryUserData);
                    SetGameRecord(endMessage.count, endMessage.sessionList);
                    GameManager.GetInstance().ChangeState(GameManager.GameState.Over);
                    break;

                case Protocol.Type.Key:
                    KeyMessage keyMessage = DataParser.ReadJsonData<KeyMessage>(args.BinaryUserData);
                    ProcessKeyEvent(args.From.SessionId, keyMessage);
                    break;

                case Protocol.Type.PlayerMove:
                    PlayerMoveMessage moveMessage = DataParser.ReadJsonData<PlayerMoveMessage>(args.BinaryUserData);
                    ProcessPlayerData(moveMessage);
                    break;

                case Protocol.Type.PlayerAttack:
                    PlayerAttackMessage attackMessage = DataParser.ReadJsonData<PlayerAttackMessage>(args.BinaryUserData);
                    ProcessPlayerData(attackMessage);
                    break;

                case Protocol.Type.PlayerDamaged:
                    PlayerDamegedMessage damegedMessage = DataParser.ReadJsonData<PlayerDamegedMessage>(args.BinaryUserData);
                    ProcessPlayerData(damegedMessage);
                    break;

                case Protocol.Type.PlayerNoMove:
                    PlayerNoMoveMessage noMoveMessage = DataParser.ReadJsonData<PlayerNoMoveMessage>(args.BinaryUserData);
                    ProcessPlayerData(noMoveMessage);
                    break;

                case Protocol.Type.GameSync:
                    GameSyncMessage syncMessage = DataParser.ReadJsonData<GameSyncMessage>(args.BinaryUserData);
                    ProcessSyncData(syncMessage);
                    break;

                default:
                    Debug.Log("Unknown protocol type");
                    return;
            }
        }
        public void OnRecieveForLocal(KeyMessage keyMessage)
        {
            ProcessKeyEvent(userPlayerIndex, keyMessage);
        }
        private void ProcessKeyEvent(SessionId index, KeyMessage keyMessage)
        {
            if (MatchManager.GetInstance().IsHost() == false)
            {
                //호스트만 수행
                return;
            }
            bool isMove = false;
            bool isAttack = false;
            bool isNoMove = false;

            int keyData = keyMessage.keyData;

            Vector3 moveVecotr = Vector3.zero;
            Vector3 attackPos = Vector3.zero;
            Vector3 playerPos = players[index].GetPosition();
            if ((keyData & KeyEventCode.MOVE) == KeyEventCode.MOVE)
            {
                players[index].EngineMovementAudio();
                moveVecotr = new Vector3(keyMessage.x, keyMessage.y, keyMessage.z);
                moveVecotr = Vector3.Normalize(moveVecotr);
                isMove = true;
            }
            if ((keyData & KeyEventCode.ATTACK) == KeyEventCode.ATTACK)
            {
                attackPos = new Vector3(keyMessage.x, keyMessage.y, keyMessage.z);
                players[index].Attack(attackPos);
                isAttack = true;
            }

            if ((keyData & KeyEventCode.NO_MOVE) == KeyEventCode.NO_MOVE)
            {
                //players[index].EngineIdleAudio();
                isNoMove = true;
            }

            if (isMove)
            {
                players[index].SetMoveVector(moveVecotr);
                PlayerMoveMessage msg = new PlayerMoveMessage(index, playerPos, moveVecotr);
                MatchManager.GetInstance().SendDataToInGame<PlayerMoveMessage>(msg);
            }
            
            if (isNoMove)
            {
                PlayerNoMoveMessage msg = new PlayerNoMoveMessage(index, playerPos);
                MatchManager.GetInstance().SendDataToInGame<PlayerNoMoveMessage>(msg);
            }

            if (isAttack)
            {
                PlayerAttackMessage msg = new PlayerAttackMessage(index, attackPos);
                MatchManager.GetInstance().SendDataToInGame<PlayerAttackMessage>(msg);
            }
        }

        private void ProcessAttackKeyData(SessionId session, Vector3 pos)
        {
            players[session].Attack(pos);
            PlayerAttackMessage msg = new PlayerAttackMessage(session, pos);
            MatchManager.GetInstance().SendDataToInGame<PlayerAttackMessage>(msg);
        }

        private void ProcessPlayerData(PlayerMoveMessage data)
        {
            if (MatchManager.GetInstance().IsHost() == true)
            {
                //호스트면 리턴
                return;
            }
            Vector3 moveVecotr = new Vector3(data.xDir, data.yDir, data.zDir);
            /*players[data.playerSession].SetPosition(data.xPos, data.yPos, data.zPos);
            players[data.playerSession].SetMoveVector(moveVecotr);*/
            // moveVector가 같으면 방향 & 이동량 같으므로 적용 굳이 안함
            if (!moveVecotr.Equals(players[data.playerSession].moveVector))
            {
                //players[data.playerSession].SetPosition(data.xPos, data.yPos, data.zPos);
                players[data.playerSession].SetMoveVector(moveVecotr);
                players[data.playerSession].EngineMovementAudio();
            }
        }
        private void ProcessPlayerData(PlayerNoMoveMessage data)
        {
            players[data.playerSession].SetPosition(data.xPos, data.yPos, data.zPos);
            players[data.playerSession].SetMoveVector(Vector3.zero);
            //players[data.playerSession].EngineIdleAudio();
        }

        private void ProcessPlayerData(PlayerAttackMessage data)
        {
            if (MatchManager.GetInstance().IsHost() == true)
            {
                //호스트면 리턴
                return;
            }
            players[data.playerSession].Attack(new Vector3(data.dir_x, data.dir_y, data.dir_z));
        }
        private void ProcessPlayerData(PlayerDamegedMessage data)
        {
            players[data.playerSession].Damaged(data.hit_damage);
            /*EffectManager.instance.EnableEffect(data.hit_x, data.hit_y, data.hit_z);*/
        }

        private void ProcessSyncData(GameSyncMessage syncMessage)
        {
            // 플레이어 데이터 동기화
            int index = 0;
            if (players == null)
            {
                Debug.LogError("Player Poll is null!");
                return;
            }
            foreach (var player in players)
            {
                var y = player.Value.GetPosition().y;
                player.Value.SetPosition(new Vector3(syncMessage.xPos[index], y, syncMessage.zPos[index]));
                player.Value.SetHealthUI();
                index++;
            }
            MatchManager.GetInstance().SetHostSession(syncMessage.host);
        }

        public bool IsMyPlayerMove()
        {
            return players[userPlayerIndex].isMove;
        }

        public bool IsMyPlayerRotate()
        {
            return players[userPlayerIndex].isRotate;
        }

        private void SetGameRecord(int count, int[] arr)
        {
            gameRecord = new Stack<SessionId>();
            // 스택에 넣어야 하므로 제일 뒤에서 부터 스택에 push
            for (int i = count - 1; i >= 0; i--)
            {
                gameRecord.Push((SessionId)arr[i]);
            }
        }

        public GameSyncMessage GetNowGameState(SessionId hostSession)
        {
            int numOfClient = players.Count;

            float[] xPos = new float[numOfClient];
            float[] zPos = new float[numOfClient];
            float[] hp = new float[numOfClient];
            bool[] online = new bool[numOfClient];
            int index = 0;
            foreach (var player in players)
            {
                xPos[index] = player.Value.GetPosition().x;
                zPos[index] = player.Value.GetPosition().z;
                hp[index] = player.Value.m_CurrentHealth;
                index++;
            }
            return new GameSyncMessage(hostSession, numOfClient, xPos, zPos, hp, online);
        }

        public Vector3 GetMyPlayerPos()
        {
            return players[userPlayerIndex].GetPosition();
        }

    }
}