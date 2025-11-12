using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NarrationSystem : MonoBehaviour {
    [Header("UI")]
    [SerializeField] private GameObject panelSubtitles;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;

    [Header("Clavier")]
    [SerializeField] private KeyCode nextKey = KeyCode.E;
    [SerializeField] private KeyCode closeKey = KeyCode.X;

    [Header("Optionnel")]
    [SerializeField] private PlayerControllerCC player;

    private DialogueAsset current;
    private int index = -1;
    public bool IsPlaying => current != null;

    public System.Action<string> OnDialogueStarted;
    public System.Action<string> OnDialogueEnded;

    private void Start() {
        if (panelSubtitles) panelSubtitles.SetActive(false);
        if (nextButton) nextButton.onClick.AddListener(Next);
        if (skipButton) skipButton.onClick.AddListener(Close);
    }

    private void Update() {
        if (!IsPlaying) return;
        if (Input.GetKeyDown(nextKey)) Next();
        if (Input.GetKeyDown(closeKey)) Close();
    }

    public void StartDialogue(DialogueAsset asset) {
        if (IsPlaying || asset == null) return;
        current = asset;
        index = -1;

        if (panelSubtitles) panelSubtitles.SetActive(true);
        OnDialogueStarted?.Invoke(current.id);

        if (player) player.SetInputEnabled(false);
        Next();
    }

    public void Next() {
        if (!IsPlaying) return;
        index++;

        if (current.lines == null || current.lines.Length == 0) {
            if (subtitleText) subtitleText.text = "[Dialogue vide: " + current.id + "]";
            return;
        }

        if (index >= current.lines.Length) { Close(); return; }
        if (subtitleText) subtitleText.SetText(current.lines[index]);
        Debug.Log($"[Narration] {current.id}[{index}] = {current.lines[index]}");
    }

    public void Close() {
        if (!IsPlaying) return;
        string id = current.id;
        current = null;

        if (panelSubtitles) panelSubtitles.SetActive(false);
        OnDialogueEnded?.Invoke(id);

        if (player) player.SetInputEnabled(true);
    }
}
