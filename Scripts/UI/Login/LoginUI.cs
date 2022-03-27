using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battlehub.Dispatcher;
using TMPro;

namespace PvpTank
{
    public class LoginUI : MonoBehaviour
    {
        private static LoginUI instance;    // 인스턴스

        public GameObject mainTitle;
        public GameObject subTitle;
        public GameObject touchStart;
        public GameObject loginObject;
        public GameObject customLoginObject;
        public GameObject signUpObject;
        public GameObject errorObject;
        public GameObject nicknameObject;
        public GameObject updateObject;
        public GameObject LobbyObject;

        private TMP_InputField[] loginField; // Login Field / 0 : ID, 1 : PW
        private TMP_InputField[] signUpField; // Sign Up Field / 0 : ID, 1 : PW
        private TMP_InputField nicknameField; //Nick Name Field
        private TextMeshProUGUI errorText; // All Error Message
        private GameObject loadingObject; // rotation loading Object (where is this gameObject?)
        private FadeAnimation fadeObject; // ?
                                          // Start is called before the first frame update

        private const byte ID_INDEX = 0;
        private const byte PW_INDEX = 1;
        private const string VERSION_STR = "Ver {0}";

        const string PlayStoreLink = "market://details?id=io.thebackend.backendMatch"; // download Android market link.

        void Awake()
        {
            instance = this;
        }

        public static LoginUI GetInstance()
        {
            if (instance == null)
            {
                Debug.LogError("Do not Exist LoginUI Object.");
                return null;
            }
            return instance;
        }

        void Start()
        {   
            //Setting Init UI Objects
            errorText = errorObject.GetComponentInChildren<TextMeshProUGUI>();
            touchStart.SetActive(true);
            mainTitle.SetActive(true);

            loginObject.SetActive(false);
            customLoginObject.SetActive(false);
            signUpObject.SetActive(false);
            errorObject.SetActive(false);
            nicknameObject.SetActive(false);

            loginField = customLoginObject.GetComponentsInChildren<TMP_InputField>();
            signUpField = signUpObject.GetComponentsInChildren<TMP_InputField>();
            nicknameField = nicknameObject.GetComponentInChildren<TMP_InputField>();

            subTitle.GetComponentInChildren<TextMeshProUGUI>().text = string.Format(VERSION_STR, Application.version);

            loadingObject = GameObject.FindGameObjectWithTag("Loading");
            loadingObject.SetActive(false);

            
            var fade = GameObject.FindGameObjectWithTag("Fade");
            if (fade != null)
            {
                fadeObject = fade.GetComponent<FadeAnimation>();
            }

            /*var google = loginObject.transform.GetChild(0).gameObject;
            var apple = loginObject.transform.GetChild(1).gameObject;
    #if UNITY_ANDROID
            google.SetActive(true);
            apple.SetActive(false);
    #elif UNITY_IOS
            google.SetActive(false);
            apple.SetActive(true);
    #endif*/
        }
        public void OpenUpdatePopup()
        {
            updateObject.SetActive(true);
        }

        public void OpenStoreLink()
        {
#if UNITY_ANDROID
        Application.OpenURL(PlayStoreLink);
#elif UNITY_IOS
        Application.OpenURL(AppStoreLink);
#else
            Debug.LogError("Do not surportted Platform");
#endif
            Debug.Log("Open Store link Button Click");
        }

        public void TouchStart() // Touch Start Button
        {
            // 업데이트 팝업이 떠있으면 진행 X
            /*
            if (updateObject.activeSelf == true)
            {
                return;
            }*/

            loadingObject.SetActive(true); // Loading Object Enable
            ServerManager.GetInstance().BackendTokenLogin((bool result, string error) =>
            {
                Dispatcher.Current.BeginInvoke(() =>
                {
                    if (result)
                    {
                        ChangeLobbyScene();
                        MatchManager.GetInstance().SettingHanddler();
                        return;
                    }
                    loadingObject.SetActive(false); // Loading object Disable
                if (!error.Equals(string.Empty))
                    {
                        errorText.text = "Failed to get User Data\n\n" + error;
                        errorObject.SetActive(true);
                        return;
                    }
                    touchStart.SetActive(false);
                    loginObject.SetActive(true);
                    
                });
            });
        }

        public void Login() // Touch User Login Button
        {
            if (errorObject.activeSelf)
            {
                return;
            }
            string id = loginField[ID_INDEX].text;
            string pw = loginField[PW_INDEX].text;

            if (id.Equals(string.Empty) || pw.Equals(string.Empty))
            {
                errorText.text = "ID 혹은 PW 를 먼저 입력해주세요.";
                errorObject.SetActive(true);
                return;
            }

            loadingObject.SetActive(true);
            ServerManager.GetInstance().CustomLogin(id, pw, (bool result, string error) =>
            {
                Dispatcher.Current.BeginInvoke(() =>
                {
                    if (!result)
                    {
                        loadingObject.SetActive(false);
                        errorText.text = "로그인 에러\n\n" + error;
                        errorObject.SetActive(true);
                        return;
                    }
                    ChangeLobbyScene();
                    MatchManager.GetInstance().SettingHanddler();
                });
            });
        }
        
        public void SignUp()
        {
            if(errorObject.activeSelf)
            {
                return;
            }
            string id = signUpField[ID_INDEX].text;
            string pw = signUpField[PW_INDEX].text;

            if(id.Equals(string.Empty) || pw.Equals(string.Empty))
            {
                errorText.text = "Please Checking your ID or Password";
                errorObject.SetActive(true);
                return;
            }

            loadingObject.SetActive(true);
            ServerManager.GetInstance().CustomSignIn(id, pw, (bool result, string error) =>
            {
                Dispatcher.Current.BeginInvoke(() =>
                {
                    if(!result)
                    {
                        loadingObject.SetActive(false);
                        errorText.text = "Error : You can't sign up\n\n"+ error;
                        errorObject.SetActive(true);
                        return;
                    }
                    ActiveNickNameObject();
                });
            });
        }
        public void ActiveNickNameObject()
        {
            Dispatcher.Current.BeginInvoke(() =>
            {
                mainTitle.SetActive(false);
                touchStart.SetActive(false);
                subTitle.SetActive(false);
                loginObject.SetActive(false);
                customLoginObject.SetActive(false);
                signUpObject.SetActive(false);
                errorObject.SetActive(false);
                loadingObject.SetActive(false);
                nicknameObject.SetActive(true);
            });
        }
        public void UpdateNickName()
        {
            if (errorObject.activeSelf)
            {
                return;
            }
            string nickname = nicknameField.text;
            if (nickname.Equals(string.Empty))
            {
                errorText.text = "Input your nickname in inputField";
                errorObject.SetActive(true);
                return;
            }
            loadingObject.SetActive(true);
            ServerManager.GetInstance().UpdateNickname(nickname, (bool result, string error) =>
            {
                Dispatcher.Current.BeginInvoke(() =>
                {
                    if(!result)
                    {
                        loadingObject.SetActive(false);
                        errorText.text = "Error : Create nickname";
                        errorObject.SetActive(true);
                        return;
                    }
                    ChangeLobbyScene();
                });
            });
        }
        /* public void GoogleFederation() // Google Login?
         {
             if (errorObject.activeSelf)
             {
                 return;
             }

             loadingObject.SetActive(true);
             ServerManager.GetInstance().GoogleAuthorizeFederation((bool result, string error) =>
             {
                 Dispatcher.Current.BeginInvoke(() =>
                 {
                     if (!result)
                     {
                         loadingObject.SetActive(false);
                         errorText.text = "Error Login\n\n" + error;
                         errorObject.SetActive(true);
                         return;
                     }
                     ChangeLobbyScene();
                 });
             });
         }

        public void GuestLogin() // BackEndServer Guest Login
        {
            if (errorObject.activeSelf)
            {
                return;
            }

            loadingObject.SetActive(true);
            ServerManager.GetInstance().GuestLogin((bool result, string error) =>
            {
                Dispatcher.Current.BeginInvoke(() =>
                {
                    if (!result)
                    {
                        loadingObject.SetActive(false);
                        errorText.text = "Error Login\n\n" + error;
                        errorObject.SetActive(true);
                        return;
                    }
                    ChangeLobbyScene();
                });
            });
        }*/

        
        public void ChangeLobbyScene()
        {
            if (fadeObject != null)
            {
                GameManager.GetInstance().ChangeState(GameManager.GameState.MatchLobby, (bool isDone) =>
                {
                    Dispatcher.Current.BeginInvoke(() => loadingObject.transform.Rotate(0, 0, -10));
                    if (isDone)
                    {
                        fadeObject.ProcessFadeOut();
                    }
                });
            }
            else
            {
                GameManager.GetInstance().ChangeState(GameManager.GameState.MatchLobby);
            }
        }

    }
}