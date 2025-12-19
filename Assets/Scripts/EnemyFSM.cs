using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용을 위해 추가
using UnityEngine.AI; // NavMeshAgent 사용을 위해 추가

public class EnemyFSM : MonoBehaviour
{
    //에너미 상태 상수
    public enum EnemyState
    {
        Idle, Move, Attack, AttackDelay, Return, Damaged, Die
    }
    public enum ZombieType 
    { 
        Chaser, Runner, Tank 
    }

    //에너미 상태 변수
    EnemyState m_State;
    public ZombieType zombieType;

    //플레이어 발견 범위
    public float findDistance = 8f;
    public float hearDistance = 40f; // 총 소리 감지 거리
    Transform player;
    //이동속도
    public float moveSpeed;
    //공격 범위
    public float attackDistance = 2f;
    //누적 시간
    float currentTime = 0;
    //공격속도
    public float attackDelay = 2f;

    // NavMeshAgent 컴포넌트
    NavMeshAgent agent;
    
    //공격력
    public int attackDamage = 3;

    //초기 위치값
    Vector3 originPos;
    Quaternion originRot;
    //이동 가능 범위
    public float moveDistance = 20f;

    //적 체력
    public int hp;
    public int maxHp;

    //UI 관련
    public Slider hpBar;
    public TextMeshProUGUI nameText;
    public GameObject bloodSplatterEffect; // 피격 효과 프리팹

    // 사운드 관련 변수
    private AudioSource audioSource;
    public AudioClip basicZombieSound;      // 기본 좀비 소리
    public AudioClip detectedPlayerSound;   // 플레이어 발견 소리
    public AudioClip hitSound;              // 피격 소리
    public float basicSoundInterval = 5f;   // 기본 소리 재생 간격
    private float lastBasicSoundTime;

    //애니메이터 변수
    Animator anim;

    // 이 메서드는 스폰될 때 SpawnManager에 의해 호출됩니다.
    public void Init(ZombieType type)
    {
        zombieType = type;
        switch (zombieType)
        {
            case ZombieType.Chaser:
                maxHp = 10;
                moveSpeed = 3f;
                if (nameText != null) nameText.text = "Chaser";
                break;
            case ZombieType.Runner:
                maxHp = 6;
                moveSpeed = 5f;
                if (nameText != null) nameText.text = "Runner";
                break;
            case ZombieType.Tank:
                maxHp = 24;
                moveSpeed = 2.5f;
                if (nameText != null) nameText.text = "Tank";
                break;
        }
        hp = maxHp;
    }

    public void OnGunShot(Vector3 gunshotPosition)
    {
        if (m_State == EnemyState.Idle)
        {
            if (Vector3.Distance(transform.position, gunshotPosition) < hearDistance)
            {
                m_State = EnemyState.Move;
                print("상태 전환 : Idle -> Move (총 소리 감지)");
                anim.SetTrigger("IdleToMove");
                if (agent != null) agent.isStopped = false; // 이동 시작
                // 목표를 총 소리 위치로 설정하여 그 방향으로 이동하게 할 수도 있습니다.
                // agent.SetDestination(gunshotPosition); 
            }
        }
    }

    void Start()
    {
        //에너미 상태 초기값은 idle
        m_State = EnemyState.Idle;
        //플레이어의 트랜스폼 컴포넌트 찾아옴
        player = GameObject.Find("Player").transform;
        
        // NavMeshAgent 초기화
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("EnemyFSM: NavMeshAgent 컴포넌트를 찾을 수 없습니다!");
        }
        else
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackDistance;
            agent.updateRotation = true; // NavMeshAgent가 회전도 제어하도록 설정
            agent.isStopped = true; // 초기에는 멈춰있도록 설정
        }

        // AudioSource 초기화
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError($"EnemyFSM: {gameObject.name}에 AudioSource 컴포넌트가 없습니다!");
        }
        lastBasicSoundTime = Time.time; // 기본 소리 재생 시간 초기화

        //적 초기 위치값 저장
        originPos = transform.position;
        originRot = transform.rotation;

        anim = transform.GetComponentInChildren<Animator>();
    }    

    // Update is called once per frame
    void Update()
    {
        // NavMeshAgent가 비활성화된 상태에서는 움직임 처리 스킵
        if (agent != null && !agent.enabled) return;

        //현재 상태를 체크해 상태별 정해진 기능 수행
        switch (m_State)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Move:
                Move();
                break;
            case EnemyState.Attack:
                Attack();
                break;
            case EnemyState.AttackDelay:
                AttackDelay();
                break;
            case EnemyState.Return:
                Return();
                break;
            case EnemyState.Damaged:
                //Damaged();
                break;
            case EnemyState.Die:
                //Die();
                break;
        }
        //현재 체력 비율을 반영
        if (hpBar != null)
        {
            hpBar.value = (float)hp / (float)maxHp;
        }
    }

    void Idle()
    {
        // 일정 간격으로 기본 좀비 소리 재생
        if (audioSource != null && basicZombieSound != null && Time.time >= lastBasicSoundTime + basicSoundInterval)
        {
            audioSource.PlayOneShot(basicZombieSound);
            lastBasicSoundTime = Time.time;
        }

        //만일 플레이어와의 거리가 findDistance 이내라면 Move 상태로 전환
        if(Vector3.Distance(transform.position, player.position) < findDistance)
        {
            m_State= EnemyState.Move;
            print("상태 전환 : Idle -> Move");
            anim.SetTrigger("IdleToMove");
            if(agent != null) agent.isStopped = false; // 이동 시작

            // 플레이어 발견 시 소리 재생
            if (audioSource != null && detectedPlayerSound != null)
            {
                audioSource.PlayOneShot(detectedPlayerSound);
            }
        }
    }
    void Move()
    {
        if (agent != null && !agent.isOnNavMesh)
        {
            // NavMesh 위에 있지 않으면 이동 로직을 건너뜁니다.
            return;
        }

        //만약 현재 위치가 초기 위치보다 멀어진다면 복귀상태로 전환
        if (Vector3.Distance(transform.position, originPos) > moveDistance)
        {
            m_State = EnemyState.Return;
            print("상태 전환 : Move -> Return");
            anim.SetTrigger("MoveToReturn");
            if(agent != null) agent.isStopped = false; // 이동 시작
        }
        //만약 플레이어와의 거리가 공격범위 밖이면 플레이어를 향해 이동
        else if (Vector3.Distance(transform.position, player.position) > attackDistance)
        {
            if(agent != null) agent.SetDestination(player.position);
        }
        else //상태를 공격 상태로 변경
        {
            m_State = EnemyState.Attack;
            print("상태 전환 : Move -> Attack");
            //누적 시간을 공격 딜레이만큼 설정
            currentTime = 0;
            //공격 대기 애니메이션 실행
            anim.SetTrigger("AttackDelay");
            if(agent != null) agent.isStopped = true; // 공격 시 멈춤
        }
    }
    void Attack()
    {
        // 1. 플레이어에게 데미지를 주고
        player.GetComponent<PlayerMove>().DamageAction(attackDamage);
        print("공격 실행");

        // 2. 공격 애니메이션 실행
        anim.SetTrigger("StartAttack"); // ⬅️ 공격 애니메이션 실행

        // 4. 즉시 상태 전환 (로직은 유지)
        m_State = EnemyState.AttackDelay;
        print("상태 전환 : Attack -> AttackDelay");

        // 5. 딜레이 애니메이션 (Idle) 실행 (AttackDelay 상태로 가는 Transition만 필요)
        anim.SetTrigger("AttackDelay");
        if(agent != null) agent.isStopped = true; // 공격 대기 시 멈춤
    }

    void AttackDelay()
    {
        //플레이어를 계속 바라보도록 설정 (NavMeshAgent가 회전을 제어하므로 목적지를 설정하여 바라보게 함)
        if(agent != null && player != null)
        {
            agent.SetDestination(player.position);
            agent.isStopped = true; // 멈춰있지만 플레이어를 바라보게
        }

        //누적 시간 증가
        currentTime += Time.deltaTime;

        //AttackDelay 중 복귀 조건
        if (Vector3.Distance(transform.position, originPos) > moveDistance)
        {
            m_State = EnemyState.Return;
            print("상태 전환 : AttackDelay -> Return");
            currentTime = 0;
            anim.SetTrigger("MoveToReturn"); // 애니메이션 트리거 호출
            if(agent != null) agent.isStopped = false; // 이동 시작
            return; // Return으로 전환했으므로 나머지 AttackDelay 로직은 건너뜀
        }

        //딜레이 시간이 지나면
        if (currentTime > attackDelay)
        {
            // 딜레이 시간 초기화
            currentTime = 0;

            //플레이어와의 거리가 공격 범위 이내라면 다시 공격 상태로
            if (Vector3.Distance(transform.position, player.position) < attackDistance)
            {
                m_State = EnemyState.Attack;
                print("상태 전환 : AttackDelay -> Attack (재공격)");
                if(agent != null) agent.isStopped = true; // 공격 시 멈춤
            }
            //플레이어가 범위 밖으로 벗어났다면 이동 상태로
            else
            {
                m_State = EnemyState.Move;
                print("상태 전환 : AttackDelay -> Move");
                anim.SetTrigger("AttackDelayToMove"); // 애니메이션 전환
                if(agent != null) agent.isStopped = false; // 이동 시작
            }
        }
    }

    void Return()
    {
        if (agent == null) return; // agent가 null일 경우 추가 방어 로직

        if (!agent.isOnNavMesh)
        {
            // NavMesh 위에 있지 않으면 이동 로직을 건너뜁니다.
            return;
        }

        // 목적지를 초기 위치로 설정
        agent.SetDestination(originPos);

        // 만약 NavMeshAgent가 거의 목적지에 도달했고, 경로 계산이 끝났다면 Idle로 전환
        // agent.remainingDistance <= agent.stoppingDistance 와 같이 작은 값으로 확인하고,
        // agent.pathPending이 false인 것을 확인하여 경로 계산이 완료된 상태인지 체크합니다.
        // 또한, agent.velocity.sqrMagnitude < 0.1f 와 같이 에이전트가 실제로 멈췄는지도 확인합니다.
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending && agent.velocity.sqrMagnitude < 0.1f)
        {
            // 목적지에 도달했으므로 멈춤
            agent.isStopped = true;
            // 회전을 초기 회전으로 즉시 설정 (선택 사항)
            transform.rotation = originRot;

            hp = maxHp;
            m_State = EnemyState.Idle;
            print("상태 전환 : Return -> Idle");
            anim.SetTrigger("ReturnToIdle");
        }
        else
        {
            // 아직 목적지에 도달하지 않았으므로 계속 이동
            agent.isStopped = false;
        }
    }

    void Damaged()
    {
        StartCoroutine(DamageProcess());
    }

    //데미지 코루틴 함수
    IEnumerator DamageProcess()
    {
        if(agent != null) agent.isStopped = true; // 피격 시 멈춤
        yield return new WaitForSeconds(0.2f);
        //이동 상태로 변경
        m_State = EnemyState.Move;
        anim.SetTrigger("ToMove");
        print("상태 전환 : Damaged -> Move");
        if(agent != null) agent.isStopped = false; // 이동 시작
    }

    //데미지 실행 함수
    public void HitEnemy(int hitPower, Vector3 hitPoint)
    {
        //만일 사망, 복귀, 데미지 상태일 때 아무 처리 X
        if(m_State == EnemyState.Damaged || m_State == EnemyState.Die || m_State == EnemyState.Return)
        {
            return;
        }
        // 피격 효과 생성
        if (bloodSplatterEffect != null)
        {
            Instantiate(bloodSplatterEffect, hitPoint, Quaternion.identity);
        }

        // 피격 소리 재생
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        //플레이어 공격력만큼 체력 감소
        hp -= hitPower;
        print("적 남은 체력 : " + hp);
        //적 체력이 0보다 크면 Damaged 상태로 전환
        if (hp > 0)
        {
            m_State = EnemyState.Damaged;
            print("상태 전환 : Any State -> Damaged");
            Damaged();
        }
        else//적 체력이 0보다 작다면 Die 상태로 전환
        {
            m_State=EnemyState.Die;
            print("상태 전환 : Any State -> Die");
            Die();
        }
    }
    void Die() 
    {
        //진행중인 코루틴 중지
        StopAllCoroutines();
        //사망 상태 코루틴 실행
        StartCoroutine(DieProcess());
    }
    //사망 상태 코루틴
    IEnumerator DieProcess()
    {
        // NavMeshAgent 비활성화
        if(agent != null) agent.enabled = false;
        anim.SetTrigger("Die");
        //일정 시간(애니메이션) 대기 후 제거
        yield return new WaitForSeconds(2.5f);
        print("적 소멸");
        //GameManager에 좀비가 죽었음을 알림
        GameManager.gm.OnZombieKilled();
        Destroy(gameObject);
    }
}
