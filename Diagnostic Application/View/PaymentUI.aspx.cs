using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Diagnostic_Application.BLL;
using Diagnostic_Application.Models;

namespace Diagnostic_Application.View {
    public partial class PaymentUI : System.Web.UI.Page
    {

        private double amount;
        private decimal _totalAmount;
        private decimal _dueAmount;

        private PaymentManager _paymentManager = new PaymentManager();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (ViewState["success"] == null)
            {
                PaymentButton.Enabled = false;
                AmountTextBox.Enabled = false;
            }
            else
            {
                PaymentButton.Enabled = true;
                AmountTextBox.Enabled = true;
            }
            
        }



        protected void SearchButton_Click(object sender, EventArgs e) {

            if (BillNoTextBox.Text == ""){
                
            }

            string billNo = BillNoTextBox.Text;
            string message = _paymentManager.IsBillNoExists(billNo);

            if (message == "success")
            {
                ViewState["success"] = true;
                InfoMessageLabel.Visible = true;
                InfoMessageLabel.Text = "Found Customer Information.";
                InfoMessageLabel.BackColor = Color.ForestGreen;
                ShowPaymentInformation(billNo);
            }
            else if(message == "failed"){

                InfoMessageLabel.Visible = true;
                InfoMessageLabel.Text = "Bill No Not Found.";
                InfoMessageLabel.BackColor = Color.DarkRed;
            }


        }

        private void ShowPaymentInformation(string billNo)
        {
            TestEntry testEntry = _paymentManager.SearchByBill(billNo);
            Patient patient = _paymentManager.SearchPatientInfoByBillNo(billNo);

            _totalAmount = Convert.ToDecimal(testEntry.TotalAmount);
            _dueAmount = Convert.ToDecimal(patient.DueAmount.ToString());
            
            BillDateLabel.Text = patient.DueDate.ToString();
            TotalFeeLabel.Text = _totalAmount + " Taka";
            DueAmountLabel.Text = _dueAmount + " Taka";
            PaidAmountLabel.Text = (_totalAmount - _dueAmount).ToString() + " Taka";

            ViewState["DueAmount"] = _dueAmount;
            ViewState["TotalAmount"] = _totalAmount;
            ViewState["billNo"] = billNo;

            //Enable Payment Button and textbox
            PaymentButton.Enabled = true;
            AmountTextBox.Enabled = true;
        }


        protected void PaymentButton_Click(object sender, EventArgs e) {
            
            //check empty
            if (AmountTextBox.Text == "") {
                InfoMessageLabel.Visible = true;
                InfoMessageLabel.Text = "Empty Amount.";
                InfoMessageLabel.BackColor = Color.DarkRed;
                return;
            }

            //collect the amount;
            string _billNo = (string)ViewState["billNo"];
            decimal _paidAmount = Convert.ToDecimal(AmountTextBox.Text);

            _dueAmount = (decimal) ViewState["DueAmount"];


            //check amount is greater than the dueAmount
            if (_paidAmount > _dueAmount) {
                InfoMessageLabel.Visible = true;
                InfoMessageLabel.Text = "Cannot Proced. Payment Amount greater than Due Amount.";
                InfoMessageLabel.BackColor = Color.DarkRed;
                return;
            }

            if (_paidAmount == _dueAmount){
                //save information with UpdateStatus = 1
                decimal _newDueAmount = _dueAmount - _paidAmount;
                string message = _paymentManager.UpdatePaymentWithStatus(_billNo, _newDueAmount, 1);
                if (message == "success"){

                    ViewState["success"] = true;
                    InfoMessageLabel.Visible = true;
                    InfoMessageLabel.Text = "Full Paid!! Update customer information.";
                    InfoMessageLabel.BackColor = Color.ForestGreen;
                    ShowPaymentInformation(_billNo);
                }

                return;
            }
            else {
                //everything is fine.
                decimal _newDueAmount = _dueAmount - _paidAmount;
                //check the newDueAmount
                string message = _paymentManager.UpdatePayment(_billNo, _newDueAmount);
                if (message == "success") {
                    ViewState["success"] = true;
                    InfoMessageLabel.Visible = true;
                    InfoMessageLabel.Text = "Partial Paid! Update customer information.";
                    InfoMessageLabel.BackColor = Color.ForestGreen;
                    ShowPaymentInformation(_billNo);
                }

                return;
            }

        }
    }
}