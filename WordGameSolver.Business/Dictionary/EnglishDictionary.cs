using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WordGameSolver.Interfaces.Dictionary;

namespace WordGameSolver.Business.Dictionary
{
    public class EnglishDictionary : IEnglishDictionary
    {
        private HashSet<string> dictionaryWords;

        public EnglishDictionary()
        {
            dictionaryWords = new HashSet<string>();
        }

        public bool SearchWord(string word)
        {
            string lowerWord = word.ToLower();

            if (!ValidateWord(lowerWord))
            {
                return false;
            }

            IEnumerable<string> searchWords = GenerateSearchWords(lowerWord);

            foreach (var searchWord in searchWords)
            {
                if (dictionaryWords.Contains(searchWord))
                {
                    return true;
                }
            }

            return false;
        }

        public void AddWords(IEnumerable<string> words)
        {
            foreach (var word in words)
            {
                AddWord(word);
            }
        }

        public void AddWord(string word)
        {
            if (!ValidateWord(word))
            {
                return;
            }

            dictionaryWords.Add(word);
        }

        private bool ValidateWord(string word)
        {
            string pattern = @"^[a-z]*$";

            return Regex.IsMatch(word, pattern);
        }

        private IEnumerable<string> GenerateSearchWords(string searchWord)
        {
            char wildcardChar = '*';

            if (!searchWord.Contains(wildcardChar))
            {
                return new List<string>()
                {
                    searchWord
                };
            }

            List<List<char>> charList = new List<List<char>>();

            foreach (char wordChar in searchWord)
            {
                if (wordChar == wildcardChar)
                {
                    for (var i = 0; i < 26; i++)
                    {
                        AppendLetter(charList, (char)(i + 97));
                    }
                }
                else
                {
                    AppendLetter(charList, wordChar);
                }
            }

            return charList.Select(x => new string(x.ToArray()));
        }

        private void AppendLetter(List<List<char>> charList, char letter)
        {
            foreach (var list in charList)
            {
                list.Add(letter);
            }
        }
    }
}