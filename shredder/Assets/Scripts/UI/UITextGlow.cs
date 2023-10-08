using TMPro;
using UnityEngine;


// TODO(Zack): actually get the [_Glow] value from the shader, and modify that instead
public class UITextGlow : MonoBehaviour {
    [Header("Scene References")]
    [SerializeField] private TMP_Text text;
    [SerializeField] private Material prefab = default;

    private Material mat;

    // base: shader id's 
    private int baseColourID;

    // base: values
    [DisableInInspector] private Color baseCol;
    private float baseIntensity = 0f;
    private float baseFactor    => maths.Pow2(baseIntensity);
    public float BaseIntensity { get => baseIntensity; }

    // outline: shader id's
    private int outlineColourID;
    private int outlineThicknessID;

    // outline: values
    [DisableInInspector] private Color outlineCol = new (0, 0, 0, 1);
    private float outlineThickness = 0f;
    private float outlineIntensity = 0f;
    private float outlineFactor    => maths.Pow2(outlineIntensity);
    public float OutlineIntensity { get => outlineIntensity; }

    private void Awake() {
        Init();
    }

    private void Init() {
        if (text == null) text = GetComponent<TMP_Text>();
        Debug.Assert(text != null, "Text Mesh Pro component has not been added to this Entity");
        
        // setup variables;
        mat     = new Material(prefab);
        text.fontMaterial = mat;

        var shader   = mat.shader;
        baseColourID = shader.GetPropertyNameId(shader.FindPropertyIndex("_FaceColor"));
        baseCol      = mat.GetColor(baseColourID);

        // outline variable setup
        outlineColourID    = shader.GetPropertyNameId(shader.FindPropertyIndex("_OutlineColor"));
        outlineThicknessID = shader.GetPropertyNameId(shader.FindPropertyIndex("_OutlineWidth"));
    }

    // NOTE(Zack): [UpdateBaseColour()] must be called after setting a value to update the shader value
    public void UpdateBaseColour()          => mat.SetColor(baseColourID, baseCol * baseFactor);
    public void SetBaseColour(Color col)    => baseCol = col;
    public void SetBaseIntensity(float val) => baseIntensity = val;

    // NOTE(Zack): use if you want the shader values to be immediately updated
    public void SetBaseColourAndUpdate(Color col) {
        SetBaseColour(col);
        UpdateBaseColour();
    }
    
    // NOTE(Zack): use if you want the shader values to be immediately updated
    public void SetBaseIntensityAndUpdate(float val) {
        SetBaseIntensity(val);
        UpdateBaseColour();
    }

    // NOTE(Zack): [UpdateOutlineColour()] must be called after setting a value to update the shader value
    public void UpdateOutlineColour()          => mat.SetColor(outlineColourID, outlineCol * outlineFactor);
    public void SetOutlineColour(Color col)    => outlineCol = col;
    public void SetOutlineIntensity(float val) => outlineIntensity = val;

    // NOTE(Zack): use if you want the shader values to be immediately updated
    public void SetOutlineColourAndUpdate(Color col) {
        SetOutlineColour(col);
        UpdateOutlineColour();
    }
    
    // NOTE(Zack): use if you want the shader values to be immediately updated
    public void SetOutlineIntensityAndUpdate(float val) {
        SetOutlineIntensity(val);
        UpdateOutlineColour();
    }
    
    public void SetOutlineThickness(float val) {
        outlineThickness = maths.Clamp01(val);
        mat.SetFloat(outlineThicknessID, outlineThickness);
    }


#if UNITY_EDITOR
    private void OnValidate() => Init();

    private void Update() {
        
    }
#endif
}
