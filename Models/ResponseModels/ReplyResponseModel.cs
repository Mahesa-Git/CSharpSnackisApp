using CSharpSnackisApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Models.ResponseModels
{
    public class ReplyResponseModel
    {
            public string replyID { get; set; }
            public string bodyText { get; set; }
            public DateTime createDate { get; set; }
            public bool isReported { get; set; }
            public User user { get; set; }
            public Post post { get; set; }
            public GroupChat groupChat { get; set; }
    }
}
