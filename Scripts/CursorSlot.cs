using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorSlot : ItemContainer
{
    [Header("--- CursorSlot ---")]
    public Canvas canvas;
    public delegate Vector3 CursorPosition();
    public static CursorPosition cursorPosition = new CursorPosition(() =>
    {
        Vector3 v3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        v3.z = current.canvas.transform.position.z;
        return v3;
    });
    public delegate Vector3 CursorSlotPosition();
    public CursorSlotPosition cursorSlotPosition = new CursorSlotPosition(cursorPosition);

    public static CursorSlot current;

    private void Awake()
    {
        current = this;
    }


    private void Update()
    {
        transform.position = cursorSlotPosition();
    }
}
