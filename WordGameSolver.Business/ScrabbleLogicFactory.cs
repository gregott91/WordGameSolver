using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordGameSolver.Business.Board;
using WordGameSolver.Business.Dictionary;
using WordGameSolver.Business.Game;
using WordGameSolver.Business.Prediction;
using WordGameSolver.Interfaces;
using WordGameSolver.Interfaces.Board;
using WordGameSolver.Interfaces.Dictionary;
using WordGameSolver.Interfaces.Game;
using WordGameSolver.Interfaces.Prediction;

namespace WordGameSolver.Business
{
    public class ScrabbleLogicFactory : ILogicFactory
    {
        public IDictionaryBuilder GetDictionaryBuilder()
        {
            return new EnglishDictionaryBuilder();
        }

        public IGameBoardBuilder GetBoardBuilder()
        {
            return new ScrabbleBoardBuilder();
        }

        public IPointCalculatorLogic GetPointCalculator()
        {
            return new ScrabblePointCalculatorLogic();
        }

        public ITurnCalculatorLogic GetTurnCalculator()
        {
            IDictionaryBuilder dictionaryBuilder = GetDictionaryBuilder();
            IEnglishDictionary dictionary = dictionaryBuilder.BuildDictionary();
            IPointCalculatorLogic pointCalculator = GetPointCalculator();
            ILetterRackLogic rackLogic = GetLetterRackLogic();
            IGameBoardLogic boardLogic = GetGameBoardLogic();

            return new ScrabbleTurnCalculatorLogic(dictionary, pointCalculator, boardLogic, rackLogic);
        }

        public ILetterBagBuilder GetLetterBagBuilder()
        {
            return new ScrabbleLetterBagBuilder();
        }

        public ILetterBagLogic GetLetterBagLogic()
        {
            ILetterBagBuilder bagBuilder = GetLetterBagBuilder();

            return new ScrabbleLetterBagLogic(bagBuilder.GenerateLetterBag());
        }

        public ILetterRackLogic GetLetterRackLogic()
        {
            return new ScrabbleLetterRackLogic();
        }

        public IGameBoardLogic GetGameBoardLogic()
        {
            return new ScrabbleBoardLogic();
        }
    }
}
