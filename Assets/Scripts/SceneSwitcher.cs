using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] private string sceneName;

    public void SwitchScene()
    {
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            PlayerPrefs.SetFloat("PlayerX", player.transform.position.x);
            PlayerPrefs.SetFloat("PlayerY", player.transform.position.y);
            PlayerPrefs.SetFloat("PlayerZ", player.transform.position.z);
            PlayerPrefs.SetFloat("PlayerRotY", player.transform.eulerAngles.y);
            PlayerPrefs.SetInt("HasReturnPosition", 1);
            PlayerPrefs.Save();
        }

        SceneManager.LoadScene(sceneName);
    }
}