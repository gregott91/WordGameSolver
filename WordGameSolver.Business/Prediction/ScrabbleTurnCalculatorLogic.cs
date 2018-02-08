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

            //List<PotentialTurn> rowTurns = await CalculateTurnsForCellListsAsync(board.Cells, rack, letterRackPermutations, progress);

            //List<List<BoardCell>> columns = gameBoardLogic.GetColumnsFromRows(board);
            //List<PotentialTurn> columnTurns = await CalculateTurnsForCellListsAsync(columns, rack, letterRackPermutations, progress);

            //List<PotentialTurn> potentialTurns = rowTurns;
            //potentialTurns.AddRange(columnTurns);

            Task<List<PotentialTurn>> rowTurns = CalculateTurnsForCellListsAsync(board.Cells, rack, letterRackPermutations, progress);

            List<List<BoardCell>> columns = gameBoardLogic.GetColumnsFromRows(board);
            Task<List<PotentialTurn>> columnTurns = CalculateTurnsForCellListsAsync(columns, rack, letterRackPermutations, progress);

            List<PotentialTurn> potentialTurns = await rowTurns;
            potentialTurns.AddRange(await columnTurns);

            return potentialTurns.OrderByDescending(x => x.Points).ToList();
        }

        private async Task<List<PotentialTurn>> CalculateTurnsForCellListsAsync(List<List<BoardCell>> boardCellLists, LetterRack rack, Dictionary<int, List<List<Letter>>> letterRackPermutations, ProgressReport progress)
        {
            List<PotentialTurn> potentialTurns = new List<PotentialTurn>();
            List<Task<List<PotentialTurn>>> calculateTasks = new List<Task<List<PotentialTurn>>>();

            //foreach (var boardCellList in boardCellLists)
            //{
            //    potentialTurns.AddRange(await CalculateTurnsForCellsAsync(boardCellList, rack, letterRackPermutations, progress));
            //}

            foreach (var boardCellList in boardCellLists)
            {
                calculateTasks.Add(CalculateTurnsForCellsAsync(boardCellList, rack, letterRackPermutations, progress));
            }

            foreach (Task<List<PotentialTurn>> task in calculateTasks)
            {
                potentialTurns.AddRange(await task);
            }

            return potentialTurns;
        }

        private List<List<BoardCell>> PartitionBoardCells(List<BoardCell> cells, int partitionSize)
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
            for (var cellIndex = partitionStartIndex; cellIndex < cells.Count(); cellIndex++)
            {
                BoardCell cell = cells[cellIndex];
                if (cell.Letter == null)
                {
                    contiguousEmptyCells++;
                }
                else
                {
                    hasFilledCell = true;
                    contiguousEmptyCells = 0;
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
        private async Task<List<PotentialTurn>> CalculateTurnsForCellsAsync(List<BoardCell> cells, LetterRack rack, Dictionary<int, List<List<Letter>>> letterRackPermutations, ProgressReport progress)
        {
            List<PotentialTurn> potentialTurns = new List<PotentialTurn>();
            List<Task<List<PotentialTurn>>> turnTasks = new List<Task<List<PotentialTurn>>>();

            // count the empty cells. The number of empty cells is the maximum amount of letters that can be placed down
            int emptyCells = cells.Count(x => x.Letter == null);
            int maxLength = Math.Min(emptyCells, rack.Letters.Count());

            if (emptyCells == cells.Count())
            {
                return potentialTurns;
            }

            // generate the list of potential turns for each possible length of string
            for (var length = 1; length <= maxLength; length++)
            {
                List<List<Letter>> words = letterRackPermutations[length];
                List<List<BoardCell>> partitions = PartitionBoardCells(cells, length);

                //foreach (var partition in partitions)
                //{
                //    potentialTurns.AddRange(await CalculatePointsForWordsAsync(words, partition, progress));
                //}

                foreach (var partition in partitions)
                {
                    turnTasks.Add(CalculatePointsForWordsAsync(words, partition, progress));
                }
            }

            foreach (var turnTask in turnTasks)
            {
                potentialTurns.AddRange(await turnTask);
            }

            return potentialTurns;
        }

        /// <summary>
        /// Checks the points of all the permutations of a string for a given string length
        /// </summary>
        /// <param name="cells"></param>
        /// <param name="rack"></param>
        /// <param name="checkedSpaces"></param>
        /// <param name="wordLength"></param>
        /// <returns></returns>
        private Task<List<PotentialTurn>> CalculatePointsForWordsAsync(List<List<Letter>> words, List<BoardCell> cells, ProgressReport progress)
        {
            return Task.Run(() =>
            {
                List<PotentialTurn> potentialTurns = new List<PotentialTurn>();

                int wordLength = words[0].Count();

                int startIndex = 0;
                int endIndex = 0;
                int emptyCells = 0;
                foreach (var cell in cells)
                {
                    if (cell.Letter == null)
                    {
                        emptyCells++;
                    }

                    if (emptyCells > wordLength)
                    {
                        endIndex--;
                        break;
                    }

                    endIndex++;
                }

                while (endIndex < cells.Count())
                {
                    potentialTurns.AddRange(CheckPermutations(words, cells, startIndex, endIndex, progress));

                    startIndex = GetNextIndex(cells, startIndex);
                    endIndex = GetNextIndex(cells, endIndex);
                }

                return potentialTurns;
            });
        }

        private int GetNextIndex(List<BoardCell> cells, int index)
        {
            if (cells[index].Letter == null)
            {
                return index + 1;
            }

            while (index < cells.Count())
            {
                if (cells[index].Letter == null)
                {
                    return index;
                }

                index++;
            }

            return index;
        }

        private List<PotentialTurn> CheckPermutations(IEnumerable<List<Letter>> permutations, List<BoardCell> cells, int startIndex, int endIndex, ProgressReport progress)
        {
            List<PotentialTurn> potentialTurns = new List<PotentialTurn>();

            List<BoardCell> cellRange = cells.GetRange(startIndex, endIndex - startIndex + 1);
            Letter[] cellLetters = cellRange.Select(x => x.Letter).ToArray();

            foreach (List<Letter> permutation in permutations)
            {
                int letterIndex = 0;
                List<char> finalLetters = new List<char>();
                for (var i = 0; i < cellLetters.Length; i++)
                {
                    if (cellLetters[i] == null)
                    {
                        finalLetters.Add(permutation[letterIndex].Character);
                        letterIndex++;
                    }
                    else
                    {
                        finalLetters.Add((char)cellLetters[i].Character);
                    }
                }

                string word = new string(finalLetters.ToArray());

                if (dictionary.SearchWord(word))
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
    }
}
