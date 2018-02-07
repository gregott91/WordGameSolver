using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WordGameSolver.Interfaces.Dictionary;

namespace WordGameSolver.Business.Dictionary
{
    public class EnglishDictionaryBuilder : IDictionaryBuilder
    {
        public IEnglishDictionary BuildDictionary()
        {
            List<string> words = new List<string>();

            Assembly assembly = Assembly.GetExecutingAssembly();
            using (StreamReader streamReader = new StreamReader(assembly.GetManifestResourceStream("WordGameSolver.Business.Resources.EnglishWords.txt")))
            {
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine();

                    words.Add(line);
                }
            }

            IEnglishDictionary dictionary = new EnglishDictionary();

            dictionary.AddWords(words);

            return dictionary;
        }
    }
}
