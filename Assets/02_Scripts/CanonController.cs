using Photon.Pun;
using UnityEngine;

public class CanonController : MonoBehaviour
{
    [SerializeField] private float speed = 1000.0f;
    private float r => Input.GetAxis("Mouse ScrollWheel");
    private PhotonView pv;

    void Start()
    {
        pv = transform.root.GetComponent<PhotonView>();
        // 탱크에 있는 photonview 컴포넌트를 가져오기 위해서, 루트를 찾아준다.
    }

    void Update()
    {
        if (!pv.IsMine) return;
        transform.Rotate(Vector3.right * Time.deltaTime * r * speed);
    }
}
