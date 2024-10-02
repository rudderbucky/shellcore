using UnityEngine;

public class Draggable : MonoBehaviour
{
    int _drags = 0;

    public bool Dragging
    {
        get => _drags > 0;
    }

    public void AddDrag()
    {
        _drags++;
    }

    public void RemoveDrag()
    {
        _drags--;
    }
}
