using System;
using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(WireframeEffect))]
public class Spam2D : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private SpriteRenderer image;
  [SerializeField] private Rigidbody2D body;
  [SerializeField] private new Collider2D collider;

  [Header("Spam Freeze")] 
  [SerializeField] [TimeField] private float timeBeforeFreeze;
  [SerializeField] private float velocityThreshold;
  [SerializeField] private int numberOfCollisionsThreshold;

  // Clear Lane Variables
  // NOTE(WSWhitehouse): These values should not be used outside of the ClearLane.cs
  // script because it handles resetting them to appropriate values. 
  public Vector3 ClearLaneStartPos { get; set; }
  public Vector3 ClearLaneEndPos   { get; set; }
  public float ClearLaneDuration   { get; set; }
  public bool ClearLaneComplete    { get; set; }

  public Rigidbody2D Body    => body;
  public Collider2D Collider => collider;

  public float CreationTime
  {
    get => _creationTime;
    set => _creationTime = value;
  }

  private float _creationTime;
  
  private WireframeEffect _wireframeEffect;

  private DelegateUtil.EmptyCoroutineDel Freeze;
  private Coroutine _freezeCoroutine;

  private delegate IEnumerator FadeDel(float duration);
  private FadeDel Fade;
  private FadeDel Dim;
  private Coroutine fadeCo;
  
  private int _collisionCount = 0;

  private Color currentColour;
  
  private void Awake()
  {
    Freeze = __Freeze;
    Fade   = __Fade;
    Dim    = __Dim;
    _wireframeEffect = GetComponent<WireframeEffect>();
  }

  private void Start()
  {
    image.material = _wireframeEffect.MaterialInstance;
  }

  public void SetColour(Color colour)
  {
    currentColour = colour;
    _wireframeEffect.SetColour(colour);
    _wireframeEffect.SetBaseColour(colour);
  }

  public void EnableSpam()
  {
    // Reset freeze values
    CoroutineUtil.StopSafelyWithRef(this, ref _freezeCoroutine);
    _collisionCount = 0;
    
    // Allow the rigidbody to move
    Body.isKinematic  = false;
    Body.gravityScale = 1;
    
    // Enable components
    Collider.enabled = true;
    Body.simulated   = true;
    gameObject.SetActive(true);
  }

  public void DisableSpam()
  {
    // Stop the rigidbody from moving
    Body.isKinematic     = true;
    Body.velocity        = new Vector2(0.0f, 0.0f);
    Body.angularVelocity = 0.0f;
    
    // Disable components and stop rb from being simulated
    Collider.enabled = false;
    Body.simulated   = false;
    gameObject.SetActive(false);
  }

  public void FreezeSpam()
  {
    CoroutineUtil.StartSafelyWithRef(this, ref _freezeCoroutine, Freeze());
  }

  private IEnumerator __Freeze()
  {
    yield return CoroutineUtil.Wait(timeBeforeFreeze);

    while (_collisionCount <= numberOfCollisionsThreshold)
    {
      yield return CoroutineUtil.WaitForFixedUpdate;
    }

    while (maths.Abs(Body.velocity.x) > velocityThreshold &&
           maths.Abs(Body.velocity.y) > velocityThreshold)
    {
      yield return CoroutineUtil.WaitForFixedUpdate;
    }

    Body.isKinematic     = true;
    Body.velocity        = new Vector2(0.0f, 0.0f);
    Body.angularVelocity = 0.0f;
    
    _freezeCoroutine = null;
  }

  private void OnCollisionExit2D(Collision2D other)
  {
    _collisionCount++;
  }

  public void FadeSpamOut(float duration)
  {
    CoroutineUtil.StartSafelyWithRef(this, ref fadeCo, Fade(duration));
  }

  private IEnumerator __Fade(float duration)
  {
    Color startColour     = currentColour;
    Color startBaseColour = new (currentColour.r, currentColour.g, currentColour.b, _wireframeEffect.BaseColourAlpha);
    
    Color endColour     = Colour.Transparent(startColour);
    Color endBaseColour = Colour.Transparent(startBaseColour);
    
    float elapsed = 0f;
    while (elapsed < duration) {
      elapsed += Time.deltaTime;
      float t  = elapsed / duration;

      Color c = Colour.Lerp(startColour, endColour, t);
      _wireframeEffect.SetColourAndAlpha(c);

      c = Colour.Lerp(startBaseColour, endBaseColour, t);      
      _wireframeEffect.SetBaseColourAndAlpha(c);

      
      yield return CoroutineUtil.WaitForUpdate;
    }

     _wireframeEffect.SetColourAndAlpha(endColour);
     _wireframeEffect.SetBaseColourAndAlpha(endBaseColour);


    Spam2DPool.ReturnSpamToPool(this);
    
    yield break;
  }

  public void DimSpam(float duration) {
      CoroutineUtil.StartSafelyWithRef(this, ref fadeCo, Dim(duration));
  }
  
  private IEnumerator __Dim(float duration)
  {
    Color startColour     = currentColour;
    Color startBaseColour = new (currentColour.r, currentColour.g, currentColour.b, _wireframeEffect.BaseColourAlpha);
    
    Color endColour     = Colour.ChangeAlpha(startColour, 0.35f);
    Color endBaseColour = Colour.ChangeAlpha(startBaseColour, _wireframeEffect.BaseColourAlpha
    * 0.5f);

    // set the current colour
    currentColour = endColour;
    
    float elapsed = 0f;
    while (elapsed < duration) {
      elapsed += Time.deltaTime;
      float t  = elapsed / duration;

      Color c = Colour.Lerp(startColour, endColour, t);
      _wireframeEffect.SetColourAndAlpha(c);

      c = Colour.Lerp(startBaseColour, endBaseColour, t);      
      _wireframeEffect.SetBaseColourAndAlpha(c);

      
      yield return CoroutineUtil.WaitForUpdate;
    }
    
    yield break;
  }
}
