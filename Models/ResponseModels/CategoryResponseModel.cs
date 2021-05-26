using System;

namespace CSharpSnackisApp.Models.ResponseModels
{

    public class CategoryResponseModel
    {
        public string categoryID { get; set; }
        public object topics { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public DateTime createDate { get; set; }
    }
}
