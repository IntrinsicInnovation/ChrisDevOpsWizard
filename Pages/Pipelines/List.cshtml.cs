using DevOpsWizard.data;
using DevOpsWizard.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace DevOpsWizard.Pages.Pipelines
{
    public class ListModel : PageModel
    {
        private readonly WizardDbContext dbContext;
        public List<Pipeline> Pipelines { get; set; }

        public ListModel(WizardDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public void OnGet()
        {
            Pipelines = dbContext.Pipelines.ToList();


        }
    }
}
