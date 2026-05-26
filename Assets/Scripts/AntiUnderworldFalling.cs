using UnityEngine;

public class AntiUnderworldFalling : MonoBehaviour
{
    [SerializeField] public GameObject targetObject;
    [SerializeField] public float respawnHeight = -10f;
    [SerializeField] private bool UseOwnRespawnPosition = false;

    [ShowIf(nameof(UseOwnRespawnPosition))]
    [SerializeField] public Vector3 respawnPosition = new Vector3(0, 100, 0);

    private Vector3 targetRespawnPos;

    public void Start()
    {
        if (UseOwnRespawnPosition)
            targetRespawnPos = respawnPosition;
        else
            targetRespawnPos = transform.position;
    }

    public void Update()
    {
        if (targetObject.transform.position.y < respawnHeight)
        {
            RespawnObject();
        }
    }

    private void RespawnObject()
    {
        targetObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        targetObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        targetObject.transform.position = targetRespawnPos;
    }
}
