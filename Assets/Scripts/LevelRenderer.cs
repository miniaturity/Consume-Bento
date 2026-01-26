using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelRenderer : MonoBehaviour
{
    public Tilemap background;
    public Tilemap contents;
    public Grid bentoGrid;
    public Vector2Int contentsOffset = new Vector2Int(1, 1);

    public Dictionary<GameManager.CellType, TileBase> foodTextures;
    public TileBase defaultTile;

    public GameObject targetPiecePrefab;
    public Transform targetPieceContainer;
    public Vector3 targetStartPosition = new Vector3(8, 4, 0);
    public float targetSpacing = 2f;

    private GameManager gameManager;

    void Awake()
    {
        gameManager = GetComponent<GameManager>();
    }

    public void RenderLevel()
    {
        if (gameManager.level == null) return;

        background.ClearAllTiles();
        contents.ClearAllTiles();

        for (int y = 0; y < gameManager.level.board.Length; y++)
        {
            if (gameManager.level.board[y] == null) continue;

            for (int x = 0; x < gameManager.level.board[y].Length; x++)
            {
                var cell = gameManager.level.board[y][x];
                Vector3Int pos = new Vector3Int(x + contentsOffset.x, y + contentsOffset.y, 0);

                // bg
                if (cell.type != GameManager.CellType.none)
                {
                    background.SetTile(pos, backgroundTile);
                }

                // filled
                if (cell.filled)
                {
                    contents.SetTile(pos, GetTileForType(cell.type));
                }
            }
        }
    }

    public void SpawnTargetPieces()
    {
        if (gameManager.level == null || gameManager.level.targetPieces == null) return;

        if (targetPieceContainer != null)
        {
            foreach (Transform child in targetPieceContainer)
            {
                Destroy(child.gameObject);
            }
        }

        // spawn
        for (int i = 0; i < gameManager.level.targetPieces.Length; i++)
        {
            Vector3 spawnPos = targetStartPosition + Vector3.down * (i * targetSpacing);
            GameObject pieceObj = Instantiate(targetPiecePrefab, spawnPos, Quaternion.identity);
            
            if (targetPieceContainer != null)
                pieceObj.transform.SetParent(targetPieceContainer);

            TargetPieceObject pieceScript = pieceObj.GetComponent<TargetPieceObject>();
            if (pieceScript != null)
            {
                pieceScript.Initialize(gameManager.level.targetPieces[i], this);
            }
        }
    }

    public TileBase GetTileForType(GameManager.CellType type)
    {
        return foodTextures[type] ?? defaultTile;
    }
}