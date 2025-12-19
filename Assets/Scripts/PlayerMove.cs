using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class PlayerMove : MonoBehaviour
{
    //이동속도 변수
    public float moveSpeed = 5f;
    CharacterController cc;
    //중력 변수
    float gravity = -20f;
    //수직 속력 변수
    float yVelocity = 0;
    //점프력
    public float jumpPower = 10f;
    bool isJumping = false;

    //플레이어 체력
    public int hp = 20;
    public int maxHp = 20;
    //체력 슬라이더 변수
    public Slider hpSlider;
    //Hit 효과 오브젝트
    public GameObject hitEffect;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cc = GetComponent<CharacterController>();
        hitEffect.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //게임 상태가 Run일때만 조작 가능
        if (GameManager.gm.g_State != GameState.Run)
        {
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        //이동 방향 설정
        Vector3 dir = new Vector3(h, 0, v);
        dir = dir.normalized;

        //메인 카메라를 기준으로 방향 변경
        dir = Camera.main.transform.TransformDirection(dir);
        transform.position += dir * moveSpeed * Time.deltaTime;

        if(isJumping && cc.collisionFlags  == CollisionFlags.Below)
        {
            isJumping = false;
            yVelocity = 0;
        }

        //점프
        if(Input.GetButtonDown("Jump") && !isJumping)
        {
            yVelocity = jumpPower;
            isJumping = true;
        }

        //캐릭터 수직 속도에 중력값 적용
        yVelocity += gravity * Time.deltaTime;
        dir.y = yVelocity;

        //이동 함수
        cc.Move(dir * moveSpeed * Time.deltaTime);
        //플레이어 체력 비율을 슬라이더 값에 반영
        hpSlider.value = (float)hp / (float)maxHp;
    }
    //플레이어 피격 함수
    public void DamageAction(int damage)
    {
        //적의 공격력 만큼 플레이어 체력 깎음
        hp -= damage;
        print("남은 체력 : " + hp);
        //체력이 0일 경우 음수로 초기화
        if(hp < 0)
        {
            hp = 0;
        }
        else
        {
            //피격 이펙트 코루틴 시작
            StartCoroutine(PlayerHitEffect());
        }
    }
    //피격효과 코루틴 함수
    IEnumerator PlayerHitEffect()
    {
        //피격 UI 활성화
        hitEffect.SetActive(true);
        //일정 시간동안 대기
        yield return new WaitForSeconds(0.2f);
        //피격 UI 비활성화
        hitEffect.SetActive(false);
    }
}
