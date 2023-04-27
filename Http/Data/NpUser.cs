using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Data
{
    public class NpUser
    {
        public Guid Id { get; set; }
        public String Name { get; set; } = null!;
        public String Email { get; set; } = null!;
        public String? ConfirmCode { get; set; }
        public String Login { get; set; } = null!;
        public String Password { get; set; } = null!;
    }
}
