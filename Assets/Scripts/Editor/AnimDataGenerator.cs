using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class AnimDataGenerator : EditorWindow
{
    static readonly string[] ScanFolders =
    {
        "Assets/Art/Sprites/Player",
        "Assets/Art/Sprites/Enemy"
    };
    const string OutputFolder = "Assets/Art/Animations";

    [MenuItem("Roguelike/Generate Anim Data")]
    static void Generate()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Art/Animations"))
            AssetDatabase.CreateFolder("Assets/Art", "Animations");

        var idleRunPattern = new Regex(@"^(.+?)_(idle|run)_anim_f(\d+)$");
        var genericPattern  = new Regex(@"^(.+?)_anim_f(\d+)$");

        // charName → (idle list, run list)
        var map = new Dictionary<string, (List<Sprite> idle, List<Sprite> run)>();

        foreach (var folder in ScanFolders)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Sprite", new[] { folder }))
            {
                var path   = AssetDatabase.GUIDToAssetPath(guid);
                var fname  = Path.GetFileNameWithoutExtension(path);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null) continue;
                if (fname.Contains("_hit_")) continue;

                var m = idleRunPattern.Match(fname);
                if (m.Success)
                {
                    var key  = m.Groups[1].Value;
                    var type = m.Groups[2].Value;
                    if (!map.ContainsKey(key)) map[key] = (new List<Sprite>(), new List<Sprite>());
                    (type == "run" ? map[key].run : map[key].idle).Add(sprite);
                    continue;
                }

                m = genericPattern.Match(fname);
                if (m.Success)
                {
                    var key = m.Groups[1].Value;
                    if (!map.ContainsKey(key)) map[key] = (new List<Sprite>(), new List<Sprite>());
                    map[key].idle.Add(sprite);
                }
            }
        }

        int count = 0;
        foreach (var kv in map)
        {
            var assetPath = $"{OutputFolder}/{kv.Key}.asset";
            var existing  = AssetDatabase.LoadAssetAtPath<CharacterAnimData>(assetPath);
            var data      = existing ?? ScriptableObject.CreateInstance<CharacterAnimData>();

            data.idleFrames = SortFrames(kv.Value.idle);
            data.runFrames  = SortFrames(kv.Value.run);
            data.fps        = 8f;

            if (existing == null)
                AssetDatabase.CreateAsset(data, assetPath);
            else
                EditorUtility.SetDirty(data);

            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료", $"{count}개 CharacterAnimData 생성/갱신\n→ {OutputFolder}", "OK");
        Debug.Log($"[AnimDataGenerator] {count}개 완료");
    }

    static Sprite[] SortFrames(List<Sprite> list)
    {
        var frameNum = new Regex(@"_f(\d+)$");
        return list
            .OrderBy(s => { var m = frameNum.Match(s.name); return m.Success ? int.Parse(m.Groups[1].Value) : 0; })
            .ToArray();
    }
}
