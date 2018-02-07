using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordGameSolver.Models.Game;

namespace WordGameSolver.Interfaces.Game
{
    public interface ILetterRackLogic
    {
        List<List<Letter>> GenerateAllPossiblePermutations(LetterRack rack, int permutationLength);
    }
}
