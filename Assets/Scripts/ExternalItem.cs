using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CellType = GameManager.CellType;
using CellLocation = GameManager.CellLocation;
public class ExternalItem : MonoBehaviour
{
    GameController controller;
    
    private void Awake() {
        controller = FindObjectOfType<GameController>();
    }

    public CellType type;
    public CellLocation[] shape = {
        new CellLocation { x = 0, y = 0 }
    };

    void OnMouseDown() {
        controller.BeginExternalDrag(this);
    }

    void OnMouseUp() {
        controller.EndExternalDrag();
    }
}
