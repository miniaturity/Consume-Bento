using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CellLocation = GameManager.CellLocation;
using Level = GameManager.Level;


public class GameController : MonoBehaviour
{
    public Grid bentoGrid;
    public Tilemap contents;
    public Level level;

    private CellLocation? selectedCell = null;
    private CellLocation dragOrigin;

    ExternalItem draggedExternalFood;
    Vector3 externalStartPosition;

    public Vector2Int contentsOffset = new Vector2Int(1, 1); // padding

    void Update()
    {
        HandleMouse();
        HandleKeyboardMovement();

        HandleExternalDragging();
    }

    #region input
    void HandleMouse() {
        if (Input.GetMouseButtonDown(0)) {
            var cell = MouseToCell();
            if (cell != null && level.board[cell.Value.x][cell.Value.y].filled) {
                selectedCell = cell;
                dragOrigin = cell.Value;
            }
        }

        if (Input.GetMouseButtonUp(0) && selectedCell != null) {
            var release = MouseToCell();
            if (release != null)
                level.MoveCellGroup(dragOrigin, release.Value);

            selectedCell = null;
        }
    }


    void HandleKeyboardMovement() {
        if (selectedCell == null) return;

        int dx = 0, dy = 0;

        if (Input.GetKeyDown(KeyCode.W)) dy = 1;
        if (Input.GetKeyDown(KeyCode.S  )) dy = -1;
        if (Input.GetKeyDown(KeyCode.A)) dx = -1;
        if (Input.GetKeyDown(KeyCode.D)) dx = 1;

        if (dx != 0 || dy != 0) {
            CellLocation from = selectedCell.Value;
            CellLocation to = new CellLocation {
                x = from.x + dx,
                y = from.y + dy
            };

            level.MoveCellGroup(from, to);
        }
    }

    CellLocation? MouseToCell() {
        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector3Int cell = bentoGrid.WorldToCell(world);

        cell.x -= 1;
        cell.y -= 1; // starts at (1, 1) so i have to normalize



        CellLocation l = new CellLocation { x = cell.x, y = cell.y };

        return level.ValidLocation(l) ? l : null;
    }
    #endregion

    #region external input

    public void BeginExternalDrag(ExternalItem food) {
        draggedExternalFood = food;
        externalStartPosition = food.transform.position;
    }

    void HandleExternalDragging() {
        if (draggedExternalFood == null) return;

        var cell = MouseToCell();

        if (cell != null) {
            Vector3Int tilePos = new Vector3Int(
                cell.Value.x + contentsOffset.x,
                cell.Value.y + contentsOffset.y,
                0
            );

            Vector3 snap = contents.CellToWorld(tilePos);
            draggedExternalFood.transform.position = snap;
        }
        else {
            Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0;
            draggedExternalFood.transform.position = world;
        }
    }

    public void EndExternalDrag() {
        if (draggedExternalFood == null) return;

        var cell = MouseToCell();

        if (cell != null && CanPlaceExternalFood(cell.Value, draggedExternalFood)) {
            PlaceExternalFood(cell.Value, draggedExternalFood);
            draggedExternalFood = null;
        }
        else {
            draggedExternalFood.transform.position = externalStartPosition;
            draggedExternalFood = null;
        }
    }

    bool CanPlaceExternalFood(CellLocation origin, ExternalItem food) {
        foreach (var offset in food.shape) {
            var target = new CellLocation {
                x = origin.x + offset.x,
                y = origin.y + offset.y
            };

            if (!level.ValidLocation(target)) return false;
            if (level.board[target.x][target.y].filled) return false;
        }

        return true;
    }

    void PlaceExternalFood(CellLocation origin, ExternalItem food) {
        foreach (var offset in food.shape) {
            var cell = new CellLocation {
                x = origin.x + offset.x,
                y = origin.y + offset.y
            };

            level.board[cell.x][cell.y].type = food.type;
            level.board[cell.x][cell.y].filled = true;
        }

        Destroy(food.gameObject);
    }


    #endregion
}
