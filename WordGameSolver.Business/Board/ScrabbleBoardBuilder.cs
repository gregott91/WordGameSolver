using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordGameSolver.Interfaces.Board;
using WordGameSolver.Models.Board;

namespace WordGameSolver.Business.Board
{
    public class ScrabbleBoardBuilder : IGameBoardBuilder
    {
        private const int boardSize = 15;

        public GameBoard BuildBoard()
        {
            return new GameBoard(GenerateBoard());
        }

        private List<List<BoardCell>> GenerateBoard()
        {
            List<List<BoardCell>> backingBoard = new List<List<BoardCell>>();

            for (var rowIndex = 0; rowIndex < boardSize; rowIndex++)
            {
                backingBoard.Add(new List<BoardCell>());

                for (var columnIndex = 0; columnIndex < boardSize; columnIndex++)
                {
                    CellModifier modifier = GetModifier(rowIndex, columnIndex);

                    backingBoard[rowIndex].Add(new BoardCell(rowIndex, columnIndex, modifier));
                }
            }

            return backingBoard;
        }

        private CellModifier GetModifier(int rowIndex, int columnIndex)
        {
            int rowDistanceFromEdge = GetDistanceFromEdge(rowIndex);
            int columnDistanceFromEdge = GetDistanceFromEdge(columnIndex);

            return GetCellModifier(rowDistanceFromEdge, columnDistanceFromEdge);
        }

        private int GetDistanceFromEdge(int cellIndex)
        {
            return 7 - Math.Abs(cellIndex - 7);
        }

        private CellModifier GetCellModifier(int rowDistanceFromEdge, int columnDistanceFromEdge)
        {
            CellModifier threeWordScore = new CellModifier(true, 3);
            CellModifier twoWordScore = new CellModifier(true, 2);
            CellModifier threeLetterScore = new CellModifier(false, 3);
            CellModifier twoLetterScore = new CellModifier(false, 2);

            switch (rowDistanceFromEdge)
            {
                case 0:
                    switch (columnDistanceFromEdge)
                    {
                        case 0: return threeWordScore;
                        case 3: return twoLetterScore;
                        case 7: return threeWordScore;
                        default: return null;
                    }
                case 1:
                    switch (columnDistanceFromEdge)
                    {
                        case 1: return twoWordScore;
                        case 5: return threeLetterScore;
                        default: return null;
                    }
                case 2:
                    switch (columnDistanceFromEdge)
                    {
                        case 2: return twoWordScore;
                        case 6: return twoLetterScore;
                        default: return null;
                    }
                case 3:
                    switch (columnDistanceFromEdge)
                    {
                        case 0: return twoLetterScore;
                        case 3: return twoWordScore;
                        case 7: return twoLetterScore;
                        default: return null;
                    }
                case 4:
                    switch (columnDistanceFromEdge)
                    {
                        case 4: return twoWordScore;
                        default: return null;
                    }
                case 5:
                    switch (columnDistanceFromEdge)
                    {
                        case 1: return threeLetterScore;
                        case 5: return threeLetterScore;
                        default: return null;
                    }
                case 6:
                    switch (columnDistanceFromEdge)
                    {
                        case 2: return twoLetterScore;
                        case 6: return twoLetterScore;
                        default: return null;
                    }
                case 7:
                    switch (columnDistanceFromEdge)
                    {
                        case 0: return threeWordScore;
                        case 3: return twoLetterScore;
                        default: return null;
                    }
                default: return null;
            }
        }
    }
}
