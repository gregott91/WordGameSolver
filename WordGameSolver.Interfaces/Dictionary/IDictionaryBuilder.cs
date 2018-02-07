using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordGameSolver.Interfaces.Dictionary
{
    public interface IDictionaryBuilder
    {
        IEnglishDictionary BuildDictionary();
    }
}
