using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //싱글톤 패턴
    public static GameManager gm;
    private void Awake()
    {
        if(gm == null)
        {
            gm = this;
        }
    }

    // BGM 관련 변수
    private AudioSource audioSource;
    public AudioClip gameBGM;

    //게임 상태 열거형
    public enum GameState
    {
        Ready,
        Run,
        GameEnd
    }
    public GameState g_State;

    //난이도 열거형
    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }
    public static Difficulty currentDifficulty = Difficulty.Normal;
    private string difficultyString;

    //UI 관련
    public UIManager uiManager;
    public GameObject gameLabel; // Ready, GO! 텍스트
    TextMeshProUGUI gameText;
    public GameObject gameEndPanel; // 게임 종료 패널
    public SpawnManager spawnManager; // 좀비 스폰 매니저

    PlayerMove player;

    //플레이타임 변수
    private float playtime = 0f;
    //좀비 추적 변수
    private int totalZombies;
    private int zombiesKilled = 0;
    //최고 기록 변수
    private float bestScore;

    void Start()
    {
        g_State = GameState.Ready;

        // UI 초기화
        gameText = gameLabel.GetComponent<TextMeshProUGUI>();
        gameText.text = "Ready...";
        gameText.color = new Color32(255, 255, 0, 255);
        gameEndPanel.SetActive(false); // 게임 종료 패널 비활성화

        player = GameObject.Find("Player").GetComponent<PlayerMove>();

        // AudioSource 초기화 및 BGM 재생
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError($"GameManager: {gameObject.name}에 AudioSource 컴포넌트가 없습니다!");
        }
        else
        {
            if (gameBGM != null)
            {
                audioSource.clip = gameBGM;
                audioSource.loop = true; // BGM 반복 재생 설정
                audioSource.Play();
            }
        }
        
        //난이도에 따른 좀비 수 및 문자열 설정
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                totalZombies = 10;
                difficultyString = "Easy";
                break;
            case Difficulty.Normal:
                totalZombies = 20;
                difficultyString = "Normal";
                break;
            case Difficulty.Hard:
                totalZombies = 30;
                difficultyString = "Hard";
                break;
        }

        //최고 기록 로드
        bestScore = PlayerPrefs.GetFloat(currentDifficulty.ToString() + "BestScore", 9999f);

        //메인 UI 초기 설정
        uiManager.UpdateZombieCount(totalZombies - zombiesKilled, totalZombies);
        uiManager.UpdateDifficulty(difficultyString);
        uiManager.UpdateBestScore(bestScore);
        uiManager.UpdatePlayTime(0);

        // 좀비 스폰
        if (spawnManager != null)
        {
            spawnManager.SpawnZombies(currentDifficulty);
        }
        else
        {
            Debug.LogError("SpawnManager is not assigned in GameManager!");
        }

        //게임시작 카운트다운 코루틴 시작
        StartCoroutine(ReadyToStart());
    }

    void Update()
    {
        //게임 상태에 따라 커서 잠금/해제
        if (g_State == GameState.Run)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        //게임 상태가 Run이 아니면 아래 로직 실행 안함
        if (g_State != GameState.Run)
        {
            return;
        }

        //플레이타임 계산 및 UI 업데이트
        playtime += Time.deltaTime;
        uiManager.UpdatePlayTime(playtime);

        //플레이어의 hp가 0 이하면 게임 종료
        if(player.hp <= 0)
        {
            EndGame();
        }
    }

    //좀비가 죽었을 때 호출될 함수
    public void OnZombieKilled()
    {
        zombiesKilled++;
        uiManager.UpdateZombieCount(totalZombies - zombiesKilled, totalZombies);

        //모든 좀비를 잡았으면 게임 종료
        if (zombiesKilled >= totalZombies)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        g_State = GameState.GameEnd;

        // 최고 기록 갱신 확인
        if (playtime < bestScore)
        {
            bestScore = playtime;
            PlayerPrefs.SetFloat(currentDifficulty.ToString() + "BestScore", bestScore);
            PlayerPrefs.Save();
        }

        // 게임 종료 패널 활성화 및 정보 업데이트
        gameEndPanel.SetActive(true);
        uiManager.UpdateGameEndUI(playtime, bestScore, difficultyString);
    }
    
    IEnumerator ReadyToStart()
    {
        yield return new WaitForSeconds(2f);
        gameText.text = "GO!";
        yield return new WaitForSeconds(0.5f);
        gameLabel.SetActive(false);
        g_State = GameState.Run;
    }

    // --- 버튼 핸들러 ---
    public void OnRestartButtonClick()
    {
        // 현재 씬 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnTitleButtonClick()
    {
        // 시작 씬 로드
        SceneManager.LoadScene("StartScene");
    }

    public void OnQuitButtonClick()
    {
        // 애플리케이션 종료
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}