using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordGameSolver.Models.Game;
namespace WordGameSolver.Models.Board
{
    public class BoardCell
    {
        public Letter Letter { get; set; }

        public CellModifier Modifier { get; private set; }

        public int Row { get; set; }

        public int Column { get; set; }

        public BoardCell(int row, int column)
            :this(row, column, null)
        {
        }

        public BoardCell(int row, int column, CellModifier modifier)
        {
            Row = row;
            Column = column;
            Modifier = modifier;
        }
    }
}
