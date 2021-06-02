using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Models.Toolbox
{
    public static class TokenChecker
    {
        public static string TokenName = "_Token";
        public static string LoggedInUserID { get; set; }
        public static string userID { get; set; }
        public static string ActiveRole { get; set; }
        public static bool UserStatus { get; set; }
        public static string UserName { get; set; }
        public static string ThreadID { get; set; }
        public static string TopicID { get; set; }
        public static string CategoryID{ get; set; }

    }
}
