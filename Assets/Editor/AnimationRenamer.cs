// https://stackoverflow.com/questions/42743329/rename-animation-clips-when-importing-in-the-editor
// https://forum.unity.com/threads/modelimporter-how-to-modify-filescale-in-unity.885484/

using UnityEngine;
using UnityEditor;
public class AnimationRenamer : EditorWindow
{
    [MenuItem("Assets/Fix Animation Names")]
    private static void FixAnimationNames()
    {
        var guids = Selection.assetGUIDs;
        foreach (var g in guids)
        {
            if (g != null)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(g);

                var assetImporter = AssetImporter.GetAtPath(assetPath);

                if (assetImporter is ModelImporter model)
                {
                    Debug.Log($"Renaming animations of: {assetPath}");
                    ModelImporterClipAnimation[] clipAnimations = model.defaultClipAnimations;
                    for (int i = 0; i < clipAnimations.Length; i++)
                    {
                        clipAnimations[i].name = clipAnimations[i].name.Split('|')[1];
                    }
                    model.clipAnimations = clipAnimations;
                    model.SaveAndReimport();
                }
                else Debug.LogError($"This isn't a model: {assetPath}");
            }
        }
        Debug.Log("Done!");
    }
}