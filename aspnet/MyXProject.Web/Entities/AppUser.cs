using System.ComponentModel.DataAnnotations;
using Xams.Core.Attributes;
using Xams.Core.Entities;

namespace MyXProject.Web.Entities;

public class AppUser : User
{
    [UIRequired]
    [MaxLength(100)]
    public string? EmailAddress { get; set; }
    
}