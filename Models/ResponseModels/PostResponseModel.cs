using CSharpSnackisApp.Models.Entities;
using System;
using System.Collections.Generic;

namespace CSharpSnackisApp.Models.ResponseModels
{
    public class PostResponseModel
    {
        public string postID { get; set; }
        public string title { get; set; }
        public string bodyText { get; set; }
        public string image { get; set; }
        public DateTime createDate { get; set; }
        public DateTime editDate { get; set; }
        public bool isReported { get; set; }
        public bool ButtonVisibility { get; set; }
        public User user { get; set; }
        public Thread thread { get; set; }
        public List<PostReaction> postReactions { get; set; }
        public List<ReplyResponseModel> replies { get; set; }

    }
}
