using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CSharpSnackisApp.Models;
using CSharpSnackisApp.Models.Entities;
using CSharpSnackisApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace CSharpSnackisApp.Pages
{
    public class RegisterViewModel : PageModel
    {
        private readonly SnackisAPI _client;
        public string ErrorMessage { get; set; }
        public string MessageUserName { get; set; }
        public string MessageMail { get; set; }

        [BindProperty]
        public RegisterModel User { get; set; }

        public RegisterViewModel(SnackisAPI client)
        {
            _client = client;
        }
        public void OnGet()
        {

        }
        public async Task<IActionResult> OnPostAsync()
        {
            var values = new Dictionary<string, string>()
                 {
                    {"username", $"{User.Username}"},
                    {"email", $"{User.Email}"},
                    {"password", $"{User.Password}"},
                    {"country", $"{User.Country}"},   
                 };
            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("UserAuth/register", content);

            string request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                if (request == "wrong username")
                {
                    MessageUserName = "Vänligen välj ett användarnamn som inte innehåller mellanslag";
                    return Page();
                }
                if (request == "Username in use")
                {
                    MessageUserName = "Användarnamnet används redan, testa ett annat";
                    return Page();
                }
                if (request == "E-mail in use")
                {
                    MessageMail = "Mailen används redan, testa en annan";
                    return Page();
                }
                if (request == "Registration failed")
                {
                    ErrorMessage = "Det gick inte att registrera en användare just nu," +
                                   " vänligen försök igen senare eller kontakta " +
                                   "systemadministratören på csharpsnackis@gmail.com så ordnar vi detta";
                    return Page();
                }
            }
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //ToolBox.userID = request;
                //return RedirectToPage("/RegisterConfirmation");
                return RedirectToPage("/index");
            }
            else
            {
                ErrorMessage = "Det gick inte att registrera en användare just nu," +
                               " vänligen försök igen senare eller kontakta" +
                               " systemadministratören på csharpsnackis@gmail.com så ordnar vi detta";
                return Page();
            }
        }
    }
}
