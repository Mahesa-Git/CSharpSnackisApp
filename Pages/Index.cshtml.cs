﻿using CSharpSnackisApp.Models.ResponseModels;
using CSharpSnackisApp.Services;
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
        
        private readonly ILogger<IndexModel> _logger;
        public List<CategoryResponseModel> _categoryResponseModel { get; set; }
        private readonly SnackisAPI _client;
        public string Message { get; set; }
        public IndexModel(ILogger<IndexModel> logger, SnackisAPI client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task<IActionResult> OnGetAsync()
        {    
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
    }
}
