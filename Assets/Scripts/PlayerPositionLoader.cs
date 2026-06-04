using UnityEngine;

public class PlayerPositionLoader : MonoBehaviour
{
    void Start()
    {
        if (PlayerPrefs.GetInt("HasReturnPosition", 0) != 1)
            return;

        CharacterController controller = GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        transform.position = new Vector3(
            PlayerPrefs.GetFloat("PlayerX"),
            PlayerPrefs.GetFloat("PlayerY"),
            PlayerPrefs.GetFloat("PlayerZ")
        );

        transform.rotation = Quaternion.Euler(
            0f,
            PlayerPrefs.GetFloat("PlayerRotY"),
            0f
        );

        if (controller != null)
            controller.enabled = true;

        PlayerPrefs.DeleteKey("HasReturnPosition");
        PlayerPrefs.DeleteKey("PlayerX");
        PlayerPrefs.DeleteKey("PlayerY");
        PlayerPrefs.DeleteKey("PlayerZ");
        PlayerPrefs.DeleteKey("PlayerRotY");
        PlayerPrefs.Save();
    }
}