using UnityEngine;

public class SceneColourTester : MonoBehaviour {
    // NOTE(Zack): wrapped in an #ifdef incase I forget to remove the script/object from the GameScene for build
#if UNITY_EDITOR 
    [SerializeField] private SceneColourScheme scheme;


    private void Update() {
        SceneColourer.UpdateSceneMaterialsColours(scheme);
    }
#endif
}
