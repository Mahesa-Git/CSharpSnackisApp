using CSharpSnackisApp.Models.Toolbox;
using CSharpSnackisApp.Services;
using Microsoft.AspNetCore.Http;
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
    public class ChatViewModel : PageModel
    {
        private readonly SnackisAPI _client;
        public string Token { get; set; }
        public string Message { get; set; }
        public string GroupChatID { get; set; }
        public string BodyText { get; set; }

        [BindProperty(SupportsGet = true)]
        public string UserID { get; set; }
        [BindProperty]
        public string RecipantID { get; set; }

        public ChatViewModel(SnackisAPI client)
        {
            _client = client;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            //SELECTED CHATID I TOOLBOX S�TTS G.M ONPOSTSELECTEDCHAT. TA H�R FRAM DEN CHATEN OM DEN != NULL. �R DEN NULL == RENDERA INGET. KOLLA TOKEN
            return Page();
        }
        public async Task<IActionResult> OnPostNewChat() //SKAPA NY CHAT, TAR IN ReciantChatID .. aktiv UserID finns i toolbox eller session. KOLLA TOKEN
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
                Message = "Du m�ste logga in f�rst";
                return Page();
            }
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{Token}");

            if (!String.IsNullOrEmpty(Token))
            {
                var values = new Dictionary<string, string>()
                 {
                    {"value", $"{UserID}"},
                    {"value", $"{RecipantID}"},
                 };
                string payload = JsonConvert.SerializeObject(values);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("", content);

                string request = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {

                    return Page();
                }
                if(response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    Message = "Chatt existerar redan! V�lj den i menyn.";
                    return Page();
                }
                else
                {
                    return Page();
                }
            }
            else
            {
                Message = "Du m�ste logga in f�rst";
                return Page();
            }

        }
        public async Task<IActionResult> OnPostSelectedChat() //VALD CHATT, TAR IN CHATID, S�TT PROPERTY I TOOLBOX K�R EN ONGET. KOLLA TOKEN
        {
            return Page();
        }
        public async Task<IActionResult> OnPostNewReplyInChat() //NY REPLY I VALD CHATT KOLLA TOKEN.
        {
            return Page();
        }
    }
}
