using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
public class GameManager : MonoBehaviour
{
    public bool hasWon = false;
    public Level level;

    #region cell behavior

    public enum CellType {
        none,
        a, // rice
        b, // egg
        c, //
        d
    }

    [Serializable]
    public class Level {
        public Cell[][] board;
        public Cell[][] targets;

        public Level(Cell[][] board, Cell[][] targets) {
            this.board = board;
            this.targets = targets;
        }

        public CellLocation[] GetEmptyNeighbors(CellLocation location) {
            int[] rowVectors = { -1, 1, 0, 0 };
            int[] colVectors = { 0, 0, -1, 1 };
            List<CellLocation> neighbors = new List<CellLocation>();

            for (int V = 0; V < rowVectors.Length; V++) {
                int nextR = location.x + rowVectors[V];
                int nextC = location.y + colVectors[V];

                if (ValidLocation(nextR, nextC) && !board[nextR][nextC].filled) {
                    neighbors.Add(new CellLocation() { x = nextR, y = nextC });
                }
            }

            return neighbors.ToArray();
        }

        public CellLocation[] GetConnectedCells(CellLocation location) {
            List<CellLocation> connected = new List<CellLocation>();
            bool[][] traversed = new bool[board.Length][];

            for (int i = 0; i < traversed.Length; i++) {
                if (board[i] != null) { // annoying way to copy the dimensions of the array.
                    traversed[i] = new bool[board[i].Length];
                }
            }

            for (int r = 0; r < traversed.Length; r++) {
                if (traversed[r] == null) continue;
                for (int c = 0; c < traversed[r].Length; c++) {
                    traversed[r][c] = false;
                }
            }

            dfs(location, traversed, connected); 

            return connected.ToArray();
        }

        private void dfs(CellLocation l, bool[][] traversed, List<CellLocation> connected) {
            traversed[l.x][l.y] = true;
            connected.Add(l);

            Cell currentCell = board[l.x][l.y];
            int[] rowVectors = { -1, 1, 0, 0 };
            int[] colVectors = { 0, 0, -1, 1 };

            for (int v = 0; v < rowVectors.Length; v++) {
                int nextR = l.x + rowVectors[v];
                int nextC = l.y + colVectors[v];

                if (
                    ValidLocation(nextR, nextC, currentCell.type) &&
                    !traversed[nextR][nextC]
                ) {
                    dfs(new CellLocation { x = nextR, y = nextC }, traversed, connected);
                }
            }
        }

        public bool MoveCellGroup(CellLocation from, CellLocation to) {
            if (!ValidLocation(from) || !ValidLocation(to))
                return false;

            CellLocation offset = new CellLocation {
                x = to.x - from.x,
                y = to.y - from.y
            };

            CellLocation[] group = GetConnectedCells(from);

            foreach (var cell in group) {
                int nx = cell.x + offset.x;
                int ny = cell.y + offset.y;

                if (!ValidLocation(nx, ny))
                    return false;

                bool occupiedByOther = board[nx][ny].filled && Array.FindIndex(group, g => g.x == nx && g.y == ny) == -1;

                if (occupiedByOther)
                    return false;
            }

            foreach (var cell in group)
                board[cell.x][cell.y].filled = false;

            foreach (var cell in group) {
                int nx = cell.x + offset.x;
                int ny = cell.y + offset.y;
                board[nx][ny].filled = true;
            }

            return true;
        }

        public bool ValidLocation(int x, int y) {
            return x >= 0 && x < board.Length && board[x] != null && y >= 0 && y < board[x].Length && board[x][y].type != CellType.none;
        }

        public bool ValidLocation(CellLocation l) {
            return ValidLocation(l.x, l.y);
        }

        public bool ValidLocation(int x, int y, CellType type) {
            if (type == CellType.none) return false;
            return x >= 0 && x < board.Length && board[x] != null && y >= 0 && y < board[x].Length && board[x][y].type == type;
        }

        
    }

    [Serializable]
    public class Cell {
        public CellType type;
        public bool filled = false;
        public bool dragging = false;

        public Cell(CellType type) {
            this.type = type;
        }
    }

    public struct CellLocation {
        public int x;
        public int y;
    }

    #endregion

    #region load level data
    
    [Serializable]
    public class LevelData {
        public int width;
        public int height;
        public int[][] cells;
        public int[][] externalCells;
    }

    #endregion

    #region rendering
    public Dictionary<CellType, TileBase> tiles;
    public TileBase backgroundTile;



    #endregion

    void Start() {  
        
    }

    void Update() {
        
    }
}
