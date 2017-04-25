// © Sayel Rammaha 2017
// 4/10/17
// "Debt Calculator" - Amortized Loan class

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

namespace DebtCalculator
{
    class Amortized : Debt
    {
        public int compoundingPeriod { get; set; }

        override public void Update()
        {
            // process month
            months++;
            // calculate payment breakdown
            double interest = Math.Round(principalLeft * interestFactor, 2, MidpointRounding.AwayFromZero);
            double principal = payment - interest;            
            interestPaid += interest;
            // add annual fees if due this month
            if (months == feesMonth || (months - feesMonth) % 12 == 0)
            {
                principalLeft += annualFees;
                feesPaid += annualFees;
            }        
            // update principal remaining
            principalLeft -= principal;
            // if paid off, set flag
            if (principalLeft <= 0)
                paidOff = true;                
        }
        
        override public Debt CloneDebt()
        {
            Debt clone = new Amortized();
            clone.order = order;
            clone.interestPaid = interestPaid;
            clone.maxPayment = maxPayment;
            clone.apr = apr;
            clone.interestFactor = interestFactor;
            clone.period = period;
            clone.months = months;
            clone.annualFees = annualFees;
            clone.feesMonth = feesMonth;
            clone.feesPaid = feesPaid;
            clone.paidOff = paidOff;
            clone.payment = payment;
            clone.principalLeft = principalLeft;
            clone.resultsPanel = resultsPanel;
            return clone;
        }
    }
}
