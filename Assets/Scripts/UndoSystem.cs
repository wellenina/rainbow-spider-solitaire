using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UndoSystem : MonoBehaviour
{
    private GameManager gameManagerScript;
    private GUImanager gui;
    private List<SavedMove> moves = new List<SavedMove>();


    void Start()
    {
        gameManagerScript = GetComponent<GameManager>();
        gui = GetComponent<GUImanager>();
    }

    public void SaveMove(CardController movedCard, int oldPileIndex, bool wasFaceUp)
	{
		SavedMove newMove = new SavedMove(movedCard, oldPileIndex, wasFaceUp);
		moves.Add(newMove);
        gui.IncrementMoves();
	}

    public void SaveMove()
	{
		SavedMove newMove = new SavedMove();
		moves.Add(newMove);
        gui.IncrementMoves();
	}

    public void Undo()
	{
		if (moves.Count < 1) { return; }

        gui.IncrementMoves();
		SavedMove lastMove = moves.Last();

        if (lastMove.dealtCards)
        {
            gameManagerScript.UndoDealingCards(); 
            moves.RemoveAt(moves.Count - 1);
            return;
        }

		if (!lastMove.wasFaceUp)
		{
            gameManagerScript.TurnCardFaceDown(lastMove.oldPileIndex);
		}

		CardController movedCard = lastMove.movedCard;
		List<CardController> removed = gameManagerScript.RemoveFromPile(movedCard, movedCard.pileIndex);
		gameManagerScript.AddToPile(movedCard, lastMove.oldPileIndex, removed);
		moves.RemoveAt(moves.Count - 1);
	}

    public void ClearMoves()
    {
        moves.Clear();
    }

}
