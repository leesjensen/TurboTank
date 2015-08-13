using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboTank
{
    public class Beam
    {
        public int CandidateCount = 1;
        public TankAction StartAction;
        private const int MaxSize = 10;
        EvalState[] bestStates = new EvalState[MaxSize];
        List<EvalState> candidates = new List<EvalState>();

        public Beam(TankOperation operation, Grid grid, SignalWeights weights)
        {
            StartAction = operation.GetAction();
            EvalState startingState = new EvalState(operation.GetAction(), grid);
            bestStates[0] = operation.GetScore(startingState, weights);
        }

        public void Add(EvalState candidateState)
        {
            CandidateCount++;
            if (candidateState.Score >= 0)
            {
                candidates.Add(candidateState);
            }
        }

        public void Evaluate()
        {
            foreach (var candidateState in candidates)
            {
                if (bestStates[MaxSize - 1] == null || candidateState.Score > bestStates[MaxSize - 1].Score)
                {
                    for (int pos = 0; pos < MaxSize; pos++)
                    {
                        if (bestStates[pos] == null)
                        {
                            bestStates[pos] = candidateState;
                            break;
                        }
                        else if (candidateState.Score > bestStates[pos].Score)
                        {
                            for (int movePos = MaxSize - 1; movePos > pos; movePos--)
                            {
                                bestStates[movePos] = bestStates[movePos - 1];
                            }

                            bestStates[pos] = candidateState;
                            break;
                        }
                    }
                }
            }

            candidates.Clear();
        }

        public IEnumerable<EvalState> Iterate()
        {
            for (int pos = 0; pos < MaxSize; pos++)
            {
                if (bestStates[pos] != null && !bestStates[pos].Evaluated)
                {
                    bestStates[pos].Evaluated = true;

                    yield return bestStates[pos];
                }
            }
        }

        public EvalState GetBest()
        {
            return bestStates[0];
        }

        public override string ToString()
        {
            return bestStates[0].ToString();
        }
    }
}
