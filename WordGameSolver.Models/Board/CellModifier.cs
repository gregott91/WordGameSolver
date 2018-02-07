using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordGameSolver.Models.Board
{
    public class CellModifier
    {
        public bool AffectsWholeString { get; private set; }

        public int ModifierValue { get; private set; }

        public CellModifier(bool affectsWholeString, int modifierValue)
        {
            AffectsWholeString = affectsWholeString;
            ModifierValue = modifierValue;
        }
    }
}
