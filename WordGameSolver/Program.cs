using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WordGameSolver.Business;
using WordGameSolver.Interfaces.Prediction;
using WordGameSolver.Models.Game;
using WordGameSolver.Models.Prediction;

namespace WordGameSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ScrabbleLogicFactory();

            var turnCalculator = factory.GetTurnCalculator();
            var board = factory.GetBoardBuilder().BuildBoard();
            var bagLogic = factory.GetLetterBagLogic();
            var bag = factory.GetLetterBagBuilder().GenerateLetterBag();

            var csvFile = ReadCsvFile();

            for (int rowIndex = 0; rowIndex < csvFile.Count(); rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < csvFile[rowIndex].Count; columnIndex++)
                {
                    if (!string.IsNullOrWhiteSpace(csvFile[rowIndex][columnIndex]))
                    {
                        char character = csvFile[rowIndex][columnIndex][0];

                        var letter = bag.First(x => x.Character == character);

                        board.Cells[rowIndex][columnIndex].Letter = letter;
                    }
                }
            }

            var chars = new List<Letter>()
            {
                bag.First(x => x.Character == 't'),
                bag.First(x => x.Character == 't'),
                bag.First(x => x.Character == 'i'),
                bag.First(x => x.Character == 'n'),
                bag.First(x => x.Character == 'j'),
                bag.First(x => x.Character == 'i'),
                bag.First(x => x.Character == 'm')
            };

            Stopwatch watch = new Stopwatch();
            watch.Start();
            ProgressReport progress = new ProgressReport();

            var turnTask = turnCalculator.CalculateTurnAsync(board, new LetterRack()
            {
                Letters = chars
            }, progress);

            while (!progress.IsProgressInitialized) { }

            while (!turnTask.IsCompleted)
            {
                //double percentDone = progress.CellsChecked / (double) progress.TotalCells * 100.0;
                //Console.WriteLine(string.Format("{0}% done, {1} words checked, {2}/{3} cells checked", percentDone, progress.WordsChecked, progress.CellsChecked, progress.TotalCells));

                //Thread.Sleep(100);
            }

            var turns = turnTask.Result;
            watch.Stop();

            turns = turns.Take(50).ToList();

            int turnNum = 1;

            foreach (var turn in turns)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(turnNum + " -Letters: ");
                builder.Append(string.Join(",", turn.Letters.Select(x => x.Character)));
                builder.Append(", total points: ");
                builder.Append(turn.Points);
                builder.Append(", to create word: ");
                builder.Append(turn.Word);

                Console.WriteLine(builder.ToString());

                turnNum++;
            }

            string next = Console.ReadLine();

            while (next != "e")
            {
                int selectedNum;
                if (!Int32.TryParse(next, out selectedNum))
                {
                    continue;
                }

                var copiedBoard = new char[board.Cells.Count()][];

                int cellIndex = 0;
                foreach (var row in board.Cells)
                {
                    copiedBoard[cellIndex] = board.Cells[cellIndex].Select(x => x.Letter == null ? '.' : x.Letter.Character).ToArray();
                    cellIndex++;
                }

                PotentialTurn selectedTurn = turns.ElementAt(selectedNum - 1);

                int letterIndex = 0;
                foreach (var cell in selectedTurn.Cells)
                {
                    copiedBoard[cell.Row][cell.Column] = selectedTurn.Word[letterIndex];
                    letterIndex++;
                }

                foreach (var row in copiedBoard)
                {
                    Console.WriteLine(string.Join(" ", row));
                }

                next = Console.ReadLine();
            }
        }

        static List<List<string>> ReadCsvFile()
        {
            List<List<string>> grid = new List<List<string>>();

            using (var reader = new StreamReader(@"D:\Documents\board.csv"))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    List<string> row = values.ToList();
                    grid.Add(row);
                }
            }

            return grid;
        }
    }
}
