using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public bool hasWon = false;
    public Level level;
    public Dictionary<CellType, TileBase> tiles;
    public TileBase backgroundTile;

    private GameController controller;
    private LevelRenderer renderer;

    public enum CellType
    {
        none,
        a, // rice
        b, // egg
        c, // protein
        d  // vegetable
    }

    [Serializable]
    public class Level
    {
        public Cell[][] board;
        public TargetPiece[] targetPieces;
        public int consumedCount = 0;

        public Level(Cell[][] board, TargetPiece[] targetPieces)
        {
            this.board = board;
            this.targetPieces = targetPieces;
        }

        public CellLocation[] GetConnectedCells(CellLocation location)
        {
            List<CellLocation> connected = new List<CellLocation>();
            bool[][] traversed = new bool[board.Length][];

            for (int i = 0; i < traversed.Length; i++)
            {
                if (board[i] != null)
                {
                    traversed[i] = new bool[board[i].Length];
                }
            }

            dfs(location, traversed, connected);
            return connected.ToArray();
        }

        private void dfs(CellLocation l, bool[][] traversed, List<CellLocation> connected)
        {
            traversed[l.x][l.y] = true;
            connected.Add(l);

            Cell currentCell = board[l.x][l.y];
            int[] rowVectors = { -1, 1, 0, 0 };
            int[] colVectors = { 0, 0, -1, 1 };

            for (int v = 0; v < rowVectors.Length; v++)
            {
                int nextR = l.x + rowVectors[v];
                int nextC = l.y + colVectors[v];

                if (ValidLocation(nextR, nextC, currentCell.type) &&
                    !traversed[nextR][nextC] &&
                    board[nextR][nextC].filled)
                {
                    dfs(new CellLocation { x = nextR, y = nextC }, traversed, connected);
                }
            }
        }

        public bool MoveCellGroup(CellLocation from, CellLocation to)
        {
            if (!ValidLocation(from) || !ValidLocation(to))
                return false;

            if (!board[from.x][from.y].filled)
                return false;

            CellLocation offset = new CellLocation
            {
                x = to.x - from.x,
                y = to.y - from.y
            };

            CellLocation[] group = GetConnectedCells(from);

            foreach (var cell in group)
            {
                int nx = cell.x + offset.x;
                int ny = cell.y + offset.y;

                if (!ValidLocation(nx, ny))
                    return false;

                bool occupiedByOther = board[nx][ny].filled && 
                    Array.FindIndex(group, g => g.x == nx && g.y == ny) == -1;

                if (occupiedByOther)
                    return false;
            }

            CellType[] types = new CellType[group.Length];
            for (int i = 0; i < group.Length; i++)
            {
                types[i] = board[group[i].x][group[i].y].type;
                board[group[i].x][group[i].y].filled = false;
            }

            for (int i = 0; i < group.Length; i++)
            {
                int nx = group[i].x + offset.x;
                int ny = group[i].y + offset.y;
                board[nx][ny].filled = true;
                board[nx][ny].type = types[i];
            }

            return true;
        }

        public void ConsumeCell(CellLocation location)
        {
            if (!ValidLocation(location) || !board[location.x][location.y].filled)
                return;

            CellLocation[] group = GetConnectedCells(location);
            
            foreach (var cell in group)
            {
                board[cell.x][cell.y].filled = false;
                consumedCount++;
            }
        }

        public bool ValidLocation(int x, int y)
        {
            return x >= 0 && x < board.Length && 
                   board[x] != null && 
                   y >= 0 && y < board[x].Length && 
                   board[x][y].type != CellType.none;
        }

        public bool ValidLocation(CellLocation l)
        {
            return ValidLocation(l.x, l.y);
        }

        public bool ValidLocation(int x, int y, CellType type)
        {
            if (type == CellType.none) return false;
            return x >= 0 && x < board.Length && 
                   board[x] != null && 
                   y >= 0 && y < board[x].Length && 
                   board[x][y].type == type;
        }

        public bool CheckWinCondition()
        {
            foreach (var piece in targetPieces)
            {
                if (!piece.placed)
                    return false;
            }
            return true;
        }
    }

    [Serializable]
    public class Cell
    {
        public CellType type;
        public bool filled = false;

        public Cell(CellType type, bool filled = false)
        {
            this.type = type;
            this.filled = filled;
        }
    }

    [Serializable]
    public class TargetPiece
    {
        public CellType[][] shape;
        public bool placed = false;

        public TargetPiece(CellType[][] shape)
        {
            this.shape = shape;
        }

        public CellLocation[] GetFilledCells()
        {
            List<CellLocation> filled = new List<CellLocation>();
            for (int y = 0; y < shape.Length; y++)
            {
                if (shape[y] == null) continue;
                for (int x = 0; x < shape[y].Length; x++)
                {
                    if (shape[y][x] != CellType.none)
                    {
                        filled.Add(new CellLocation { x = x, y = y });
                    }
                }
            }
            return filled.ToArray();
        }

        public CellType GetTypeAt(int x, int y)
        {
            if (y >= 0 && y < shape.Length && shape[y] != null && 
                x >= 0 && x < shape[y].Length)
            {
                return shape[y][x];
            }
            return CellType.none;
        }
    }

    public struct CellLocation
    {
        public int x;
        public int y;
    }

    [Serializable]
    public class LevelData
    {
        public int width;
        public int height;
        public int[][] boardCells;  // 0 = none, 1-4 = types respectively
        public int[][] initialFilled;  
        public TargetPieceData[] targets;
    }

    [Serializable]
    public class TargetPieceData
    {
        public int[][] shape; 
    }

    void Awake()
    {
        controller = GetComponent<GameController>();
        renderer = GetComponent<LevelRenderer>();
    }

    public void LoadLevel(LevelData data)
    {
        Cell[][] board = new Cell[data.height][];
        
        for (int y = 0; y < data.height; y++)
        {
            board[y] = new Cell[data.width];
            for (int x = 0; x < data.width; x++)
            {
                CellType cellType = (CellType)data.boardCells[y][x];
                bool filled = data.initialFilled[y][x] == 1;
                board[y][x] = new Cell(cellType, filled);
            }
        }

        TargetPiece[] targets = new TargetPiece[data.targets.Length];
        for (int i = 0; i < data.targets.Length; i++)
        {
            int[][] shapeData = data.targets[i].shape;
            CellType[][] shape = new CellType[shapeData.Length][];
            
            for (int y = 0; y < shapeData.Length; y++)
            {
                shape[y] = new CellType[shapeData[y].Length];
                for (int x = 0; x < shapeData[y].Length; x++)
                {
                    shape[y][x] = (CellType)shapeData[y][x];
                }
            }
            
            targets[i] = new TargetPiece(shape);
        }

        level = new Level(board, targets);
        
        if (renderer != null)
            renderer.RenderLevel();
    }

    void Update()
    {
        if (!hasWon && level != null && level.CheckWinCondition())
        {
            hasWon = true;
            Debug.Log($"Level Complete! Consumed {level.consumedCount} pieces");
        }
    }
}