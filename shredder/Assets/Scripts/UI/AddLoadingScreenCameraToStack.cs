using UnityEngine;
using UnityEngine.Rendering.Universal;


[RequireComponent(typeof(Camera))]
public class AddLoadingScreenCameraToStack : MonoBehaviour {    
    private Camera cam;
    private UniversalAdditionalCameraData data;

#if UNITY_EDITOR
    private bool startingInGameScene => (SceneHandler.PreviousSceneIndex != (int)Scene.TRACK_SELECT) && (SceneHandler.SceneIndex == (int)Scene.GAME_SCENE);
#endif

    
    private void Awake() {
        cam  = GetComponent<Camera>();
        data = cam.GetUniversalAdditionalCameraData();
        ClearCameraStack();
        
        LoadingScreen.OnShowingLoadingScreen += AddCameraToStack;
        LoadingScreen.OnHidingLoadingScreen  += ClearCameraStack;

        // NOTE(Zack): this is so that we can start in the [GameScene] and have fullscreen camera effects work
#if UNITY_EDITOR
        if (startingInGameScene) return;
#endif
        
        // edge case handling
        if (SceneHandler.PreviousSceneIndex == (int)Scene.START_SCENE) return;        
        if (LoadingScreen.OverlayCamera == null) return;

        AddCameraToStack();
    }
    
    private void Start() {
        if (SceneHandler.PreviousSceneIndex == (int)Scene.START_SCENE) return;
        if (data.cameraStack.Count > 0) return;

        // NOTE(Zack): this is so that we can start in the [GameScene] and have fullscreen camera effects work
#if UNITY_EDITOR
        if (startingInGameScene) return;
#endif
        
        AddCameraToStack();
    }

    private void OnDestroy() {
        LoadingScreen.OnShowingLoadingScreen -= AddCameraToStack;
        LoadingScreen.OnHidingLoadingScreen  -= ClearCameraStack;        
    }
    
    private void AddCameraToStack() => data.cameraStack.Add(LoadingScreen.OverlayCamera);
    private void ClearCameraStack() => data.cameraStack.Clear();
}
