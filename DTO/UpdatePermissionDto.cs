namespace TaskTrackingApi.Dtos
{
    public class UpdatePermissionDto
    {
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool HasGlobalView { get; set; }
    }
}
