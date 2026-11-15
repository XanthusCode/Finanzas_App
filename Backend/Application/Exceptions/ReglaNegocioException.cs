namespace Finanzas.Application.Exceptions
{
    // Error esperado de una regla de negocio, con un mensaje pensado para mostrarse al usuario.
    // Los controladores solo devuelven el Message de esta excepción; cualquier otra
    // (p. ej. InvalidOperationException de Entity Framework) queda como 500 sin detalle.
    public class ReglaNegocioException(string mensaje) : Exception(mensaje);
}
