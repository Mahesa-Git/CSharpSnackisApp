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
    public class ThreadViewModel : PageModel
    {
        [BindProperty]
        public string Title { get; set; }
        [BindProperty]
        public string BodyText { get; set; }

        private readonly ILogger<IndexModel> _logger;
        public List<ThreadResponseModel> _threadResponseModels { get; set; }
        private readonly SnackisAPI _client;
        public string Message { get; set; }

        [BindProperty(SupportsGet = true)]
        public string TopicID { get; set; }
        public bool ButtonVisibility { get; set; }
        [BindProperty]
        public string ThreadID { get; set; }

        public ThreadViewModel(ILogger<IndexModel> logger, SnackisAPI client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            string userId = null;
            try
            {
                string userRole = HttpContext.Session.GetString("Role");
                userId = HttpContext.Session.GetString("Id");

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
            HttpResponseMessage response = await _client.GetAsync($"/Post/ReadThreadsInTopic/{TopicID}");
            var request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _threadResponseModels = JsonConvert.DeserializeObject<List<ThreadResponseModel>>(request);
                if (userId is not null)
                {
                    foreach (var model in _threadResponseModels)
                    {
                        if (userId == model.user.Id)
                        {
                            model.ButtonVisibility = true;
                        }
                    }
                }
                return Page();
            }
            else
                return RedirectToPage("/Error");
        }
        public async Task<IActionResult> OnPostAsync()
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
                Message = "Du m�ste logga in f�rst";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");

            if (!String.IsNullOrEmpty(token))
            {
                var values = new Dictionary<string, string>()
                 {
                    {"title", $"{Title}"},
                    {"bodyText", $"{BodyText}"},
                    {"topicID", $"{TopicID}"}
                 };
                string payload = JsonConvert.SerializeObject(values);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("Post/CreateThread", content);

                string request = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    values.Remove("topicID");
                    values.Add("threadID", $"{request}");
                    values.Add("isThreadStart", $"{true}");
                    payload = JsonConvert.SerializeObject(values);
                    content = new StringContent(payload, Encoding.UTF8, "application/json");
                    response = await _client.PostAsync("Post/CreatePost", content);
                    request = response.Content.ReadAsStringAsync().Result;

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        IActionResult resultPage = await OnGetAsync();
                        return resultPage;
                    }
                    else
                    {
                        Message = "N�got gick tv�rfel";
                        IActionResult resultPage = await OnGetAsync();
                        return resultPage;
                    }
                }
                else
                {
                    Message = "Ej beh�rig";
                    return Page();
                }
            }
            else
            {
                Message = "Du m�ste logga in f�rst";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostDeleteThread()
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
                Message = "Du m�ste logga in f�rst";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");

            if (!String.IsNullOrEmpty(token))
            {
                HttpResponseMessage response = await _client.DeleteAsync($"/Post/DeleteThread/{ThreadID}");
                var request = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return RedirectToPage("/Index");
                }
                else
                {
                    Message = "Det gick inte att radera tr�den";
                    return Page();
                }

            }
            else
            {
                Message = "Det gick inte att radera tr�den";
                return Page();
            }
        }
    }
}
