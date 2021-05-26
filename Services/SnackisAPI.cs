using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Services
{
    public class SnackisAPI : HttpClient
    {
        public SnackisAPI(HttpClient client) : base()
        {
            BaseAddress = new Uri("https://localhost:44302");
        }
    }
}
