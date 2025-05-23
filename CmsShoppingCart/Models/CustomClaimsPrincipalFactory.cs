using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CmsShoppingCart.Models
{
    public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser> // Asegúrate de usar tu clase de usuario correcta aquí
    {
        public CustomClaimsPrincipalFactory(
            UserManager<AppUser> userManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        { }

        // Aquí agregamos el Claim del Email
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Agregar el Email como un Claim
            identity.AddClaim(new Claim(ClaimTypes.Email, user.Email ?? ""));
            return identity;
        }
    }
}
