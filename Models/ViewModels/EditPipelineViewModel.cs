namespace DevOpsWizard.Models.ViewModels
{
    public class EditPipelineViewModel
    {

        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        //public string ImageURL { get; set; }

        public string FileName { get; set; }
        public byte[] Content { get; set; }

        public string BuildJsonString { get; set; }
        public string ReleaseJsonString { get; set; }
        public bool IsVisible { get; set; }

    }
}
