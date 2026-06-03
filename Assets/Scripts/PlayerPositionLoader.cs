using UnityEngine;

public class PlayerPositionLoader : MonoBehaviour
{
    void Start()
    {
        if (PlayerPrefs.HasKey("PlayerX"))
        {
            transform.position = new Vector3(
                PlayerPrefs.GetFloat("PlayerX"),
                PlayerPrefs.GetFloat("PlayerY"),
                PlayerPrefs.GetFloat("PlayerZ")
            );

            transform.rotation = Quaternion.Euler(
                0,
                PlayerPrefs.GetFloat("PlayerRotY"),
                0
            );
        }
    }
}
