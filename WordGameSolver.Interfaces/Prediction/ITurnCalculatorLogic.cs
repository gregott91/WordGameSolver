using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordGameSolver.Models.Board;
using WordGameSolver.Models.Game;
using WordGameSolver.Models.Prediction;

namespace WordGameSolver.Interfaces.Prediction
{
    public interface ITurnCalculatorLogic
    {
        Task<List<PotentialTurn>> CalculateTurnAsync(GameBoard board, LetterRack rack, ProgressReport progress);
    }
}
