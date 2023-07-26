using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    private GameObject[] cards = new GameObject[104];
    private List<CardController> deck = new List<CardController>();
    private List<List<CardController>> piles = new List<List<CardController>>();
    [SerializeField] private float startDelay = 0.8f;

    [SerializeField] private GameObject[] slots;
    [SerializeField] private GameObject cardsParent;
    [SerializeField] private GameObject talon;
    private UndoSystem undo;
    private TalonManager talonScript;

    // TO POSITION THE CARDS:
    [SerializeField] private float FIRST_CARD_X = -7f;
    [SerializeField] private float FIRST_CARD_Y = 4.4f;
    [SerializeField] private float FIRST_CARD_Z = 5.0f;
    [SerializeField] private float X_GAP = 1.35f;
    [SerializeField] private float Y_GAP = -0.3f;
    [SerializeField] private float Z_GAP = -0.2f;

    [SerializeField] private float minDistance = 1.5f;
    private Vector3 completeSequencesPos = new Vector3(6.6f, 3.1f, 25.0f);
    private int completeSequences;


    void StartNewGame()
    {
        SceneManager.LoadScene(0);
    }
    
    
    void Start()
    {
        cards = GameObject.FindGameObjectsWithTag("Card");
        foreach (GameObject card in cards)
        {
            deck.Add(card.GetComponent<CardController>());
        }
        completeSequences = 0;
        undo = GetComponent<UndoSystem>();
        talonScript = talon.GetComponent<TalonManager>();
        Helpers.GetCamera();

        Invoke("StartGame", startDelay);
    }


    void StartGame()
    {
        Helpers.Shuffle(deck);
        PreparePiles();
        StartCoroutine("PositionPiles");
    }


    void PreparePiles()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            piles.Add(new List<CardController>());

            int cardsToAdd = i < 4 ? 6 : 5;
            piles[i].AddRange(deck.GetRange(0, cardsToAdd));
            deck.RemoveRange(0, cardsToAdd);
        }
    }


    IEnumerator PositionPiles()
    {
        // for each row of cards
        for (int cardIndex = 0; cardIndex < piles[0].Count; cardIndex++)
        {
            float yPos = FIRST_CARD_Y + (Y_GAP * cardIndex);
            float zPos = FIRST_CARD_Z + (Z_GAP * cardIndex);

            // for each pile
            for (int pileIndex = 0; pileIndex < piles.Count; pileIndex++)
            {
                if (cardIndex > piles[pileIndex].Count - 1) { continue; } 
                CardController card = piles[pileIndex][cardIndex];
                float xPos = FIRST_CARD_X + (X_GAP * pileIndex);
                Vector3 position = new Vector3(xPos, yPos, zPos);

                card.pileIndex =  pileIndex;
                card.Move(position);
                yield return new WaitForSeconds(0.06f);
            }
        }
        yield return new WaitForSeconds(0.5f);

        // turn top cards:
        foreach (List<CardController> pile in piles)
        {
            CardController topCard = pile.Last();
            topCard.TurnFaceUp();
            yield return new WaitForSeconds(0.2f);
        }
    }


    public int GetCloserPile(Vector3 cardPosition, int originPileIndex)
    {
        List<float> distances = new List<float>();

        for (int i = 0; i < piles.Count; i++)
        {
            if (piles[i].Count == 0)
            {
                float distance = Helpers.GetDistanceXY(cardPosition, slots[i].transform.position);
                distances.Add(distance);
            }
            else if (originPileIndex == i)
            {
                distances.Add(minDistance);
            }
            else
            {
                CardController topCard = piles[i].Last();
                float distance = Helpers.GetDistanceXY(cardPosition, topCard.gameObject.transform.position);
                distances.Add(distance);
            }
        }
        
        float smallestDistance = distances.Min();
        if (smallestDistance >= minDistance) { return -1; }
        return distances.IndexOf(smallestDistance);
    }


    public bool CheckLegalMove(int droppedCardValue, int destinationPileIndex)
    {
        if (destinationPileIndex < 0) { return false; }
        if (piles[destinationPileIndex].Count == 0) { return true; }
        int topCardValue = piles[destinationPileIndex].Last().VALUE;
        return topCardValue == droppedCardValue + 1;
    }

    public List<CardController> RemoveFromPile(CardController card, int originPileIndex)
    { 
        // se la carta spostata aveva genitori:
        card.gameObject.transform.SetParent(cardsParent.transform);

        List<CardController> originPile = piles[originPileIndex];
        List<CardController> removed = new List<CardController>();
        int index = originPile.IndexOf(card);
        int count = originPile.Count - index;

        if (count <= 1)
        {
            removed = null;
            originPile.Remove(card);
        }
        else
        {
            removed = originPile.GetRange(index, count);
            originPile.RemoveRange(index, count);
        }

        if (originPile.Count > 0)  // LA LISTA RIMASTA NON è VUOTA
        {
            HandleNewTopCard(originPile);
        }

        return removed;
    }

    void HandleNewTopCard(List<CardController> pile)
    {
        CardController newTopCard = pile.Last();
        if (!newTopCard.isFaceUp)
        {
            newTopCard.TurnFaceUp();
        }
        else if (!newTopCard.canBeMoved)
        {
            newTopCard.MakeAvailable(true);
        }
    }

    public void AddToPile(CardController card, int destinationPileIndex, List<CardController> removed)
    {
        List<CardController> destinationPile = piles[destinationPileIndex];
        Vector3 newPosition;

        if (destinationPile.Count == 0) // if the pile is empty
        {
            // position the card in the empty slot:
            newPosition = slots[destinationPileIndex].transform.position;
        }
        else
        {
            CardController topCard = piles[destinationPileIndex].Last();
            // position the card on the last card of the pile:
            newPosition = topCard.gameObject.transform.position + new Vector3(0, Y_GAP, Z_GAP);
            HandleCardAbove(topCard, card);
        }

        card.Move(newPosition);

        if (removed == null)
        {
            destinationPile.Add(card);
            card.pileIndex = destinationPileIndex;
        }
        else
        {
            destinationPile.AddRange(removed);
            foreach (CardController rCard in removed)
            {
                rCard.pileIndex = destinationPileIndex;
            }
        }

        // check se la sequenza è completa:
        if (CheckCompleteSequence(destinationPile))
        {
            IEnumerator coroutine = CompleteSequence(destinationPile);
            StartCoroutine(coroutine);
        }
    }

    void HandleCardAbove(CardController lastCard, CardController movedCard)
    {
            if (!lastCard.isFaceUp) { return; }

            if (movedCard.SUIT == lastCard.SUIT && movedCard.VALUE + 1 == lastCard.VALUE)
            {
                movedCard.gameObject.transform.SetParent(lastCard.gameObject.transform);
                return;
            }
            lastCard.MakeAvailable(false);
    }


    public void AutoPlace(CardController card)
    {
        int destinationPile = GetBestMove(card);

        if (destinationPile < 0)
        {
            card.MoveBack();
            return;
        }

        int pileIndex = card.pileIndex;
        bool wasFaceUp = IsCardAboveFaceUp(pileIndex, card);
        undo.SaveMove(card, pileIndex, wasFaceUp);

        List<CardController> removed = RemoveFromPile(card, pileIndex);
        AddToPile(card, destinationPile, removed);
    }

    int GetBestMove(CardController card)
    {
        for (int i = 0; i < piles.Count; i++)
        {
            if (card.pileIndex == i) { continue; } // ignore origin pile
            if (piles[i].Count < 1) { continue; }
            if (piles[i].Last().SUIT != card.SUIT) { continue; }
            if (CheckLegalMove(card.VALUE, i))
            {
                return i;
            }
        }

        for (int i = 0; i < piles.Count; i++)
        {
            if (card.pileIndex == i) { continue; } // ignore origin pile
            if (piles[i].Count < 1) { continue; }
            if (CheckLegalMove(card.VALUE, i))
            {
                return i;
            }
        }

        for (int i = 0; i < piles.Count; i++)
        {
            if (piles[i].Count == 0)
            {
                return i;
            }
        }

        return -1;
    }


    bool CheckCompleteSequence(List<CardController> pile)
    {
        if (pile.Count < 13) { return false; }
        int cardIndex = pile.Count - 1;
        int value = 1;
        string suit = pile[cardIndex].SUIT;

        while (value <= 13 && cardIndex >= 0)
        {
            if (!pile[cardIndex].isFaceUp) { return false; }
            if (pile[cardIndex].VALUE != value) { return false; }
            if (pile[cardIndex].SUIT != suit) { return false; }
            value++;
            cardIndex--;
        }

        return true;
    }

    IEnumerator CompleteSequence(List<CardController> pile)
    {
        yield return new WaitForSeconds(0.3f);
        for (int i = 1; i <= 13; i++)
        {
            CardController card = pile[pile.Count-i];
            card.gameObject.transform.SetParent(cardsParent.transform);
            float yOffset = Y_GAP*completeSequences;
            float zOffset = (0.2f*i) - (completeSequences*13);
            card.Move(completeSequencesPos + new Vector3(0, yOffset, zOffset));
            yield return new WaitForSeconds(0.16f);
        }
        pile.RemoveRange(pile.Count - 13, 13);
        completeSequences++;

        if (completeSequences == 8)
        {
            // YOU WON! congrats!
        }

        if (pile.Count > 0)
        {
            HandleNewTopCard(pile);
        }
    }


    public void DealFromTalon()
    {
        StartCoroutine("DealCards");
    }

    IEnumerator DealCards()
    {
        for (int i = 0; i < piles.Count; i++)
        {
            CardController card = deck[i];
            AddToPile(card, i, null);
            yield return new WaitForSeconds(0.1f);
            card.TurnFaceUp();
        }
        deck.RemoveRange(0, piles.Count);
        if (deck.Count == 0)
        {
            talonScript.Empty();
        }
        yield return null;
    }

    public bool IsCardAboveFaceUp(int pileIndex, CardController card)
    {
        int cardIndex = piles[card.pileIndex].IndexOf(card);

        if (cardIndex < 1)
        {
            return true;
        }
        CardController newTopCard = piles[pileIndex][cardIndex-1];
        return newTopCard.isFaceUp;
    }

    public void TurnCardFaceDown(int pileIndex)
    {
        CardController topCard = piles[pileIndex].Last();
		topCard.TurnFaceUp(false);
    }

}
