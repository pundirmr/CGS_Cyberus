using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TrackEditorFollowPlayHead : MonoBehaviour
{
  // [SerializeField] private TrackEditorPlayHead playHead;
  // [SerializeField] private TrackEditorLaneManager laneManager;
  // [SerializeField] private Scrollbar scrollbar;
  // [Space]
  // [SerializeField] private RectTransform viewportTop;
  // [SerializeField] private RectTransform viewportBottom;
  // [SerializeField] private float distanceToMoveScroll = 2f;
  //
  // private void Update()
  // {
  //   if (!TrackEditor.IsPlayingTrack) return;
  //   
  //   float playHeadDist = viewportBottom.position.y - playHead.transform.position.y;
  //   if (playHeadDist < -maths.Abs(distanceToMoveScroll)) return;
  //   
  //   float normalisedVal = maths.Abs(maths.NormalizeValue(playHead.transform.localPosition.y + distanceToMoveScroll, laneManager.scrollContent.GetHeight()));
  //   normalisedVal       = 1 - normalisedVal;
  //   scrollbar.value     = normalisedVal;
  // }
}