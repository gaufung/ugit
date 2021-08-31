namespace Tindo.Ugit.Server.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Repository
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime LastModified { get; set; }

    }
}
