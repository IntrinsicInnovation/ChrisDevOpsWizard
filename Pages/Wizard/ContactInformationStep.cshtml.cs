using System.Drawing;

namespace ServerWizardExample.Pages.Wizard
{
    public class ContactInformationStep : StepViewModel
    {
        public string Email { get; set; }
        public string Phone { get; set; }

        public ContactInformationStep()
        {
            Position = 1;
        }
    }


    public class PipelineInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } 
        public Image Image { get; set; }

    }
}