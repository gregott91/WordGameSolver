using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordGameSolver.Interfaces.Dictionary
{
    public interface IEnglishDictionary
    {
        bool SearchWord(string word);

        void AddWord(string word);

        void AddWords(IEnumerable<string> word);
    }
}
