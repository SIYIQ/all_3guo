using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Panels")]
    public GameState CurrentState;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ChangeGameState(GameState.StartMenu);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && CurrentState == GameState.LevelPlaying)
        {
            PauseGame();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && CurrentState == GameState.Paused)
        {
            ResumeGame();
        }
    }

    public void ChangeGameState(GameState newState)
    {
        ExitState(CurrentState);
        EnterState(newState);
        CurrentState = newState;
    }

    private void EnterState(GameState state)
    {
        switch (state)
        {
            case GameState.StartMenu:
                UIManager.Instance.HideAllPanels();
                UIManager.Instance.ShowPanel(UIPanelType.StartPanel);
                Time.timeScale = 1;
                break;

            case GameState.LevelPlaying:
                UIManager.Instance.HideAllPanels();
                UIManager.Instance.ShowPanel(UIPanelType.LevelUIPanel);
                Time.timeScale = 1;
                break;

            case GameState.Paused:
                UIManager.Instance.HidePanel(UIPanelType.LevelUIPanel);
                UIManager.Instance.ShowPanel(UIPanelType.PausePanel);
                Time.timeScale = 0;
                break;

            case GameState.LevelComplete:
                UIManager.Instance.HidePanel(UIPanelType.LevelUIPanel);
                UIManager.Instance.ShowPanel(UIPanelType.CompletePanel);
                Time.timeScale = 1;
                break;

            case GameState.LevelPass:
                UIManager.Instance.ShowPanel(UIPanelType.PassPanel);
                Time.timeScale = 0;
                break;
        }
    }

    private void ExitState(GameState state)
    {
        switch (state)
        {
            case GameState.Paused:
                UIManager.Instance.HidePanel(UIPanelType.PausePanel);
                break;

            case GameState.LevelComplete:
                UIManager.Instance.HidePanel(UIPanelType.CompletePanel);
                break;

            default:
                break;
        }
    }

    #region Quick Actions
    public void StartGame()
    {
        LevelManager.Instance.LoadLevel(1);
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.LevelPlaying)
        {
            ChangeGameState(GameState.Paused);
        }
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            ChangeGameState(GameState.LevelPlaying);
        }
    }

    public void BackToStartMenu()
    {
        ChangeGameState(GameState.StartMenu);
    }

    public void RestartCurrentLevel()
    {
        LevelManager.Instance.ReloadCurrentLevel();
    }

    public void GoToNextLevel()
    {
        LevelManager.Instance.LoadNextLevel();
    }
    #endregion

    public void ExitGame()
    {
        Application.Quit();
    }
}
