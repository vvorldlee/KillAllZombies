using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필요
using UnityEngine.UI; // UI 요소를 위해 필요
using TMPro; // TextMeshPro 컴포넌트를 사용하기 위해 필요
using System.Collections.Generic; // List를 사용하기 위해 필요
using System; // Enum을 사용하기 위해 필요

public class StartSceneManager : MonoBehaviour
{
    // 난이도 선택 드롭다운
    public TMP_Dropdown difficultyDropdown;

    void Start()
    {
        // 드롭다운 초기화
        if (difficultyDropdown != null)
        {
            // 기존 옵션들을 모두 제거
            difficultyDropdown.ClearOptions();

            // GameManager.Difficulty 열거형의 모든 값을 가져와서 리스트로 변환
            List<string> options = new List<string>(Enum.GetNames(typeof(GameManager.Difficulty)));

            // 드롭다운에 옵션 추가
            difficultyDropdown.AddOptions(options);

            // 기본값으로 Normal 설정 (옵션 리스트에서의 인덱스)
            int defaultIndex = options.FindIndex(option => option == "Normal");
            if (defaultIndex != -1)
            {
                difficultyDropdown.value = defaultIndex;
            }
        }
    }

    // 시작 버튼 클릭 시 호출될 함수
    public void OnStartButtonClick()
    {
        if (difficultyDropdown != null)
        {
            // 선택된 드롭다운의 인덱스를 기반으로 난이도 설정
            GameManager.currentDifficulty = (GameManager.Difficulty)difficultyDropdown.value;
        }

        // "GameScene"이라는 이름의 씬을 로드합니다.
        SceneManager.LoadScene("GameScene");
    }

    // 게임 종료 버튼 클릭 시 호출될 함수
    public void OnQuitButtonClick()
    {
        // 에디터에서 게임을 종료하거나 빌드된 애플리케이션을 종료합니다.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}