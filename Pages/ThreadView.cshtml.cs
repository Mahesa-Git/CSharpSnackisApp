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
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Pages
{
    public class ThreadViewModel : PageModel
    {
        private readonly SessionCheck _sessionCheck;
        public string Token { get; set; }
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
        public bool UserButtonVisibility { get; set; }
        [BindProperty]
        public string ThreadID { get; set; }
        [BindProperty(SupportsGet = true)]
        public string TextID { get; set; }
        [BindProperty]
        public IFormFile UploadFile { get; set; }
        public int PostCount { get; set; }

        public ThreadViewModel(ILogger<IndexModel> logger, SnackisAPI client, SessionCheck sessionCheck)
        {
            _logger = logger;
            _client = client;
            _sessionCheck = sessionCheck;
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
            if (userId is null)
                UserButtonVisibility = false;
            else
                UserButtonVisibility = true;

            if (TextID is not null)
            {
                TopicID = TextID;
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
            Token = _sessionCheck.GetSession(HttpContext);
            if (Token == null)
            {
                Message = "Du måste logga in först";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");

            string file = null;
            string path = "./wwwroot/img/";
            if (UploadFile != null)
            {
                file = Guid.NewGuid().ToString() + UploadFile.FileName;
                using (var fileStream = new FileStream($"{path}{file}", FileMode.Create))
                {
                    await UploadFile.CopyToAsync(fileStream);
                }
            }
            var values = new Dictionary<string, string>()
            {
                 {"title", $"{Title}"},
                 {"bodyText", $"{BodyText}"},
                 {"topicID", $"{TopicID}"},
                 {"image", $"{file}"}
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
                    ModelState.Clear();
                    Title = null;
                    BodyText = null;
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
        public async Task<IActionResult> OnPostDeleteThread()
        {
            Token = _sessionCheck.GetSession(HttpContext);
            if (Token == null)
            {
                Message = "Du måste logga in först";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");

            HttpResponseMessage response = await _client.DeleteAsync($"/Post/DeleteThread/{ThreadID}");
            var request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var ReplyAndPostImages = JsonConvert.DeserializeObject<List<string>>(request);
                if (ReplyAndPostImages is not null)
                {
                    foreach (var image in ReplyAndPostImages)
                    {
                        FileDelete.DeleteImage(image);
                    }
                }
                return RedirectToPage("/Index");
            }
            else
            {
                Message = "Det gick inte att radera tråden";
                return Page();
            }
        }
    }
}
