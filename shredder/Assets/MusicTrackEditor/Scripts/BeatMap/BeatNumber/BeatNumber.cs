using TMPro;
using UnityEngine;

public class BeatNumber : MonoBehaviour
{
  public RectTransform RectTransform => (RectTransform)transform;
  
  [SerializeField] private TMP_Text beatNumberText;
  [SerializeField] private string beatNumberFormat = "000";

  public void Init(int beatNumber)
  {
    beatNumberText.text = beatNumber.ToString(beatNumberFormat);
  }
}