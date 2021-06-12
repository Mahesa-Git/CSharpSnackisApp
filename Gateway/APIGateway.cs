using CSharpSnackisApp.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Gateway
{
    public class APIGateway : Models.IAPIGateway
    {
        private readonly IConfiguration _configuration;
        private readonly SnackisAPI _client;
        public APIGateway(SnackisAPI client, IConfiguration configuration)
        {
            _configuration = configuration;
            _client = client;
        }
    }
}
