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

    private const int maxCollisions = 7;

    private float clickTimer = .0f;

    private Vector3 clickOffset;
    private Vector3 mousePosition;
    private Vector2 newPosition;

    private Transform connectedObj;

    private RaycastHit2D[] hitBuffer = new RaycastHit2D[maxCollisions];
    private List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(maxCollisions);

    public System.Action OnDestroy;

    #region Движение прямоугольников
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
        //Сдвиг курсора относительно центра объекта,
        //чтобы он не двигался когда курсор изначально не стоял на центре прямоугольника
        clickOffset = transform.position - mousePosition;

        if ((Time.time - clickTimer) < clickCooldown) //Ловим двойной клик
        {
            Invoke(nameof(DeleteRectangle), .1f); //Задержка удаления, чтобы пропустить клик и не создать прямоугольник в месте удаления
        }

        clickTimer = Time.time; //Запоминаем время первого клика
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

    private void MoveRectangle(Vector2 move)
    {
        float distance = move.magnitude;

        if (distance > minMoveDistance)
        {
            //Проецирование всех коллайдеров на объекте наперёд и обнаружение коллизий с проекциями
            int count = myRigidbody.Cast(move, hitBuffer, distance + shellRadius);

            hitBufferList.Clear();

            for (int i = 0; i < count; i++)
            {
                if (hitBuffer[i].collider != ignoreCollider) //Игнорируем узел
                    hitBufferList.Add(hitBuffer[i]); //Регистрируем не пустые коллизии
            }

            //Участок для проверки коллизий и последующее отталкивание прямоугольника
            for (int i = 0; i < hitBufferList.Count; i++)
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
    #endregion

    public void ChangeColor()
    {
        //Функция, в котором случайно задаются каналы RGB
        myRenderer.color = new Color32((byte)Random.Range(0, 255),
                                       (byte)Random.Range(0, 255),
                                       (byte)Random.Range(0, 255),
                                       255);

        newPosition = transform.position;
    }

    private void UpdateMousePosition()
    {
        //Обновляем положение курсора в глобальном пространстве
        mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
    }

    private void DeleteRectangle()
    {
        //Удаление прямоугольника

        OnDestroy?.Invoke(); //Подчищаем связи
        
        if(connectedObj != null)
            connectedObj.GetComponent<RectangleUnit>().OnDestroy -= DestroyLink; //Отписываемся от других удалений связей

        Destroy(gameObject);
        RectangleSpawner.Instance.aboveRectangle = false;
    }

    #region Связи между прямоугольниками
    public void CreateLink(Transform hitTransform)
    {
        //Cоздание визуальной связи между прямоугольниками
        if (transform.position != hitTransform.position)
        {
            connectedObj = hitTransform; //Запоминаем положение того с кем связались
            UpdateLink(); //Обновляем положение точек
            connectedObj.GetComponent<RectangleUnit>().OnDestroy += DestroyLink; //Подписываемся на событие удаления того с кем связались

            nod.SetActive(false); //Отключаем узел для связи, тк можем иметь одну связь
        }
        else
        {
            DestroyLink(); //Обнуляем значения, если узел связи упал на нас самих
        }
    }

    private void UpdateLink()
    {
        //Обновление положения точек связи между двумя прямоугольниками в LineRenderer
        //Также вызывается при движении прямоугольника
        if (connectedObj != null)
        {
            line.SetPosition(0, transform.position);
            line.SetPosition(1, connectedObj.position);
        }
    }

    private void DestroyLink()
    {
        //Уничтожение визуальной связи, путем обнуления точек в LineRenderer
        line.SetPosition(0, transform.position);
        line.SetPosition(1, transform.position);
        nod.SetActive(true);
    }
    #endregion
}
