using UnityEngine;
using UnityEngine.PlayerLoop;

public class Canon : MonoBehaviour
{
    [SerializeField] private float force = 1200.0f;
    [SerializeField] private GameObject expEffect;

    // 누가 발사했는지(유저) 식별 ID 저장
    public int shooterId;


    void Awake()
    {
        expEffect = Resources.Load<GameObject>("BigExplosion"); // 리소시스 폴더에서 게임오브젝트를 불러온다, BigExPlosion 이라는 오브젝트를.
    }

    void Start()
    {
        GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * force);

        // 무한히 떨어지는 포탄이 있으면 안되니까 5초 후에 삭제할거임
        Destroy(this.gameObject, 5.0f);
    }


    private void OnCollisionEnter(Collision other)
    {
        var obj = Instantiate(expEffect, transform.position, Quaternion.identity); // 정확한 위치를 가져오진 않아도 되기 때문에
        // obj 라는 변수 값에다가 instantiate 아래의 함수를 담는다

        Destroy(obj, 5.0f);
        Destroy(this.gameObject);

    }
}
