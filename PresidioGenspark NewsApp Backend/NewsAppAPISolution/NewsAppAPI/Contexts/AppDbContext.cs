﻿using Microsoft.EntityFrameworkCore;
using NewsAppAPI.Models;

namespace NewsAppAPI.Contexts
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<NewsArticle> NewsArticles { get; set; }
    }
}
