using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Models.Toolbox
{
    public class SessionCheck : PageModel
    {
        public string GetSession(HttpContext context)
        {
            try
            {
                byte[] tokenByte;
                context.Session.TryGetValue(TokenChecker.TokenName, out tokenByte);
                string token = Encoding.ASCII.GetString(tokenByte);

                return token;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
