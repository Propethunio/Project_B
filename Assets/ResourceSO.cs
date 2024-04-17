using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "resource")]
public class ResourceSO : ScriptableObject {

    [field: SerializeField] public Sprite icon { get; private set; }
    [field: SerializeField] public string resourceName { get; private set; }
}