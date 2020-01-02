using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MyAIThinker : IThinker
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
            return (FutureMove)test;
        }

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

        for(int i = 0; i < board.rows ; i++)
        {
            for(int z = 0; z < board.cols; z++)
            {
                if(board[i,z] != null)
                {
                    hasPiece = false;
                    piece = (Piece)board[i,z];
                    pos = new Pos(i, z);
                    if (piece.color == board.Turn)
                    {
                        foreach(Pos position in myPiece)
                        {
                            if(position.col == pos.col && position.row == pos.row)
                            {
                                hasPiece = true;
                            }
                        }

                        if (!hasPiece)
                        {
                            myPiece.Add(pos);
                        }
                        
                    }else if(piece.color != board.Turn)
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
        FutureMove? move = default;
        int[] rows = new int[board.rows * board.cols];
        int[] cols = new int[board.rows * board.cols];
        int i = 0;

        foreach(Pos pos in myPiece)
        {            
            rows.SetValue(pos.row, i);
            cols.SetValue(pos.col, i);
            i++;

        }

        for(int x = 0; x < board.cols; x++)
        {
            move = CheckCols(board, x);
        }

        return move;

    }

    private FutureMove? CheckCols(Board board, int col)
    {
        FutureMove move;
        List<bool> threeInLine = new List<bool>(3);
        Piece piece;  

        for (int i = 0; i < board.rows; i++)
        {
            if(board[i, col] == null && !(threeInLine.Count == 3))
            {
                return null;
            }else if (board[i, col] == null && (threeInLine.Count == 3))
            {
                return move = new FutureMove(col, PShape.Round);
            }
            piece = (Piece)board[i, col];
            if(piece.color == board.Turn)
            {
                threeInLine.Add(true);
            }
            else
            {
                threeInLine.RemoveRange(0, threeInLine.Count);
            }            
        }

        return null;
    }

}
