using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    void Start()
    {
        Bind("StartButton",   () => SceneManager.LoadScene("GridScene01"));
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

    void OnExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
