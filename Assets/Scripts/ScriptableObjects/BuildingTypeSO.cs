using UnityEditor;
using System.IO;
using UnityEngine;
using System;

[Serializable]
public class BuildingTypeData {

    [field: SerializeField] public Sprite icon { get; private set; }
    [field: SerializeField] public string typeName { get; private set; }
}

[CreateAssetMenu(menuName = "buildingTypes")]
public class BuildingTypeSO : ScriptableObject {

    [field: SerializeField] public BuildingTypeData[] buildingTypes { get; private set; }

    public void GenerateEnum() {
        string enumName = "BuildingType";
        string filePathAndName = Path.Combine("Assets", "Scripts", "Enums", $"{enumName}.cs");
        string[] enumEntries = Array.ConvertAll(buildingTypes, bt => bt.typeName);

        using(StreamWriter streamWriter = new StreamWriter(filePathAndName)) {
            streamWriter.WriteLine($"public enum {enumName} {{");
            foreach(string entry in enumEntries) {
                streamWriter.WriteLine($"\t{entry},");
            }
            streamWriter.WriteLine("}");
        }
        AssetDatabase.Refresh();
    }
}

[CustomEditor(typeof(BuildingTypeSO))]
public class BuildingTypeEnumGenerator : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        BuildingTypeSO script = (BuildingTypeSO)target;

        if(GUILayout.Button("Generate Building Types", GUILayout.Height(40))) {
            script.GenerateEnum();
        }
    }
}