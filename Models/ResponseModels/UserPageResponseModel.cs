using CSharpSnackisApp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Models.ResponseModels
{
    public class UserPageResponseModel
    {
        public string Id { get; set; }
        public string Country { get; set; }
        public string MailToken { get; set; }
        public DateTime CreateDate { get; set; }
        public string ProfileText { get; set; }
        public bool IsBanned { get; set; }
        public string Image { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public List<Post> Posts { get; set; }
        public List<Reply> Replies { get; set; }
        public List<Thread> Threads { get; set; }
        public List<GroupChat> GroupChats { get; set; }

    }
}
