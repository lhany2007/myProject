using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneDiversion : MonoBehaviour
{
    Button startButton;

    void Start()
    {
        startButton = GetComponent<Button>();
        startButton.onClick.AddListener(() => OnButtonClick());
    }

    void OnButtonClick()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        NextScene.Instance.NumNext(currentSceneIndex + 1);
    }
}
