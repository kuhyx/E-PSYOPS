using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    public enum TileState
    {
        free,
        taken,
        outOfBounds
    }

    public struct Tile
    {
        public Entity standingEntity;
    }

    [Header("Common Values")]

    [SerializeField] private Vector2Int mapSize = new Vector2Int(5,5);
    [SerializeField] private Vector3 WORLD_SPACE_OFFSET = new Vector3(0.5f, 1f, 0.5f);

    [Header("Soldiers values")]

    [SerializeField] private Vector2Int allyBaseCoord = Vector2Int.zero;
    [SerializeField] private Vector2Int[] soldierStartingPositions = null;

    [Header("Enemies values")]

    [SerializeField] private Vector2Int enemyBaseCoord = Vector2Int.zero;
    [SerializeField] private Vector2Int[] enemyStartingPositions = null;

    [Header("References")]

    [SerializeField] private Tilemap tilemap = null;
    [SerializeField] private GameObject soldierPrefab = null;
    [SerializeField] private GameObject basePrefab = null;

    // private (do not edit) variables

    [Header("Debug (do not change)")]

    private Tile[,] tiles = null;

    // ---------- Unity messages

    private void Awake()
    {
        tiles = new Tile[mapSize.x, mapSize.y];
    }

    private void Start()
    {
        //spawn bases
        SpawnSoldier(allyBaseCoord.x, allyBaseCoord.y, true, true);
        SpawnSoldier(enemyBaseCoord.x, enemyBaseCoord.y, false, true);

        //spawn soldiers
        foreach (Vector2Int vec in soldierStartingPositions)
            SpawnSoldier(vec.x, vec.y, true);
        foreach (Vector2Int vec in enemyStartingPositions)
            SpawnSoldier(vec.x, vec.y, false);
    }

    private void OnValidate()
    {
        for (int i=0; i< soldierStartingPositions.Length; i++)
        {
            if (soldierStartingPositions[i].x < 0)
                soldierStartingPositions[i].x = 0;
            if (soldierStartingPositions[i].y < 0)
                soldierStartingPositions[i].y = 0;
            if (soldierStartingPositions[i].x >= mapSize.x)
                soldierStartingPositions[i].x = mapSize.x - 1;
            if (soldierStartingPositions[i].y >= mapSize.y)
                soldierStartingPositions[i].y = mapSize.y - 1;
        }

        for (int i = 0; i < enemyStartingPositions.Length; i++)
        {
            if (enemyStartingPositions[i].x < 0)
                enemyStartingPositions[i].x = 0;
            if (enemyStartingPositions[i].y < 0)
                enemyStartingPositions[i].y = 0;
            if (enemyStartingPositions[i].x >= mapSize.x)
                enemyStartingPositions[i].x = mapSize.x - 1;
            if (enemyStartingPositions[i].y >= mapSize.y)
                enemyStartingPositions[i].y = mapSize.y - 1;
        }
    }

    // ---------- public functions

    public bool SpawnSoldier(int x, int y, bool isAlly, bool isBase=false)
    {
        if (GetTileState(x, y) != TileState.free)
            return false;

        if (isBase)
            tiles[x, y].standingEntity = Instantiate(basePrefab, tilemap.CellToWorld(new Vector3Int(x, y, 0)) + WORLD_SPACE_OFFSET, Quaternion.identity).GetComponent<Base>();
        else
            tiles[x, y].standingEntity = Instantiate(soldierPrefab, tilemap.CellToWorld(new Vector3Int(x, y, 0)) + WORLD_SPACE_OFFSET, Quaternion.identity).GetComponent<Soldier>();

        if (isAlly)
            tiles[x, y].standingEntity.SetOwnTeam(Base.Team.Ally);
        else
            tiles[x, y].standingEntity.SetOwnTeam(Base.Team.Enemy);

        if (tiles[x, y].standingEntity != null)
            return true;

        return false;
    }

    public bool DespawnSoldier(int x, int y)
    {
        if (GetTileState(x, y) != TileState.taken)
            return false;

        Destroy(tiles[x, y].standingEntity.gameObject);
        tiles[x, y].standingEntity = null;
        Debug.Log("Despaned a soldier");

        return true;
    }

    public Entity GetSoldier(int x, int y)
    {
        if (GetTileState(x, y) != TileState.taken)
            return null;

        return tiles[x,y].standingEntity;
    }

    public bool MoveEntity(int x1, int y1, int x2, int y2) // old MoveSoldier
    {
        if (GetTileState(x1, y1) == TileState.taken && GetTileState(x2, y2) == TileState.free)
        {
            tiles[x2, y2].standingEntity = tiles[x1, y1].standingEntity;
            tiles[x1, y1].standingEntity = null;

            tiles[x2, y2].standingEntity.transform.position = tilemap.CellToWorld(new Vector3Int(x2, y2, 0)) + WORLD_SPACE_OFFSET;

            return true;
        }

        return false;
    }

    // ---------- private methods

    private TileState GetTileState(int x, int y)
    {
        if (x < 0 || y < 0 || x >= mapSize.x || y >= mapSize.y)
            return TileState.outOfBounds;

        if (tiles[x, y].standingEntity == null)
            return TileState.free;

        return TileState.taken;
    }

    public TileState GetTileFromWorldCoords(Vector3 worldCoords, out Tile selectedTile, out int x, out int y)
	{
        Vector3Int tilemapCoords = tilemap.WorldToCell(worldCoords);
        TileState tileState = GetTileState(tilemapCoords.x, tilemapCoords.y);
        x = tilemapCoords.x;
        y = tilemapCoords.y;

        if (tileState == TileState.outOfBounds)
        {
            selectedTile = new Tile();
            return TileState.outOfBounds;
        }
        // valid tile selected
        selectedTile = tiles[tilemapCoords.x, tilemapCoords.y];
        return tileState;
        
	}
}
