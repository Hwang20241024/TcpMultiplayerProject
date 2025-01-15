using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameObjectManager : MonoBehaviour
{
    // 싱글턴 인스턴스
    private static GameObjectManager _instance;

    public static GameObjectManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 인스턴스가 없으면 게임 오브젝트에서 추가하고, 싱글턴 객체를 설정
                _instance = FindObjectOfType<GameObjectManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("GameObjectManager");
                    _instance = obj.AddComponent<GameObjectManager>();
                }
            }
            return _instance;
        }
    }

    // 게임 오브젝트들을 관리하는 딕셔너리
    private Dictionary<string, GameObject> gameObjects;


    void Awake()
    {
        // 기존에 인스턴스가 없다면 설정하고, 있으면 이미 존재하는 인스턴스를 사용
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 변경되어도 게임 오브젝트가 유지되도록 설정
        }
        else
        {
            Destroy(gameObject); // 중복 생성 방지
        }

        gameObjects = new Dictionary<string, GameObject>();
    }

    // 게임 오브젝트 추가
    public void AddGameObject(string id, GameObject gameObject)
    {
        if (!gameObjects.ContainsKey(id))
        {
            gameObjects.Add(id, gameObject);
        }
        else
        {
            Debug.LogWarning("GameObject with this ID already exists.");
        }
    }

    // 게임 오브젝트 가져오기
    public GameObject GetGameObject(string id)
    {
        if (gameObjects.ContainsKey(id))
        {
            return gameObjects[id];
        }
        return null;
    }

    // 게임 오브젝트 삭제
    public void RemoveGameObject(string id)
    {
        if (gameObjects.ContainsKey(id))
        {
            Destroy(gameObjects[id]);
            gameObjects.Remove(id);
        }
        else
        {
            Debug.LogWarning("No GameObject with this ID.");
        }
    }

    // 모든 게임 오브젝트 삭제
    public void RemoveAllGameObjects()
    {
        // 딕셔너리의 모든 게임 오브젝트 삭제
        foreach (var gameObject in gameObjects.Values)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        // 딕셔너리 초기화
        gameObjects.Clear();
    }


    // 모든 오브젝트 비활성화
    public void DeactivateAllGameObjects()
    {
        foreach (var gameObject in gameObjects.Values)
        {
            gameObject.SetActive(false);
        }
    }

    // 모든 오브젝트 활성화
    public void ActivateAllGameObjects()
    {
        foreach (var gameObject in gameObjects.Values)
        {
            gameObject.SetActive(true);
        }
    }

    // 게임 오브젝트의 상태 업데이트
    public void UpdateGameObjectState(string id, bool isActive)
    {
        GameObject gameObject = GetGameObject(id);
        if (gameObject != null)
        {
            gameObject.SetActive(isActive);
        }
    }

    // 제네릭 함수 <- 컴포넌트에 국한된게 아니다. 나중에 다시 검색해보자.
    // ( 컴포넌트 타입을 매개변수로 받아서 해당 타입의 컴포넌트를 게임 오브젝트에서 가져오는 공통된 처리를 수행하는 함수)
    public T GetComponentFromGameObject<T>(string id) where T : Component
    {
        GameObject gameObject = GetGameObject(id);
        if (gameObject != null)
        {
            T component = gameObject.GetComponent<T>();
            if (component != null)
            {
                return component;
            }
            else
            {
                Debug.LogWarning($"해당 게임오브젝트는 {typeof(T)} 컴포넌트를 가지고 있지 않습니다.");
                return null;
            }
        }
        else
        {
            Debug.LogWarning("입력하신 게임오브젝트는 존재하지 않습니다.");
            return null;
        }
    }
    
    // 컴퍼넌트 찾기
    public InputField GetInputField(string id)
    {
        return GetComponentFromGameObject<InputField>(id);
    }

    public Text GetText(string id)
    {
        return GetComponentFromGameObject<Text>(id);
    }

    // 알라트 
    public void AlertText(string str)
    {
        GameObject errorPanel = GetGameObject("ErrorPanel");
        Text errorText = GameObjectManager.Instance.GetText("ErrorPanelText");
        errorText.text = str;
        errorPanel.SetActive(true);
    }


    // 메세지 생성. (채팅, 랭킹, 목록)(Text)
    private GameObject CreateMessagePrefab()
    {
        // 메시지를 담을 프리팹을 동적으로 생성
        GameObject messageObj = new GameObject("Message", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
        TMPro.TextMeshProUGUI messageText = messageObj.GetComponent<TMPro.TextMeshProUGUI>();

        // TMP_FontAsset 로드
        TMPro.TMP_FontAsset customFont = Resources.Load<TMPro.TMP_FontAsset>("Fonts/NanumGothicBold SDF");

        if (customFont != null)
        {
            messageText.font = customFont;
        }
        else
        {
            Debug.LogError("TMP Font not found!");
        }

        messageText.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        messageText.fontSize = 28; // 폰트 크기 3배로 증가
        messageText.color = Color.black;

        RectTransform rectTransform = messageObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(380 * 2, 30 * 2); // 메시지 크기 3배로 증가
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);

        return messageObj;
    }

    public void AddMessage(string obj1, string obj2 ,string message)
    {
        GameObject chatMessagePrefab = CreateMessagePrefab();

        //GameObject content = GetGameObject("LobbyPanel02ScrollViewContent");
        //GameObject scrollView = GetGameObject("LobbyPanel02ScrollView");
        GameObject content = GetGameObject(obj1);
        GameObject scrollView = GetGameObject(obj2);

        // Content에 새 메시지를 추가
        GameObject newMessage = Instantiate(chatMessagePrefab, content.transform);
        TMPro.TextMeshProUGUI messageText = newMessage.GetComponent<TMPro.TextMeshProUGUI>();
        messageText.text = message;

        // Content 높이 조정
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, content.transform.childCount * 30); // 메시지 개수에 따라 Content의 크기 조정

        // 스크롤 아래로 이동
        Canvas.ForceUpdateCanvases();
        ScrollRect scrollRect = scrollView.GetComponent<ScrollRect>();
        scrollRect.verticalNormalizedPosition = 0; // 스크롤을 제일 아래로 이동
    }

    public void ClearMessages(string str)
    {
        
        GameObject content = GetGameObject(str);

        // Content의 자식 객체들을 모두 삭제
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject); // 자식 객체 삭제
        }
    }

}
