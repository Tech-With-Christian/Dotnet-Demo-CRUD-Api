﻿using Application.Services.Categories;
using Data.Database;
using Domain.Entitites.Categories;
using Domain.Helpers.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Categories
{
    internal class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _db;

        public CategoryService(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// This method will start by checking if the category has already been created in the database.<br />
        /// If it already exists, it will throw an exception of type CustomException with an error message.<br />
        /// If the category doesn't exist, it will be added and saved in the database and then returned to the requesting method.
        /// </summary>
        /// <param name="category">Category to create in the database</param>
        /// <returns>The category that has been created in the database</returns>
        /// <exception cref="CustomException">Throws if the category has already been added</exception>
        public async Task<Category> CreateCategoryAsync(Category category)
        {
            // Make sure that we do not have any categories in the database with the same name
            if (await _db.Categories.AnyAsync(x => x.Name == category.Name))
                throw new CustomException($"A category with the name {category.Name} already exists.");

            // Add and save the category in the database
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            return category;
        }

        /// <summary>
        ///  This method will request the helper method to make sure that the category exists in the database<br />
        ///  IF the category exists it will be removed and the database will be updated.
        /// </summary>
        /// <param name="id">ID of the category to delete</param>
        public async Task DeleteCategoryAsync(Guid id)
        {
            Category category = await getCategoryByIdAsync(id);
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Async method that enumerates the Categories asynchronously and returns them as an IEnumerable.
        /// </summary>
        /// <returns>List of categories from database</returns>
        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            List<Category> categories = await _db.Categories.ToListAsync();

            if (categories.Count() == 0)
            {
                throw new KeyNotFoundException("There are no categories in the database. Please add a category and try again.");
            }

            return categories;
        }

        /// <summary>
        /// Get a category by it's ID.
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>A category</returns>
        public async Task<Category> GetCategoryByIdAsync(Guid id)
        {
            return await getCategoryByIdAsync(id);
        }

        /// <summary>
        /// Get a category by it's name.
        /// </summary>
        /// <param name="name">Category Name</param>
        /// <returns>A category</returns>
        public async Task<Category> GetCategoryByNameAsync(string name)
        {
            return await getCategoryByNameAsync(name);
        }

        /// <summary>
        /// This method will start by checking if the category exist in the database.<br />
        /// It will then continue to validate the category to update and make sure that no category already exist with the same name
        /// and that the category we would like to update isn't of the same name as the update data.<br />
        /// Finally it will update the entity in the database and return the updated values.
        /// </summary>
        /// <param name="categoryToUpdate">The category we would like to update</param>
        /// <returns>The updated category</returns>
        /// <exception cref="CustomException">Thrown if a category in the database with the same name already exists</exception>
        public async Task<Category> UpdateCategoryAsync(Category categoryToUpdate)
        {
            // Make sure that the category exist in the database
            Category category = await getCategoryByIdAsync(categoryToUpdate.Id);

            // Validation before updating the category
            // We don't want any categories in the database with the same name
            if (category.Name != categoryToUpdate.Name && await _db.Categories.AnyAsync(x => x.Name == categoryToUpdate.Name))
            {
                throw new CustomException($"A category with the name {categoryToUpdate.Name} already exist in the database. Please choose another name.");
            }

            // No problems, let's update the category
            _db.Categories.Update(categoryToUpdate);
            await _db.SaveChangesAsync();
            return categoryToUpdate;
        }

        // Helper methods

        /// <summary>
        /// Get a category by the id of the category.
        /// </summary>
        /// <param name="id">ID for category</param>
        /// <returns>The category matching the ID provided for the request.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the category wasn't found in the database</exception>
        private async Task<Category> getCategoryByIdAsync(Guid id)
        {
            Category category = await _db.Categories.FindAsync(id);
            if (category == null) throw new KeyNotFoundException("The category was not found in the database.");
            return category;
        }

        /// <summary>
        /// Get a category by it's name.
        /// </summary>
        /// <param name="name">Name of the category</param>
        /// <returns>The category matching the name provided for the request.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the category wasn't found in the database</exception>
        private async Task<Category> getCategoryByNameAsync(string name)
        {
            Category category = await _db.Categories.FindAsync(name);
            if (category == null) throw new KeyNotFoundException("The category was not found in the database.");
            return category;
        }
    }
}