namespace ProConnect.Application.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de eliminación completa de usuarios
    /// </summary>
    public interface IUserDeletionService
    {
        /// <summary>
        /// Elimina un usuario permanentemente junto con todos sus datos relacionados
        /// </summary>
        /// <param name="userId">ID del usuario a eliminar</param>
        /// <returns>True si se eliminó correctamente</returns>
        Task<bool> DeleteUserPermanentlyAsync(string userId);
    }
} 