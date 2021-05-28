using CSharpSnackisApp.Models.ResponseModels;
using CSharpSnackisApp.Models.Toolbox;
using CSharpSnackisApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Pages
{
    public class PostAndReplyViewModel : PageModel
    {
        [BindProperty]
        public string Title { get; set; }
        [BindProperty]
        public string BodyText { get; set; }
        private readonly ILogger<IndexModel> _logger;
        public List<PostResponseModel> _postResponseModel { get; set; }
        public List<ReplyResponseModel> _replyResponseModel { get; set; }
        private readonly SnackisAPI _client;
        public string Message { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ThreadID { get; set; }
        [BindProperty]
        public string PostID { get; set; }

        public PostAndReplyViewModel(ILogger<IndexModel> logger, SnackisAPI client)
        {
            _logger = logger;
            _client = client;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            if (ThreadID is not null)
            {
                TokenChecker.ThreadID = ThreadID;
            }
            else
            {
                ThreadID = TokenChecker.ThreadID;
            }

            HttpResponseMessage response = await _client.GetAsync($"/Post/ReadPostsInThread/{ThreadID}");
            var request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _postResponseModel = JsonConvert.DeserializeObject<List<PostResponseModel>>(request);

                foreach (var post in _postResponseModel)
                {
                    response = await _client.GetAsync($"/Post/ReadRepliesToPost/{post.postID}");
                    request = response.Content.ReadAsStringAsync().Result;
                    _replyResponseModel = JsonConvert.DeserializeObject<List<ReplyResponseModel>>(request);
                    post.replies = _replyResponseModel;
                }
                return Page();

            }
            else
                return RedirectToPage("/Error");
        }
        public async Task<IActionResult> OnPostPost()
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
                    {"threadID", $"{ThreadID}"}
                 };
                string payload = JsonConvert.SerializeObject(values);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("Post/CreatePost", content);

                string request = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {

                    IActionResult resultPage = await OnGetAsync();
                    return resultPage;

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
        public async Task<IActionResult> OnPostReply()
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
                    {"bodyText", $"{BodyText}"},
                    {"postID", $"{PostID}"}
                 };
                string payload = JsonConvert.SerializeObject(values);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("Post/CreateReply", content);

                string request = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    IActionResult resultPage = await OnGetAsync();
                    return resultPage;

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
