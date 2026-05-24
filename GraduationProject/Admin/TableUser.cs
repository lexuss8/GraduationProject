using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraduationProject.Admin
{
    public class TableUser
    {
        public int id { get; set; }
        public string login { get; set; }
        public string password { get; set; }

        public string employee_token { get; set; }
        public string fio { get; set; }

        public int role_id { get; set; }
        public string role_name { get; set; }
    }
}
