using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public const string SaveFileName = "gamesave.json";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("SaveManager");
            Instance = go.AddComponent<SaveManager>();
            DontDestroyOnLoad(go);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.Respawn(targetPos);
            }
            else
            {
                player.transform.position = targetPos;
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.position = new Vector2(targetPos.x, targetPos.y);
                    rb.linearVelocity = Vector2.zero;
                }
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
            TaskManager.Instance.UpdateActiveTarget();
            TaskManager.Instance.SetImage();
            TaskManager.Instance.OnCurrentIndexChange?.Invoke();
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
