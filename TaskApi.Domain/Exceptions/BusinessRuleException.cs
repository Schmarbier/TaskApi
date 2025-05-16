using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskApi.Domain.Exceptions
{
    public class BusinessRuleException : Exception
    {
        public BusinessErrorCode Code { get; }

        public int InternalCode => (int)Code;

        public BusinessRuleException(BusinessErrorCode code) : base(BusinessError.GetMessage(code)) => Code = code;
    }
}
