using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
  [TextArea] public string tooltipContent;
  [Space]
  [SerializeField] private float hoverTime = 1.5f;
  
  private DelegateUtil.EmptyCoroutineDel WaitAndShowTooltip;
  private Coroutine _waitAndShowTooltipCoroutine;

  private void Awake()
  {
    WaitAndShowTooltip = __WaitAndShowTooltip;
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    CoroutineUtil.StartSafelyWithRef(this, ref _waitAndShowTooltipCoroutine, WaitAndShowTooltip());
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    CoroutineUtil.StopSafelyWithRef(this, ref _waitAndShowTooltipCoroutine);
    TooltipCanvas.Hide();
  }

  private IEnumerator __WaitAndShowTooltip()
  {
    yield return CoroutineUtil.WaitUnscaled(hoverTime);
    TooltipCanvas.Show(tooltipContent);
    _waitAndShowTooltipCoroutine = null;
  }
}