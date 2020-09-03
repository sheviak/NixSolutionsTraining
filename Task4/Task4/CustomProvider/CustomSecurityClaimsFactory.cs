using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using Task4.Models;

namespace Task4.CustomProvider
{
    public class CustomSecurityClaimsFactory : IUserClaimsPrincipalFactory<CustomUser>
    {
        public Task<ClaimsPrincipal> CreateAsync(CustomUser user)
        {
            return Task.Factory.StartNew(() =>
            {
                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));

                var principle = new ClaimsPrincipal(identity);

                return principle;
            });
        }
    }
}