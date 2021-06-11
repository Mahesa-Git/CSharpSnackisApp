using CSharpSnackisApp.Models.Entities;
using CSharpSnackisApp.Models.ResponseModels;
using CSharpSnackisApp.Models.Toolbox;
using CSharpSnackisApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Pages
{
    public class UserViewModel : PageModel
    {

        private readonly ILogger<UserViewModel> _logger;
        private readonly SnackisAPI _client;

        [BindProperty(SupportsGet = true)]
        public UserPageResponseModel _userPageResponseModel { get; set; }
        public User user { get; set; }
        public bool ButtonVisibility { get; set; }
        public bool UserButtonVisibility { get; set; }
        [BindProperty]
        public object UserID { get; set; }
        [BindProperty]
        public string ProfileText { get; set; }
        [BindProperty]
        public string Username { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        [BindProperty]
        public string Email { get; set; }
        [BindProperty]
        public string Country { get; set; }

        public UserViewModel(ILogger<UserViewModel> logger, SnackisAPI client)
        {
            _logger = logger;
            _client = client;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            byte[] tokenByte;
            HttpContext.Session.TryGetValue(TokenChecker.TokenName, out tokenByte);
            Token = Encoding.ASCII.GetString(tokenByte);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");

            string userId = null;

            userId = HttpContext.Session.GetString("Id");
            UserID = userId;

            if (!String.IsNullOrEmpty(Token))
            {
                HttpResponseMessage response = await _client.GetAsync($"/UserAuth/Profile/{UserID}");
                var request = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _userPageResponseModel = JsonConvert.DeserializeObject <UserPageResponseModel>(request);

                    return Page();
                }
                else
                    return RedirectToPage("/Error");
            }
            return RedirectToPage("/Error");
        }
        public async Task<IActionResult> OnPostEditProfileText()
        {

            byte[] tokenByte;
            HttpContext.Session.TryGetValue(TokenChecker.TokenName, out tokenByte);
            string token = Encoding.ASCII.GetString(tokenByte);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");

            var values = new Dictionary<string, string>()
                 {
                    {"profileText", $"{ProfileText}"}
                 };
            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync($"UserAuth/userProfileTextUpdate", content);

            string request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                IActionResult resultPage = await OnGetAsync();
                return resultPage;
            }
            else
            {
                Message = "Kunde inte ändra profil texten, försök igen.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostEditUsername()
        {

            byte[] tokenByte;
            HttpContext.Session.TryGetValue(TokenChecker.TokenName, out tokenByte);
            string token = Encoding.ASCII.GetString(tokenByte);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");

            var values = new Dictionary<string, string>()
                 {
                    {"username", $"{Username}"},
                    {"email", $"{Email}"},
                    {"country", $"{Country}" }
                 };
            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync($"UserAuth/userProfileUsernameUpdate", content);

            string request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                HttpContext.Session.Clear();

                LoginResponseModel result = JsonConvert.DeserializeObject<LoginResponseModel>(request);
                TokenChecker.UserName = Username;
                TokenChecker.LoggedInUserID = result.UserID;
                TokenChecker.ActiveRole = result.Role;

                byte[] tokenInByte = Encoding.ASCII.GetBytes(result.Token);

                HttpContext.Session.Set(TokenChecker.TokenName, tokenInByte);
                HttpContext.Session.SetString("Role", result.Role);
                HttpContext.Session.SetString("Id", result.UserID);

                TokenChecker.UserName = Username;

                return RedirectToPage("./UserView");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                string result = JsonConvert.DeserializeObject<string>(request);

                if (result == "Username in use")
                {
                    Message = "Användarnamnet används redan, välj ett annat.";
                    return Page();
                }
                if (result == "E-mail in use")
                {
                    Message = "E-posten används redan, välj en annan.";
                    return Page();
                }
                if (result == "Error, user not found")
                {
                    Message = "Det gick inte att registrera en användare just nu," +
                              " vänligen försök igen senare eller kontakta" +
                              " systemadministratören på MotorDuonForum@gmail.com så ordnar vi detta";
                    return Page();
                }

                return Page();
            }
            else
            {
                Message = "Det gick inte att registrera en användare just nu," +
                               " vänligen försök igen senare eller kontakta" +
                               " systemadministratören på MotorDuonForum@gmail.com så ordnar vi detta";
                return Page();
            }
        }
    }
}
