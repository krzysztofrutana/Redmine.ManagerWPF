using Redmine.ManagerWPF.Abstraction.Interfaces;
using Redmine.ManagerWPF.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Desktop.Services
{
    public class DoingTimeService : IService
    {
        private readonly Context _context;

        public DoingTimeService(Context context)
        {
            _context = context;
        }
    }
}