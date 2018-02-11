using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordGameSolver.Models.Game
{
    public class Letter
    {
        public char Character { get; set; }

        public int Value { get; set; }

        public Letter(char character, int value)
        {
            Character = character;
            Value = value;
        }

        public override bool Equals(object obj)
        {
            Letter comp = obj as Letter;

            if (comp == null)
            {
                return false;
            }

            return comp.Character == Character;
        }

        public override int GetHashCode()
        {
            return Character.GetHashCode();
        }
    }
}
