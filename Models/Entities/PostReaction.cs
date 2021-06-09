using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Models.Entities
{
    public class PostReaction
    {
        public string PostReactionID { get; set; }
        public List<string> UserIds { get; set; }
        public int LikeCounter { get; set; }
    }
}
