using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordGameSolver.Interfaces.Board;
using WordGameSolver.Interfaces.Dictionary;
using WordGameSolver.Interfaces.Game;
using WordGameSolver.Interfaces.Prediction;

namespace WordGameSolver.Interfaces
{
    public interface ILogicFactory
    {
        IDictionaryBuilder GetDictionaryBuilder();

        IGameBoardBuilder GetBoardBuilder();

        ITurnCalculatorLogic GetTurnCalculator();

        IPointCalculatorLogic GetPointCalculator();

        ILetterBagLogic GetLetterBagLogic();

        ILetterBagBuilder GetLetterBagBuilder();

        ILetterRackLogic GetLetterRackLogic();

        IGameBoardLogic GetGameBoardLogic();
    }
}
