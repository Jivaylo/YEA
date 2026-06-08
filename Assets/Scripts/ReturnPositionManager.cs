using UnityEngine;

public static class ReturnPositionManager
{
    public static void SaveReturnPoint(Transform point)
    {
        if (point == null) return;

        PlayerPrefs.SetFloat("PlayerX", point.position.x);
        PlayerPrefs.SetFloat("PlayerY", point.position.y);
        PlayerPrefs.SetFloat("PlayerZ", point.position.z);
        PlayerPrefs.SetFloat("PlayerRotY", point.eulerAngles.y);
        PlayerPrefs.SetInt("HasReturnPosition", 1);
        PlayerPrefs.Save();

        Debug.Log("Saved return point: " + point.name + " / " + point.position);
    }

    public static void ClearReturnPoint()
    {
        PlayerPrefs.DeleteKey("HasReturnPosition");
        PlayerPrefs.DeleteKey("PlayerX");
        PlayerPrefs.DeleteKey("PlayerY");
        PlayerPrefs.DeleteKey("PlayerZ");
        PlayerPrefs.DeleteKey("PlayerRotY");
        PlayerPrefs.Save();
    }
}