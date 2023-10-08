using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PCMenuCanvas : MonoBehaviour
{
  [Header("Scene References")]
  [SerializeField] private Canvas canvas;
  [SerializeField] private GraphicRaycaster raycaster;
  [Space]
  [SerializeField] private Canvas mainButtonsCanvas;
  [SerializeField] private Canvas volumeSlidersCanvas;
  
  [Header("Main Buttons")]
  [SerializeField] private Button volumeSlidersButton;
  [Space]
  [SerializeField] private Button setupSceneButton;
  [SerializeField] [UnityScene] private int setupSceneIndex;
  [Space]
  [SerializeField] private Button mainMenuSceneButton;
  [SerializeField] [UnityScene] private int mainMenuSceneIndex;
  [Space]
  [SerializeField] private Button quitButton;
  
  [Header("Volume Sliders")]
  [SerializeField] private Button backButton;
  [SerializeField] private Slider masterSlider;
  [SerializeField] private Slider musicSlider;
  [SerializeField] private Slider sfxSlider;
  [SerializeField] private Slider voiceOverSlider;

  public static bool MenuActive { get; private set; } = true;
  private static PCMenuCanvas Instance = null;
  
  private static PCMenuInput _input;

  private void Awake()
  {
    if (Instance != null)
    {
      Destroy(this.gameObject);
      return;
    }
    
    Instance = this;
    DontDestroyOnLoad(this.gameObject);

    // NOTE(WSWhitehouse): We are enabling each part of the input asset
    // because otherwise it doesnt work - unsure why
    _input = new PCMenuInput();
    _input.asset.Enable();
    _input.InputMap.Enable();
    _input.ToggleMenu.Enable();
    
    // NOTE(WSWhitehouse): Ensure sorting order is always set to max
    canvas.sortingOrder = short.MaxValue;
    
    _input.ToggleMenu.performed += OnToggleMenuPerformed;
    SetMenuInactive();
    
    // Subscribe to buttons
    volumeSlidersButton.onClick.AddListener(OpenVolumeSliders);
    backButton.onClick.AddListener(OpenMainButtons);
    setupSceneButton.onClick.AddListener(TransitionToSetupScene);
    mainMenuSceneButton.onClick.AddListener(TransitionToMainMenu);
    quitButton.onClick.AddListener(QuitGame);
    
    // Set slider values
    masterSlider.value          = AudioManager.MasterVol;
    musicSlider.value           = AudioManager.MusicVol;
    sfxSlider.value             = AudioManager.SFXVol;
    voiceOverSlider.value       = AudioManager.VoiceOverVol;
    
    // Subscribe to sliders
    masterSlider.onValueChanged.AddListener(OnMasterSliderUpdated);
    musicSlider.onValueChanged.AddListener(OnMusicSliderUpdated);
    sfxSlider.onValueChanged.AddListener(OnSFXSliderUpdated);
    voiceOverSlider.onValueChanged.AddListener(OnVoiceOverSliderUpdated);
    
    SceneHandler.OnSceneLoadingEnded += OnSceneLoadingEnded;
  }

  private void OnDestroy()
  {
    if (Instance != this) return;
    Instance = null;
    
    _input.ToggleMenu.performed -= OnToggleMenuPerformed;
    
    // Unsubscribe to buttons
    volumeSlidersButton.onClick.RemoveListener(OpenVolumeSliders);
    backButton.onClick.RemoveListener(OpenMainButtons);
    setupSceneButton.onClick.RemoveListener(TransitionToSetupScene);
    mainMenuSceneButton.onClick.RemoveListener(TransitionToMainMenu);
    quitButton.onClick.RemoveListener(QuitGame);
    
    // Unsubscribe to sliders
    masterSlider.onValueChanged.RemoveListener(OnMasterSliderUpdated);
    musicSlider.onValueChanged.RemoveListener(OnMusicSliderUpdated);
    sfxSlider.onValueChanged.RemoveListener(OnSFXSliderUpdated);
    voiceOverSlider.onValueChanged.RemoveListener(OnVoiceOverSliderUpdated);
    
    SceneHandler.OnSceneLoadingEnded -= OnSceneLoadingEnded;
  }

  private void OnToggleMenuPerformed(InputAction.CallbackContext obj)
  {
    // Dont set the menu to active in a loading screen
    if (SceneHandler.IsLoading)
    {
      SetMenuInactive();
      return;
    }
    
    if (MenuActive)
    {
      SetMenuInactive();
    }
    else
    {
      SetMenuActive();
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void SetMenuActive()
  {
    OpenMainButtons();
    
    canvas.enabled    = true;
    raycaster.enabled = true;
    MenuActive        = true;
    
    CursorUtil.Unlock();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void SetMenuInactive()
  {
    canvas.enabled    = false;
    raycaster.enabled = false;
    MenuActive        = false;
    
    CursorUtil.Lock();
  }
  
  private void OnSceneLoadingEnded()
  {
    SetMenuInactive();
    
    // NOTE(WSWhitehouse): If we transition back to the setup scene we should destroy this menu 
    // to ensure we cant transition anywhere without going through the setup again.
    if (SceneHandler.SceneIndex != (int)Scene.START_SCENE) return;
    Destroy(this.gameObject);
  }

  private void OnMasterSliderUpdated(float value)
  {
    AudioManager.MasterVol = value;
  }
  
  private void OnMusicSliderUpdated(float value)
  {
    AudioManager.MusicVol = value;
  }
  
  private void OnVoiceOverSliderUpdated(float value)
  {
    AudioManager.VoiceOverVol = value;
  }
  
  private void OnSFXSliderUpdated(float value)
  {
    AudioManager.SFXVol = value;
  }
  
  private void OpenVolumeSliders()
  {
    mainButtonsCanvas.enabled   = false;
    volumeSlidersCanvas.enabled = true;
  }

  private void OpenMainButtons()
  {
    mainButtonsCanvas.enabled   = true;
    volumeSlidersCanvas.enabled = false;
  }

  private void TransitionToSetupScene()
  {
    SetMenuInactive();
    SceneLoad.LoadScene(setupSceneIndex);
  }

  private void TransitionToMainMenu()
  {
    SetMenuInactive();
    SceneLoad.LoadScene(mainMenuSceneIndex);
  }

  private void QuitGame()
  {
    SetMenuInactive();
    Application.Quit();
  }
}
