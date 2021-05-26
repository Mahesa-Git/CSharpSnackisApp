using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Services
{
    public class SnackisAPI
    {
        public HttpClient InitClient { get; init; }
        public SnackisAPI()
        {
            InitClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:44302")
            };
        }

    }
}
