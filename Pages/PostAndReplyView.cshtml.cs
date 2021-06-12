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
    public class PostAndReplyViewModel : PageModel
    {
        private readonly SnackisAPI _client;
        private readonly SessionCheck _sessionCheck;
        private readonly ILogger<IndexModel> _logger;
        public string Token { get; set; }
        public List<PostResponseModel> _postResponseModel { get; set; }
        public List<ReplyResponseModel> _replyResponseModel { get; set; }
        public PostReactionModel _postReactionModel { get; set; }
        public string Message { get; set; }
        public string ReportMessage { get; set; }
        public bool ButtonVisibility { get; set; }
        public bool UserButtonVisibility { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ThreadID { get; set; }

        [BindProperty]
        public string Title { get; set; }
        [BindProperty]
        public string BodyText { get; set; }
        [BindProperty]
        public string Image { get; set; }
        [BindProperty]
        public string PostID { get; set; }
        [BindProperty]
        public string ReplyID { get; set; }
        [BindProperty(SupportsGet = true)]
        public string TextID { get; set; }
        [BindProperty]
        public string AddOrRemoveReaction { get; set; }
        [BindProperty]
        public IFormFile UploadFile { get; set; }

        public PostAndReplyViewModel(ILogger<IndexModel> logger, SnackisAPI client, SessionCheck sessionCheck)
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
                    ButtonVisibility = true;
                else
                    ButtonVisibility = false;
            }
            catch (Exception)
            {
                ButtonVisibility = false;
            }
            if (userId is null)
                UserButtonVisibility = false;
            else
                UserButtonVisibility = true;

            if (ThreadID is not null)
                TokenChecker.ThreadID = ThreadID;
            else
                ThreadID = TokenChecker.ThreadID;

            if (TextID is not null)
            {
                ThreadID = TextID;
            }

            HttpResponseMessage response = await _client.GetAsync($"/Post/ReadPostsInThread/{ThreadID}");
            var request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _postResponseModel = JsonConvert.DeserializeObject<List<PostResponseModel>>(request);
                if (userId is not null)
                {
                    foreach (var model in _postResponseModel)
                    {
                        if (userId == model.user.Id)
                            model.ButtonVisibility = true;
                    }
                }
                foreach (var post in _postResponseModel)
                {
                    HttpResponseMessage reactionResponse = await _client.GetAsync($"Post/ReadPostReaction/{post.postID}");
                    var reactionRequest = reactionResponse.Content.ReadAsStringAsync().Result;

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        post.postReactions = JsonConvert.DeserializeObject<PostReactionModel>(reactionRequest);
                    }
                    else
                    {
                        Message = "Detta gick åt pipan";
                        return Page();
                    }

                    response = await _client.GetAsync($"/Post/ReadRepliesToPost/{post.postID}");
                    request = response.Content.ReadAsStringAsync().Result;

                    _replyResponseModel = JsonConvert.DeserializeObject<List<ReplyResponseModel>>(request);

                    foreach (var reply in _replyResponseModel)
                    {
                        HttpResponseMessage reactionReplyResponse = await _client.GetAsync($"Post/ReadReplyReaction/{reply.replyID}");
                        var reactionReplyRequest = reactionReplyResponse.Content.ReadAsStringAsync().Result;

                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            reply.postReactions = JsonConvert.DeserializeObject<PostReactionModel>(reactionReplyRequest);
                        }
                        else
                        {
                            Message = "Detta gick åt pipan";
                            return Page();
                        }
                    }
                    if (userId is not null)
                    {
                        foreach (var model in _replyResponseModel)
                        {
                            if (userId == model.user.Id)
                                model.ButtonVisibility = true;
                        }
                    }
                    post.replies = _replyResponseModel;
                }
                return Page();
            }
            else
                return RedirectToPage("/Error");
        }
        public async Task<IActionResult> OnPostPost()
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
                    {"threadID", $"{ThreadID}"},
                    {"image", $"{file}"}
                 };
            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("Post/CreatePost", content);

            string request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                ModelState.Clear();
                Title = null;
                BodyText = null;
                return RedirectToPage("/PostAndReplyView");
            }
            else
            {
                Message = "Ej behörig";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostReply()
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
                    {"bodyText", $"{BodyText}"},
                    {"postID", $"{PostID}"},
                    {"image", $"{file}"}
                 };
            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("Post/CreateReply", content);

            string request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                ModelState.Clear();
                Title = null;
                BodyText = null;
                return RedirectToPage("/PostAndReplyView");

            }
            else
            {
                Message = "Ej behörig";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostDeletePost()
        {
            Token = _sessionCheck.GetSession(HttpContext);
            if (Token == null)
            {
                Message = "Du måste logga in först";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");

            HttpResponseMessage response = await _client.DeleteAsync($"/Post/DeletePost/{PostID}");
            var request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if (Image is not null)
                    FileDelete.DeleteImage(Image);

                var ReplyImages = JsonConvert.DeserializeObject<List<string>>(request);
                if (ReplyImages is not null)
                {
                    foreach (var image in ReplyImages)
                    {
                        FileDelete.DeleteImage(image);
                    }
                }
                return RedirectToPage("/index");
            }
            else
            {
                Message = "Det gick inte att radera posten";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostDeleteReply()
        {
            Token = _sessionCheck.GetSession(HttpContext);
            if (Token == null)
            {
                Message = "Du måste logga in först";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");

            HttpResponseMessage response = await _client.DeleteAsync($"/Post/DeleteReply/{ReplyID}");
            var request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if (Image is not null)
                    FileDelete.DeleteImage(Image);

                return RedirectToPage("/PostAndReplyView");
            }
            else
            {
                Message = "Det gick inte att radera posten";
                return Page();
            }

        }
        public async Task<IActionResult> OnPostEditPost()
        {

            Token = _sessionCheck.GetSession(HttpContext);
            if (Token == null)
            {
                Message = "Du måste logga in först";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");

            var values = new Dictionary<string, string>()
                 {
                    {"title", $"{Title}"},
                    {"bodyText", $"{BodyText}"}
                 };
            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync($"Post/UpdatePost/{PostID}", content);

            string request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return RedirectToPage("/PostAndReplyView");
            }
            else
            {
                Message = "Kunde inte ändra posten";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostEditReply()
        {
            Token = _sessionCheck.GetSession(HttpContext);
            if (Token == null)
            {
                Message = "Du måste logga in först";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");

            var values = new Dictionary<string, string>()
                 {
                    {"bodyText", $"{BodyText}"}
                 };
            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync($"Post/UpdateReply/{ReplyID}", content);

            string request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return RedirectToPage("/PostAndReplyView");
            }
            else
            {
                Message = "Kunde inte ändra svaret";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostReportPost()
        {
            Token = _sessionCheck.GetSession(HttpContext);
            if (Token == null)
            {
                Message = "Du måste logga in först";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");
            HttpResponseMessage response = await _client.GetAsync($"Post/ReportPost/{PostID}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                ReportMessage = "Rapport skickad!";
                IActionResult resultPage = await OnGetAsync();
                return resultPage;
            }
            else
            {
                Message = "Kunde inte skicka rapport, testa senare.";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostReportReply()
        {
            Token = _sessionCheck.GetSession(HttpContext);
            if (Token == null)
            {
                Message = "Du måste logga in först";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");
            HttpResponseMessage response = await _client.GetAsync($"Post/ReportReply/{ReplyID}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                ReportMessage = "Rapport skickad!";
                IActionResult resultPage = await OnGetAsync();
                return resultPage;
            }
            else
            {
                Message = "Kunde inte skicka rapport, testa senare.";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostAddReaction()
        {
            Token = _sessionCheck.GetSession(HttpContext);
            if (Token == null)
            {
                Message = "Du måste logga in först";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");

            var values = new Dictionary<string, string>()
                 {
                    {"textID", $"{TextID}"},
                    {"addOrRemove", $"{AddOrRemoveReaction}"}
                 };
            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync("Post/ReactToPost", content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                TextID = null;
                return RedirectToPage("/PostAndReplyView");
            }
            else
            {
                Message = "Kunde inte registrera reaktion.";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostRemoveReaction()
        {
            Token = _sessionCheck.GetSession(HttpContext);
            if (Token == null)
            {
                Message = "Du måste logga in först";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");

            var values = new Dictionary<string, string>()
                 {
                    {"textID", $"{TextID}"},
                    {"addOrRemove", $"{AddOrRemoveReaction}"}
                 };
            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync("Post/ReactToPost", content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                TextID = null;
                return RedirectToPage("/PostAndReplyView");
            }
            else
            {
                Message = "Kunde inte avregistrera reaktion.";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostAddReplyReaction()
        {
            Token = _sessionCheck.GetSession(HttpContext);
            if (Token == null)
            {
                Message = "Du måste logga in först";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");

            var values = new Dictionary<string, string>()
                 {
                    {"textID", $"{TextID}"},
                    {"addOrRemove", $"{AddOrRemoveReaction}"}
                 };
            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync("Post/ReactToReply", content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                TextID = null;
                return RedirectToPage("/PostAndReplyView");
            }
            else
            {
                Message = "Kunde inte registrera reaktion.";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostRemoveReplyReaction()
        {

            Token = _sessionCheck.GetSession(HttpContext);
            if (Token == null)
            {
                Message = "Du måste logga in först";
                return Page();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");

            var values = new Dictionary<string, string>()
                 {
                    {"textID", $"{TextID}"},
                    {"addOrRemove", $"{AddOrRemoveReaction}"}
                 };
            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync("Post/ReactToReply", content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                TextID = null;
                return RedirectToPage("/PostAndReplyView");
            }
            else
            {
                Message = "Kunde inte avregistrera reaktion.";
                return Page();
            }
        }
    }
}

