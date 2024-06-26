﻿using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TodoMiniAPI.Data;
using TodoMiniAPI.Helpers;
using TodoMiniAPI.Models;
using TodoMiniAPI.Services;

namespace TodoMiniAPI.Endpoints
{
    public static class TodosEndpoints
    {
        public static void Map(WebApplication app)
        {
            var todoItemsEndpoint = app.MapGroup("/todoitems");

            todoItemsEndpoint.MapGet("", async (AppDbContext db, HttpRequest request) =>
            {
                if (request.Headers.TryGetValue("Pagination", out var paginationHeaderValues))
                {
                    var paginationHeaderJson = paginationHeaderValues.FirstOrDefault();
                    var paginationHeader = JsonSerializer.Deserialize<PaginationHeader>(paginationHeaderJson);

                    return await PageList<Todo>.CreateAsync(
                        db.Todos.AsQueryable(),
                        paginationHeader.currentPage,
                        paginationHeader.pageSize);
                }


                return await db.Todos.ToListAsync();
            });

            todoItemsEndpoint.MapGet("count", async (AppDbContext db) => await db.Todos.CountAsync());

            todoItemsEndpoint.MapGet("{id:int}", async (int id, AppDbContext db) => await db.Todos.FindAsync(id));

            todoItemsEndpoint.MapPost("", async (Todo todo, AppDbContext db, ICategoryService categoryService) =>
            {
                var categories = await categoryService.UpdateCategoriesAsync(todo.Categories, typeof(Todo));
                todo.Categories = categories;

                db.Todos.Add(todo);

                await db.SaveChangesAsync();
                return Results.Created($"/todoitems/{todo.Id}", todo);
            });

            todoItemsEndpoint.MapPut("{id:int}", async (int id, Todo inputTodo, AppDbContext db, ICategoryService categoryService) =>
            {
                var todo = await db.Todos
                    .Include(t=>t.Categories)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (todo == null) return Results.NotFound();

                todo.IsComplete = inputTodo.IsComplete;
                todo.Description = inputTodo.Description;
                todo.Title = inputTodo.Title;

                var categories = await categoryService.UpdateCategoriesAsync(inputTodo.Categories, typeof(Todo));
                todo.Categories = categories;

                await db.SaveChangesAsync();

                return Results.Ok(todo);
            });

            todoItemsEndpoint.MapPut("setComplete/{id:int}", async (int id, Todo selectedTodo, AppDbContext db) =>
            {
                var todo = await db.Todos.FindAsync(id);
                if (todo == null) return Results.NotFound();
                todo.IsComplete = selectedTodo.IsComplete;
                await db.SaveChangesAsync();
                return Results.Ok(todo);
            });

            todoItemsEndpoint.MapDelete("{id:int}", async (int id, AppDbContext db) =>
            {
                if (await db.Todos.FindAsync(id) is Todo todo)
                {
                    db.Todos.Remove(todo);
                    await db.SaveChangesAsync();
                    return Results.NoContent();
                }
                return Results.NotFound();
            });
        }
    }
}
