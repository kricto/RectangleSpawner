using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkingNod : MonoBehaviour
{
    [SerializeField] private Rigidbody2D myRigidbody;

    private const int numberOfHits = 3;
    private const int distanceInFrontOfCamera = 10;

    private Vector3 mousePosition;
    private ContactFilter2D filter = new ContactFilter2D();
    private List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(numberOfHits);

    //Движение узла мышкой
    private void OnMouseDrag()
    {
        UpdateMousePosition();

        transform.position = mousePosition;
    }

    //Когда отпускаем левую кнопку, надо проверить что было под курсором
    private void OnMouseUp()
    {
        RaycastHit2D[] hits = new RaycastHit2D[numberOfHits];

        int count = myRigidbody.Cast(Vector2.zero, filter, hits); //Получаем всех под курсором проектируя все коллайдеры в область под собой

        hitBufferList.Clear();

        for (int i = 0; i < count; i++)
        {
            hitBufferList.Add(hits[i]); //Переводим в лист не нуловые взаимодействия
        }

        foreach (RaycastHit2D hit in hitBufferList)
        {
            GetComponentInParent<RectangleUnit>().CreateLink(hit.transform); //Создаем связи со всеми, кого коснулись
        }

        transform.localPosition = Vector3.zero; //Возвращаем узел на стартовую позицию
    }

    private void UpdateMousePosition()
    {
        mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceInFrontOfCamera);
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
    }
}
