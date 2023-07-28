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
    
    private float speed = 32.0f; // number of units the card moves x second in tween

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


    public void Move(Vector3 targetPosition, bool singleCard = false)
    {
        if (!InputManager.areMultipleCardsMoving) { InputManager.DisableInput(true); }
        transform.position = Helpers.GetXYposition(transform.position);
        Vector3 temporaryPosition = Helpers.GetXYposition(targetPosition);
        transform.DOMove(temporaryPosition, speed).SetEase(Ease.OutSine).SetSpeedBased(true).OnComplete(() => MoveToCorrectZ(targetPosition));
    }

    void MoveToCorrectZ(Vector3 targetPosition)
    {
        transform.position = targetPosition;
        if (!InputManager.areMultipleCardsMoving)
        {
            InputManager.DisableInput(false);
        }
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
        if (InputManager.inputDisabled) { return; }
        originalPosition = transform.position;
        offset = Helpers.GetMousePos() - Helpers.GetXYposition(originalPosition);
        // source.PlayOneShot(pickUpClip);
    }


    void OnMouseDrag()
    {
        if (InputManager.inputDisabled) { return; }
        Vector3 mousePosition = Helpers.GetMousePos();
        transform.position = mousePosition - offset;
    }


    void OnMouseUp()
    {
        if (InputManager.inputDisabled) { return; }

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

        bool wasFaceUp = gameManagerScript.IsCardAboveFaceUp(pileIndex, this);
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
