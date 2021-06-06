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
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Pages
{
    public class IndexModel : PageModel
    {
        
        private readonly ILogger<IndexModel> _logger;
        public List<CategoryResponseModel> _categoryResponseModel { get; set; }
        private readonly SnackisAPI _client;
        public string Message { get; set; }

        [BindProperty]
        public string CategoryID { get; set; }

        public bool ButtonVisibility { get; set; }

        [BindProperty]
        public string Description { get; set; }

        [BindProperty]
        public string Title { get; set; }

        public IndexModel(ILogger<IndexModel> logger, SnackisAPI client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                string userRole = HttpContext.Session.GetString("Role");
               
                if(userRole == "root" || userRole == "admin")
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

        public async Task<IActionResult> OnPostDeleteCategory()
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
                HttpResponseMessage response = await _client.DeleteAsync($"/AdminPost/DeleteCategory/{CategoryID}");
                var request = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return RedirectToPage("/Index");
                }
                else
                {
                    Message = "Det gick inte att radera kategorin";
                    return Page();
                }

            }
            else
            {
                Message = "Det gick inte att radera Kategorin";
                return Page();
            }
        }


        public async Task<IActionResult> OnPostCreateCategory()
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
                    {"description", $"{Description}"}
                 };

                string payload = JsonConvert.SerializeObject(values);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync($"/AdminPost/CreateCategory", content);

                var request = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    IActionResult resultPage = await OnGetAsync();
                    return resultPage;
                }
                else
                {
                    Message = "Det gick inte att skapa kategorin";
                    return Page();
                }

            }
            else
            {
                Message = "Det gick inte att skapa kategorin";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostEditCategory()
        {

            byte[] tokenByte;
            HttpContext.Session.TryGetValue(TokenChecker.TokenName, out tokenByte);
            string token = Encoding.ASCII.GetString(tokenByte);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");

            var values = new Dictionary<string, string>()
                 {
                    {"title", $"{Title}"},
                    {"description", $"{Description}"}
                 };
            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync($"/AdminPost/UpdateCategory/{CategoryID}", content);

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
