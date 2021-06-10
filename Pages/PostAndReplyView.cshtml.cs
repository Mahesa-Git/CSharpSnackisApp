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
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Pages
{
    public class PostAndReplyViewModel : PageModel
    {
        private readonly SnackisAPI _client;

        private readonly ILogger<IndexModel> _logger;
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
        public string PostID { get; set; }
        [BindProperty]
        public string ReplyID { get; set; }
        [BindProperty(SupportsGet = true)]
        public string TextID { get; set; }
        [BindProperty]
        public string AddOrRemoveReaction { get; set; }
        [BindProperty]
        public IFormFile UploadFile { get; set; }

        public PostAndReplyViewModel(ILogger<IndexModel> logger, SnackisAPI client)
        {
            _logger = logger;
            _client = client;
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
                    
                    IActionResult resultPage = await OnGetAsync();
                    ModelState.Clear();
                    Title = null;
                    BodyText = null;
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
                   
                    IActionResult resultPage = await OnGetAsync();
                    ModelState.Clear();
                    Title = null;
                    BodyText = null;
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
        public async Task<IActionResult> OnPostDeletePost()
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
                HttpResponseMessage response = await _client.DeleteAsync($"/Post/DeletePost/{PostID}");
                var request = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return RedirectToPage("/index");
                }
                else
                {
                    Message = "Det gick inte att radera posten";
                    return Page();
                }

            }
            else
            {
                Message = "Det gick inte att radera posten";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostDeleteReply()
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
                HttpResponseMessage response = await _client.DeleteAsync($"/Post/DeleteReply/{ReplyID}");
                var request = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    IActionResult resultPage = await OnGetAsync();
                    return resultPage;
                }
                else
                {
                    Message = "Det gick inte att radera posten";
                    return Page();
                }
            }
            else
            {
                Message = "Det gick inte att radera posten";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostEditPost()
        {

            byte[] tokenByte;
            HttpContext.Session.TryGetValue(TokenChecker.TokenName, out tokenByte);
            string token = Encoding.ASCII.GetString(tokenByte);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");

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
                IActionResult resultPage = await OnGetAsync();
                return resultPage;
            }
            else
            {
                Message = "Kunde inte ändra posten";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostEditReply()
        {
            byte[] tokenByte;
            HttpContext.Session.TryGetValue(TokenChecker.TokenName, out tokenByte);
            string token = Encoding.ASCII.GetString(tokenByte);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");

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
                IActionResult resultPage = await OnGetAsync();
                return resultPage;
            }
            else
            {
                Message = "Kunde inte ändra svaret";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostReportPost()
        {
            byte[] tokenByte;
            HttpContext.Session.TryGetValue(TokenChecker.TokenName, out tokenByte);
            string token = Encoding.ASCII.GetString(tokenByte);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");
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
            byte[] tokenByte;
            HttpContext.Session.TryGetValue(TokenChecker.TokenName, out tokenByte);
            string token = Encoding.ASCII.GetString(tokenByte);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");
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
            byte[] tokenByte;
            HttpContext.Session.TryGetValue(TokenChecker.TokenName, out tokenByte);
            string token = Encoding.ASCII.GetString(tokenByte);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");

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
                var result = await OnGetAsync();
                return result;
            }
            else
            {
                Message = "Kunde inte registrera reaktion.";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostRemoveReaction()
        {

            byte[] tokenByte;
            HttpContext.Session.TryGetValue(TokenChecker.TokenName, out tokenByte);
            string token = Encoding.ASCII.GetString(tokenByte);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");

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
                var result = await OnGetAsync();
                return result;
            }
            else
            {
                Message = "Kunde inte avregistrera reaktion.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAddReplyReaction()
        {
            byte[] tokenByte;
            HttpContext.Session.TryGetValue(TokenChecker.TokenName, out tokenByte);
            string token = Encoding.ASCII.GetString(tokenByte);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");

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
                var result = await OnGetAsync();
                return result;
            }
            else
            {
                Message = "Kunde inte registrera reaktion.";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostRemoveReplyReaction()
        {

            byte[] tokenByte;
            HttpContext.Session.TryGetValue(TokenChecker.TokenName, out tokenByte);
            string token = Encoding.ASCII.GetString(tokenByte);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");

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
                var result = await OnGetAsync();
                return result;
            }
            else
            {
                Message = "Kunde inte avregistrera reaktion.";
                return Page();
            }
        }
    }
}

   