using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI; // UI 사용하기 위해서 필요
using TMPro; // 글자 텍스트 치기 위해서 필요
using UnityEngine.SceneManagement;
using Photon.Realtime; // 방(씬)을 나가기 위해서 필요

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance = null;

    [SerializeField] private Button exitButton;

    [SerializeField] private TMP_Text connectInfoText;
    // 버튼이랑 텍스트 연결해주기
    [SerializeField] private TMP_Text msgText;

    // RPC 를 이용해서 채팅 보낼 때 쓸 인풋필드와 버튼추가
    [SerializeField] private TMP_InputField chatMsgIf;
    [SerializeField] private Button SendMsgButton;
    // 플레이어 리스트를 표시하기 위한 
    [SerializeField] private TMP_Text playerListText;



    private PhotonView pv;

    // void OnEnable()
    // {
    //     exitButton.onClick.AddListener(() => OnExitButtonClick());
    // }

    void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        exitButton.onClick.AddListener(() => OnExitButtonClick());
        // 동시성 접속 에러가 나지 않도록 잠깐~ 딜레이를 줌
        SendMsgButton.onClick.AddListener(() => SendChatMessage());

        pv = GetComponent<PhotonView>();

        yield return new WaitForSeconds(0.5f);
        CreatTank();
        DisplayConnectInfo();
        DisplayPlayerListInfo();
    }


    void CreatTank() // 함수 만들기
    {
        Vector3 pos = new Vector3(Random.Range(-150.0f, 150.0f), 5.0f, Random.Range(-150.0f, 150.0f));

        // 네트워크 플레이어 설정
        PhotonNetwork.Instantiate("Tank", pos, Quaternion.identity, 0); // 같은 그룹 안에 있는 탱크들 끼리만 보인다~
        // 그동안 사용했던 Instantiate 가 아니라 포톤에서 상속받는 것. Resources 폴더 안에 있는 Tank 를 가져옴
    }

    public void SendChatMessage()
    {
        // 누가 보냈는지 [Zackiller], 뒤에 문장 안녕하세요~
        string msg = $"<color=#00ff00>[{PhotonNetwork.NickName}]</color> {chatMsgIf.text}";
        DisplayMessage(msg); // 자기자신한테 메시지를 출력
        pv.RPC(nameof(DisplayMessage), RpcTarget.OthersBuffered, msg); // 다른 사람들한테 메시지 보내기
    }

    [PunRPC] // RPC 로 변경하기. 로컬과 RPC로 모두 사용이 가능하다.
    public void DisplayMessage(string msg) // 탱크 터트렸을 때 메세지리스트에 띄우기
    {
        msgText.text += msg + "\n"; // \n 은 캐리지 리턴이다.. 
    }

    #region 사용자 정의 콜백
    private void OnExitButtonClick()
    {
        // 방에서 나가겠다는 요청
        // 네트워크에다가 자기 데이터 다 삭제해달라고 하는 요청
        PhotonNetwork.LeaveRoom();
    }

    #endregion

    #region 포톤 콜백함수 
    // C# 안에서 찾기 쉽게 구역을 나누는 것 
    // 룸을 나갔을 때 호출

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }

    // 룸에 클라이언트가 입장했을 때
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        DisplayConnectInfo();
        DisplayPlayerListInfo();
    }

    // 룸에 클라이언트가 나갔을 때
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        DisplayConnectInfo();
        DisplayPlayerListInfo();
    }

    private void DisplayConnectInfo()
    {
        int CurrPlayer = PhotonNetwork.CurrentRoom.PlayerCount;
        int MaxPlayer = PhotonNetwork.CurrentRoom.MaxPlayers;
        string roomName = PhotonNetwork.CurrentRoom.Name;
        string msg = $"[{roomName}] {CurrPlayer}/{MaxPlayer}";
        connectInfoText.text = msg;
    }
    #endregion

    private void DisplayPlayerListInfo() // 게임매니저에 플레이어 리스트 텍스트에 표시할 것
    {
        string playerList = "";

        foreach (var player in PhotonNetwork.PlayerList)
        {
            string _color = player.IsMasterClient ? "#ff0000" : "#00ff00";
            // 초기화 시키기
            playerList += $"<color={_color}>{player.NickName}</color>\n";
        }

        playerListText.text = playerList;
    }
}
