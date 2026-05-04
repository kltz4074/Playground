using UnityEngine;
using UnityEngine.UI;

public class InteractionSystemCursor : MonoBehaviour
{
    public Image cursor;
    public void ChangeCursorColor(Color color)
    {
        cursor.color = color;
    }
}
