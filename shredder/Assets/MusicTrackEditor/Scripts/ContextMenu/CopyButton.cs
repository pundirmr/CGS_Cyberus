using UnityEngine;
using UnityEngine.UI;

public class CopyButton : MonoBehaviour
{
  [SerializeField] private Button button;

  private void Awake()
  {
    button.onClick.AddListener(ClipBoardManager.Copy);
  }
  
  private void OnDestroy()
  {
    button.onClick.RemoveListener(ClipBoardManager.Copy);
  }
}