using UnityEngine;
using UnityEngine.Tilemaps;
using CellLocation = GameManager.CellLocation;
using CellType = GameManager.CellType;

public class GameController : MonoBehaviour {
    public Grid bentoGrid;
    public Tilemap contents;
    public Vector2Int contentsOffset = new Vector2Int(1, 1);

    private GameManager gm;
    private LevelRenderer levelRenderer;

    private CellLocation? selectedCell = null;
    private CellLocation dragOrigin;
    private TargetPieceObject draggedTargetPiece;
    private Vector3 externalStartPosition;

    private enum InputMode {
        None,
        DraggingFood,
        DraggingTarget
    }

    private InputMode currentMode = InputMode.None;

    void Awake() {
        gm = GetComponent<GameManager>();
        levelRenderer = GetComponent<LevelRenderer>();
    }

    void Update() {
        if (gm.level == null) return;

        if (currentMode == InputMode.DraggingTarget) {
            HandleTargetDragging();
        }
        else if (currentMode == InputMode.None) {
            HandleMouse();
            HandleKeyboardMovement();
        }
    }

    #region food mvmnt
    void HandleMouse() {
        if (Input.GetMouseButtonDown(0)) {
            var cell = MouseToCell();
            if (cell != null && gm.level.board[cell.Value.x][cell.Value.y].filled) {
                selectedCell = cell;
                dragOrigin = cell.Value;
                currentMode = InputMode.DraggingFood;
            }
        }

        if (Input.GetMouseButtonDown(1)) {
            var cell = MouseToCell();
            if (cell != null) {
                gm.level.ConsumeCell(cell.Value);
                levelRenderer.RenderLevel();
            }
        }

        if (Input.GetMouseButtonUp(0) && currentMode == InputMode.DraggingFood) {
            if (selectedCell != null) {
                var release = MouseToCell();
                if (release != null && gm.level.MoveCellGroup(dragOrigin, release.Value)) {
                    levelRenderer.RenderLevel();
                }
                selectedCell = null;
            }
            currentMode = InputMode.None;
        }
    }

    void HandleKeyboardMovement() {
        if (selectedCell == null) return;

        int dx = 0, dy = 0;

        if (Input.GetKeyDown(KeyCode.W)) dy = 1;
        if (Input.GetKeyDown(KeyCode.S)) dy = -1;
        if (Input.GetKeyDown(KeyCode.A)) dx = -1;
        if (Input.GetKeyDown(KeyCode.D)) dx = 1;

        if (dx != 0 || dy != 0) {
            CellLocation from = selectedCell.Value;
            CellLocation to = new CellLocation {
                x = from.x + dx,
                y = from.y + dy
            };

            if (gm.level.MoveCellGroup(from, to)) {
                selectedCell = to;
                levelRenderer.RenderLevel();
            }
        }
    }
    #endregion

    #region dragging
    public void BeginTargetDrag(TargetPieceObject piece) {
        draggedTargetPiece = piece;
        externalStartPosition = piece.transform.position;
        currentMode = InputMode.DraggingTarget;
    }

    void HandleTargetDragging() {
        if (draggedTargetPiece == null) return;

        var cell = MouseToCell();

        if (cell != null) {
            Vector3Int tilePos = new Vector3Int(
                cell.Value.x + contentsOffset.x,
                cell.Value.y + contentsOffset.y,
                0
            );

            Vector3 snap = contents.CellToWorld(tilePos);
            draggedTargetPiece.transform.position = snap;
        }
        else {
            Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0;
            draggedTargetPiece.transform.position = world;
        }
    }

    public void EndTargetDrag() {
        if (draggedTargetPiece == null) return;

        var cell = MouseToCell();

        if (cell != null && CanPlaceTargetPiece(cell.Value, draggedTargetPiece)) {
            PlaceTargetPiece(cell.Value, draggedTargetPiece);
            Destroy(draggedTargetPiece.gameObject);
            levelRenderer.RenderLevel();
        }
        else {
            draggedTargetPiece.transform.position = externalStartPosition;
        }

        draggedTargetPiece = null;
        currentMode = InputMode.None;
    }

    bool CanPlaceTargetPiece(CellLocation origin, TargetPieceObject piece) {
        CellLocation[] filledCells = piece.targetPiece.GetFilledCells();

        foreach (var cell in filledCells) {
            var target = new CellLocation {
                x = origin.x + cell.x,
                y = origin.y + cell.y
            };

            if (!gm.level.ValidLocation(target)) return false;
            if (gm.level.board[target.x][target.y].filled) return false;
        }

        return true;
    }

    void PlaceTargetPiece(CellLocation origin, TargetPieceObject piece) {
        CellLocation[] filledCells = piece.targetPiece.GetFilledCells();

        foreach (var cell in filledCells) {
            var boardCell = new CellLocation {
                x = origin.x + cell.x,
                y = origin.y + cell.y
            };

            CellType type = piece.targetPiece.GetTypeAt(cell.x, cell.y);
            gm.level.board[boardCell.x][boardCell.y].type = type;
            gm.level.board[boardCell.x][boardCell.y].filled = true;
        }

        piece.targetPiece.placed = true;
    }
    #endregion

    #region Utility
    CellLocation? MouseToCell() {
        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cell = bentoGrid.WorldToCell(world);

        cell.x -= contentsOffset.x;
        cell.y -= contentsOffset.y;

        CellLocation l = new CellLocation { x = cell.x, y = cell.y };

        return gm.level.ValidLocation(l) ? l : null;
    }
    #endregion
}