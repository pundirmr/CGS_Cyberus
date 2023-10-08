using UnityEngine.InputSystem;

/// <summary>
/// A small input class for PC Menu, this exists so we dont have to have an Input Action Asset
/// in the assets menu within Unity. Dont use it elsewhere.
/// </summary>
public class PCMenuInput
{
  public InputActionAsset asset { get; }
  
  public InputActionMap InputMap { get; }
  public InputAction ToggleMenu  { get; }

  public PCMenuInput()
  {
    asset = InputActionAsset.FromJson(@"{
        ""name"": ""CanvasInput"",
        ""maps"": 
        [
            {
                ""name"": ""Input"",
                ""id"": ""174d0416-bb2d-4214-b091-1b0674924a3b"",
                ""actions"": 
                [
                    {
                        ""name"": ""ToggleMenu"",
                        ""type"": ""Button"",
                        ""id"": ""facf1fa1-7a40-405f-81bf-972c14f9b87b"",
                        ""expectedControlType"": ""Button"",
                        ""processors"": """",
                        ""interactions"": """",
                        ""initialStateCheck"": false
                    }
                ],
                ""bindings"": 
                [
                    {
                        ""name"": """",
                        ""id"": ""2d0c25ab-56c3-466c-8076-95e83fb8a0f0"",
                        ""path"": ""<Keyboard>/escape"",
                        ""interactions"": """",
                        ""processors"": """",
                        ""groups"": """",
                        ""action"": ""ToggleMenu"",
                        ""isComposite"": false,
                        ""isPartOfComposite"": false
                    }
                ]
            }
        ],
        ""controlSchemes"": []
    }");
    
    // Find actions and maps
    InputMap   = asset.FindActionMap("Input", true);
    ToggleMenu = InputMap.FindAction("ToggleMenu", true);
  }
}