using UnityEngine;

public class PlayerModeSwitcher : MonoBehaviour
{
    [Header("Third Person")]
    public MonoBehaviour thirdPersonController;
    public MonoBehaviour thirdPersonCameraScript;
    public Camera thirdPersonCamera;

    [Header("First Person Puzzle")]
    public MonoBehaviour firstPersonController;
    public MonoBehaviour puzzleInteraction;
    public Camera puzzleCamera;

    [Header("Visuals")]
    public GameObject lowPolyMan;

    public bool inPuzzleMode = false;

    public void EnterPuzzleMode()
    {
        inPuzzleMode = true;

        if (thirdPersonController != null) thirdPersonController.enabled = false;
        if (thirdPersonCameraScript != null) thirdPersonCameraScript.enabled = false;
        if (thirdPersonCamera != null) thirdPersonCamera.enabled = false;

        if (lowPolyMan != null) lowPolyMan.SetActive(false);

        if (firstPersonController != null) firstPersonController.enabled = true;
        if (puzzleInteraction != null) puzzleInteraction.enabled = true;
        if (puzzleCamera != null) puzzleCamera.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ExitPuzzleMode()
    {
        inPuzzleMode = false;

        if (firstPersonController != null) firstPersonController.enabled = false;
        if (puzzleInteraction != null) puzzleInteraction.enabled = false;
        if (puzzleCamera != null) puzzleCamera.enabled = false;

        if (lowPolyMan != null) lowPolyMan.SetActive(true);

        if (thirdPersonController != null) thirdPersonController.enabled = true;
        if (thirdPersonCameraScript != null) thirdPersonCameraScript.enabled = true;
        if (thirdPersonCamera != null) thirdPersonCamera.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}