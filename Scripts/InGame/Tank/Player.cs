using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Protocol;
using BackEnd.Tcp;
using BackEnd;
namespace PvpTank
{
    public partial class Player : MonoBehaviour
    {
        /*public enum SurfaceType
        {
            Opaque,
            Transparent
        }
        public enum BlendMode
        {
            Alpha,
            Premultiply,
            Additive,
            Multiply
        }*/

        private SessionId index = 0;
        private string nickName = string.Empty;
        private bool isMe = false;
        private Rigidbody m_Rigidbody;
        private TankShooting m_Tankshoot;
        private AudioListener m_AudioListener;
        private BoxCollider m_boxCollider;
        
        // 스테이터스
        public int hp { get; private set; } = 0;
        //private const int MAX_HP = 5;
        private bool isLive = false;
        private float coolTime = 0.0f;
        private float MAX_COOLTIME = 0.3f;
        private bool isHide = false;

        // UI or Camera setting
        public GameObject nameObject;
        public GameObject CameraRig;
        public GameObject m_gameObject;
        //public GameObject hpObject;
        public VirtualStick TESTONLY_vertualStick;
        public ShotButton TESTONLY_attackButton;
        private List<GameObject> hpUi;
        private readonly string playerCanvas = "PlayerCanvas";
        private readonly string Cameras = "CameraRig";

        // 애니메이터
        // private Animator anim;

        // 이동관련
        public bool isMove { get; private set; }
        public Vector3 moveVector { get; private set; }
        public bool isRotate { get; private set; }

        private float rotSpeed = 12.0f;
        private float moveSpeed = 12.5f;

        private GameObject playerModelObject;
        private Rigidbody rigidBody;

        void Awake()
        {
            m_boxCollider = GetComponent<BoxCollider>();
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Tankshoot = GetComponent<TankShooting>();
            m_AudioListener = GetComponent<AudioListener>();
            CameraRig = GameObject.FindGameObjectWithTag(Cameras);

        }
        void Start()
        {
            if (MatchManager.GetInstance() == null)
            {
                // 매칭 인스턴스가 존재하지 않을 경우 (인게임 테스트 용도)
                Initialize(true, SessionId.None, "testPlayer", 0);
            }
        }

        public void Initialize(bool isMe, SessionId index, string nickName, float rot)
        {
            this.isMe = isMe;
            this.index = index;
            this.nickName = nickName;
            var playerUICanvas = GameObject.FindGameObjectWithTag(playerCanvas);
            //nameObject = Instantiate(nameObject, Vector3.zero, Quaternion.identity, playerUICanvas.transform);
            //hpObject = Instantiate(hpObject, Vector3.zero, Quaternion.identity, playerUICanvas.transform);
            nameObject.GetComponentInChildren<TextMeshProUGUI>().text = nickName;

            if (this.isMe)
            {
                CameraRig.GetComponent<CameraControl>().m_DesiredPosition = this.transform;
                CameraRig.GetComponent<CameraControl>().m_rigidBody = this.m_Rigidbody;
                m_AudioListener.enabled = true;
                Debug.Log("FUCKING ITS ME!");
            }

            this.isLive = true;
            this.isMove = false;
            this.moveVector = new Vector3(0, 0, 0);
            this.isRotate = false;

            //hp
            /*hp = MAX_HP;
            hpUi = new List<GameObject>();
            for (int i = 0; i < 5; ++i)
            {
                hpUi.Add(hpObject.transform.GetChild(i + 5).gameObject);
                hpUi[i].SetActive(false);
            }*/

            playerModelObject = this.gameObject;
            playerModelObject.transform.rotation = Quaternion.Euler(0, rot, 0);
            m_gameObject = this.transform.GetChild(0).gameObject;

            rigidBody = this.GetComponent<Rigidbody>();

            ///nameObject.transform.position = GetNameUIPos();
            
            //hpObject.transform.position = GetHeartUIPos();
            if (MatchManager.GetInstance().nowModeType == MatchModeType.TeamOnTeam)
            {
                var teamNumber = MatchManager.GetInstance().GetTeamInfo(index);
                var mySession = Backend.Match.GetMySessionId();
                var myTeam = MatchManager.GetInstance().GetTeamInfo(mySession);

                if (teamNumber == myTeam)
                {
                    nameObject.GetComponent<TextMeshProUGUI>().color = new Color(0, 0, 1);
                    Debug.Log("myTeam : " + index);
                }
                else
                {
                    nameObject.GetComponent<TextMeshProUGUI>().color = new Color(1, 0, 0);
                    Debug.Log("enemyTeam : " + index);
                }
            }
        }

        #region 이동관련 함수
        /*
         * 변화량만큼 이동
         * 특정 좌표로 이동
         */
        public void SetMoveVector(Vector3 vector)
        {
            moveVector = vector;

            if (vector == Vector3.zero)
            {
                isMove = false;
            }
            else
            {
                isMove = true;
            }
        }

        public void Move()
        {
            Move(moveVector);
        }

        public void Move(Vector3 var)
        {
            if (!isLive)
            {
                return;
            }
            // 회전
            if (var.Equals(Vector3.zero))
            {
                isRotate = false;
            }

            else
            {
                if (Quaternion.Angle(playerModelObject.transform.rotation, Quaternion.LookRotation(var)) > Quaternion.kEpsilon)
                {
                    isRotate = true;
                }
                else
                {
                    isRotate = false;
                }
            }
            if (!isLive)
            {
                return;
            }
            //playerModelObject.transform.rotation = Quaternion.LookRotation(var);
            //var pos = gameObject.transform.position + playerModelObject.transform.forward * moveSpeed * Time.deltaTime;

            var pos = Vector3.forward * moveVector.z + Vector3.right * moveVector.x; // vertical / horizontal 
            if (pos == Vector3.zero) return;
            m_Rigidbody.AddForce(pos * moveSpeed * Time.fixedDeltaTime, ForceMode.VelocityChange);
            //m_Rigidbody.MovePosition(m_Rigidbody.position + pos);

            //Vector3 aimPos = new Vector3(virtualStick.GetHorizontalValue(), 0, virtualStick.GetVerticalValue());
            //var pos = new Vector3(virtualStick.GetHorizontalValue(), 0, virtualStick.GetVerticalValue());
            // 이동
            //Debug.Log("CURRENT VECTOR : " + movement);
            //SetPosition(movement);
            /*if (!isLive)
            {
                return;
            }*/
            //SetPosition(pos);
        }
        /*public void Rotate()
        {
            if (moveVector.Equals(Vector3.zero))
            {
                isRotate = false;
                return;
            }
            if (Quaternion.Angle(playerModelObject.transform.rotation, Quaternion.LookRotation(moveVector)) < Quaternion.kEpsilon)
            {
                isRotate = false;
                return;
            }
            playerModelObject.transform.rotation = Quaternion.Lerp(playerModelObject.transform.rotation, Quaternion.LookRotation(moveVector), Time.deltaTime * rotSpeed);
        }*/
        private void Rotate()
        {
            if (moveVector.Equals(Vector3.zero))
            {
                isRotate = false;
                return;
            }
            if (Quaternion.Angle(playerModelObject.transform.rotation, Quaternion.LookRotation(moveVector)) < Quaternion.kEpsilon)
            {
                isRotate = false;
                return;
            }

            playerModelObject.transform.rotation = Quaternion.Lerp(playerModelObject.transform.rotation, Quaternion.LookRotation(moveVector), Time.deltaTime * rotSpeed);
        }

        public void SetPosition(Vector3 pos)
        {
            if (!isLive)
            {
                return;
            }
            Debug.Log(pos);
            gameObject.transform.position = pos;
            //nameObject.transform.position = pos;
            m_Rigidbody.position = pos;
            //m_Rigidbody.MovePosition(m_Rigidbody.position + pos);
        }

        // isStatic이 true이면 해당 위치로 바로 이동
        public void SetPosition(float x, float y, float z)
        {
            if (!isLive)
            {
                return;
            }
            Vector3 pos = new Vector3(x, y, z);
            SetPosition(pos);
        }

        public Vector3 GetPosition()
        {
            return gameObject.transform.position;
        }

        public Vector3 GetRotation()
        {
            //return gameObject.transform.rotation;
            return gameObject.transform.rotation.eulerAngles;
        }
        #endregion

        public void Attack()
        {
            if (!isLive)
            {
                return;
            }
            if (coolTime > 0.0f)
            {
                return;
            }
            coolTime = MAX_COOLTIME;
            //Vector3 pos = this.transform.position + (this.transform.forward * 2);
            /*BulletManager.Instance.ShootBullet(pos, this.transform.forward);*/
            m_Tankshoot.Fire();
        }

        public void Attack(Vector3 target)
        {
            if (!isLive)
            {
                return;
            }
            if (coolTime > 0.0f)
            {
                return;
            }
            //StartAnimation(AnimIndex.stop);
            coolTime = MAX_COOLTIME;
            m_Tankshoot.Fire(target);
            /*target.y = this.transform.position.y;
            Vector3 dir = Vector3.Normalize(target - this.transform.position);
            Vector3 pos = this.transform.position + (dir * 2);*/
           /* BulletManager.Instance.ShootBullet(pos, dir);*/
        }

        public bool GetIsLive()
        {
            return isLive;
        }

        private void PlayerDie()
        {
            isLive = false;
            nameObject.SetActive(false);
            m_gameObject.SetActive(false);
            DisableRagdoll();
            m_boxCollider.enabled = false;
        }
        void DisableRagdoll()
        {
            m_Rigidbody.isKinematic = true;
            m_Rigidbody.detectCollisions = false;
        }

        void Update()
        {
            if (MatchManager.GetInstance() == null)
            {
                // 매칭 인스턴스가 존재하지 않는 경우 (인게임 테스트 용도)
                Vector3 tmp = new Vector3(TESTONLY_vertualStick.GetHorizontalValue(), 0, TESTONLY_vertualStick.GetVerticalValue());
                tmp = Vector3.Normalize(tmp);
                SetMoveVector(tmp);

                if (TESTONLY_attackButton.buttondown)
                {
                    Vector3 tmp2 = new Vector3(TESTONLY_vertualStick.GetHorizontalValue(), 0, TESTONLY_vertualStick.GetVerticalValue());
                    if (!tmp2.Equals(Vector3.zero))
                    {
                        tmp2 += GetPosition();
                        Attack(tmp2);
                    }
                }
            }

            if (!isLive)
            {
                return;
            }

            if (isMove)
            {
                Move();
            }
            if (isRotate)
            {
                Rotate();
            }

            if (coolTime > 0.0f)
            {
                coolTime -= Time.deltaTime;
            }

            if (Mathf.Abs(m_Rigidbody.velocity.x) <= 0.001f)
            {
                EngineIdleAudio();
            }

            nameObject.transform.localPosition = new Vector3(this.transform.localPosition.x,
                                                                nameObject.transform.localPosition.y,
                                                                    this.transform.localPosition.z);
            /*if (nameObject.activeSelf)
            {
                nameObject.transform.position = GetNameUIPos();
                //hpObject.transform.position = GetHeartUIPos();
            }*/
        }

        public SessionId GetIndex()
        {
            return index;
        }

        public bool IsMe()
        {
            return isMe;
        }

        public string GetNickName()
        {
            return nickName;
        }
        
    }
}