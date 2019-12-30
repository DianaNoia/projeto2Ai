using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MyAIThinker : IThinker
{
    private int lastCol = -1;
    private int bestCol;
    private int roundPiece;
    private int squarePiece;

    private List<int> col;
    private int row;

    private bool hasWinCorridors;

    public FutureMove Think(Board board, CancellationToken ct)
    {
        // The move to perform
        FutureMove move;

        foreach (IEnumerable winCorridors in board.winCorridors)
        {
            foreach (Pos pos in winCorridors)
            {
                if(pos.col == null)
                {
                    col.Add(pos.col);
                    hasWinCorridors = true;
                }
                
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

        

        /*for (int i = 0; i >= board.cols; i++)
        {
            bool isCollumFull = board.IsColumnFull(i);

            if (i == 0 && !isCollumFull)
            {
                bestCol = i;
            }
            else if (!isCollumFull)
            {

            }
        }*/

        if (hasWinCorridors)
        {
            
            if (board.PieceCount(board.Turn, PShape.Square) > 0)
            {
                move = new FutureMove(col[Random.Range(0, col.Count)], PShape.Square);
            }
            // If there's no round pieces left, let's try a square pieces
            else if (board.PieceCount(board.Turn, PShape.Round) > 0)
            {
                move = new FutureMove(col[Random.Range(0, col.Count)], PShape.Round);
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
                move = new FutureMove(lastCol, PShape.Square);
            }
            // If there's no round pieces left, let's try a square pieces
            else if (board.PieceCount(board.Turn, PShape.Round) > 0)
            {
                move = new FutureMove(lastCol, PShape.Round);
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
}
