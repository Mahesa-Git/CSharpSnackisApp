using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Models.Entities
{
    public class Post
    {
        public string PostID { get; set; }
        public string Title { get; set; }
        public string BodyText { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsReported { get; set; }
        public User User { get; set; }
        public Thread Thread { get; set; }
        public List<PostReaction> PostReactions { get; set; }
        public List<Reply> Replies { get; set; }
    }
}
