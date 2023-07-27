using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalonManager : MonoBehaviour
{
    [SerializeField] GameObject gameManager;
    private GameManager gameManagerScript;
    private Collider talonCollider;

    void Start()
    {
        gameManagerScript = gameManager.GetComponent<GameManager>();
        talonCollider = GetComponent<Collider>();
    }

    void OnMouseDown()
    {
        IEnumerator coroutine = gameManagerScript.DealCards();
        StartCoroutine(coroutine);
    }

    public void Enable(bool isEnabled)
    {
        talonCollider.enabled = isEnabled;
    }
}