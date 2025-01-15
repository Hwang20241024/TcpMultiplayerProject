using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFadeTransition : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static SceneFadeTransition Instance;

    private Image fadeImage;  // �������� ������ Image
    private Canvas fadeCanvas; // Fade�� Canvas
    public float fadeDuration = 1.0f;

    void Awake()
    {
        // �̱��� ����: �̹� �ν��Ͻ��� �����ϸ� �� ������Ʈ�� �ı�
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // �� ��ȯ �ÿ��� �� ��ü�� �ı����� �ʵ��� ����
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

    // Scene ��ȯ �� Fade ȿ�� ����
    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoadScene(sceneName));
    }

    // Fade ȿ�� ����
    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        // Fade Out (ȭ���� �˰� �����)
        fadeImage.gameObject.SetActive(true);
        float timeElapsed = 0;
        //while (timeElapsed < fadeDuration)
        //{
        //    fadeImage.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, timeElapsed / fadeDuration));
        //    timeElapsed += Time.deltaTime;
        //    yield return null;
        //}
        fadeImage.color = new Color(0, 0, 0, 1);  // ������ �˰�

        // �� �ε�
        SceneManager.LoadScene(sceneName);

        // Fade In (ȭ���� �ٽ� �����ϰ� �����)
        timeElapsed = 0;
        while (timeElapsed < fadeDuration)
        {
            fadeImage.color = new Color(0, 0, 0, Mathf.Lerp(1, 0, timeElapsed / fadeDuration));
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 0);  // ������ �����ϰ� ����

        fadeImage.gameObject.SetActive(false);  // UI�� ����

        // ���� ��ȯ�� �� Canvas �� Image ����
        Destroy(fadeCanvas.gameObject);  // Canvas�� �� �ڽ� ������Ʈ�鵵 �Բ� ����
    }

    // �ڵ�� fadeImage (Image) ���� �� ����
    private void CreateFadeImage()
    {
        // Canvas ����
        fadeCanvas = new GameObject("FadeCanvas").AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;  // ȭ�鿡 ��������
        CanvasScaler canvasScaler = fadeCanvas.gameObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;  // UI ũ�� ȭ�鿡 ���߱�
        fadeCanvas.gameObject.AddComponent<GraphicRaycaster>(); // Ŭ�� �̺�Ʈ ���ܿ�

        // Canvas�� Image �߰�
        GameObject fadeObject = new GameObject("FadeImage");
        fadeObject.transform.SetParent(fadeCanvas.transform);  // Canvas�� �ڽ����� ����
        fadeImage = fadeObject.AddComponent<Image>();  // Image ������Ʈ �߰�

        // Image ����
        fadeImage.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);  // ȭ�� ũ�� ����
        fadeImage.rectTransform.anchorMin = new Vector2(0, 0);  // anchor�� (0,0)���� ����
        fadeImage.rectTransform.anchorMax = new Vector2(1, 1);  // anchor�� (1,1)���� ����
        fadeImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);  // pivot�� �߾����� ����

        fadeImage.color = new Color(0, 0, 0, 0);  // ó������ �����ϰ� ����
        fadeImage.gameObject.SetActive(false);  // �ʱ⿡�� ��Ȱ��ȭ

        // Canvas�� �� �ڽĵ��� �� ��ȯ �Ŀ��� �����ǰ� ����
        DontDestroyOnLoad(fadeCanvas.gameObject);  // Canvas�� �� �ڽĵ��� �� ��ȯ �Ŀ��� ����
    }

}
