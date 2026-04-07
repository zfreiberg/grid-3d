using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreditsMenuController : MonoBehaviour
{
    void Start()
    {
        var go = GameObject.Find("BackButton");
        if (go != null)
            go.GetComponent<Button>()?.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
    }
}
