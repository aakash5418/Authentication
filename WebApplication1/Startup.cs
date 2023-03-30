using Microsoft.OpenApi.Models;
using WebApplication1.Authorication;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WebApplication1
{
    public class Startup
    {
        public Startup (IWebHostEnvironment environment, IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }


        //use this method to add services to container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            //Entity Framework 

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("ShoppingDb")));

            // for Identity

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();



            //Adding Authentication

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

            })

            //Adding JwtBearer 

            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                //var validAudience = Configuration["JWT:ValidAudience"];
                //var ValidIssuer = Configuration["JWT:ValidIssuer"];
                //var secret = Configuration["JWT:Secret"];
                //var IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]));
                
            options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = Configuration["JWT:ValidAudience"],
                    ValidIssuer = Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secert"]))
                };
            });


            //services.AddDbContext<>
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("api", new OpenApiInfo
                {
                    Title = "Customer Api",
                    Version = "v1",
                    Description = "Authentication & Authorization in ASP.Net Core 5.0 with JWT & Swagger"
                });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat= "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer'[space] and then valid token in the text below.\r\n\n\r\nExample: \"Bearer kjhksjfhoihndnleiruoijalkjklajdklaj\""
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{}
                    }

                });
            });
        }


        //use this method to configure http request pipeline.
        
        public void Configure(IWebHostEnvironment env, IApplicationBuilder app)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("api/swagger.json", "Customer Names"));

            app.UseCors(builder =>
            {
               builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            });
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                   
            });

        }

       
    }
}
