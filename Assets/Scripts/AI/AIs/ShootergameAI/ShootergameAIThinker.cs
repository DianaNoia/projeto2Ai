using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ShootergameAIThinker : IThinker
{
    private List<Pos> positions = new List<Pos>();

    private System.Random random;

    private List<Pos> enemyPiece = new List<Pos>();
    private List<Pos> myPiece = new List<Pos>();
    private List<Pos> allPiece = new List<Pos>();

    private bool[] blockCols; 

    private int maxDepth = 2;

    PColor color;
    PShape shape;

    struct Play
    {
        public int? pos { get; set; }
        public int score { get; set; }
        public Play(int? pos, int score)
        {
            this.pos = pos;
            this.score = score;
        }
    }

    public FutureMove Think(Board board, CancellationToken ct)
    {
        FutureMove? test;
        Play play;
        random = new System.Random();

        //int i = random.Next(0, 11);

        blockCols = new bool[board.cols];

        color = board.Turn;

        if (color == PColor.White)
        {
            shape = PShape.Round;
        }
        else
        {
            shape = PShape.Square;
        }

        Check(board);

        test = PlayPiece(board);
        if (test != null)
        {
            Debug.LogError("Win");
            return (FutureMove)test;
        }

        test = CheckEnemy(board);
        if (test != null)
        {
            Debug.LogError("Stopped Win");
            return (FutureMove)test;
        }

        play = Negamax(board, board.Turn, maxDepth, ct);

        if (test != null)
        {
            if(play.pos == null)
            {
                return FutureMove.NoMove;
            }
            else
            {
                Debug.LogError("Play with NegaMax");
                return new FutureMove((int)play.pos, PShape.Round);
            }
        }

        if(allPiece.Count % 2 == 0)
        {
            test = CheckWinCorridors(board);
        }
        else
        {
            test = CheckEnemyWinCorridors(board);
        }

        

        if(test != null)
        {
            Debug.LogError("Play with WinCorridoes");
            return (FutureMove)test;
        }        

        int roundPieces = board.PieceCount(board.Turn, PShape.Round);
        int squarePieces = board.PieceCount(board.Turn, PShape.Square);
        shape = squarePieces < roundPieces ? PShape.Round : PShape.Square;

        return new FutureMove(random.Next(0, board.cols), shape); 
    }

    private FutureMove? CheckWinCorridors(Board board)
    {
        Piece piece;
        foreach(IEnumerable<Pos> enumerable in board.winCorridors)
        {
            foreach(Pos pos in enumerable)
            {
                positions.Add(pos);
            }
        }

        foreach(Pos pos in positions)
        {
            if(board[pos.row, pos.col] == null)
            {
                if(pos.col == 0)
                {
                    if(board[pos.row, pos.col + 1] != null)
                    {
                        piece = (Piece)board[pos.row, pos.col + 1];

                        if (piece.color == board.Turn || piece.shape == shape)
                        {
                            return new FutureMove(pos.col, shape);
                        }
                    }
                    
                }else if(pos.col == board.cols - 1)
                {
                    if (board[pos.row, pos.col - 1] != null)
                    {
                        piece = (Piece)board[pos.row, pos.col - 1];

                        if (piece.color == board.Turn || piece.shape == shape)
                        {
                            return new FutureMove(pos.col, shape);
                        }
                    }
                }
                else
                {
                    if (board[pos.row, pos.col + 1] != null)
                    {
                        piece = (Piece)board[pos.row, pos.col + 1];

                        if (piece.color == board.Turn || piece.shape == shape)
                        {
                            return new FutureMove(pos.col, shape);
                        }
                    }

                    if (board[pos.row, pos.col - 1] != null)
                    {
                        piece = (Piece)board[pos.row, pos.col - 1];
                        if (piece.color == board.Turn || piece.shape == shape)
                        {
                            return new FutureMove(pos.col, shape);
                        }
                    }
                }               
                
            }
        }

        return null;
    }

    private FutureMove? CheckEnemyWinCorridors(Board board)
    {
        Piece piece;
        PColor enemyColor =
            color == PColor.White ? PColor.Red : PColor.White;
        PShape enemyShape =
            shape == PShape.Round ? PShape.Square : PShape.Round;

        foreach (IEnumerable<Pos> enumerable in board.winCorridors)
        {
            foreach (Pos pos in enumerable)
            {
                positions.Add(pos);
            }
        }

        foreach (Pos pos in positions)
        {
            if (board[pos.row, pos.col] == null)
            {
                if (pos.col == 0)
                {
                    if (board[pos.row, pos.col + 1] != null)
                    {
                        piece = (Piece)board[pos.row, pos.col + 1];

                        if (piece.color == enemyColor || piece.shape == enemyShape)
                        {
                            return new FutureMove(pos.col, shape);
                        }
                    }

                }
                else if (pos.col == board.cols - 1)
                {
                    if (board[pos.row, pos.col - 1] != null)
                    {
                        piece = (Piece)board[pos.row, pos.col - 1];

                        if (piece.color == enemyColor || piece.shape == enemyShape)
                        {
                            return new FutureMove(pos.col, shape);
                        }
                    }
                }
                else
                {
                    if (board[pos.row, pos.col + 1] != null)
                    {
                        piece = (Piece)board[pos.row, pos.col + 1];

                        if (piece.color == enemyColor || piece.shape == enemyShape)
                        {
                            return new FutureMove(pos.col, shape);
                        }
                    }

                    if (board[pos.row, pos.col - 1] != null)
                    {
                        piece = (Piece)board[pos.row, pos.col - 1];
                        if (piece.color == enemyColor || piece.shape == enemyShape)
                        {
                            return new FutureMove(pos.col, shape);
                        }
                    }
                }

            }
        }

        return null;
    }

    private Play Negamax(Board board, PColor turn, int maxDepth, CancellationToken ct)
    {
        Play bestMove = new Play(null, int.MinValue);

        PColor proxTurn =
            turn == PColor.Red ? PColor.White : PColor.Red;

        bool stup = false;

        if (ct.IsCancellationRequested)
        {
            return new Play(null, 0);
        }
        else
        {
            if(maxDepth <= 0)
            {
                return bestMove;
            }            

            for (int i = 0; i < board.rows; i++)
            {
                for (int j = 0; j < board.cols; j++)
                {
                    int pos = j;

                    if (board[i, j] == null)
                    {
                        int roundPieces = board.PieceCount(board.Turn, PShape.Round);
                        int squarePieces = board.PieceCount(board.Turn, PShape.Square);
                        if (shape == PShape.Round)
                        {
                            if (roundPieces == 0)
                            {
                                shape = PShape.Square;
                            }
                        }
                        else
                        {
                            if (squarePieces == 0)
                            {
                                shape = PShape.Round;
                            }
                        }

                        Play move = default;

                        board.DoMove(shape, j);

                        maxDepth--;

                        if (board.CheckWinner() != Winner.None)
                        {
                            stup = true;
                        }

                        if (!stup)
                        {
                            move = Negamax(board, proxTurn, maxDepth, ct);
                        }

                        board.UndoMove();

                        move.score = -move.score;

                        if (move.score > bestMove.score)
                        {
                            bestMove.score = move.score;
                            bestMove.pos = pos;
                        }
                    }
                }
            }

            return bestMove;
        }
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
                    foreach (Pos position in allPiece)
                    {
                        if (position.col == pos.col && position.row == pos.row)
                        {
                            hasPiece = true;
                        }
                    }

                    if (!hasPiece)
                    {
                        allPiece.Add(pos);
                    }
                }
            }
        }
    }

    private FutureMove? PlayPiece(Board board)
    {
        FutureMove? move = null;

        foreach(Pos pos in allPiece)
        {
            if(move == null)
            {
                move = CheckColsShape(board, pos.col);
            }
        }

       if(move == null)
       {
            foreach (Pos pos in allPiece)
            {
                if (move == null)
                {
                    move = CheckCols(board, pos.col);
                }
            }
       }

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

        for (int i = 0; i < board.rows; i++)
        {
            if (board[i, col] == null)
            {
                return null;
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
                return move = new FutureMove(col, shape);
            }
        }

        return move = null;
    }

    private FutureMove? CheckEnemy(Board board)
    {
        FutureMove? move = null;

        for(int i = 0; i < blockCols.Length; i++)
        {
            Debug.LogError(blockCols[i]);
        }

        foreach (Pos pos in allPiece)
        {
            if (move == null)
            {
                if(blockCols[pos.col] == false)
                {
                    move = CheckEnemyColsShape(board, pos.col);
                }
                
            }
        }

        if (move == null)
        {
            foreach (Pos pos in allPiece)
            {
                if (move == null)
                {
                    if (blockCols[pos.col] == false)
                    {
                        move = CheckEnemyCols(board, pos.col);
                    }
                    
                }
            }
        }

        return move;
    }

    private FutureMove? CheckEnemyCols(Board board, int col)
    {
        FutureMove? move;
        List<bool> threeInLine = new List<bool>(3);
        Piece piece;
        PColor enemyColor = color == PColor.White ? PColor.Red: PColor.White;

        for (int i = 0; i < board.rows; i++)
        {
            if (board[i, col] == null)
            {
                return null;
            }
            
            piece = (Piece)board[i, col];
            if (piece.color == enemyColor)
            {
                threeInLine.Add(true);
            }
            else
            {
                threeInLine.RemoveRange(0, threeInLine.Count);
            }
            if (threeInLine.Count == 3)
            {
                if (board[i + 1, col].HasValue || i == board.rows)
                {
                    piece = (Piece)board[i + 1, col];
                    if (piece.shape == shape)
                    {
                        threeInLine.RemoveRange(0, threeInLine.Count);
                    }
                }
                else
                {
                    return move = new FutureMove(col, shape);
                }
                
            }
        }

        return move = null;
    }

    private FutureMove? CheckEnemyColsShape(Board board, int col)
    {
        FutureMove? move;
        List<bool> threeInLine = new List<bool>();
        Piece piece;
        PShape enemyShape =
            shape == PShape.Round ? PShape.Square : PShape.Round;

        for (int i = 0; i < board.rows; i++)
        {
            if (board[i, col] == null)
            {
                return null;
            }
            piece = (Piece)board[i, col];
            if (piece.shape == enemyShape)
            {
                threeInLine.Add(true);
            }
            else
            {
                threeInLine.RemoveRange(0, threeInLine.Count);
            }
            if (threeInLine.Count == 3)
            {
                if(board[i + 1, col].HasValue || i == board.rows)
                {
                    piece = (Piece)board[i + 1, col];
                    if(piece.shape == shape)
                    {
                        threeInLine.RemoveRange(0, threeInLine.Count);
                    }
                }
                else
                {
                    return move = new FutureMove(col, shape);
                }
                
            }
        }

        return move = null;
    }
}
