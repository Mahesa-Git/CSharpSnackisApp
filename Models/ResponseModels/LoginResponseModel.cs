using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Models.ResponseModels
{
    public class LoginResponseModel
    {
        public string Token { get; set; }
        public string Expires { get; set; }
        public string UserID { get; set; }
        public string Role { get; set; }
    }
}
