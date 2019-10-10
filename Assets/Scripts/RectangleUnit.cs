using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RectangleUnit : MonoBehaviour
{
    [SerializeField] private SpriteRenderer myRenderer;
    [SerializeField] private Rigidbody2D myRigidbody;
    [SerializeField] private CircleCollider2D ignoreCollider;
    [SerializeField] private LineRenderer line;
    [SerializeField] private GameObject nod;
    [Range(1f, 100f)]
    [SerializeField] private float speed = 1f;

    private const float minMoveDistance = .001f;
    private const float shellRadius = .01f;
    private const float clickCooldown = .2f;

    private float clickTimer = .0f;

    private Vector3 clickOffset;
    private Vector3 mousePosition;
    private Vector2 newPosition;

    private Transform connectedObj;

    private RaycastHit2D[] hitBuffer = new RaycastHit2D[6];
    private List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(6);

    public System.Action OnDestroy;

    private void FixedUpdate()
    {
        //Вызов главной функции движения, происходит в FixedUpdate для плавности
        MoveRectangle((newPosition - (Vector2)transform.position) * speed * Time.deltaTime);
    }

    private void OnMouseEnter()
    {
        //Флаг для проверки курсора над объектом.
        //Нужен для уменьшения количества вызова функции проверки коллайдеров под курсором в RectangleSpawner
        RectangleSpawner.Instance.aboveRectangle = true;
    }

    private void OnMouseDown()
    {
        UpdateMousePosition();
        clickOffset = transform.position - mousePosition; //Сдвиг курсора относительно центра объекта, чтобы он не двигался пока курсор стоит на мыши

        if ((Time.time - clickTimer) < clickCooldown)
        {
            Invoke(nameof(DeleteRectangle), .1f); //Задержка удаления, чтобы пропустить клик и не создать прямоугольник в месте удаления
        }

        clickTimer = Time.time;
    }

    private void OnMouseDrag()
    {
        UpdateMousePosition();

        //Задается новое положение курсора со сдвигом относительно центра объекта,
        //для движения прямоугольника
        newPosition = mousePosition + clickOffset;
    }

    private void OnMouseExit()
    {
        newPosition = transform.position; //Останавливаем движение, когда отпускаем кнопку мыши
        RectangleSpawner.Instance.aboveRectangle = false;
    }

    //Функция, в котором случайно задаются каналы RGB
    public void ChangeColor()
    {
        myRenderer.color = new Color32((byte)Random.Range(0, 255),
                                       (byte)Random.Range(0, 255),
                                       (byte)Random.Range(0, 255),
                                       255);

        newPosition = transform.position;
    }

    //Функция для обновления положения курсора в глобальном пространстве
    private void UpdateMousePosition()
    {
        mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
    }

    private void MoveRectangle(Vector2 move)
    {
        float distance = move.magnitude;

        if(distance > minMoveDistance)
        {
            //Проецирование всех коллайдеров на объекте наперёд и обнаружение коллизий с проекциями
            int count = myRigidbody.Cast(move, hitBuffer, distance + shellRadius);
            
            hitBufferList.Clear();
            
            for (int i = 0; i < count; i++)
            {
                if(hitBuffer[i].collider != ignoreCollider)
                    hitBufferList.Add(hitBuffer[i]);
            }

            //Участок для проверки коллизий и последующее отталкивание прямоугольника
            for(int i = 0; i < hitBufferList.Count; i++)
            {
                float modifiedDistance = hitBufferList[i].distance - shellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance;

                //Данные проверки помогают избежать вхождение прямоугольников друг в друга, 
                //во время движения курсором после коллизии(на коллизии нужно потратить много времени для решения данной проблемы)
                if (hitBufferList[i].collider.transform.position.x > transform.position.x)
                    move.x = .1f;
                if (hitBufferList[i].collider.transform.position.x < transform.position.x)
                    move.x = -.1f;
                if (hitBufferList[i].collider.transform.position.y > transform.position.y)
                    move.y = .1f;
                if (hitBufferList[i].collider.transform.position.y < transform.position.y)
                    move.y = -.1f;
            }
        }

        myRigidbody.position += move.normalized * distance; //Непосредственно само движение

        UpdateLink();
    }

    private void DeleteRectangle()
    {
        OnDestroy?.Invoke();

        Destroy(gameObject);
        RectangleSpawner.Instance.aboveRectangle = false;
    }

    public void CreateLink(Transform hitTransform)
    {
        if(transform.position != hitTransform.position)
        {
            connectedObj = hitTransform;
            UpdateLink();
            connectedObj.GetComponent<RectangleUnit>().OnDestroy += DestroyLink;

            nod.SetActive(false);
        }
        else
        {
            DestroyLink();
        }
    }

    private void UpdateLink()
    {
        if (connectedObj != null)
        {
            line.SetPosition(0, transform.position);
            line.SetPosition(1, connectedObj.position);
        }
    }

    private void DestroyLink()
    {
        line.SetPosition(0, transform.position);
        line.SetPosition(1, transform.position);
        nod.SetActive(true);
    }
}
