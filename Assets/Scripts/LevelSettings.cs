using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "LevelSettings", menuName = "Game/Level/Settings")]
public class LevelSettings : ScriptableObject
{
    [SerializeField]
    public TileBase WallTile;
    
    [SerializeField] 
    public TileBase FloorTile;

    [Header("Maze Settings")] 
    [SerializeField]
    public Vector2Int MazeSize = new Vector2Int(25, 25);

    [SerializeField, Range(1, 2)]
    public int CorridorWidth = 2;
    
    [Header("Maze Exit Settings")] 
    [SerializeField] 
    public int ExitCount = 2;
}
