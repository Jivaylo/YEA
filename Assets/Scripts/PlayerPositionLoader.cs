using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPositionLoader : MonoBehaviour
{
    [SerializeField] private string museumSceneName = "yeajasper14-4";

    void Awake()
    {
        if (SceneManager.GetActiveScene().name != museumSceneName)
            return;

        if (PlayerPrefs.GetInt("HasReturnPosition", 0) != 1)
            return;

        Vector3 pos = new Vector3(
            PlayerPrefs.GetFloat("PlayerX"),
            PlayerPrefs.GetFloat("PlayerY"),
            PlayerPrefs.GetFloat("PlayerZ")
        );

        Quaternion rot = Quaternion.Euler(
            0f,
            PlayerPrefs.GetFloat("PlayerRotY"),
            0f
        );

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        transform.SetPositionAndRotation(pos, rot);

        if (cc != null) cc.enabled = true;

        Debug.Log("Loaded return position in museum: " + pos);

        ReturnPositionManager.ClearReturnPoint();
    }
}