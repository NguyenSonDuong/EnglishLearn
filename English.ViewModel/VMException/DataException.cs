using English.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace English.ViewModel.VMException
{
    public class DataException : ViewModelException
    {
        public DataException(string message) : base(message)
        {
            _title = ExceptionTitle.DATA_ERROR;
        }
        public DataException(string message, Exception innelException) : base(message, innelException)
        {
            _title = ExceptionTitle.DATA_ERROR;
        }
    }
}
