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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CSharpSnackisApp.Pages
{
    public class TopicViewModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public List<TopicResponseModel> _topicResponseModels { get; set; }
        private readonly SnackisAPI _client;
        public string Message { get; set; }

        [BindProperty (SupportsGet = true)]
        public string categoryID { get; set; }
        [BindProperty]
        public string TopicID { get; set; }
        [BindProperty]
        public string Title { get; set; }
        public bool ButtonVisibility { get; set; }

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
                    return RedirectToPage("/Index");
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
    }
}
