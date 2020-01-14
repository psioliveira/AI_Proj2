using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Assets.Scripts.AI.AIs.G06KiwIA
{
    class G06KiwIAThinker : IThinker
    {
        internal PShape shape;
        internal PColor color;
        private int _random;
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

            _random = 1;
            
            play = Negamax(_maxDepth, board, board.Turn, cancelationToken);

            if (play.Position != null) { return new FutureMove(Convert.ToInt32(play.Position), PShape.Round); }

            else { return new FutureMove(_random, shape); }
        }


        private Play Negamax(int depth, Board board, PColor turn, CancellationToken cancellationToken, int alpha = int.MinValue, int beta = int.MaxValue)
        {
            Play selectedMove = new Play(null, int.MinValue);
            PColor nextTurn = (turn == PColor.Red) ? PColor.White : PColor.Red;

            if (cancellationToken.IsCancellationRequested) { return new Play(null, 0); }

            if (_maxDepth <= 0) {
                int tempScore = default;

                if(board.CheckWinner() != Winner.None)
                {
                    tempScore = 30;


                }
                tempScore += board.winCorridors.Count();

                if(tempScore %2 != 0)
                {
                    tempScore *= -1;
                }
                selectedMove.Score = tempScore;


                return selectedMove;
            }
            for (int j = 0; j < board.cols; j++)
            {
                int column = j;
                for (int i= board.rows-1 ; i >= 0; i--)
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
                                move = Negamax(depth-1, board, nextTurn, cancellationToken, -beta, -alpha);
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
                                return bestMove;
                            }
                        }
                    }

                    bestMove.Score = alpha;
                    j++;

                }
            }
            return bestMove;
        }

    }

}

