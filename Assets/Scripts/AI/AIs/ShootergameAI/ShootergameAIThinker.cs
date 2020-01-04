using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ShootergameAIThinker : IThinker
{
    private int lastCol = -1;

    private List<int> col = new List<int>();

    private bool hasWinCorridors;

    private System.Random random;

    private List<Pos> enemyPiece = new List<Pos>();
    private List<Pos> myPiece = new List<Pos>();

    public FutureMove Think(Board board, CancellationToken ct)
    {
        // The move to perform
        FutureMove move;
        FutureMove? test;

        test = Play(board);
        if (test != null)
        {
            Debug.LogWarning("WON VIA SCRIPT");
            return (FutureMove)test;
        }
        /*for (int x = 0; x < board.cols; x++)
        {
            test = CheckColsShape(board, x);
        }
        if (test != null)
        {
            Debug.LogWarning("WON VIA SCRIPT");
            return (FutureMove)test;
        }*/

        random = new System.Random();

        // Check how many pieces current player has
        int roundPieces = board.PieceCount(board.Turn, PShape.Round);
        int squarePieces = board.PieceCount(board.Turn, PShape.Square);
        PShape shape = squarePieces < roundPieces ? PShape.Round : PShape.Square;

        foreach (IEnumerable winCorridors in board.winCorridors)
        {
            foreach (Pos pos in winCorridors)
            {
                col.Add(pos.col);
                hasWinCorridors = true;
            }
        }

        if (!hasWinCorridors)
        {
            // Find next free column where to play
            do
            {
                // Get next column
                lastCol++;
                if (lastCol >= board.cols) lastCol = 0;
                // Is this task to be cancelled?
                if (ct.IsCancellationRequested) return FutureMove.NoMove;
            }
            while (board.IsColumnFull(lastCol));
        }

        if (hasWinCorridors)
        {

            if (board.PieceCount(board.Turn, PShape.Square) > 0)
            {
                move = new FutureMove(col[random.Next(0, col.Count)], shape);
            }
            // If there's no round pieces left, let's try a square pieces
            else if (board.PieceCount(board.Turn, PShape.Round) > 0)
            {
                move = new FutureMove(col[random.Next(0, col.Count)], shape);
            }
            // Otherwise return a "no move" (this should never happen)
            else
            {
                move = FutureMove.NoMove;
            }
        }
        else
        {
            // Try to use a round piece first
            if (board.PieceCount(board.Turn, PShape.Square) > 0)
            {
                move = new FutureMove(lastCol, shape);
            }
            // If there's no round pieces left, let's try a square pieces
            else if (board.PieceCount(board.Turn, PShape.Round) > 0)
            {
                move = new FutureMove(lastCol, shape);
            }
            // Otherwise return a "no move" (this should never happen)
            else
            {
                move = FutureMove.NoMove;
            }
        }

        Check(board);
        Debug.Log($"My pieces {myPiece.Count + 1} | Enemie Pieces {enemyPiece.Count + 1}");

        // Return move
        return move;
    }

    private void Check(Board board)
    {
        Piece piece;
        Pos pos;
        bool hasPiece;

        for (int i = 0; i < board.rows; i++)
        {
            for (int z = 0; z < board.cols; z++)
            {
                if (board[i, z] != null)
                {
                    hasPiece = false;
                    piece = (Piece)board[i, z];
                    pos = new Pos(i, z);
                    if (piece.color == board.Turn)
                    {
                        foreach (Pos position in myPiece)
                        {
                            if (position.col == pos.col && position.row == pos.row)
                            {
                                hasPiece = true;
                            }
                        }

                        if (!hasPiece)
                        {
                            myPiece.Add(pos);
                        }

                    }
                    else if (piece.color != board.Turn)
                    {
                        foreach (Pos position in enemyPiece)
                        {
                            if (position.col == pos.col && position.row == pos.row)
                            {
                                hasPiece = true;
                            }
                        }

                        if (!hasPiece)
                        {
                            enemyPiece.Add(pos);
                        }
                    }
                }
            }
        }
    }

    private FutureMove? Play(Board board)
    {
        FutureMove? move = null;
        int[] rows = new int[board.rows * board.cols];
        int[] cols = new int[board.rows * board.cols];
        int i = 0;

        foreach (Pos pos in myPiece)
        {
            rows.SetValue(pos.row, i);
            cols.SetValue(pos.col, i);
            i++;

        }

        for (int x = 0; x < board.cols; x++)
        {
            return CheckCols(board, x);
        }

        if(move == null)
        {
            for (int x = 0; x < board.cols; x++)
            {
                return CheckColsShape(board, x);
            }
        }
        /*if(move == null)
        {
            for (int x = board.rows - 1; x > 0; x--)
            {
                move = CheckRows(board, x);
            }
        }*/

        return move;

    }

    private FutureMove? CheckCols(Board board, int col)
    {
        FutureMove? move;
        List<bool> threeInLine = new List<bool>(3);
        Piece piece;

        for (int i = 0; i < board.rows; i++)
        {
            if (board[i, col] == null && threeInLine.Count != 3)
            {
                return null;
            }
            else if (board[i, col] == null && (threeInLine.Count == 3))
            {
                return move = new FutureMove(col, PShape.Round);
            }
            piece = (Piece)board[i, col];
            if (piece.color == board.Turn)
            {
                threeInLine.Add(true);
            }
            else
            {
                threeInLine.RemoveRange(0, threeInLine.Count);
            }
        }

        return move = null;
    }

    private FutureMove? CheckColsShape(Board board, int col)
    {
        FutureMove? move;
        List<bool> threeInLine = new List<bool>();
        Piece piece;
        PColor color = board.Turn;
        PShape shape;

        if (color == PColor.White)
        {
            shape = PShape.Round;
        }
        else
        {
            shape = PShape.Square;
        }
        

        for (int i = 0; i < board.rows; i++)
        {
            Debug.LogWarning($"3 IN LINE COUNT {threeInLine.Count}");
            if (board[i, col] == null && threeInLine.Count != 3)
            {
                return null;
            }
            else if (board[i, col] == null && (threeInLine.Count == 3))
            {
                Debug.LogWarning("RETURNED");
                return move = new FutureMove(col, shape);
            }
            piece = (Piece)board[i, col];
            if (piece.shape == shape)
            {
                threeInLine.Add(true);
            }
            else
            {
                threeInLine.RemoveRange(0, threeInLine.Count);
            } 
            if(threeInLine.Count == 3)
            {
                Debug.LogWarning("RETURNED");
                return move = new FutureMove(col, shape);
            }
        }

        return move = null;
    }

    private FutureMove? CheckRows(Board board, int row)
    {
        FutureMove? move = null;
        List<byte> threeInLine = new List<byte>();
        Piece piece;

        for (int i = 0; i < board.cols; i++)
        {
            if (board[row, i] == null)
            {
                threeInLine.Add(0);
            }
            else
            {
                piece = (Piece)board[row, i];
                if (piece.color == board.Turn)
                {
                    threeInLine.Add(2);
                }
                else
                {
                    threeInLine.Add(1);
                }
            }
        }

        for (int i = 0; i < threeInLine.Count - 2; i++)
        {
            if (threeInLine[i] == 2 && threeInLine[i + 1] == 2 &&
                (threeInLine[i + 2] == 2 || threeInLine[i + 2] == 0))
            {
                return move = new FutureMove(i, PShape.Round);
            }
        }
        /*if(move == null)
        {
            for (int i = threeInLine.Count; i > 2; i--)
            {
                if (threeInLine[i] == 2 && threeInLine[i - 1] == 2 &&
                    (threeInLine[i - 2] == 2 || threeInLine[i - 2] == 0))
                {
                    return move = new FutureMove(i, PShape.Round);
                }
            }
        }*/


        return null;
    }
}
