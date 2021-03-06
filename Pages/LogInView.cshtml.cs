using CSharpSnackisApp.Models.ResponseModels;
using CSharpSnackisApp.Models.Toolbox;
using CSharpSnackisApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Pages
{
    public class LogInViewModel : PageModel
    {
        private readonly SnackisAPI _client;
        [BindProperty]
        public string Username { get; set; }
        [BindProperty]
        public string Password { get; set; }
        public string Message { get; set; }
        public string MessageMail { get; set; }
        public int MyProperty { get; set; }

        public LogInViewModel(SnackisAPI client)
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
               {"user", $"{Username}"},
               {"password", $"{Password}"}
            };

            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync("UserAuth/login", content);
            string request = response.Content.ReadAsStringAsync().Result;

            if (request == "No user or password matched, try again.")
            {
                Message = "Fel e-post/anv?ndarnamn eller l?senord, f?rs?k igen.";
                return Page();
            }
            if (request == "No such user exists")
            {
                Message = "Ingen s?dan anv?ndare finns registrerad.";
                return Page();
            }
            if (request == "banned")
            {
                Message = "Du ?r bannlyst. Skriv ett mail om du tycker det ?r or?ttvist. Shame! Shame!";
                return Page();
            }
            else
            {
                LoginResponseModel result = JsonConvert.DeserializeObject<LoginResponseModel>(request);

                byte[] tokenInByte = Encoding.ASCII.GetBytes(result.Token);

                HttpContext.Session.Set("_Token", tokenInByte);
                HttpContext.Session.SetString("Role", result.Role);
                HttpContext.Session.SetString("Id", result.UserID);
                HttpContext.Session.SetString("UserName", Username);

            }

            return RedirectToPage("/index");
        }
    }
}
