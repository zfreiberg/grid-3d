using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    void Start()
    {
        Bind("StartButton",   OnStartClicked);
        Bind("OptionsButton", () => SceneManager.LoadScene("Options"));
        Bind("CreditsButton", () => SceneManager.LoadScene("Credits"));
        Bind("ExitButton",    OnExitClicked);
    }

    void Bind(string goName, UnityEngine.Events.UnityAction action)
    {
        var go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"MainMenuController: '{goName}' not found."); return; }
        var btn = go.GetComponent<Button>();
        if (btn == null) { Debug.LogWarning($"MainMenuController: '{goName}' has no Button."); return; }
        btn.onClick.AddListener(action);
    }

    void OnStartClicked()
    {
        // Tear down any existing run and start fresh
        if (RunManager.Instance != null)
            Destroy(RunManager.Instance.gameObject);

        var go = new GameObject("RunManager");
        var rm = go.AddComponent<RunManager>();
        rm.StartNewRun(new MapSettings());
        SceneManager.LoadScene("MapScene");
    }

    void OnExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
