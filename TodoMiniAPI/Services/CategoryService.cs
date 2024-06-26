﻿using Microsoft.EntityFrameworkCore;
using TodoMiniAPI.Data;
using TodoMiniAPI.Models;

namespace TodoMiniAPI.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;    
        }

        public async Task<List<Category>> UpdateCategoriesAsync(List<Category> categories, Type type)
        {
            string objectType = type.Name;
            var resultCategories = new List<Category>();

            foreach (var category in categories)
            {
                Category? existingCategory = null;

                if (category.Id == 0)
                {
                    // find by Name
                    existingCategory = await _context.Categories
                        .FirstOrDefaultAsync(x => x.ObjectType == objectType && x.Text == category.Text);
                }
                else
                {
                    // find by Id
                    existingCategory = await _context.Categories.FindAsync(category.Id);

                    if(existingCategory != null && existingCategory.Text != category.Text)
                    {
                        // update category 
                        existingCategory.Text = category.Text;
                    }
                }

                if (existingCategory != null)
                {
                    resultCategories.Add(existingCategory);
                }
                else
                {
                    // create a new
                    var newCategory = new Category
                    {
                        Text = category.Text,
                        ObjectType = objectType,
                        Color = category.Color,
                    };

                    _context.Categories.Add(newCategory);
                    resultCategories.Add(newCategory);
                }
            }

            await _context.SaveChangesAsync();

            return resultCategories;
        }
    }
}
