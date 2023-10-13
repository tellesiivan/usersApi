namespace DotnetApi
{
    public partial class UserJobInfo
    {
        public string JobTitle { get; set; }
        public string Department { get; set; }

        public UserJobInfo()
        {
            JobTitle ??= "";
            Department ??= "";
        }
    }
}