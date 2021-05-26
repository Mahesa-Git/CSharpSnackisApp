using CSharpSnackisApp.Models.Entities;
using System;
using System.Collections.Generic;

namespace CSharpSnackisApp.Models.ResponseModels
{

    public class CategoryResponseModel
    {
        public string categoryID { get; set; }
        public List<Topic> topics { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public DateTime createDate { get; set; }
    }
}
