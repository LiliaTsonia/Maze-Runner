#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(MazeCreator))]
public class MazeCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var generator = (MazeCreator)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Setup Scene and Assets", GUILayout.Height(40)))
        {
            SetupSceneAndAssets(generator);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Maze", GUILayout.Height(30)))
        {
            generator.CreateMaze();
        }

        if (GUILayout.Button("Clear Maze", GUILayout.Height(30)))
        {
            generator.ClearMaze();
        }
    }

    private void SetupSceneAndAssets(MazeCreator creator)
    {
        var grid = FindFirstObjectByType<Grid>();

        if (grid == null)
        {
            var gridObject = new GameObject("Grid");
            grid = gridObject.AddComponent<Grid>();
        }

        var floorTilemap = FindOrCreateTilemap("Floor", grid.transform, 0);
        var wallTilemap = FindOrCreateTilemap("Walls", grid.transform, 1);

        var assetsFolderPath = "Assets/Resources/LocalPack/Tiles";

        if (!Directory.Exists(assetsFolderPath))
        {
            Directory.CreateDirectory(assetsFolderPath);
        }

        CreateOrLoadTile(
            Path.Combine(assetsFolderPath, "FloorTile.asset"),
            creator.floorSprite
        );

        creator.SetReferencesFromEditor(floorTilemap, wallTilemap);
        EditorUtility.SetDirty(creator);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private Tilemap FindOrCreateTilemap(string name, Transform parent, int sortingOrder)
    {
        Transform t = parent.Find(name);
        Tilemap tilemap;

        if (t == null || !t.TryGetComponent<Tilemap>(out tilemap))
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            tilemap = go.AddComponent<Tilemap>();
            var rend = go.AddComponent<TilemapRenderer>();
            rend.sortingOrder = sortingOrder;
        }

        return tilemap;
    }

    private Tile CreateOrLoadTile(string tileAssetPath, Sprite sprite)
    {
        var tile = AssetDatabase.LoadAssetAtPath<Tile>(tileAssetPath);
        if (tile == null)
        {
            tile = CreateInstance<Tile>();
            AssetDatabase.CreateAsset(tile, tileAssetPath);
        }


        tile.sprite = sprite;
        tile.color = Color.white;
        EditorUtility.SetDirty(tile);
        return tile;
    }
}

#endif
