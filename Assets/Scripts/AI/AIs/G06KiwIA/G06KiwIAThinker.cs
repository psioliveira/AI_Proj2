using System;
using System.Collections.Generic;
using System.Threading;

namespace Assets.Scripts.AI.AIs.G06KiwIA
{
    class G06KiwIAThinker : IThinker
    {
        internal PShape shape;
        internal PColor color;
        private Random _random;
        private int _maxDepth = 2;

        private List<Pos> _positions = new List<Pos>();
        private List<Pos> _allPiecesPosition = new List<Pos>();
        private List<Pos> _thisPlayerPiecePosition = new List<Pos>();
        private List<Pos> _otherPlayerPiecePosition = new List<Pos>();

        public G06KiwIAThinker(int maxDepth)
        {
            this._maxDepth = maxDepth;
        }

        struct Play
        {
            public int? Position { get; set; }
            public int Score { get; set; }
            public Play(int? pos, int score)
            {
                this.Position = pos;
                this.Score = score;
            }
        }


        public FutureMove Think(Board board, CancellationToken cancelationToken)
        {
            FutureMove? futureMove;
            Play play;

            _random = new Random();

            int roundPieces;
            int squarePieces;

            //choosing the shape who it will play first
            shape = (board.Turn == PColor.White) ? PShape.Round : PShape.Square;

            futureMove = PlayPiece(board);

            if (futureMove != null)
            { return (FutureMove)futureMove; }

            play = Negamax(_maxDepth, board, board.Turn, cancelationToken);

            if (futureMove != null)
            {
                if (play.Position != null)
                { return new FutureMove(Convert.ToInt32(play.Position), PShape.Round); }
                else
                { return FutureMove.NoMove; }
            }

            futureMove = CheckWinSequences(board, shape);

            if (futureMove != null)
            { return (FutureMove)futureMove; }

            roundPieces = board.PieceCount(board.Turn, PShape.Round);
            squarePieces = board.PieceCount(board.Turn, PShape.Square);
            shape = roundPieces < squarePieces ? PShape.Square : PShape.Round;

            return new FutureMove(_random.Next(0, board.cols), shape);
        }

        private FutureMove? PlayPiece(Board board)
        {
            FutureMove? move = null;
            Check(board);

            foreach (Pos pos in _allPiecesPosition)
            {//if it gets a sequence of 3 pieces of same shape, returns that column for the next play with a specific piece shape
                if (move == null) { move = CheckColsShape(board, pos.column); }
                else { break; }
            }

            if (move == null)
            {//if it gets a sequence of 3 pieces of same color, returns that column for the next play ignoring the shape
                foreach (Pos pos in _allPiecesPosition)
                {
                    if (move == null) { move = CheckCols(board, pos.column); }
                }
            }

            if (move == null)
            {
                foreach (Pos pos in _allPiecesPosition)
                {
                    if (move == null) { move = CheckRows(board, pos.column); }
                }
            }

            return move;
        }


        private FutureMove? CheckWinSequences(Board board, PShape myShape)
        {
            Piece piece;
            foreach (IEnumerable<Pos> positions in board.winCorridors)
            {
                foreach (Pos pos in positions) { _positions.Add(pos); }
            }

            foreach (Pos pos in _positions)
            {
                if (board[pos.row, pos.column] == null)
                {
                    if (pos.column == 0)
                    {
                        //if the first column is epmty and the next one have one of mine pieces i'll play at the first col
                        if (board[pos.row, pos.column + 1] != null)
                        {
                            piece = (Piece)board[pos.row, pos.column + 1];
                            if (piece.color == board.Turn || piece.shape == myShape) { return new FutureMove(pos.column, myShape); }
                        }

                    }
                    //if the last column is epmty and the previous one have one of mine pieces i'll play at the first col
                    else if (pos.column == board.cols - 1)
                    {
                        if (board[pos.row, pos.column - 1] != null)
                        {
                            piece = (Piece)board[pos.row, pos.column - 1];
                            if (piece.color == board.Turn || piece.shape == myShape) { return new FutureMove(pos.column, myShape); }
                        }
                    }
                    else
                    {
                        if (board[pos.row, pos.column + 1] != null)
                        {
                            piece = (Piece)board[pos.row, pos.column + 1];
                            if (piece.color == board.Turn || piece.shape == myShape) { return new FutureMove(pos.column, myShape); }
                        }

                        if (board[pos.row, pos.column - 1] != null)
                        {
                            piece = (Piece)board[pos.row, pos.column - 1];
                            if (piece.color == board.Turn || piece.shape == myShape) { return new FutureMove(pos.column, myShape); }
                        }
                    }
                }
            }
            return null;
        }

        private Play Negamax(int maxDepth, Board board, PColor turn, CancellationToken cancellationToken)
        {
            Play selectedMove = new Play(null, int.MinValue);
            PColor nextTurn = (turn == PColor.Red) ? PColor.White : PColor.Red;


            if (cancellationToken.IsCancellationRequested)
            { return new Play(null, 0); }
            else
            {
                if (maxDepth <= 0)
                { return selectedMove; }
                for (int i = 0; i < board.rows; i++)
                {
                    for (int j = 0; j < board.cols; j++)
                    {
                        int column = j;
                        if (board[i, j] == null)
                        {
                            int roundPieces = board.PieceCount(board.Turn, PShape.Round);
                            int squarePieces = board.PieceCount(board.Turn, PShape.Square);
                            Play move = default;

                            //if ran out of one shape, chooses another one
                            if (shape == PShape.Round)
                            {
                                if (roundPieces <= 0) { shape = PShape.Square; }
                            }
                            else
                            {
                                if (squarePieces <= 0) { shape = PShape.Round; }
                            }

                            board.DoMove(shape, j);
                            maxDepth = -1;

                            if (!(board.CheckWinner() == Winner.None)) { move = Negamax(maxDepth, board, nextTurn, cancellationToken); }

                            board.UndoMove();
                            move.Score = -move.Score;

                            if (move.Score > selectedMove.Score)
                            {
                                selectedMove.Score = move.Score;
                                selectedMove.Position = column;
                            }
                        }
                    }
                }
                return selectedMove;
            }
        }

        //add pieces into the enumerable lists

        private void Check(Board board)
        {
            bool hasPiece;
            Piece piece;
            Pos pos;

            for (int i = 0; i < board.rows; i++)
            {
                for (int j = 0; j < board.cols; j++)
                {
                    if (board[i, j] != null)
                    {
                        hasPiece = false;
                        pos = new Pos(i, j);
                        piece = (Piece)board[i, j];

                        if (piece.color == board.Turn)
                        {
                            foreach (Pos position in _thisPlayerPiecePosition)
                            {
                                if (position.column == pos.column && position.row == pos.row) { hasPiece = true; }
                            }

                            if (!hasPiece) { _thisPlayerPiecePosition.Add(pos); }

                        }
                        else if (piece.color != board.Turn)
                        {
                            foreach (Pos position in _otherPlayerPiecePosition)
                            {
                                if (position.column == pos.column && position.row == pos.row) { hasPiece = true; }
                            }

                            if (!hasPiece) { _otherPlayerPiecePosition.Add(pos); }
                        }
                        foreach (Pos position in _allPiecesPosition)
                        {
                            if (position.column == pos.column && position.row == pos.row) { hasPiece = true; }
                        }

                        if (!hasPiece) { _allPiecesPosition.Add(pos); }
                    }
                }
            }
        }


        private FutureMove? CheckCols(Board board, int column)
        {

            List<bool> sequenceOfPieces = new List<bool>(3);
            Piece? piece;

            for (int i = 0; i < board.rows; i++)
            {
                piece = board[i, column];
                if (piece == null)
                {
                    if (sequenceOfPieces.Count != 3)
                    { return null; }

                    else if ((sequenceOfPieces.Count == 3))
                    { return new FutureMove(column, PShape.Round); }
                }
                else if (((Piece)piece).color == board.Turn)
                { sequenceOfPieces.Add(true); }
                else
                { sequenceOfPieces.RemoveRange(0, sequenceOfPieces.Count); }

            }
            return null;
        }

        private FutureMove? CheckColsShape(Board board, int column)
        {
            List<bool> sequenceOfPieces = new List<bool>();
            Piece piece;


            for (int i = 0; i < board.rows; i++)
            {
                if (board[i, column] == null) { return null; }
                piece = (Piece)board[i, column];
                if (piece.shape == shape) { sequenceOfPieces.Add(true); }
                else { sequenceOfPieces.RemoveRange(0, sequenceOfPieces.Count); }
                if (sequenceOfPieces.Count == 3)
                { return new FutureMove(column, shape); }
            }
            return null;
        }

        private FutureMove? CheckRows(Board board, int row)
        {
            Piece piece;
            List<int> sequenceOfPieces = new List<int>();

            for (int i = 0; i < board.cols; i++)
            {
                if (board[row, i] != null)
                {
                    piece = (Piece)board[row, i];
                    if (piece.shape == PShape.Round && board.Turn == PColor.White ||
                       piece.shape == PShape.Square && board.Turn == PColor.Red)
                    { sequenceOfPieces.Add(3); }

                    else if (piece.color == board.Turn)
                    { sequenceOfPieces.Add(11); }

                    else { sequenceOfPieces.Add(7); }
                }
                else { sequenceOfPieces.Add(0); }
            }

            for (int i = 0; i < sequenceOfPieces.Count - 3; i++)
            {
                if (sequenceOfPieces[i] + sequenceOfPieces[i + 1] + sequenceOfPieces[i + 2] + sequenceOfPieces[i + 3] != 0 ||
                    sequenceOfPieces[i] + sequenceOfPieces[i + 1] + sequenceOfPieces[i + 2] + sequenceOfPieces[i + 3] == 21)
                {
                    if (board.Turn == PColor.White)
                    {
                        if (sequenceOfPieces[i] == 0) return new FutureMove(i, PShape.Round);
                        if (sequenceOfPieces[i + 1] == 0) return new FutureMove(i + 1, PShape.Round);
                        if (sequenceOfPieces[i + 2] == 0) return new FutureMove(i + 2, PShape.Round);
                        if (sequenceOfPieces[i + 3] == 0) return new FutureMove(i + 3, PShape.Round);
                    }
                    else
                    {
                        if (sequenceOfPieces[i] == 0) return new FutureMove(i, PShape.Square);
                        if (sequenceOfPieces[i + 1] == 0) return new FutureMove(i + 1, PShape.Square);
                        if (sequenceOfPieces[i + 2] == 0) return new FutureMove(i + 2, PShape.Square);
                        if (sequenceOfPieces[i + 3] == 0) return new FutureMove(i + 3, PShape.Square);
                    }
                }
                if (sequenceOfPieces[i] + sequenceOfPieces[i + 1] + sequenceOfPieces[i + 2] + sequenceOfPieces[i + 3] == 9)
                {
                    if (board.Turn == PColor.White)
                    {
                        if (sequenceOfPieces[i] == 0) return new FutureMove(i, PShape.Round);
                        if (sequenceOfPieces[i + 1] == 0) return new FutureMove(i + 1, PShape.Round);
                        if (sequenceOfPieces[i + 2] == 0) return new FutureMove(i + 2, PShape.Round);
                        if (sequenceOfPieces[i + 3] == 0) return new FutureMove(i + 3, PShape.Round);
                    }
                    else
                    {
                        if (sequenceOfPieces[i] == 0) return new FutureMove(i, PShape.Square);
                        if (sequenceOfPieces[i + 1] == 0) return new FutureMove(i + 1, PShape.Square);
                        if (sequenceOfPieces[i + 2] == 0) return new FutureMove(i + 2, PShape.Square);
                        if (sequenceOfPieces[i + 3] == 0) return new FutureMove(i + 3, PShape.Square);
                    }
                }
                if (sequenceOfPieces[i] + sequenceOfPieces[i + 1] + sequenceOfPieces[i + 2] + sequenceOfPieces[i + 3] == 33)
                {
                    if (board.Turn == PColor.White)
                    {
                        if (sequenceOfPieces[i] == 0) return new FutureMove(i, PShape.Round);
                        if (sequenceOfPieces[i + 1] == 0) return new FutureMove(i + 1, PShape.Round);
                        if (sequenceOfPieces[i + 2] == 0) return new FutureMove(i + 2, PShape.Round);
                        if (sequenceOfPieces[i + 3] == 0) return new FutureMove(i + 3, PShape.Round);
                    }
                    else
                    {
                        if (sequenceOfPieces[i] == 0) return new FutureMove(i, PShape.Square);
                        if (sequenceOfPieces[i + 1] == 0) return new FutureMove(i + 1, PShape.Square);
                        if (sequenceOfPieces[i + 2] == 0) return new FutureMove(i + 2, PShape.Square);
                        if (sequenceOfPieces[i + 3] == 0) return new FutureMove(i + 3, PShape.Square);
                    }
                }

            }
            return null;
        }
    }
}
