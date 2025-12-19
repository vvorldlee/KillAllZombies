using UnityEngine;

public class PlayerArmsAnimatorController : MonoBehaviour
{
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>(); 
        if (animator == null)
        {
            Debug.LogError("PlayerArmsAnimatorController: Animator component not found on this GameObject!");
            enabled = false; // Animator가 없으면 스크립트 비활성화
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (animator == null) return;
        bool isMovingInput = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || 
                             Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
        
        // Animator의 isMoving 파라미터를 입력 상태에 따라 설정
        animator.SetBool("isMoving", isMovingInput);

        if(Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("shoot"); // "shoot" Trigger 활성화
            Debug.Log("Shoot Triggered!");
        }

        if (Input.GetMouseButtonDown(1))
        {
            animator.SetTrigger("grenade");
        }
 
        if (Input.GetKeyDown(KeyCode.R)) // R 키 눌렀을 때
        {
            animator.SetTrigger("reload"); // "reload" Trigger 활성화
            Debug.Log("Reload Triggered!");
        }
    }
}
