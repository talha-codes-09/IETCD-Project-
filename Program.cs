using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IETCD
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<IETCD.Data.ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<IETCD.Data.ApplicationDbContext>()
            .AddDefaultTokenProviders();

            var app = builder.Build();

            // Apply pending migrations automatically on startup
            using (var migrationScope = app.Services.CreateScope())
            {
                var dbContext = migrationScope.ServiceProvider.GetRequiredService<IETCD.Data.ApplicationDbContext>();
                await dbContext.Database.MigrateAsync();
            }

            // Seed roles, admin user, categories and tags
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var dbContext = scope.ServiceProvider.GetRequiredService<IETCD.Data.ApplicationDbContext>();

                string[] roles = { "Admin", "Student" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }

                string adminEmail = "admin@ietcd.com";
                string adminPassword = "Admin@123";

                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                    await userManager.CreateAsync(adminUser, adminPassword);
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }

                if (!dbContext.Categories.Any())
                {
                    var taxonomy = new Dictionary<string, string[]>
                    {
                        ["Teaching & Academics"] = new[] { "Literature", "Adult Education", "Architecture", "Classroom Management", "Climate Change", "Educational Psychology", "Human Anatomy", "Motivation", "Music Theory", "Psychology", "Teaching", "Writing Skills" },
                        ["Engineering & Construction"] = new[] { "Risk Management", "Construction", "Electrical Engineering", "Carpentry", "Automotive Engineering", "Operations", "Auditing", "Compliance", "Engineering", "Health And Safety", "ISO", "Kaizen", "Kanban" },
                        ["Sales & Marketing"] = new[] { "Entrepreneurship", "Management", "Digital Marketing", "Advertising", "Amazon", "Content Marketing", "Data Security", "Ethics", "Market Research", "Marketing Strategy", "Presentation Skills", "Product Marketing", "Retail" },
                        ["Personal Development"] = new[] { "Fitness", "Psychology", "Finance", "Music", "Photography", "Anxiety", "Communication Skills", "Depression", "Diet", "DSLR", "Health", "Mental Health", "Mindset" },
                        ["Management"] = new[] { "Operations", "Accounting", "Supervision", "Auditing", "Health and Safety", "Human Resources", "ISO", "Lean", "Manufacturing", "Motivation", "Nursing", "Productivity", "Project Management" },
                        ["Business"] = new[] { "Human Resources", "Operations", "Supply Chain Management", "Customer Service", "Manufacturing", "Health And Safety", "Quality Management", "E-commerce", "Management", "Sales", "Accounting", "Hospitality", "Communication Skills" },
                        ["Language"] = new[] { "English Language", "Spanish Language", "German Language", "Irish Language", "French Language", "Chinese Language", "Swedish Language", "Japanese Language", "Business English", "English Conversation", "English For Stem", "English Literature", "English Pronunciation" },
                        ["Health"] = new[] { "Mental Health", "Healthcare", "Nursing", "Caregiving", "Nutrition", "Food Safety", "Pharmacology", "Dementia", "Disability", "Health And Fitness", "Hygiene", "Management", "Physical Therapy" },
                        ["Information Technology (IT)"] = new[] { "Network Security", "Programming", "Information Systems", "Management", "Engineering", "Data Science", "Databases", "Administration", "AWS", "Business Management", "CCNA", "Comptia", "Computer Networking" }
                    };

                    foreach (var entry in taxonomy)
                    {
                        var category = new IETCD.Models.Category { Name = entry.Key };
                        foreach (var tagName in entry.Value)
                        {
                            category.Tags.Add(new IETCD.Models.Tag { Name = tagName });
                        }
                        dbContext.Categories.Add(category);
                    }

                    await dbContext.SaveChangesAsync();
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}