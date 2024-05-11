using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class costDict {
    [field: SerializeField, HideLabel, HorizontalGroup(.7f)] public ResourceSO resource { get; private set; }
    [field: SerializeField, HideLabel, HorizontalGroup()] public int cost { get; private set; }
}

[CreateAssetMenu(menuName = "building")]
public class BuildingSO : ScriptableObject {

    [field: SerializeField, PreviewField(75), HideLabel, HorizontalGroup("GameData", 75), VerticalGroup("GameData/left")]
    public AssetReferenceT<Sprite> icon { get; private set; }

    [field: SerializeField, VerticalGroup("GameData/right"), LabelText("Name")]
    public string buildingName { get; private set; }

    [field: SerializeField, MultiLineProperty(2), VerticalGroup("GameData/right"), LabelText("Description")]
    public string buildingDsc { get; private set; }

    [field: SerializeField, VerticalGroup("GameData/right"), LabelText("Type")]
    public BuildingType buildingType { get; private set; }

    [field: SerializeField, LabelText("Bullet Description")]
    public string[] buildingBulletDsc { get; private set; }

    [field: SerializeField, LabelText("Cost")]
    public costDict[] buildingCost { get; private set; }
}