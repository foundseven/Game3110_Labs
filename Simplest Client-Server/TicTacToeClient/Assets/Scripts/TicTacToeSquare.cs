using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Networking.Transport;

public class TicTacToeSquare : MonoBehaviour
{
    public int row, column, ID;

    public bool diagonal1, diagonal2, isSquareTaken;
    public string icon;
    private const int maxColumns = 3;

    public delegate void SquarePressedDelegate(TicTacToeSquare squarePressed);
    public event SquarePressedDelegate OnSquarePressed;
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnPressed);
        ID = (row * maxColumns) + column;
        diagonal1 = row == column;
        diagonal2 = (row + column) == maxColumns - 1;
    }

    void OnPressed()
    {
        if (!isSquareTaken)
            OnSquarePressed.Invoke(this);
    }

    //set square as taken, assign it an X or an O
    public void ClaimSquare(string icon)
    {
        this.icon = icon;
        isSquareTaken = true;
        GetComponentInChildren<TextMeshProUGUI>().text = icon;
    }


}
