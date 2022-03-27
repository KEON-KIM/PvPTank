using System;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using static BackEnd.SendQueue;
/*using GooglePlayGames;
using GooglePlayGames.BasicApi;*/
using UnityEngine.SocialPlatforms;
namespace PvpTank
{
    public class ServerManager : MonoBehaviour
    {
        private static ServerManager instance;   // 인스턴스
        public bool isLogin { get; private set; }   // 로그인 여부

        private string tempNickName; // 설정할 닉네임 (id와 동일)
        public string userNickName { get; private set; } = string.Empty;  // 로그인한 계정의 닉네임
        public string userIndate { get; private set; } = string.Empty;    // 로그인한 계정의 inDate
        private Action<bool, string> loginSuccessFunc = null;

        private const string BackendError = "statusCode : {0}\nErrorCode : {1}\nMessage : {2}"; // Error Message

        void Awake()
        {
            if (instance != null)
            {
                Destroy(instance);
            }
            instance = this;

            DontDestroyOnLoad(this.gameObject);
        }

        public static ServerManager GetInstance()
        {
            if (instance == null)
            {
                Debug.LogError("Not exist ServerManager instnace.");
                return null;
            }

            return instance;
        }

        // Start is called before the first frame update
        void Start()
        {
            /*#if UNITY_ANDROID
                    PlayGamesClientConfiguration config = new PlayGamesClientConfiguration
                        .Builder()
                        .RequestServerAuthCode(false)
                        .RequestIdToken()
                        .Build();
                    PlayGamesPlatform.InitializeInstance(config);
                    PlayGamesPlatform.DebugLogEnabled = true;

                    PlayGamesPlatform.Activate();
            #endif*/
            isLogin = false;

            var bro = Backend.Initialize(true);

            if (bro.IsSuccess())
            {
                Debug.Log("Success to Initialize Backend : " + bro);
                #if UNITY_ANDROID
                            Debug.Log("GoogleHash - " + Backend.Utils.GetGoogleHash());
                #endif
                #if !UNITY_EDITOR
                            //안드로이드, iOS 환경에서만 작동
                            GetVersionInfo();
                #endif
            }
            else
            {
                Debug.LogError("Fail to Initialize Backend : " + bro);
            }
        }
        // Update is called once per frame
        void Update()
        {
            //비동기함수 풀링
            Backend.AsyncPoll();
        }

        private void GetVersionInfo()
        {
            Enqueue(Backend.Utils.GetLatestVersion, callback =>
            {
                if (callback.IsSuccess() == false)
                {
                    Debug.LogError("Failed to get the version information.\n" + callback);
                    return;
                }

                var version = callback.GetReturnValuetoJSON()["version"].ToString();

                Version server = new Version(version);
                Version client = new Version(Application.version);

                var result = server.CompareTo(client);
                if (result == 0)
                {
                // 0 이면 두 버전이 일치
                return;
                }
                else if (result < 0)
                {
                // 0 미만이면 server 버전이 client 이전 버전
                // 검수를 넣었을 경우 여기에 해당된다.
                // ex) 검수버전 3.0.0, 라이브에 운용되고 있는 버전 2.0.0, 콘솔 버전 2.0.0
                return;
                }
                else
                {
                // 0보다 크면 server 버전이 client 이후 버전
                if (client == null)
                    {
                    // 클라이언트가 null인 경우 예외처리
                    Debug.LogError("클라이언트 버전정보가 null 입니다.");
                        return;
                    }
                }

            // 버전 업데이트 팝업
            LoginUI.GetInstance().OpenUpdatePopup();
            });
        }
        

        public void BackendTokenLogin(Action<bool, string> func)
        {
            Enqueue(Backend.BMember.LoginWithTheBackendToken, callback =>
            {
                if (callback.IsSuccess())
                {
                    Debug.Log("Success Token Login");
                    loginSuccessFunc = func;

                    OnPrevBackendAuthorized();
                    return;
                }

                Debug.Log("Failed Token Login\n" + callback.ToString());
                func(false, string.Empty);
            });
        }

        // 커스텀 로그인
        public void CustomLogin(string id, string pw, Action<bool, string> func)
        {
            Enqueue(Backend.BMember.CustomLogin, id, pw, callback =>
            {
                if (callback.IsSuccess())
                {
                    Debug.Log("Success Custom Login");
                    loginSuccessFunc = func;

                    OnPrevBackendAuthorized();
                    return;
                }

                Debug.Log("Failed Custom Login\n" + callback);
                func(false, string.Format(BackendError,
                    callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
            });
        }

        public void CustomSignIn(string id, string pw, Action<bool, string> func)
        {
            tempNickName = id;
            Enqueue(Backend.BMember.CustomSignUp, id, pw, callback =>
            {
                if (callback.IsSuccess())
                {
                    Debug.Log("Success Custom Sign Up");
                    loginSuccessFunc = func;

                    OnPrevBackendAuthorized();
                    return;
                }
                Debug.LogError(id + pw);
                Debug.LogError("Failed Custom Sign Up\n" + callback.ToString());
                func(false, string.Format(BackendError,
                    callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
            });
        }

        public void UpdateNickname(string nickname, Action<bool, string> func)
        {
            Enqueue(Backend.BMember.UpdateNickname, nickname, bro =>
            {
            // 닉네임이 없으면 매치서버 접속이 안됨
            if (!bro.IsSuccess())
                {
                    Debug.LogError("Failed Create User Nickname\n" + bro.ToString());
                    func(false, string.Format(BackendError,
                        bro.GetStatusCode(), bro.GetErrorCode(), bro.GetMessage()));
                    return;
                }
                loginSuccessFunc = func;
                OnBackendAuthorized(); // Load UserInfo
        });
        }

        // 유저 정보 불러오기 사전작업
        private void OnPrevBackendAuthorized()
        {
            isLogin = true;

            OnBackendAuthorized();
        }

        // 실제 유저 정보 불러오기
        private void OnBackendAuthorized()
        {                                                    // Lambda
            Enqueue(Backend.BMember.GetUserInfo, callback =>
            {
                if (!callback.IsSuccess())
                {
                    Debug.LogError("Failed to get the user information.\n" + callback);
                    loginSuccessFunc(false, string.Format(BackendError,
                        callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
                    return;
                }
                Debug.Log("UserInfo\n" + callback);

                var info = callback.GetReturnValuetoJSON()["row"];
                if (info["nickname"] == null)
                {
                    LoginUI.GetInstance().ActiveNickNameObject();
                    return;
                }

                userNickName = info["nickname"].ToString();
                userIndate = info["inDate"].ToString();

                if (loginSuccessFunc != null)
                {
                    MatchManager.GetInstance().GetMatchList(loginSuccessFunc);
                }
            });
        }

        public void GuestLogin(Action<bool, string> func)
        {
            Enqueue(Backend.BMember.GuestLogin, callback =>
            {
                if (callback.IsSuccess())
                {
                    Debug.Log("Success Guest Login");
                    loginSuccessFunc = func;

                    OnPrevBackendAuthorized();
                    return;
                }

                Debug.Log("Failed Guest Login\n" + callback);
                func(false, string.Format(BackendError,
                    callback.GetStatusCode(), callback.GetErrorCode(), callback.GetMessage()));
            });
        }

        /* Friend module Function
        public void GetFriendList(Action<bool, List<Friend>> func)
        {
            Enqueue(Backend.Social.Friend.GetFriendList, 15, callback =>
            {
                if (callback.IsSuccess() == false)
                {
                    func(false, null);
                    return;
                }

                var friendList = new List<Friend>();

                foreach (LitJson.JsonData tmp in callback.Rows())
                {
                    if (tmp.Keys.Contains("nickname") == false)
                    {
                        continue;
                    }
                    Friend friend = new Friend();
                    friend.nickName = tmp["nickname"]["S"].ToString();
                    friend.inDate = tmp["inDate"]["S"].ToString();

                    friendList.Add(friend);
                }

                func(true, friendList);
            });
        }

        public void GetReceivedRequestFriendList(Action<bool, List<Friend>> func)
        {
            Enqueue(Backend.Social.Friend.GetReceivedRequestList, 15, callback =>
            {
                if (callback.IsSuccess() == false)
                {
                    func(false, null);
                    return;
                }

                var friendList = new List<Friend>();

                foreach (LitJson.JsonData tmp in callback.Rows())
                {
                    if (tmp.Keys.Contains("nickname") == false)
                    {
                        continue;
                    }
                    Friend friend = new Friend();
                    friend.nickName = tmp["nickname"]["S"].ToString();
                    friend.inDate = tmp["inDate"]["S"].ToString();

                    friendList.Add(friend);
                }

                func(true, friendList);
            });
        }

        public void RequestFirend(string nickName, Action<bool, string> func)
        {
            Enqueue(Backend.Social.GetUserInfoByNickName, nickName, callback =>
            {
                Debug.Log(callback);
                if (callback.IsSuccess() == false)
                {
                    func(false, callback.GetMessage());
                    return;
                }
                string inDate = callback.GetReturnValuetoJSON()["row"]["inDate"].ToString();
                Enqueue(Backend.Social.Friend.RequestFriend, inDate, callback2 =>
                {
                    Debug.Log(callback2);
                    if (callback2.IsSuccess() == false)
                    {
                        func(false, callback2.GetMessage());
                        return;
                    }

                    func(true, string.Empty);
                });
            });
        }

        public void AcceptFriend(string inDate, Action<bool, string> func)
        {
            Enqueue(Backend.Social.Friend.AcceptFriend, inDate, callback2 =>
            {
                if (callback2.IsSuccess() == false)
                {
                    func(false, callback2.GetMessage());
                    return;
                }

                func(true, string.Empty);
            });
        }

        public void RejectFriend(string inDate, Action<bool, string> func)
        {
            Enqueue(Backend.Social.Friend.RejectFriend, inDate, callback2 =>
            {
                if (callback2.IsSuccess() == false)
                {
                    func(false, callback2.GetMessage());
                    return;
                }

                func(true, string.Empty);
            });
        }
        */
    }
}