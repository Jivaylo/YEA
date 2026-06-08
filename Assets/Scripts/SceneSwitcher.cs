using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] private string sceneName;

    [Header("Return Point For This Portal")]
    [SerializeField] private Transform returnPoint;

    public void SwitchScene()
    {
        if (returnPoint != null)
        {
            ReturnPositionManager.SaveReturnPoint(returnPoint);
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                ReturnPositionManager.SaveReturnPoint(player.transform);
        }

        SceneManager.LoadScene(sceneName);
    }
}