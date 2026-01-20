using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class GameManager : MonoBehaviour
{


    #region cell behavior

    public enum CellType {
        none,
        a,
        b,
        c,
        d
    }

    [Serializable]
    public class Level {
        public Cell[][] board { get; set; }
        public Cell[] targets { get; set; }

        public Level(Cell[][] board, Cell[] targets) {
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

            for (int r = 0; r < traversed.Length; r++) { // fill w/ false
                for (int c = 0; c < traversed[r].Length; c++) {
                    traversed[r][c] = false;
                }
            }

            dfs(location, traversed, connected); // i love you dfs : )

            return connected.ToArray();
        }

        private void dfs(CellLocation l, bool[][] traversed, List<CellLocation> connected) { // dfs algorithm to find shapes
            traversed[l.x][l.y] = true;
            Cell currentCell = board[l.x][l.y];
            int[] rowVectors = { -1, 1, 0, 0 };
            int[] colVectors = { 0, 0, -1, 1 };

            for (int V = 0; V < rowVectors.Length; V++) {
                int nextR = l.x + rowVectors[V];
                int nextC = l.y + colVectors[V];

                if (ValidLocation(nextR, nextC, currentCell.type) && !traversed[nextR][nextC]) {
                    connected.Add(l);
                    dfs(new CellLocation() { x = nextR, y = nextC }, traversed, connected);
                }
            }
        }

        public void MoveCellGroup(CellLocation from, CellLocation to) {
            if (!ValidLocation(from) || !ValidLocation(to)) return;
            CellLocation offset = new CellLocation() { x = to.x - from.x, y = to.y - from.y };
            CellLocation[] connectedCells = GetConnectedCells(from);


        }

        /* private CellLocation[] OffsetCells(CellLocation[] cells, CellLocation off) {
            CellLocation[] offsetCells = new CellLocation[cells.Length];

            
        } */

        

        public bool ValidLocation(int x, int y) {
            return x >= 0 && x < board.Length && y >= 0 && y < board[x].Length && board[x][y].type != CellType.none;
        }

        public bool ValidLocation(CellLocation l) {
            return l.x >= 0 && l.x < board.Length && l.y >= 0 && l.y < board[l.x].Length && board[l.x][l.y].type != CellType.none;
        }

        public bool ValidLocation(int x, int y, CellType type) {
            if (type == CellType.none) return false;
            return x >= 0 && x < board.Length && y >= 0 && y < board[x].Length && board[x][y].type == type;
        }
    }

    [Serializable]
    public class Cell {

        public CellType type { get; set; }
        public bool filled { get; set; } = false;
        public bool dragging { get; set; } = false;

        public Cell(CellType type) {
            this.type = type;
        }
    }

    public struct CellLocation {
        public int x;
        public int y;
    }

    #endregion

    
    void Start() {
        
    }

    void Update() {
        
    }
}
