using System.Collections;
using UnityEngine;

public class PlayerFire : MonoBehaviour
{
    //UI 매니저
    public UIManager uiManager;
    //발사 위치
    public GameObject firePosition;
    //수류탄 프리팹
    public GameObject grenadeFactory;
    //수류탄 투척 파워
    public float throwPower = 15f;
    //총알 피격 이펙트
    public GameObject bulletEffect;
    //총알 피격 파티클 시스템
    ParticleSystem ps;
    //공격력
    public int attackDamage = 3;
    
    //최대 총알 수
    public int maxAmmo = 30;
    //현재 총알 수
    private int currentAmmo;
    //재장전 시간
    public float reloadTime = 1.5f;
    //재장전 중인지 확인
    private bool isReloading = false;

    // 수류탄 쿨타임 변수
    public float grenadeCooldown = 15f;
    private bool canThrowGrenade = true;

    // 총 소리 관련 변수
    private AudioSource audioSource;
    public AudioClip gunSound;
    public AudioClip reloadSound; // 재장전 소리

    void OnEnable()
    {
        //스크립트가 활성화될 때 재장전 상태 초기화
        isReloading = false;
    }

    void Start()
    {
        ps = bulletEffect.GetComponent<ParticleSystem>();
        //현재 총알 수를 최대로 설정
        currentAmmo = maxAmmo;
        //UI 업데이트
        uiManager.UpdateAmmo(currentAmmo, maxAmmo);

        // AudioSource 초기화
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError($"PlayerFire: {gameObject.name}에 AudioSource 컴포넌트가 없습니다!");
        }
    }

    void Update()
    {
        //게임 상태가 Run이 아니거나 재장전 중이면 발사 로직을 실행하지 않음
        if (GameManager.gm.g_State != GameManager.GameState.Run || isReloading)
        {
            return;
        }
        
        //재장전 키(R)를 누르면 재장전 코루틴 시작
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
            return; //재장전 중에는 다른 행동을 막음
        }

        //수류탄 발사 (마우스 오른쪽 버튼)
        if (Input.GetMouseButtonDown(1) && canThrowGrenade)
        {
            ThrowGrenade();
        }

        //총알 발사 (마우스 왼쪽 버튼)
        if(Input.GetMouseButtonDown(0))
        {
            //총알이 없으면 발사하지 않음
            if (currentAmmo <= 0)
            {
                //(추후 추가) 빈 총 소리 재생
                Debug.Log("총알 없음!");
                return;
            }
            FireBullet();
        }
    }
    
    void ThrowGrenade()
    {
        canThrowGrenade = false; // 수류탄 투척 후 쿨타임 시작
        StartCoroutine(GrenadeCooldownRoutine()); // 쿨타임 코루틴 시작

        GameObject grenade = Instantiate(grenadeFactory);
        grenade.transform.position = firePosition.transform.position;
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        //카메라의 정면 방향으로 수류탄에 힘을 가함
        rb.AddForce(Camera.main.transform.forward * throwPower, ForceMode.Impulse);
    }

    // 수류탄 쿨타임 코루틴
    IEnumerator GrenadeCooldownRoutine()
    {
        yield return new WaitForSeconds(grenadeCooldown);
        canThrowGrenade = true; // 쿨타임 종료 후 다시 투척 가능
    }

    void FireBullet()
    {
        //총알 수 감소 및 UI 업데이트
        currentAmmo--;
        uiManager.UpdateAmmo(currentAmmo, maxAmmo);

        // 총 소리 재생
        if (audioSource != null && gunSound != null)
        {
            audioSource.PlayOneShot(gunSound);
        }

        //레이를 생성하여 발사
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hittedInfo = new RaycastHit();
        
        //레이캐스트로 충돌 감지
        if(Physics.Raycast(ray, out hittedInfo))
        {
            //충돌한 콜라이더가 있는지 확인
            if (hittedInfo.collider != null)
            {
                //충돌한 오브젝트가 Enemy 레이어일 경우
                if(hittedInfo.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    EnemyFSM eFSM = hittedInfo.collider.GetComponent<EnemyFSM>();
                    //EnemyFSM 컴포넌트가 존재하는지 확인 후 공격
                    if (eFSM != null)
                    {
                        eFSM.HitEnemy(attackDamage, hittedInfo.point);
                    }
                }
                else //그 외의 경우 피격 이펙트 표시
                {
                    bulletEffect.transform.position = hittedInfo.point;
                    bulletEffect.transform.forward = hittedInfo.normal;
                    ps.Play();
                }
            }
        }

        // 총 소리 감지 로직 추가 (근처 좀비들에게 총 소리 전달)
        // 40f는 임시 값이며, 필요에 따라 EnemyFSM.hearDistance와 연동하여 동적으로 가져올 수 있습니다.
        Collider[] colliders = Physics.OverlapSphere(firePosition.transform.position, 40f); 
        foreach (Collider col in colliders)
        {
            EnemyFSM enemyFSM = col.GetComponent<EnemyFSM>();
            if (enemyFSM != null)
            {
                enemyFSM.OnGunShot(firePosition.transform.position);
            }
        }
    }

    //재장전 코루틴
    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("재장전 중...");
        
        // 재장전 소리 재생
        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        //재장전 시간만큼 대기
        yield return new WaitForSeconds(reloadTime);

        //총알 재충전
        currentAmmo = maxAmmo;
        uiManager.UpdateAmmo(currentAmmo, maxAmmo);
        isReloading = false;
        Debug.Log("재장전 완료.");
    }
}