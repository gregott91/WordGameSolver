using System;
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
        /// <returns>The list of potential turns</returns>
        public async Task<List<PotentialTurn>> CalculateTurnAsync(GameBoard board, LetterRack rack, ProgressReport progress)
        {
            progress.TotalCells = CountTotalFilledCells(board);
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

            Task<List<PotentialTurn>> rowTurns = CalculateTurnsForCellListsAsync(board.Cells, columns, letterRackPermutations, progress);
            Task<List<PotentialTurn>> columnTurns = CalculateTurnsForCellListsAsync(columns, board.Cells, letterRackPermutations, progress);

            List<PotentialTurn> potentialTurns = await rowTurns;
            potentialTurns.AddRange(await columnTurns);

            return potentialTurns.OrderByDescending(x => x.Points).ToList();
        }

        /// <summary>
        /// Calculates all the possible turns for lists of cells
        /// </summary>
        /// <param name="boardCellLists">The list of cells</param>
        /// <param name="perpendicularLists">The perpendicular lists. If the boardCellLists is a list of rows, this should be a list of columns</param>
        /// <param name="letterRackPermutations">The list of all possible permutations from the letter rack</param>
        /// <param name="progress"></param>
        /// <returns>The list of potential turns</returns>
        private async Task<List<PotentialTurn>> CalculateTurnsForCellListsAsync(List<List<BoardCell>> boardCellLists, List<List<BoardCell>> perpendicularLists, Dictionary<int, List<List<Letter>>> letterRackPermutations, ProgressReport progress)
        {
            List<PotentialTurn> potentialTurns = new List<PotentialTurn>();
            List<Task<List<PotentialTurn>>> calculateTasks = new List<Task<List<PotentialTurn>>>();

            int cellListIndex = 0;
            foreach (var boardCellList in boardCellLists)
            {
                calculateTasks.Add(CalculateTurnsForCellsAsync(boardCellList, perpendicularLists, cellListIndex, letterRackPermutations, progress));
                cellListIndex++;
            }

            foreach (Task<List<PotentialTurn>> task in calculateTasks)
            {
                potentialTurns.AddRange(await task);
            }

            return potentialTurns;
        }

        /// <summary>
        /// Counts the total filled cells on the board
        /// </summary>
        /// <param name="board">The game board</param>
        /// <returns>The total filled cells</returns>
        private int CountTotalFilledCells(GameBoard board)
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
        /// Calculates all potential turns for a list (row or column) of cells
        /// </summary>
        /// <param name="cells">The row or column of cells</param>
        /// <param name="perpendicularLists">The perpendicular rows or columns</param>
        /// <param name="perpendicularIndex">If this is a row, this should be the row index. Vice versa for column</param>
        /// <param name="letterRackPermutations">All possible permutations of the letter rack</param>
        /// <param name="progress">The progress</param>
        /// <returns>The list of potential turns</returns>
        private Task<List<PotentialTurn>> CalculateTurnsForCellsAsync(List<BoardCell> cells, List<List<BoardCell>> perpendicularLists, int perpendicularIndex, Dictionary<int, List<List<Letter>>> letterRackPermutations, ProgressReport progress)
        {
            return Task.Run(() =>
            {
                HashSet<char>[] invalidCharsPerCell = new HashSet<char>[cells.Count()];
                bool[] checkedSpaces = new bool[cells.Count()];

                List<PotentialTurn> potentialTurns = new List<PotentialTurn>();

                // count the empty cells. The number of empty cells is the maximum amount of letters that can be placed down
                int emptyCells = cells.Count(x => x.Letter == null);
                int maxLength = Math.Min(emptyCells, letterRackPermutations.Keys.Count());

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
        /// Calculates all the possible words that can be made for the given cells and permutations
        /// </summary>
        /// <param name="permutations">The permutations to check</param>
        /// <param name="cells">The cells to check</param>
        /// <param name="perpendicularLists">The list of perpendicular cells</param>
        /// <param name="perpendicularIndex">If checking a row, this should be the row index. Otherwise, it should be the column index.</param>
        /// <param name="checkedSpaces">The list of checked spaces</param>
        /// <param name="initialCellIndex">The initial cell that the word is being built around</param>
        /// <param name="invalidCharsPerCell">The list of invalid characters per cell</param>
        /// <param name="progress">The progress</param>
        /// <returns>The list of potential turns</returns>
        private List<PotentialTurn> CalculatePointsForWords(
            List<List<Letter>> permutations,
            List<BoardCell> cells,
            List<List<BoardCell>> perpendicularLists,
            int perpendicularIndex,
            bool[] checkedSpaces,
            int initialCellIndex,
            HashSet<char>[] invalidCharsPerCell,
            ProgressReport progress)
        {
            bool[] newCheckedSpaces = new bool[checkedSpaces.Length];

            List<PotentialTurn> potentialTurns = new List<PotentialTurn>();

            // gets the earliest cell that the words can be placed in
            int wordLength = permutations[0].Count();
            int startIndex = GetStartIndex(cells, initialCellIndex, wordLength);

            // begin placing words in all possible cells around the index to check
            for (var index = startIndex; index <= (initialCellIndex + 1); index++)
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

                // if there's not enough empty spaces, keep moving
                if (usedLetters < (wordLength - 1))
                {
                    break;
                }

                lastEmptySpaceIndex = checkIndex - 1;

                // if all the spaces have already been checked, keep moving
                if (!areAnyNotChecked)
                {
                    continue;
                }

                potentialTurns.AddRange(CheckPermutations(permutations, cells, perpendicularLists, perpendicularIndex, firstEmptySpaceIndex, lastEmptySpaceIndex, emptyIndices, invalidCharsPerCell, progress));
            }

            // record all spaces which have been checked
            for (var checkedSpaceIndex = 0; checkedSpaceIndex < newCheckedSpaces.Length; checkedSpaceIndex++)
            {
                checkedSpaces[checkedSpaceIndex] = checkedSpaces[checkedSpaceIndex] || newCheckedSpaces[checkedSpaceIndex];
            }

            return potentialTurns;
        }

        /// <summary>
        /// Checks all possible permutations for given list of empty cells
        /// </summary>
        /// <param name="permutations">The list of permutations to check</param>
        /// <param name="cells">The row/column</param>
        /// <param name="perpendicularLists">The list of perpendicular cells</param>
        /// <param name="perpendicularIndex">If checking a row, this should be the row index. Otherwise, it should be the column index.</param>
        /// <param name="emptyCellIndices">The list of empty cells to fill</param>
        /// <param name="invalidCharsPerCell">The list of invalid characters</param>
        /// <param name="progress">The progress</param>
        /// <returns>The list of potential turns</returns>
        private List<PotentialTurn> CheckPermutations(
            IEnumerable<List<Letter>> permutations,
            List<BoardCell> cells,
            List<List<BoardCell>> perpendicularLists,
            int perpendicularIndex,
            List<int> emptyCellIndices,
            HashSet<char>[] invalidCharsPerCell,
            ProgressReport progress)
        {
            List<PotentialTurn> potentialTurns = new List<PotentialTurn>();

            List<BoardCell> cellRange = GetCompleteWord(cells, emptyCellIndices[0], emptyCellIndices[emptyCellIndices.Count() - 1]);
            List<Letter> cellLetters = cellRange.Select(x => x.Letter).ToList();

            // check each permutation
            foreach (List<Letter> permutation in permutations)
            {
                int placedLetterIndex = 0;
                bool isValid = true;
                List<char> finalLetters = new List<char>();

                // for each cell that is about to be filled in
                for (var i = 0; i < cellLetters.Count(); i++)
                {
                    // if the letter is empty, place the first letter
                    if (cellLetters[i] == null)
                    {
                        char character = permutation[placedLetterIndex].Character;
                        int emptyCellIndex = emptyCellIndices[placedLetterIndex];

                        // check to see if the list of invalid characters contains this letter that's being placed
                        if (invalidCharsPerCell[emptyCellIndex] != null && invalidCharsPerCell[emptyCellIndex].Contains(character))
                        {
                            isValid = false;
                            break;
                        }

                        finalLetters.Add(character);
                        placedLetterIndex++;
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

                // check to see if the word is valid, and also if that any perpendicular cells are valid with the placed letters
                if (dictionary.SearchWord(word) && DoesFitWithPerpendicularLists(permutation, emptyCellIndices, perpendicularLists, perpendicularIndex, invalidCharsPerCell))
                {
                    progress.WordsChecked++;

                    PotentialTurn potentialTurn = new PotentialTurn()
                    {
                        Word = word,
                        Cells = cellRange,
                        Letters = permutation
                    };

                    // calculate the points
                    potentialTurn.Points = pointCalculatorLogic.CalculatePoints(potentialTurn);

                    potentialTurns.Add(potentialTurn);
                }
            }

            return potentialTurns;
        }

        /// <summary>
        /// Searches the cells to find the first and last empty cell outside of the given indices
        /// </summary>
        /// <param name="cells">The list of cells</param>
        /// <param name="startSearchIndex">The cell to start searching backward from</param>
        /// <param name="lastSearchIndex">The cell to start searching forward from</param>
        /// <returns></returns>
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

        /// <summary>
        /// Checks to see if the cell fits if the perpendicular lists
        /// </summary>
        /// <param name="letters">The letters to place</param>
        /// <param name="indices">The indices the letters will be placed at</param>
        /// <param name="perpendicularLists">The perpendicular lists</param>
        /// <param name="perpendicularIndex">The row/column index the letters are being placed at</param>
        /// <param name="invalidCharsPerCell">The list of invalid characters for each cell</param>
        /// <returns>True if it fits, false otherwise</returns>
        private bool DoesFitWithPerpendicularLists(List<Letter> letters, List<int> indices, List<List<BoardCell>> perpendicularLists, int perpendicularIndex, HashSet<char>[] invalidCharsPerCell)
        {
            int index = 0;
            foreach (var letter in letters)
            {
                int emptyIndex = indices[index];

                List<BoardCell> perpendicularList = perpendicularLists[emptyIndex];

                // if there's no perpendicular cell that's filled, skip past it
                if (HasAdjacentFilledSpace(perpendicularList, perpendicularIndex))
                {
                    // build the new word that was created with perpendicular cells
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

        /// <summary>
        /// Checks to see if the cell has an adjacent empty space
        /// </summary>
        /// <param name="cells">The list of cells</param>
        /// <param name="index">The index to check around</param>
        /// <returns>True if it has an adjacent empty space, false otherwise</returns>
        private bool HasAdjacentEmptySpace(List<BoardCell> cells, int index)
        {
            List<BoardCell> adjacentCells = GetAdjacentCells(cells, index);

            foreach (BoardCell cell in adjacentCells)
            {
                if (cell.Letter == null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks to see if the cell has an adjacent filled space
        /// </summary>
        /// <param name="cells">The list of cells</param>
        /// <param name="index">The index to check around</param>
        /// <returns>True if it has an adjacent filled space, false otherwise</returns>
        private bool HasAdjacentFilledSpace(List<BoardCell> cells, int index)
        {
            List<BoardCell> adjacentCells = GetAdjacentCells(cells, index);

            foreach (BoardCell cell in adjacentCells)
            {
                if (cell.Letter != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the list of adjacent cells
        /// </summary>
        /// <param name="cells">The list of cells</param>
        /// <param name="index">The index to check around</param>
        /// <returns>The list of adjacent cells/returns>
        private List<BoardCell> GetAdjacentCells(List<BoardCell> cells, int index)
        {
            if (index == 0)
            {
                return new List<BoardCell>()
                {
                    cells[1]
                };
            }

            if (index == (cells.Count() - 1))
            {
                return new List<BoardCell>()
                {
                    cells[cells.Count() - 2]
                };
            }

            return new List<BoardCell>()
            {
                cells[index - 1],
                cells[index + 1]
            };
        }

        /// <summary>
        /// Gets the earliest cell words can be placed in
        /// </summary>
        /// <param name="cells">The list of cells</param>
        /// <param name="startingLetterIndex">The index to start the search at</param>
        /// <param name="length">The of word that will be placed</param>
        /// <returns>The earliest starting index</returns>
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
