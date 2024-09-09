using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class RoomData : MonoBehaviour
{
    [SerializeField] private TMP_Text roomText;
    private RoomInfo roomInfo;

    // 프로퍼티
    // Getter , Setter
    public RoomInfo RoomInfo
    {
        // Getter
        get
        {
            return roomInfo;
        }
        // Setter
        set
        {
            roomInfo = value; // 외부에서 넘겨받은 값
            // 룸이름 (11/20)
            roomText.text = $"{roomInfo.Name} ({roomInfo.PlayerCount}/{roomInfo.MaxPlayers})";
            // 버튼 이벤트 연결
            // 처음부터 버튼이 연결되어있으면 안되고, 동적인 버튼이기 때문에
            // 생긴 이후에 이벤트가 연결 되어야 함.
            GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => PhotonNetwork.JoinRoom(roomInfo.Name));
            // 룸데이터 안에 있는 버튼을 가져온 것, 클릭을 하면~ 포톤네트워크의 룸네임으로 조인을 한다.

        }
    }
}
