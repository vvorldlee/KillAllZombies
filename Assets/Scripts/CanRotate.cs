using UnityEngine;
using static GameManager;

public class CanRotate : MonoBehaviour
{
    //회전 속도 변수
    public float rotSpeed = 20f;

    float mx = 0;
    float my = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //게임 상태가 Run일때만 조작 가능
        if (GameManager.gm.g_State != GameState.Run)
        {
            return;
        }

        //마우스 입력을 받음
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        //회전값 변수에 마우스 입력 값 누적
        mx += mouseX * rotSpeed * Time.deltaTime;
        my += mouseY * rotSpeed * Time.deltaTime;

        my = Mathf.Clamp(my, -90f, 90f);

        transform.eulerAngles = new Vector3(-my, mx, 0); 
         
        ////회정 방향 결정
        //Vector3 dir = new Vector3(-mouseY, mouseX, 0);

        ////물체 회전
        //transform.eulerAngles += dir * rotSpeed * Time.deltaTime;

        ////y축 값 제한
        //Vector3 rot = transform.eulerAngles;
        //rot.x = Mathf.Clamp(rot.x, -90f, 90f);
        //transform.eulerAngles = rot;

    }
}
