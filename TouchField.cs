using UnityEngine;

public class TouchField : MonoBehaviour
{
    private RectTransform thisRect;
    public Vector2 TouchDist = new Vector2();
    private Vector2 oldPos;
    public bool pressed = false;
    private int pressingFinger = -1;
    //TouchFieldの範囲内のタップのみに有効
    //最初に押したタップが離れるまでTouchFieldは一本の指が占有
    private void Start()
    {
        thisRect = GetComponent<RectTransform>();
    }
    void Update()
    {
        if (Input.touchCount < 1) return;
        if (pressed)
        {
            Touch touch = Input.GetTouch(pressingFinger);
            TouchDist = touch.position - oldPos;
            oldPos = touch.position;
            if (touch.phase == TouchPhase.Ended)
            {
                pressed = false;
                TouchDist = new Vector2();
            }
            return;
        }
        for(int i=0; i<Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase != TouchPhase.Began) continue;
            if (touch.position.y > thisRect.position.y + thisRect.rect.height / 2 || touch.position.y < thisRect.position.y - thisRect.rect.height / 2) continue;
            if (touch.position.x > thisRect.position.x + thisRect.rect.width / 2 || touch.position.y < thisRect.position.y - thisRect.rect.width / 2) continue;
            pressed = true;
            pressingFinger = i;
            oldPos = touch.position;
        }
    }
}
