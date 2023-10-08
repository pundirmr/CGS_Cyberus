using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/* NOTE(Zack): This script is to enforce there being only one "Main Camera" in a scene.
 * NOTE(Zack): This is in order to get smooth scene transitions
 */

[RequireComponent(typeof(AddLoadingScreenCameraToStack))]
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(GlitchEffectController))]
public class StaticCamera : MonoBehaviour {
    public static Camera Main                        { get; private set; }
    public static UniversalAdditionalCameraData Data { get; private set; }
    public static GlitchEffectController Glitch      { get; private set; }

    private void Awake() {
        Main   = gameObject.GetComponent<Camera>();
        Data   = gameObject.GetComponent<UniversalAdditionalCameraData>();
        Glitch = gameObject.GetComponent<GlitchEffectController>();
        Debug.Assert(Main   != null, "No camera component is on this GameObject", this);
        Debug.Assert(Data   != null, "No camera data is on this GameObject",      this);
        Debug.Assert(Glitch != null, "No Glitch controller on this GameObject",   this);

        Main.tag = "MainCamera";
    }

    // NOTE(Zack): these functions are to be used before any glitch effects are used on the camera
    public static void SetToDefaultRenderer() => Data.SetRenderer(0);
    public static void SetToGlitchRenderer()  => Data.SetRenderer(1);
}
