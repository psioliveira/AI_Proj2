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
            FutureMove? futureMove;
            Play play;
            int roundPieces;
            int squarePieces;

            _random = new Random();

            play = Negamax(depth, board, board.Turn, cancelationToken);

            if (play.Position != null) { return new FutureMove(Convert.ToInt32(play.Position), PShape.Round); }

            else { return new FutureMove(_random.Next(0, board.cols), shape); }


            return new FutureMove(_random.Next(0, board.cols), shape);
        }


        private Play Negamax(int depth, Board board, PColor turn, int alpha = int.MinValue, int beta = int.MaxValue, CancellationToken cancellationToken)
        {
            Play selectedMove = new Play(null, int.MinValue);
            PColor nextTurn = (turn == PColor.Red) ? PColor.White : PColor.Red;

            if (cancellationToken.IsCancellationRequested) { return new Play(null, 0); }

            if (maxDepth <= 0) {
                int tempScore;

                if(board.CheckWinner() != Winner.None)
                {
                    tempScore = 30;


                }
                tempScore += board.winCorridors.Count;

                if(tempScore %2 != 0)
                {
                    tempScore *= -1;
                }
                return tempScore;
            }

            for (int i = board.rows; i > 0; i--)
            {
                for (int j = 0; j < board.cols; j++)
                {
                    int column = j;
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
                                move = Negamax(maxDepth, board, nextTurn, -beta, -alpha, cancellationToken);
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
                                return bestMove;
                            }

                        }

                        if (squarePieces > 0)
                        {
                            shape = PShape.Square;
                            board.DoMove(shape, j);
                            if ((board.CheckWinner() == Winner.None))
                            {
                                move = Negamax(maxDepth, board, nextTurn, -beta, -alpha, cancellationToken);
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
                                return bestMove;
                            }
                        }
                    }

                    bestMove.Score = alpha;
                    i--;

                }
            }
            return bestMove;
        }

    }

}

