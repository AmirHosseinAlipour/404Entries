using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance;
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<SaveManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SaveManager");
                    _instance = go.AddComponent<SaveManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    public const string SaveFileName = "gamesave.json";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        if (_instance == null)
        {
            GameObject go = new GameObject("SaveManager");
            _instance = go.AddComponent<SaveManager>();
            DontDestroyOnLoad(go);
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0 && HasSaveFile())
        {
            StartCoroutine(ApplySaveNextFrame());
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        bool jPressed = false;
        if (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame)
        {
            jPressed = true;
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            jPressed = true;
        }

        if (jPressed)
        {
            DeleteSaveAndRestart();
        }
    }

    public string GetSaveFilePath()
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName);
    }

    public bool HasSaveFile()
    {
        return File.Exists(GetSaveFilePath());
    }

    public void SaveGame(Vector3? overridePlayerPos = null)
    {
        SaveData data = new SaveData();
        data.saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        data.sceneName = SceneManager.GetActiveScene().name;

        Vector3 playerPosition = Vector3.zero;
        if (overridePlayerPos.HasValue)
        {
            playerPosition = overridePlayerPos.Value;
        }
        else
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerPosition = playerObj.transform.position;
            }
        }
        data.playerX = playerPosition.x;
        data.playerY = playerPosition.y;
        data.playerZ = playerPosition.z;

        if (TaskManager.Instance != null && TaskManager.Instance.taskCompleted != null)
        {
            data.taskCompleted = (bool[])TaskManager.Instance.taskCompleted.Clone();
            data.currentTaskIndex = TaskManager.Instance.GetCurrentTaskIndex();
        }

        if (SelectionPlayer.instance != null)
        {
            data.playerID = SelectionPlayer.instance.PlayerID;
        }

        data.language = (int)LocalizationManager.CurrentLanguage;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSaveFilePath(), json);
        Debug.Log("[SaveManager] Save written to: " + GetSaveFilePath() + " | Task Index: " + data.currentTaskIndex + " | Player Position: " + playerPosition);
    }

    public bool LoadGame()
    {
        string path = GetSaveFilePath();
        if (!File.Exists(path))
        {
            return false;
        }

        string json = File.ReadAllText(path);
        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        SaveData data = JsonUtility.FromJson<SaveData>(json);
        if (data == null)
        {
            return false;
        }

        ApplySaveData(data);
        Debug.Log("[SaveManager] Save loaded successfully! Task Index: " + data.currentTaskIndex + " | Player Position: (" + data.playerX + ", " + data.playerY + ", " + data.playerZ + ")");
        return true;
    }

    public void ApplySaveData(SaveData data)
    {
        LocalizationManager.SetLanguage((GameLanguage)data.language);

        if (SelectionPlayer.instance != null)
        {
            SelectionPlayer.instance.PlayerID = data.playerID;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Vector3 targetPos = new Vector3(data.playerX, data.playerY, data.playerZ);
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.position = new Vector2(targetPos.x, targetPos.y);
            }
            player.transform.position = targetPos;
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.Respawn(targetPos);
            }
        }

        if (TaskManager.Instance != null && data.taskCompleted != null && TaskManager.Instance.taskCompleted != null)
        {
            int count = Mathf.Min(data.taskCompleted.Length, TaskManager.Instance.taskCompleted.Length);
            for (int i = 0; i < count; i++)
            {
                TaskManager.Instance.taskCompleted[i] = data.taskCompleted[i];
                if (TaskManager.Instance.taskDataList != null && i < TaskManager.Instance.taskDataList.Count && TaskManager.Instance.taskDataList[i] != null)
                {
                    TaskManager.Instance.taskDataList[i].isCompleted = data.taskCompleted[i];
                }
            }
            TaskManager.Instance.SetImage();
            TaskManager.Instance.NotifyTasksUpdated();
        }

        BaseProfessors.IsAnyMinigameActive = false;
        BaseProfessors.ActiveProfessor = null;
        TaskUI.SetTopUIVisible(true);

        TaskUI taskUI = FindAnyObjectByType<TaskUI>();
        if (taskUI != null)
        {
            taskUI.UpdateUI();
        }

        if (GameManager.Instance != null && TaskManager.Instance != null)
        {
            GameManager.Instance.ChangeCurrentIndex();
            int currentIdx = TaskManager.Instance.GetCurrentTaskIndex();
            if (GameManager.Instance.taskTriggers != null)
            {
                for (int i = 0; i < GameManager.Instance.taskTriggers.Count; i++)
                {
                    if (GameManager.Instance.taskTriggers[i] == null) continue;
                    Collider2D col = GameManager.Instance.taskTriggers[i].GetComponent<Collider2D>();
                    Target target = GameManager.Instance.taskTriggers[i].GetComponent<Target>();
                    if (i == currentIdx)
                    {
                        if (col != null) col.isTrigger = true;
                        if (target != null) target.EnableTarget();
                    }
                    else
                    {
                        if (col != null) col.isTrigger = false;
                        if (target != null) target.DisableTarget();
                    }
                }
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex != 0)
        {
            StartCoroutine(ApplySaveNextFrame());
        }
    }

    private IEnumerator ApplySaveNextFrame()
    {
        yield return null;
        if (HasSaveFile())
        {
            LoadGame();
        }
        yield return new WaitForFixedUpdate();
        if (HasSaveFile())
        {
            LoadGame();
        }
    }

    public void DeleteSaveAndRestart()
    {
        string path = GetSaveFilePath();
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        if (SelectionPlayer.instance != null)
        {
            Destroy(SelectionPlayer.instance.gameObject);
        }
        SceneManager.LoadScene(0);
    }
}
