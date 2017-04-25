// © Sayel Rammaha 2017
// 4/10/17
// "Debt Calculator" base Debt class
// ----------------------------------------------------------------------
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.If not, see<http://www.gnu.org/licenses/>.
// ----------------------------------------------------------------------

using System;
using System.Windows.Forms;

namespace DebtCalculator
{
    abstract class Debt
    {
        public int order { get; set; }
        public double principalLeft { get; set; }                
        public double payment { get; set; }        
        public double apr { get; set; }
        public double interestFactor { get; set; }
        public double period { get; set; } // compounding period
        public double maxPayment { get; set; }
        public double annualFees { get; set; }
        
        public Panel resultsPanel { get; set; }
        public int feesMonth { get; set; }
        public int months { get; set; }        
        public double interestPaid { get; set; }
        public double feesPaid { get; set; }
        public bool paidOff { get; set; }
        public bool feesChargedThisMonth { get; set; }

        public double AddToMonthlyPayment(double additionalPayment)
        {
            double rolloverLeft = 0;
            payment += additionalPayment;
            if (maxPayment != 0)
            {
                // ensure any rollover amount doesn't exceed maximum payment if set
                rolloverLeft = payment - maxPayment;
                payment = Math.Min(payment, maxPayment);
            }
            // returns any rollover that wasn't applied to this debt due to max payment set
            return Math.Max(rolloverLeft, 0);
        }     

        public void CalculateInterestFactor()
        {
            // this multiplier will produce one month's worth of interest when multiplied by a balance.            
            interestFactor = Math.Pow((1 + (apr / period)), period / 12) - 1;
        }

        // this is my attempt at preventing neverending debt explosions that never pay off
        // and continue calculating until overflow.  I tried to keep the minimum as low as I can
        // to account for more situations but the monthly payment needs to be enough to decrease principal
        // and by enough so that it doesn't take millions of years to pay off.
        // I had to come up with something more arbitrary than I would have liked
        // the formula is: 
        //    minimum payment = first month's interest + annual fees including interest that will
        //        accrue on those fees for however long is left in the year (starting "now") after
        //        those fees are assessed.
        // then I had to increase that amount because in some cases the debt would technically have 
        // decreasing principal but still take far too long to calculate millions of years worth of payments
        // so i multiply that value * 1.0014 (or ~ 1 + 1/720) and then add 1$ to it 
        // 1/720 was chosen to try to put a soft limit of around 60years (720 months) by ensuring the
        // first month's payment would pay 1/12 of the interest due for that year + 1/720 worth of principal.
        // due to the nonlinear nature of these calculations, this of course still produces limits of far less
        // or far greater pay off times when applied to different data sets.
        // Most of these would never be real life scenarios, as many of the problems I was seeing were 
        // related to loans with much smaller principals than annual fees, which wouldn't exist.
        // but I needed to come up with a way to handle any kind of input either by accident or design
        // while not restricting data any more than I had to
        // I'm still not certain this prevents runaway calculations and presumably eventual crashes
        
        public virtual double CalculateMinimumPayment()
        {

            double firstMonthInterest = principalLeft * interestFactor;
            double minPayment;
            if (annualFees > 0)
            {
                int interval = 13 - feesMonth;
                double intervalMultiplier = Math.Pow((1 + (apr / period)), period / 12 * interval);
                double feesPlusIntervalInterest = annualFees * intervalMultiplier;
                minPayment = (firstMonthInterest + (feesPlusIntervalInterest / 12)) * (1.0014) + 1;
                minPayment = Math.Round(minPayment, 2, MidpointRounding.AwayFromZero);
            }
            else
            {
                minPayment = firstMonthInterest * 1.0014 + 1;
                minPayment = Math.Round(minPayment, 2, MidpointRounding.AwayFromZero);
            }
            return minPayment;
        }

        abstract public void Update();        

        abstract public Debt CloneDebt();
    }
}
