using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using LinqToDB.Mapping;
using SpruceItUp.Shared.UserManagement;
using System.Collections.Generic;

namespace UserManagement.Models
{
    [Table(Name = "users")]
    public record User
    {
        public User(Guid userId, string friendlyName, string email, AccountType type, DateTime created, Guid[] signups)
        {
            UserId = userId;
            FriendlyName = friendlyName;
            Email = email;
            Created = created;
            Type = type;
            Signups = signups;
        }
        [PrimaryKey] [Column(Name = "userid")] public Guid UserId { get; set; }
        [Column(Name = "friendlyname")] public string FriendlyName { get; set; }
        [Column(Name = "email")] public string Email { get; set; }
        [Column(Name = "created")] public DateTime Created { get; set; }
        [Column(Name = "kind")] public AccountType Type { get; set; }
        [Column(Name = "signups")] public Guid[] Signups { get; set; }
    }
}