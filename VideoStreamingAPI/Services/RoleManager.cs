using Microsoft.AspNetCore.Identity;

namespace VideoStreamingAPI.Services
{
    public class RoleManager
    {
        public async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roleNames = ["admin"];
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExitst = await roleManager.RoleExistsAsync(roleName);
                if (!roleExitst)
                {
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                }

            }
        }
    }
}