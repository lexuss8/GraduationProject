using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GraduationProject
{
    public class PeopleView
    {
        public string Фамилия { get; set; }
        public string Имя { get; set; }
        public string Отчество { get; set; }
        public string Дата_рождения { get; set; }
        public BitmapImage Фото { get; set; }
        public string ИНН { get; set; }
        public string Паспорт { get; set; }
        public string Телефон { get; set; }
        public string Прописка { get; set; }
    }
}
