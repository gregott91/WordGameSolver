using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordGameSolver.Interfaces.Game;
using WordGameSolver.Models.Game;
using WordGameSolver.Models.Prediction;

namespace WordGameSolver.Business.Game
{
    public class ScrabblePointCalculatorLogic : IPointCalculatorLogic
    {
        public int CalculatePoints(PotentialTurn turn)
        {
            int pointCount = 0;
            int multiplier = 1;

            int letterIndex = 0;
            for (int i = 0; i < turn.Cells.Count(); i++)
            {
                if (turn.Cells[i].Letter == null)
                {
                    int pointsForNewLetter = turn.Letters[letterIndex].Value;

                    if (turn.Cells[i].Modifier != null)
                    {
                        if (turn.Cells[i].Modifier.AffectsWholeString)
                        {
                            multiplier = multiplier * turn.Cells[i].Modifier.ModifierValue;
                        }
                        else
                        {
                            pointsForNewLetter = pointsForNewLetter * turn.Cells[i].Modifier.ModifierValue;
                        }
                    }

                    pointCount += pointsForNewLetter;
                    
                    letterIndex++;
                }
                else
                {
                    pointCount += turn.Cells[i].Letter.Value;
                }
            }

            return pointCount * multiplier;
        }
    }
}
