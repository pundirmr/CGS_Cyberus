using System;
using UnityEngine;
using Unity.Mathematics;

[CreateAssetMenu(fileName = "Avatar", menuName = "Avatar", order = 2)]
public class Avatar : ScriptableObject {
    public string Name;
    public Sprite[] Sprites = new Sprite[3];

    public void Init() {}
}
