using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordGameSolver.Interfaces.Game;
using WordGameSolver.Models.Game;

namespace WordGameSolver.Business.Game
{
    public class ScrabbleLetterRackLogic : ILetterRackLogic
    {
        public List<List<Letter>> GenerateAllPossiblePermutations(LetterRack rack, int permutationLength)
        {
            List<List<Letter>> permutations = Permutate(rack.Letters, permutationLength);

            return permutations;
        }

        private List<List<Letter>> GenerateAllPossiblePermutations(List<Letter> characters, int length)
        {
            List<List<Letter>> permutations = Permutate(characters, length);

            return permutations;
        }

        private List<List<Letter>> Permutate(List<Letter> characters, int length)
        {
            if (length == 1)
            {
                List<List<Letter>> results = new List<List<Letter>>();

                foreach (Letter c in characters)
                {
                    results.Add(new List<Letter>() { c });
                }

                return results;
            }

            List<List<Letter>> permutations = new List<List<Letter>>();

            for (var i = 0; i < characters.Count(); i++)
            {
                Letter character = characters.ElementAt(i);

                List<Letter> newCharacters = new List<Letter>(characters);
                newCharacters.RemoveAt(i);

                List<List<Letter>> characterPermutations = Permutate(newCharacters, length - 1);

                foreach (List<Letter> permutation in characterPermutations)
                {
                    permutation.Insert(0, character);
                }

                permutations.AddRange(characterPermutations);
            }

            return permutations;
        }
    }
}
