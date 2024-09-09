using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using NUnit.Framework.Constraints;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    [Header("Game Settings")]
    // 게임 버전 정의 v1.0 2.0 ~~~
    [SerializeField] private const string version = "1.0";
    // 한 번 설정하면 바꿀 수 없도록 const 로 지정

    // 유저명을 입력
    [SerializeField] private string nickName = "DeBe";

    [Header("UI")] // 눈에 잘 띄게 해주기 위해서 헤더를 달아줌
    [SerializeField] private TMP_InputField nickNameIf;
    [SerializeField] private TMP_InputField roomNameIf;

    [Header("Button")]
    [SerializeField] private Button loginButton;
    [SerializeField] private Button makeRoomButton;

    [Header("Room List")]
    // 룸 프리펩을 불러오기
    public GameObject roomPrefab;
    // 룸 프리펩을 생성할 부모 객체 선택 보통 GameObject가 아닌 Transform 으로 선택한다
    // 아마도 부모 쪽으로 이동하니까? 쓰는듯
    public Transform contentTr;
    // 룸 목록을 저장하기 위한 딕셔너리 키+밸류 일종의 자료형태 <Key, Object>
    // 새로운 룸을 만들 때 이전에 같은 이름의 룸이 있는지 확인하기도 해야하니까 만들어진 프리펩을 확인
    private Dictionary<string, GameObject> roomDict = new Dictionary<string, GameObject>();

    void Awake()
    {
        // 게임 버전을 설정
        PhotonNetwork.GameVersion = version;
        // // 유저명 설정
        // PhotonNetwork.NickName = nickName; 밑에서 닉네임 설정해가지고 더이상 필요가 없음
        // 방장이 게임을 시작 했을 때(씬을 로딩 했을 때) 다른 유저(클라이언트)도 자동으로 해당 씬이 로딩되는 옵션
        PhotonNetwork.AutomaticallySyncScene = true;

        // 포톤 서버에 접속
        if (PhotonNetwork.IsConnected == false) // 이미 접속 해 있으면 다시 부를 필요가 없으니까
        {
            PhotonNetwork.ConnectUsingSettings();
        }

    }

    void Start()
    {

        // 닉네임을 한 번이라도 설정했다면 그 닉네임을 가져오는 것
        // = 저장된 닉네임이 있을 경우에 출력
        if (PlayerPrefs.HasKey("NICK_NAME"))
        {
            nickName = PlayerPrefs.GetString("NICK_NAME");
            nickNameIf.text = nickName;
        }

        SetNickName();
        loginButton.onClick.AddListener(() => OnLoginButtonClick()); // 로그인 버튼이 눌렸을 때 해당 이벤트를 발생시켜주세요
        makeRoomButton.onClick.AddListener(() => OnMakeRoomButtonClick());

    }

    private void OnMakeRoomButtonClick()
    {
        // 닉네임 여부 확인
        SetNickName();

        // 룸 이름 입력여부 확인
        if (string.IsNullOrEmpty(roomNameIf.text)) // 비어있으면 랜덤한 숫자 발생시켜 생성
        {
            roomNameIf.text = $"Room_{Random.Range(0, 1000):0000}";
        }

        // 룸 속성을 정의
        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = 20;
        ro.IsOpen = true;
        ro.IsVisible = true;

        // 룸 생성
        PhotonNetwork.CreateRoom(roomNameIf.text, ro);
    }

    private void SetNickName() // 닉네임 칸이 비어있을 때 난수를 발생시켜서 입력하는 함수
    {
        // 닉네임이 비어있는지 확인
        if (string.IsNullOrEmpty(nickNameIf.text))
        {
            // 닉네임 랜덤하게 설정
            nickName = $"USER_{Random.Range(0, 1000):0000}"; // 0~1000까지의 랜덤한 수, 4자리의.
            nickNameIf.text = nickName; // 윗줄에서 생성된 닉네임을 닉네임인풋필드에 넣기
        }

        nickName = nickNameIf.text; // 이미 입력된 닉네임을 닉네임인풋필드에서 사용함

        //포톤의 닉네임 설정
        PhotonNetwork.NickName = nickName;
    }

    public void OnLoginButtonClick()
    {
        SetNickName(); // 닉네임 한 번 설정하고,

        // 사용한 닉네임을 저장
        PlayerPrefs.SetString("NICK_NAME", nickName); // 플레이어 프리펩에 사용한 닉네임을 저장한다
        PhotonNetwork.JoinRandomRoom(); // 아무 방이나 랜덤하게 입장한다
    }


    // 포톤 서버에 접속되었을 때 호출되는 콜백
    public override void OnConnectedToMaster()
    {
        Debug.Log("서버 접속 완료");

        // 로비 입장 요청
        PhotonNetwork.JoinLobby();
    }

    // 로비에 입장 했을 때 호출되는 콜백
    public override void OnJoinedLobby()
    {
        Debug.Log("로비 입장 완료");
        // 랜덤한 방을 선택해서 입장하도록 요청
        // PhotonNetwork.JoinRandomRoom();
    }

    // 랜덤하게 조인할 수 있는 방이 없다는(방 입장 실패) 호출되는 콜백
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"방 입장 실패 : {message}");

        // 방을 생성하기
        // 방의 룸 옵션 더해주기 ("MyRoom", [ 여기에 들어감 ])
        RoomOptions ro = new RoomOptions
        {
            MaxPlayers = 100,
            IsOpen = true,
            IsVisible = true
        };

        PhotonNetwork.CreateRoom("MyRoom" + Random.Range(0, 100), ro); // MyRoom 뒤에 랜덤한 숫자 0~100까지 붙게 만들기
    }

    // 방 생성 완료 콜백
    public override void OnCreatedRoom()
    {
        Debug.Log("방이 생성 되었습니다.");
    }

    // 방 입장 후 콜백
    public override void OnJoinedRoom()
    {
        Debug.Log("방에 입장했습니다.");

        // 씬 로딩하는거는 방장만 해야한다.
        if (PhotonNetwork.IsMasterClient)
        {
            // 전투 씬을 로딩
            PhotonNetwork.LoadLevel("BattleField");

            // 아래의 코드는 나중에 GameManager 의 CreatTank 함수로 들어갈거다
            // Vector3 pos = new Vector3(Random.Range(-150.0f, 150.0f), 5.0f, Random.Range(-150.0f, 150.0f));

            // // 네트워크 플레이어 설정
            // PhotonNetwork.Instantiate("Tank", pos, Quaternion.identity, 0); // 같은 그룹 안에 있는 탱크들 끼리만 보인다~
            // // 그동안 사용했던 Instantiate 가 아니라 포톤에서 상속받는 것. Resources 폴더 안에 있는 Tank 를 가져옴
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    { // 룸의 리스트가 바뀌면 포톤에서 호출해주는 함수
        foreach (var room in roomList) // 룸에 넘어온 리스트를 처음부터 끝까지 찍어보는 것
        {
            Debug.Log($"{room.Name} {room.PlayerCount}/{room.MaxPlayers}");

            // 삭제된 룸
            if (room.RemovedFromList == true)
            {
                // 룸 삭제
                if (roomDict.TryGetValue(room.Name, out GameObject tempRoom))
                {
                    // 프리펩을 삭제
                    Destroy(tempRoom);
                    // 딕셔너리의 레코드를 삭제
                    roomDict.Remove(room.Name); // 딕셔너리에 있는 것도 지우기
                }
                continue; // 바로 위로 올라가서 다시 돈다
            }
            // 새로 생성된 룸, 변경된 경우
            if (roomDict.ContainsKey(room.Name) == false) // 생성된 룸이 없으면~ 처음 생성되면!
            {
                // 처음 생성된 룸, 파라미터가 2개면 부모다~ 는걸 알 수 있다
                var _room = Instantiate(roomPrefab, contentTr);

                // RoomPrefab에 RoomInfo 값을 저장
                _room.GetComponent<RoomData>().RoomInfo = room; // 프로퍼티에 룸 값을 대입하면
                // setter 부분이 실행, value 값이 RoomData가 될 것이고 Roomtext 에다가 출력한다
                // 딕셔너리에 저장  위에서 룸프리펩을 이용해서 _room 으로 저장했으니까
                roomDict.Add(room.Name, _room);
            }
            else
            {
                // 이전에 생성되었던 룸...
                // 룸 정보를 갱신(가져오기)
                // 값이 있으면 tempRoom에 담아서 건낸다. 그러고서 True 값을 리턴
                if (roomDict.TryGetValue(room.Name, out GameObject tempRoom))
                { // RoomInfo를 RoomData에서 갖다넣고 그걸 room으로 담는다
                    tempRoom.GetComponent<RoomData>().RoomInfo = room;
                }


            }

            // 삭제된 룸
        }
    }


}
