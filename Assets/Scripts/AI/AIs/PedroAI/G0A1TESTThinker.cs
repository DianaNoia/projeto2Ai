using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class G0A1TESTThinker : IThinker
{
    private PColor  _myColor;
    
    //variables to try to use later
    private int     _iterationsLimit = 2;
    private int     _actualIteration = 0;

    private float   _maxThinkTime;
    private float   _actualThinkingTime;

    private bool    _cancelationRequested;
    private bool    _turn;
    

    public FutureMove Think(Board board, CancellationToken ct)
    {
        //Get my color
        _myColor = board.Turn;

        int bestScore = int.MinValue;
        FutureMove finalMoveToReturn = FutureMove.NoMove;

        for (int i = 0; i < board.cols; i++)
        {
            int iterationScore;

            PShape bestShape;

            if(board.IsColumnFull(i))
            {
                i++;
                if(i > board.cols)
                {
                    i = 0;
                }
            }
            board.DoMove(PShape.Round, i);
            int roundPieceMoveScore = -NegaMax(board, ct, _actualIteration + 1, board.Turn) + GetBoardScore(board);
            board.UndoMove();

            board.DoMove(PShape.Square, i);
            int squarePieceMoveScore = -NegaMax(board, ct, _actualIteration + 1, board.Turn) + GetBoardScore(board);
            board.UndoMove();

            if (squarePieceMoveScore > roundPieceMoveScore)
            {
                iterationScore = squarePieceMoveScore;
                bestShape = PShape.Round;
            }
            else
            {
                iterationScore = roundPieceMoveScore;
                bestShape = PShape.Square;
            }

            //Get the move with the best score
            if (iterationScore > bestScore)
            {
                bestScore = roundPieceMoveScore;
                finalMoveToReturn = new FutureMove(i, bestShape);
            }
        }

        if (_cancelationRequested == true)
        {
            finalMoveToReturn = FutureMove.NoMove;
        } 

        return finalMoveToReturn;
    }

    private int NegaMax(Board board, CancellationToken ct, int iteration, PColor turn)
    {

        int bestScore = int.MinValue;
        FutureMove finalMoveToReturn = FutureMove.NoMove;

        if (ct.IsCancellationRequested == true)
        {
            _cancelationRequested = true;
        }   
        else
        {

            if (iteration == _iterationsLimit || board.CheckWinner() != Winner.None)
            {
                return bestScore;
            }

            for (int i = 0; i < board.cols; i++)
            {
                int iterationScore;
                int roundPieceMoveScore = 0;
                int squarePieceMoveScore = 0;

                PShape bestShape;

                if(board.IsColumnFull(i))
                {
                    do
                    {
                        i++;
                        if (i > board.cols)
                        {
                            i = 0;
                        }
                    }
                    while (board.IsColumnFull(i));
                }
                

                if (board.PieceCount(board.Turn, PShape.Round) != 0)
                {
                    board.DoMove(PShape.Round, i);
                    roundPieceMoveScore = -NegaMax(board, ct, iteration + 1, turn) + GetBoardScore(board);
                    board.UndoMove();
                }
                else
                {
                    board.DoMove(PShape.Square, i);
                    squarePieceMoveScore = -NegaMax(board, ct, iteration + 1, turn) + GetBoardScore(board);
                    board.UndoMove();
                }
                

                if (squarePieceMoveScore > roundPieceMoveScore)
                {
                    iterationScore = squarePieceMoveScore;
                    bestShape = PShape.Round;
                }
                else
                {
                    iterationScore = roundPieceMoveScore;
                    bestShape = PShape.Square;
                }

                //Set maxScore to largest value
                if (iterationScore > bestScore)
                {
                    bestScore = roundPieceMoveScore;
                    finalMoveToReturn = new FutureMove(i, bestShape);
                }
            }
        }

        return bestScore;
    }

    private int GetBoardScore(Board board)
    {
        int heuristicTotal = 0;

        PShape myShape;

        if (board.Turn == PColor.White)
        {
            myShape = PShape.Round;
        }
        else
        {
            myShape = PShape.Square;
        }

        int colorSequence = 0;
        int shapeSequence = 0;

        List<int> colorSequenceList = new List<int>();
        List<int> shapeSequenceList = new List<int>();

        if(board.CheckWinner() == Winner.None)
        {

            foreach(IEnumerable corridor in board.winCorridors)
            {
                //reset variables
                Piece piece;

                foreach (Pos pos in corridor)
                {
                    // Does position contain the appropriate piece?

                    if (board[pos.col, pos.row].HasValue)
                    {
                        piece = (Piece)board[pos.col, pos.row];

                        if (piece.color == board.Turn)
                        {

                            colorSequence+=2;
                        }
                        else
                        {
                            colorSequenceList.Add(colorSequence);
                            colorSequence = 0;
                        }

                        if (piece.shape == myShape)
                        {
                            shapeSequence+=2;
                        }
                        else
                        {
                            shapeSequenceList.Add(shapeSequence);
                            shapeSequence = 0;
                        }
                    } 
                    else
                    {
                        shapeSequence = 1;
                        colorSequence = 1;

                    }
                        
                }

            }
            
            foreach(int i in colorSequenceList)
            {
                if(i == 1)
                {
                    heuristicTotal += 1;
                }
                else if(i == 2)
                {
                    heuristicTotal += 3; 
                }
                else if (i == 3)
                {
                    heuristicTotal += 7;
                }
                else if (i == 4)
                {
                    heuristicTotal = 1000;
                    Debug.LogWarning(heuristicTotal);
                    return heuristicTotal;
                }

            }

            foreach (int i in shapeSequenceList)
            {

                if(i == 1)
                {
                    heuristicTotal += 2;
                }
                if (i == 2)
                {
                    heuristicTotal += 5;
                }
                else if (i == 3)
                {
                    heuristicTotal += 10;
                }
                else if (i == 4)
                {
                    heuristicTotal = 1000;
                    Debug.LogWarning(heuristicTotal);
                    return heuristicTotal;
                }

            }

            Debug.LogWarning(heuristicTotal);
            return heuristicTotal;
        }
        else if (board.Turn == PColor.Red && board.CheckWinner() != Winner.Red)
        {
            return 1000000;
        }
        else if (board.Turn == PColor.White && board.CheckWinner() != Winner.White)
        {
            return 1000000;
        }
        else
        {
            return 0;
        }

    }
}