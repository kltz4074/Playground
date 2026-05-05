using UnityEngine;
using UnityEngine.UI;

public class InteractionSystemCursor : MonoBehaviour
{
    public Image cursor;
    public void ChangeCursorColor(Color color)
    {
        if (cursor != null)
        {
            cursor.color = color;
        }
    }
}
