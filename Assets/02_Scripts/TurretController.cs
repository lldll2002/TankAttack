using System;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class TurretController : MonoBehaviour
{
    // 터렛이 돌아가는 속도
    [SerializeField] private float turnSpeed = 20.0f;
    private PhotonView pv;


    void Start()
    {
        pv = transform.root.GetComponent<PhotonView>();
        // 탱크에 있는 photonview 컴포넌트를 가져오기 위해서, 루트를 찾아준다.
    }

    void Update()
    {
        if (!pv.IsMine) return;
        // 레이를 발사함.
        // 메인 카메라에서 Mouse Position 에서 발사하는 Ray 생성 (마우스의 x,y값을 입력)
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // 레이를 발사하는걸 Debug로 보여줌, 원점에서부터 100미터 거리까지, 초록색으로
        Debug.DrawRay(ray.origin, ray.direction * 100.0f, Color.green);

        // 레이케스팅
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1 << 8))
        {
            // hit 월드좌표를 터렛기준의 로컬좌표로 변환
            Vector3 pos = transform.InverseTransformPoint(hit.point);
            // 좌표를 받아 왔으면, 각도를 계산한다. Atan(x/z) X.... --> Atan2(x, z)
            // 나눗셈은 코딩에서 계산이 느리기 때문에 곱셈을 사용함. 앜탄의 역수
            // 라디안 값으로 결과가 나옴 --> 오일러각 변환이 필요
            float angle = Mathf.Atan2(pos.x, pos.z) * Mathf.Rad2Deg;

            // 계산한 값 만큼 터렛을 회전시킨다.
            transform.Rotate(Vector3.up * angle * Time.deltaTime * turnSpeed);

            // 위의 코드는 게임로직에서 가장 많이 쓰이는 코드 중 하나임
        }
    }
}
