using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Project.Web.Entities;
using Xams.Core.Entities;

namespace Project.Web.Utils;

public class UserUtil
{
    public static async Task<Guid> GetUserId(HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.Claims
            .Where(x => x.Type == ClaimTypes.NameIdentifier)
            .Select(x => x.Value).FirstOrDefault();
        var emailClaim = httpContext.User.Claims
            .Where(x => x.Type == ClaimTypes.Email)
            .Select(x => x.Value).FirstOrDefault();

        if (userIdClaim == null)
        {
            throw new Exception("UserId not found");
        }
        
        using (var db = new DataContext())
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.FirebaseId == userIdClaim) ?? 
                       await db.Users.FirstOrDefaultAsync(x => x.EmailAddress == emailClaim);
            try
            {
                // Attempt to create the user
                if (user == null)
                {
                    user = new AppUser();
                    user.FirebaseId = userIdClaim;
                    user.Name = emailClaim;
                    user.EmailAddress = emailClaim;
                    user.CreatedDate = DateTime.UtcNow;
                    db.Add(user);
                    await db.SaveChangesAsync();

                    var userRole = new UserRole<AppUser, Role>();
                    userRole.UserId = user.UserId;
                    userRole.RoleId = new Guid("0526627a-ac19-4228-a512-8a817edd4e95"); // User role
                    db.Add(userRole);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                // Wait a variable amount of time
                var rnd = new Random(DateTime.Now.Millisecond);
                await Task.Delay(rnd.Next(20, 150));
                // Reattempt to retrieve
                user = await db.Users.FirstOrDefaultAsync(x => x.FirebaseId == userIdClaim) ?? 
                       await db.Users.FirstOrDefaultAsync(x => x.EmailAddress == emailClaim);
                if (user == null)
                {
                    user = new AppUser();
                    user.FirebaseId = userIdClaim;
                    user.Name = emailClaim;
                    user.EmailAddress = emailClaim;
                    user.CreatedDate = DateTime.UtcNow;
                    db.Add(user);
                    await db.SaveChangesAsync();

                    var userRole = new UserRole<AppUser, Role>();
                    userRole.UserId = user.UserId;
                    userRole.RoleId = new Guid("0526627a-ac19-4228-a512-8a817edd4e95"); // User role
                    db.Add(userRole);
                    await db.SaveChangesAsync();
                }
            }

            return user.UserId;
        }
    }
}