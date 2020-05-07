﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Pickomino : MonoBehaviour
{
    [SerializeField] GameObject InitializationGroup;
    [SerializeField] GameObject GameGroup;

    [SerializeField] Button AddPlayerButton;
    [SerializeField] Button StartGameButton;
    [SerializeField] Button RollButton;
    [SerializeField] Button StopButton;

    [SerializeField] Text ScoreText;

    [SerializeField] GameObject PlayerZone;
    [SerializeField] GameObject TableArea;
    [SerializeField] GameObject RollArea;
    [SerializeField] GameObject SaveArea;

    [SerializeField] GameObject PlayerAreaPrefab;
    [SerializeField] GameObject ContainerPrefab;

    [SerializeField] GameObject TilePrefab;
    [SerializeField] GameObject DicePrefab;

    [SerializeField] Sprite DiceFaceSprite1;
    [SerializeField] Sprite DiceFaceSprite2;
    [SerializeField] Sprite DiceFaceSprite3;
    [SerializeField] Sprite DiceFaceSprite4;
    [SerializeField] Sprite DiceFaceSprite5;
    [SerializeField] Sprite DiceFaceSpriteW;

    [SerializeField] string tileWormsString1;
    [SerializeField] string tileWormsString2;
    [SerializeField] string tileWormsString3;
    [SerializeField] string tileWormsString4;

    [SerializeField] int nPlayersMax = 5;

    [SerializeField] int nDice = 8;
    [SerializeField] int nFaces = 6;

    [SerializeField] int wormGroupSize = 4;

    [SerializeField] int tileValMin = 21;
    [SerializeField] int tileValMax = 36;

    [SerializeField] float rollSpreadScale = 2/3;

    private List<Sprite> DiceFaceSprites = new List<Sprite>();
    private List<string> tileWormsStrings = new List<string>();

    private List<GameObject> RolledDice = new List<GameObject>();
    private List<GameObject> Tiles = new List<GameObject>();
    private List<GameObject> Containers = new List<GameObject>();
    private List<GameObject> PlayerTileAreas = new List<GameObject>();
    private List<Text> PlayerNameTexts = new List<Text>();

    private List<int> takenFaces = new List<int>();
    private List<int> tableTileVals = new List<int>();
    private List<int> stealableTileVals = new List<int>();
    private List<List<int>> PlayerStacks = new List<List<int>>();

    private int nPlayers = 0;
    private int currentPlayerIndex;
    private int nDiceLeft;
    private int score;
    private int minScore;
    private bool validTurn;
    private bool wormTaken;
    private bool dicePickable;
    private bool tilesPickable;


    public void Start()
    {
        DiceFaceSprites.Add(DiceFaceSprite1);
        DiceFaceSprites.Add(DiceFaceSprite2);
        DiceFaceSprites.Add(DiceFaceSprite3);
        DiceFaceSprites.Add(DiceFaceSprite4);
        DiceFaceSprites.Add(DiceFaceSprite5);
        DiceFaceSprites.Add(DiceFaceSpriteW);

        tileWormsStrings.Add(tileWormsString1);
        tileWormsStrings.Add(tileWormsString2);
        tileWormsStrings.Add(tileWormsString3);
        tileWormsStrings.Add(tileWormsString4);

        // Create containers to keep the tiles in place in the Table Area
        for (int tileVal = tileValMin; tileVal <= tileValMax; tileVal++)
        {
            GameObject NewContainer = Instantiate(ContainerPrefab, new Vector2(0, 0), Quaternion.identity);
            NewContainer.transform.SetParent(TableArea.transform, false);
            Containers.Add(NewContainer);
        }

        GameGroup.SetActive(false);

    }


    public void OnPressAddPlayer()
    {
        nPlayers++;

        StartGameButton.interactable = true;

        if (nPlayers == nPlayersMax)
        {
            AddPlayerButton.interactable = false;
        }

        GameObject NewPlayerArea = Instantiate(PlayerAreaPrefab, new Vector2(0, 0), Quaternion.identity);
        NewPlayerArea.transform.SetParent(PlayerZone.transform, false);

        Text PlayerNameText = NewPlayerArea.GetComponent<PlayerAreaSpecs>().PlayerNameText;
        PlayerNameTexts.Add(PlayerNameText);
        PlayerNameText.color = Color.red;

        InputField PlayerNameInputField = NewPlayerArea.GetComponent<PlayerAreaSpecs>().PlayerNameInputField;
        PlayerNameInputField.Select();
        PlayerNameInputField.onEndEdit.AddListener(delegate { PlayerNameText.color = Color.black; });

        PlayerTileAreas.Add(NewPlayerArea.GetComponent<PlayerAreaSpecs>().PlayerTileArea);
        PlayerStacks.Add(new List<int>());
    }


    public void OnPressStartGame()
    {
        AddPlayerButton.gameObject.SetActive(false);
        StartGameButton.GetComponentInChildren<Text>().text = "Next Game";
        InitializationGroup.SetActive(false);
        GameGroup.SetActive(true);
        StartGame();
    }


    public void StartGame()
    {
        // Clean up from possible previous game
        Tiles.Clear();
        for (int iPlayer = 0; iPlayer < nPlayers; iPlayer++)
        {
            ClearArea(PlayerTileAreas[iPlayer]);
            PlayerStacks[iPlayer].Clear();
            PlayerNameTexts[iPlayer].color = Color.black;
        }

        // Create tiles and place them in the Table Area
        for (int tileVal = tileValMin; tileVal <= tileValMax; tileVal++)
        {
            int tileIndex = tileVal - tileValMin;
            int tileWorms = (tileVal - tileValMin) / wormGroupSize + 1;

            GameObject NewTile = Instantiate(TilePrefab, new Vector2(0, 0), Quaternion.identity);
            NewTile.transform.SetParent(Containers[tileIndex].transform, false);
            NewTile.GetComponent<TileSpecs>().tileVal = tileVal;
            NewTile.GetComponent<TileSpecs>().tileWorms = tileWorms;
            NewTile.GetComponent<TileSpecs>().ValText.text = tileVal.ToString();
            NewTile.GetComponent<TileSpecs>().WormText.text = tileWormsStrings[tileWorms - 1];
            NewTile.GetComponent<Button>().interactable = false;
            NewTile.GetComponent<Button>().onClick.AddListener(() => OnPressTile(tileIndex));
            Tiles.Add(NewTile);

            tableTileVals.Add(tileVal);
        }

        currentPlayerIndex = -1;

        StartTurn();
    }


    public void FinishGame()
    {
        List<int> worms = Enumerable.Repeat(0, nPlayers).ToList();
        List<int> maxTileVals = new List<int>(nPlayers);

        // add up worms on tiles collected by players
        for (int iPlayer = 0; iPlayer < nPlayers; iPlayer++)
        {
            PlayerTileAreas[iPlayer].GetComponent<HorizontalLayoutGroup>().spacing = 5;
            foreach (int tileVal in PlayerStacks[iPlayer])
            {
                worms[iPlayer] += Tiles[tileVal - tileValMin].GetComponent<TileSpecs>().tileWorms;
            }
            maxTileVals.Add(PlayerStacks[iPlayer].DefaultIfEmpty().Max());
        }

        List<int> wormMaxers = Enumerable.Range(0, worms.Count)
            .Where(iPlayer => worms[iPlayer] == worms.Max())
            .ToList();

        // find winner
        int winnerIndex;
        if (wormMaxers.Count == 1)
        {
            winnerIndex = wormMaxers[0];
        }
        else
        {
            List<int> tmpList = new List<int>();
            wormMaxers.ForEach(iPlayer => tmpList.Add(maxTileVals[iPlayer]));
            winnerIndex = wormMaxers
                .Where(i => maxTileVals[i] == tmpList.Max())
                .ToList()[0];
        }

        PlayerNameTexts[winnerIndex].color = Color.yellow;

        GameGroup.SetActive(false);
        InitializationGroup.SetActive(true);
    }


    public void StartTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % nPlayers;
        PlayerNameTexts[currentPlayerIndex].color = Color.red;
        nDiceLeft = nDice;
        validTurn = true;
        wormTaken = false;
        score = 0;
        minScore = tableTileVals.Min();
        UpdateStealableTiles();
        RollButton.interactable = true;
    }


    private void FinishTurn()
    {
        ClearArea(RollArea);
        ClearArea(SaveArea);
        RolledDice.Clear();
        takenFaces.Clear();
        foreach (GameObject Tile in Tiles)
        {
            Tile.GetComponent<Button>().interactable = false;
        }
        ScoreText.text = "";
        PlayerNameTexts[currentPlayerIndex].color = Color.black;
        if (tableTileVals.Any())
        {
            StartTurn();
        }
        else
        {
            FinishGame();
        }
    }


    private void ClearArea(GameObject Area)
    {
        foreach (Transform child in Area.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }


    private void UpdateScoreText(int score)
    {
        ScoreText.text = String.Format("Score: {0,2}", score);
    }


    public void OnPressRoll()
    {
        RollButton.interactable = false;
        StopButton.interactable = false;
        foreach (GameObject Tile in Tiles)
        {
            Tile.GetComponent<Button>().interactable = false;
        }
        ClearArea(RollArea);
        RollDice(nDiceLeft);
        FindPickableDice();
        if (dicePickable == false)
        {
            validTurn = false;
            StopButton.interactable = true;
        }
    }


    public void OnPressStop()
    {
        StopButton.interactable = false;
        RollButton.interactable = false;
        if (!validTurn || !wormTaken || !tilesPickable)
        {
            ReturnTile();
            FinishTurn();
        }
    }


    public void RollDice(int nDiceToRoll)
    {
        int face;
        List<int> rolledFaces = new List<int>();

        for (int i = 0; i < nDiceToRoll; i++)
        {
            face = UnityEngine.Random.Range(1, nFaces + 1);
            rolledFaces.Add(face);
        }
        rolledFaces.Sort();
        for (int i = 0; i < nDiceToRoll; i++)
        {
            //face = UnityEngine.Random.Range(1, nFaces + 1);
            face = rolledFaces[i];
            GameObject NewDice = Instantiate(DicePrefab, new Vector2(0, 0), Quaternion.identity);
            //GameObject NewDice = Instantiate(DicePrefab, FindNewDicePos(), Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f)));
            NewDice.transform.SetParent(RollArea.transform, false);
            NewDice.GetComponent<DiceSpecs>().diceFace = face;
            NewDice.GetComponent<Image>().sprite = DiceFaceSprites[face - 1];
            RolledDice.Add(NewDice);
        }
    }


    private void FindPickableDice()
    {
        dicePickable = false;
        for (int i = 0; i < nDiceLeft; i++)
        {
            int face = RolledDice[i].GetComponent<DiceSpecs>().diceFace;
            if (!takenFaces.Contains(face))
            {
                dicePickable = true;
                RolledDice[i].GetComponent<Button>().interactable = true;
                RolledDice[i].GetComponent<Button>().onClick.AddListener(() => OnPressDice(face));
            }
        }
    }


    private void OnPressDice(int pickedFace)
    {
        takenFaces.Add(pickedFace);

        if (pickedFace == nFaces)
        {
            wormTaken = true;
        }

        int nDiceTaken = TakeDice(pickedFace);

        nDiceLeft -= nDiceTaken;

        score += nDiceTaken * Math.Min(pickedFace, nFaces - 1);
        UpdateScoreText(score);

        RolledDice.Clear();

        StopButton.interactable = true;
        if (nDiceLeft > 0)
        {
            RollButton.interactable = true;
        }

        if (wormTaken)
        {
            FindPickableTiles();
        }
        
    }


    private int TakeDice(int pickedFace)
    {
        int face;
        int nDiceTaken = 0;

        for (int i = 0; i < nDiceLeft; i++)
        {
            face = RolledDice[i].GetComponent<DiceSpecs>().diceFace;
            if (face == pickedFace)
            {
                RolledDice[i].transform.SetParent(SaveArea.transform, false);
                RolledDice[i].transform.rotation = Quaternion.identity;
                Destroy(RolledDice[i].GetComponent<Button>());
                nDiceTaken++;
            }
            else
            {
                RolledDice[i].GetComponent<Button>().interactable = false;
            }
        }

        return nDiceTaken;
    }


    private void FindPickableTiles()
    {
        int tileIndex = score - tileValMin;
        tilesPickable = false;

        if (tableTileVals.Contains(score))
        {
            Tiles[tileIndex].GetComponent<Button>().interactable = true;
            tilesPickable = true;
        }
        else
        {
            if (stealableTileVals.Contains(score))
            {
                Tiles[tileIndex].GetComponent<Button>().interactable = true;
                tilesPickable = true;
            }

            if (score >= minScore)
            {
                int tileVal = tableTileVals.Where(tmp => tmp < score).Max();
                Tiles[tileVal - tileValMin].GetComponent<Button>().interactable = true;
                tilesPickable = true;
            }
        }
    }


    private void OnPressTile(int tileIndex)
    {
        int tileVal = Tiles[tileIndex].GetComponent<TileSpecs>().tileVal;

        Tiles[tileIndex].GetComponent<Button>().interactable = false;
        StopButton.interactable = false;
        RollButton.interactable = false;

        Tiles[tileIndex].transform.SetParent(PlayerTileAreas[currentPlayerIndex].transform, false);

        tableTileVals.Remove(tileVal);
        for (int iPlayer = 0; iPlayer < nPlayers; iPlayer++)
        {
            PlayerStacks[iPlayer].Remove(tileVal);
        }
        PlayerStacks[currentPlayerIndex].Add(tileVal);

        FinishTurn();
    }


    private void ReturnTile()
    {
        int topTileVal = PlayerStacks[currentPlayerIndex].LastOrDefault();
        int topTileIndex = topTileVal - tileValMin;

        if (topTileVal != 0)
        {
            PlayerStacks[currentPlayerIndex].Remove(topTileVal);
            tableTileVals.Add(topTileVal);
            Tiles[topTileIndex].transform.SetParent(Containers[topTileIndex].transform, false);

            int maxTableTileVal = tableTileVals.Max();
            if (topTileVal != maxTableTileVal)
            {
                tableTileVals.Remove(maxTableTileVal);
                Tiles[maxTableTileVal - tileValMin].SetActive(false);
            }
        }
    }


    private void UpdateStealableTiles()
    {
        stealableTileVals.Clear();

        for (int iPlayer = 0; iPlayer < nPlayers; iPlayer++)
        {
            if (iPlayer == currentPlayerIndex)
            {
                continue;
            }

            int topTileVal = PlayerStacks[iPlayer].LastOrDefault();
            if (topTileVal != 0)
            {
                stealableTileVals.Add(topTileVal);
            }
        }
    }


}
