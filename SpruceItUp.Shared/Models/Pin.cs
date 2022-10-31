using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace SpruceItUp.Shared.Models
{
    public record Pin
    {
        public Guid Id { get; set; }
        public Guid Author { get; set; }
        public string Title { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public Position Position
        {
            get => new Position(Lat, Lon);
            set
            {
                Lat = value.Lat;
                Lon = value.Lon;
            }
        }
        public DateTime Created { get; set; }
        public PinKind Kind { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public DateTime Expires { get; set; }
    }
    public enum PinKind
    {
        [Display(Name ="Litter Pickup")] Litter,
        [Display(Name="Park Cleanup")] Park,
        [Display(Name="Trail Maintenance")] Trail,
        [Display(Name="Other")] Other,
    }
}