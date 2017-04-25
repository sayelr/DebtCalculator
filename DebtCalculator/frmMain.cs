// © Sayel Rammaha 2017
// 4/10/17
// "Debt Calculator" - Main program form file 

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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DebtCalculator
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }
        // The tab order of the UI is relied upon for all of the functionality
        // Do not change the tab order of the UI controls         

        List<Panel> panelList;     // list of all panels sorted by tab order  
        List<Panel> mainPanels = new List<Panel>();  // left side panels only
        int oldOrder; // used to store previous value before changed
                      // of numericupdowns
        int lastMonthsTotal = 0;  // used to store previous calculation totals
        double lastInterestTotal = 0; // ^^

        // ****************** Methods for UI Controls ******************

        // upon form load
        private void frmMain_Load(object sender, EventArgs e)
        {
            // create panelList - contains all panels in form, sorted by tab order
            Form thisForm = sender as Form;
            panelList = thisForm.Controls.OfType<Panel>().ToList();
            panelList.Sort((c1, c2) => c1.TabIndex.CompareTo(c2.TabIndex));

            // create mainPanels - list of all panels on left side of ui, sorted by tab order
            for (int i = 0; i < panelList.Count; i += 3)
                mainPanels.Add(panelList[i]);

            // set ListBox default for debt type and max payment
            foreach (Panel p in mainPanels)
            {
                List<Control> controls = GetControls(p);
                ListBox box = controls[2] as ListBox; // this panel's debt type box
                box.SelectedIndex = 0;  // initialize to amortized loan

                // set max payment box to 0 
                controls[5].Text = "0";
            }
        }
       
        // returns sorted list of controls on a panel
        public List<Control> GetControls(Panel panel)
        {
            var controls = panel.Controls.OfType<Control>().ToList();
            controls.Sort((c1, c2) => c1.TabIndex.CompareTo(c2.TabIndex));
            return controls;
        }

        // takes a panel and number.  uses the number as an index offset to 
        // return a panel relative to the passed panel
        private Panel GetPanel(Panel panel, int indexOffset)
        {
            int index = panelList.IndexOf(panel);
            return panelList.ElementAt(index + indexOffset);
        }

        // draws dynamic matching right side panels for cc or loans
        private void DrawPanel(Panel panel, string type, string structure)
        {
            if (type == "Amortized Loan" || type == "Credit Card/Line")
            {
                panel.Controls.Add(new TextBox()); //principal 

                TextBox tb = new TextBox();  // apr
                tb.Size = new Size(50, 20);
                panel.Controls.Add(tb);

                tb = new TextBox();  // fees
                tb.Size = new Size(50, 20);
                tb.Text = "0";
                panel.Controls.Add(tb);

                NumericUpDown upDown = new NumericUpDown();  // how many months from now will the fees be assessed
                upDown.Size = new Size(35, 20);
                upDown.Minimum = 1;
                upDown.Maximum = 12;
                upDown.Value = 12;
                panel.Controls.Add(upDown);

                Label label = new Label();
                label.Text = "Current Principal Balance";
                label.Size = new Size(100, 26);
                panel.Controls.Add(label);

                label = new Label();
                label.Text = "Rate (APR)";
                label.Size = new Size(50, 26);
                panel.Controls.Add(label);

                label = new Label();
                label.Text = "Annual Fees";
                label.Size = new Size(50, 26);
                panel.Controls.Add(label);

                label = new Label();
                label.Text = "Months until fees charged";
                label.Size = new Size(50, 39);
                panel.Controls.Add(label);
            }            
            else if (type == "Add-on Loan")
            {
                TextBox tb = new TextBox();  // principal
                panel.Controls.Add(tb);

                tb = new TextBox();  // fees
                tb.Size = new Size(50, 20);
                tb.Text = "0";
                panel.Controls.Add(tb);

                NumericUpDown upDown = new NumericUpDown();  // how many months from now will the fees be assessed
                upDown.Size = new Size(35, 20);
                upDown.Minimum = 1;
                upDown.Maximum = 12;
                upDown.Value = 12;
                panel.Controls.Add(upDown);

                Label label = new Label();
                label.Text = "Current Principal Balance";
                label.Size = new Size(100, 26);

                panel.Controls.Add(label);
                label = new Label();
                label.Text = "Annual Fees";
                label.Size = new Size(50, 26);
                panel.Controls.Add(label);

                label = new Label();
                label.Text = "Months until fees charged";
                label.Size = new Size(50, 39);
                panel.Controls.Add(label);
            }
        }

        // returns list of mainPanels whose checkbox is checked
        private List<Panel> GetActivePanels()
        {
            List<Panel> activePanels = new List<Panel>();
            foreach (Panel p in mainPanels)
            {
                List<Control> controls = GetControls(p); // taborder sorted control list
                CheckBox box = controls[0] as CheckBox; // this panel's checkbox
                if (box.Checked)
                    activePanels.Add(p);
            }

            return activePanels;
        }

        // parse active main panel data to create a list of debt objects
        private DebtManager CreateDebtList(List<Panel> activePanels)
        {
            DebtManager debtManager = new DebtManager();

            foreach (Panel p in activePanels)
            {
                List<Control> controls = GetControls(p);
                List<Control> matchingControls = GetControls(GetPanel(p, 1));
                ListBox type = controls[2] as ListBox;
                ListBox structure = controls[3] as ListBox;
                NumericUpDown order = controls[10] as NumericUpDown;
                
                if (type.Text == "Amortized Loan")
                {
                    Amortized debt = new Amortized();                    
                    // extract info from input boxes
                    debt.payment = Convert.ToDouble(controls[4].Text);
                    if (rollover.Checked)
                        debt.maxPayment = Convert.ToDouble(controls[5].Text);

                    debt.order = (int)order.Value;

                    debt.principalLeft = Convert.ToDouble(matchingControls[0].Text);
                    debt.apr = Convert.ToDouble(matchingControls[1].Text) / 100;
                    debt.annualFees = Convert.ToDouble(matchingControls[2].Text);
                    NumericUpDown feesMonthUpDown = matchingControls[3] as NumericUpDown;
                    debt.feesMonth = (int)feesMonthUpDown.Value;

                    debt.resultsPanel = GetPanel(p, 2);

                    // set compounding period and interestFactor
                    if (structure.Text == "Daily")
                        debt.period = 365.25;
                    else if (structure.Text == "Monthly")
                        debt.period = 12;
                    debt.CalculateInterestFactor();

                    // add completed debt object to list
                    debtManager.Add(debt);
                }
                else if (type.Text == "Credit Card/Line")
                {
                    CreditCard debt = new CreditCard();                    

                    // extract info from text boxes
                    debt.payment = Convert.ToDouble(controls[4].Text);
                    if (rollover.Checked)
                        debt.maxPayment = Convert.ToDouble(controls[5].Text);

                    debt.order = (int)order.Value;

                    debt.principalLeft = Convert.ToDouble(matchingControls[0].Text);
                    debt.apr = Convert.ToDouble(matchingControls[1].Text) / 100;
                    debt.annualFees = Convert.ToDouble(matchingControls[2].Text);
                    NumericUpDown feesMonthUpDown = matchingControls[3] as NumericUpDown;
                    debt.feesMonth = Convert.ToInt16(feesMonthUpDown.Value);

                    debt.resultsPanel = GetPanel(p, 2);

                    // set compounding period and interestFactor
                    if (structure.Text == "Daily")
                        debt.period = 365.25;
                    else if (structure.Text == "Monthly")
                        debt.period = 12;
                    debt.CalculateInterestFactor();

                    // add completed debt object to list
                    debtManager.Add(debt);
                }
                else if (type.Text == "Add-on Loan")
                {
                    AddOn debt = new AddOn();
                   
                    // extract info from text boxes
                    debt.payment = Convert.ToDouble(controls[4].Text);
                    if (rollover.Checked)
                        debt.maxPayment = Convert.ToDouble(controls[5].Text);

                    debt.order = (int)order.Value;

                    debt.principalLeft = Convert.ToDouble(matchingControls[0].Text);
                    debt.annualFees = Convert.ToDouble(matchingControls[1].Text);
                    NumericUpDown feesMonthUpDown = matchingControls[2] as NumericUpDown;
                    debt.feesMonth = Convert.ToInt16(feesMonthUpDown.Value);

                    debt.resultsPanel = GetPanel(p, 2);

                    // add completed debt object to list
                    debtManager.Add(debt);
                }
            }            
            debtManager.Sort((d1, d2) => d1.order.CompareTo(d2.order));
            return debtManager;
        }

        // display results for each debt in results panels
        private void DisplayResults(DebtManager debtManager)
        {
            List<Control> controls;
            Panel resultsPanel;
            int months;
            string interestStr;
            double interest;
            Color monthsColor;
            Color interestColor;

            // totals
            int monthsTotal = 0;
            double interestTotal = 0;
            
            foreach (Debt debt in debtManager)
            {
                monthsColor = Color.Black;
                interestColor = Color.Black;
                resultsPanel = debt.resultsPanel;
                controls = GetControls(resultsPanel);

                if (controls[0].Text != "" && controls[1].Text != "")
                {
                    // get results panel values from previous calculation
                    months = Convert.ToInt32(controls[0].Text);
                    interestStr = controls[1].Text.Trim('$');
                    interest = Convert.ToDouble(interestStr);

                    // set results panel colors to reflect improvement or worsening of previous calculation
                    if (months < debt.months)
                        monthsColor = Color.Red;
                    else if (months > debt.months)
                        monthsColor = Color.Green;

                    if (interest < Math.Round(debt.interestPaid, 2, MidpointRounding.AwayFromZero))
                        interestColor = Color.Red;
                    else if (interest > Math.Round(debt.interestPaid, 2, MidpointRounding.AwayFromZero))
                        interestColor = Color.Green;
                }
                // populate results panel for this debt
                controls[0].Text = debt.months.ToString();
                controls[1].Text = (debt.interestPaid + debt.feesPaid).ToString("c");
                controls[2].Text = debt.payment.ToString("c");
                controls[0].ForeColor = monthsColor;
                controls[1].ForeColor = interestColor;

                // add to calculation totals
                if (debt.months > monthsTotal)
                    monthsTotal = debt.months;
                interestTotal += debt.interestPaid + debt.annualFees;
            }

            // populate total results of all debts
            int monthsDifference;
            double interestDifference;
            Color monthsDifferenceColor = Color.Black;
            Color interestDifferenceColor = Color.Black;

            // set total time to pay off all debts
            lblTimeToPayAll.Text = FormatMonths(monthsTotal);

            // skip difference if first calculation
            // if not, display time difference with appropriate color
            if (lastMonthsTotal != 0)
            {
                monthsDifference = lastMonthsTotal - monthsTotal;
                if (monthsDifference > 0)
                    monthsDifferenceColor = Color.Green;
                if (monthsDifference < 0)
                    monthsDifferenceColor = Color.Red;
                lblTimeDifference.Text = FormatMonths(monthsDifference);
                lblTimeDifference.ForeColor = monthsDifferenceColor; 
            }

            // set total interest paid on all debts
            lblTotalInterest.Text = interestTotal.ToString("c");

            // skip difference if first calculation
            // if not, display interest difference with appropriate color
            if (lastInterestTotal != 0)
            {
                interestDifference = lastInterestTotal - interestTotal;
                interestDifference = Math.Round(interestDifference, 2, MidpointRounding.AwayFromZero);
                if (interestDifference > 0)
                    interestDifferenceColor = Color.Green;
                else if (interestDifference < 0)
                    interestDifferenceColor = Color.Red;
                lblInterestDifference.Text = interestDifference.ToString("c");
                lblInterestDifference.ForeColor = interestDifferenceColor; 
            }

            // store values for comparison on next calculation
            lastMonthsTotal = monthsTotal;
            lastInterestTotal = interestTotal;
        }

        // converts months to a string with months/years
        private string FormatMonths(int months)
        {
            int years = months / 12;
            int monthsLeftOver = months % 12;
            if (years > 0)
                return years + " years, " + monthsLeftOver + " months";
            else 
                return monthsLeftOver + " months";
        }

        // ----------------------------------------------------
        // ****************** Event Handlers ******************

        // Calculate Button Main Event
        private void btnCalculate_Click(object sender, EventArgs e)
        {
            DebtManager debtManager;
            List<Panel> activePanels = GetActivePanels();
            if (activePanels.Count != 0)  // if there are active panels
            {  
                bool allPanelsValid = true;
                // validate panels
                foreach (Panel p in activePanels)
                {
                    if (!IsValidPanelSet(p))
                    {
                        allPanelsValid = false;
                        break;
                    }
                }
                
                // if the panels weren't all valid, this method ends here
                // the focus returns to the form on the first field that was invalid                
                if (allPanelsValid)
                {
                    // clear error messages
                    statusLabel.Text = "";
                    
                    // gather form data to create list of debts
                    debtManager = CreateDebtList(activePanels);

                    // ensure that monthly payments are high enough
                    // I would've liked to do this in the validation method but I think it's 
                    // easier and cleaner to do it here once the objects have been created
                    // but maybe it's a mistake
                    bool paymentsAreValid = true;
                    foreach (Debt debt in debtManager)
                    {
                        if (!IsValidPayment(debt))
                        {
                            paymentsAreValid = false;
                            break;
                        }
                    }

                    if (paymentsAreValid)
                    {
                        debtManager.ProcessDebts(rollover.Checked);
                        DisplayResults(debtManager);
                    }
                }
            }
        }
                
        // change debt type event
        private void DebtTypeChanged(object sender, EventArgs e)
        {
            ListBox thisBox = sender as ListBox;
            Panel thisPanel = thisBox.Parent as Panel;
            ListBox structureBox = thisPanel.GetNextControl(thisBox, true) as ListBox;
            Panel matchingPanel = GetPanel(thisPanel, 1);

            // populate structure box with daily/monthly compounding
            if (thisBox.Text == "Amortized Loan" || thisBox.Text == "Credit Card/Line")
            {
                structureBox.Enabled = true;
                structureBox.Items.Clear();
                structureBox.Items.Add("Daily");
                structureBox.Items.Add("Monthly");
                structureBox.SelectedIndex = 1;  // select monthly by default

            }            
            // disable/clear structure box for addon loan  
            else if (thisBox.Text == "Add-on Loan")
            {
                structureBox.Items.Clear();
                structureBox.Enabled = false;
            }
            // draw panel
            matchingPanel.Controls.Clear();
            DrawPanel(matchingPanel, thisBox.Text, structureBox.Text);
        }

        // activate/deactivate all panels (checkboxes)
        private void ActivateAllChanged(object sender, EventArgs e)
        {
            foreach (Panel p in mainPanels)
            {
                List<Control> controls = GetControls(p);
                CheckBox cb = controls[0] as CheckBox;
                CheckBox thisBox = sender as CheckBox;
                cb.Checked = thisBox.Checked;
            }
        }

        // reset debt order (numeric updowns)
        private void btnResetOrder_Click(object sender, EventArgs e)
        {
            // sets each panel's order updown to its position 1-10
            // using the index of the panels 0-9 in the list
            foreach (Panel panel in mainPanels)
            {
                NumericUpDown test = GetControls(panel)[10] as NumericUpDown;
                test.Value = mainPanels.IndexOf(panel) + 1;
            }
        }

        // debt order updown changed
        private void OrderChanged(object sender, EventArgs e)
        {
            NumericUpDown upDown = sender as NumericUpDown;
            Panel thisPanel = upDown.Parent as Panel;       //
            // panel containing the updown that was changed //
            List<Control> controls = GetControls(thisPanel);//
            NumericUpDown order = controls[10] as NumericUpDown;
            foreach (Panel panel in mainPanels)
            {
                List<Control> currentControls = GetControls(panel);
                NumericUpDown currentOrder = currentControls[10] as NumericUpDown;
                // if this panel in the list isn't the panel that raised the event
                // and both panels' upDown's are the same value
                if (!thisPanel.Equals(panel) && order.Value == currentOrder.Value)
                    // change this panel's upDown to the previous value of the 
                    // changed upDown so they are all 1 through 10 with no duplicates
                    currentOrder.Value = oldOrder;                
            }
            thisPanel.Focus();  // drop focus on order updown so every click 
            // will trigger OrderFocused event to capture old updown value
        }

        // debt order updown gained focus
        private void OrderFocused(object sender, EventArgs e)
        {
            // this is used to capture the value of a numeric updown right 
            // before it is changed
            NumericUpDown upDown = sender as NumericUpDown;
            oldOrder = (int)upDown.Value;
        }

        // clickable paypal link
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://paypal.me/SayelR");
        }

        // -------------------------------------------------------
        // ************** Info label Event Handlers **************

        // ***** For Left label - lblInfoType *****

        // reset debt order button
        private void ResetOrderInfo(object sender, EventArgs e)
        {
            lblinfoType.Text = "This button resets the order rollover " +
                "payments are applied";
        }

        // activate all checkbox
        private void ActivateAllInfo(object sender, EventArgs e)
        {
            lblinfoType.Text = "Click to either activate or deactivate " +
                "all debt entry panels";
        }
        
        // name box
        private void NameInfo(object sender, EventArgs e)
        {
            lblinfoType.Text = "(optional) You can give each debt an " +
                "identifying name for your reference";
        }

        // debt type box
        private void TypeInfo(object sender, EventArgs e)
        {
            lblinfoType.Text = "Choose a debt type.\n" +
                "-Most installment loans are amortized.\n" +
                "-Add-on loans are loans in which the interest " +
                "is pre-computed and added on to the initial " +
                "amount borrowed.  With these loans, you pay the " +
                "full amount of interest no matter how quickly " +
                "you pay the loan off, thus interest is not " +
                "considered here.";
        }

        // debt structure box
        private void StructureInfo(object sender, EventArgs e)
        {
            lblinfoType.Text = "Select how often the debt is compounded.\n" +
                "-Most credit cards and lines of credit coumpound daily.\n" +
                "-Amortized loans, such as many mortgages and car loans, " +
                "are typically compounded monthly.";
        }

        // current payment box
        private void PaymentInfo(object sender, EventArgs e)
        {
            lblinfoType.Text = "Enter how much you are currently paying per month.";
        }

        // max payment box
        private void MaxPaymentInfo(object sender, EventArgs e)
        {
            lblinfoType.Text = "If you would like to ensure that the monthly payment " +
                "applied to this debt never exceeds a certain amount, you can enter " +
                "that amount here.  Enter 0 for unlimited";
        }

        // --------------------------------------------------------
        // ****************** Validation methods ******************
        
        // validates each panel's set containing one debt
        private bool IsValidPanelSet(Panel thisPanel)
        {
            List<Control> controls = GetControls(thisPanel); // controls in main panel
            TextBox paymentBox = controls[4] as TextBox;
            TextBox maximumBox = controls[5] as TextBox;

            // validate payment box
            try
            {
                Convert.ToDouble(paymentBox.Text);
            }
            catch (FormatException)
            {
                paymentBox.Focus();
                paymentBox.SelectAll();
                statusLabel.Text = "Monthly payment must be numeric without $ or commas.";                
                return false;
            }

            // validate max payment box
            if (rollover.Checked)
            {
                try
                {
                    Convert.ToDouble(maximumBox.Text);
                    
                }
                catch (FormatException)
                {
                    maximumBox.Focus();
                    maximumBox.SelectAll();
                    statusLabel.Text = "Maximum payment must be numeric without $ or commas.";
                    return false;
                }

                if (!IsValidMaxPayment(paymentBox, maximumBox))
                {
                    maximumBox.Focus();
                    maximumBox.SelectAll();
                    statusLabel.Text = "Maximum payment must be either 0 for unlimited, " +
                        "or at least as much as the current payment.";
                    return false;
                }
            }            

            // validate matching dynamic panel
            Panel matchingPanel = GetPanel(thisPanel, 1);
            List<Control> matchingControls = GetControls(matchingPanel);
            foreach (Control control in matchingControls)  // controls in matching dynamic panel
            {
                if (control is TextBox)  // only check textboxes, ignore labels and updowns
                {
                    TextBox box = control as TextBox;
                    try
                    {
                        Convert.ToDouble(box.Text);
                    }
                    catch (FormatException)
                    {
                        box.Focus();    
                        box.SelectAll();
                        statusLabel.Text = "Must be numeric without $ or commas."; 
                        return false; 
                    }                    
                }
            }

            return true;  // reached if all were valid
        }

        // tests max payment for either 0 or >= current payment
        private bool IsValidMaxPayment(TextBox paymentBox, TextBox maxPaymentBox)
        {
            double current = Convert.ToDouble(paymentBox.Text);
            double max = Convert.ToDouble(maxPaymentBox.Text);
            if (max != 0 && max < current)
            {
                return false;
            }
            return true;
        }

        // tests current monthly payment against minimum payment
        private bool IsValidPayment(Debt debt)
        {
            double minPayment = debt.CalculateMinimumPayment();
            if (debt.payment < minPayment)  // not valid payment
            {
                List<Control> controls = GetControls(GetPanel(debt.resultsPanel, -2));
                TextBox paymentBox = controls[4] as TextBox;
                statusLabel.Text = "Monthly payment must be enough so that the principal " +
                    "decreases and the debt can be paid off.";
                paymentBox.Focus();
                paymentBox.Text = minPayment.ToString();
                paymentBox.SelectAll();
                return false;
            }
            return true;
        }
    }
}
