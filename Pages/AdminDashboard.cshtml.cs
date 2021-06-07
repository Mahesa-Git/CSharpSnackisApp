using CSharpSnackisApp.Models.Entities;
using CSharpSnackisApp.Models.Toolbox;
using CSharpSnackisApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Pages
{
    public class AdminDashboardModel : PageModel
    {
        private readonly SnackisAPI _client;
        [BindProperty]
        public string Token { get; set; }
        public string Title { get; set; }
        [BindProperty]
        public string BodyText { get; set; }
        [BindProperty]
        public string TextID { get; set; }
        [BindProperty]
        public string Type { get; set; }
        [BindProperty]
        public string UserID { get; set; }
        public List<object> ReportedObject { get; set; }
        public int[] Statistics { get; set; }
        public string Message { get; set; }
        public List<User> Users { get; set; }
        public AdminDashboardModel(SnackisAPI client)
        {
            _client = client;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                byte[] tokenByte;
                HttpContext.Session.TryGetValue(TokenChecker.TokenName, out tokenByte);
                Token = Encoding.ASCII.GetString(tokenByte);
            }
            catch (Exception)
            {
                Message = "Du måste logga in först";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");
            HttpResponseMessage response = await _client.GetAsync("AdminPost/GetStatistics");
            var request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Statistics = JsonConvert.DeserializeObject<int[]>(request);

                response = await _client.GetAsync("AdminPost/GetReportedObjects");
                request = response.Content.ReadAsStringAsync().Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var allCollections = JsonConvert.DeserializeObject<List<List<object>>>(request);
                    var tempObjects = new List<object>();
                    for (int i = 0; i < 4; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                foreach (var collection in allCollections[i])
                                {
                                    var desCollectionUser = JsonConvert.DeserializeObject<User>(collection.ToString());
                                    tempObjects.Add(desCollectionUser);
                                }
                                break;
                            case 1:
                                foreach (var collection in allCollections[i])
                                {
                                    var desCollectionPost = JsonConvert.DeserializeObject<Post>(collection.ToString());
                                    tempObjects.Add(desCollectionPost);
                                }
                                break;
                            case 2:
                                foreach (var collection in allCollections[i])
                                {
                                    var desCollectionReply = JsonConvert.DeserializeObject<Reply>(collection.ToString());
                                    tempObjects.Add(desCollectionReply);
                                }
                                break;
                            case 3:
                                foreach (var collection in allCollections[i])
                                {
                                    var desCollectionThread = JsonConvert.DeserializeObject<Thread>(collection.ToString());
                                    tempObjects.Add(desCollectionThread);
                                }
                                break;
                        }
                        ReportedObject = tempObjects;

                    }
                    response = await _client.GetAsync("AdminAuth/GetUsers");
                    request = response.Content.ReadAsStringAsync().Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Users = JsonConvert.DeserializeObject<List<User>>(request);
                        return Page();
                    }
                    else
                    {
                        Message = "Kunde inte hämta användarlistan";
                        return Page();
                    }
                }
                return Page();
            }
            else
            {
                Message = "Kunde inte hämta statistiken, kontakta sysadmin";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostBanUser()
        {
            try
            {
                byte[] tokenByte;
                HttpContext.Session.TryGetValue(TokenChecker.TokenName, out tokenByte);
                Token = Encoding.ASCII.GetString(tokenByte);
            }
            catch (Exception)
            {
                Message = "Du måste logga in först";
                return Page();
            }
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");
            HttpResponseMessage response = await _client.GetAsync($"AdminAuth/BanUser/{UserID}");
            var request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = await OnGetAsync();
                return result;
            }
            else
            {
                Message = "Kunde inte banna nu, kontakta sysadmin";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostUnbanUser()
        {
            try
            {
                byte[] tokenByte;
                HttpContext.Session.TryGetValue(TokenChecker.TokenName, out tokenByte);
                Token = Encoding.ASCII.GetString(tokenByte);
            }
            catch (Exception)
            {
                Message = "Du måste logga in först";
                return Page();
            }
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");
            HttpResponseMessage response = await _client.GetAsync($"AdminAuth/UnbanUser/{UserID}");
            var request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = await OnGetAsync();
                return result;
            }
            else
            {
                Message = "Kunde inte tillåta användaren nu, kontakta sysadmin";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostReviewDone()
        {
            try
            {
                byte[] tokenByte;
                HttpContext.Session.TryGetValue(TokenChecker.TokenName, out tokenByte);
                Token = Encoding.ASCII.GetString(tokenByte);
            }
            catch (Exception)
            {
                Message = "Du måste logga in först";
                return Page();
            }
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");

            var values = new Dictionary<string, string>()
                 {
                    {"type", $"{Type}"},
                    {"textID", $"{TextID}"}
                 };
            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync($"AdminAuth/ReviewDone", content);
            var request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = await OnGetAsync();
                return result;
            }
            else
            {
                Message = "Kunde inte registrera den godkända rapporten, kontakta sysadmin";
                return Page();
            }
        }
    }
}
