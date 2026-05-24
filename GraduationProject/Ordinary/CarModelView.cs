using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GraduationProject.Ordinary
{
    public class CarModelView
    {
        public string Марка { get; set; }
        public string Модель { get; set; }
        public string Категория { get; set; }
        public string Кузов { get; set; }
        public BitmapImage Фото { get; set; }
    }
}
