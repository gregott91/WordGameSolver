using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordGameSolver.Interfaces.Game;
using WordGameSolver.Models.Game;

namespace WordGameSolver.Business.Game
{
    public class ScrabbleLetterBagBuilder : ILetterBagBuilder
    {
        public List<Letter> GenerateLetterBag()
        {
            List<Tuple<char, int, int>> letterCounts = new List<Tuple<char, int, int>>()
            {
                Tuple.Create('*', 0, 2),
                Tuple.Create('a', 1, 9),
                Tuple.Create('b', 3, 2),
                Tuple.Create('c', 3, 2),
                Tuple.Create('d', 2, 4),
                Tuple.Create('e', 1, 12),
                Tuple.Create('f', 4, 2),
                Tuple.Create('g', 2, 3),
                Tuple.Create('h', 4, 2),
                Tuple.Create('i', 1, 9),
                Tuple.Create('j', 8, 1),
                Tuple.Create('k', 5, 1),
                Tuple.Create('l', 1, 4),
                Tuple.Create('m', 3, 2),
                Tuple.Create('n', 1, 6),
                Tuple.Create('o', 1, 8),
                Tuple.Create('p', 3, 2),
                Tuple.Create('q', 10, 1),
                Tuple.Create('r', 1, 6),
                Tuple.Create('s', 1, 4),
                Tuple.Create('t', 1, 6),
                Tuple.Create('u', 1, 4),
                Tuple.Create('v', 4, 2),
                Tuple.Create('w', 4, 2),
                Tuple.Create('x', 8, 1),
                Tuple.Create('y', 4, 2),
                Tuple.Create('z', 10, 1)
            };

            List<Letter> letters = new List<Letter>();

            foreach (Tuple<char, int, int> letterCount in letterCounts)
            {
                for (int i = 0; i < letterCount.Item3; i++)
                {
                    letters.Add(new Letter(letterCount.Item1, letterCount.Item2));
                }
            }

            return letters;
        }
    }
}
