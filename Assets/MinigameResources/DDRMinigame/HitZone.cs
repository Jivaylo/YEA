using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class HitZone : MonoBehaviour
{
    private Note currentNote;
    private TraumaInducer inducer;
    [SerializeField] private NoteSpawner spawner;
    [SerializeField] private ParticleSystem particles;

    [SerializeField] private InputActionAsset inputActionsAsset;

    [Tooltip("How much score a single correctly-hit arrow gives.")]
    [SerializeField] private int scorePerHit = 100;

    private KeyControl upKey, downKey, leftKey, rightKey;

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Note note))
            currentNote = note;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Note note))
            if (currentNote == note)
                currentNote = null;
    }

    private void Start()
    {
        inducer = GetComponent<TraumaInducer>();
        LoadBoundKeys();
    }

    private void LoadBoundKeys()
    {
        // Defaults — used if asset isn't assigned or a binding can't be resolved
        upKey    = Keyboard.current.wKey;
        downKey  = Keyboard.current.sKey;
        leftKey  = Keyboard.current.aKey;
        rightKey = Keyboard.current.dKey;

        if (inputActionsAsset == null) return;

        // Clone so we don't mutate the shared asset
        var asset = Instantiate(inputActionsAsset);
        string json = PlayerPrefs.GetString("InputBindingOverrides", "");
        if (!string.IsNullOrEmpty(json))
            asset.LoadBindingOverridesFromJson(json);

        var moveAction = asset.FindActionMap("Player")?.FindAction("Move");
        if (moveAction == null) { Destroy(asset); return; }

        // Move composite binding indices: 0=composite, 1=up, 2=down, 3=left, 4=right
        upKey    = KeyFromPath(moveAction.bindings[1].effectivePath) ?? upKey;
        downKey  = KeyFromPath(moveAction.bindings[2].effectivePath) ?? downKey;
        leftKey  = KeyFromPath(moveAction.bindings[3].effectivePath) ?? leftKey;
        rightKey = KeyFromPath(moveAction.bindings[4].effectivePath) ?? rightKey;

        Destroy(asset);
    }

    private KeyControl KeyFromPath(string path)
    {
        if (Keyboard.current == null || string.IsNullOrEmpty(path)) return null;
        int slash = path.LastIndexOf('/');
        if (slash < 0) return null;
        return Keyboard.current[path.Substring(slash + 1)] as KeyControl;
    }

    void Update()
    {
        if (currentNote == null) return;

        if ((upKey != null && upKey.wasPressedThisFrame) || Keyboard.current.upArrowKey.wasPressedThisFrame)
            Hit(Direction.Up);
        else if ((downKey != null && downKey.wasPressedThisFrame) || Keyboard.current.downArrowKey.wasPressedThisFrame)
            Hit(Direction.Down);
        else if ((leftKey != null && leftKey.wasPressedThisFrame) || Keyboard.current.leftArrowKey.wasPressedThisFrame)
            Hit(Direction.Left);
        else if ((rightKey != null && rightKey.wasPressedThisFrame) || Keyboard.current.rightArrowKey.wasPressedThisFrame)
            Hit(Direction.Right);
    }

    void Hit(Direction dir)
    {
        if (currentNote == null) return;

        if (currentNote.direction == dir)
        {
            var main = particles.main;
            main.startColor = currentNote.CurrentColor;
            Destroy(currentNote.gameObject);
            spawner.AddScore(scorePerHit);
            particles.Play();
            currentNote = null;
        }
    }
}
