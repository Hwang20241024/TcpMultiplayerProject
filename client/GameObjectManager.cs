using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameObjectManager : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    private static GameObjectManager _instance;

    public static GameObjectManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // �ν��Ͻ��� ������ ���� ������Ʈ���� �߰��ϰ�, �̱��� ��ü�� ����
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

    // ���� ������Ʈ���� �����ϴ� ��ųʸ�
    private Dictionary<string, GameObject> gameObjects;


    void Awake()
    {
        // ������ �ν��Ͻ��� ���ٸ� �����ϰ�, ������ �̹� �����ϴ� �ν��Ͻ��� ���
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // ���� ����Ǿ ���� ������Ʈ�� �����ǵ��� ����
        }
        else
        {
            Destroy(gameObject); // �ߺ� ���� ����
        }

        gameObjects = new Dictionary<string, GameObject>();
    }

    // ���� ������Ʈ �߰�
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

    // ���� ������Ʈ ��������
    public GameObject GetGameObject(string id)
    {
        if (gameObjects.ContainsKey(id))
        {
            return gameObjects[id];
        }
        return null;
    }

    // ���� ������Ʈ ����
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

    // ��� ���� ������Ʈ ����
    public void RemoveAllGameObjects()
    {
        // ��ųʸ��� ��� ���� ������Ʈ ����
        foreach (var gameObject in gameObjects.Values)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        // ��ųʸ� �ʱ�ȭ
        gameObjects.Clear();
    }


    // ��� ������Ʈ ��Ȱ��ȭ
    public void DeactivateAllGameObjects()
    {
        foreach (var gameObject in gameObjects.Values)
        {
            gameObject.SetActive(false);
        }
    }

    // ��� ������Ʈ Ȱ��ȭ
    public void ActivateAllGameObjects()
    {
        foreach (var gameObject in gameObjects.Values)
        {
            gameObject.SetActive(true);
        }
    }

    // ���� ������Ʈ�� ���� ������Ʈ
    public void UpdateGameObjectState(string id, bool isActive)
    {
        GameObject gameObject = GetGameObject(id);
        if (gameObject != null)
        {
            gameObject.SetActive(isActive);
        }
    }

    // ���׸� �Լ� <- ������Ʈ�� ���ѵȰ� �ƴϴ�. ���߿� �ٽ� �˻��غ���.
    // ( ������Ʈ Ÿ���� �Ű������� �޾Ƽ� �ش� Ÿ���� ������Ʈ�� ���� ������Ʈ���� �������� ����� ó���� �����ϴ� �Լ�)
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
                Debug.LogWarning($"�ش� ���ӿ�����Ʈ�� {typeof(T)} ������Ʈ�� ������ ���� �ʽ��ϴ�.");
                return null;
            }
        }
        else
        {
            Debug.LogWarning("�Է��Ͻ� ���ӿ�����Ʈ�� �������� �ʽ��ϴ�.");
            return null;
        }
    }
    
    // ���۳�Ʈ ã��
    public InputField GetInputField(string id)
    {
        return GetComponentFromGameObject<InputField>(id);
    }

    public Text GetText(string id)
    {
        return GetComponentFromGameObject<Text>(id);
    }

    // �˶�Ʈ 
    public void AlertText(string str)
    {
        GameObject errorPanel = GetGameObject("ErrorPanel");
        Text errorText = GameObjectManager.Instance.GetText("ErrorPanelText");
        errorText.text = str;
        errorPanel.SetActive(true);
    }


    // �޼��� ����. (ä��, ��ŷ, ���)(Text)
    private GameObject CreateMessagePrefab()
    {
        // �޽����� ���� �������� �������� ����
        GameObject messageObj = new GameObject("Message", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
        TMPro.TextMeshProUGUI messageText = messageObj.GetComponent<TMPro.TextMeshProUGUI>();

        // TMP_FontAsset �ε�
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
        messageText.fontSize = 28; // ��Ʈ ũ�� 3��� ����
        messageText.color = Color.black;

        RectTransform rectTransform = messageObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(380 * 2, 30 * 2); // �޽��� ũ�� 3��� ����
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

        // Content�� �� �޽����� �߰�
        GameObject newMessage = Instantiate(chatMessagePrefab, content.transform);
        TMPro.TextMeshProUGUI messageText = newMessage.GetComponent<TMPro.TextMeshProUGUI>();
        messageText.text = message;

        // Content ���� ����
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, content.transform.childCount * 30); // �޽��� ������ ���� Content�� ũ�� ����

        // ��ũ�� �Ʒ��� �̵�
        Canvas.ForceUpdateCanvases();
        ScrollRect scrollRect = scrollView.GetComponent<ScrollRect>();
        scrollRect.verticalNormalizedPosition = 0; // ��ũ���� ���� �Ʒ��� �̵�
    }

    public void ClearMessages(string str)
    {
        
        GameObject content = GetGameObject(str);

        // Content�� �ڽ� ��ü���� ��� ����
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject); // �ڽ� ��ü ����
        }
    }

}
