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

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(!aboveRectangle)
                SpawnRectangle();
        }
    }

    private void SpawnRectangle()
    {
        var v3 = Input.mousePosition;
        v3.z = 10.0f;
        v3 = Camera.main.ScreenToWorldPoint(v3);

        if (Physics2D.OverlapBox(v3, new Vector2(8, 4), 0) == null)
        {

            GameObject obj = Instantiate(rectanglePrefab);

            obj.transform.position = v3;

            obj.GetComponent<RectangleUnit>().ChangeColor();
        }
    }
}
