using System;
using System.Collections.Generic;

namespace CSharpSnackisApp.Models.ResponseModels
{
    public class TopicResponseModel
    {
        public string topicID { get; set; }
        public Category category { get; set; }
        public object threads { get; set; }
        public string title { get; set; }
        public DateTime createDate { get; set; }
    }

    public class Category
    {
        public string categoryID { get; set; }
        public Topic[] topics { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public DateTime createDate { get; set; }
    }

    public class Topic
    {
        public string topicID { get; set; }
        public object threads { get; set; }
        public string title { get; set; }
        public DateTime createDate { get; set; }
    }


}
