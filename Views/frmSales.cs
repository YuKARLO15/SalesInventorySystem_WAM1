﻿using System;
using System.Windows.Forms;
using SalesInventorySystem_WAM1.Handlers;
using SalesInventorySystem_WAM1.Models;

namespace SalesInventorySystem_WAM1
{
    public partial class frmSales : Form
    {
        private ItemHandler ih = new ItemHandler();
        private SalesHandler sh = new SalesHandler();
        private DateTime selected_transaction = DateTime.MinValue;

        public frmSales()
        {
            InitializeComponent();
            UpdateTransactionsList(null);
        }

        /// <summary>
        /// Updates the transactions list in the DataGridView.
        /// </summary>
        /// <param name="query">If not null, search for transactions with details containing this query.</param>
        private void UpdateTransactionsList(string query)
        {
            cbItem.Items.Clear();
            foreach (var item in ih.GetAllItems())
                cbItem.Items.Add($"[{item.Id}] {item.Name}");

            var transactions =
                query == null ? sh.GetAllTransactions() : sh.SearchTransactions(query);
            dgvSales.Rows.Clear();
            foreach (var transaction in transactions)
                dgvSales.Rows.Add(
                    transaction.Id,
                    $"[{transaction.ItemId}] {ih.GetItem(transaction.ItemId).Name}",
                    transaction.Category,
                    transaction.Price,
                    transaction.Quantity,
                    transaction.Status,
                    transaction.Notes
                );
        }

        /// <summary>
        /// Validates the values of the components in the form.
        /// </summary>
        /// <returns>If the values are valid.</returns>
        private bool ValidateValues()
        {
            if (cbItem.SelectedIndex == -1)
            {
                MessageBox.Show(
                    "Please select an item.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return false;
            }
            if (cbCategory.SelectedIndex == -1)
            {
                MessageBox.Show(
                    "Please select a category.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return false;
            }
            if (txtQuantity.Text == string.Empty)
            {
                MessageBox.Show(
                    "Please enter a quantity.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return false;
            }
            if (int.TryParse(txtQuantity.Text, out int _) == false)
            {
                MessageBox.Show(
                    "Quantity must be a number.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return false;
            }
            if (txtPrice.Text == string.Empty)
            {
                MessageBox.Show(
                    "Please select an item and enter the quantity to get a price.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return false;
            }
            return true;
        }

        /// <summary>
        /// Fills up the values of the form based on the selected item and quantity.
        /// </summary>
        private void FillUpValues()
        {
            // Get the item data
            if (cbItem.SelectedIndex != -1)
            {
                // Get the item ID from the combobox
                int item_id = int.Parse(cbItem.Text.Split(']')[0].Substring(1));
                var item = ih.GetItem(item_id);
                // Fill up the category combobox
                cbCategory.SelectedIndex = item.Category == "general" ? 0 : 1;
                // Fill up the price textbox if the item and quantity are selected/entered.
                if (int.TryParse(txtQuantity.Text, out int _))
                    txtPrice.Text = (item.UnitPrice * int.Parse(txtQuantity.Text)).ToString();

                // Fill up the status with default value if it is empty.
                if (txtStatus.Text == string.Empty)
                    txtStatus.Text = "Unpaid";

                return;
            }
            txtPrice.Text = string.Empty;
            txtStatus.Text = string.Empty;
        }

        private void btnClear_Click(object sender, System.EventArgs e)
        {
            selected_transaction = DateTime.MinValue;
            txtTransactionID.Text = string.Empty;
            cbItem.SelectedIndex = -1;
            cbCategory.SelectedIndex = -1;
            txtPrice.Text = string.Empty;
            txtQuantity.Text = string.Empty;
            txtStatus.Text = string.Empty;
            txtNotes.Text = string.Empty;
            UpdateTransactionsList(null);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateValues())
                return;

            sh.AddTransaction(
                new Transaction
                {
                    Id = DateTime.Now,
                    ItemId = int.Parse(cbItem.Text.Split(']')[0].Substring(1)),
                    Category = cbCategory.SelectedIndex == 0 ? "general" : "electronic",
                    Quantity = int.Parse(txtQuantity.Text),
                    Price = double.Parse(txtPrice.Text),
                    Status = txtStatus.Text,
                    Notes = txtNotes.Text,
                }
            );

            MessageBox.Show(
                "Transaction added successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
            btnClear.PerformClick();
            UpdateTransactionsList(null);
        }

        private void cbItem_SelectedIndexChanged(object sender, EventArgs e) => FillUpValues();

        private void txtQuantity_TextChanged(object sender, EventArgs e) => FillUpValues();

        private void dgvSales_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return; // do nothing if the header is clicked
            var trans_id = (DateTime)dgvSales.Rows[e.RowIndex].Cells["id"].Value;
            var transaction = sh.GetTransaction(trans_id);

            selected_transaction = trans_id;
            txtTransactionID.Text = trans_id.ToString();
            cbItem.SelectedIndex = cbItem.FindString($"[{transaction.ItemId}]");
            cbCategory.SelectedIndex = transaction.Category == "general" ? 0 : 1;
            txtPrice.Text = transaction.Price.ToString();
            txtQuantity.Text = transaction.Quantity.ToString();
            txtStatus.Text = transaction.Status;
            txtNotes.Text = transaction.Notes;

            // Change the status button text depending on the status of the transaction
            btnStatus.Text = transaction.Status == "Unpaid" ? "PAID" : "UNPAID";
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selected_transaction == DateTime.MinValue)
            {
                MessageBox.Show(
                    "Please select a transaction to delete.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            if (
                MessageBox.Show(
                    "Are you sure you want to delete this transaction?",
                    "Delete Transaction",
                    MessageBoxButtons.YesNo
                ) == DialogResult.No
            )
                return;

            sh.DeleteTransaction(selected_transaction);
            MessageBox.Show(
                "Transaction deleted successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
            btnClear.PerformClick();
            UpdateTransactionsList(null);
        }

        private void btnStatus_Click(object sender, EventArgs e)
        {
            if (selected_transaction == DateTime.MinValue)
            {
                MessageBox.Show(
                    "Please select a transaction to update.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            var transaction = sh.GetTransaction(selected_transaction);
            transaction.Status = transaction.Status == "Unpaid" ? "Paid" : "Unpaid";
            sh.UpdateTransaction(transaction);
            btnClear.PerformClick();
            UpdateTransactionsList(null);
        }

        private void btnModify_Click(object sender, EventArgs e)
        {
            if (selected_transaction == DateTime.MinValue)
            {
                MessageBox.Show(
                    "Please select a transaction to update.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            if (!ValidateValues())
                return;

            sh.UpdateTransaction(
                new Transaction
                {
                    Id = selected_transaction,
                    ItemId = int.Parse(cbItem.Text.Split(']')[0].Substring(1)),
                    Category = cbCategory.SelectedIndex == 0 ? "general" : "electronic",
                    Quantity = int.Parse(txtQuantity.Text),
                    Price = double.Parse(txtPrice.Text),
                    Status = txtStatus.Text,
                    Notes = txtNotes.Text,
                }
            );

            MessageBox.Show(
                "Transaction updated successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
            btnClear.PerformClick();
            UpdateTransactionsList(null);
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            var searchbar = new Searchbar();
            searchbar.searchbar_title = "Search Transactions";
            var query = searchbar.ShowDialog();
            if (query == DialogResult.OK)
                UpdateTransactionsList(searchbar.query);
        }
    }
}
