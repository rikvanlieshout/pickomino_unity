//using UnityEngine;
//using System.Collections;

//public class NewMonoBehaviour : MonoBehaviour
//{
//    // Use this for initialization
//    void Start()
//    {

//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }


//    private Vector2 FindNewDicePos()
//    {
//        Vector2 newPos;
//        Collider2D[] neighbours;
//        Rect rollAreaRect = RollArea.GetComponent<RectTransform>().rect;
//        Rect diceRect = DicePrefab.GetComponent<RectTransform>().rect;
//        float minDistance = diceRect.width * Mathf.Sqrt(2);
//        float sigma = diceRect.width * rollSpreadScale;
//        float x;
//        float xMin = rollAreaRect.xMin + minDistance;
//        float xMax = rollAreaRect.xMax - minDistance;
//        float y;
//        float yMin = rollAreaRect.yMin + minDistance;
//        float yMax = rollAreaRect.yMax - minDistance;
//        do
//        {
//            do
//            {
//                x = NextGaussian() * sigma;
//            } while (x < xMin || x > xMax);
//            do
//            {
//                y = NextGaussian() * sigma;
//            } while (y < yMin || y > yMax);
//            newPos = new Vector2(x, y);
//            neighbours = UnityEngine.Physics2D.OverlapCircleAll(newPos, minDistance);
//        } while (neighbours.Length > 0);
//        return newPos;
//    }


//    public static float NextGaussian()
//    {
//        float v1, v2, s;
//        do
//        {
//            v1 = 2.0f * UnityEngine.Random.Range(0f, 1f) - 1.0f;
//            v2 = 2.0f * UnityEngine.Random.Range(0f, 1f) - 1.0f;
//            s = v1 * v1 + v2 * v2;
//        } while (s >= 1.0f || s == 0f);

//        s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);

//        return v1 * s;
//    }
//}
