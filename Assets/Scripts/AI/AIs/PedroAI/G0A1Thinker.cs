using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class G0A1Thinker : IThinker
{
    //iterations check variables
    private int     _iterationsLimit = 2;
    private int     _actualIteration = 0;

    //TUDO ADD TIME LIMIT
    //private float   _maxThinkTime;
    //private float   _actualThinkingTime;

    private bool    _cancelationRequested;
    
    public FutureMove Think(Board board, CancellationToken ct)
    {
        int bestScore = int.MinValue;
        FutureMove finalMoveToReturn = FutureMove.NoMove;

        for (int i = 0; i < board.cols; i++)
        {

            //check if the column is full
            if (board.IsColumnFull(i))
            {
                /*
                if (i == board.cols)
                {
                    i = 0;
                    //return FutureMove.NoMove;
                }
                */
                continue;
            }

            //score for this iteration
            int iterationScore;

            int roundPieceMoveScore = 0;
            int squarePieceMoveScore = 0;

            //best shape to be returned
            PShape bestShape;

            if(board.PieceCount(board.Turn, PShape.Round) != 0)
            {
                //try a move with a round piece and get score
                board.DoMove(PShape.Round, i);
                roundPieceMoveScore += -NegaMax(board, ct, _actualIteration + 1, board.Turn);
                board.UndoMove();
            }

            if (board.PieceCount(board.Turn, PShape.Round) != 0)
            {
                //try a move with a square piece and get score
                board.DoMove(PShape.Square, i);
                squarePieceMoveScore = -NegaMax(board, ct, _actualIteration + 1, board.Turn);
                board.UndoMove();
            }

            if (board.PieceCount(board.Turn, PShape.Round) == 0 && board.PieceCount(board.Turn, PShape.Square) == 0)
            {
                _cancelationRequested = true;
            }


            //check what piece is better
            if (squarePieceMoveScore > roundPieceMoveScore)
            {
                iterationScore = squarePieceMoveScore;
                if(board.PieceCount(board.Turn,PShape.Square) != 0)
                {
                    bestShape = PShape.Square;
                }
                else
                {
                    bestShape = PShape.Round;
                }
                
            }
            else
            {
                iterationScore = roundPieceMoveScore;
                if(board.PieceCount(board.Turn, PShape.Round) != 0)
                {
                    bestShape = PShape.Round;
                }
                else
                {
                    bestShape = PShape.Square;
                }
                
            }

            //Get the move with the best score
            if (iterationScore > bestScore)
            {
                bestScore = iterationScore;
                finalMoveToReturn = new FutureMove(i, bestShape);
            }
        }

        //check for think cancelation request
        
        if (_cancelationRequested == true)
        {
            finalMoveToReturn = FutureMove.NoMove;
        } 
        

        //return the move
        return finalMoveToReturn;
    }

    private int NegaMax(Board board, CancellationToken ct, int iteration, PColor turn)
    {
        //score variable
        int bestScore = int.MinValue;

        //future move to be returned
        FutureMove finalMoveToReturn = FutureMove.NoMove;
        
        //check for cancelation request
        if (ct.IsCancellationRequested == true)
        {
            _cancelationRequested = true;
        }   
        else
        {
            //check if we cant search more
            if (iteration == _iterationsLimit || board.CheckWinner() != Winner.None)
            {
                return bestScore;
            }
            //if we can

            for (int i = 0; i < board.cols; i++)
            {
              
                //check if column is full
                if (board.IsColumnFull(i))
                {
                    /*
                    if (i == board.cols)
                    {
                        i = 0;
                        //return 0;
                    }
                    */
                    // i = 0;
                    continue;
                }
                
                
                int iterationScore;

                //score for each move
                int roundPieceMoveScore = 0;
                int squarePieceMoveScore = 0;

                //best shape to be returned
                PShape bestShape;

                //check if AI still have pieces
                if (board.PieceCount(board.Turn, PShape.Round) != 0)
                {
                    //try a move
                    board.DoMove(PShape.Round, i);
                    roundPieceMoveScore = -NegaMax(board, ct, iteration + 1, turn) + GetBoardScore(board);
                    board.UndoMove();
                }

                //check if AI still have pieces
                if (board.PieceCount(board.Turn, PShape.Square) != 0)
                {
                    //try a move
                    board.DoMove(PShape.Square, i);
                    squarePieceMoveScore = -NegaMax(board, ct, iteration + 1, turn) + GetBoardScore(board);
                    board.UndoMove();
                }
                
                if(board.PieceCount(board.Turn, PShape.Round) == 0 && board.PieceCount(board.Turn, PShape.Square) == 0)
                {
                    _cancelationRequested = true;
                }

                //check witch move is better
                if (squarePieceMoveScore > roundPieceMoveScore)
                {
                    iterationScore = squarePieceMoveScore;
                    bestShape = PShape.Square;
                }
                else
                {
                    iterationScore = roundPieceMoveScore;
                    bestShape = PShape.Round;
                }

                //Set maxScore to largest value
                if (iterationScore > bestScore)
                {
                    bestScore = iterationScore;
                    finalMoveToReturn = new FutureMove(i, bestShape);
                }
            }
        }
        //return best value
        return bestScore;
    }

    private int GetBoardScore(Board board)
    {
        int finalScore = 0;

        int scoreP1 = 0;
        int scoreP2 = 0;

        Piece piece;
        PShape myShape;

        //get AI turn shape
        if (board.Turn == PColor.White)
        {
            myShape = PShape.Round;
        }
        else
        {
            myShape = PShape.Square;
        }

        //TODO create array with dynamic size
        //int[] ammountOfXColorSequencesP1 = new int[board.cols];
        //int[] ammountOfXShapeSequencesP1 = new int[board.cols];

        //int[] ammountOfXColorSequencesP2 = new int[board.cols];
        //int[] ammountOfXShapeSequencesP2 = new int[board.cols];

        //player 1 variable
        int ammountOf0ColorSequencesP1 = 0;
        int ammountOf1ColorSequencesP1 = 0;
        int ammountOf2ColorSequencesP1 = 0;
        int ammountOf3ColorSequencesP1 = 0;
        int ammountOf4ColorSequencesP1 = 0;

        int ammountOf0ShapeSequencesP1 = 0;
        int ammountOf1ShapeSequencesP1 = 0;
        int ammountOf2ShapeSequencesP1 = 0;
        int ammountOf3ShapeSequencesP1 = 0;
        int ammountOf4ShapeSequencesP1 = 0;

        int piecesInColorSequenceP1 = 0;
        int piecesInShapeSequenceP1 = 0;

        //lists with good pieces sequences of player 1
        List<int> colorSequencesListP1 = new List<int>();
        List<int> shapeSequencesListP1 = new List<int>();

        //player 2 variables
        int ammountOf0ColorSequencesP2 = 0;
        int ammountOf1ColorSequencesP2 = 0;
        int ammountOf2ColorSequencesP2 = 0;
        int ammountOf3ColorSequencesP2 = 0;
        int ammountOf4ColorSequencesP2 = 0;

        int ammountOf0ShapeSequencesP2 = 0;
        int ammountOf1ShapeSequencesP2 = 0;
        int ammountOf2ShapeSequencesP2 = 0;
        int ammountOf3ShapeSequencesP2 = 0;
        int ammountOf4ShapeSequencesP2 = 0;

        int piecesInColorSequenceP2 = 0;
        int piecesInShapeSequenceP2 = 0;

        //lists with good pieces sequences of player 2
        List<int> colorSequencesListP2 = new List<int>();
        List<int> shapeSequencesListP2 = new List<int>();

        //temporary corridor list
        List<Pos> thisCorridorPositions = new List<Pos>();


        //foreach corridor in win corridors
        foreach (IEnumerable corridor in board.winCorridors)
        {
            //add the corridor positions to a temporary corridor // problem might be here
            foreach (Pos pos in corridor)
            {
                thisCorridorPositions.Add(pos);
            }

            //foreach position in the temporary corridor
            foreach (Pos pos in thisCorridorPositions)
            {
                //check if there is no piece on this position // this is always true
                if (board[pos.row, pos.col] == null)
                {

                    //Debug.LogWarning("entrou aqui");
                    //add latest piece sequence to the sequences lists
                    colorSequencesListP1.Add(piecesInColorSequenceP1);
                    shapeSequencesListP1.Add(piecesInShapeSequenceP1);

                    colorSequencesListP2.Add(piecesInColorSequenceP2);
                    shapeSequencesListP2.Add(piecesInShapeSequenceP2);

                }

                //if there is a piece on this position
                if (board[pos.row, pos.col] != null)
                {
                    //get the piece
                    piece = (Piece)board[pos.row, pos.col];

                    //if piece color is mine 
                    if (piece.color == board.Turn)
                    {
                        //increase the counter
                        piecesInColorSequenceP2 = 0;
                        piecesInColorSequenceP1++;
                    }
                    else
                    {
                        piecesInColorSequenceP1 = 0;
                        piecesInColorSequenceP2++;
                    }
                    
                    
                    //if piece shape is mine
                    if (piece.shape == myShape )
                    {
                        //increase the counter
                        piecesInShapeSequenceP2 = 0;
                        piecesInShapeSequenceP1++;
                    }
                    else
                    {
                        piecesInShapeSequenceP1 = 0;
                        piecesInShapeSequenceP2++;
                    }
                    
                }
            }

            //clear the temporary corridor positions for the next corridor
            thisCorridorPositions.Clear();
        }


        //TODO create dynamic variables to use on dynamic board

        //get scores for the heuristic
        foreach (int i in colorSequencesListP1)
        {
            if (i == 0)
            {
                ammountOf0ColorSequencesP1 += 1;
            }
            else if (i == 1)
            {
                ammountOf1ColorSequencesP1 += 1;
            }
            else if (i == 2)
            {
                ammountOf2ColorSequencesP1 += 1;
            }
            else if (i == 3)
            {
                ammountOf3ColorSequencesP1 += 1;
            }
            else
            {
                ammountOf4ColorSequencesP1 += 1;
            }
        }

        foreach (int i in shapeSequencesListP1)
        {
            if (i == 0)
            {
                ammountOf0ShapeSequencesP1 += 1;
            }
            else if (i == 1)
            {
                ammountOf1ShapeSequencesP1 += 1;
            }
            else if (i == 2)
            {
                ammountOf2ShapeSequencesP1 += 1;
            }
            else if (i == 3)
            {
                ammountOf3ShapeSequencesP1 += 1;
            }
            else
            {
                ammountOf4ShapeSequencesP1 += 1;
            }
        }

        //get scores for the heuristic
        foreach (int i in colorSequencesListP2)
        {
            if (i == 0)
            {
                ammountOf0ColorSequencesP2 += 1;
            }
            else if (i == 1)
            {
                ammountOf1ColorSequencesP2 += 1;
            }
            else if (i == 2)
            {
                ammountOf2ColorSequencesP2 += 1;
            }
            else if (i == 3)
            {
                ammountOf3ColorSequencesP2 += 1;
            }
            else
            {
                ammountOf4ColorSequencesP2 += 1;
            }
        }

        foreach (int i in shapeSequencesListP2)
        {
            if (i == 0)
            {
                ammountOf0ShapeSequencesP2 += 1;
            }
            else if (i == 1)
            {
                ammountOf1ShapeSequencesP2 += 1;
            }
            else if (i == 2)
            {
                ammountOf2ShapeSequencesP2 += 1;
            }
            else if (i == 3)
            {
                ammountOf3ShapeSequencesP2 += 1;
            }
            else
            {
                ammountOf4ShapeSequencesP2 += 1;
            }
            
        }

        ammountOf1ColorSequencesP1 *= 1;
        ammountOf2ColorSequencesP1 *= 10;
        ammountOf3ColorSequencesP1 *= 100;
        ammountOf4ColorSequencesP1 *= 50000;

        ammountOf1ShapeSequencesP1 *= 2;
        ammountOf2ShapeSequencesP1 *= 20;
        ammountOf3ShapeSequencesP1 *= 200;
        ammountOf4ShapeSequencesP1 *= 100000;


        ammountOf1ColorSequencesP2 *= 1;
        ammountOf2ColorSequencesP2 *= 10;
        ammountOf3ColorSequencesP2 *= 100;
        ammountOf4ColorSequencesP2 *= 50000;

        ammountOf1ShapeSequencesP2 *= 2;
        ammountOf2ShapeSequencesP2 *= 20;
        ammountOf3ShapeSequencesP2 *= 200;
        ammountOf4ShapeSequencesP2 *= 100000;

        scoreP1 += ammountOf0ColorSequencesP1 + ammountOf0ShapeSequencesP1;
        scoreP1 += ammountOf1ColorSequencesP1 + ammountOf1ShapeSequencesP1;
        scoreP1 += ammountOf2ColorSequencesP1 + ammountOf2ShapeSequencesP1;
        scoreP1 += ammountOf3ColorSequencesP1 + ammountOf3ShapeSequencesP1;
        scoreP1 += ammountOf4ColorSequencesP1 + ammountOf4ShapeSequencesP1;

        scoreP2 += ammountOf0ColorSequencesP2 + ammountOf0ShapeSequencesP2;
        scoreP2 += ammountOf1ColorSequencesP2 + ammountOf1ShapeSequencesP2;
        scoreP2 += ammountOf2ColorSequencesP2 + ammountOf2ShapeSequencesP2;
        scoreP2 += ammountOf3ColorSequencesP2 + ammountOf3ShapeSequencesP2;
        scoreP2 += ammountOf4ColorSequencesP2 + ammountOf4ShapeSequencesP2;

        
        if (ammountOf4ColorSequencesP1 > 0 || ammountOf4ShapeSequencesP1 > 0)
        {
            return int.MaxValue;
        }
        if (ammountOf4ColorSequencesP2 > 0 || ammountOf4ShapeSequencesP2 > 0)
        {
            return int.MinValue;
        }

        finalScore = scoreP1;
        finalScore -= scoreP2;
        
        Debug.LogWarning(finalScore);
        return finalScore;

    }

}