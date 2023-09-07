using DevOpsWizard.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace DevOpsWizard.data
{
    public class WizardDbContext : DbContext

    {
        public WizardDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Pipeline> Pipelines { get; set; }
    }
}
