using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordGameSolver.Models.Board
{
    public class GameBoard
    { 
        public List<List<BoardCell>> Cells { get; set; }

        public GameBoard(List<List<BoardCell>> cells)
        {
            Cells = cells;
        }

        public BoardCell GetCellAtPosition(int rowIndex, int columnIndex)
        {
            return Cells[rowIndex][columnIndex];
        }
    }
}
