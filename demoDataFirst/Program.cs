﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using demoDataFirst.Data;
using demoDataFirst.Repositories;
using demoDataFirst.Models;
using demoDataFirst.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace demoDataFirst
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<IceScreamProject3Context>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext") ?? throw new InvalidOperationException("Connection string 'DataContext' not found.")));
            
            // Add GenericRepositories
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            
            // Add services user
            builder.Services.AddScoped<IUserService, UserService>();
            // Add service Authentication
            builder.Services.AddScoped<IAuthService, AuthService>();
            // Add service OrderService
            builder.Services.AddScoped<IOrderService, OrderService>();
            // Add service OrderDetail
            builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
            // Add service Product
            builder.Services.AddScoped<IProductService, ProductService>();
            // Add Service RecipeIngredient
            builder.Services.AddScoped<IRecipeIngredientService, RecipeIngredientService>();
            // Add Service RecipeStep
            builder.Services.AddScoped<IRecipeStepService, RecipeStepService>();
            // Add Service Membership
            builder.Services.AddScoped<IMembershipService, MembershipService>();
            // Add Service Transaction
            builder.Services.AddScoped<ITransactionService, TransactionService>();
            //CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    policy => policy.WithOrigins("http://localhost:3000") // Thay domain này bằng domain của Next.js
                                    .AllowAnyHeader()
                                    .AllowAnyMethod());
            });
            //Authentication
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
                var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseStaticFiles();
            app.UseCors("AllowSpecificOrigin");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
