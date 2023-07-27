using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavedMove
{
	public CardController movedCard;
	public int oldPileIndex;
	public bool wasFaceUp;

	public bool dealtCards;

	public SavedMove(CardController card, int pileIndex, bool faceUp)
	{
		movedCard = card;
		oldPileIndex = pileIndex;
		wasFaceUp = faceUp;
	}

	public SavedMove()
	{
		dealtCards = true;
	}
}
