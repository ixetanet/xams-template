using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MyXProject.Web.Entities;
using Xams.Core.Entities;
using Xams.Core.Startup;

namespace MyXProject.Web.Utils;

public class UserUtil
{
    private static ConcurrentDictionary<string, Guid> _users = new();
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
            var userId = await GetUserId(db, userIdClaim, emailClaim);
            try
            {
                // Attempt to create the user
                if (userId == Guid.Empty)
                {
                    userId = await CreateUser(db, userIdClaim, emailClaim);
                }
            }
            catch (Exception e)
            {
                // Wait a variable amount of time in case multiple
                // requests are trying to create the user at once
                var rnd = new Random(DateTime.Now.Millisecond);
                await Task.Delay(rnd.Next(20, 150));
                // Reattempt to retrieve
                userId = await GetUserId(db, userIdClaim, emailClaim);
                if (userId == Guid.Empty)
                {
                    userId = await CreateUser(db, userIdClaim, emailClaim);
                }
            }

            return userId;
        }
    }

    private static async Task<Guid> GetUserId(DataContext db, string? userIdClaim, string? emailClaim)
    {
        Guid userId;

        if (userIdClaim != null)
        {
            if (_users.TryGetValue(userIdClaim, out userId))
            {
                return userId;
            }
        }

        userId =await db.Users
            .Where(x => x.EmailAddress == emailClaim)
            .Select(x => x.UserId)
            .FirstOrDefaultAsync();

        return userId;
    }

    private static async Task<Guid> CreateUser(DataContext db, string userIdClaim, string? emailClaim)
    {
        var user = new AppUser();
        user.Name = emailClaim;
        user.EmailAddress = emailClaim;
        user.CreatedDate = DateTime.UtcNow;
        db.Add(user);
        await db.SaveChangesAsync();

        // Assign default Roles
        if (emailClaim == "xxx@xxx.io")
        {
            var adminRole = new UserRole<AppUser, Role>();
            adminRole.UserId = user.UserId;
            adminRole.RoleId = SystemRecords.SystemAdministratorRoleId; // Admin role
            db.Add(adminRole);
        }

        await db.SaveChangesAsync();

        _users.TryAdd(userIdClaim, user.UserId);

        return user.UserId;
    }
}