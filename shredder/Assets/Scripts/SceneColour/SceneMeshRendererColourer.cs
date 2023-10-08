using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMeshRendererColourer : MonoBehaviour
{
    public enum MaterialType
    {
        LampPost = 0,
        Pavement = 1,
        Building = 2,
    }
    
    
    [SerializeField] private MaterialType materialTypeToApply;

    private MeshRenderer _meshRenderer;
    
    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        switch (materialTypeToApply)
        {
            case MaterialType.LampPost:
                _meshRenderer.material = SceneColourer.LampPostMaterial;
                break;
            case MaterialType.Pavement:
                _meshRenderer.material = SceneColourer.PavementMaterial;
                break;
            case MaterialType.Building:
                _meshRenderer.material = SceneColourer.BuildingMaterial;
                break;
            default:
                Log.Error($"No material type set on {gameObject.name}");
                break;
        }
    }

}
