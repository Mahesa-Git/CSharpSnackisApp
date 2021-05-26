using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CSharpSnackisApp.Models.ResponseModels;
using CSharpSnackisApp.Services;
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

        public TopicViewModel(ILogger<IndexModel> logger, SnackisAPI client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task<IActionResult> OnGetAsync()
        {
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
    }
}
