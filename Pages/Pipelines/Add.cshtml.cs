using DevOpsWizard.data;
using DevOpsWizard.Models.Domain;
using DevOpsWizard.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using System.IO.Compression;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;

namespace DevOpsWizard.Pages.Pipelines
{
    public class AddModel : PageModel
    {
        private readonly WizardDbContext dbContext;

        private IHostEnvironment Environment;


        public AddModel(WizardDbContext dbContext, IHostEnvironment _environment)
        {
            this.dbContext = dbContext;
            Environment = _environment;
        }
        [BindProperty]
        public AddPipelineViewModel AddPipelineRequest { get; set; }

        [BindProperty]
        public FileViewModel FileUpload { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {

            var pipelineDomainModel = new Pipeline
            {
                BuildJsonString = AddPipelineRequest.BuildJsonString,
                Name = AddPipelineRequest.Name,
                Description = AddPipelineRequest.Description,
                // ImageURL = AddPipelineRequest.ImageURL,

                ReleaseJsonString = AddPipelineRequest.ReleaseJsonString,
                IsVisible = false
                
            };

            if (FileUpload?.FormFile?.Length > 0)
            {
                using (var stream = new FileStream(Path.Combine(Environment.ContentRootPath, "uploadfiles", FileUpload.FormFile.FileName), FileMode.Create))
                {
                    await FileUpload.FormFile.CopyToAsync(stream);
                }
            }
            //save image to database.
            using (var memoryStream = new MemoryStream())
            {
                if (FileUpload.FormFile != null)
                {
                    await FileUpload.FormFile.CopyToAsync(memoryStream);

                    // Upload the file if less than 2 MB
                    if (memoryStream.Length < 2097152)
                    {
                        pipelineDomainModel.FileName = FileUpload.FormFile.FileName;
                        pipelineDomainModel.Content = memoryStream.ToArray();
                    }
                    else
                    {
                        ModelState.AddModelError("File", "The file is too large.");
                    }
                }
            }




            dbContext.Pipelines.Add(pipelineDomainModel);
            await dbContext.SaveChangesAsync();
            //dbContext.SaveChanges();

            ViewData["Message"] = $"{pipelineDomainModel.Name} created successfully!";
            return RedirectToPage("/Pipelines/list");
            //return Ok(new { count = files.Count, size });


        }

    }
}
