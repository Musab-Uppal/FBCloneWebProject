using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using semproject.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace semproject.Authorization
{
    public class PostOwnerAuthorizationHandler : AuthorizationHandler<PostOwnerRequirement, Post>
    {
        private readonly UserManager<IdentityUser> _userManager;

        public PostOwnerAuthorizationHandler(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PostOwnerRequirement requirement,
            Post resource)
        {
            var currentUser = await _userManager.GetUserAsync(context.User);

            if (currentUser != null && resource.UserId == currentUser.Id)
            {
                context.Succeed(requirement);
            }
        }
    }
}