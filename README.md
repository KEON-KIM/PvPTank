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

로그인 이후, **Match Scene**으로 넘어가며 **MatchManager**의 동작이 시작된다. 이후, **InGame Scene**에서 인게임이 종료됨과 동시에 **MatchManager**의 동작이 종료된다.

##### 동작방식    

**Matching** 버튼 클릭시 유저는 매칭 룸을 생성하게 되며, 동시에 유저는 매칭 서버와 접속을 요청하게 된다. 접속이 정상 처리 될 경우, 매칭 룸의 정보가 UI로 나타나며 현재 게임 옵션을 확인할 수 있다. **Start** 버튼 클릭시 유저는 매칭 신청을 서버에게 요청하며, 매칭 서버는 만들어진 매칭 룸끼리 매칭을 이루어준다. 매칭이 정상 처리 될 경우, **Ingame Scene**으로 전환되고 유저들이 보내는 메세지를 받아 처리 한다. **InputManager**에 의해 변경되는 정보가 있을 경우에 메세지를 전달한다. 만약, 게임이 종료 되어 종료 요청이 들어오면 게임 결과창을 UI에 나타내며 동시에 매칭 서버와 접속을 종료한다. 



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

​	유저가 움직일 때, 유저의 위치 정보가 변경 되었을 경우 전송되는 메세지로, 유저의 SessionID, **위치** 정보를 담고 있다.

**- StartCountMessage**

​	게임이 시작되기 전 카운트 다운을 접속 된 모든 유저에게 브로드캐스팅 하기위한 메세지로, **카운트** 정보를 담고 있다.

**- GameEndMessage**

​	게임이 종료될 때 모든 유저에게 브로드캐스팅하기 위한 메세지로, UI로 나타낼 유저들의 **정보 리스트**를 담고 있다.



##### (3) DataParser

서버에 보내거나 받은 메세지를 읽기 위해 위해 메세지를 인코딩/ 디코딩 작업을 한다. 

( 서버 - 클라이언트 : string[] -> byte[] // 클라이언트 - 서버 byte[] -> string[] ) 

##### 

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

##### (2) InputManager





## 2. 동작

![]()

1)Login

2)Lobby(MatchRoom)

3)Loading

4)InGame

## 3. 참고

@Reference BACKEND SDK Tutorial <https://developer.thebackend.io/unity3d/realtime/matchMake/tutorial/>  
@Reference UNITY Tutorial <https://learn.unity.com/project/tanks-tutorial>

​	
