using SIA.Models;
using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace SIA.Controllers
{
    public class HomeController : Controller
    {
        private string dbConnect = "Data Source=AARON\\SQLEXPRESS;Initial Catalog=SIRBELL;Integrated Security=True;Encrypt=False";

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAccount(AccountViewModel model)
        {
            //Raise error early if modelstate is invalid
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please contact the IT Support to fix the errors.";
                return View(model);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(dbConnect))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("CreateAccount", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        //Add parameters to the command
                        command.Parameters.AddWithValue("@USERNAME", model.Username);
                        command.Parameters.AddWithValue("@PASSWORD", model.Password);
                        command.Parameters.AddWithValue("@FIRSTNAME", model.FirstName);
                        command.Parameters.AddWithValue("@LASTNAME", model.LastName);
                        command.Parameters.AddWithValue("@ROLE", model.Role);

                        //Execute the Command
                        command.ExecuteNonQuery();
                    }
                }

                TempData["Success"] = "Account created successfully!";
                return RedirectToAction("CreateAccount");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while creating the account: " + ex.Message;
            }

            return View(model);
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Messages = new List<string> { "Username and Password are required." };
                return View();
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(dbConnect))
                {
                    connection.Open();

                    //Rename the stored procedure to match the actual name in the database
                    using (SqlCommand command = new SqlCommand("*LogIn SP name*", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@USERNAME", username);
                        command.Parameters.AddWithValue("@PASSWORD", password);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if(reader.HasRows)
                            {
                                reader.Read();
                                Session["Username"] = reader["Username"].ToString();
                                string role = reader["Role"].ToString();
                                Session["Role"] = role;

                                //Trappings to check roles to redirect
                                //the user to the page they can access
                                switch (role)
                                {
                                    case "Admin":
                                        return RedirectToAction("AdminAccounts", "Admin");
                                    case "Purchasing":
                                        return RedirectToAction("Purchasing", "Purchasing");
                                    case "Inventory":
                                        return RedirectToAction("Inventory", "Inventory");
                                    //Default is omitted since the database will only return these three roles
                                }
                            }
                            else
                            {
                                //If no user found, show error message
                                ViewBag.Messages = new List<string> { "Invalid username or password." };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Messages = new List<string> { "An error occurred: " + ex.Message };
            }

            return View();
        }

        public ActionResult AdminAccounts()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //_context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}