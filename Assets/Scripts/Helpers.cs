using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{

    public static Camera mainCamera;

    public static void GetCamera()
    {
        mainCamera = Camera.main;
    }

    public static Vector3 GetMousePos()
    {
        return mainCamera.ScreenToWorldPoint(Input.mousePosition);
    }

    public static void Shuffle(List<CardController> list)
    {
        int count = list.Count;
        int last = count - 1;

        for (int i = 0; i < last; ++i)
        {
            int rnd = UnityEngine.Random.Range(i, count);
            CardController temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }


    public static Vector3 GetXYposition(Vector3 position)
    {
        return new Vector3(position.x, position.y, 0);
    }


    public static float GetDistanceXY(Vector3 positionA, Vector3 positionB)
    {
        positionA = GetXYposition(positionA);
        positionB = GetXYposition(positionB);
        return Vector3.Distance(positionA, positionB);
    }

}
