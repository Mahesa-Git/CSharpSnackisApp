using System;

namespace CSharpSnackisApp.Models.ResponseModels
{

    public class ThreadResponseModel
    {
        public string threadID { get; set; }
        public object user { get; set; }
        public string title { get; set; }
        public string bodyText { get; set; }
        public DateTime createDate { get; set; }
        public bool isReported { get; set; }
        public Topic topic { get; set; }
        public object posts { get; set; }
    }

    //public class Topic
    //{
    //    public string topicID { get; set; }
    //    public object category { get; set; }
    //    public Thread[] threads { get; set; }
    //    public string title { get; set; }
    //    public DateTime createDate { get; set; }
    //}

    //public class Thread
    //{
    //    public string threadID { get; set; }
    //    public object user { get; set; }
    //    public string title { get; set; }
    //    public string bodyText { get; set; }
    //    public DateTime createDate { get; set; }
    //    public bool isReported { get; set; }
    //    public object posts { get; set; }
    //}


}
