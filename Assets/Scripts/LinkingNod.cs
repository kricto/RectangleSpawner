using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkingNod : MonoBehaviour
{
    [SerializeField] private Rigidbody2D myRigidbody;

    private Vector3 mousePosition;
    private ContactFilter2D filter;
    private List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(3);

    private void Awake()
    {
        filter = new ContactFilter2D();
        filter.useTriggers = false;
    }

    private void OnMouseDrag()
    {
        UpdateMousePosition();

        transform.position = mousePosition;
    }

    private void OnMouseUp()
    {
        RaycastHit2D[] hits = new RaycastHit2D[3];

        int count = myRigidbody.Cast(Vector2.zero, filter, hits);

        hitBufferList.Clear();

        for (int i = 0; i < count; i++)
        {
            hitBufferList.Add(hits[i]);
        }

        foreach (RaycastHit2D hit in hitBufferList)
        {
            if (hit.transform.position != transform.position)
            {
                GetComponentInParent<RectangleUnit>().CreateLink(hit.transform);
            }
        }

        transform.localPosition = Vector3.zero;
    }

    private void UpdateMousePosition()
    {
        mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
    }
}
