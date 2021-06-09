using CSharpSnackisApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Models.ResponseModels
{
    public class PostReactionModel
    {
        public string PostReactionID { get; set; }
        public List<User> Users { get; set; }
        public bool AddOrRemove { get; set; }
        public int LikeCounter { get; set; }
        public PostReactionModel()
        {
            Users = new List<User>();
        }
    }
}
