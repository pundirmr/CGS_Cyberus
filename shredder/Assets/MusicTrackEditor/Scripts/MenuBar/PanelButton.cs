using UnityEngine;
using UnityEngine.UI;

public class PanelButton : MonoBehaviour
{
  [SerializeField] private Button button;
  [SerializeField] private WindowPanel windowPanel;
  
  private void Awake()
  {
    button.onClick.AddListener(windowPanel.Open);
  }

  private void OnDestroy()
  {
    button.onClick.RemoveListener(windowPanel.Open);
  }
}