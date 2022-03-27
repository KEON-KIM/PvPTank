using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BackEnd;
using BackEnd.Tcp;
using TMPro;

namespace PvpTank
{
    public class LoadRoomUI : MonoBehaviour
    {
        //public GameObject[] userObject = new GameObject[4];
        public GameObject userPrefab;
        public GameObject UserListParent;
        public GameObject[] TankGameObjects = new GameObject[4];
        private List<GameObject> userObject = new List<GameObject>();
        private int numOfClient = -1;
        private const string MMR_RECORD = "MMR : {0}";
        private const string POINT_RECORD = "POINT : {0}";
        private const string NUM_RECORD = "매치수 : {0}";
        private const string WIN_RECORD = "승 : {0}";
        private const string DEFEAT_RECORD = "패 : {0}";

        void Awake()
        {
            Debug.Log("ENTER LOADROOM SCENE");
        }
        void Start()
        {
            var matchInstance = MatchManager.GetInstance();
            if (matchInstance == null)
            {
                return;
            }

            numOfClient = matchInstance.gameRecords.Count;

            if (numOfClient <= 0)
            {
                Debug.LogError("numOfClient가 0이하입니다.");
                return;
            }
            for(int i = 0; i < numOfClient; i++)
            {
                GameObject user = GameObject.Instantiate(userPrefab, Vector3.zero, Quaternion.identity, UserListParent.transform);
                user.transform.localPosition = new Vector3(user.transform.localPosition.x, user.transform.localPosition.y, 0.0f);
                userObject.Add(user);
            }

            int index = 0;
            foreach (var record in matchInstance.gameRecords.OrderByDescending(x => x.Key))
            {
                var name = record.Value.m_nickname;
                string score = string.Empty;

                if (matchInstance.nowMatchType == MatchType.MMR)
                {
                    score = string.Format(MMR_RECORD, record.Value.m_mmr);
                }
                else if (matchInstance.nowMatchType == MatchType.Point)
                {
                    score = string.Format(POINT_RECORD, record.Value.m_points);
                }

                var data = userObject[index++].GetComponentsInChildren<TextMeshProUGUI>();

                data[0].text = name;    
                data[1].text = score;
                data[2].text = string.Format(NUM_RECORD, record.Value.m_numberOfMatches);
                data[3].text = string.Format(WIN_RECORD, record.Value.m_numberOfWin);
                data[4].text = string.Format(DEFEAT_RECORD, record.Value.m_numberOfDefeats);
               
                /*if (matchInstance.nowModeType == MatchModeType.TeamOnTeam)
                {
                    var teamNumber = matchInstance.GetTeamInfo(record.Key);
                    var mySession = Backend.Match.GetMySessionId();
                    var myTeam = matchInstance.GetTeamInfo(mySession);

                    if (teamNumber == myTeam)
                    {
                        data[0].color = new Color(0, 0, 1);
                    }
                    else
                    {
                        data[0].color = new Color(1, 0, 0);
                    }
                }*/
            }
        }

        void Update()
        {
            foreach(GameObject player in TankGameObjects)
                player.transform.Rotate(0.0f, 2.5f, 0.0f, Space.Self);
        }
    }
}