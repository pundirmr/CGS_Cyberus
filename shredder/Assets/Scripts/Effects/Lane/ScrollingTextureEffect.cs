using System.Collections;
using UnityEngine;

public class ScrollingTextureEffect : MonoBehaviour {
    [Header("Scene References")]
    [SerializeField] private Material material;
    [SerializeField] private MeshRenderer meshRenderer;

    [Header("Effect Settings")]
    [SerializeField] private float scrollSpeed = 0.05f;

    [Header("Lerp Colour Settings")]
    [SerializeField] private Color onScreenColour  = new (0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] private Color offScreenColour = new (0.5f, 0.5f, 0.5f, 0f);


    // delegate allocations
    private delegate IEnumerator ColourDel(float duration, Color startColour, Color finalColour);
    private ColourDel LerpColour;
    
    // Shader Variable ID's
    private int offsetID;
    private int colourID;

    private Coroutine colourCo;
    
    private Material instancedMat;
    
    private void Awake() {
        LerpColour = __LerpColour;
        
        // NOTE(WSWhitehouse): Creating a copy of the material to ensure the original
        // isn't changed in the Assets folder. This should stop the merge conflicts.
        instancedMat = new Material(material);
        meshRenderer.material = instancedMat;
    }
    
    private void Start() {
        Shader shader = instancedMat.shader;

        offsetID = shader.GetPropertyNameId(shader.FindPropertyIndex("_OffsetSpeed"));
        colourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_ColourMultiplier"));
            
        instancedMat.SetFloat(offsetID, scrollSpeed);

        // set the colour to be transparent on start
        instancedMat.SetColor(colourID, offScreenColour);
    }


    public void FadeToOnScreen(float duration)  => CoroutineUtil.StartSafelyWithRef(this, ref colourCo, LerpColour(duration, offScreenColour, onScreenColour));
    public void FadeToOffScreen(float duration) => CoroutineUtil.StartSafelyWithRef(this, ref colourCo, LerpColour(duration, onScreenColour, offScreenColour));
    
    private IEnumerator __LerpColour(float duration, Color startColour, Color finalColour) {
        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t  = elapsed / duration;

            Color c = Colour.Lerp(startColour, finalColour, t);
            instancedMat.SetColor(colourID, c);
                
            yield return CoroutineUtil.WaitForUpdate;
        }

        instancedMat.SetColor(colourID, finalColour);
        
        yield break;
    }
}
