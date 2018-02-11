﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordGameSolver.Interfaces.Board;
using WordGameSolver.Interfaces.Dictionary;
using WordGameSolver.Interfaces.Game;
using WordGameSolver.Interfaces.Prediction;
using WordGameSolver.Models.Board;
using WordGameSolver.Models.Game;
using WordGameSolver.Models.Prediction;

namespace WordGameSolver.Business.Prediction
{
    public class ScrabbleTurnCalculatorLogic : ITurnCalculatorLogic
    {
        private IEnglishDictionary dictionary;
        private IPointCalculatorLogic pointCalculatorLogic;
        private IGameBoardLogic gameBoardLogic;
        private ILetterRackLogic letterRackLogic;

        public ScrabbleTurnCalculatorLogic(IEnglishDictionary dictionary, IPointCalculatorLogic pointCalculatorLogic, IGameBoardLogic gameBoardLogic, ILetterRackLogic letterRackLogic)
        {
            this.dictionary = dictionary;
            this.pointCalculatorLogic = pointCalculatorLogic;
            this.gameBoardLogic = gameBoardLogic;
            this.letterRackLogic = letterRackLogic;
        }

        /// <summary>
        /// Calculates list of potential turns to make
        /// </summary>
        /// <param name="board"></param>
        /// <param name="rack"></param>
        /// <returns></returns>
        public async Task<List<PotentialTurn>> CalculateTurnAsync(GameBoard board, LetterRack rack, ProgressReport progress)
        {
            progress.TotalCells = CountTotalCells(board);
            progress.CellsChecked = 0;
            progress.WordsChecked = 0;
            progress.IsProgressInitialized = true;

            Dictionary<int, List<List<Letter>>> letterRackPermutations = new Dictionary<int, List<List<Letter>>>();

            // generate the list of potential turns for each possible length of string
            for (var length = 1; length <= rack.Letters.Count(); length++)
            {
                List<List<Letter>> words = letterRackLogic.GenerateAllPossiblePermutations(rack, length);

                letterRackPermutations.Add(length, words);
            }

            List<List<BoardCell>> columns = gameBoardLogic.GetColumnsFromRows(board);

            // fix
            List<PotentialTurn> rowTurns = await CalculateTurnsForCellListsAsync(board.Cells, columns, rack, letterRackPermutations, progress);
            List<PotentialTurn> columnTurns = await CalculateTurnsForCellListsAsync(columns, board.Cells, rack, letterRackPermutations, progress);

            List<PotentialTurn> potentialTurns = rowTurns;
            potentialTurns.AddRange(columnTurns);

            return potentialTurns.OrderByDescending(x => x.Points).ToList();
        }

        private async Task<List<PotentialTurn>> CalculateTurnsForCellListsAsync(List<List<BoardCell>> boardCellLists, List<List<BoardCell>> perpendicularLists, LetterRack rack, Dictionary<int, List<List<Letter>>> letterRackPermutations, ProgressReport progress)
        {
            List<PotentialTurn> potentialTurns = new List<PotentialTurn>();
            List<Task<List<PotentialTurn>>> calculateTasks = new List<Task<List<PotentialTurn>>>();

            int cellListIndex = 0;
            foreach (var boardCellList in boardCellLists)
            {
                potentialTurns.AddRange(await CalculateTurnsForCellsAsync(boardCellList, perpendicularLists, cellListIndex, rack, letterRackPermutations, progress));
                cellListIndex++;
            }

            //foreach (Task<List<PotentialTurn>> task in calculateTasks)
            //{
            //    potentialTurns.AddRange(await task);
            //}

            return potentialTurns;
        }

        private List<List<BoardCell>> PartitionBoardCell(List<BoardCell> cells, int partitionSize)
        {
            List<List<BoardCell>> partitions = new List<List<BoardCell>>();

            int partitionStartIndex = 0;
            foreach (var cell in cells)
            {
                if (cell.Letter != null)
                {
                    partitionStartIndex = Math.Max(partitionStartIndex - partitionSize, 0);
                    break;
                }

                partitionStartIndex++;
            }

            bool hasFilledCell = false;
            int contiguousEmptyCells = 0;
            List<BoardCell> partition = new List<BoardCell>();
            for (var cellIndex = 0; cellIndex < cells.Count(); cellIndex++)
            {
                BoardCell cell = cells[cellIndex];
                if (cell.Letter == null)
                {
                    contiguousEmptyCells++;
                }
                else
                {
                    hasFilledCell = true;
                }

                if (contiguousEmptyCells > partitionSize)
                {
                    if (hasFilledCell)
                    {
                        partitions.Add(partition);
                    }

                    partition = new List<BoardCell>();
                    contiguousEmptyCells = 1;
                    hasFilledCell = false;
                }
                else
                {
                    partition.Add(cell);
                }
            }

            if (hasFilledCell)
            {
                partitions.Add(partition);
            }

            return partitions;
        }

        private int CountTotalCells(GameBoard board)
        {
            int count = 0;

            foreach (var row in board.Cells)
            {
                foreach (var cell in row)
                {
                    if (cell.Letter != null)
                    {
                        count++;
                    }
                }
            }

            // double the cells to account for checking vertical and horizontal
            return count * 2;
        }

        /// <summary>
        /// Calculates all potential turns for a list of cells
        /// </summary>
        /// <param name="cells"></param>
        /// <param name="rack"></param>
        /// <param name="checkedSpaces"></param>
        /// <returns></returns>
        private Task<List<PotentialTurn>> CalculateTurnsForCellsAsync(List<BoardCell> cells, List<List<BoardCell>> perpendicularLists, int perpendicularIndex, LetterRack rack, Dictionary<int, List<List<Letter>>> letterRackPermutations, ProgressReport progress)
        {
            return Task.Run(() =>
            {
                HashSet<char>[] invalidCharsPerCell = new HashSet<char>[cells.Count()];
                bool[] checkedSpaces = new bool[cells.Count()];

                List<PotentialTurn> potentialTurns = new List<PotentialTurn>();

                // count the empty cells. The number of empty cells is the maximum amount of letters that can be placed down
                int emptyCells = cells.Count(x => x.Letter == null);
                int maxLength = Math.Min(emptyCells, rack.Letters.Count());

                // find all the cells which are not empty and have an adjacent empty cell, then build words around those cells
                for (var originalIndex = 0; originalIndex < cells.Count(); originalIndex++)
                {
                    BoardCell cell = cells[originalIndex];

                    if (cell.Letter == null)
                    {
                        continue;
                    }

                    if (!HasAdjacentEmptySpace(cells, originalIndex))
                    {
                        // if the cell has a letter, mark it as checked
                        progress.CellsChecked++;
                        continue;
                    }

                    // generate the list of potential turns for each possible length of string
                    for (var length = 1; length <= maxLength; length++)
                    {
                        List<List<Letter>> words = letterRackPermutations[length];
                        potentialTurns.AddRange(CalculatePointsForWords(words, cells, perpendicularLists, perpendicularIndex, checkedSpaces, originalIndex, invalidCharsPerCell, progress));
                    }

                    progress.CellsChecked++;
                }

                return potentialTurns;
            });
        }

        /// <summary>
        /// Checks the points of all the permutations of a string for a given string length
        /// </summary>
        /// <param name="cells"></param>
        /// <param name="rack"></param>
        /// <param name="checkedSpaces"></param>
        /// <param name="wordLength"></param>
        /// <param name="originalIndex"></param>
        /// <returns></returns>
        private List<PotentialTurn> CalculatePointsForWords(
            List<List<Letter>> words,
            List<BoardCell> cells,
            List<List<BoardCell>> perpendicularLists,
            int perpendicularIndex,
            bool[] checkedSpaces,
            int originalIndex,
            HashSet<char>[] invalidCharsPerCell,
            ProgressReport progress)
        {
            bool[] newCheckedSpaces = new bool[checkedSpaces.Length];

            List<PotentialTurn> potentialTurns = new List<PotentialTurn>();

            // gets the earliest cell that the words can be placed in
            int wordLength = words[0].Count();
            int startIndex = GetStartIndex(cells, originalIndex, wordLength);

            // begin placing words in all possible cells around the index to check
            for (var index = startIndex; index <= (originalIndex + 1); index++)
            {
                int usedLetters = 0;
                int firstEmptySpaceIndex = -1;
                int lastEmptySpaceIndex;
                bool areAnyNotChecked = false;
                List<int> emptyIndices = new List<int>();

                // check to see if there are enough empty spaces to fill in the given word
                int checkIndex = index;
                while (usedLetters < wordLength && checkIndex < cells.Count())
                {
                    if (cells[checkIndex].Letter == null)
                    {
                        emptyIndices.Add(checkIndex);
                        usedLetters++;

                        if (firstEmptySpaceIndex == -1)
                        {
                            firstEmptySpaceIndex = checkIndex;
                        }

                        if (!checkedSpaces[checkIndex])
                        {
                            areAnyNotChecked = true;
                        }
                    }

                    newCheckedSpaces[checkIndex] = true;

                    checkIndex++;
                }

                if (usedLetters < (wordLength - 1))
                {
                    break;
                }

                lastEmptySpaceIndex = checkIndex - 1;

                if (!areAnyNotChecked)
                {
                    continue;
                }

                potentialTurns.AddRange(CheckPermutations(words, cells, perpendicularLists, perpendicularIndex, firstEmptySpaceIndex, lastEmptySpaceIndex, emptyIndices, invalidCharsPerCell, progress));
            }

            // record all spaces which have been checked
            for (var checkedSpaceIndex = 0; checkedSpaceIndex < newCheckedSpaces.Length; checkedSpaceIndex++)
            {
                checkedSpaces[checkedSpaceIndex] = checkedSpaces[checkedSpaceIndex] || newCheckedSpaces[checkedSpaceIndex];
            }

            return potentialTurns;
        }

        private List<PotentialTurn> CheckPermutations(
            IEnumerable<List<Letter>> permutations,
            List<BoardCell> cells,
            List<List<BoardCell>> perpendicularLists,
            int perpendicularIndex,
            int firstEmptySpaceIndex,
            int lastEmptySpaceIndex,
            List<int> emptyCellIndices,
            HashSet<char>[] invalidCharsPerCell,
            ProgressReport progress)
        {
            List<PotentialTurn> potentialTurns = new List<PotentialTurn>();

            List<BoardCell> cellRange = GetCompleteWord(cells, firstEmptySpaceIndex, lastEmptySpaceIndex);
            List<Letter> cellLetters = cellRange.Select(x => x.Letter).ToList();

            foreach (List<Letter> permutation in permutations)
            {
                int letterIndex = 0;
                List<char> finalLetters = new List<char>();
                bool isValid = true;
                for (var i = 0; i < cellLetters.Count(); i++)
                {
                    if (cellLetters[i] == null)
                    {
                        char character = permutation[letterIndex].Character;
                        int emptyCellIndex = emptyCellIndices[letterIndex];

                        if (invalidCharsPerCell[emptyCellIndex] != null && invalidCharsPerCell[emptyCellIndex].Contains(character))
                        {
                            isValid = false;
                            break;
                        }

                        finalLetters.Add(character);
                        letterIndex++;
                    }
                    else
                    {
                        finalLetters.Add((char)cellLetters[i].Character);
                    }
                }

                if (!isValid)
                {
                    continue;
                }

                string word = new string(finalLetters.ToArray());

                if (dictionary.SearchWord(word) && DoesFitWithPerpendicularLists(permutation, emptyCellIndices, perpendicularLists, perpendicularIndex, invalidCharsPerCell))
                {
                    progress.WordsChecked++;

                    PotentialTurn potentialTurn = new PotentialTurn()
                    {
                        Word = word,
                        Cells = cellRange,
                        Letters = permutation
                    };

                    potentialTurn.Points = pointCalculatorLogic.CalculatePoints(potentialTurn);

                    potentialTurns.Add(potentialTurn);
                }
            }

            return potentialTurns;
        }

        private List<BoardCell> GetCompleteWord(List<BoardCell> cells, int startSearchIndex, int lastSearchIndex)
        {
            int startIndex = 0;
            int endIndex = cells.Count() - 1;

            for (var index = startSearchIndex - 1; index >= 0; index--)
            {
                if (cells[index].Letter == null)
                {
                    startIndex = index + 1;
                    break;
                }
            }

            for (var index = lastSearchIndex + 1; index < cells.Count(); index++)
            {
                if (cells[index].Letter == null)
                {
                    endIndex = index - 1;
                    break;
                }
            }

            List<BoardCell> cellRange = cells.GetRange(startIndex, endIndex - startIndex + 1);

            return cellRange;
        }

        private bool DoesFitWithPerpendicularLists(List<Letter> letters, List<int> indices, List<List<BoardCell>> perpendicularLists, int perpendicularIndex, HashSet<char>[] invalidCharsPerCell)
        {
            int index = 0;
            foreach (var letter in letters)
            {
                int emptyIndex = indices[index];

                List<BoardCell> perpendicularList = perpendicularLists[emptyIndex];

                if (HasAdjacentFilledSpace(perpendicularList, emptyIndex))
                {
                    List<BoardCell> perpendicularCells = GetCompleteWord(perpendicularList, perpendicularIndex, perpendicularIndex);
                    char[] perpendicularLetters = perpendicularCells.Select(x => x.Letter != null ? x.Letter.Character : letter.Character).ToArray();
                    string perpendicularWord = new string(perpendicularLetters);

                    if (!dictionary.SearchWord(perpendicularWord))
                    {
                        if (invalidCharsPerCell[emptyIndex] == null)
                        {
                            invalidCharsPerCell[emptyIndex] = new HashSet<char>();
                        }

                        invalidCharsPerCell[emptyIndex].Add(letter.Character);

                        return false;
                    }
                }

                index++;
            }

            return true;
        }

        private bool HasAdjacentEmptySpace(List<BoardCell> cells, int index)
        {
            return HasAdjacentSpaceOfType(cells, index, true);
        }

        private bool HasAdjacentFilledSpace(List<BoardCell> cells, int index)
        {
            return HasAdjacentSpaceOfType(cells, index, false);
        }

        private bool HasAdjacentSpaceOfType(List<BoardCell> cells, int index, bool hasEmpty)
        {
            if (index == 0)
            {
                if (cells[1].Letter != null)
                {
                    return !hasEmpty;
                }
                else
                {
                    return hasEmpty;
                }
            }

            if (index == (cells.Count() - 1))
            {
                if (cells[cells.Count() - 2].Letter != null)
                {
                    return !hasEmpty;
                }
                else
                {
                    return hasEmpty;
                }
            }

            if (hasEmpty && (cells[index - 1].Letter == null || cells[index + 1].Letter == null))
            {
                return true;
            }

            if (!hasEmpty && (cells[index - 1].Letter != null || cells[index + 1].Letter != null))
            {
                return true;
            }

            return false;
        }

        private int GetStartIndex(List<BoardCell> cells, int startingLetterIndex, int length)
        {
            int startIndex = -1;

            int emptySpaces = 0;
            for (int i = (startingLetterIndex - 1); i >= 0; i--)
            {
                if (cells[i].Letter == null)
                {
                    emptySpaces++;
                    startIndex = i;
                }

                if (emptySpaces == length)
                {
                    return startIndex;
                }
            }

            if (startIndex == -1)
            {
                return startingLetterIndex;
            }

            return startIndex;
        }
    }
}
