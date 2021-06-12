using CSharpSnackisApp.Models.ResponseModels;
using CSharpSnackisApp.Models.Toolbox;
using CSharpSnackisApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SnackisAPI _client;
        private readonly SessionCheck _sessionCheck;
        public string Token { get; set; }
        public string Message { get; set; }
        public List<CategoryResponseModel> _categoryResponseModel { get; set; }
        [BindProperty]
        public string CategoryID { get; set; }
        public bool ButtonVisibility { get; set; }
        [BindProperty]
        public string Description { get; set; }
        [BindProperty]
        public string Title { get; set; }

        public IndexModel(SnackisAPI client, SessionCheck sessionCheck)
        {
            _client = client;
            _sessionCheck = sessionCheck;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                string userRole = HttpContext.Session.GetString("Role");

                if (userRole == "root" || userRole == "admin")
                {
                    ButtonVisibility = true;
                }
                else
                {
                    ButtonVisibility = false;
                }
            }
            catch (Exception)
            {
                ButtonVisibility = false;
            }

            HttpResponseMessage response = await _client.GetAsync("Post/ReadCategory");
            var request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _categoryResponseModel = JsonConvert.DeserializeObject<List<CategoryResponseModel>>(request);

                return Page();
            }
            else
                return RedirectToPage("/Error");
        }
        public async Task<IActionResult> OnPostDeleteCategory()
        {
            Token = _sessionCheck.GetSession(HttpContext);
            if (Token == null)
            {
                Message = "Du måste logga in först";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");

            HttpResponseMessage response = await _client.DeleteAsync($"/AdminPost/DeleteCategory/{CategoryID}");
            var request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var ReplyAndPostImages = JsonConvert.DeserializeObject<List<string>>(request);
                if (ReplyAndPostImages is not null)
                {
                    foreach (var image in ReplyAndPostImages)
                    {
                        FileDelete.DeleteImage(image);
                    }
                }
                return RedirectToPage("/Index");
            }
            else
            {
                Message = "Det gick inte att radera kategorin";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostCreateCategory()
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
                    {"title", $"{Title}"},
                    {"description", $"{Description}"}
                 };

            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync($"/AdminPost/CreateCategory", content);

            var request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                IActionResult resultPage = await OnGetAsync();
                return resultPage;
            }
            else
            {
                Message = "Det gick inte att skapa kategorin";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostEditCategory()
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
                {"title", $"{Title}"},
                {"description", $"{Description}"}
            };
            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync($"/AdminPost/UpdateCategory/{CategoryID}", content);

            string request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                IActionResult resultPage = await OnGetAsync();
                return resultPage;
            }
            else
            {
                Message = "Kunde inte ändra ämnet";
                return Page();
            }
        }
    }
}
