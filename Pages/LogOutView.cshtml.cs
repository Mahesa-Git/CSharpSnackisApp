using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpSnackisApp.Models.Toolbox;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CSharpSnackisApp.Pages
{
    public class LogOutViewModel : PageModel
    {
        public IActionResult OnGet()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/index");
        }
    }
}
