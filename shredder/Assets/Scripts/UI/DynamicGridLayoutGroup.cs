using System;
using UnityEngine;
using UnityEngine.UI;


// TODO(Zack): allow an aspect ratio to be set for the size of cells
// TODO(Zack): make the grid ensure that all grid cells stay within the confines of parent [RectTransform]
[RequireComponent(typeof(GridLayoutGroup))]
[ExecuteAlways]
public class DynamicGridLayoutGroup : MonoBehaviour {

  private GridLayoutGroup grid;
  private RectOffset gridPadding;
  private RectTransform parent;

  public int rows = 6; // BUG/TODO(Zack): should be the [y] value, but is acually used for the [x] value
  public int cols = 7;
  public float spacing = 10;
  public bool matchParentHeight = false;
  
  public GridLayoutGroup UnityGridLayout => grid;

  private Vector2 _lastSize;

  private void Start() 
  {
    grid = GetComponent<GridLayoutGroup>();
    grid.spacing = new Vector2(spacing, spacing); 
    parent = GetComponent<RectTransform>();
    gridPadding = grid.padding;
    _lastSize = Vector2.zero;
  }
  
  private void Update()
  {
    if (float3Util.Compare((Vector3)_lastSize, (Vector3)parent.rect.size)) return;

    int paddingX     = gridPadding.left + gridPadding.right;
    float cellWidth  = maths.Round((parent.rect.width - paddingX - (rows - 1) * spacing) / rows);
    float cellHeight = matchParentHeight ? parent.GetHeight() : cellWidth;
    grid.cellSize    = new Vector2(cellWidth, cellHeight);
  }

  #if UNITY_EDITOR
  private void OnValidate()
  {
    if (Application.isPlaying) return;
    Start();
    Update();
  }
  #endif
}
