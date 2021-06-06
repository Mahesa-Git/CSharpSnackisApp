using CSharpSnackisApp.Models.Entities;
using CSharpSnackisApp.Models.Toolbox;
using CSharpSnackisApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
    public class ChatViewModel : PageModel
    {
        private readonly SnackisAPI _client;
        public List<User> Users { get; set; }
        public List<GroupChat> GroupChats { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }
        public GroupChat SelectedGroupChat { get; set; }
        [BindProperty]
        public string GroupChatID { get; set; }
        [BindProperty]
        public string BodyText { get; set; }

        [BindProperty(SupportsGet = true)]
        public string UserID { get; set; }
        [BindProperty]
        public string RecipantID { get; set; }
        [BindProperty]
        public List<string> RecipantIDs { get; set; }

        public ChatViewModel(SnackisAPI client)
        {
            _client = client;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                UserID = HttpContext.Session.GetString("Id");
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

            if (!String.IsNullOrEmpty(Token))
            {
                HttpResponseMessage response = await _client.GetAsync("/Chat/GetUsers");
                string request = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Users = JsonConvert.DeserializeObject<List<Models.Entities.User>>(request);

                    response = await _client.GetAsync("/Chat/GetChats");
                    request = response.Content.ReadAsStringAsync().Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        GroupChats = JsonConvert.DeserializeObject<List<GroupChat>>(request);
                        SelectedGroupChat = GroupChats.Where(x => x.GroupChatID == GroupChatID).FirstOrDefault();
                        return Page();
                    }
                    else
                    {
                        Message = "Kunde inte hämta info, kontrollera att du är inloggad";
                        return Page();
                    }
                }
            }
            else
            {
                Message = "Kunde inte hämta info, kontrollera att du är inloggad";
                return Page();
            }
            return Page();
        }
        public async Task<IActionResult> OnPostNewChat()
        {
            try
            {
                UserID = HttpContext.Session.GetString("Id");
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

            if (!String.IsNullOrEmpty(Token))
            {
                string payload = JsonConvert.SerializeObject(RecipantIDs);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("/Chat/NewChat", content);

                string request = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return RedirectToPage("/ChatView");
                }
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    IActionResult result = await OnGetAsync();
                    Message = "Chatt existerar redan! Välj den i menyn.";
                    return result;
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Message = "Ej behörig att skapa chatt.";
                    return Page();
                }
                else
                {
                    return Page();
                }
            }
            else
            {
                Message = "Du måste logga in först";
                return Page();
            }
        }
        public async Task<IActionResult> OnPostSelectedChat() //VALD CHATT, TAR IN CHATID, SÄTT PROPERTY I TOOLBOX KÖR EN ONGET. KOLLA TOKEN
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
           
            var result = await OnGetAsync();
            return result;
        }
        public async Task<IActionResult> OnPostNewReplyInChat() //NY REPLY I VALD CHATT KOLLA TOKEN.
        {
            try
            {
                UserID = HttpContext.Session.GetString("Id");
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

            if (!String.IsNullOrEmpty(Token))
            {
                var values = new Dictionary<string, string>()
                 {
                    {"groupChatID", $"{GroupChatID}"},
                    { "bodyText",$"{BodyText}"}
                 };
                string payload = JsonConvert.SerializeObject(values);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("/Chat/NewReply", content);

                string request = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    IActionResult result = await OnGetAsync();
                    ModelState.Clear();
                    BodyText = null;
                    GroupChatID = GroupChatID;
                    return result;
                }
                else
                {
                    return Page();
                }

            }
            return Page();
        }
    }
}
