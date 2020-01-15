using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Assets.Scripts.AI.AIs.G06KiwIA
{
    class G06KiwIAThinker : IThinker
    {
        internal PShape shape;
        private Random _random;
        private int _maxDepth = 1;

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

        private Play bestMove;

        public FutureMove Think(Board board, CancellationToken cancelationToken)
        {

            Play play;

            _random = new Random();
            play = Negamax(_maxDepth, board, board.Turn, cancelationToken);

            if (play.Position != null) { return new FutureMove(Convert.ToInt32(play.Position), PShape.Round); }

            else { return new FutureMove(_random.Next(0, board.cols), shape); }
        }


        private Play Negamax(int depth, Board board, PColor turn, CancellationToken cancellationToken, int alpha = int.MinValue, int beta = int.MaxValue)
        {
            Play selectedMove = new Play(null, int.MinValue);
            PColor nextTurn = (turn == PColor.Red) ? PColor.White : PColor.Red;

            if (cancellationToken.IsCancellationRequested) { return new Play(null, 0); }

            if (depth <= 0)
            {
                int tempScore = _random.Next(0, 100);

                if (board.CheckWinner() != Winner.None)
                {
                    tempScore = 300;
                }
                tempScore += board.winCorridors.Count() * 10;

                if (tempScore % 2 != 0)
                {
                    tempScore *= -1;
                }
                selectedMove.Score = tempScore;


                return selectedMove;
            }
            for (int j = 0; j < board.cols; j++)
            {
                int column = j;
                for (int i = 0; i < board.rows; i++)
                {

                    if (board[i, j] == null)
                    {
                        int roundPieces = board.PieceCount(board.Turn, PShape.Round);
                        int squarePieces = board.PieceCount(board.Turn, PShape.Square);
                        Play move = default;

                        if (roundPieces > 0)
                        {
                            shape = PShape.Round;
                            board.DoMove(shape, j);
                            if ((board.CheckWinner() == Winner.None))
                            {
                                move = Negamax(depth - 1, board, nextTurn, cancellationToken, -beta, -alpha);
                            }
                            board.UndoMove();
                            move.Score = -move.Score;
                            if (move.Score > bestMove.Score)
                            {
                                bestMove = move;
                            }

                            if (bestMove.Score > alpha)
                            {
                                alpha = bestMove.Score;
                            }

                            if (bestMove.Score >= beta)
                            {
                                bestMove.Score = alpha;
                                bestMove.Position = column;
                                return bestMove;
                            }

                        }

                        if (squarePieces > 0)
                        {
                            shape = PShape.Square;
                            board.DoMove(shape, j);
                            if ((board.CheckWinner() == Winner.None))
                            {
                                move = Negamax(depth - 1, board, nextTurn, cancellationToken, -beta, -alpha);
                            }
                            board.UndoMove();
                            if (move.Score > bestMove.Score)
                            {
                                bestMove = move;
                            }

                            if (bestMove.Score > alpha)
                            {
                                alpha = bestMove.Score;
                            }

                            if (bestMove.Score >= beta)
                            {
                                bestMove.Score = alpha;
                                bestMove.Position = column;
                                return bestMove;
                            }
                        }
                    }
                }
            }
            return bestMove;
        }

    }

}

