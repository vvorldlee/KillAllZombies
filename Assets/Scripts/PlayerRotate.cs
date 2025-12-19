using UnityEngine;
using static GameManager;

public class PlayerRotate : MonoBehaviour
{
    public float rotSpeed = 200f;
    float mx = 0;
   
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

        float mouseX = Input.GetAxis("Mouse X");
        mx += mouseX * rotSpeed * Time.deltaTime;
        transform.eulerAngles = new Vector3(0, mx, 0);

    }
}
