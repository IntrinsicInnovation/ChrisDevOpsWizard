using DevOpsWizard.data;
using DevOpsWizard.Models.Domain;
using DevOpsWizard.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DevOpsWizard.Pages.Pipelines
{
    public class EditModel : PageModel
    {
        private readonly WizardDbContext dbContext;
        private IHostEnvironment Environment;

        [BindProperty]
        public EditPipelineViewModel EditPipelineViewModel { get; set; }
        public EditModel(WizardDbContext dbContext, IHostEnvironment _environment)
        {
            this.dbContext = dbContext;
            Environment = _environment;

        }

        [BindProperty]
        public FileViewModel FileUpload { get; set; }

        public void OnGet(int id)
        {
            var pipeline = dbContext.Pipelines.Find(id);

            if (pipeline != null)
            {
                EditPipelineViewModel = new EditPipelineViewModel()
                {
                    Id = pipeline.Id,
                    BuildJsonString = pipeline.BuildJsonString,
                    Description = pipeline.Description,
                    Name = pipeline.Name,
                    ReleaseJsonString = pipeline.ReleaseJsonString,
                    Content = pipeline.Content,
                    FileName = pipeline.FileName,
                    IsVisible = pipeline.IsVisible
                };
            }
        }

        public async Task<IActionResult> OnPostUpdate(int id)
        {
            if (EditPipelineViewModel != null)
            {
                var existingpipeline = dbContext.Pipelines.Find(id);
                if (existingpipeline != null)
                {
                   // existingpipeline.Id = EditPipelineViewModel.Id;
                    existingpipeline.BuildJsonString = EditPipelineViewModel.BuildJsonString;
                    existingpipeline.Description = EditPipelineViewModel.Description;
                    existingpipeline.Name = EditPipelineViewModel.Name;
                    existingpipeline.ReleaseJsonString = EditPipelineViewModel.ReleaseJsonString;
                    existingpipeline.IsVisible = EditPipelineViewModel.IsVisible;

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
                                existingpipeline.FileName = FileUpload.FormFile.FileName;
                                existingpipeline.Content = memoryStream.ToArray();
                            }
                            else
                            {
                                ModelState.AddModelError("File", "The file is too large.");
                            }
                        }
                    }


                   // dbContext.Pipelines.Add(pipelineDomainModel);
                    await dbContext.SaveChangesAsync();

                    //dbContext.SaveChanges();


                    ViewData["Message"] = "Pipeline Saved Successfully";
                    return RedirectToPage("/Pipelines/list");
                }
               
            }
            ViewData["Message"] = "Error with Pipeline Data!";
            return Page(); // RedirectToPage("/Pipelines/list");
        }

        public IActionResult OnPostDelete(int id)
        {
            var existingpipeline = dbContext.Pipelines.Find(id);

            if (existingpipeline != null)
            {
                dbContext.Remove(existingpipeline);
                dbContext.SaveChanges();

                return RedirectToPage("/Pipelines/List");
            }

            return Page();
        }

    }
}

