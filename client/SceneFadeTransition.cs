using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFadeTransition : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static SceneFadeTransition Instance;

    private Image fadeImage;  // 동적으로 생성한 Image
    private Canvas fadeCanvas; // Fade용 Canvas
    public float fadeDuration = 1.0f;

    void Awake()
    {
        // 싱글톤 설정: 이미 인스턴스가 존재하면 이 오브젝트를 파괴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // 씬 전환 시에도 이 객체를 파괴하지 않도록 설정
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        CreateFadeImage();
    }

    // Scene 전환 시 Fade 효과 적용
    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoadScene(sceneName));
    }

    // Fade 효과 구현
    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        // Fade Out (화면을 검게 만들기)
        fadeImage.gameObject.SetActive(true);
        float timeElapsed = 0;
        //while (timeElapsed < fadeDuration)
        //{
        //    fadeImage.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, timeElapsed / fadeDuration));
        //    timeElapsed += Time.deltaTime;
        //    yield return null;
        //}
        fadeImage.color = new Color(0, 0, 0, 1);  // 완전히 검게

        // 씬 로드
        SceneManager.LoadScene(sceneName);

        // Fade In (화면을 다시 투명하게 만들기)
        timeElapsed = 0;
        while (timeElapsed < fadeDuration)
        {
            fadeImage.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, timeElapsed / fadeDuration));
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 0);  // 완전히 투명하게 설정

        fadeImage.gameObject.SetActive(false);  // UI를 숨김

        // 씬이 전환된 후 Canvas 및 Image 삭제
        Destroy(fadeCanvas.gameObject);  // Canvas와 그 자식 오브젝트들도 함께 삭제
    }

    // 코드로 fadeImage (Image) 생성 및 설정
    private void CreateFadeImage()
    {
        // Canvas 생성
        fadeCanvas = new GameObject("FadeCanvas").AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;  // 화면에 오버레이
        CanvasScaler canvasScaler = fadeCanvas.gameObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;  // UI 크기 화면에 맞추기
        fadeCanvas.gameObject.AddComponent<GraphicRaycaster>(); // 클릭 이벤트 차단용

        // Canvas에 Image 추가
        GameObject fadeObject = new GameObject("FadeImage");
        fadeObject.transform.SetParent(fadeCanvas.transform);  // Canvas의 자식으로 설정
        fadeImage = fadeObject.AddComponent<Image>();  // Image 컴포넌트 추가

        // Image 설정
        fadeImage.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);  // 화면 크기 설정
        fadeImage.rectTransform.anchorMin = new Vector2(0, 0);  // anchor를 (0,0)으로 설정
        fadeImage.rectTransform.anchorMax = new Vector2(1, 1);  // anchor를 (1,1)으로 설정
        fadeImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);  // pivot을 중앙으로 설정

        fadeImage.color = new Color(0, 0, 0, 0);  // 처음에는 투명하게 설정
        fadeImage.gameObject.SetActive(false);  // 초기에는 비활성화

        // Canvas와 그 자식들이 씬 전환 후에도 유지되게 설정
        DontDestroyOnLoad(fadeCanvas.gameObject);  // Canvas와 그 자식들을 씬 전환 후에도 유지
    }

}
