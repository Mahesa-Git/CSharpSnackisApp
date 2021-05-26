using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Models.Entities
{
    public class GroupChat
    {
        public string GroupChatID { get; set; }
        public List<User> Users { get; set; }
        public List<Reply> Replies { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
