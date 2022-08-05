using Accounting.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Accounting.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly SqlConnection connection =
            new(@"Server=(localdb)\MSSQLLocalDB;Database=AccountingBudget;Trusted_Connection=True;");

        private static List<AccountBudget> budgets = new();

        private static bool selectedMonth = false;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        #region Getting all records from database
        //Getting records from database
        public async Task<List<AccountBudget>> GetRecordsAsync()
        {
            connection.Open();

            List<AccountBudget> accountBudgets = new();

            SqlCommand sqlCommandGetData = new("Select * from BudgetAccount", connection);

            using (SqlDataReader reader = sqlCommandGetData.ExecuteReader())
            {
                while (await reader.ReadAsync())
                {
                    AccountBudget accountBudget = new();
                    accountBudget.Budget = reader["Budget"].ToString();
                    accountBudget.Category = reader["Category"].ToString();
                    accountBudget.AmountOfMoney = int.Parse(reader["AmountOfMoney"].ToString());
                    string s = reader["Date"].ToString();
                    accountBudget.Date = DateTime.ParseExact(s, "d.M.yyyy H:m:s", new CultureInfo("en-US"));
                    accountBudget.Comments = reader["Comments"].ToString();
                    accountBudgets.Add(accountBudget);
                }
            }

            connection.Close();

            return accountBudgets;
        }
        #endregion

        #region Uploading data into database
        //Upload data into database
        public IActionResult UploadRecord(AccountBudget budgett)
        {
            try
            {
                connection.Open();
                string date = budgett.Date.ToString("M.d.yyyy H:m:s");
                SqlCommand cmd = new($"INSERT INTO BudgetAccount (Date, Budget, Category, AmountOfMoney, Comments) " +
                    $"VALUES " +
                    $"(N'{date}', N'{budgett.Budget}', N'{budgett.Category}', N'{budgett.AmountOfMoney}', " +
                    $"N'{budgett.Comments}')", connection);

                cmd.ExecuteNonQuery();

                connection.Close();
                if (selectedMonth)
                {
                    if (budgets[0].Date.Month == budgett.Date.Month)
                    {
                        budgets.Add(budgett);
                    }
                }
                else
                {
                    budgets.Add(budgett);
                }
                ViewData["Records"] = budgets;
                ViewData["Calculation"] = CalculatingOverallMoney(budgets);
                return View("Index");

            } 
            catch (Exception)
            {
                return Error();
            }

        }
        #endregion

        #region Sorting data by month
        // Sorting data by given month
        public async Task<IActionResult> SortByMonth(AccountBudget budget)
        {
            if (DateTime.MinValue == budget.Date)
            {
                budgets = await GetRecordsAsync();
                selectedMonth = false;
            }
            else
            {
                var allRecords = await GetRecordsAsync();
                var records = from record in allRecords
                              where record.Date.Month == budget.Date.Month 
                              && record.Date.Year == budget.Date.Year
                              select record;

                budgets = records.ToList();
                selectedMonth = true;
            }

            ViewData["Records"] = budgets;
            ViewData["Calculation"] = CalculatingOverallMoney(budgets);
            return View("Index");
        }
        #endregion

        #region Searching data by given month and data
        //Searching data by given text and by chosen characteristic
        public IActionResult SearchBy(AccountBudget account)
        {
            var accounts = budgets;
            IEnumerable<AccountBudget> records = null;

            switch (account.SearchBy)
            {
                case "AmountOfMoney":
                    records = from record in accounts
                              where record.AmountOfMoney == int.Parse(account.Text)
                              select record;
                    break;
                case "Category":
                    records = from record in accounts
                              where record.Category.Contains(account.Text)
                              select record;
                    break;
                case "Comments":
                    records = from record in accounts
                              where record.Comments.Contains(account.Text)
                              select record;
                    break;
            }

            budgets = records.ToList();
            ViewData["Records"] = budgets;
            ViewData["Calculation"] = CalculatingOverallMoney(budgets);
            return View("Index");
        }
        #endregion
        public int CalculatingOverallMoney(List<AccountBudget> accounts)
        {
            int result = 0;

            foreach (var item in accounts)
            {
                if(item.Budget == "Income")
                {
                    result += item.AmountOfMoney;
                }
                else if (item.Budget == "Debt")
                {
                    result -= item.AmountOfMoney;
                }
            }

            return result;
        }
        public async Task<IActionResult> Index()
        {
            budgets = await GetRecordsAsync();

            selectedMonth = false;
            ViewData["Calculation"] = CalculatingOverallMoney(budgets);
            ViewData["Records"] = budgets;
            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
