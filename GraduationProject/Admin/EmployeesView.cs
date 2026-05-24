using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GraduationProject.Admin
{
    public class EmployeesView
    {
        public string Фамилия { get; set; }
        public string Имя { get; set; }
        public string Отчество { get; set; }
        public string Звание { get; set; }
        public string Отдел { get; set; }
        public BitmapImage Фото { get; set; }
    }
}
