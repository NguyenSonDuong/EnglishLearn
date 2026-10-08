using English.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace English.ViewModel.VMException
{
    public class UnknowException : ViewModelException
    {
        public UnknowException(string message) : base(message)
        {
            _title = ExceptionTitle.UNKNOW_ERROR;
        }
        public UnknowException(string message, Exception innelException) : base(message, innelException)
        {
            _title = ExceptionTitle.UNKNOW_ERROR;
        }
    }
}
