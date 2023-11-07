// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Identity;

namespace Evat.IdentityServer.Models; 

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public int State { get; set;} 
    public string FirstName { get; internal set; }
    public string LastName { get; internal set; }
}
