using CSharpSnackisApp.Models.Entities;
using System;

namespace CSharpSnackisApp.Models.ResponseModels
{
    public class ReplyResponseModel
    {
        public string replyID { get; set; }
        public string bodyText { get; set; }
        public DateTime createDate { get; set; }
        public DateTime editDate { get; set; }
        public bool isReported { get; set; }
        public bool ButtonVisibility { get; set; }
        public string Image { get; set; }
        public User user { get; set; }
        public Post post { get; set; }
        public GroupChat groupChat { get; set; }
        public PostReactionModel postReactions { get; set; }
    }
}
