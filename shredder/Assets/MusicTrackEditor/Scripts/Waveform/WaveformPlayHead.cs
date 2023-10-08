using UnityEngine;

public class WaveformPlayHead : MonoBehaviour
{
  private RectTransform RectTransform => (RectTransform)transform;
  
  [SerializeField] private RectTransform waveform;
  
  private void Update()
  {
    // float songPercentage = (float)TrackEditor.TrackPlaybackTime / TrackEditor.MusicTrack.TrackDuration;
    //   
    // float x = RectTransform.localPosition.x;
    // // NOTE(WSWhitehouse): Rect transform is dodgy so we're offsetting y by half the waveform height!
    // float y = (-maths.Abs(waveform.GetHeight() * songPercentage)) + waveform.GetHeight() * 0.5f;
    // float z = RectTransform.localPosition.z;
    // RectTransform.localPosition = new Vector3(x, y, z);
  }
}