// © Sayel Rammaha 2017
// 4/10/17
// "Debt Calculator" - DebtManager class to store debts as a list
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

// Used to add a ProcessDebts() method to keep it separate from the UI

using System.Collections.Generic;
using System.Linq;

namespace DebtCalculator
{
    class DebtManager : List<Debt>
    {
        public void ProcessDebts(bool rollover)
        {            
            bool rolloverFlag = false;
            List<Debt> debtsToRemove = new List<Debt>();
            List<Debt> debts = this.ToList(); // make a copy to work with so debts can be removed from
            // this list but retained in the list stored in this class, but the debts in each list
            // are the same objects so they get update()d in both lists, so when the original list
            // stored in this class gets sent to DisplayResults() the debts are all paid off
            double currentRolloverToApply = 0;
            double nextMonthRolloverToApply = 0;

            // while debt remains
            while (debts.Count > 0)
            {
                currentRolloverToApply += nextMonthRolloverToApply;
                nextMonthRolloverToApply = 0;

                if (rollover && currentRolloverToApply > 0)
                    rolloverFlag = true;

                // process each debt remaining for one month
                foreach (Debt debt in debts)
                {
                    if (rolloverFlag) // if a debt was paid off last month, add rollover to next debt
                    {
                        currentRolloverToApply = debt.AddToMonthlyPayment(currentRolloverToApply);
                        if (currentRolloverToApply == 0) // if rollover for this month gone
                            rolloverFlag = false;
                    }

                    debt.Update(); // simulate one month of calculations

                    if (debt.paidOff) // if paid off, add rollover amount for next month
                    {
                        nextMonthRolloverToApply += debt.payment;
                        debtsToRemove.Add(debt); // and queue paid debt for removal
                    }
                }

                // remove paid off debts from debt list
                foreach (Debt paid in debtsToRemove)
                    debts.Remove(paid);
                debtsToRemove.Clear();
            }
        }
    }
}
