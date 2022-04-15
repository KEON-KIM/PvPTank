# PvPTank

## 1. 구조 

SingleTone, 각 매니저들은 모두 인스턴스화 한다.

#### 1) BackEnd (using BACKEND SDK)



##### (1) ServerManager

서버와 클라이언트간 로그인 접속 및 회원가입을 위한 **API**, 현재 서버 접속 상태를 확인할 수 있다.

##### 실행 주기

어플리케이션 시작과 동시에 실행되며, 어플리케이션이 종료되기 전까지 종료 되지 않는다.

##### 동작방식

Login 버튼 클릭시 사용자의 ID와 PW가 서버에 전달되어 로그인 요청이 이루어지고, 로그인이 활성화 되었을 경우 **토큰**을 반환하여 어플리케이션이 종료되더라도 일정시간동안 재로그인 없이 **로그인활성화**가 가능하다. 마찬가지로, 회원 가입의 경우도 Sign Up 버튼 클릭시 사용자의 ID와 PW 그리고 닉네임이 서버에 전달되어 회원 가입 요청이 이루어진다.



##### (2) MatchManager

서버와 클라이언트간 매칭과 인게임 메세지 전달을 위한 **API**, 유저가 만든 매칭룸간 매칭이 이루어지며 접속 된 매칭 서버에서 메세지를 주고 받는다.

##### 실행주기

ServerManager와 동일하게 어플리케이션 시작과 동시에 실행되며 어플리케이션 종료 전까지 종료되지 않지만 Match Scene 이후 동작한다. 

##### 동작방식    

**Login** 버튼클릭 이후에 **MatchManager**는 이벤트핸들러를 설정하며 본격적으로 실행 된다. **Matching** 버튼 클릭시 유저는 매칭 룸을 생성하게 되며, 동시에 유저는 매칭 서버와 접속을 요청하게 된다. 접속이 정상 처리 될 경우, 매칭 룸의 정보가 UI로 나타나며 현재 게임 옵션을 확인할 수 있다. **Start** 버튼 클릭시 유저는 매칭 신청을 서버에게 요청하며, 매칭 서버는 만들어진 매칭 룸끼리 매칭을 이루어준다. 매칭이 정상 처리 될 경우, **Ingame Scene**으로 전환되고 유저들이 보내는 메세지를 받아 처리 한다. **InputManager**에 의해 변경되는 정보가 있을 경우에 메세지를 전달한다. 만약, 게임이 종료 되어 종료 요청이 들어오면 게임 결과창을 UI에 나타내며 동시에 매칭 서버와 접속을 종료한다. 



#### 2) In Application

##### (1) GameManager

현재 진행중인 게임의 상태를 저장하고 처리하며, 핸들러를 통해 Scene을 전환하는 매니저. 

##### 실행주기

ServerManager와 동일하게 어플리케이션 시작과 동시에 실행되며, 어플리케이션이 종료되기 전까지 종료되지 않는다.

##### 동작방식

시작과 동시에 각 Scene에서 사용될 **EventHandler**들을 초기화하며, Scene전환 함수가 호출 되면 Scene을 전환하고 해당 Scene에 필요한 GameManager의 State정보를 변경한다.



##### (2) Protocol

유저간 서버를 통해 주고 받는 메세지의 정보를 정의한다.

##### 동작방식

주고 받는 인게임 메세지의 타입 다음과 같다.

 **- AttackMessage**

​	유저가 공격 버튼을 누를 시 전송 되는 메세지로, 공격 하는 유저의 SessionID, **방향** 정보를 담고있다.

**- DamageMessage**

​	유저가 공격 당할 경우 전송 되는 메세지로, 공격 받는 유저의 SessionID, **위치** 정보를 담고 있다.

**- NoMoveMessage**

​	유저가 움직이지 않을 때, 유저의 위치 정보가 변경 되지 않을 경우 전송 되는 메세지로, 유저의 SessionID, **위치** 정보를 담고 있다.

**- MoveMessage**

​	유저가 움직일 때, 유저의 위치 정보가 변경 되었을 경우 전송되는 메세지로, 유저의 SessionID, **위치**, **방향** 정보를 담고 있다.

**- StartCountMessage**

​	게임이 시작되기 전 카운트 다운을 접속 된 모든 유저에게 브로드캐스팅 하기위한 메세지로, **카운트** 정보를 담고 있다.

**- GameEndMessage**

​	게임이 종료될 때 모든 유저에게 브로드캐스팅하기 위한 메세지로, UI로 나타낼 유저들의 **정보 리스트**를 담고 있다.



##### (3) DataParser

서버에 보내거나 받은 메세지를 읽기 위해 위해 메세지를 인코딩/ 디코딩 작업을 한다. 

( 서버 - 클라이언트 : string[] -> byte[] // 클라이언트 - 서버 byte[] -> string[] ) 



#### 3) In Game 

##### (1) WorldManager

유저의 업데이트 정보를 API를 통해 서버로 발신하거나, 수신 된 메세지를 통해 인게임 정보를 수시로 업데이트하는 매니저. 

##### 실행 주기

Loading Scene에서 InGame Scene으로 전환되면서 실행되고, 인게임이 종료됨과 동시에 종료 된다.

##### 동작 방식

생성과 동시에 게임을 초기화한다. 플레이어 정보(체력, 위치, 공격력, 닉네임 등), StartCountSetting, GameManager의 InGame.EventHandler 등

게임이 진행되면, MatchManager에 의해 정해진 HOST는 타 유저가 불러오는 메세지를 OnReceive()를 통해 처리하지 않고 LocalQue에 담아놓고 처리한다.

만약,  로컬 큐에 담겨 있는 타 유저의 메세지 타입에 따라 ProcessPlayerData()를 통해 처리하며, 해당 메세지안의 정보가 현재 로컬에서의 해당 타 유저와의 정보가 동일하다면



##### (2) InGameUIManager

게임에 남은 유저 수 및 카운트 다운 UI를 업데이트 하는 매니저. 또한, 서버와 연결이 끊긴 유저가 있을 경우 Reconnection UI를 표시 한다.

**실행 주기**

**WorldManager**와 동일하게 InGame Scene으로 전환 되면서 실행되고, 인게임이 종료됨과 동시에 종료 된다.

**동작 방식**

**WorldManager**에 의해 호출 되며, 게임의 시작과 동시에 **SetStartCount()**로 게임의 시작을 알린다. 이후 게임이 진행됨에 따라서 유저 수가 감소한다면(플레이어가 죽을 경우) **SetScoreBoard()**를 호출하여 현재 남아 있는 유저 수를 업데이트한다.



##### (3) InputManager

유저의 조이스틱 정보를 서버에 전송시키거나, 유저의 탱크를 알맞게 업데이트 시키기 위한 매니저이다.

##### 실행 주기

**WorldManager**와 동일하게 InGame Scene으로 전환 되면서 실행되고, 인게임이 종료됨과 동시에 종료 된다.

**동작 방식**

**GameManager**의 Ingame() 델리게이트를 호출하는 **GameManager**의 **InGameUpdate** 코루틴에 의해 매 프레임 호출 된다. 인풋 입력이 있는 순간마다 서버에게 **SendDataToInGame()**를 통해 **KeyMessage**를 전달케 한다. 





## 2. 동작

![](C:\Users\srihw\OneDrive\바탕 화면\PvPTank.png)

#### 1) Login Scene

- ServerManager와 GameManager, 그리고 로그인에 사용될 UI를 관리할 LoginUIManager를 인스턴스화 하고, 유저의 접속을 기다린다.

- 유저가 로그인 또는 회원가입을 통해 서버접속을 원할 때 SeverManager는 해당 접근을 확인하고 승인 및 거절한다.

- ServerManager를 통해 Server의 승인이 되었을 경우 해당 유저에게 Login 토큰을 발행하여 일정 시간 자유롭게 로그인할 수 있는 권한을 주며, 게임매니저는 다음 Lobby Scene으로 전환한다.

  

#### 2) Match Scene(Lobby)

- Match Scene으로 전환되며, **MatchManager**의 동작 시작을 알리는 **Initilalize()**와 MatchManager와 매칭 룸을 생성할 때 사용될 UI를 관리할 **RoomUIManager**를 인스턴스화 하고, MatchManager를 통해 **Match Server**와의 접속 후 유저의 명령을 기다린다.

- **Matching** 버튼 클릭시 유저는 매칭 룸을 생성하게 되며, 동시에 유저는 매칭 서버와 접속을 요청하게 된다. 접속이 정상 처리 될 경우, 서버는 매칭 룸의 토큰을 발급하고, 매칭 룸의 환경설정이 RoomUIManager에 의해 UI로 표시된다.

- **Start** 버튼 클릭시 유저는 매칭 신청을 서버에게 요청하며, 매칭 서버는 만들어진 매칭 룸끼리 매칭룸 토큰을 사용해 매칭한다. 만약, 활성화 되어있는 룸 토큰이 없을 경우 샌드박스(인공지능) 모드로 진행된다.

-  매칭이 정상 처리 될 경우, **Loading Scene**으로 전환되며, **InGame Server**와의 접속을 요청한다.

  

#### 3) Loading Scene

- InGame Server와의 정상 접속 처리 후, 서버내에 접속 된 유저들의 정보를 **SessionID**을 통해 유저 카드를 생성하고 UIManager에 통해 UI로 표시한다.

- Scene이 유지된 채, **InGame Scene**의 설정이 **완료될 때까지** **기다린다**.

  

#### 4) InGame Scene

- **WorldManager**와 **InputManager**를 생성하고, InGame Scene에서 사용될 UI를을 처리하는 InGameUIManager를 인스턴스화 한다.
- 조이스틱의 인풋이 있다면, **GameManager.InGameUpdate()** 코루틴에 의해 **KeyMessage**를 생성 후 MatchManager를 통해 전송한다. 만약, 클라이언트가 **호스트**일 경우 **localQueue**에 Enque한다. 전송 된 KeyMessage들은 **모든** 클라이언트들에게 **브로드캐스트** 된다.
- **KeyMessage**를 받은 클라이언트가 **호스트**라면,**WorldManager.OnReceiveForLocal(**)을 통해 처리하고  **MoveMessage, AttackMessage, DamageMessage, NoMoveMessage**로 재가공하여 모든 클라이언트들에게 메세지를 브로드캐스트 한다. 해당 TypeMessage를 받은 **비호스트**들은 **WorldManager.OnReceive()**를 통해 처리 한다.
- 게임이 종료 되면, 게임의 결과창이 나타난다. 이 때, 결과창은 DieEvent에서 담아두었던 Stack에서 Pop하여 생성한다.
- **ServerManager**를 통해 유저의 정보가 최신화 되고, MatchManager가 초기화 되며 **Match Scene**으로 이동한다.





## 3. 참고

@Reference BACKEND SDK Tutorial <https://developer.thebackend.io/unity3d/realtime/matchMake/tutorial/>  
@Reference UNITY Tutorial <https://learn.unity.com/project/tanks-tutorial>

​	