using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Put this on ONE empty GameObject in your scene.
/// Drag your manually built Pause UI into the "Pause UI" slot in the Inspector.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    /// <summary>Other scripts check this to ignore input while paused.</summary>
    public static bool IsPaused { get; private set; }

    [Header("UI References")]
    [Tooltip("Drag your Pause Menu Canvas or Panel here.")]
    public GameObject pauseUI; 

    [Header("Scene names")]
    public string mainMenuScene = "MainMenu";
    public string levelSelectScene = "LevelSelect";

    private CanvasGroup canvasGroup;

    void Start()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        // Safety check: Did you forget to assign the UI?
        if (pauseUI == null)
        {
            Debug.LogError("Pause UI is not assigned on " + gameObject.name);
            return;
        }

        // Try to get a CanvasGroup (good for blocking raycasts when hidden)
        canvasGroup = pauseUI.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            // If you don't have one, add it automatically
            canvasGroup = pauseUI.AddComponent<CanvasGroup>();
        }

        // Ensure UI is hidden at start
        pauseUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            SetPaused(!IsPaused);
    }

    void OnDestroy()
    {
        // Static state survives scene loads, so always clean up
        IsPaused = false;
        Time.timeScale = 1f;
    }

    public void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        
        if (pauseUI != null)
            pauseUI.SetActive(paused);

        // Show/hide mouse cursor based on pause state
        Cursor.visible = paused;
        // Lock cursor when playing (Optional - remove if you don't want it locked)
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    // ---------- BUTTON FUNCTIONS ----------
    // Link these in the OnClick() events of your UI Buttons

    public void Resume()
    {
        SetPaused(false);
    }

    public void RestartLevel()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        LoadScene(mainMenuScene);
    }

    public void GoToLevelSelect()
    {
        LoadScene(levelSelectScene);
    }

    void LoadScene(string sceneName)
    {
        // Must unpause first or the next scene starts frozen
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}