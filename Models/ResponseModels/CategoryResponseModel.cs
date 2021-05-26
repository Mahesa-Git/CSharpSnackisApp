using System;

namespace CSharpSnackisApp.Models.ResponseModels
{
    public class CategoryResponseModel
    {
        public Category[] Categories { get; set; }
    }

    public class Category
    {
        public string CategoryID { get; set; }
        public object Topics { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
