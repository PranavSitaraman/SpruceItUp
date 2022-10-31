using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace SpruceItUp.Shared.Models
{
    public record Loc
    {
        public Guid Id { get; set; }
        public Guid Author { get; set; }
        public string Title { get; set; }
        public double[] Lat { get; set; }
        public double[] Lng { get; set; }
        public DateTime Created { get; set; }
        public LocKind Kind { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public DateTime Expires { get; set; }
        public string[] Pins { get; set; }
    }
    public enum LocKind
    {
        [Display(Name ="Litter Pickup")] Litter,
        [Display(Name="Park Cleanup")] Park,
        [Display(Name="Trail Maintenance")] Trail,
        [Display(Name="Other")] Other,
    }
}