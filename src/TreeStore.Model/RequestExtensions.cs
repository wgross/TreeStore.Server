using System;
using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    public static class RequestExtensions
    {
        public static void Apply(this UpdateCategoryRequest updateCategoryRequest, CategoryModel category)
        {
            category.Name = updateCategoryRequest.Name;
        }

       

        
    }
}