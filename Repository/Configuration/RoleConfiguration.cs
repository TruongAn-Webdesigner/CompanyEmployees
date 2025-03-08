using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Configuration
{
    public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.HasData(
                new IdentityRole
                {
                    Id = "6ac343b0-00ef-4a1c-8f64-68daaca77b5b",
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                    ConcurrencyStamp = "6ac343b0-00ef-4a1c-8f64-68daaca77b5b"
                },
                new IdentityRole
                {
                    Id = "6ac343b0-00ef-4a1c-8f64-68daaca77b5c",
                    Name = "Administrator",
                    NormalizedName = "ADMINISTRATOR",
                    ConcurrencyStamp = "6ac343b0-00ef-4a1c-8f64-68daaca77b5c"
                });
        }
    }
}
