using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class NoScoreAIThinker : IThinker
{
    /// <summary>
    /// Lis of positions.
    /// </summary>
    private List<Pos> positions = new List<Pos>();

    /// <summary>
    /// A random.
    /// </summary>
    private System.Random random;

    /// <summary>
    /// List containing all the enemy pieces.
    /// </summary>
    private List<Pos> enemyPiece = new List<Pos>();

    /// <summary>
    /// List containing all my pieces.
    /// </summary>
    private List<Pos> myPiece = new List<Pos>();

    /// <summary>
    /// List containing all pieces.
    /// </summary>
    private List<Pos> allPiece = new List<Pos>();

    /// <summary>
    /// A readonly integer that contains the max depth a NegaMax is going.
    /// </summary>
    private readonly int maxDepth = 20;

    /// <summary>
    /// Color of my AI.
    /// </summary>
    PColor color;

    /// <summary>
    /// Shape of my AI.
    /// </summary>
    PShape shape;

    private List<Play> myWinCorridors = new List<Play>();

    /// <summary>
    /// Struck play used to assign scores to positions.
    /// </summary>
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

    /// <summary>
    /// Method used to play a move.
    /// </summary>
    /// <param name="board"></param>
    /// <param name="ct"></param>
    /// <returns>Move</returns>
    public FutureMove Think(Board board, CancellationToken ct)
    {
        FutureMove? test;
        Play play;
        random = new System.Random();

        color = board.Turn;

        if (color == PColor.White)
        {
            shape = PShape.Round;
        }
        else
        {
            shape = PShape.Square;
        }

        if (shape == PShape.Round && board.PieceCount(color, PShape.Round) == 0)
        {
            shape = PShape.Square;
        }
        else if (shape == PShape.Square && board.PieceCount(color, PShape.Square) == 0)
        {
            shape = PShape.Round;
        }

        Check(board);

        test = PlayPiece(board);
        if (test != null)
        {
            return (FutureMove)test;
        }

        test = CheckEnemy(board);
        if (test != null)
        {
            return (FutureMove)test;
        }

        play = Negamax(board, board.Turn, maxDepth, ct);

        if (test != null)
        {
            if (play.pos == null)
            {
                return FutureMove.NoMove;
            }
            else
            {
                return new FutureMove((int)play.pos, PShape.Round);
            }
        }

        if (allPiece.Count % 2 == 0)
        {
            test = CheckWinCorridors(board);
        }
        else
        {
            test = CheckEnemyWinCorridors(board);
        }



        if (test != null)
        {
            return (FutureMove)test;
        }

        /*int roundPieces = board.PieceCount(board.Turn, PShape.Round);
        int squarePieces = board.PieceCount(board.Turn, PShape.Square);
        shape = squarePieces < roundPieces ? PShape.Round : PShape.Square;*/

        return new FutureMove(random.Next(0, board.cols), shape);
    }

    /// <summary>
    /// Method used to check my wincorridors and play according to it.
    /// </summary>
    /// <param name="board"></param>
    /// <returns>A move for the AI to use.</returns>
    private FutureMove? CheckWinCorridors(Board board)
    {
        Piece piece;
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

                        if (piece.color == board.Turn || piece.shape == shape)
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

    /// <summary>
    /// Method used to check enemy WinCorridors and play according to it,
    /// in order to block is line
    /// </summary>
    /// <param name="board"></param>
    /// <returns>A play for the AI to use.</returns>
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

    /// <summary>
    /// Negamax.
    /// </summary>
    /// <param name="board"></param>
    /// <param name="turn"></param>
    /// <param name="maxDepth"></param>
    /// <param name="ct"></param>
    /// <returns>A play for the AI to use.</returns>
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
            if (maxDepth <= 0)
            {
                return bestMove;
            }

            foreach (Play play in myWinCorridors)
            {
                for (int j = 0; j < board.cols; j++)
                {
                    int pos = j;

                    if (board[(int)play.pos, j] == null)
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

            /*for (int i = 0; i < board.rows; i++)
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
            }*/

            return bestMove;
        }
    }

    /// <summary>
    /// Method used to check all the pieces in game.
    /// </summary>
    /// <param name="board"></param>
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

    /// <summary>
    /// Method used to play a Piece.
    /// </summary>
    /// <param name="board"></param>
    /// <returns>A play for the AI to use.</returns>
    private FutureMove? PlayPiece(Board board)
    {
        FutureMove? move = null;

        foreach (Pos pos in allPiece)
        {
            if (move == null)
            {
                move = CheckColsShape(board, pos.col);
            }
        }

        if (move == null)
        {
            foreach (Pos pos in allPiece)
            {
                if (move == null)
                {
                    move = CheckCols(board, pos.col);
                }
            }
        }

        if (move == null)
        {
            foreach (Pos pos in allPiece)
            {
                if (move == null)
                {
                    move = CheckRowsColorShape(board, pos.row);
                }
            }
        }

        return move;

    }

    private FutureMove? CheckRowsColorShape(Board board, int row)
    {
        string[] check = new string[board.cols];
        string result = "";
        const string pos1 = "_OOO";
        const string pos2 = "O_OO";
        const string pos3 = "OO_O";
        const string pos4 = "OOO_";

        for (int col = 0; col < board.cols; col++)
        {
            if (board[row, col] != null)
            {
                if (board[row, col].Value.color == color || board[row, col].Value.shape == shape)
                {
                    //2 Indicates its my piece of my color
                    check[col] = "O";
                }
                else if (board[row, col].Value.color != color || board[row, col].Value.shape != shape)
                {
                    //1 Indicates its enemy piece of enemy color
                    check[col] = "X";
                }
            }
            else
            {
                //0 Indicates theres nothing there
                check[col] = "_";
            }
        }

        for (int i = 0; i < check.Length; i++)
        {
            result += check[i];
        }

        if (result.Contains(pos1))
        {
            return PlayWinBlockRow(board, pos1, check);
        }
        else if (result.Contains(pos2))
        {
            return PlayWinBlockRow(board, pos2, check);
        }
        else if (result.Contains(pos3))
        {
            return PlayWinBlockRow(board, pos3, check);
        }
        else if (result.Contains(pos4))
        {
            return PlayWinBlockRow(board, pos4, check);
        }
        return null;
    }

    private FutureMove? CheckEnemyRowsColorShape(Board board, int row)
    {
        string[] check = new string[board.cols];
        string result = "";
        const string pos1 = "_OOO";
        const string pos2 = "O_OO";
        const string pos3 = "OO_O";
        const string pos4 = "OOO_";

        for (int col = 0; col < board.cols; col++)
        {
            if (board[row, col] != null)
            {
                if (board[row, col].Value.color != color || board[row, col].Value.shape != shape)
                {
                    //2 Indicates its my piece of my color
                    check[col] = "O";
                }
                else if (board[row, col].Value.color == color || board[row, col].Value.shape == shape)
                {
                    //1 Indicates its enemy piece of enemy color
                    check[col] = "X";
                }
            }
            else
            {
                //0 Indicates theres nothing there
                check[col] = "_";
            }
        }

        for (int i = 0; i < check.Length; i++)
        {
            result += check[i];
        }

        if (result.Contains(pos1))
        {
            return PlayWinBlockRow(board, pos1, check);
        }
        else if (result.Contains(pos2))
        {
            return PlayWinBlockRow(board, pos2, check);
        }
        else if (result.Contains(pos3))
        {
            return PlayWinBlockRow(board, pos3, check);
        }
        else if (result.Contains(pos4))
        {
            return PlayWinBlockRow(board, pos4, check);
        }
        return null;
    }

    private FutureMove? PlayWinBlockRow(Board board, string pos, string[] results)
    {
        int wCol = 0;
        //Debug.LogError(results);
        //Debug.LogError(pos);
        //Dictionary<string, int> winPos = new Dictionary<string, int>();
        List<int> cols = new List<int>();
        int x = 0;

        for (int i = 0; i < results.Length; i++)
        {
            if (pos.Contains(results[i]))
            {
                Debug.LogError($"Added {results[i]}, range is {cols.Count}");
                cols.Add(i);
                if (results[i] == "_") wCol = i;
                if (cols.Count == 4)
                {
                    Debug.LogError("RETURNED");
                    return new FutureMove(wCol, shape);
                }
                x++;
            }
            else
            {
                if (cols.Count < 4)
                {
                    cols.RemoveRange(0, cols.Count);
                }
            }
        }

        /*foreach(string cPos in results)
        {
            if(cPos == pos[i])
            {
                Debug.Log($"Added {cPos} to dictionary {cPos.GetType()}");
                winPos.Add(cPos, i);
                i++;
            }
            if(winPos.Count == 4)
            {
                break;
            }
            
        }*/

        return null;
    }

    /// <summary>
    /// Checks colums containing a winning move according to the color.
    /// </summary>
    /// <param name="board"></param>
    /// <param name="col"></param>
    /// <returns>A winning move for the AI to use.</returns>
    private FutureMove? CheckCols(Board board, int col)
    {
        FutureMove? move;
        List<bool> threeInLine = new List<bool>(3);
        Piece piece;
        PColor enemyColor = color == PColor.White ? PColor.Red : PColor.White;

        for (int i = 0; i < board.rows; i++)
        {
            if (board[i, col] == null)
            {
                return null;
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
            if (threeInLine.Count == 3)
            {
                if (board[i + 1, col].HasValue || i == board.rows)
                {
                    piece = (Piece)board[i + 1, col];
                    if (piece.color == enemyColor)
                    {
                        threeInLine.RemoveRange(0, threeInLine.Count);
                    }
                }
                else
                {
                    if (!board.IsColumnFull(col))
                    {
                        return move = new FutureMove(col, shape);
                    }

                }
            }
        }

        return move = null;
    }

    /// <summary>
    /// Check colums containing a winning move according to the shape.
    /// </summary>
    /// <param name="board"></param>
    /// <param name="col"></param>
    /// <returns>A winning move for the AI to use.</returns>
    private FutureMove? CheckColsShape(Board board, int col)
    {
        FutureMove? move;
        List<bool> threeInLine = new List<bool>();
        Piece piece;
        PShape enemyShape =
            color == PColor.White ? PShape.Square : PShape.Round;

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
            if (threeInLine.Count == 3)
            {
                if (board[i + 1, col].HasValue || i == board.rows)
                {
                    piece = (Piece)board[i + 1, col];
                    if (piece.shape == enemyShape)
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

    /// <summary>
    /// Checks enemy Colums.
    /// </summary>
    /// <param name="board"></param>
    /// <returns>A move to block enemy winning.</returns>
    private FutureMove? CheckEnemy(Board board)
    {
        FutureMove? move = null;

        foreach (Pos pos in allPiece)
        {
            if (move == null)
            {
                move = CheckEnemyColsShape(board, pos.col);

            }
        }

        if (move == null)
        {
            foreach (Pos pos in allPiece)
            {
                if (move == null)
                {
                    move = CheckEnemyCols(board, pos.col);

                }
            }
        }

        if (move == null)
        {
            foreach (Pos pos in allPiece)
            {
                if (move == null)
                {
                    move = CheckEnemyRowsColorShape(board, pos.row);
                }
            }
        }

        return move;
    }

    /// <summary>
    /// Checks enemy colums for an winning move according to color.
    /// </summary>
    /// <param name="board"></param>
    /// <param name="col"></param>
    /// <returns>A move to block enemy win.</returns>
    private FutureMove? CheckEnemyCols(Board board, int col)
    {
        FutureMove? move;
        List<bool> threeInLine = new List<bool>(3);
        Piece piece;
        PColor enemyColor = color == PColor.White ? PColor.Red : PColor.White;

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
                    if (piece.color == color)
                    {
                        threeInLine.RemoveRange(0, threeInLine.Count);
                    }
                }
                else
                {
                    if (!board.IsColumnFull(col))
                    {
                        return move = new FutureMove(col, shape);
                    }

                }

            }
        }

        return move = null;
    }

    /// <summary>
    /// Check enemy colums for a winning move according to shapes.
    /// </summary>
    /// <param name="board"></param>
    /// <param name="col"></param>
    /// <returns>A move that blocks enemy win.</returns>
    private FutureMove? CheckEnemyColsShape(Board board, int col)
    {
        FutureMove? move;
        List<bool> threeInLine = new List<bool>();
        Piece piece;
        PShape enemyShape =
            color == PColor.White ? PShape.Square : PShape.Round;

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
                    if (!board.IsColumnFull(col))
                    {
                        return move = new FutureMove(col, shape);
                    }
                }
            }
        }

        return move = null;
    }
}
