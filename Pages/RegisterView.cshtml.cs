using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CSharpSnackisApp.Models;
using CSharpSnackisApp.Models.Entities;
using CSharpSnackisApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace CSharpSnackisApp.Pages
{
    public class RegisterViewModel : PageModel
    {
        public string Message { get; set; }
        private readonly SnackisAPI _client;
        public string ErrorMessage { get; set; }
        public string MessageUserName { get; set; }
        public string MessageMail { get; set; }

        [BindProperty]
        public RegisterModel User { get; set; }

        [BindProperty]
        public IFormFile UploadFile { get; set; }

        public RegisterViewModel(SnackisAPI client)
        {
            _client = client;
        }
        public void OnGet()
        {

        }
        public async Task<IActionResult> OnPostAsync()
        {
            var file = Guid.NewGuid().ToString() + UploadFile.FileName;
            string path = "./wwwroot/img/";
            if (UploadFile != null)
            {
                using (var fileStream = new FileStream($"{path}{file}", FileMode.Create))
                {
                    await UploadFile.CopyToAsync(fileStream);
                }
            }
            var values = new Dictionary<string, string>()
                 {
                    {"username", $"{User.Username}"},
                    {"email", $"{User.Email}"},
                    {"password", $"{User.Password}"},
                    {"country", $"{User.Country}"},
                    {"image", $"{file}" }
                 };
            string payload = JsonConvert.SerializeObject(values);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("UserAuth/register", content);

            string request = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                if (request == "wrong username")
                {
                    MessageUserName = "V�nligen v�lj ett anv�ndarnamn som inte inneh�ller mellanslag";
                    return Page();
                }
                if (request == "Username in use")
                {
                    MessageUserName = "Anv�ndarnamnet anv�nds redan, testa ett annat";
                    return Page();
                }
                if (request == "E-mail in use")
                {
                    MessageMail = "Mailen anv�nds redan, testa en annan";
                    return Page();
                }
                if (request == "Registration failed")
                {
                    ErrorMessage = "Det gick inte att registrera en anv�ndare just nu," +
                                   " v�nligen f�rs�k igen senare eller kontakta " +
                                   "systemadministrat�ren p� csharpsnackis@gmail.com s� ordnar vi detta";
                    return Page();
                }
            }
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //ToolBox.userID = request;
                //return RedirectToPage("/RegisterConfirmation");
                return RedirectToPage("/index");
            }
            else
            {
                ErrorMessage = "Det gick inte att registrera en anv�ndare just nu," +
                               " v�nligen f�rs�k igen senare eller kontakta" +
                               " systemadministrat�ren p� csharpsnackis@gmail.com s� ordnar vi detta";
                return Page();
            }
        }
    }
}
