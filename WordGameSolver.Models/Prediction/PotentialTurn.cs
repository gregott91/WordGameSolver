using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordGameSolver.Models.Board;
using WordGameSolver.Models.Game;

namespace WordGameSolver.Models.Prediction
{
    public class PotentialTurn
    {
        public string Word { get; set; }

        public List<BoardCell> Cells { get; set; }

        public int Points { get; set; }

        public List<Letter> Letters { get; set; }
    }
}
