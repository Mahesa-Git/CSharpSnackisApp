using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Models.Entities
{
    public class PostReaction
    {
        public string PostReactionID { get; set; }
        public Post Post { get; set; }
        public Reply Reply { get; set; }
        public int TypeID { get; set; }
        public int Counter { get; set; }
    }
}
