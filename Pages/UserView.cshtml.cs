using CSharpSnackisApp.Models.Entities;
using CSharpSnackisApp.Models.ResponseModels;
using CSharpSnackisApp.Models.Toolbox;
using CSharpSnackisApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Pages
{
    public class UserViewModel : PageModel
    {
        private readonly SessionCheck _sessionCheck;

        private readonly SnackisAPI _client;
        [BindProperty]
        public User _user { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SearchedUserID { get; set; }
        [BindProperty(SupportsGet = true)]
        public string TextID { get; set; }
        [BindProperty]
        public UserPageResponseModel _userPageResponseModel { get; set; }
        [BindProperty]
        public IFormFile UploadFile { get; set; }
        public bool ButtonVisibility { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }

        public UserViewModel(SnackisAPI client, User user, SessionCheck sessionCheck)
        {
            _client = client;
            _user = user;
            _sessionCheck = sessionCheck;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            Token = _sessionCheck.GetSession(HttpContext);
            if (Token == null)
            {
                Message = "Du måste logga in först";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");

            _user.Id = HttpContext.Session.GetString("Id");
            if (SearchedUserID is not null)
                _user.Id = SearchedUserID;
            else if (TextID is not null)
                _user.Id = TextID;

            HttpResponseMessage response = await _client.GetAsync($"/UserAuth/Profile/{_user.Id}");
            var request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _userPageResponseModel = JsonConvert.DeserializeObject<UserPageResponseModel>(request);
                return Page();
            }
            else
                return RedirectToPage("/Error");
        }
        public async Task<IActionResult> OnPostEditProfileText()
        {
            Token = _sessionCheck.GetSession(HttpContext);
            if (Token == null)
            {
                Message = "Du måste logga in först";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");

            var values = new Dictionary<string, string>()
            {
                {"id", $"{_user.Id}"},
                {"profileText", $"{_user.ProfileText}"}
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
            Token = _sessionCheck.GetSession(HttpContext);
            if (Token == null)
            {
                Message = "Du måste logga in först";
                return Page();
            }
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");
            var values = new Dictionary<string, string>()
            {
                {"id", $"{_user.Id}"},
                {"username", $"{_user.UserName}"},
                {"email", $"{_user.Email}"},
                {"country", $"{_user.Country}"}
            };

            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PutAsync($"UserAuth/userProfileUpdate", content);
            string request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if (request != "admin changed successfully")
                {
                    HttpContext.Session.Clear();
                    LoginResponseModel result = JsonConvert.DeserializeObject<LoginResponseModel>(request);
                    byte[] tokenInByte = Encoding.ASCII.GetBytes(result.Token);

                    HttpContext.Session.Set("_Token", tokenInByte);
                    HttpContext.Session.SetString("Role", result.Role);
                    HttpContext.Session.SetString("Id", result.UserID);
                    HttpContext.Session.SetString("UserName", _user.UserName);
                    return RedirectToPage("./UserView");

                }
                else
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
        public async Task<IActionResult> OnPostEditImage()
        {
            Token = _sessionCheck.GetSession(HttpContext);
            if (Token == null)
            {
                Message = "Du måste logga in först";
                return Page();
            }
            if (UploadFile is not null)
            {
                var file = Directory.GetFiles("./wwwroot/img/").Select(x => _user.Image).FirstOrDefault();
                if (file is not null)
                {
                    System.IO.File.Delete($"./wwwroot/img/{file}");
                }

                string path = "./wwwroot/img/";
                string newFile = Guid.NewGuid().ToString() + UploadFile.FileName;
                using (var fileStream = new FileStream($"{path}{newFile}", FileMode.Create))
                {
                    await UploadFile.CopyToAsync(fileStream);

                }

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");

                HttpResponseMessage response = await _client.GetAsync($"UserAuth/userImageUpdate/{newFile}");
                string request = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    return RedirectToPage("./UserView");
                else
                {
                    Message = "Kunde inte uppdatera bilden.";
                    return Page();
                }
            }
            else
                return RedirectToPage("./UserView");
        }
        public async Task<IActionResult> OnPostDeleteImage()
        {
            Token = _sessionCheck.GetSession(HttpContext);
            if (Token == null)
            {
                Message = "Du måste logga in först";
                return Page();
            }
            if (_user.Image is not null)
            {
                var file = Directory.GetFiles("./wwwroot/img/").Select(x => _user.Image).FirstOrDefault();
                System.IO.File.Delete($"./wwwroot/img/{file}");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");

            HttpResponseMessage response = await _client.GetAsync($"UserAuth/userImageDelete/{_user.Image}");
            string request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return RedirectToPage("./UserView");

            else
            {
                Message = "Kunde inte radera bilden.";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostReportUser()
        {
            Token = _sessionCheck.GetSession(HttpContext);
            if (Token == null)
            {
                Message = "Du måste logga in först";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");

            HttpResponseMessage response = await _client.GetAsync($"AdminAuth/ReportUser/{_user.Id}");
            string request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Message = "Användaren rapporterad";
                return Page();
            }
            else
            {
                Message = "Kunde inte rapportera användaren.";
                return Page();
            }
        }
    }
}
