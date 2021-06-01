using CSharpSnackisApp.Models.Entities;
using System;

namespace CSharpSnackisApp.Models.ResponseModels
{

    public class ThreadResponseModel
    {
        public string threadID { get; set; }
        public User user { get; set; }
        public string title { get; set; }
        public string bodyText { get; set; }
        public DateTime createDate { get; set; }
        public bool isReported { get; set; }
        public Topic topic { get; set; }
        public object posts { get; set; }
        public bool ButtonVisibility { get; set; }
    }
}
