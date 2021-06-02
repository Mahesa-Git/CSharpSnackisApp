using CSharpSnackisApp.Models.Entities;
using System;
using System.Collections.Generic;

namespace CSharpSnackisApp.Models.ResponseModels
{
    public class TopicResponseModel
    {
        public string topicID { get; set; }
        public Category category { get; set; }
        public List<Thread> threads { get; set; }
        public string title { get; set; }
        public DateTime createDate { get; set; }
        public DateTime editDate { get; set; }
    }
}
