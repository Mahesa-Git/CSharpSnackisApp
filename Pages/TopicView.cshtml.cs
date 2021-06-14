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
    public class TopicViewModel : PageModel
    {
        private readonly SessionCheck _sessionCheck;
        private readonly SnackisAPI _client;
        public string Token { get; set; }
        public bool ButtonVisibility { get; set; }
        public string Message { get; set; }
        public List<TopicResponseModel> _topicResponseModels { get; set; }

        [BindProperty(SupportsGet = true)]
        public string categoryID { get; set; }
        [BindProperty]
        public string TopicID { get; set; }
        [BindProperty]
        public string Title { get; set; }

        public TopicViewModel(SnackisAPI client, SessionCheck sessionCheck)
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

                if (categoryID is not null)
                    HttpContext.Session.SetString("TextID", categoryID);
                else
                    categoryID = HttpContext.Session.GetString("TextID");

            }
            catch (Exception)
            {
                ButtonVisibility = false;
            }
            HttpResponseMessage response = await _client.GetAsync($"/Post/ReadTopicsInCategory/{categoryID}");
            var request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _topicResponseModels = JsonConvert.DeserializeObject<List<TopicResponseModel>>(request);

                return Page();
            }
            else
                return RedirectToPage("/Error");
        }
        public async Task<IActionResult> OnPostCreateTopic()
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
                 {"categoryId", $"{categoryID}"}
            };

            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync($"/AdminPost/CreateTopic", content);

            var request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                IActionResult resultPage = await OnGetAsync();
                return resultPage;
            }
            else
            {
                Message = "Det gick inte att skapa ämnet";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostDeleteTopic()
        {
            Token = _sessionCheck.GetSession(HttpContext);
            if (Token == null)
            {
                Message = "Du måste logga in först";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");

            HttpResponseMessage response = await _client.DeleteAsync($"/AdminPost/DeleteTopic/{TopicID}");
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

                IActionResult resultPage = await OnGetAsync();
                return resultPage;
            }
            else
            {
                Message = "Det gick inte att radera ämnet";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostEditTopic()
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
                {"title", $"{Title}"}
            };
            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync($"/AdminPost/UpdateTopic/{TopicID}", content);

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
