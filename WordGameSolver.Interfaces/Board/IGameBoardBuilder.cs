using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordGameSolver.Models.Board;

namespace WordGameSolver.Interfaces.Board
{
    public interface IGameBoardBuilder
    {
        GameBoard BuildBoard();
    }
}
