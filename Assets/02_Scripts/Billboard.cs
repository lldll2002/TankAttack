using UnityEngine;

public class Billboard : MonoBehaviour
{

    private Transform cameraTr;

    void Start()
    {
        cameraTr = Camera.main.transform;
    }

    void LateUpdate() // 카메라가 이동한 뒤에 마지막으로 처리하기 위해 update아닌 lateupdate를 사용함
    {
        transform.LookAt(cameraTr);
    }
}
