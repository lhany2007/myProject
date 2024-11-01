using UnityEngine;
using UnityEngine.SceneManagement;

public class NextScene : MonoBehaviour
{
    public static NextScene Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void NumNext(int num)
    {
        SceneManager.LoadScene(num);
    }

    public void NameNext(string name)
    {
        SceneManager.LoadScene(name);
    }
}
