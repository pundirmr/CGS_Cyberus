using UnityEngine;
using UnityEngine.UI;

public class MetronomeButtons : MonoBehaviour
{
  [Header("Beat")]
  [SerializeField] private Button beatMetronomeButton;
  [SerializeField] private Image beatMetronomeTick;
  
  [Header("Note")]
  [SerializeField] private Button noteMetronomeButton;
  [SerializeField] private Image noteMetronomeTick;

  private void Awake()
  {
    beatMetronomeButton.onClick.AddListener(OnBeatMetronomeButtonClicked);
    noteMetronomeButton.onClick.AddListener(OnNoteMetronomeButtonClicked);
  }

  private void OnEnable() => UpdateUI();
  
  private void OnDestroy()
  {
    beatMetronomeButton.onClick.AddListener(OnBeatMetronomeButtonClicked);
    noteMetronomeButton.onClick.AddListener(OnNoteMetronomeButtonClicked);
  }

  private void OnBeatMetronomeButtonClicked()
  {
    MetronomePlayer.BeatMetronomeEnabled = !MetronomePlayer.BeatMetronomeEnabled;
    UpdateUI();
  }
  
  private void OnNoteMetronomeButtonClicked()
  {
    MetronomePlayer.NoteMetronomeEnabled = !MetronomePlayer.NoteMetronomeEnabled;
    UpdateUI();
  }

  private void UpdateUI()
  {
    beatMetronomeTick.enabled = MetronomePlayer.BeatMetronomeEnabled;
    noteMetronomeTick.enabled = MetronomePlayer.NoteMetronomeEnabled;
  }
}