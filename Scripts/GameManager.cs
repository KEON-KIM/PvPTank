using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PvpTank
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;
        private static bool isCreate = false;

        #region Scene_name
        private const string LOGIN = "0. Login";
        private const string LOBBY = "1. Match";
        private const string READY = "2. LoadRoom";
        private const string INGAME = "3. InGame";
        #endregion

        #region Actions-Events
        public static event Action OnGameReady = delegate { }; // Play User Ready UI? (Action? Data?)
        public static event Action InGame = delegate { }; // Decrease Heart point? or Player Action Data? // INIT IN GAME
        public static event Action AfterInGame = delegate { }; // Result Window? // INIT IN GAME
        public static event Action OnGameOver = delegate { };  // INIT IN GAME
        public static event Action OnGameResult = delegate { }; // INIT IN GAME
        public static event Action OnGameReconnect = delegate { };

        private string asyncSceneName = string.Empty;
        private IEnumerator InGameUpdateCoroutine; // Not Exist Update() Function!

        public enum GameState { Login, MatchLobby, Ready, Start, InGame, Over, Result, Reconnect };
        private GameState gameState;
        #endregion
        //Used before : GameManager.Instance.Awake();
        //Current : GameManager.GetInstance().Awake();

        public static GameManager GetInstance()
        {
            if (instance == null)
            {
                Debug.LogError("Do not exist GameManager Object Instance.");
                return null;
            }
            return instance;
        }

        void Awake()
        {
            if (!instance)
            {
                instance = this;
            }

            Application.targetFrameRate = 60; // 60프레임 고정

            Screen.sleepTimeout = SleepTimeout.NeverSleep; // 게임중 화면슬립모드 해제
            InGameUpdateCoroutine = InGameUpdate(); // Update() => Coroutine

            DontDestroyOnLoad(this.gameObject); // GameManager is can not Destory
        }

        // Start is called before the first frame update
        void Start()
        {
            if (isCreate)
            {
                DestroyImmediate(gameObject, true);
                return;
            }
            gameState = GameState.Login;
            isCreate = true;
        }

        // InGameUpdate is called once per frame
        IEnumerator InGameUpdate()
        {
            while (true)
            {
                if (gameState != GameState.InGame)
                {
                    StopCoroutine(InGameUpdateCoroutine);
                    yield return null;
                }
                InGame();
                AfterInGame();
                yield return new WaitForSeconds(.1f); //1초 단위
            }
        }

        private void Login()
        {

        }

        private void MatchLobby(Action<bool> func)
        {
            
            
            if (func != null)
            {
                ChangeSceneAsync(LOBBY, func);
            }
            else
            {
                ChangeScene(LOBBY);
            }
            
        }

        private void GameReady()
        {
            Debug.Log("Game is Ready");
            ChangeScene(READY);
            OnGameReady();
        }

        private void GameStart()
        {
            //delegate 초기화
            InGame = delegate { };
            AfterInGame = delegate { };
            OnGameOver = delegate { };
            OnGameResult = delegate { };

            //OnGameStart();
            // 게임씬이 로드되면 Start에서 OnGameStart 호출
            ChangeScene(INGAME);
        }

        private void GameOver()
        {
            OnGameOver();
        }
        private void GameResult()
        {
            OnGameResult();
        }

        private void GameReconnect()
        {
            //delegate 초기화
            InGame = delegate { };
            AfterInGame = delegate { };
            OnGameOver = delegate { };
            OnGameResult = delegate { };

            OnGameReconnect();
            ChangeScene(INGAME);
            ChangeState(GameManager.GameState.InGame);
        }
        public void ChangeState(GameState state, Action<bool> func = null)
        {
            gameState = state;
            switch (gameState)
            {
                case GameState.Login:
                    Login();
                    break;
                case GameState.MatchLobby:
                    MatchLobby(func);
                    break;
                case GameState.Ready:
                    GameReady();
                    break;
                case GameState.Start:
                    GameStart();
                    break;
                case GameState.Over:
                    GameOver();
                    break;
                case GameState.Result:
                    GameResult();
                    break;
                case GameState.InGame:
                    // 코루틴 시작
                    StartCoroutine(InGameUpdateCoroutine);
                    break;
                case GameState.Reconnect:
                    GameReconnect();
                    break;
                default:
                    Debug.Log("Unknown State. Please Confirm current state");
                    break;
            }
        }
        public GameState GetGameState()
        {
            return gameState;
        }
        public bool IsLobbyScene()
        {
            return SceneManager.GetActiveScene().name == LOBBY;
        }

        private void ChangeScene(string scene)
        {
            if (scene != LOGIN  && scene != INGAME && scene != LOBBY && scene != READY)
            {
                Debug.Log("Unknown Scene");
                return;
            }
            Debug.Log("CURRENT SCENE :: " + scene);
            SceneManager.LoadScene(scene);
        }

        private void ChangeSceneAsync(string scene, Action<bool> func)
        {
            asyncSceneName = string.Empty;
            if (scene != LOGIN && scene != INGAME && scene != LOBBY && scene != READY)
            {
                Debug.Log("Unknown Scene");
                return;
            }
            asyncSceneName = scene;
            StartCoroutine("LoadScene", func);
        }

        private IEnumerator LoadScene(Action<bool> func)
        {
            var asyncScene = SceneManager.LoadSceneAsync(asyncSceneName);
            asyncScene.allowSceneActivation = true;

            bool isCallFunc = false;
            while (asyncScene.isDone == false)
            {
                if (asyncScene.progress <= 0.9f)
                {
                    func(false);
                }
                else if (isCallFunc == false)
                {
                    isCallFunc = true;
                    func(true);
                }
                yield return null;
            }
        }

    }
}