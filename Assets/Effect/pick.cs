using UnityEngine;

public class pick : MonoBehaviour
{

    public MeshRenderer a;
    public bool isPick;


public float floatHeight = 0.2f;
public float floatSpeed = 2f;
public float rotateSpeed = 90f;

private Vector3 startPos;

private void Start()
{
    startPos = transform.localPosition;
}

private void Update()
{
    if(!isPick)return;
    // 上下浮動
    /*transform.localPosition =
        startPos + Vector3.up * Mathf.Sin(Time.time * floatSpeed) * floatHeight;*/

    // 繞 Y 軸旋轉
    transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
}
}
