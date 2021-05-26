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
    public class ThreadViewModel : PageModel
    {
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
            HttpResponseMessage response = await _client.GetAsync($"/Post/ReadTopicsInCategory/{TopicID}");
            var request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _threadResponseModels = JsonConvert.DeserializeObject<List<ThreadResponseModel>>(request);

                return Page();
            }
            else
                return RedirectToPage("/Error");
        }
    }
}
