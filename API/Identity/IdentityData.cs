using API.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace API.Identity
{
    public class IdentityData
    {
        public static void AddRolePolicies(AuthorizationOptions options)
        {
            foreach (Role role in Enum.GetValues(typeof(Role)))
            {
                options.AddPolicy(role.ToString(), policy =>
                    policy.RequireRole(role.ToString()));
            }

            //Just an Example
            options.AddPolicy("ClientOrAgent", policy =>
                policy.RequireRole(Role.Agent.ToString(), Role.Client.ToString()));
        }
    }
}
