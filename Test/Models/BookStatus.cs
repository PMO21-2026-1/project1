using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Test.Models;

public enum BookStatus
{
    [Display(Name = "Доступна")]
    Available,

    [Display(Name = "Видана")]
    Borrowed,

    [Display(Name = "Зарезервована")]
    Reserved,

    [Display(Name = "Втрачена")]
    Lost,

    [Display(Name = "На реставрації")] 
    UnderRestoration
}