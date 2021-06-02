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
    public class TopicViewModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public List<TopicResponseModel> _topicResponseModels { get; set; }
        private readonly SnackisAPI _client;
        public string Message { get; set; }

        [BindProperty(SupportsGet = true)]
        public string categoryID { get; set; }
        [BindProperty]
        public string TopicID { get; set; }
        [BindProperty]
        public string Title { get; set; }
        public bool ButtonVisibility { get; set; }
        [BindProperty]
        public string newCategoryID { get; set; }

        public TopicViewModel(ILogger<IndexModel> logger, SnackisAPI client)
        {
            _logger = logger;
            _client = client;
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
                    TokenChecker.CategoryID = categoryID;
                else
                    categoryID = TokenChecker.CategoryID;

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
            string token = null;
            try
            {
                byte[] tokenByte;
                HttpContext.Session.TryGetValue(TokenChecker.TokenName, out tokenByte);
                token = Encoding.ASCII.GetString(tokenByte);
            }
            catch (Exception)
            {
                Message = "Du måste logga in först";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");

            if (!String.IsNullOrEmpty(token))
            {
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
            else
            {
                Message = "Det gick inte att skapa ämnet";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteTopic()
        {
            string token = null;
            try
            {
                byte[] tokenByte;
                HttpContext.Session.TryGetValue(TokenChecker.TokenName, out tokenByte);
                token = Encoding.ASCII.GetString(tokenByte);
            }
            catch (Exception)
            {
                Message = "Du måste logga in först";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");

            if (!String.IsNullOrEmpty(token))
            {
                HttpResponseMessage response = await _client.DeleteAsync($"/AdminPost/DeleteTopic/{TopicID}");
                var request = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    IActionResult resultPage = await OnGetAsync();
                    return resultPage;
                }
                else
                {
                    Message = "Det gick inte att radera ämnet";
                    return Page();
                }

            }
            else
            {
                Message = "Det gick inte att radera ämnet";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostEditTopic()
        {

            byte[] tokenByte;
            HttpContext.Session.TryGetValue(TokenChecker.TokenName, out tokenByte);
            string token = Encoding.ASCII.GetString(tokenByte);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");

            var values = new Dictionary<string, string>()
                 {
                    {"title", $"{Title}"}
                 };
            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync($"/AdminPost/UpdateTopic/{TopicID}", content);

            string request = response.Content.ReadAsStringAsync().Result;

            if (!String.IsNullOrEmpty(token))
            {
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
            else
            {
                Message = "Det gick inte att ändra ämnet";
                return Page();
            }
        }

    }
}
