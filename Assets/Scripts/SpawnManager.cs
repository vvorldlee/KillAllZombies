using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // NavMesh 사용을 위해 추가

public class SpawnManager : MonoBehaviour
{
    // 단일 좀비 프리팹
    public GameObject zombiePrefab;

    // 스폰 반경 (SpawnManager의 위치를 중심으로)
    public float spawnRadius = 50f;
    // NavMesh 위에서 유효한 스폰 위치를 찾기 위한 최대 시도 횟수
    private const int MAX_NAVMESH_SAMPLE_ATTEMPTS = 30;

    /// <summary>
    /// 난이도에 따라 좀비들을 스폰하고 각 좀비의 타입을 초기화합니다.
    /// </summary>
    /// <param name="difficulty">현재 게임 난이도</param>
    public List<GameObject> SpawnZombies(GameManager.Difficulty difficulty)
    {
        List<GameObject> spawnedZombies = new List<GameObject>();
        List<EnemyFSM.ZombieType> zombieTypesToSpawn = new List<EnemyFSM.ZombieType>();

        int chaserCount = 0, runnerCount = 0, tankCount = 0;

        // 난이도에 따른 좀비 수 설정
        switch (difficulty)
        {
            case GameManager.Difficulty.Easy:
                chaserCount = 7; runnerCount = 2; tankCount = 1;
                break;
            case GameManager.Difficulty.Normal:
                chaserCount = 15; runnerCount = 4; tankCount = 1;
                break;
            case GameManager.Difficulty.Hard:
                chaserCount = 20; runnerCount = 8; tankCount = 2;
                break;
        }

        // 스폰할 좀비 타입 리스트 채우기
        for (int i = 0; i < chaserCount; i++) zombieTypesToSpawn.Add(EnemyFSM.ZombieType.Chaser);
        for (int i = 0; i < runnerCount; i++) zombieTypesToSpawn.Add(EnemyFSM.ZombieType.Runner);
        for (int i = 0; i < tankCount; i++) zombieTypesToSpawn.Add(EnemyFSM.ZombieType.Tank);
        
        if (zombiePrefab == null)
        {
            Debug.LogError("SpawnManager: 좀비 프리팹이 할당되지 않았습니다!");
            return spawnedZombies;
        }

        // 좀비들을 랜덤 스폰 지점에 스폰
        foreach (var zombieType in zombieTypesToSpawn)
        {
            // NavMesh 위에서 유효한 스폰 위치 찾기
            if (GetRandomPointOnNavMesh(out Vector3 spawnPosition))
            {
                // 좀비 인스턴스화
                GameObject zombieInstance = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
                
                // EnemyFSM 컴포넌트를 가져와서 타입 초기화
                EnemyFSM fsm = zombieInstance.GetComponent<EnemyFSM>();
                if (fsm != null)
                {
                    fsm.Init(zombieType);
                }
                else
                {
                    Debug.LogError($"생성된 좀비 인스턴스에 EnemyFSM 컴포넌트가 없습니다: {zombieInstance.name}");
                }
                spawnedZombies.Add(zombieInstance);
            }
            else
            {
                Debug.LogWarning("NavMesh 위에서 유효한 스폰 위치를 찾지 못했습니다. 일부 좀비가 스폰되지 않았을 수 있습니다.");
            }
        }
        return spawnedZombies;
    }

    /// <summary>
    /// NavMesh 위에서 유효한 랜덤 스폰 위치를 찾습니다.
    /// </summary>
    /// <param name="result">찾은 스폰 위치</param>
    /// <returns>성공 여부</returns>
    private bool GetRandomPointOnNavMesh(out Vector3 result)
    {
        for (int i = 0; i < MAX_NAVMESH_SAMPLE_ATTEMPTS; i++)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * spawnRadius;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, spawnRadius, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }
}
