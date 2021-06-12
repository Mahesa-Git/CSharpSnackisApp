using Microsoft.AspNetCore.Http;
using System;
using System.Text;

namespace CSharpSnackisApp.Models.Toolbox
{
    public static class TokenChecker
    {
        public static string TokenName = "_Token";
        public static string LoggedInUserID { get; set; }
        public static string ActiveRole { get; set; }
        public static bool UserStatus { get; set; }
        public static string UserName { get; set; }
        public static string ThreadID { get; set; }
        public static string CategoryID { get; set; }
    }
}
