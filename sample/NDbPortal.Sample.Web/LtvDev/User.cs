using NDbPortal.Names.MappingAttributes;

namespace NDbPortal.Sample.Web.LtvDev

{
    [Table("users")]
    public class User
    {
        public long Id { get; set; }

        public string UserName { get; set; }

        public string FullName { get; set; }

        public string Password { get; set; }

        public string Salt { get; set; }

        public string Email { get; set; }

        public int RoleId { get; set; }

    }
}
