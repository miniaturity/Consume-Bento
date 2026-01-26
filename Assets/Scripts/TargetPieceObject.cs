using UnityEngine;
using UnityEngine.Tilemaps;

public class TargetPieceObject : MonoBehaviour
{
    public GameManager.TargetPiece targetPiece;
    
    private GameController controller;
    private Grid localGrid;
    private Tilemap localTilemap;

    void Awake()
    {
        controller = FindObjectOfType<GameController>();
        
        GameObject gridObj = new GameObject("Grid");
        gridObj.transform.SetParent(transform);
        gridObj.transform.localPosition = Vector3.zero;
        
        localGrid = gridObj.AddComponent<Grid>();
        localGrid.cellSize = new Vector3(1, 1, 0);
        
        GameObject tilemapObj = new GameObject("Tilemap");
        tilemapObj.transform.SetParent(gridObj.transform);
        tilemapObj.transform.localPosition = Vector3.zero;
        
        localTilemap = tilemapObj.AddComponent<Tilemap>();
        TilemapRenderer renderer = tilemapObj.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = 10; // z-val
    }

    public void Initialize(GameManager.TargetPiece piece, LevelRenderer levelRenderer)
    {
        targetPiece = piece;
        
        if (localTilemap == null) return;

        localTilemap.ClearAllTiles();

        for (int y = 0; y < piece.shape.Length; y++)
        {
            if (piece.shape[y] == null) continue;
            
            for (int x = 0; x < piece.shape[y].Length; x++)
            {
                GameManager.CellType cellType = piece.shape[y][x];
                
                if (cellType != GameManager.CellType.none)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    TileBase tile = levelRenderer.GetTileForType(cellType);
                    localTilemap.SetTile(pos, tile);
                }
            }
        }

        BoxCollider2D collider = gameObject.GetComponent<BoxCollider2D>();
        if (collider == null)
            collider = gameObject.AddComponent<BoxCollider2D>();

        BoundsInt bounds = localTilemap.cellBounds;
        collider.offset = new Vector2(bounds.center.x, bounds.center.y);
        collider.size = new Vector2(bounds.size.x, bounds.size.y);
    }

    void OnMouseDown()
    {
        if (targetPiece != null && !targetPiece.placed)
        {
            controller.BeginTargetDrag(this);
        }
    }

    void OnMouseUp()
    {
        controller.EndTargetDrag();
    }
}