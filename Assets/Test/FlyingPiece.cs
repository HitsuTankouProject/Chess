using UnityEngine;
public class FlyingPiece : MonoBehaviour
{
    private Vector3 velocity;
    private Vector3 rotate;
    private MeshRenderer meshRenderer => gameObject.GetComponent<MeshRenderer>();
    public void Init(Vector3 v, float rotateSpeed , Material material)
    {
        velocity = v;
        rotate = Random.onUnitSphere * rotateSpeed;
        meshRenderer.material = material;

    }

    void Update()
    {
        transform.position += velocity * Time.deltaTime;
        velocity += Vector3.down * 15f * Time.deltaTime;
        transform.Rotate(rotate * Time.deltaTime);
    }



}