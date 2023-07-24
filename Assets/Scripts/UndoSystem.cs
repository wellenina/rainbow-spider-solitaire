using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UndoSystem : MonoBehaviour
{
    private GameManager gameManagerScript;
    private List<SavedMove> moves = new List<SavedMove>();


    void Start()
    {
        gameManagerScript = GetComponent<GameManager>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Undo();
        }
    }

    public void SaveMove(CardController movedCard, int oldPileIndex, bool wasFaceUp)
	{
		SavedMove newMove = new SavedMove(movedCard, oldPileIndex, wasFaceUp);
		moves.Add(newMove);
	}


    public void Undo()
	{
		if (moves.Count < 1) { return; }

		SavedMove lastMove = moves.Last();

		if (!lastMove.wasFaceUp)
		{
            gameManagerScript.TurnCardFaceDown(lastMove.oldPileIndex);
		}

		CardController movedCard = lastMove.movedCard;
		List<CardController> removed = gameManagerScript.RemoveFromPile(movedCard, movedCard.pileIndex);
		gameManagerScript.AddToPile(movedCard, lastMove.oldPileIndex, removed);
		moves.RemoveAt(moves.Count - 1);
	}

}
