using System;
using System.Collections.Generic;

namespace Assignment3.Shared
{ 
    public class CategoryService
{
    private List<Category> _categories;

    public CategoryService()
    {
        // Initialize the in-memory list of categories
        _categories = new List<Category>
        {
            new Category { Id = 1, Name = "Beverages" },
            new Category { Id = 2, Name = "Condiments" },
            new Category { Id = 3, Name = "Confections" }
        };
    }

    // Return all categories
    public List<Category> GetCategories()
    {
        return _categories;
    }

    // Find one category by ID
    public Category? GetCategory(int cid)
    {
        for (int i = 0; i < _categories.Count; i++)
        {
            if (_categories[i].Id == cid)
            {
                return _categories[i];
            }
        }
        return null;
    }

    // Add new category
    public bool CreateCategory(int id, string name)
    {
        for (int i = 0; i < _categories.Count; i++)
        {
            if (_categories[i].Id == id)
            {
                return false; // Category with this ID already exists
            }
        }

        Category newCategory = new Category();
        newCategory.Id = id;
        newCategory.Name = name;
        _categories.Add(newCategory);
        return true;
    }

    // Update existing category
    public bool UpdateCategory(int id, string newName)
    {
        for (int i = 0; i < _categories.Count; i++)
        {
            if (_categories[i].Id == id)
            {
                _categories[i].Name = newName;
                return true;
            }
        }
        return false; // Category not found
    }

    // Delete category
    public bool DeleteCategory(int id)
    {
        for (int i = 0; i < _categories.Count; i++)
        {
            if (_categories[i].Id == id)
            {
                _categories.RemoveAt(i);
                return true;
            }
        }
        return false; // Category not found
    }
}
}
