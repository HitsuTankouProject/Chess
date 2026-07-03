using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

public class aaa : MonoBehaviour
{
    public Material material;
    public float speed = 5f;
    public float rotateSpeed = 360f;
    public float lifeTime = 0.8f;

    private List<Vector3> allPiecesLocalPosition = new();
    private List<FlyingPiece> allPieces = new();

    private void Awake()
    {
        foreach (Transform piece in transform)
        {
            allPiecesLocalPosition.Add(piece.localPosition);
            allPieces.Add(piece.gameObject.AddComponent<FlyingPiece>());
        }
        gameObject.SetActive(false);

    }

    private IEnumerator TurnOffEffect()
    {
        yield return new WaitForSeconds(lifeTime * 1.1f);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).localPosition = allPiecesLocalPosition[i];
        }
    }
    public void PlayEffect(ChessColor color)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Vector3 dir = Random.onUnitSphere;
            allPieces[i].Init(lifeTime, dir * speed, rotateSpeed, material);
        }
        //StartCoroutine(TurnOffEffect());
    }

    private void Start()
    {
        PlayEffect(ChessColor.White);
    }


}
