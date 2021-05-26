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
    public class PostAndReplyViewModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public List<PostResponseModel> _postResponseModel { get; set; }
        public List<ReplyResponseModel> _replyResponseModel { get; set; }
        private readonly SnackisAPI _client;
        public string Message { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ThreadID { get; set; }

        public PostAndReplyViewModel(ILogger<IndexModel> logger, SnackisAPI client)
        {
            _logger = logger;
            _client = client;
        }
        public async Task<IActionResult> OnGetAsync()
        {
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
    }
}
