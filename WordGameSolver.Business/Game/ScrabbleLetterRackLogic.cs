using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            HashSet<string> usedWords = new HashSet<string>();
            List<List<Letter>> permutations = Permutate(rack.Letters, permutationLength);
            List<List<Letter>> finalPermutations = new List<List<Letter>>();

            foreach (var permutation in permutations)
            {
                string word = new string(permutation.Select(x => x.Character).ToArray());

                if (!usedWords.Contains(word))
                {
                    finalPermutations.Add(permutation);
                    usedWords.Add(word);
                }
            }

            return finalPermutations;
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

                foreach (var characterPermutation in characterPermutations)
                {
                    permutations.Add(characterPermutation);
                }
            }

            return permutations;
        }
    }
}
