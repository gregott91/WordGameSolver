using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordGameSolver.Interfaces.Board;
using WordGameSolver.Models.Board;

namespace WordGameSolver.Business.Board
{
    public class ScrabbleBoardLogic : IGameBoardLogic
    {
        public List<List<BoardCell>> GetColumnsFromRows(GameBoard board)
        {
            List<List<BoardCell>> columns = new List<List<BoardCell>>();

            foreach (var row in board.Cells)
            {
                int columnIndex = 0;

                foreach (var cell in row)
                {
                    if (columns.Count() <= columnIndex)
                    {
                        columns.Add(new List<BoardCell>());
                    }

                    columns[columnIndex].Add(cell);
                    columnIndex++;
                }
            }

            return columns;
        }
    }
}
