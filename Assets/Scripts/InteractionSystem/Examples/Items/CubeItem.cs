using UnityEngine;

public class CubeItem : IItem
{
    public override void OnItemUpdate()
    {
        GetComponent<Renderer>().material.color = Color.Lerp(Color.red, Color.blue, Mathf.PingPong(Time.time, 1));
    }
}
