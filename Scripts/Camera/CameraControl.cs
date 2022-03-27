using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BackEnd.Tcp;


namespace PvpTank
{
    public class CameraControl : MonoBehaviour
    {
        private static CameraControl instance = null; // 인스턴스

        public static CameraControl GetInstance()
        {
            if (!instance)
            {
                Debug.LogError("Not Exist MatchManager Instance.");
                return null;
            }

            return instance;
        }
        public float m_DampTime = 0.2f;
        public float m_DampZoomOutTime = 2.8f;
        public float m_ScreenEdgeBuffer = 4f;
        public float m_MaxSize = 6.5f;
        public float m_MinSize = 1.5f;
        /*[HideInInspector]*/
        public Transform[] m_Targets;
        public SessionId userPlayerIndex;
        public Dictionary<SessionId, Player> players = new Dictionary<SessionId, Player>();
        private List<SessionId> gamers = new List<SessionId>();


        [SerializeField] private Camera m_Camera;

        private float m_ZoomSpeed;
        private int userIndex;
        private Vector3 m_MoveVelocity;
        private float MaxDistance = 5.5f;
        private int MaxUserAliveCount;
        private int UserScreenSize;
        private bool isLive;
        public Transform m_DesiredPosition;
        public Rigidbody m_rigidBody;


        private void Awake()
        {
            instance = this;
            m_Camera = GetComponentInChildren<Camera>();
        }

        private void FixedUpdate()
        {
            if(isLive)
            {
                Move();
                Zoom();
            }
                
            else
            {
                DeadMove();
            }
        }
        public void UserCameraInitialize()
        {
            isLive = true;
            UserScreenSize = Screen.width/2;
            if (players.Count <= 0)
            {
                return;
            }
            else
            {
                int index = 0;
                foreach(var sessionId in players)
                {
                    if(sessionId.Key == userPlayerIndex)
                    {
                        userIndex = index;
                        break;
                    }
                    index += 1;

                }
                MaxUserAliveCount = index;
            }
        }
        public void SetPlayerDie()
        {
            isLive = false;
        }

        private void Move()
        {
            //FindAveragePosition();
            transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition.position, ref m_MoveVelocity, m_DampTime);
        }
        private void DeadMove()
        {
            Vector3 touchStartPosition = Vector3.zero;
            Vector3 touchEndPosition = Vector3.zero;
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        touchStartPosition = touch.position;
                        break;

                    case TouchPhase.Ended:
                        touchEndPosition = touch.position;
                        break;
                }
                if (touch.position.x <= UserScreenSize) // left
                {
                    Debug.Log("TOUCH LEFT");
                    while (!players[gamers[userIndex]].GetIsLive())
                    {
                        userIndex = userIndex >= MaxUserAliveCount ? 0 : userIndex + 1;
                    }
                    m_DesiredPosition = players[gamers[userIndex]].transform;
                }
                else // right
                {
                    Debug.Log("TOUCH RIHGT");
                    while (!players[gamers[userIndex]].GetIsLive())
                    {
                        userIndex = userIndex < 0 ? MaxUserAliveCount : userIndex - 1;
                    }
                    m_DesiredPosition = players[gamers[userIndex]].transform;
                }
            }
            if (Vector3.Distance(transform.position, m_DesiredPosition.position) > MaxDistance) // more fater!
            {
                transform.position = m_DesiredPosition.position;
            }
            else
            {
                transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition.position, ref m_MoveVelocity, m_DampTime);
            }
        }
        

        /*private void FindAveragePosition()
        {
            Vector3 averagePos = new Vector3();
            int numTargets = 0;

            for (int i = 0; i < m_Targets.Length; i++)
            {
                if (!m_Targets[i].gameObject.activeSelf)
                    continue;

                averagePos += m_Targets[i].position;
                numTargets++;
            }

            if (numTargets > 0)
                averagePos /= numTargets;

            averagePos.y = transform.position.y;

            m_DesiredPosition = averagePos;
        }*/


        private void Zoom()
        {
            if (Mathf.Abs(m_rigidBody.velocity.x) < m_MinSize)
            {
                m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, m_MinSize, ref m_ZoomSpeed, m_DampZoomOutTime);
            }
            else
            {
                m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, Mathf.Abs(m_rigidBody.velocity.x), ref m_ZoomSpeed, m_DampZoomOutTime - 1.4f);
            }
            /*if (Mathf.Abs(m_rigidBody.velocity.x) < m_MinSize)
            {
                Debug.Log("#1");
                m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, m_MinSize, ref m_ZoomSpeed, m_DampZoomOutTime);
            }

            else if (Mathf.Abs(m_rigidBody.velocity.x) < m_MaxSize)
            {
                Debug.Log("#2");
                m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, Mathf.Abs(m_rigidBody.velocity.x), ref m_ZoomSpeed, m_DampZoomOutTime - 1.4f);
            }

            else
            {
                Debug.Log("#3");
                m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, m_MaxSize, ref m_ZoomSpeed, m_DampZoomOutTime - 1.4f);
            }*/

        }


        private float FindRequiredSize()
        {
            Vector3 desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition.position);

            float size = 0f;

            for (int i = 0; i < m_Targets.Length; i++)
            {
                if (!m_Targets[i].gameObject.activeSelf)
                    continue;

                Vector3 targetLocalPos = transform.InverseTransformPoint(m_Targets[i].position);

                Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;

                size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));

                size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / m_Camera.aspect);
            }

            size += m_ScreenEdgeBuffer;

            size = Mathf.Max(size, m_MinSize);

            return size;
        }
        /*

        public void SetStartPositionAndSize()
        {
            FindAveragePosition();

            //transform.position = m_DesiredPosition;

            m_Camera.orthographicSize = FindRequiredSize();
        }*/
    }
}