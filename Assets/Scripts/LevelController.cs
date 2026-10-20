using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    [System.Serializable]
    public class LevelEntry
    {
        public string displayName;   // e.g. "Level 1"
        public string sceneName;     // must match Build Settings exactly
        public Button button;        // the button in LevelSelect
    }

    [Header("Levels")]
    public LevelEntry[] levels;

    void Start()
    {
        foreach (var level in levels)
        {
            if (level.button == null) continue;

            // Capture into a local variable (closure safety)
            string sceneToLoad = level.sceneName;
            level.button.onClick.AddListener(() => LoadLevel(sceneToLoad));
        }
    }

    public void LoadLevel(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogError($"Scene '{sceneName}' is not in Build Settings!");
    }

    // Optional: back to MainMenu
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}