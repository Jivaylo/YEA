using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] private string sceneName;

    public void SwitchScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}