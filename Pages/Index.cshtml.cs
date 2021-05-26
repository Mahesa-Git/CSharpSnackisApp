using CSharpSnackisApp.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Pages
{
    public class IndexModel : PageModel
    {
        private HttpClient _client = new HttpClient();
        private readonly ILogger<IndexModel> _logger;
        private CategoryResponseModel _categoryResponseModel;
        public string Message { get; set; }
        public IndexModel(ILogger<IndexModel> logger, CategoryResponseModel categoryResponseModel)
        {
            _logger = logger;
            _categoryResponseModel = categoryResponseModel;
        }

        public async Task<IActionResult> OnGetAsync()
        {

            HttpResponseMessage response = await _client.GetAsync("/Post/ReadCategory");
            string request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _categoryResponseModel = JsonConvert.DeserializeObject<CategoryResponseModel>(request);
            
                return Page();
            }

            return RedirectToPage("/Error");
        }
    }
}
