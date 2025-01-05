using SIA.Models;
using System;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

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
            if (ModelState.IsValid)
            {
                try
                {
                   // Call the stored procedure to create a new user
                   string hashedPassword = HashPassword(model.Password); // Hash the password
                    
                   using (SqlConnection connection = new SqlConnection(dbConnect))
                    {
                        connection.Open();

                        using (SqlCommand command = new SqlCommand("CreateAccount", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            //Add parameters to the command
                            command.Parameters.AddWithValue("@USERNAME", model.Username);
                            command.Parameters.AddWithValue("@PASSWORD", hashedPassword);
                            command.Parameters.AddWithValue("@PASSWORD", model.FirstName);
                            command.Parameters.AddWithValue("@PASSWORD", model.LastName);
                            command.Parameters.AddWithValue("@PASSWORD", model.Role);

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
            }
            else
            {
                TempData["Error"] = "Please contact the IT Support to fix the errors.";
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
                string hashedPassword = HashPassword(password);

                using (SqlConnection connection = new SqlConnection(dbConnect))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand("*LogIn SP name*", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@USERNAME", username);
                        command.Parameters.AddWithValue("@PASSWORD", hashedPassword);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if(reader.HasRows)
                            {
                                reader.Read();
                                Session["Username"] = reader["Username"].ToString();
                                Session["Role"] = reader["Role"].ToString();
                                
                                //Create trap to check roles to redirect
                                //the user to the page they can access
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

        // Placeholder for password hashing
        private string HashPassword(string password)
        {
            // Implement a secure password hashing mechanism
            return password;//Placeholder
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
