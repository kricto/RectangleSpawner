using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RectangleUnit : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private SpriteRenderer myRenderer;

    public void OnBeginDrag(PointerEventData eventData)
    {
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
    }

    private void OnMouseEnter()
    {
        RectangleSpawner.Instance.aboveRectangle = true;
    }

    private void OnMouseExit()
    {
        RectangleSpawner.Instance.aboveRectangle = false;
    }

    public void ChangeColor()
    {
        myRenderer.color = new Color32((byte)Random.Range(0, 255),
                                       (byte)Random.Range(0, 255),
                                       (byte)Random.Range(0, 255),
                                       255);
    }

    public void CreateLink()
    {
        
    }

    public void DestroyLink()
    {

    }
}
