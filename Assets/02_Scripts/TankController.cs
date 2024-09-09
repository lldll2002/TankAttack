using Photon.Pun;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Photon.Realtime;

[RequireComponent(typeof(AudioSource))] // 오디오 소스는 꼭 필요하다는 것을 표시하는 함수 한 줄
public class TankController : MonoBehaviour
{
    // 위치와 rigidbody 조절해야함
    private Transform tr;
    private Rigidbody rb;
    private new AudioSource audio; // new 는 내가 이 안에서 새롭게 정의한 함수다~
    private CinemachineCamera cvc;
    private PhotonView pv;

    [SerializeField] private float moveSpeed = 10.0f;
    // SerializedField 쓰면 코드보다 유니티상에서의 숫자가 우선시 되기 때문에 이 안에서 코드 수정해도 반영 안될 수 있다.
    [SerializeField] private float turnSpeed = 100.0f;

    // 람다식으로 
    // v 라는 변수를 참조하면, 뒤의 문장을 실행해주세요.
    private float v => Input.GetAxis("Vertical");
    private float h => Input.GetAxis("Horizontal");
    private bool isFire => Input.GetMouseButtonDown(0);

    public GameObject canonPrefab;
    public Transform firePos;
    public AudioClip FireSfx;

    [NonSerialized]
    public TMP_Text nickName;
    public Image hpBar;

    private float initHp = 100.0f;
    private float currHp = 100.0f;



    MeshRenderer[] renderers; // 배열로 렌더러를 추가해줌 (막타 쳤을 때 없어지도록 메쉬를 끄는 함수 SetVisibleTank에 씀)


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        audio = GetComponent<AudioSource>();
        cvc = GameObject.Find("CinemachineCamera").GetComponent<CinemachineCamera>();
        pv = GetComponent<PhotonView>();
        nickName = transform.Find("Canvas/Panel/NickName").GetComponent<TMP_Text>();

        nickName.text = pv.Owner.NickName;

        // SetVisibleTank와 연결
        renderers = this.gameObject.GetComponentsInChildren<MeshRenderer>();

        // 카메라 연결
        if (pv.IsMine == true) // 이 탱크가 나의 탱크인가 확인
        {
            cvc.Follow = tr;
            cvc.LookAt = tr;
        }
        else
        {
            rb.isKinematic = true; // 인게임 상에서 포톤네트워크를 통해서 위치값을 받아오고 있으니 물리연산 겹치는걸 방지하기 위해서
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!pv.IsMine)
        {
            return;
        }
        Locomotion();

        if (isFire)
        {
            // Fire(); 그냥 Fire 하면 로컬함수를 호출하는건데, 네트워크에서는 RPC함수를 호출해야 하므로 변경해줌.
            // Instantiate(canonPrefab, firePos.position, firePos.rotation);
            // instantiate를 함수적으로 분리하기 위해서 canon Fire에 분리
            // RPC 함수 호출
            pv.RPC(nameof(Fire), RpcTarget.AllViaServer, pv.Owner.ActorNumber); // pv.owner~ 는 마지막에 포탄 추가 해주고 더해줬다
            // 아래처럼도 쓸 수 있음
            // Fire();
            // pv.RPC(nameof(Fire),~~~~) 시간차가 생길 수 있어서 단점임
        }
    }

    void Locomotion()
    {
        // 로코모션은 이동 및 회전을 담당
        tr.Translate(Vector3.forward * Time.deltaTime * v * moveSpeed); //이동
        tr.Rotate(Vector3.up * Time.deltaTime * h * turnSpeed); //회전
    }


    // RPC라고 정의한다. RPC (Remote Procedure Call) – 다른 PC에 있는 함수를 호출한다. 결과적으로 내 화면에 다른 PC에서 작업한 함수가 보인다.
    [PunRPC]
    // Canon Fire
    void Fire(int actorNumber) // 내가 쏜 포탄에는 내 액터넘버가 저장된다
    {
        audio.PlayOneShot(FireSfx, 0.8f); // 한 번 쏠 때 마다 80% 정도의 소리로 재생한다
        var canon = Instantiate(canonPrefab, firePos.position, firePos.rotation); // 캐논에 함수 담아줌
        canon.GetComponent<Canon>().shooterId = actorNumber;

    }

    void OnCollisionEnter(Collision coll)
    {
        // 캐논 태그를 확인하기
        if (coll.collider.CompareTag("CANNON"))
        {
            // ActorNumber => NickName 으로 변경하기
            int actorNumber = coll.gameObject.GetComponent<Canon>().shooterId;
            // 와서 충돌한 캐논아이디 안에 들어있는 캐논 스크립트에 저장되어있는 슈터아이디를 추출해서 actorNumber에 담는다

            // 현재 룸에 들어와있는 플레이어 중에서 액터넘버를 검색해서 플레이어 정보 가져오기
            Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);

            // 찾아 낸 플레이어의 닉네임을 출력하기
            // Debug.Log("Hit by " + player.NickName);

            // 유저를 죽인 막타 유저 표시하기
            currHp -= 20.0f;

            hpBar.fillAmount = currHp / initHp;

            if (currHp <= 0.0f)
            {
                // 구분을 위해서 색을 추가해줬다.
                string msg = $"<color=#00ff00>{pv.Owner.NickName}</color>님은 <color=#ff0000>{player.NickName}</color>에 의해 사망했습니다.";
                // DisplayMassage 함수 불러오기
                GameManager.Instance.DisplayMessage(msg);
                Debug.Log(msg);

                SetVisibleTank(false);
                Invoke(nameof(RespawnTank), 3.0f);
            }
        }
    }

    // 파괴된거 리스폰 하는 함수
    void RespawnTank()
    {
        currHp = initHp;
        hpBar.fillAmount = 1.0f;
        SetVisibleTank(true);
    }


    void SetVisibleTank(bool isVisible) // 선택된 모든 렌더러를 비활성화 시키는 함수
    {
        foreach (var renderer in renderers)
        {
            renderer.enabled = isVisible;
        }

        tr.Find("Canvas").gameObject.SetActive(isVisible);
    }
}