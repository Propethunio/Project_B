using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class UIContoller : MonoBehaviour {

    [SerializeField] BuildingTypeSO buildingTypeSO;

    VisualElement types, buildings, info, infoIcon, infoCost, dsc;
    Label infoName;
    Dictionary<BuildingType, List<BuildingSO>> buildingsList = new Dictionary<BuildingType, List<BuildingSO>>();

    void Start() {
        GetRefs();
        SetDic();
        SetTypes();
    }

    void GetRefs() {
        UIDocument document = GetComponent<UIDocument>();
        VisualElement root = document.rootVisualElement;
        types = root.Q("types");
        buildings = root.Q("buildings");
        info = root.Q("info");
        infoIcon = root.Q("info-icon");
        infoName = root.Q<Label>("info-name");
        infoCost = root.Q("info-cost");
        dsc = root.Q("dsc");
    }

    void SetDic() {
        BuildingSO[] AllBuildings = Resources.LoadAll<BuildingSO>(Path.Combine("Data", "Buildings"));
        foreach(BuildingType type in Enum.GetValues(typeof(BuildingType))) {
            buildingsList[type] = new List<BuildingSO>();
        }
        foreach(BuildingSO building in AllBuildings) {
            buildingsList[building.buildingType].Add(building);
        }
    }

    void SetTypes() {
        foreach(BuildingTypeData type in buildingTypeSO.buildingTypes) {
            Button btn = new Button();
            btn.AddToClassList("icon");
            btn.style.backgroundImage = new StyleBackground(type.icon);
            BuildingType buildingType = GetEnum(type.typeName);
            btn.clicked += () => SetBuildings(buildingType);
            types.Add(btn);
            types.style.display = DisplayStyle.Flex;
        }
    }

    BuildingType GetEnum(string typeName) {
        foreach(BuildingType key in buildingsList.Keys) {
            if(key.ToString() == typeName) {
                return key;
            }
        }
        throw new ArgumentException("ERROR: Enum not found", nameof(typeName));
    }

    void SetBuildings(BuildingType type) {
        buildings.Clear();
        foreach(BuildingSO building in buildingsList[type]) {
            Button btn = new Button();
            btn.AddToClassList("icon");
            btn.style.backgroundImage = new StyleBackground(building.icon);
            btn.clicked += () => SetInfo(building);
            buildings.Add(btn);
        }
        buildings.style.display = DisplayStyle.Flex;
    }

    void SetInfo(BuildingSO buildingInfo) {
        infoCost.Clear();
        dsc.Clear();
        infoIcon.style.backgroundImage = new StyleBackground(buildingInfo.icon);
        infoName.text = buildingInfo.buildingName;
        foreach(costDict cost in buildingInfo.buildingCost) {
            Label lb = new Label();
            lb.AddToClassList("pop-up-text-color");
            lb.AddToClassList("pop-up-resources-amount");
            lb.text = cost.cost.ToString();
            infoCost.Add(lb);
            VisualElement ve = new VisualElement();
            ve.AddToClassList("pop-up-resources-icon");
            ve.style.backgroundImage = new StyleBackground(cost.resource.icon);
            infoCost.Add(ve);
        }
        Label lbl = new Label();
        lbl.AddToClassList("pop-up-dsc-text");
        lbl.text = buildingInfo.buildingDsc;
        dsc.Add(lbl);
        foreach(string str in buildingInfo.buildingBulletDsc) {
            VisualElement ve = new VisualElement();
            ve.style.flexDirection = FlexDirection.Row;
            Label bullet = new Label();
            bullet.AddToClassList("pop-up-dsc-text");
            bullet.AddToClassList("pop-up-dsc-text-bullet");
            bullet.text = "�";
            ve.Add(bullet);
            Label lb = new Label();
            lb.AddToClassList("pop-up-dsc-text");
            lb.AddToClassList("pop-up-dsc-text-after-bullet");
            lb.text = str;
            ve.Add(lb);
            dsc.Add(ve);
        }
        info.style.display = DisplayStyle.Flex;
    }
}