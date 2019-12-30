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

    public FutureMove Think(Board board, CancellationToken ct)
    {
        // The move to perform
        FutureMove move;

        random = new System.Random();

        //This method is not working
        Check(board);

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
            return move;
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

        // Return move
        return move;
    }

    private void Check(Board board)
    {
        Piece piece;

        for(int i = 0; i >= board.rows; i++)
        {
            for(int z = 0; i >= board.cols; z++)
            {
                if(board[i,z] != null)
                {
                    piece = (Piece)board[i, z];
                    if(piece.color == board.Turn)
                    {
                        Debug.Log($"Found one of my Pieces at {i} | {z}");
                    }
                }
            }
        }
    }

}
