using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _client;
        private readonly ILogger<IndexModel> _logger;
        public IndexModel(ILogger<IndexModel> logger, HttpClient client)
        {
            _logger = logger;
            _client = client;
        }

        public void OnGet()
        {

        }
    }
}
