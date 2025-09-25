using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// We can move maze settings to scriptable for example & get data from it in the future
/// Made sample script at Scripts/LevelSettings
/// </summary>
[ExecuteInEditMode]
public class MazeCreator : MonoBehaviour
{
    [SerializeField] public Sprite floorSprite;
    [SerializeField] public Tilemap floorTilemap;
    [SerializeField] public Tilemap wallTilemap;

    [SerializeField] public TileBase wallTile;
    [SerializeField] public TileBase floorTile;

    [SerializeField] public Vector2Int mazeSize = new Vector2Int(10, 10);

    [SerializeField, Range(1, 2)] public int corridorWidth = 1;

    [Header("Player and Objects")] public MazeExit ExitPrefab;
    [SerializeField] private Transform _spawnedObjectsParent;

    [Header("Camera Settings")] public Camera mainCamera;

    [Header("Maze Exit Settings")] [SerializeField]
    private int exitCount = 2;

    private Vector2Int _finalMazeSize;
    private Vector2Int _mazeOffset;

    private List<MazeExit> _spawnedExits = new List<MazeExit>();
    private HashSet<Vector2Int> _floorPositions = new HashSet<Vector2Int>();

    private float _originalCameraSize;
    private bool _originalCameraStateSaved;

    private HashSet<Vector2Int> _thinPathData;

    private Vector2Int[] _directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    private static readonly int[] _dirIdx = { 0, 1, 2, 3 };

    private readonly List<Vector2Int> _neighbourBuffer = new List<Vector2Int>(4);
    private readonly HashSet<Vector2Int> _wallPositions = new HashSet<Vector2Int>();
    private readonly List<Vector3Int> _posBuffer3Int = new List<Vector3Int>(256);
    private readonly List<TileBase> _tileBuffer = new List<TileBase>(256);
    private readonly List<RectInt> _existingExitRects = new List<RectInt>(4);
    private readonly List<Vector2Int> _candidatesBuffer = new List<Vector2Int>(64);

    private void Awake()
    {
        if (mainCamera != null)
        {
            _originalCameraSize = mainCamera.orthographicSize;
            _originalCameraStateSaved = true;
        }
    }

    public void SetReferencesFromEditor(Tilemap floorTM, Tilemap wallTM)
    {
        floorTilemap = floorTM;
        wallTilemap = wallTM;
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (floorTilemap == null || wallTilemap == null)
            {
                Debug.LogWarning(
                    "[MazeGenerator] Tilemap referencia hi�nyzik. Futtasd a 'Setup Scene and Assets' gombot.");
            }

            if (floorTile == null)
            {
                Debug.LogWarning(
                    "[MazeGenerator] FloorTile hi�nyzik. Futtasd a 'Setup Scene and Assets' gombot (vagy �ll�ts be egy Tile assetet).");
            }

            if (wallTile == null)
            {
                Debug.LogWarning("[MazeGenerator] WallTile nincs be�ll�tva (RuleTile vagy sima Tile sz�ks�ges).");
            }
#endif
        }
    }

    public void CreateMaze()
    {
        ClearMaze();

        _thinPathData = GenerateThinPathData();
        _floorPositions.Clear();

        foreach (var pos in _thinPathData)
        {
            for (var dx = -corridorWidth / 2; dx <= corridorWidth / 2; dx++)
            {
                for (var dy = -corridorWidth / 2; dy <= corridorWidth / 2; dy++)
                {
                    _floorPositions.Add(pos * corridorWidth + new Vector2Int(dx, dy));
                }
            }
        }

        _finalMazeSize = mazeSize * corridorWidth;
        _mazeOffset = new Vector2Int(_finalMazeSize.x / 2, _finalMazeSize.y / 2);

        DrawMaze();
    }

    public void ClearMaze()
    {
        if (mainCamera != null && _originalCameraStateSaved)
        {
            mainCamera.orthographicSize = _originalCameraSize;
        }

        _floorPositions.Clear();

        if (floorTilemap != null)
        {
            floorTilemap.ClearAllTiles();
        }

        if (wallTilemap != null)
        {
            wallTilemap.ClearAllTiles();
        }

        for (var i = 0; i < _spawnedExits.Count; i++)
        {
            _spawnedExits[i].Hide();
        }

        _spawnedExits.Clear();
    }

    private HashSet<Vector2Int> GenerateThinPathData()
    {
        var path = new HashSet<Vector2Int>();
        var stack = new Stack<Vector2Int>();
        var startPos = new Vector2Int(
            Random.Range(1, mazeSize.x / 2) * 2 - 1,
            Random.Range(1, mazeSize.y / 2) * 2 - 1
        );

        stack.Push(startPos);
        path.Add(startPos);

        while (stack.Count > 0)
        {
            var current = stack.Peek();
            var neighbours = GetUnvisitedNeighbours(ref current, path);

            if (neighbours.Count > 0)
            {
                var chosen = neighbours[Random.Range(0, neighbours.Count)];
                var wallToRemove = current + (chosen - current) / 2;

                path.Add(wallToRemove);
                path.Add(chosen);
                stack.Push(chosen);
            }
            else
            {
                stack.Pop();
            }
        }

        return path;
    }

    private void ShuffleDirections()
    {
        for (var i = _dirIdx.Length - 1; i > 0; i--)
        {
            var j = Random.Range(0, i + 1);
            var tmp = _dirIdx[i];
            _dirIdx[i] = _dirIdx[j];
            _dirIdx[j] = tmp;
        }
    }

    private List<Vector2Int> GetUnvisitedNeighbours(ref Vector2Int pos, HashSet<Vector2Int> visited)
    {
        _neighbourBuffer.Clear();
        ShuffleDirections();

        for (var k = 0; k < _dirIdx.Length; k++)
        {
            var dir = _directions[_dirIdx[k]];
            var newPos = pos + dir * 2;
            if (newPos.x > 0 && newPos.x < mazeSize.x - 1 &&
                newPos.y > 0 && newPos.y < mazeSize.y - 1 &&
                !visited.Contains(newPos))
            {
                _neighbourBuffer.Add(newPos);
            }
        }

        return _neighbourBuffer;
    }

    private void DrawMaze()
    {
        if (floorTilemap == null || wallTilemap == null || floorTile == null)
        {
            return;
        }

        _posBuffer3Int.Clear();
        _tileBuffer.Clear();

        foreach (Vector2Int pos in _floorPositions)
        {
            _posBuffer3Int.Add(new Vector3Int(pos.x - _mazeOffset.x, pos.y - _mazeOffset.y, 0));
            _tileBuffer.Add(floorTile);
        }

        if (_posBuffer3Int.Count > 0)
        {
            floorTilemap.SetTiles(_posBuffer3Int.ToArray(), _tileBuffer.ToArray());
        }

        _wallPositions.Clear();
        foreach (var pos in _floorPositions)
        {
            for (var x = -1; x <= 1; x++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    var n = pos + new Vector2Int(x, y);
                    if (!_floorPositions.Contains(n))
                    {
                        _wallPositions.Add(n);
                    }
                }
            }
        }

        if (wallTile != null && _wallPositions.Count > 0)
        {
            _posBuffer3Int.Clear();
            _tileBuffer.Clear();

            foreach (var pos in _wallPositions)
            {
                _posBuffer3Int.Add(new Vector3Int(pos.x - _mazeOffset.x, pos.y - _mazeOffset.y, 0));
                _tileBuffer.Add(wallTile);
            }

            wallTilemap.SetTiles(_posBuffer3Int.ToArray(), _tileBuffer.ToArray());
        }

        floorTilemap.CompressBounds();
        wallTilemap.CompressBounds();
    }

    public Vector2 GetMazeCenterPosition()
    {
        if (_floorPositions.Count == 0)
        {
            return Vector2.zero;
        }

        var center = new Vector2Int(mazeSize.x / 2, mazeSize.y / 2) * corridorWidth;
        var best = center;
        var bestDist = int.MaxValue;

        foreach (var p in _floorPositions)
        {
            int dx = p.x - center.x;
            int dy = p.y - center.y;
            int dist = dx * dx + dy * dy;
            if (dist < bestDist)
            {
                bestDist = dist;
                best = p;
            }
        }

        return floorTilemap.GetCellCenterWorld((Vector3Int)(best - _mazeOffset));
    }

    public void CreateExits()
    {
        if (wallTilemap == null || _floorPositions.Count == 0)
        {
            return;
        }

        _existingExitRects.Clear();
        var exitsPlaced = 0;
        var attempts = 0;
        var maxAttempts = 1000;

        var minDistanceBetweenExitsSq = Mathf.Pow(Mathf.Min(_finalMazeSize.x, _finalMazeSize.y) / 3f, 2f);

        while (exitsPlaced < exitCount && attempts < maxAttempts)
        {
            attempts++;

            var side = Random.Range(0, 4);
            CollectBorderCandidates(side);
            if (_candidatesBuffer.Count == 0)
            {
                continue;
            }

            var validCount = FilterCandidatesByDistance(minDistanceBetweenExitsSq);
            if (validCount == 0)
            {
                continue;
            }

            var candidate = _candidatesBuffer[Random.Range(0, validCount)];
            var newExitRect = new RectInt(candidate.x, candidate.y, 1, 1);
            if (HasOverlap(newExitRect))
            {
                continue;
            }

            ClearExitTiles(newExitRect);

            SpawnExitPoint(ref candidate, ref exitsPlaced);
            _existingExitRects.Add(newExitRect);
            exitsPlaced++;
        }

        wallTilemap.CompressBounds();
    }

    private void CollectBorderCandidates(int side)
    {
        _candidatesBuffer.Clear();
        foreach (var pos in _floorPositions)
        {
            switch (side)
            {
                case 0:
                    if (pos.y == _finalMazeSize.y - 2)
                    {
                        _candidatesBuffer.Add(new Vector2Int(pos.x, _finalMazeSize.y - 1));
                    }

                    break;
                case 1:
                    if (pos.y == 1)
                    {
                        _candidatesBuffer.Add(new Vector2Int(pos.x, 0));
                    }

                    break;
                case 2:
                    if (pos.x == 1)
                    {
                        _candidatesBuffer.Add(new Vector2Int(0, pos.y));
                    }

                    break;
                case 3:
                    if (pos.x == _finalMazeSize.x - 2)
                    {
                        _candidatesBuffer.Add(new Vector2Int(_finalMazeSize.x - 1, pos.y));
                    }

                    break;
            }
        }
    }

    private int FilterCandidatesByDistance(float minDistanceBetweenExitsSq)
    {
        var writeIdx = 0;
        for (int i = 0; i < _candidatesBuffer.Count; i++)
        {
            var c = _candidatesBuffer[i];
            var farEnough = true;
            for (var r = 0; r < _existingExitRects.Count; r++)
            {
                var rc = _existingExitRects[r].center;
                var dx = c.x - rc.x;
                var dy = c.y - rc.y;
                if (dx * dx + dy * dy < minDistanceBetweenExitsSq)
                {
                    farEnough = false;
                    break;
                }
            }

            if (farEnough)
            {
                _candidatesBuffer[writeIdx++] = c;
            }
        }

        return writeIdx;
    }

    private bool HasOverlap(RectInt newExitRect)
    {
        for (int i = 0; i < _existingExitRects.Count; i++)
        {
            if (_existingExitRects[i].Overlaps(newExitRect))
            {
                return true;
            }
        }

        return false;
    }

    private void ClearExitTiles(RectInt newExitRect)
    {
        _posBuffer3Int.Clear();
        _tileBuffer.Clear();
        for (var dx = 0; dx < newExitRect.width; dx++)
        {
            for (var dy = 0; dy < newExitRect.height; dy++)
            {
                var tileToClear = newExitRect.position + new Vector2Int(dx, dy);
                if (tileToClear.x < 0 || tileToClear.x >= _finalMazeSize.x ||
                    tileToClear.y < 0 || tileToClear.y >= _finalMazeSize.y)
                {
                    continue;
                }

                _posBuffer3Int.Add(new Vector3Int(tileToClear.x - _mazeOffset.x, tileToClear.y - _mazeOffset.y, 0));
                _tileBuffer.Add(null);
            }
        }

        if (_posBuffer3Int.Count > 0)
        {
            wallTilemap.SetTiles(_posBuffer3Int.ToArray(), _tileBuffer.ToArray());
        }
    }


    //TODO move to separate obj pool manager when needed
    private void SpawnExitPoint(ref Vector2Int candidate, ref int exitsPlaced)
    {
        if (ExitPrefab != null)
        {
            var position = wallTilemap.GetCellCenterWorld(new Vector3Int(
                candidate.x - _mazeOffset.x, candidate.y - _mazeOffset.y, 0));
            var exitObj = Instantiate(ExitPrefab, position, Quaternion.identity);
            var size = new Vector2(0.5f, 0.5f);
            exitObj.Init(_spawnedObjectsParent, ref exitsPlaced, ref size);

            _spawnedExits.Add(exitObj);
        }
    }
}