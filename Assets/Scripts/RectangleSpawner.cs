using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectangleSpawner : MonoBehaviour
{
    private static RectangleSpawner instance;
    public static RectangleSpawner Instance
    {
        get => instance;

        set
        {
            //Синглтон шаблон
            if(instance == null)
            {
                instance = value;
            }
            else
            {
                Destroy(value.gameObject);
            }
        }
    }

    public bool aboveRectangle = false;

    [SerializeField] private GameObject rectanglePrefab;

    private const float distanceInFrontOfCamera = 10.0f;
    private const float width = 8f;
    private const float height = 4f;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        //Создание прямоугольника при левом клике, флаг исключает проверки при клике на прямоугольник
        if (!aboveRectangle && Input.GetMouseButtonDown(0))
        {
            SpawnRectangle();
        }
    }

    private void SpawnRectangle()
    {
        var v3 = Input.mousePosition;
        v3.z = distanceInFrontOfCamera;
        v3 = Camera.main.ScreenToWorldPoint(v3);

        if (Physics2D.OverlapBox(v3, new Vector2(width, height), 0) == null) //Проверяем наличие прямоугольников вокруг клика мышкой
        {
            GameObject obj = Instantiate(rectanglePrefab); //Создаем прямоугольник

            obj.transform.position = v3; //Распологаем под мышкой

            obj.GetComponent<RectangleUnit>().ChangeColor(); //Задаем случайный цвет
        }
    }
}
