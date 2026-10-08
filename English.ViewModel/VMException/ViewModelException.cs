using English.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace English.ViewModel.VMException
{
    public class ViewModelException :  Exception
    {
        protected string _title;
        public string Title { get => _title; }

        public ViewModelException(string message) : base(message)
        {
            _title = ExceptionTitle.SYSTEM_ERROR;
        }
        public ViewModelException(string message, Exception innelException) : base(message, innelException)
        {
            _title = ExceptionTitle.SYSTEM_ERROR;
        }
        public ViewModelException(string title,string message, Exception innelException) : base(message, innelException)
        {
            _title = title;
        }

    }
}
