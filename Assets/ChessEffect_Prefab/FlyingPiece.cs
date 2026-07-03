using UnityEngine;
public class FlyingPiece : MonoBehaviour
{
    private float timer = 0;
    private float life;
    private Vector3 velocity;
    private Vector3 rotate;
    private MeshRenderer meshRenderer => gameObject.GetComponent<MeshRenderer>();
    public void Init(float lifeTime, Vector3 v, float rotateSpeed , Material material)
    {
        life = lifeTime;
        velocity = v;
        rotate = Random.onUnitSphere * rotateSpeed;
        meshRenderer.material = material;

    }

    void Update()
    {
        if (timer < life)
        {
            timer += Time.deltaTime;
            transform.position += velocity * Time.deltaTime;
            velocity += Vector3.down * 15f * Time.deltaTime;
            transform.Rotate(rotate * Time.deltaTime);
        }

    }



}