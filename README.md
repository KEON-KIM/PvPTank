# PvPTank
<h1> 1. 구조 (Structure)</h1>
  : SingleTone, 각 매니저는 instance화 한다.
  
  
<h2> 1. Back End (Unity.BackEnd) </h2>
  (1) ServerManager
    서버 접속과 서버의 현재 접속 상태를 위한 매니저
    
  (2) MatchManager
    매칭 룸을 생성하고, 생성 된 매칭 룸끼리 매칭을 위한 매니저
    
  (3) Protocol
    byte[]형식의 메세지를 위한 parameter들의 protocl 정의

<h2> 2. InApplication </h2>
  (1) GameManager
    게임의 상태를 저장하고, 핸들러를 통해 화면 전환을 위한 매니저

<h2> 3. InGame </h2>
  (1) WorldManager
    인게임의 상태를 저장하고, 자신의 위치를 MatchManager를 통해 메세지를 서버로 발신 및 수신등 통신을 위한 매니저
    
  (2) InputManager
    서버 동작정보를 전송하기 위한 매니저로, 입력을 실시간으로 받고 입력이 없다 판단되면 WorldManager는 메세지 전송을 하지 않는다. (메세지는 받음)
    
  (3) Player
    플레이어들의 탱크 동작과 상태 및 위치, 이벤트를 저장

<h1> 2. 동작 </h1>
  (1) Login
    어플리케이션 구동 시 ServerManager와 GameManager가 동작하여 서버상태를 나타내는 서버핸들러와 게임 핸들러가 시작 된다. (ServerManager, GameManager는 한 번 실행되면 어플리케이션 종료전 까지 종료 되지 않는다.)
    
    Login 오브젝트 내 ID.Text, Password.Text에 입력된 정보를 토대로 ServerManager를 통해 사용자 정보를 가져오거나 오류를 표출 한다. 만약, 정상적인 사용자 정보를 가져올 경우 핸들러들의 상태를 변경하고, Match Scene으로 이동한다.
    
  (2) Match Scene
    Match Scene 구동 시 MatchManager가 동작하며(InGame종료 이전까지 종료 되지 않는다.) ServerManager에 의해 MatchManager에 사용자 정보가 입력 되며 initialize된다.
    Match mode 버튼 클릭시 MatchManager는 매칭 할 수 있는 매칭 룸의 정보를 찾는다. (만약, 매칭 룸의 정보를 찾을 수 없으면 AIPlayer 샌드박스모드로 InGameRoom을 생성한다.) 매칭 성공시, GameManager와 ServerManager는 각각의 핸들러를 State.Load으로 변경하고, MatchManager는 접속된 Player User의 정보를 players Dictionary(<playerIndex, playerInfo(Message)>)에 담는다. 마지막으로, 변환 되는 핸들러로 인해 LoadScene으로 이동 되며 Match Scene이 종료된다.
    
  (3) Load Scene
    WorldManager의 구동과 동시에, initialize가 시작되고, MatchManager의 players dictionary를 통해 현재 접속 되어 있는 플레이어들의 정보를 나타내는 UI object contents를 생성한다. 또한, InGame내에서 사용할 Player의 탱크 오브젝트를 생성하고 WorldManager의 players를 새롭게 생성한다 <playerIndex, Player(Object)>.
  map과 탱크 오브젝트의 생성이 완료되면 ServerManager, GameManager 핸들러의 상태를 InGame으로 변경하고 게임 핸들러에 InGame Scene으로 이동 된다.
  
  (4) InGame
    WorldManager는 inputManager에 의해 메세지 전송을 한다. 만약, Input(Button, JoyStick)이 있다면 SendToMessage()를 통해 현재 자신의 정보를 상태에 따라 Message type을 변경하여 서버로 보낸다. 
    서버와 연결된 클라이언트의(host가 아닌) WorldManager는 Message를 모두 비동기화식으로 수신하며 수신된 Message들을 변환 작업 후 자신을 제외한 각 player 오브젝트에 적용한다. (만약, 현 플레이어가 host라면 수신 된 데이터를 LocalQueue에 저장하고 변경 되는 모든 이동 정보를 각 클라이언트에게 발신함.)
  게임이 종료 되면, ServerManager를 통해 유저 기록 정보가 최신화 되며, 게임과 서버 핸들러가 Match 상태로 변경되고, MatchManager가 초기화 된다. 그리고 WorldManager는 종료 된다.
