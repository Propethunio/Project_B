using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class costDict {
    [field: SerializeField] public ResourceSO resource { get; private set; }
    [field: SerializeField] public int cost { get; private set; }
}

[CreateAssetMenu(menuName = "building")]
public class BuildingSO : ScriptableObject {

    [field: SerializeField] public Sprite icon { get; private set; }
    [field: SerializeField] public string buildingName { get; private set; }
    [field: SerializeField] public string buildingDsc { get; private set; }
    [field: SerializeField] public string[] buildingBulletDsc { get; private set; }
    [field: SerializeField] public costDict[] buildingCost { get; private set; }
    [field: SerializeField] public BuildingType buildingType { get; private set; }
}