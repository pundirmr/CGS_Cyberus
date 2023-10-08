//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.4.1
//     from Assets/MusicTrackEditor/Input/TrackEditorInput.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @TrackEditorInput : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @TrackEditorInput()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""TrackEditorInput"",
    ""maps"": [
        {
            ""name"": ""Track"",
            ""id"": ""174d0416-bb2d-4214-b091-1b0674924a3b"",
            ""actions"": [
                {
                    ""name"": ""Scale"",
                    ""type"": ""Value"",
                    ""id"": ""0fae0c19-2e67-4466-8e45-be4eadc34f40"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""ToggleLane01Beat"",
                    ""type"": ""Button"",
                    ""id"": ""facf1fa1-7a40-405f-81bf-972c14f9b87b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ToggleLane02Beat"",
                    ""type"": ""Button"",
                    ""id"": ""19e53d7a-30d6-479f-873d-caa21b5f8691"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ToggleLane03Beat"",
                    ""type"": ""Button"",
                    ""id"": ""1aa7211f-2ca4-44d8-8746-f919424fefc0"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""ba957e33-1e77-48a2-b2e0-7246b9ffcdd7"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Scale"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2d0c25ab-56c3-466c-8076-95e83fb8a0f0"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ToggleLane01Beat"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ac5dfc1b-5bc1-472a-93bb-2cb19eb39c5c"",
                    ""path"": ""<Keyboard>/numpad1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ToggleLane01Beat"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e468de2e-6dc2-464e-aa31-00d1ac4a2e24"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ToggleLane02Beat"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a083fa5c-66c5-4ed3-a0c6-f23e04773663"",
                    ""path"": ""<Keyboard>/numpad2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ToggleLane02Beat"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e560660e-47c1-469d-be0e-24cbf79ffb3e"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ToggleLane03Beat"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f3917041-aa37-4bb7-9470-bc0be5daf44d"",
                    ""path"": ""<Keyboard>/numpad3"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ToggleLane03Beat"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Tool"",
            ""id"": ""85a67ab7-319a-4b10-b3a6-d5d46aad9128"",
            ""actions"": [
                {
                    ""name"": ""MoveTool"",
                    ""type"": ""Button"",
                    ""id"": ""f5a5b4ae-36a3-411b-b150-7f72592cf800"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""SelectTool"",
                    ""type"": ""Button"",
                    ""id"": ""1834c147-7381-41da-994c-bec213a34381"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""EditTool"",
                    ""type"": ""Button"",
                    ""id"": ""5befe173-ddda-458d-a4ad-0167a97e7851"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""5f0c902a-0762-4a0a-ad27-95b8cf07af87"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveTool"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0c3d26d4-05c3-40c7-b7d3-62850a62c904"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SelectTool"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a36cacbf-4457-4e05-913f-4a1386cec846"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""EditTool"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Mouse"",
            ""id"": ""b1e89393-b157-4381-b8fe-99705d77fd8e"",
            ""actions"": [
                {
                    ""name"": ""LeftClick"",
                    ""type"": ""Button"",
                    ""id"": ""832a478f-8d19-4838-a874-ee8e25603211"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""RightClick"",
                    ""type"": ""Button"",
                    ""id"": ""23611498-d9d0-4b9b-9c24-adfef633bdad"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""664ee588-a648-4d7a-b20d-4a73e678cda4"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""20ffc1e6-1b6d-44a7-8cd0-fb7bd4daa682"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RightClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Track
        m_Track = asset.FindActionMap("Track", throwIfNotFound: true);
        m_Track_Scale = m_Track.FindAction("Scale", throwIfNotFound: true);
        m_Track_ToggleLane01Beat = m_Track.FindAction("ToggleLane01Beat", throwIfNotFound: true);
        m_Track_ToggleLane02Beat = m_Track.FindAction("ToggleLane02Beat", throwIfNotFound: true);
        m_Track_ToggleLane03Beat = m_Track.FindAction("ToggleLane03Beat", throwIfNotFound: true);
        // Tool
        m_Tool = asset.FindActionMap("Tool", throwIfNotFound: true);
        m_Tool_MoveTool = m_Tool.FindAction("MoveTool", throwIfNotFound: true);
        m_Tool_SelectTool = m_Tool.FindAction("SelectTool", throwIfNotFound: true);
        m_Tool_EditTool = m_Tool.FindAction("EditTool", throwIfNotFound: true);
        // Mouse
        m_Mouse = asset.FindActionMap("Mouse", throwIfNotFound: true);
        m_Mouse_LeftClick = m_Mouse.FindAction("LeftClick", throwIfNotFound: true);
        m_Mouse_RightClick = m_Mouse.FindAction("RightClick", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Track
    private readonly InputActionMap m_Track;
    private ITrackActions m_TrackActionsCallbackInterface;
    private readonly InputAction m_Track_Scale;
    private readonly InputAction m_Track_ToggleLane01Beat;
    private readonly InputAction m_Track_ToggleLane02Beat;
    private readonly InputAction m_Track_ToggleLane03Beat;
    public struct TrackActions
    {
        private @TrackEditorInput m_Wrapper;
        public TrackActions(@TrackEditorInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @Scale => m_Wrapper.m_Track_Scale;
        public InputAction @ToggleLane01Beat => m_Wrapper.m_Track_ToggleLane01Beat;
        public InputAction @ToggleLane02Beat => m_Wrapper.m_Track_ToggleLane02Beat;
        public InputAction @ToggleLane03Beat => m_Wrapper.m_Track_ToggleLane03Beat;
        public InputActionMap Get() { return m_Wrapper.m_Track; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(TrackActions set) { return set.Get(); }
        public void SetCallbacks(ITrackActions instance)
        {
            if (m_Wrapper.m_TrackActionsCallbackInterface != null)
            {
                @Scale.started -= m_Wrapper.m_TrackActionsCallbackInterface.OnScale;
                @Scale.performed -= m_Wrapper.m_TrackActionsCallbackInterface.OnScale;
                @Scale.canceled -= m_Wrapper.m_TrackActionsCallbackInterface.OnScale;
                @ToggleLane01Beat.started -= m_Wrapper.m_TrackActionsCallbackInterface.OnToggleLane01Beat;
                @ToggleLane01Beat.performed -= m_Wrapper.m_TrackActionsCallbackInterface.OnToggleLane01Beat;
                @ToggleLane01Beat.canceled -= m_Wrapper.m_TrackActionsCallbackInterface.OnToggleLane01Beat;
                @ToggleLane02Beat.started -= m_Wrapper.m_TrackActionsCallbackInterface.OnToggleLane02Beat;
                @ToggleLane02Beat.performed -= m_Wrapper.m_TrackActionsCallbackInterface.OnToggleLane02Beat;
                @ToggleLane02Beat.canceled -= m_Wrapper.m_TrackActionsCallbackInterface.OnToggleLane02Beat;
                @ToggleLane03Beat.started -= m_Wrapper.m_TrackActionsCallbackInterface.OnToggleLane03Beat;
                @ToggleLane03Beat.performed -= m_Wrapper.m_TrackActionsCallbackInterface.OnToggleLane03Beat;
                @ToggleLane03Beat.canceled -= m_Wrapper.m_TrackActionsCallbackInterface.OnToggleLane03Beat;
            }
            m_Wrapper.m_TrackActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Scale.started += instance.OnScale;
                @Scale.performed += instance.OnScale;
                @Scale.canceled += instance.OnScale;
                @ToggleLane01Beat.started += instance.OnToggleLane01Beat;
                @ToggleLane01Beat.performed += instance.OnToggleLane01Beat;
                @ToggleLane01Beat.canceled += instance.OnToggleLane01Beat;
                @ToggleLane02Beat.started += instance.OnToggleLane02Beat;
                @ToggleLane02Beat.performed += instance.OnToggleLane02Beat;
                @ToggleLane02Beat.canceled += instance.OnToggleLane02Beat;
                @ToggleLane03Beat.started += instance.OnToggleLane03Beat;
                @ToggleLane03Beat.performed += instance.OnToggleLane03Beat;
                @ToggleLane03Beat.canceled += instance.OnToggleLane03Beat;
            }
        }
    }
    public TrackActions @Track => new TrackActions(this);

    // Tool
    private readonly InputActionMap m_Tool;
    private IToolActions m_ToolActionsCallbackInterface;
    private readonly InputAction m_Tool_MoveTool;
    private readonly InputAction m_Tool_SelectTool;
    private readonly InputAction m_Tool_EditTool;
    public struct ToolActions
    {
        private @TrackEditorInput m_Wrapper;
        public ToolActions(@TrackEditorInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @MoveTool => m_Wrapper.m_Tool_MoveTool;
        public InputAction @SelectTool => m_Wrapper.m_Tool_SelectTool;
        public InputAction @EditTool => m_Wrapper.m_Tool_EditTool;
        public InputActionMap Get() { return m_Wrapper.m_Tool; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(ToolActions set) { return set.Get(); }
        public void SetCallbacks(IToolActions instance)
        {
            if (m_Wrapper.m_ToolActionsCallbackInterface != null)
            {
                @MoveTool.started -= m_Wrapper.m_ToolActionsCallbackInterface.OnMoveTool;
                @MoveTool.performed -= m_Wrapper.m_ToolActionsCallbackInterface.OnMoveTool;
                @MoveTool.canceled -= m_Wrapper.m_ToolActionsCallbackInterface.OnMoveTool;
                @SelectTool.started -= m_Wrapper.m_ToolActionsCallbackInterface.OnSelectTool;
                @SelectTool.performed -= m_Wrapper.m_ToolActionsCallbackInterface.OnSelectTool;
                @SelectTool.canceled -= m_Wrapper.m_ToolActionsCallbackInterface.OnSelectTool;
                @EditTool.started -= m_Wrapper.m_ToolActionsCallbackInterface.OnEditTool;
                @EditTool.performed -= m_Wrapper.m_ToolActionsCallbackInterface.OnEditTool;
                @EditTool.canceled -= m_Wrapper.m_ToolActionsCallbackInterface.OnEditTool;
            }
            m_Wrapper.m_ToolActionsCallbackInterface = instance;
            if (instance != null)
            {
                @MoveTool.started += instance.OnMoveTool;
                @MoveTool.performed += instance.OnMoveTool;
                @MoveTool.canceled += instance.OnMoveTool;
                @SelectTool.started += instance.OnSelectTool;
                @SelectTool.performed += instance.OnSelectTool;
                @SelectTool.canceled += instance.OnSelectTool;
                @EditTool.started += instance.OnEditTool;
                @EditTool.performed += instance.OnEditTool;
                @EditTool.canceled += instance.OnEditTool;
            }
        }
    }
    public ToolActions @Tool => new ToolActions(this);

    // Mouse
    private readonly InputActionMap m_Mouse;
    private IMouseActions m_MouseActionsCallbackInterface;
    private readonly InputAction m_Mouse_LeftClick;
    private readonly InputAction m_Mouse_RightClick;
    public struct MouseActions
    {
        private @TrackEditorInput m_Wrapper;
        public MouseActions(@TrackEditorInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @LeftClick => m_Wrapper.m_Mouse_LeftClick;
        public InputAction @RightClick => m_Wrapper.m_Mouse_RightClick;
        public InputActionMap Get() { return m_Wrapper.m_Mouse; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MouseActions set) { return set.Get(); }
        public void SetCallbacks(IMouseActions instance)
        {
            if (m_Wrapper.m_MouseActionsCallbackInterface != null)
            {
                @LeftClick.started -= m_Wrapper.m_MouseActionsCallbackInterface.OnLeftClick;
                @LeftClick.performed -= m_Wrapper.m_MouseActionsCallbackInterface.OnLeftClick;
                @LeftClick.canceled -= m_Wrapper.m_MouseActionsCallbackInterface.OnLeftClick;
                @RightClick.started -= m_Wrapper.m_MouseActionsCallbackInterface.OnRightClick;
                @RightClick.performed -= m_Wrapper.m_MouseActionsCallbackInterface.OnRightClick;
                @RightClick.canceled -= m_Wrapper.m_MouseActionsCallbackInterface.OnRightClick;
            }
            m_Wrapper.m_MouseActionsCallbackInterface = instance;
            if (instance != null)
            {
                @LeftClick.started += instance.OnLeftClick;
                @LeftClick.performed += instance.OnLeftClick;
                @LeftClick.canceled += instance.OnLeftClick;
                @RightClick.started += instance.OnRightClick;
                @RightClick.performed += instance.OnRightClick;
                @RightClick.canceled += instance.OnRightClick;
            }
        }
    }
    public MouseActions @Mouse => new MouseActions(this);
    public interface ITrackActions
    {
        void OnScale(InputAction.CallbackContext context);
        void OnToggleLane01Beat(InputAction.CallbackContext context);
        void OnToggleLane02Beat(InputAction.CallbackContext context);
        void OnToggleLane03Beat(InputAction.CallbackContext context);
    }
    public interface IToolActions
    {
        void OnMoveTool(InputAction.CallbackContext context);
        void OnSelectTool(InputAction.CallbackContext context);
        void OnEditTool(InputAction.CallbackContext context);
    }
    public interface IMouseActions
    {
        void OnLeftClick(InputAction.CallbackContext context);
        void OnRightClick(InputAction.CallbackContext context);
    }
}
