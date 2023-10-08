using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class WaveformGenerator : MonoBehaviour
{
  [SerializeField] private RawImage image;
  [SerializeField] private Color waveformColour;
  [SerializeField] private Color backgroundColour;
  [SerializeField] private float resolution = 1f;

  private RectTransform RectTransform => (RectTransform)transform;
  
  private void Awake()
  {
    TrackEditor.OnMusicTrackChanged += GenerateWaveForm;
  }

  private void OnDestroy()
  {
    TrackEditor.OnMusicTrackChanged += GenerateWaveForm;
  }

  public void GenerateWaveForm()
  {
    if (TrackEditor.MusicTrack == null)
    {
      image.texture = null;
      return;
    }

    ref AudioClip audio = ref TrackEditor.MusicTrack.AudioClip;
    int width  = (int)(RectTransform.GetHeight() * resolution);
    int height = (int)(RectTransform.GetHeight() * resolution);

    Texture2D tex    = new Texture2D(width, height, TextureFormat.RGBA32, false);
    float[] samples  = new float[audio.samples * audio.channels];
    float[] waveform = new float[width];
    audio.GetData(samples, 0);
    int packSize = (samples.Length / width) + 1;
    
    int waveformIndex = 0;
    for (int i = 0; i < samples.Length; i += packSize)
    {
      waveform[waveformIndex] = Mathf.Abs(samples[i]);
      waveformIndex++;
    }

    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        tex.SetPixel(x, y, backgroundColour);
      }
    }

    for (int x = 0; x < waveform.Length; x++)
    {
      for (int y = 0; y <= waveform[x] * ((float)height * 0.75f); y++)
      {
        tex.SetPixel(x, (height / 2) + y, waveformColour);
        tex.SetPixel(x, (height / 2) - y, waveformColour);
      }
    }

    tex.Apply();

    image.texture = tex;
  }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WaveformGenerator))]
public class WaveformGeneratorEditor : Editor
{
  private WaveformGenerator Target => (WaveformGenerator)target;  
  
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();
    
    using (new EditorGUI.DisabledScope(!Application.isPlaying))
    {
      if (GUILayout.Button("Generate Waveform"))
      {
        Target.GenerateWaveForm();
      }
    }
  }
}
#endif