using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ToolBar : MonoBehaviour
{
  [Header("Scene References")]
  [SerializeField] private Button moveButton;
  [SerializeField] private Button selectButton;
  [SerializeField] private Button editButton;
  
  private Color _defaultCol;
  private Color _selectedCol;
  
  private TrackEditorInput _input;

  private void Awake()
  {
    Tool.OnModeChanged += UpdateUI;
    
    // NOTE(WSWhitehouse): We use the move button colours here but it should be the same across all 3 buttons
    _defaultCol  = moveButton.colors.normalColor;
    _selectedCol = moveButton.colors.pressedColor;

    moveButton.onClick.AddListener(OnMoveSelected);
    selectButton.onClick.AddListener(OnSelectSelected);
    editButton.onClick.AddListener(OnEditSelected);
    
    _input = new TrackEditorInput();
    _input.Enable();
    
    _input.Tool.MoveTool.performed   += OnMovePerformed;
    _input.Tool.SelectTool.performed += OnSelectPerformed;
    _input.Tool.EditTool.performed   += OnEditPerformed;
  }
  
  private void Start()
  {
    // Reset tool when starting up, this also updates the UI for the first time
    Tool.Mode = ToolMode.MOVE;
  }

  private void OnDestroy()
  {
    Tool.OnModeChanged -= UpdateUI;
    
    moveButton.onClick.RemoveListener(OnMoveSelected);
    selectButton.onClick.RemoveListener(OnSelectSelected);
    editButton.onClick.RemoveListener(OnEditSelected);
    
    _input.Tool.MoveTool.performed   -= OnMovePerformed;
    _input.Tool.SelectTool.performed -= OnSelectPerformed;
    _input.Tool.EditTool.performed   -= OnEditPerformed;
  }

  private void UpdateUI()
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SetButtonCol(Button button, bool selected)
    {
      ColorBlock colBlock = new ColorBlock()
      {
        colorMultiplier  = button.colors.colorMultiplier,
        disabledColor    = button.colors.disabledColor,
        fadeDuration     = button.colors.fadeDuration,
        highlightedColor = button.colors.highlightedColor,
        normalColor      = selected ? _selectedCol : _defaultCol,
        pressedColor     = button.colors.pressedColor,
        selectedColor    = button.colors.selectedColor
      };
    
      button.colors = colBlock;
    }
    
    // Reset all buttons
    SetButtonCol(moveButton,    false);
    SetButtonCol(selectButton, false);
    SetButtonCol(editButton,   false);

    // Set active button colour
    switch (Tool.Mode)
    {
      case ToolMode.MOVE:  SetButtonCol(moveButton,    true); break;
      case ToolMode.SELECT: SetButtonCol(selectButton, true); break;
      case ToolMode.EDIT:  SetButtonCol(editButton,    true); break;

      default: throw new ArgumentOutOfRangeException();
    }
  }
  
  private static void OnMovePerformed(InputAction.CallbackContext obj)   => OnMoveSelected();
  private static void OnSelectPerformed(InputAction.CallbackContext obj) => OnSelectSelected();
  private static void OnEditPerformed(InputAction.CallbackContext obj)   => OnEditSelected();
  
  private static void OnMoveSelected()   => Tool.Mode = ToolMode.MOVE;
  private static void OnSelectSelected() => Tool.Mode = ToolMode.SELECT;
  private static void OnEditSelected()   => Tool.Mode = ToolMode.EDIT;
}