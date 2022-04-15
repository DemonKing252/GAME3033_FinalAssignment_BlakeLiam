using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public enum Action
{
    Start        = 0,
    Quit         = 1,
    PlayAgain    = 2,
    MainMenu     = 3,
    Victory      = 4,
    Defeat       = 5,
    Instructions = 6,
    Pause        = 7,
    Resume       = 8,
    SaveGame     = 9,
    LoadGame     = 10
}
public class MenuController : MonoBehaviour
{
    public delegate void SaveGameEvent();
    public delegate void LoadGameEvent();
    public event SaveGameEvent onSaveGame;
    public event LoadGameEvent onLoadGame;


    [SerializeField] private TMP_Text playerSkillText;

    [SerializeField] Canvas gameCanvas;
    [SerializeField] Canvas menuCanvas;
    [SerializeField] Canvas loseCanvas;
    [SerializeField] Canvas winCanvas;
    [SerializeField] Canvas instructionsCanvas;
    [SerializeField] Canvas pauseCanvas;

    private bool isPaused = false;
    private static MenuController instance;
    public static MenuController Instance => instance;

    void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 0f;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;

            if (isPaused)
                OnAction((int)Action.Pause);
            else
                OnAction((int)Action.Resume);
        }
    }
    public void OnActionByEnum(Action action)
    {
        OnAction((int)action);
    }

    public void OnAction(int action)
    {
        AudioManager.Instance.PlaySound(Sfx.BtnClick);

        switch ((Action)action)
        {
            case Action.Quit:
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
                break;
            case Action.MainMenu:
                Cursor.lockState = CursorLockMode.None;
                gameCanvas.gameObject.SetActive(false);
                menuCanvas.gameObject.SetActive(true);
                loseCanvas.gameObject.SetActive(false);
                winCanvas.gameObject.SetActive(false);
                instructionsCanvas.gameObject.SetActive(false);
                pauseCanvas.gameObject.SetActive(false);
                Time.timeScale = 0f;
                SceneManager.LoadScene("Main");
                break;
            case Action.Pause:

                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 0f;
                gameCanvas.gameObject.SetActive(false);
                menuCanvas.gameObject.SetActive(false);
                loseCanvas.gameObject.SetActive(false);
                winCanvas.gameObject.SetActive(false);
                instructionsCanvas.gameObject.SetActive(false);
                pauseCanvas.gameObject.SetActive(true);
                break;
            case Action.Resume:

                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                gameCanvas.gameObject.SetActive(false);
                menuCanvas.gameObject.SetActive(false);
                loseCanvas.gameObject.SetActive(false);
                winCanvas.gameObject.SetActive(false);
                instructionsCanvas.gameObject.SetActive(false);
                pauseCanvas.gameObject.SetActive(false);
                break;
            case Action.Start:
                Cursor.lockState = CursorLockMode.Locked;
                gameCanvas.gameObject.SetActive(true);
                menuCanvas.gameObject.SetActive(false);
                loseCanvas.gameObject.SetActive(false);
                winCanvas.gameObject.SetActive(false);
                instructionsCanvas.gameObject.SetActive(false);
                pauseCanvas.gameObject.SetActive(false);
                Time.timeScale = 1f;
                GameDataManager.Instance.Reset();

                break;
            case Action.LoadGame:
                Cursor.lockState = CursorLockMode.Locked;
                gameCanvas.gameObject.SetActive(true);
                menuCanvas.gameObject.SetActive(false);
                loseCanvas.gameObject.SetActive(false);
                winCanvas.gameObject.SetActive(false);
                instructionsCanvas.gameObject.SetActive(false);
                pauseCanvas.gameObject.SetActive(false);
                Time.timeScale = 1f;
                LoadGame();
                break;
            case Action.SaveGame:
                Cursor.lockState = CursorLockMode.None;
                gameCanvas.gameObject.SetActive(false);
                menuCanvas.gameObject.SetActive(true);
                loseCanvas.gameObject.SetActive(false);
                winCanvas.gameObject.SetActive(false);
                instructionsCanvas.gameObject.SetActive(false);
                pauseCanvas.gameObject.SetActive(false);
                Time.timeScale = 0f;
                SaveAndQuit();
                SceneManager.LoadScene("Main");
                break;
            case Action.Instructions:

                Cursor.lockState = CursorLockMode.None;
                gameCanvas.gameObject.SetActive(false);
                menuCanvas.gameObject.SetActive(false);
                loseCanvas.gameObject.SetActive(false);
                winCanvas.gameObject.SetActive(false);
                instructionsCanvas.gameObject.SetActive(true);
                pauseCanvas.gameObject.SetActive(false);
                break;

            case Action.Victory:
                Cursor.lockState = CursorLockMode.None;
                gameCanvas.gameObject.SetActive(false);
                menuCanvas.gameObject.SetActive(false);
                loseCanvas.gameObject.SetActive(false);
                winCanvas.gameObject.SetActive(true);
                instructionsCanvas.gameObject.SetActive(false);
                pauseCanvas.gameObject.SetActive(false);
                Time.timeScale = 0f;
                break;
            case Action.Defeat:
                Cursor.lockState = CursorLockMode.None;
                gameCanvas.gameObject.SetActive(false);
                menuCanvas.gameObject.SetActive(false);
                loseCanvas.gameObject.SetActive(true);
                winCanvas.gameObject.SetActive(false);
                instructionsCanvas.gameObject.SetActive(false);
                pauseCanvas.gameObject.SetActive(false);
                Time.timeScale = 0f;
                break;
            case Action.PlayAgain:
                Cursor.lockState = CursorLockMode.None;
                SceneManager.LoadScene("Main");

                break;
        }
    }
    private void LoadGame()
    {
        onLoadGame?.Invoke();
        //Debug.Log("Loading game...");
    }
    private void SaveAndQuit()
    {
        onSaveGame?.Invoke();
        //Debug.Log("Saving game...");
    }

}
