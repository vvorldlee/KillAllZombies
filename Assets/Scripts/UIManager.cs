using TMPro; // TextMeshPro 사용을 위해 필요
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // --- 메인 게임 UI ---
    public TextMeshProUGUI ammoText; //잔탄 표시
    public TextMeshProUGUI zombieCountText; //남은 좀비 수 표시
    public TextMeshProUGUI timeText; //플레이타임 표시
    public TextMeshProUGUI bestScoreText; //베스트 스코어 표시
    public TextMeshProUGUI difficultyText; //현재 난이도 표시

    // --- 게임 종료 패널 UI ---
    public TextMeshProUGUI end_clearTimeText;
    public TextMeshProUGUI end_bestScoreText;
    public TextMeshProUGUI end_difficultyText;


    // --- UI 업데이트 함수들 ---

    //잔탄 UI 업데이트
    public void UpdateAmmo(int currentAmmo, int maxAmmo)
    {
        ammoText.text = $"<color=yellow>{currentAmmo}</color> / {maxAmmo}";
    }
    
    //남은 좀비 수 UI 업데이트
    public void UpdateZombieCount(int remainingCount, int totalCount)
    {
        zombieCountText.text = $"<color=red>{remainingCount}</color> / {totalCount}";
    }

    //플레이타임 UI 업데이트
    public void UpdatePlayTime(float time)
    {
        timeText.text = $"PLAY TIME : {FormatTime(time)}";
    }

    //베스트 스코어 UI 업데이트
    public void UpdateBestScore(float time)
    {
        if (time >= 9999f)
        {
            bestScoreText.text = "BEST: --:--";
            return;
        }
        bestScoreText.text = $"BEST: {FormatTime(time)}";
    }

    //난이도 UI 업데이트
    public void UpdateDifficulty(string difficulty)
    {
        difficultyText.text = $"{difficulty}";
    }

    //게임 종료 패널 UI 업데이트
    public void UpdateGameEndUI(float clearTime, float bestTime, string difficulty)
    {
        end_clearTimeText.text = $"CLEAR TIME : {FormatTime(clearTime)}";
        if (bestTime >= 9999f)
        {
            end_bestScoreText.text = "BEST : --:--";
        }
        else
        {
            end_bestScoreText.text = $"BEST : {FormatTime(bestTime)}";
        }
        end_difficultyText.text = $"ㅂ{difficulty}";
    }

    //시간 포맷팅 헬퍼 함수
    private string FormatTime(float time)
    {
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        // 밀리초를 추가하고 싶다면 아래 주석을 풀고 로직을 수정하세요.
        // float milliseconds = (time * 100) % 100;
        // return $"{minutes:D2}:{seconds:D2}:{milliseconds:00}";
        return $"{minutes:D2}:{seconds:D2}";
    }
}
