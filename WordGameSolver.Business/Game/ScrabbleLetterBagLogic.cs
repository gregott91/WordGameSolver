using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordGameSolver.Interfaces.Game;
using WordGameSolver.Models.Game;

namespace WordGameSolver.Business.Game
{
    public class ScrabbleLetterBagLogic : ILetterBagLogic
    {
        private readonly Random random = new Random();
        private const int letterRackSize = 7;
        private List<Letter> letters;

        public ScrabbleLetterBagLogic(List<Letter> letters)
        {
            this.letters = letters;
        }

        public LetterRack GetInitialLetterRack()
        {
            LetterRack rack = new LetterRack()
            {
                Letters = new List<Letter>()
            };

            ReplaceLetters(rack);

            return rack;
        }

        public void ReplaceLetters(LetterRack rack)
        {
            for (int i = rack.Letters.Count; i < letterRackSize; i++)
            {
                rack.Letters.Add(GetRandomLetter());
            }
        }

        private Letter GetRandomLetter()
        {
            int randomIndex = random.Next(letters.Count() - 1);
            letters.RemoveAt(randomIndex);

            return letters[randomIndex];
        }
    }
}
