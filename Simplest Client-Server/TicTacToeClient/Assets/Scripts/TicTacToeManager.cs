using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TicTacToeManager : MonoBehaviour
{
    public GameObject squarePrefab;
    public Transform gridParent;
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI player2Text;

    private NetworkClient networkClient;

    [SerializeField]
    List<TicTacToeSquare> ticTacToeSquares = new List<TicTacToeSquare>();

    [SerializeField]
    private string currentPlayerIcon;

    [SerializeField]
    private string player1Icon = "X";
    [SerializeField]
    private string player2Icon = "O";

    private bool isPlayerTurn = true;
    private bool isPlayer1Turn = true;
    // Start is called before the first frame update
    void Start()
    {
        networkClient = FindObjectOfType<NetworkClient>();
        InitializeBoard();

        bool isPlayer1 = networkClient.IsPlayer1; 
        AssignPlayers(isPlayer1);

        if (isPlayer1)
        {
            currentPlayerIcon = player1Icon; // Player 1 starts
            isPlayerTurn = true;
        }
        else
        {
            currentPlayerIcon = player2Icon; // Player 2 waits
            //isPlayerTurn = false; // Set Player 2's turn to false initially
        }
    }

    void InitializeBoard()
    {
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                GameObject square = Instantiate(squarePrefab, gridParent);
                TicTacToeSquare squareScript = square.GetComponent<TicTacToeSquare>();
                squareScript.row = row;
                squareScript.column = col;
               
                squareScript.OnSquarePressed += HandleSquarePressed;

                ticTacToeSquares.Add(squareScript);
            }
        }
    }

    void HandleSquarePressed(TicTacToeSquare squarePressed)
    {
        if (!isPlayerTurn || squarePressed.isSquareTaken)
            return;

        squarePressed.ClaimSquare(currentPlayerIcon);
        //isPlayerTurn = false;

        // Send move to the server
        string msg = $"{ClientServerSignifiers.MakeMove},{squarePressed.row},{squarePressed.column}";
        networkClient.SendMessageToServer(msg);

        CheckGameState();
    }

    public void OnOpponentMove(int row, int col)
    {
        TicTacToeSquare square = ticTacToeSquares.Find(s => s.row == row && s.column == col);
        if (square != null)
        {
            square.ClaimSquare(currentPlayerIcon == "X" ? "O" : "X");
            isPlayerTurn = true;

            CheckGameState();
        }
    }

    void CheckGameState()
    {
        if (CheckWinCondition())
        {
            Debug.Log($"{currentPlayerIcon} wins!");
            EndGame();
        }
        else if (CheckDrawCondition())
        {
            Debug.Log("It's a draw!");
            EndGame();
        }
        else
        {
            SwitchTurn();
        }
    }

    public void AssignPlayers(bool isPlayer1)
    {
        if (isPlayer1)
        {
            playerNameText.text = "You are Player 1";
            player2Text.text = "Opponent is Player 2";
        }
        else
        {
            playerNameText.text = "You are Player 2";
            player2Text.text = "Opponent is Player 1";
        }
    }
    void SwitchTurn()
    {
        Debug.Log("Switching ...");
        //currentPlayerIcon = currentPlayerIcon == "X" ? "O" : "X";

        // Switch the player turn
        isPlayer1Turn = !isPlayer1Turn;
        currentPlayerIcon = isPlayer1Turn ? player1Icon : player2Icon;

        // Update UI to reflect whose turn it is
        if (isPlayer1Turn)
        {
            playerNameText.text = "Player 1's Turn";
            player2Text.text = "Player 2 is Waiting";
        }
        else
        {
            playerNameText.text = "Player 2's Turn";
            player2Text.text = "Player 1 is Waiting";
        }
    }

    bool CheckWinCondition()
    {
        // Check rows, columns, and diagonals
        for (int i = 0; i < 3; i++)
        {
            if (CheckLine(i, 0, 0, 1) || CheckLine(0, i, 1, 0)) return true;
        }
        return CheckLine(0, 0, 1, 1) || CheckLine(0, 2, 1, -1);
    }

    bool CheckLine(int startRow, int startCol, int rowDir, int colDir)
    {
        string firstIcon = ticTacToeSquares.Find(s => s.row == startRow && s.column == startCol)?.icon;
        if (string.IsNullOrEmpty(firstIcon)) return false;

        for (int i = 1; i < 3; i++)
        {
            TicTacToeSquare square = ticTacToeSquares.Find(s => s.row == startRow + i * rowDir && s.column == startCol + i * colDir);
            if (square == null || square.icon != firstIcon) return false;
        }
        return true;
    }

    bool CheckDrawCondition()
    {
        foreach (TicTacToeSquare square in ticTacToeSquares)
        {
            if (!square.isSquareTaken) return false;
        }
        return true;
    }

    void EndGame()
    {
        foreach (TicTacToeSquare square in ticTacToeSquares)
        {
            square.GetComponent<Button>().interactable = false;
        }
    }
}

