using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CSharpSnackisApp.Models.ResponseModels;
using CSharpSnackisApp.Models.Toolbox;
using CSharpSnackisApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

        public ThreadViewModel(ILogger<IndexModel> logger, SnackisAPI client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            HttpResponseMessage response = await _client.GetAsync($"/Post/ReadThreadsInTopic/{TopicID}");
            var request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _threadResponseModels = JsonConvert.DeserializeObject<List<ThreadResponseModel>>(request);

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
                Message = "Du måste logga in först";
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

                if(response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    values.Remove("topicID");
                    values.Add("threadID", $"{request}");
                    values.Add("isThreadStart", $"{true}");
                    payload = JsonConvert.SerializeObject(values);
                    content = new StringContent(payload, Encoding.UTF8, "application/json");
                    response = await _client.PostAsync("Post/CreatePost", content);
                    request = response.Content.ReadAsStringAsync().Result;

                    if(response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        IActionResult resultPage = await OnGetAsync();
                        return resultPage;
                    }
                    else
                    {
                        Message = "Något gick tvärfel";
                        IActionResult resultPage = await OnGetAsync();
                        return resultPage;
                    }
                }
                else
                {
                    Message = "Ej behörig";
                    return Page();
                }
            }
            else
            {
                Message = "Du måste logga in först";
                return Page();
            }
        }
    }
}
