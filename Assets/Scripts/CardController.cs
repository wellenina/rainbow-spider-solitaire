using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardController : MonoBehaviour
{
    [SerializeField] public int VALUE;
    [SerializeField] public string SUIT;
    public int pileIndex = -1;

    public bool isFaceUp = false;
    public bool canBeMoved = false;
    
    private float speed = 40.0f; // number of units the card moves x second in tween

    private Collider cardCollider;
    private Material cardMaterial;
    [SerializeField] GameObject gameManager;
    private GameManager gameManagerScript;
    private UndoSystem undo;

    private Vector3 originalPosition;
    private Vector3 offset;
    private Color shaded = new Color(215.0f/255.0f, 215.0f/255.0f, 215.0f/255.0f, 1.0f);


    void Start()
    {
        cardCollider = GetComponent<Collider>();
        cardMaterial = GetComponent<MeshRenderer>().material;
        gameManagerScript = gameManager.GetComponent<GameManager>();
        undo = gameManager.GetComponent<UndoSystem>();
    }


    public void Move(Vector3 targetPosition)
    {
        transform.DOMove(targetPosition, speed).SetEase(Ease.OutSine).SetSpeedBased(true);
    }


    public void TurnFaceUp(bool faceUp = true)
    {
        float y = faceUp ? 180.0f : 0;
        transform.DORotate(new Vector3(0, y, 0), 0.7f);
        isFaceUp = faceUp;
        MakeAvailable(faceUp);
    }


    public void MakeAvailable(bool isAvailable)
    {
        canBeMoved = isAvailable;
        cardCollider.enabled = isAvailable;
        if (isAvailable)
        {
            cardMaterial.color = Color.white;
        }
        else
        {
            cardMaterial.color = shaded;
        }
        
        GameObject parent = transform.parent.gameObject;
        if (parent.name != "Cards") 
        {
            parent.GetComponent<CardController>().MakeAvailable(isAvailable);
        }
    }


    void OnMouseDown()
    {
        originalPosition = transform.position;
        offset = Helpers.GetMousePos() - new Vector3(originalPosition.x, originalPosition.y, -9.0f);
        // source.PlayOneShot(pickUpClip);
    }


    void OnMouseDrag()
    {
        Vector3 mousePosition = Helpers.GetMousePos();
        transform.position = mousePosition - offset;
    }


    void OnMouseUp()
    {
        float movement = Helpers.GetDistanceXY(originalPosition, transform.position);
        if (movement < 0.05f)
        {
            gameManagerScript.AutoPlace(this);
            return;
        }

        int destinationPileIndex = gameManagerScript.GetCloserPile(transform.position, pileIndex);
        bool isMoveLegal = gameManagerScript.CheckLegalMove(VALUE, destinationPileIndex);

        if (!isMoveLegal)
        {
            MoveBack();
            // source.PlayOneShot(dropClip);
            return;
        }

        bool wasFaceUp = gameManagerScript.getLastCardStatus(pileIndex);
        undo.SaveMove(this, pileIndex, wasFaceUp);
        // source.PlayOneShot(dropClip);
        List<CardController> removed = gameManagerScript.RemoveFromPile(this, pileIndex);
        gameManagerScript.AddToPile(this, destinationPileIndex, removed);
    }


    public void MoveBack()
    {
        transform.position = originalPosition;
    }

}
