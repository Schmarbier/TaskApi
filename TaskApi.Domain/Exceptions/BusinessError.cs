using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskApi.Domain.Exceptions
{
    public enum BusinessErrorCode
    {
        UsuarioSinPermiso = 480,
        ProyectoDuplicado = 481
    }

    public static class BusinessError
    {
        public static readonly Dictionary<BusinessErrorCode, string> Messages = new()
        {
            { BusinessErrorCode.ProyectoDuplicado, "Ya existe un proyecto con ese mismo nombre" },
            { BusinessErrorCode.UsuarioSinPermiso, "El usuario no tiene permisos para realizar esta acción" }
        
        };

        public static string GetMessage(BusinessErrorCode code) => Messages.TryGetValue(code, out var msg)
            ? msg
            : "Error de negocio desconocido";
    }
}
