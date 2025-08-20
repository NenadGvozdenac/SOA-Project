using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace tours_service.src.Tours.BuildingBlocks.Infrastructure;

public static class ClaimsPrincipalExtensions 
{
    public static string UserId(this ClaimsPrincipal user) 
    {
        var claim = user.Claims.FirstOrDefault(i => i.Type == "userID" || i.Type == "userid")
            ?? throw new UnauthorizedAccessException("User ID claim not found");
        return claim.Value;
    }

    public static string UserEmail(this ClaimsPrincipal user) 
    {
        var claim = user.Claims.FirstOrDefault(i => i.Type == "userEmail" || i.Type == "useremail")
            ?? throw new UnauthorizedAccessException("User email claim not found");
        return claim.Value;
    }

    public static string UserRole(this ClaimsPrincipal user) 
    {
        var claim = user.Claims.FirstOrDefault(i => i.Type == "userRole" || i.Type == "userrole")
            ?? throw new UnauthorizedAccessException("User role claim not found");
        return claim.Value;
    }

    public static string UserName(this ClaimsPrincipal user) 
    {
        var claim = user.Claims.FirstOrDefault(i => i.Type == "userName" || i.Type == "username")
            ?? throw new UnauthorizedAccessException("User name claim not found");
        return claim.Value;
    }
}

public static class ControllerBaseExtensions {
    public static UserDTO GetUser(this ControllerBase controller) {
        return new UserDTO(controller.User.UserId(), controller.User.UserEmail(), controller.User.UserRole(), controller.User.UserName());  
    }
}