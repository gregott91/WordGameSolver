using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordGameSolver.Models.Game;
using WordGameSolver.Models.Prediction;

namespace WordGameSolver.Interfaces.Game
{
    public interface IPointCalculatorLogic
    {
        int CalculatePoints(PotentialTurn turn);
    }
}
