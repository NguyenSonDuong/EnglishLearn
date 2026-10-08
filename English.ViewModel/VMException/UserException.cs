using English.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace English.ViewModel.VMException
{
    public class UserException : ViewModelException
    {
        public UserException(string message) : base(message)
        {
            _title = ExceptionTitle.USER_ERROR;
        }
        public UserException(string message, Exception innelException) : base(message, innelException)
        {
            _title = ExceptionTitle.SYSTEM_ERROR;
        }
    }
}
