using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordGameSolver.Models.Prediction
{
    public class ProgressReport
    {
        public int WordsChecked { get; set; }

        public int CellsChecked { get; set; }

        public int TotalCells { get; set; }

        public bool IsProgressInitialized { get; set; }
    }
}
